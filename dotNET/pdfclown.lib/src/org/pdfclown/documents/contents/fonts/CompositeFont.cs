//-----------------------------------------------------------------------
// <copyright file="CompositeFont.cs" company="">
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
    using org.pdfclown.objects;
    using org.pdfclown.util;
    using bytes = org.pdfclown.bytes;
    using drawing = System.Drawing;

    ///
    /// <summary>
    /// Composite font, also called Type 0 font [PDF:1.6:5.6].
    /// </summary>
    /// <remarks>
    /// Do not confuse it with <see cref="Type0Font">Type 0 CIDFont</see>: the latter is a composite font descendant
    /// describing glyphs based on Adobe Type 1 font format.
    /// </remarks>
    ///
    [PDF(VersionEnum.PDF12)]
    public abstract class CompositeFont : Font
    {
        protected CompositeFont(PdfDirectObject baseObject) : base(baseObject)
        {
        }


        internal CompositeFont(Document context, OpenFontParser parser) : base(context) { this.Load(parser); }

        ///
        /// <summary>
        /// Loads the font data.
        /// </summary>
        ///
        private void Load(OpenFontParser parser)
        {
            this.glyphIndexes = parser.GlyphIndexes;
            this.glyphKernings = parser.GlyphKernings;
            this.glyphWidths = parser.GlyphWidths;

            var baseDataObject = this.BaseDataObject;

            // BaseFont.
            baseDataObject[PdfName.BaseFont] = new PdfName(parser.FontName);

            // Subtype.
            baseDataObject[PdfName.Subtype] = PdfName.Type0;

            // Encoding.
            baseDataObject[PdfName.Encoding] = PdfName.IdentityH; //TODO: this is a simplification (to refine later).

            // Descendant font.
            var cidFontDictionary = new PdfDictionary(
                new PdfName[] { PdfName.Type },
                new PdfDirectObject[] { PdfName.Font }); // CIDFont dictionary [PDF:1.6:5.6.3].
            // Subtype.
            // FIXME: verify proper Type 0 detection.
            cidFontDictionary[PdfName.Subtype] = PdfName.CIDFontType2;

            // BaseFont.
            cidFontDictionary[PdfName.BaseFont] = new PdfName(parser.FontName);

            // CIDSystemInfo.
            cidFontDictionary[PdfName.CIDSystemInfo] = new PdfDictionary(
                new PdfName[] { PdfName.Registry, PdfName.Ordering, PdfName.Supplement },
                new PdfDirectObject[] { new PdfTextString("Adobe"), new PdfTextString("Identity"), PdfInteger.Get(0) }); // Generic predefined CMap (Identity-H/V (Adobe-Identity-0)) [PDF:1.6:5.6.4].

            // FontDescriptor.
            cidFontDictionary[PdfName.FontDescriptor] = this.Load_CreateFontDescriptor(parser);

            // Encoding.
            this.Load_CreateEncoding(baseDataObject, cidFontDictionary);
            baseDataObject[PdfName.DescendantFonts] = new PdfArray(
                new PdfDirectObject[] { this.File.Register(cidFontDictionary) });

            this.Load();
        }

        ///
        /// <summary>
        /// Creates the character code mapping for composite fonts.
        /// </summary>
        ///
        private void Load_CreateEncoding(PdfDictionary font, PdfDictionary cidFont)
        {
            /*
              NOTE: Composite fonts map text shown by content stream strings through a 2-level encoding
              scheme:
                character code -> CID (character index) -> GID (glyph index)
              This works for rendering purposes, but if we want our text data to be intrinsically meaningful,
              we need a further mapping towards some standard character identification scheme (Unicode):
                Unicode <- character code -> CID -> GID
              Such mapping may be provided by a known CID collection or (in case of custom encodings like
              Identity-H) by an explicit ToUnicode CMap.
              CID -> GID mapping is typically identity, that is CIDS correspond to GIDS, so we don't bother
              about that. Our base encoding is Identity-H, that is character codes correspond to CIDs;
              however, sometimes a font maps multiple Unicode codepoints to the same GID (for example, the
              hyphen glyph may be associated to the hyphen (\u2010) and minus (\u002D) symbols), breaking
              the possibility to recover their original Unicode values once represented as character codes
              in content stream strings. In this case, we are forced to remap the exceeding codes and
              generate an explicit CMap (TODO: I tried to emit a differential CMap using the usecmap
              operator in order to import Identity-H as base encoding, but it failed in several engines
              (including Acrobat, Ghostscript, Poppler, whilst it surprisingly worked with pdf.js), so we
              have temporarily to stick with full CMaps).
            */

            // Encoding [PDF:1.7:5.6.1,5.6.4].
            PdfDirectObject encodingObject = PdfName.IdentityH;
            SortedDictionary<ByteArray, int> sortedCodes;
            this.codes = new BiDictionary<ByteArray, int>(this.glyphIndexes.Count);
            var lastRemappedCharCodeValue = 0;
            IList<int> removedGlyphIndexKeys = null;
            foreach (var glyphIndexEntry in this.glyphIndexes.ToList())
            {
                var glyphIndex = glyphIndexEntry.Value;
                var charCode = new ByteArray(new byte[] { (byte)((glyphIndex >> 8) & 0xFF), (byte)(glyphIndex & 0xFF) });

                // Checking for multiple Unicode codepoints which map to the same glyph index...
                /*
                  NOTE: In case the same glyph index maps to multiple Unicode codepoints, we are forced to
                  alter the identity encoding creating distinct cmap entries for the exceeding codepoints.
                */
                if (this.codes.ContainsKey(charCode))
                {
                    if (glyphIndex == 0) // .notdef glyph already mapped.
                    {
                        if (removedGlyphIndexKeys == null)
                        {
                            removedGlyphIndexKeys = new List<int>();
                        }
                        removedGlyphIndexKeys.Add(glyphIndexEntry.Key);
                        continue;
                    }

                    // Assigning the new character code...
                    /*
                      NOTE: As our base encoding is identity, we have to look for a value that doesn't
                      collide with existing glyph indices.
                    */
                    while (this.glyphIndexes.ContainsValue(++lastRemappedCharCodeValue))
                    {
                        ;
                    }

                    charCode.Data[0] = (byte)((lastRemappedCharCodeValue >> 8) & 0xFF);
                    charCode.Data[1] = (byte)(lastRemappedCharCodeValue & 0xFF);
                }
                else if (glyphIndex == 0) // .notdef glyph.
                {
                    this.DefaultCode = glyphIndexEntry.Key;
                }

                this.codes[charCode] = glyphIndexEntry.Key;
            }
            if (removedGlyphIndexKeys != null)
            {
                foreach (var removedGlyphIndexKey in removedGlyphIndexKeys)
                {
                    _ = this.glyphIndexes.Remove(removedGlyphIndexKey);
                }
            }
            sortedCodes = new SortedDictionary<ByteArray, int>(this.codes);
            if (lastRemappedCharCodeValue > 0) // Custom encoding.
            {
                var cmapName = "Custom";
                var cmapBuffer = CMapBuilder.Build(
                    CMapBuilder.EntryTypeEnum.CID,
                    cmapName,
                    sortedCodes,
                    delegate (KeyValuePair<ByteArray, int> codeEntry)
                    {
                        return this.glyphIndexes[codeEntry.Value];
                    });
                encodingObject = this.File
                                     .Register(
                                         new PdfStream(
                                             new PdfDictionary(
                                                 new PdfName[] { PdfName.Type, PdfName.CMapName, PdfName.CIDSystemInfo },
                                                 new PdfDirectObject[]
                                                 {
                                                     PdfName.CMap,
                                                     new PdfName(cmapName),
                                                     new PdfDictionary(
                                                     new PdfName[]
                                                         {
                                                             PdfName.Registry,
                                                             PdfName.Ordering,
                                                             PdfName.Supplement
                                                         },
                                                     new PdfDirectObject[]
                                                         {
                                                             PdfTextString.Get("Adobe"),
                                                             PdfTextString.Get("Identity"),
                                                             PdfInteger.Get(0)
                                                         })
                                                 }),
                                             cmapBuffer));
            }
            font[PdfName.Encoding] = encodingObject; // Character-code-to-CID mapping.
            cidFont[PdfName.CIDToGIDMap] = PdfName.Identity; // CID-to-glyph-index mapping.

            // ToUnicode [PDF:1.6:5.9.2].
            PdfDirectObject toUnicodeObject = null;
            var toUnicodeBuffer = CMapBuilder.Build(
                CMapBuilder.EntryTypeEnum.BaseFont,
                null,
                sortedCodes,
                delegate (KeyValuePair<ByteArray, int> codeEntry)
                {
                    return codeEntry.Value;
                });
            toUnicodeObject = this.File.Register(new PdfStream(toUnicodeBuffer));
            font[PdfName.ToUnicode] = toUnicodeObject; // Character-code-to-Unicode mapping.

            // Glyph widths.
            var widthsObject = new PdfArray();
            var lastGlyphIndex = -10;
            PdfArray lastGlyphWidthRangeObject = null;
            foreach (var glyphIndex in this.glyphIndexes.Values.OrderBy(x => x).ToList())
            {
                int width;
                if (!this.glyphWidths.TryGetValue(glyphIndex, out width))
                {
                    width = 0;
                }
                if (glyphIndex - lastGlyphIndex != 1)
                {
                    widthsObject.Add(PdfInteger.Get(glyphIndex));
                    widthsObject.Add(lastGlyphWidthRangeObject = new PdfArray());
                }
                lastGlyphWidthRangeObject.Add(PdfInteger.Get(width));
                lastGlyphIndex = glyphIndex;
            }
            cidFont[PdfName.W] = widthsObject; // Glyph widths.
        }

        ///
        /// <summary>
        /// Creates the font descriptor.
        /// </summary>
        ///
        private PdfReference Load_CreateFontDescriptor(OpenFontParser parser)
        {
            var fontDescriptor = new PdfDictionary();
            var metrics = parser.Metrics;

            // Type.
            fontDescriptor[PdfName.Type] = PdfName.FontDescriptor;

            // FontName.
            fontDescriptor[PdfName.FontName] = this.BaseDataObject[PdfName.BaseFont];

            // Flags [PDF:1.6:5.7.1].
            FlagsEnum flags = 0;
            if (metrics.IsFixedPitch)
            {
                flags |= FlagsEnum.FixedPitch;
            }
            if (metrics.IsCustomEncoding)
            {
                flags |= FlagsEnum.Symbolic;
            }
            else
            {
                flags |= FlagsEnum.Nonsymbolic;
            }
            fontDescriptor[PdfName.Flags] = PdfInteger.Get(Convert.ToInt32(flags));

            // FontBBox.
            fontDescriptor[PdfName.FontBBox] = new Rectangle(
                new drawing::PointF(metrics.XMin * metrics.UnitNorm, metrics.YMin * metrics.UnitNorm),
                new drawing::PointF(metrics.XMax * metrics.UnitNorm, metrics.YMax * metrics.UnitNorm)).BaseDataObject;

            // ItalicAngle.
            fontDescriptor[PdfName.ItalicAngle] = PdfReal.Get(metrics.ItalicAngle);

            // Ascent.
            fontDescriptor[PdfName.Ascent] = PdfReal.Get(
                (metrics.STypoAscender == 0)
                    ? (metrics.Ascender * metrics.UnitNorm)
                    : (((metrics.STypoLineGap == 0) ? metrics.SCapHeight : metrics.STypoAscender) * metrics.UnitNorm));

            // Descent.
            fontDescriptor[PdfName.Descent] = PdfReal.Get(
                (metrics.STypoDescender == 0)
                    ? (metrics.Descender * metrics.UnitNorm)
                    : (metrics.STypoDescender * metrics.UnitNorm));

            // CapHeight.
            fontDescriptor[PdfName.CapHeight] = PdfReal.Get(metrics.SCapHeight * metrics.UnitNorm);

            // StemV.
            /*
              NOTE: '100' is just a rule-of-thumb value, 'cause I've still to solve the
              'cvt' table puzzle (such a harsh headache!) for TrueType fonts...
              TODO:IMPL TrueType and CFF stemv real value to extract!!!
            */
            fontDescriptor[PdfName.StemV] = PdfInteger.Get(100);

            // FontFile.
            fontDescriptor[PdfName.FontFile2] = this.File
                                                    .Register(
                                                        new PdfStream(new bytes::Buffer(parser.FontData.ToByteArray())));
            return this.File.Register(fontDescriptor);
        }

        protected override PdfDataObject GetDescriptorValue(PdfName key)
        { return ((PdfDictionary)this.CIDFontDictionary.Resolve(PdfName.FontDescriptor)).Resolve(key); }

        protected void LoadEncoding()
        {
            var encodingObject = this.BaseDataObject.Resolve(PdfName.Encoding);

            // CMap [PDF:1.6:5.6.4].
            var cmap = CMap.Get(encodingObject);

            // 1. Unicode.
            if (this.codes == null)
            {
                this.codes = new BiDictionary<ByteArray, int>();
                if ((encodingObject is PdfName) &&
                    !(encodingObject.Equals(PdfName.IdentityH) || encodingObject.Equals(PdfName.IdentityV)))
                {
                    /*
                      NOTE: According to [PDF:1.6:5.9.1], the fallback method to retrieve
                      the character-code-to-Unicode mapping implies getting the UCS2 CMap
                      (Unicode value to CID) corresponding to the font's one (character code to CID);
                      CIDs are the bridge from character codes to Unicode values.
                    */
                    BiDictionary<ByteArray, int> ucs2CMap;
                    var cidSystemInfo = (PdfDictionary)this.CIDFontDictionary.Resolve(PdfName.CIDSystemInfo);
                    var registry = (string)((PdfTextString)cidSystemInfo[PdfName.Registry]).Value;
                    var ordering = (string)((PdfTextString)cidSystemInfo[PdfName.Ordering]).Value;
                    var ucs2CMapName = $"{registry}-{ordering}-UCS2";
                    ucs2CMap = new BiDictionary<ByteArray, int>(CMap.Get(ucs2CMapName));
                    if (ucs2CMap.Count > 0)
                    {
                        foreach (var cmapEntry in cmap)
                        {
                            this.codes[cmapEntry.Key] = ConvertUtils.ByteArrayToInt(
                                ucs2CMap.GetKey(cmapEntry.Value).Data);
                        }
                    }
                }
                if (this.codes.Count == 0)
                {
                    /*
                      NOTE: In case no clue is available to determine the Unicode resolution map,
                      the font is considered symbolic and an identity map is synthesized instead.
                    */
                    this.symbolic = true;
                    foreach (var cmapEntry in cmap)
                    {
                        this.codes[cmapEntry.Key] = ConvertUtils.ByteArrayToInt(cmapEntry.Key.Data);
                    }
                }
            }

            // 2. Glyph indexes.
            /*
            TODO: gids map for glyph indexes as glyphIndexes is used to map cids!!!
            */
            // Character-code-to-CID mapping [PDF:1.6:5.6.4,5].
            this.glyphIndexes = new Dictionary<int, int>();
            foreach (var cmapEntry in cmap)
            {
                if (!this.codes.ContainsKey(cmapEntry.Key))
                {
                    continue;
                }

                this.glyphIndexes[this.codes[cmapEntry.Key]] = cmapEntry.Value;
            }
        }

        protected override void OnLoad()
        {
            this.LoadEncoding();

            // Glyph widths.

            this.glyphWidths = new Dictionary<int, int>();
            var glyphWidthObjects = (PdfArray)this.CIDFontDictionary.Resolve(PdfName.W);
            if (glyphWidthObjects != null)
            {
                for (var iterator = glyphWidthObjects.GetEnumerator(); iterator.MoveNext();)
                {
                    //TODO: this algorithm is valid only in case cid-to-gid mapping is identity (see cidtogid map)!!
                    /*
                      NOTE: Font widths are grouped in one of the following formats [PDF:1.6:5.6.3]:
                        1. startCID [glyphWidth1 glyphWidth2 ... glyphWidthn]
                        2. startCID endCID glyphWidth
                    */
                    var startCID = ((PdfInteger)iterator.Current).RawValue;
                    _ = iterator.MoveNext();
                    var glyphWidthObject2 = iterator.Current;
                    if (glyphWidthObject2 is PdfArray) // Format 1: startCID [glyphWidth1 glyphWidth2 ... glyphWidthn].
                    {
                        var cID = startCID;
                        foreach (var glyphWidthObject in (PdfArray)glyphWidthObject2)
                        {
                            this.glyphWidths[cID++] = ((IPdfNumber)glyphWidthObject).IntValue;
                        }
                    }
                    else // Format 2: startCID endCID glyphWidth.
                    {
                        var endCID = ((PdfInteger)glyphWidthObject2).RawValue;
                        _ = iterator.MoveNext();
                        var glyphWidth = ((IPdfNumber)iterator.Current).IntValue;
                        for (var cID = startCID; cID <= endCID; cID++)
                        {
                            this.glyphWidths[cID] = glyphWidth;
                        }
                    }
                }
            }
            // Default glyph width.

            var defaultWidthObject = (PdfInteger)this.BaseDataObject[PdfName.DW];
            if (defaultWidthObject != null)
            {
                this.DefaultWidth = defaultWidthObject.IntValue;
            }
        }

        ///
        /// <summary>
        /// Gets the CIDFont dictionary that is the descendant of this composite font.
        /// </summary>
        ///
        protected PdfDictionary CIDFontDictionary => (PdfDictionary)((PdfArray)this.BaseDataObject
                                                                                   .Resolve(PdfName.DescendantFonts)).Resolve(
            0);

        public static new CompositeFont Get(Document context, bytes::IInputStream fontData)
        {
            var parser = new OpenFontParser(fontData);
            switch (parser.OutlineFormat)
            {
                case OpenFontParser.OutlineFormatEnum.PostScript:
                    return new Type0Font(context, parser);
                case OpenFontParser.OutlineFormatEnum.TrueType:
                    return new Type2Font(context, parser);
            }
            throw new NotSupportedException("Unknown composite font format.");
        }
    }
}
