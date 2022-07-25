/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/


namespace org.pdfclown.documents.contents.fonts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using org.pdfclown.objects;
    using org.pdfclown.util;
    using bytes = org.pdfclown.bytes;

    /**
      <summary>Standard Type 1 font [PDF:1.6:5.5.1].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class StandardType1Font
      : Type1Font
    {

        internal StandardType1Font(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public StandardType1Font(
Document context,
FamilyEnum family,
bool bold,
bool italic
) : base(context)
        {
            var fontName = family.ToString();
            switch (family)
            {
                case FamilyEnum.Symbol:
                case FamilyEnum.ZapfDingbats:
                    break;
                case FamilyEnum.Times:
                    if (bold)
                    {
                        fontName += "-Bold";
                        if (italic)
                        { fontName += "Italic"; }
                    }
                    else if (italic)
                    { fontName += "-Italic"; }
                    else
                    { fontName += "-Roman"; }
                    break;
                default:
                    if (bold)
                    {
                        fontName += "-Bold";
                        if (italic)
                        { fontName += "Oblique"; }
                    }
                    else if (italic)
                    { fontName += "-Oblique"; }
                    break;
            }
            var encodingName = IsSymbolic(family) ? null : PdfName.WinAnsiEncoding;

            this.Create(fontName, encodingName);
        }

        /**
  <summary>Creates the font structures.</summary>
*/
        private void Create(
          string fontName,
          PdfName encodingName
          )
        {
            /*
              NOTE: Standard Type 1 fonts SHOULD omit extended font descriptions [PDF:1.6:5.5.1].
            */
            // Subtype.
            this.BaseDataObject[PdfName.Subtype] = PdfName.Type1;
            // BaseFont.
            this.BaseDataObject[PdfName.BaseFont] = new PdfName(fontName);
            // Encoding.
            if (encodingName != null)
            { this.BaseDataObject[PdfName.Encoding] = encodingName; }

            this.Load();
        }

        private static bool IsSymbolic(
FamilyEnum value
)
        {
            switch (value)
            {
                case FamilyEnum.Courier:
                case FamilyEnum.Helvetica:
                case FamilyEnum.Times:
                    return false;
                case FamilyEnum.Symbol:
                case FamilyEnum.ZapfDingbats:
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        /**
          <summary>Loads the font metrics.</summary>
        */
        private void Load(
          string fontName
          )
        {
            try
            {
                using (var fontMetricsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"fonts.afm.{fontName}"))
                {
                    var parser = new AfmParser(new bytes::Stream(fontMetricsStream));
                    this.metrics = parser.Metrics;
                    this.symbolic = this.metrics.IsCustomEncoding;
                    this.glyphIndexes = parser.GlyphIndexes;
                    this.glyphKernings = parser.GlyphKernings;
                    this.glyphWidths = parser.GlyphWidths;
                }
            }
            catch (Exception e)
            { throw new Exception($"Failed to load '{fontName}'", e); }
        }

        protected override IDictionary<ByteArray, int> GetBaseEncoding(
  PdfName encodingName
  )
        {
            if ((encodingName == null) && this.Symbolic)
            {
                /*
                  NOTE: Symbolic standard fonts use custom encodings.
                */
                encodingName = (PdfName)this.BaseDataObject[PdfName.BaseFont];
            }
            return base.GetBaseEncoding(encodingName);
        }

        protected override void OnLoad(
          )
        {
            /*
              NOTE: Standard Type 1 fonts ordinarily omit their descriptor;
              otherwise, when overridden they degrade to a common Type 1 font.
              Metrics of non-overridden Standard Type 1 fonts MUST be loaded from resources.
            */
            this.Load(((PdfName)this.BaseDataObject[PdfName.BaseFont]).StringValue);

            base.OnLoad();
        }

        public override double Ascent => this.metrics.Ascender;

        public override double Descent => this.metrics.Descender;

        public override FlagsEnum Flags => 0;
        /**
  <summary>Standard Type 1 font families [PDF:1.6:5.5.1].</summary>
*/
        public enum FamilyEnum
        {
            Courier,
            Helvetica,
            Times,
            Symbol,
            ZapfDingbats
        };
    }
}
