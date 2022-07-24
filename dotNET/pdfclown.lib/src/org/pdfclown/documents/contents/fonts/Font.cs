//-----------------------------------------------------------------------
// <copyright file="Font.cs" company="">
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
    using System.Linq;
    using System.Text;

    using org.pdfclown.bytes;
    using org.pdfclown.objects;
    using org.pdfclown.tokens;
    using org.pdfclown.util;
    using io = System.IO;

    ///
    /// <summary>
    /// Abstract font [PDF:1.6:5.4].
    /// </summary>
    ///
    [PDF(VersionEnum.PDF10)]
    public abstract class Font : PdfObjectWrapper<PdfDictionary>
    {
        private const int UndefinedDefaultCode = int.MinValue;
        private const int UndefinedWidth = int.MinValue;

        ///
        /// <summary>
        /// Maximum character code byte size.
        /// </summary>
        ///
        private int charCodeMaxLength = 0;

        private double textHeight = -1; // TODO: temporary until glyph bounding boxes are implemented.

        /*
             NOTE: In order to avoid nomenclature ambiguities, these terms are used consistently within the
             code:
             * character code: internal codepoint corresponding to a character expressed inside a string
             object of a content stream;
             * unicode: external codepoint corresponding to a character expressed according to the Unicode
             standard encoding;
             * glyph index: internal identifier of the graphical representation of a character.
        */
        ///
        /// <summary>
        /// Unicodes by character code.
        /// </summary>
        /// <remarks>
        /// <para>When this map is populated, <code>symbolic</code> variable shall accordingly be set.</para>
        /// </remarks>
        ///
        protected BiDictionary<ByteArray, int> codes;
        ///
        /// <summary>
        /// Glyph indexes by unicode.
        /// </summary>
        ///
        protected Dictionary<int, int> glyphIndexes;
        ///
        /// <summary>
        /// Glyph kernings by (left-right) glyph index pairs.
        /// </summary>
        ///
        protected Dictionary<int, int> glyphKernings;
        ///
        /// <summary>
        /// Glyph widths by glyph index.
        /// </summary>
        ///
        protected Dictionary<int, int> glyphWidths;
        ///
        /// <summary>
        /// Whether the font encoding is custom (that is non-Unicode).
        /// </summary>
        ///
        protected bool symbolic = true;
        ///
        /// <summary>
        /// Used unicodes.
        /// </summary>
        ///
        protected HashSet<int> usedCodes;

        ///
        /// <summary>
        /// Creates a new font structure within the given document context.
        /// </summary>
        ///
        protected Font(Document context) : base(
            context,
            new PdfDictionary(new PdfName[1] { PdfName.Type }, new PdfDirectObject[1] { PdfName.Font }))
        { this.Initialize(); }

        ///
        /// <summary>
        /// Loads an existing font structure.
        /// </summary>
        ///
        protected Font(PdfDirectObject baseObject) : base(baseObject)
        {
            this.Initialize();
            this.Load();
        }

        private void Initialize()
        {
            this.usedCodes = new HashSet<int>();

            // Put the newly-instantiated font into the common cache!
            /*
              NOTE: Font structures are reified as complex objects, both IO- and CPU-intensive to load.
              So, it's convenient to put them into a common cache for later reuse.
            */
            this.Document.Cache[(PdfReference)this.BaseObject] = this;
        }

        ///
        /// <summary>
        /// Gets the specified font descriptor entry value.
        /// </summary>
        ///
        protected abstract PdfDataObject GetDescriptorValue(PdfName key);

        ///
        /// <summary>
        /// Loads font information from existing PDF font structure.
        /// </summary>
        ///
        protected void Load()
        {
            if (this.BaseDataObject.ContainsKey(PdfName.ToUnicode)) // To-Unicode explicit mapping.
            {
                var toUnicodeStream = (PdfStream)this.BaseDataObject.Resolve(PdfName.ToUnicode);
                var parser = new CMapParser(toUnicodeStream.Body);
                this.codes = new BiDictionary<ByteArray, int>(parser.Parse());
                this.symbolic = false;
            }

            this.OnLoad();

            // Maximum character code length.
            foreach (var charCode in this.codes.Keys)
            {
                if (charCode.Data.Length > this.charCodeMaxLength)
                {
                    this.charCodeMaxLength = charCode.Data.Length;
                }
            }
            // Missing character substitute.
            if (this.defaultCode == UndefinedDefaultCode)
            {
                var codePoints = this.CodePoints;
                if (codePoints.Contains('?'))
                {
                    this.DefaultCode = '?';
                }
                else if (codePoints.Contains(' '))
                {
                    this.DefaultCode = ' ';
                }
                else
                {
                    this.DefaultCode = codePoints.First();
                }
            }
        }

        ///
        /// <summary>
        /// Notifies font information loading from an existing PDF font structure.
        /// </summary>
        ///
        protected abstract void OnLoad();

        ///
        /// <summary>
        /// Gets/Sets the average glyph width.
        /// </summary>
        ///
        protected int AverageWidth
        {
            get
            {
                if (this.averageWidth == UndefinedWidth)
                {
                    if (this.glyphWidths.Count == 0)
                    {
                        this.averageWidth = 1000;
                    }
                    else
                    {
                        this.averageWidth = 0;
                        foreach (var glyphWidth in this.glyphWidths.Values)
                        {
                            this.averageWidth += glyphWidth;
                        }
                        this.averageWidth /= this.glyphWidths.Count;
                    }
                }
                return this.averageWidth;
            }
            set => this.averageWidth = value;
        }

        ///
        /// <summary>
        /// Gets/Sets the default glyph width.
        /// </summary>
        ///
        protected int DefaultWidth
        {
            get
            {
                if (this.defaultWidth == UndefinedWidth)
                {
                    this.defaultWidth = this.AverageWidth;
                }
                return this.defaultWidth;
            }
            set => this.defaultWidth = value;
        }

        ///
        /// <summary>
        /// Gets the text from the given internal representation.
        /// </summary>
        /// <param name="code">Internal representation to decode.</param>
        /// <exception cref="DecodeException"/>
        ///
        public string Decode(byte[] code)
        {
            var textBuilder = new StringBuilder();
            var codeBuffers = new byte[this.charCodeMaxLength + 1][];
            for (var codeBufferIndex = 0; codeBufferIndex <= this.charCodeMaxLength; codeBufferIndex++)
            {
                codeBuffers[codeBufferIndex] = new byte[codeBufferIndex];
            }
            var index = 0;
            var codeLength = code.Length;
            var codeBufferSize = 1;
            while (index < codeLength)
            {
                var codeBuffer = codeBuffers[codeBufferSize];
                System.Buffer.BlockCopy(code, index, codeBuffer, 0, codeBufferSize);
                int textChar;
                if (!this.codes.TryGetValue(new ByteArray(codeBuffer), out textChar))
                {
                    if ((codeBufferSize < this.charCodeMaxLength) && (codeBufferSize < codeLength - index))
                    {
                        codeBufferSize++;
                        continue;
                    }
                    else // Missing character.
                    {
                        switch (this.Document.Configuration.EncodingFallback)
                        {
                            case EncodingFallbackEnum.Exclusion:
                                textChar = -1;
                                break;
                            case EncodingFallbackEnum.Substitution:
                                textChar = this.defaultCode;
                                break;
                            case EncodingFallbackEnum.Exception:
                                throw new DecodeException(code, index);
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
                if (textChar > -1)
                {
                    _ = textBuilder.Append((char)textChar);
                }
                index += codeBufferSize;
                codeBufferSize = 1;
            }
            return textBuilder.ToString();
        }

        ///
        /// <summary>
        /// Gets the internal representation of the given text.
        /// </summary>
        /// <param name="text">Text to encode.</param>
        /// <exception cref="EncodeException"/>
        ///
        public byte[] Encode(string text)
        {
            var encodedStream = new io::MemoryStream();
            for (int index = 0, length = text.Length; index < length; index++)
            {
                int textCode = text[index];
                if (textCode < 32) // NOTE: Control characters are ignored [FIX:7].
                {
                    continue;
                }

                var code = this.codes.GetKey(textCode);
                if (code == null) // Missing glyph.
                {
                    switch (this.Document.Configuration.EncodingFallback)
                    {
                        case EncodingFallbackEnum.Exclusion:
                            continue;
                        case EncodingFallbackEnum.Substitution:
                            code = this.codes.GetKey(this.defaultCode);
                            break;
                        case EncodingFallbackEnum.Exception:
                            throw new EncodeException(text, index);
                        default:
                            throw new NotImplementedException();
                    }
                }

                var charCode = code.Data;
                encodedStream.Write(charCode, 0, charCode.Length);
                _ = this.usedCodes.Add(textCode);
            }
            encodedStream.Close();
            return encodedStream.ToArray();
        }

        public override bool Equals(object obj)
        { return (obj != null) && obj.GetType().Equals(this.GetType()) && ((Font)obj).Name.Equals(this.Name); }

        ///
        /// <summary>
        /// Creates the representation of a font.
        /// </summary>
        ///
        public static Font Get(Document context, string path)
        { return Get(context, new Stream(new io::FileStream(path, io::FileMode.Open, io::FileAccess.Read))); }

        ///
        /// <summary>
        /// Creates the representation of a font.
        /// </summary>
        ///
        public static Font Get(Document context, IInputStream fontData)
        {
            if (OpenFontParser.IsOpenFont(fontData))
            {
                return CompositeFont.Get(context, fontData);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        ///
        /// <summary>
        /// Gets the vertical offset from the baseline to the ascender line (ascent), scaled to the given font size. The
        /// value is a positive number.
        /// </summary>
        /// <param name="size">Font size.</param>
        ///
        public double GetAscent(double size) { return this.Ascent * GetScalingFactor(size); }

        ///
        /// <summary>
        /// Gets the vertical offset from the baseline to the descender line (descent), scaled to the given font size.
        /// The value is a negative number.
        /// </summary>
        /// <param name="size">Font size.</param>
        ///
        public double GetDescent(double size) { return this.Descent * GetScalingFactor(size); }

        public override int GetHashCode() { return this.Name.GetHashCode(); }

        ///
        /// <summary>
        /// Gets the unscaled height of the given character.
        /// </summary>
        /// <param name="textChar">Character whose height has to be calculated.</param>
        ///
        public double GetHeight(char textChar)
        {
            /*
              TODO: Calculate actual text height through glyph bounding box.
            */
            if (this.textHeight == -1)
            {
                this.textHeight = this.Ascent - this.Descent;
            }
            return this.textHeight;
        }

        ///
        /// <summary>
        /// Gets the unscaled height of the given text.
        /// </summary>
        /// <param name="text">Text whose height has to be calculated.</param>
        ///
        public double GetHeight(string text)
        {
            double height = 0;
            for (int index = 0, length = text.Length; index < length; index++)
            {
                var charHeight = this.GetHeight(text[index]);
                if (charHeight > height)
                {
                    height = charHeight;
                }
            }
            return height;
        }

        ///
        /// <summary>
        /// Gets the height of the given character, scaled to the given font size.
        /// </summary>
        /// <param name="textChar">Character whose height has to be calculated.</param>
        /// <param name="size">Font size.</param>
        ///
        public double GetHeight(char textChar, double size)
        { return this.GetHeight(textChar) * GetScalingFactor(size); }

        ///
        /// <summary>
        /// Gets the height of the given text, scaled to the given font size.
        /// </summary>
        /// <param name="text">Text whose height has to be calculated.</param>
        /// <param name="size">Font size.</param>
        ///
        public double GetHeight(string text, double size) { return this.GetHeight(text) * GetScalingFactor(size); }

        ///
        /// <summary>
        /// Gets the width (kerning inclusive) of the given text, scaled to the given font size.
        /// </summary>
        /// <param name="text">Text whose width has to be calculated.</param>
        /// <param name="size">Font size.</param>
        /// <exception cref="EncodeException"/>
        ///
        public double GetKernedWidth(string text, double size)
        { return (this.GetWidth(text) + this.GetKerning(text)) * GetScalingFactor(size); }

        ///
        /// <summary>
        /// Gets the unscaled kerning width inside the given text.
        /// </summary>
        /// <param name="text">Text whose kerning has to be calculated.</param>
        ///
        public int GetKerning(string text)
        {
            var kerning = 0;
            for (int index = 0, length = text.Length - 1; index < length; index++)
            {
                kerning += this.GetKerning(text[index], text[index + 1]);
            }
            return kerning;
        }

        ///
        /// <summary>
        /// Gets the unscaled kerning width between two given characters.
        /// </summary>
        /// <param name="textChar1">Left character.</param>
        /// <param name="textChar2">Right character,</param>
        ///
        public int GetKerning(char textChar1, char textChar2)
        {
            if (this.glyphKernings == null)
            {
                return 0;
            }

            int textChar1Index;
            if (!this.glyphIndexes.TryGetValue(textChar1, out textChar1Index))
            {
                return 0;
            }

            int textChar2Index;
            if (!this.glyphIndexes.TryGetValue(textChar2, out textChar2Index))
            {
                return 0;
            }

            int kerning;
            return this.glyphKernings
                       .TryGetValue(
                           textChar1Index <<
                                   (16 // Left-hand glyph index.
                +
                                       textChar2Index), // Right-hand glyph index.
                           out kerning)
                ? kerning
                : 0;
        }

        ///
        /// <summary>
        /// Gets the kerning width inside the given text, scaled to the given font size.
        /// </summary>
        /// <param name="text">Text whose kerning has to be calculated.</param>
        /// <param name="size">Font size.</param>
        ///
        public double GetKerning(string text, double size) { return this.GetKerning(text) * GetScalingFactor(size); }

        ///
        /// <summary>
        /// Gets the line height, scaled to the given font size.
        /// </summary>
        /// <param name="size">Font size.</param>
        ///
        public double GetLineHeight(double size) { return this.LineHeight * GetScalingFactor(size); }

        ///
        /// <summary>
        /// Gets the scaling factor to be applied to unscaled metrics to get actual measures.
        /// </summary>
        ///
        public static double GetScalingFactor(double size) { return 0.001 * size; }

        ///
        /// <summary>
        /// Gets the unscaled width of the given character.
        /// </summary>
        /// <param name="textChar">Character whose width has to be calculated.</param>
        /// <exception cref="EncodeException"/>
        ///
        public int GetWidth(char textChar)
        {
            int glyphIndex;
            if (!this.glyphIndexes.TryGetValue(textChar, out glyphIndex))
            {
                switch (this.Document.Configuration.EncodingFallback)
                {
                    case EncodingFallbackEnum.Exclusion:
                        return 0;
                    case EncodingFallbackEnum.Substitution:
                        return this.DefaultWidth;
                    case EncodingFallbackEnum.Exception:
                        throw new EncodeException(textChar);
                    default:
                        throw new NotImplementedException();
                }
            }

            int glyphWidth;
            return this.glyphWidths.TryGetValue(glyphIndex, out glyphWidth) ? glyphWidth : this.DefaultWidth;
        }

        ///
        /// <summary>
        /// Gets the unscaled width (kerning exclusive) of the given text.
        /// </summary>
        /// <param name="text">Text whose width has to be calculated.</param>
        /// <exception cref="EncodeException"/>
        ///
        public int GetWidth(string text)
        {
            var width = 0;
            for (int index = 0, length = text.Length; index < length; index++)
            {
                width += this.GetWidth(text[index]);
            }
            return width;
        }

        ///
        /// <summary>
        /// Gets the width of the given character, scaled to the given font size.
        /// </summary>
        /// <param name="textChar">Character whose height has to be calculated.</param>
        /// <param name="size">Font size.</param>
        /// <exception cref="EncodeException"/>
        ///
        public double GetWidth(char textChar, double size) { return this.GetWidth(textChar) * GetScalingFactor(size); }

        ///
        /// <summary>
        /// Gets the width (kerning exclusive) of the given text, scaled to the given font size.
        /// </summary>
        /// <param name="text">Text whose width has to be calculated.</param>
        /// <param name="size">Font size.</param>
        /// <exception cref="EncodeException"/>
        ///
        public double GetWidth(string text, double size) { return this.GetWidth(text) * GetScalingFactor(size); }

        ///
        /// <summary>
        /// Wraps a font reference into a font object.
        /// </summary>
        /// <param name="baseObject">Font base object.</param>
        /// <returns>Font object associated to the reference.</returns>
        ///
        public static Font Wrap(PdfDirectObject baseObject)
        {
            if (baseObject == null)
            {
                return null;
            }

            var reference = (PdfReference)baseObject;
            // Has the font been already instantiated?
            /*
              NOTE: Font structures are reified as complex objects, both IO- and CPU-intensive to load.
              So, it's convenient to retrieve them from a common cache whenever possible.
            */
            var cache = reference.IndirectObject.File.Document.Cache;
            if (cache.ContainsKey(reference))
            {
                return (Font)cache[reference];
            }

            var fontDictionary = (PdfDictionary)reference.DataObject;
            var fontType = (PdfName)fontDictionary[PdfName.Subtype];
            if (fontType == null)
            {
                throw new Exception($"Font type undefined (reference: {reference})");
            }

            if (fontType.Equals(PdfName.Type1)) // Type 1.
            {
                if (!fontDictionary.ContainsKey(PdfName.FontDescriptor)) // Standard Type 1.
                {
                    return new StandardType1Font(reference);
                }
                else // Custom Type 1.
                {
                    var fontDescriptor = (PdfDictionary)fontDictionary.Resolve(PdfName.FontDescriptor);
                    if (fontDescriptor.ContainsKey(PdfName.FontFile3) &&
                        ((PdfName)((PdfStream)fontDescriptor.Resolve(PdfName.FontFile3)).Header.Resolve(PdfName.Subtype)).Equals(
                            PdfName.OpenType)) // OpenFont/CFF.
                    {
                        throw new NotImplementedException();
                    }
                    else // Non-OpenFont Type 1.
                    {
                        return new Type1Font(reference);
                    }
                }
            }
            else if (fontType.Equals(PdfName.TrueType)) // TrueType.
            {
                return new TrueTypeFont(reference);
            }
            else if (fontType.Equals(PdfName.Type0)) // OpenFont.
            {
                var cidFontDictionary = (PdfDictionary)((PdfArray)fontDictionary.Resolve(PdfName.DescendantFonts)).Resolve(
                    0);
                var cidFontType = (PdfName)cidFontDictionary[PdfName.Subtype];
                if (cidFontType.Equals(PdfName.CIDFontType0)) // OpenFont/CFF.
                {
                    return new Type0Font(reference);
                }
                else if (cidFontType.Equals(PdfName.CIDFontType2)) // OpenFont/TrueType.
                {
                    return new Type2Font(reference);
                }
                else
                {
                    throw new NotImplementedException($"Type 0 subtype {cidFontType} not supported yet.");
                }
            }
            else if (fontType.Equals(PdfName.Type3)) // Type 3.
            {
                return new Type3Font(reference);
            }
            else if (fontType.Equals(PdfName.MMType1)) // MMType1.
            {
                return new MMType1Font(reference);
            }
            else // Unknown.
            {
                throw new NotSupportedException($"Unknown font type: {fontType} (reference: {reference})");
            }
        }

        ///
        /// <summary>
        /// Gets the unscaled vertical offset from the baseline to the ascender line (ascent). The value is a positive
        /// number.
        /// </summary>
        ///
        public virtual double Ascent
        {
            get
            {
                var ascentObject = (IPdfNumber)this.GetDescriptorValue(PdfName.Ascent);
                return (ascentObject != null) ? ascentObject.DoubleValue : 750;
            }
        }

        ///
        /// <summary>
        /// Gets the Unicode code-points supported by this font.
        /// </summary>
        ///
        public ICollection<int> CodePoints => this.glyphIndexes.Keys;

        ///
        /// <summary>
        /// Gets/Sets the Unicode codepoint used to substitute missing characters.
        /// </summary>
        /// <exception cref="EncodeException">If the value is not mapped in the font's encoding.</exception>
        ///
        public int DefaultCode
        {
            get => this.defaultCode;
            set
            {
                if (!this.glyphIndexes.ContainsKey(value))
                {
                    throw new EncodeException((char)value);
                }

                this.defaultCode = value;
            }
        }

        ///
        /// <summary>
        /// Gets the unscaled vertical offset from the baseline to the descender line (descent). The value is a negative
        /// number.
        /// </summary>
        ///
        public virtual double Descent
        {
            get
            {
                /*
                  NOTE: Sometimes font descriptors specify positive descent, therefore normalization is
                  required [FIX:27].
                */
                var descentObject = (IPdfNumber)this.GetDescriptorValue(PdfName.Descent);
                return -Math.Abs((descentObject != null) ? descentObject.DoubleValue : 250);
            }
        }

        ///
        /// <summary>
        /// Gets the font descriptor flags.
        /// </summary>
        ///
        public virtual FlagsEnum Flags
        {
            get
            {
                var flagsObject = (PdfInteger)this.GetDescriptorValue(PdfName.Flags);
                return (flagsObject != null) ? ((FlagsEnum)Enum.ToObject(typeof(FlagsEnum), flagsObject.RawValue)) : 0;
            }
        }

        ///
        /// <summary>
        /// Gets the unscaled line height.
        /// </summary>
        ///
        public double LineHeight => this.Ascent - this.Descent;

        ///
        /// <summary>
        /// Gets the PostScript name of the font.
        /// </summary>
        ///
        public string Name => ((PdfName)this.BaseDataObject[PdfName.BaseFont]).ToString();

        ///
        /// <summary>
        /// Gets whether the font encoding is custom (that is non-Unicode).
        /// </summary>
        ///
        public bool Symbolic => this.symbolic;

        ///
        /// <summary>
        /// Font descriptor flags [PDF:1.6:5.7.1].
        /// </summary>
        ///
        [Flags]
        public enum FlagsEnum
        {
            ///
            /// <summary>
            /// All glyphs have the same width.
            /// </summary>
            ///
            FixedPitch = 0x1,
            ///
            /// <summary>
            /// Glyphs have serifs.
            /// </summary>
            ///
            Serif = 0x2,
            ///
            /// <summary>
            /// Font contains glyphs outside the Adobe standard Latin character set.
            /// </summary>
            ///
            Symbolic = 0x4,
            ///
            /// <summary>
            /// Glyphs resemble cursive handwriting.
            /// </summary>
            ///
            Script = 0x8,
            ///
            /// <summary>
            /// Font uses the Adobe standard Latin character set.
            /// </summary>
            ///
            Nonsymbolic = 0x20,
            ///
            /// <summary>
            /// Glyphs have dominant vertical strokes that are slanted.
            /// </summary>
            ///
            Italic = 0x40,
            ///
            /// <summary>
            /// Font contains no lowercase letters.
            /// </summary>
            ///
            AllCap = 0x10000,
            ///
            /// <summary>
            /// Font contains both uppercase and lowercase letters.
            /// </summary>
            ///
            SmallCap = 0x20000,
            ///
            /// <summary>
            /// Thicken bold glyphs at small text sizes.
            /// </summary>
            ///
            ForceBold = 0x40000
        }

        ///
        /// <summary>
        /// Average glyph width.
        /// </summary>
        ///
        private int averageWidth = UndefinedWidth;
        ///
        /// <summary>
        /// Default Unicode for missing characters.
        /// </summary>
        ///
        private int defaultCode = UndefinedDefaultCode;
        ///
        /// <summary>
        /// Default glyph width.
        /// </summary>
        ///
        private int defaultWidth = UndefinedWidth;
    }
}
