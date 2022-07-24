//-----------------------------------------------------------------------
// <copyright file="CMapBuilder.cs" company="">
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
    using System.Collections.Generic;

    using org.pdfclown.util;
    using bytes = org.pdfclown.bytes;

    ///
    /// <summary>
    /// CMap builder [PDF:1.6:5.6.4,5.9.2;CMAP].
    /// </summary>
    ///
    internal sealed class CMapBuilder
    {
        private static readonly int SubSectionMaxCount = 100;

        private delegate bytes::IBuffer BuildEntryDelegate<T>(
            T entry,
            bytes::IBuffer buffer,
            GetOutCodeDelegate outCodeFunction,
            string outCodeFormat);

        public delegate int GetOutCodeDelegate(KeyValuePair<ByteArray, int> entry);

        private static void AddEntry(
            IList<KeyValuePair<ByteArray, int>[]> cidRanges,
            IList<KeyValuePair<ByteArray, int>> cidChars,
            KeyValuePair<ByteArray, int> lastEntry,
            KeyValuePair<ByteArray, int>[] lastRange)
        {
            if (lastRange != null) // Range.
            {
                lastRange[1] = lastEntry;
                cidRanges.Add(lastRange);
            }
            else // Single character.
            {
                cidChars.Add(lastEntry);
            }
        }

        private static bytes::IBuffer BuildCharEntry(
            KeyValuePair<ByteArray, int> cidChar,
            bytes::IBuffer buffer,
            GetOutCodeDelegate outCodeFunction,
            string outCodeFormat)
        {
            return buffer.Append("<")
                       .Append(ConvertUtils.ByteArrayToHex(cidChar.Key.Data))
                       .Append("> ")
                       .Append(string.Format(outCodeFormat, outCodeFunction(cidChar)))
                       .Append("\n");
        }

        private static void BuildEntriesSection<T>(
            bytes::IBuffer buffer,
            EntryTypeEnum entryType,
            IList<T> items,
            BuildEntryDelegate<T> buildEntryFunction,
            string operatorSuffix,
            GetOutCodeDelegate outCodeFunction,
            string outCodeFormat)
        {
            if (items.Count == 0)
            {
                return;
            }

            for (int index = 0, count = items.Count; index < count; index++)
            {
                if (index % SubSectionMaxCount == 0)
                {
                    if (index > 0)
                    {
                        _ = buffer.Append("end").Append(entryType.Tag()).Append(operatorSuffix).Append("\n");
                    }
                    _ = buffer.Append(Math.Min(count - index, SubSectionMaxCount).ToString())
                            .Append(" ")
                            .Append("begin")
                            .Append(entryType.Tag())
                            .Append(operatorSuffix)
                            .Append("\n");
                }
                _ = buildEntryFunction(items[index], buffer, outCodeFunction, outCodeFormat);
            }
            _ = buffer.Append("end").Append(entryType.Tag()).Append(operatorSuffix).Append("\n");
        }

        private static bytes::IBuffer BuildRangeEntry(
            KeyValuePair<ByteArray, int>[] cidRange,
            bytes::IBuffer buffer,
            GetOutCodeDelegate outCodeFunction,
            string outCodeFormat)
        {
            return buffer.Append("<")
                       .Append(ConvertUtils.ByteArrayToHex(cidRange[0].Key.Data))
                       .Append("> <")
                       .Append(ConvertUtils.ByteArrayToHex(cidRange[1].Key.Data))
                       .Append("> ")
                       .Append(string.Format(outCodeFormat, outCodeFunction(cidRange[0])))
                       .Append("\n");
        }

        ///
        /// <summary>
        /// Builds a CMap according to the specified arguments.
        /// </summary>
        /// <param name="entryType"></param>
        /// <param name="cmapName">CMap name (<code>null</code> in case no custom name is needed).</param>
        /// <param name="codes"></param>
        /// <param name="outCodeFunction"></param>
        /// <returns>Buffer containing the serialized CMap.</returns>
        ///
        public static bytes::IBuffer Build(
            EntryTypeEnum entryType,
            string cmapName,
            SortedDictionary<ByteArray, int> codes,
            GetOutCodeDelegate outCodeFunction)
        {
            bytes::IBuffer buffer = new bytes::Buffer();

            // Header.
            string outCodeFormat;
            switch (entryType)
            {
                case EntryTypeEnum.BaseFont:
                    if (cmapName == null)
                    {
                        cmapName = "Adobe-Identity-UCS";
                    }
                    _ = buffer.Append(
                        "/CIDInit /ProcSet findresource begin\n" +
                            "12 dict begin\n" +
                            "begincmap\n" +
                            "/CIDSystemInfo\n" +
                            "<< /Registry (Adobe)\n" +
                            "/Ordering (UCS)\n" +
                            "/Supplement 0\n" +
                            ">> def\n" +
                            "/CMapName /")
                            .Append(cmapName)
                            .Append(
                                " def\n" +
                                    "/CMapVersion 10.001 def\n" +
                                    "/CMapType 2 def\n" +
                                    "1 begincodespacerange\n" +
                                    "<0000> <FFFF>\n" +
                                    "endcodespacerange\n");
                    outCodeFormat = "<{0:X4}>";
                    break;
                case EntryTypeEnum.CID:
                    if (cmapName == null)
                    {
                        cmapName = "Custom";
                    }
                    _ = buffer.Append(
                        "%!PS-Adobe-3.0 Resource-CMap\n" +
                            "%%DocumentNeededResources: ProcSet (CIDInit)\n" +
                            "%%IncludeResource: ProcSet (CIDInit)\n" +
                            "%%BeginResource: CMap (")
                            .Append(cmapName)
                            .Append(")\n" + "%%Title: (")
                            .Append(cmapName)
                            .Append(
                                " Adobe Identity 0)\n" +
                                    "%%Version: 1\n" +
                                    "%%EndComments\n" +
                                    "/CIDInit /ProcSet findresource begin\n" +
                                    "12 dict begin\n" +
                                    "begincmap\n" +
                                    "/CIDSystemInfo 3 dict dup begin\n" +
                                    "/Registry (Adobe) def\n" +
                                    "/Ordering (Identity) def\n" +
                                    "/Supplement 0 def\n" +
                                    "end def\n" +
                                    "/CMapVersion 1 def\n" +
                                    "/CMapType 1 def\n" +
                                    "/CMapName /")
                            .Append(cmapName)
                            .Append(
                                " def\n" +
                                    "/WMode 0 def\n" +
                                    "1 begincodespacerange\n" +
                                    "<0000> <FFFF>\n" +
                                    "endcodespacerange\n");
                    outCodeFormat = "{0}";
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Entries.

            IList<KeyValuePair<ByteArray, int>> cidChars = new List<KeyValuePair<ByteArray, int>>();
            IList<KeyValuePair<ByteArray, int>[]> cidRanges = new List<KeyValuePair<ByteArray, int>[]>();
            KeyValuePair<ByteArray, int>? lastCodeEntry = null;
            KeyValuePair<ByteArray, int>[] lastCodeRange = null;
            foreach (var codeEntry in codes)
            {
                if (lastCodeEntry.HasValue)
                {
                    var codeLength = codeEntry.Key.Data.Length;
                    if ((codeLength == lastCodeEntry.Value.Key.Data.Length) &&
                        (codeEntry.Key.Data[codeLength - 1] - lastCodeEntry.Value.Key.Data[codeLength - 1] == 1) &&
                        (outCodeFunction(codeEntry) - outCodeFunction(lastCodeEntry.Value) == 1)) // Contiguous codes.
                    {
                        if (lastCodeRange == null)
                        {
                            lastCodeRange = new KeyValuePair<ByteArray, int>[]
                            {
                                lastCodeEntry.Value,
                                default(KeyValuePair<ByteArray, int>)
                            };
                        }
                    }
                    else // Separated codes.
                    {
                        AddEntry(cidRanges, cidChars, lastCodeEntry.Value, lastCodeRange);
                        lastCodeRange = null;
                    }
                }
                lastCodeEntry = codeEntry;
            }
            AddEntry(cidRanges, cidChars, lastCodeEntry.Value, lastCodeRange);
            // Ranges section.
            BuildEntriesSection(buffer, entryType, cidRanges, BuildRangeEntry, "range", outCodeFunction, outCodeFormat);
            // Chars section.
            BuildEntriesSection(buffer, entryType, cidChars, BuildCharEntry, "char", outCodeFunction, outCodeFormat);

            // Trailer.
            switch (entryType)
            {
                case EntryTypeEnum.BaseFont:
                    _ = buffer.Append(
                        "endcmap\n" + "CMapName currentdict /CMap defineresource pop\n" + "end\n" + "end\n");
                    break;
                case EntryTypeEnum.CID:
                    _ = buffer.Append(
                        "endcmap\n" +
                            "CMapName currentdict /CMap defineresource pop\n" +
                            "end\n" +
                            "end\n" +
                            "%%EndResource\n" +
                            "%%EOF");
                    break;
                default:
                    throw new NotImplementedException();
            }

            return buffer;
        }

        public enum EntryTypeEnum
        {
            BaseFont,
            CID
        }
    }

    internal static class EntryTypeEnumExtension
    {
        public static string Tag(this CMapBuilder.EntryTypeEnum entryType)
        {
            switch (entryType)
            {
                case CMapBuilder.EntryTypeEnum.BaseFont:
                    return "bf";
                case CMapBuilder.EntryTypeEnum.CID:
                    return "cid";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
