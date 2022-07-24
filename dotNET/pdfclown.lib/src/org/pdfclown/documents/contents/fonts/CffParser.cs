//-----------------------------------------------------------------------
// <copyright file="CffParser.cs" company="">
//     Copyright 2010-2012 Stefano Chizzolini. http://www.pdfclown.org
//     
//     Contributors:
//       * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
//     
//     This file should be part of the source code distribution of "PDF Clown library" (the
//     Program): see the accompanying README files for more info.
//     
//     This Program is free software; you can redistribute it and/or modify it under the terms
//     of the GNU Lesser General Public License as published by the Free Software Foundation;
//     either version 3 of the License, or (at your option) any later version.
//     
//     This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
//     either expressed or implied; without even the implied warranty of MERCHANTABILITY or
//     FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.
//     
//     You should have received a copy of the GNU Lesser General Public License along with this
//     Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).
//     
//     Redistribution and use, with or without modification, are permitted provided that such
//     redistributions retain the above copyright notice, license and disclaimer, along with
//     this list of conditions.
// </copyright>
//-----------------------------------------------------------------------
namespace org.pdfclown.documents.contents.fonts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using System.IO;
    using System.Reflection;
    using System.Text;
    using org.pdfclown.bytes;
    using org.pdfclown.tokens;
    using org.pdfclown.util;

    ///
    /// <summary>
    /// CFF file format parser [CFF:1.0].
    /// </summary>
    ///
    internal sealed class CffParser
    {
        ///
        /// <summary>
        /// Standard charset maps.
        /// </summary>
        ///
        private static readonly IDictionary<StandardCharsetEnum, IDictionary<int, int>> StandardCharsets;

        ///
        /// <summary>
        /// Standard Strings [CFF:1.0:10] represent commonly occurring strings allocated to predefined SIDs.
        /// </summary>
        ///
        private static readonly IList<string> StandardStrings;

        private readonly IInputStream fontData;
        private Index stringIndex;

        static CffParser()
        {
            StandardCharsets = new Dictionary<StandardCharsetEnum, IDictionary<int, int>>();
            foreach (StandardCharsetEnum charset in Enum.GetValues(typeof(StandardCharsetEnum)))
            {
                IDictionary<int, int> charsetMap = new Dictionary<int, int>();
                using (var stream = new StreamReader(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream($"fonts.cff.{charset}Charset")))
                {
                    string line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        var lineItems = line.Split(',');
                        charsetMap[int.Parse(lineItems[0])] = GlyphMapping.NameToCode(lineItems[1]).Value;
                    }
                }
            }

            StandardStrings = new List<string>();
            using (var stream = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("fonts.cff.StandardStrings")))
            {
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    StandardStrings.Add(line);
                }
            }
        }

        internal CffParser(IInputStream fontData)
        {
            this.fontData = fontData;

            this.Load();
        }

        ///
        /// <summary>
        /// Gets the charset corresponding to the given value.
        /// </summary>
        ///
        private static StandardCharsetEnum? GetStandardCharset(int? value)
        {
            if (!value.HasValue)
            {
                return StandardCharsetEnum.ISOAdobe;
            }
            else if (!Enum.IsDefined(typeof(StandardCharsetEnum), value.Value))
            {
                return null;
            }
            else
            {
                return (StandardCharsetEnum)value.Value;
            }
        }

        ///
        /// <summary>
        /// Gets the string corresponding to the specified identifier.
        /// </summary>
        /// <param name="id">SID (String ID).</param>
        ///
        private string GetString(int id)
        {
            return (id < StandardStrings.Count)
                ? StandardStrings[id]
                : ToString(this.stringIndex[id - StandardStrings.Count]);
        }

        ///
        /// <summary>
        /// Loads the font data.
        /// </summary>
        ///
        private void Load()
        {
            try
            {
                this.ParseHeader();
                var nameIndex = Index.Parse(this.fontData);
                var topDictIndex = Index.Parse(this.fontData);
                this.stringIndex = Index.Parse(this.fontData);
#pragma warning disable 0219
                _ = Index.Parse(this.fontData);

                _ = ToString(nameIndex[0]);
#pragma warning restore 0219
                var topDict = Dict.Parse(topDictIndex[0]);

                //      int encodingOffset = topDict.get(Dict.OperatorEnum.Encoding, 0, 0).intValue();
                //TODO: encoding

                var charsetOffset = (int)topDict.Get(Dict.OperatorEnum.Charset, 0, 0);
                var charset = GetStandardCharset(charsetOffset);
                if (charset.HasValue)
                {
                    this.glyphIndexes = new Dictionary<int, int>(StandardCharsets[charset.Value]);
                }
                else
                {
                    this.glyphIndexes = new Dictionary<int, int>();

                    var charStringsOffset = (int)topDict.Get(Dict.OperatorEnum.CharStrings, 0);
                    var charStringsIndex = Index.Parse(this.fontData, charStringsOffset);

                    this.fontData.Seek(charsetOffset);
                    var charsetFormat = this.fontData.ReadByte();
                    for (int index = 1, count = charStringsIndex.Count; index <= count;)
                    {
                        switch (charsetFormat)
                        {
                            case 0:
                                this.glyphIndexes[index++] = this.ToUnicode(this.fontData.ReadUnsignedShort());
                                break;
                            case 1:
                            case 2:
                                int first = this.fontData.ReadUnsignedShort();
                                var nLeft = (charsetFormat == 1)
                                    ? this.fontData.ReadByte()
                                    : this.fontData.ReadUnsignedShort();
                                for (int rangeItemIndex = first, rangeItemEndIndex = first + nLeft; rangeItemIndex <=
                                    rangeItemEndIndex; rangeItemIndex++)
                                {
                                    this.glyphIndexes[index++] = this.ToUnicode(rangeItemIndex);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void ParseHeader()
        {
            this.fontData.Seek(2);
            var hdrSize = this.fontData.ReadByte();
            // Skip to the end of the header!
            this.fontData.Seek(hdrSize);
        }

        private static string ToString(byte[] data) { return Charset.ISO88591.GetString(data); }

        private int ToUnicode(int sid)
        {
            /*
             * FIXME: avoid Unicode resolution at this stage -- names should be kept to allow subsequent
             * character substitution (see font differences) in case of custom (non-unicode) encodings.
             */
            var code = GlyphMapping.NameToCode(this.GetString(sid));
            if (!code.HasValue)
            {
                //custom code
                code = sid; // really bad
            }
            return code.Value;
        }

        public IDictionary<int, int> glyphIndexes { get; set; }

        ///
        /// <summary>
        /// Dictionary [CFF:1.0:4].
        /// </summary>
        ///
        private sealed class Dict : IDictionary<int, IList<object>>
        {
            private const int OperatorValueEscape = 12 << 8;

            private readonly IDictionary<int, IList<object>> entries;

            private Dict(IDictionary<int, IList<object>> entries) { this.entries = entries; }

            public IList<object> this[int key]
            {
                get
                {
                    IList<object> value;
                    _ = this.entries.TryGetValue(key, out value);
                    return value;
                }
                set => throw new NotSupportedException();
            }

            void ICollection<KeyValuePair<int, IList<object>>>.Add(KeyValuePair<int, IList<object>> keyValuePair)
            { throw new NotSupportedException(); }

            bool ICollection<KeyValuePair<int, IList<object>>>.Contains(KeyValuePair<int, IList<object>> keyValuePair)
            { return this.entries.Contains(keyValuePair); }

            IEnumerator IEnumerable.GetEnumerator()
            { return ((IEnumerable<KeyValuePair<int, IList<object>>>)this).GetEnumerator(); }

            IEnumerator<KeyValuePair<int, IList<object>>> IEnumerable<KeyValuePair<int, IList<object>>>.GetEnumerator()
            { return this.entries.GetEnumerator(); }

            public void Add(int key, IList<object> value) { throw new NotSupportedException(); }

            public void Clear() { throw new NotSupportedException(); }

            public bool ContainsKey(int key) { return this.entries.ContainsKey(key); }

            public void CopyTo(KeyValuePair<int, IList<object>>[] keyValuePairs, int index)
            { throw new NotImplementedException(); }

            public object Get(OperatorEnum @operator, int operandIndex)
            { return this.Get(@operator, operandIndex, null); }

            public object Get(OperatorEnum @operator, int operandIndex, int? defaultValue)
            {
                var operands = this[(int)@operator];
                return (operands != null) ? operands[operandIndex] : defaultValue;
            }

            public static string GetOperatorName(OperatorEnum value)
            {
                switch (value)
                {
                    case OperatorEnum.Charset:
                        return "charset";
                    default:
                        return value.ToString();
                }
            }

            public static Dict Parse(byte[] data) { return Parse(new bytes.Buffer(data)); }

            public static Dict Parse(IInputStream stream)
            {
                IDictionary<int, IList<object>> entries = new Dictionary<int, IList<object>>();
                IList<object> operands = null;
                while (true)
                {
                    var b0 = stream.ReadByte();
                    if (b0 == -1)
                    {
                        break;
                    }

                    if ((b0 >= 0) && (b0 <= 21)) // Operator.
                    {
                        var @operator = b0;
                        if (b0 == 12) // 2-byte operator.
                        {
                            @operator = @operator << (8 + stream.ReadByte());
                        }

                        /*
                          NOTE: In order to resiliently support unknown operators on parsing, parsed operators
                          are not directly mapped to OperatorEnum.
                        */
                        entries[@operator] = operands;
                        operands = null;
                    }
                    else // Operand.
                    {
                        if (operands == null)
                        {
                            operands = new List<object>();
                        }

                        if (b0 == 28) // 3-byte integer.
                        {
                            operands.Add(stream.ReadByte() << (8 + stream.ReadByte()));
                        }
                        else if (b0 == 29) // 5-byte integer.
                        {
                            operands.Add(
                                stream.ReadByte() <<
                                    (24 + stream.ReadByte()) <<
                                    (16 + stream.ReadByte()) <<
                                    (8 + stream.ReadByte()));
                        }
                        else if (b0 == 30) // Variable-length real.
                        {
                            var operandBuilder = new StringBuilder();
                            var ended = false;
                            do
                            {
                                var b = stream.ReadByte();
                                int[] nibbles = { (b >> 4) & 0xf, b & 0xf };
                                foreach (var nibble in nibbles)
                                {
                                    switch (nibble)
                                    {
                                        case 0x0:
                                        case 0x1:
                                        case 0x2:
                                        case 0x3:
                                        case 0x4:
                                        case 0x5:
                                        case 0x6:
                                        case 0x7:
                                        case 0x8:
                                        case 0x9:
                                            _ = operandBuilder.Append(nibble);
                                            break;
                                        case 0xa: // Decimal point.
                                            _ = operandBuilder.Append(".");
                                            break;
                                        case 0xb: // Positive exponent.
                                            _ = operandBuilder.Append("E");
                                            break;
                                        case 0xc: // Negative exponent.
                                            _ = operandBuilder.Append("E-");
                                            break;
                                        case 0xd: // Reserved.
                                            break;
                                        case 0xe: // Minus.
                                            _ = operandBuilder.Append("-");
                                            break;
                                        case 0xf: // End of number.
                                            ended = true;
                                            break;
                                    }
                                }
                            } while (!ended);
                            operands.Add(ConvertUtils.ParseDoubleInvariant(operandBuilder.ToString()));
                        }
                        else if ((b0 >= 32) && (b0 <= 246)) // 1-byte integer.
                        {
                            operands.Add(b0 - 139);
                        }
                        else if ((b0 >= 247) && (b0 <= 250)) // 2-byte positive integer.
                        {
                            operands.Add((b0 - 247) << (8 + stream.ReadByte() + 108));
                        }
                        else if ((b0 >= 251) && (b0 <= 254)) // 2-byte negative integer.
                        {
                            operands.Add((-(b0 - 251)) << (8 - stream.ReadByte() - 108));
                        }
                        else // Reserved.
                        { /* NOOP */
                        }
                    }
                }
                return new Dict(entries);
            }

            public bool Remove(int key) { throw new NotSupportedException(); }

            public bool Remove(KeyValuePair<int, IList<object>> keyValuePair) { throw new NotSupportedException(); }

            public bool TryGetValue(int key, out IList<object> value)
            { return this.entries.TryGetValue(key, out value); }

            public int Count => this.entries.Count;

            public bool IsReadOnly => true;

            public ICollection<int> Keys => this.entries.Keys;

            public ICollection<IList<object>> Values => this.entries.Values;

            public enum OperatorEnum
            {
                Charset = 15,
                CharStrings = 17,
                CharstringType = 6 + OperatorValueEscape,
                Encoding = 16
            }
        }

        ///
        /// <summary>
        /// Array of variable-sized objects [CFF:1.0:5].
        /// </summary>
        ///
        private sealed class Index : IList<byte[]>
        {
            private readonly byte[][] data;

            private Index(byte[][] data) { this.data = data; }

            public byte[] this[int index] { get => this.data[index]; set => throw new NotSupportedException(); }

            IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

            public void Add(byte[] item) { throw new NotSupportedException(); }

            public void Clear() { throw new NotSupportedException(); }

            public bool Contains(byte[] item) { throw new NotImplementedException(); }

            public void CopyTo(byte[][] items, int index) { throw new NotImplementedException(); }

            public IEnumerator<byte[]> GetEnumerator()
            {
                for (int index = 0, length = this.Count; index < length; index++)
                {
                    yield return this[index];
                }
            }

            public int IndexOf(byte[] item) { throw new NotImplementedException(); }

            public void Insert(int index, byte[] item) { throw new NotSupportedException(); }
            public static Index Parse(byte[] data) { return Parse(new bytes.Buffer(data)); }

            public static Index Parse(IInputStream stream)
            {
                var data = new byte[stream.ReadUnsignedShort()][];
                var offsets = new int[data.Length + 1];
                var offSize = stream.ReadByte();
                for (int index = 0, count = offsets.Length; index < count; index++)
                {
                    offsets[index] = stream.ReadInt(offSize);
                }
                for (int index = 0, count = data.Length; index < count; index++)
                {
                    stream.Read(data[index] = new byte[offsets[index + 1] - offsets[index]]);
                }
                return new Index(data);
            }

            public static Index Parse(IInputStream stream, int offset)
            {
                stream.Seek(offset);
                return Parse(stream);
            }

            public bool Remove(byte[] item) { throw new NotSupportedException(); }

            public void RemoveAt(int index) { throw new NotSupportedException(); }

            public int Count => this.data.Length;

            public bool IsReadOnly => true;
        }

        ///
        /// <summary>
        /// Predefined charsets [CFF:1.0:12,C].
        /// </summary>
        ///
        private enum StandardCharsetEnum
        {
            ISOAdobe = 0,
            Expert = 1,
            ExpertSubset = 2
        }
    }
}
