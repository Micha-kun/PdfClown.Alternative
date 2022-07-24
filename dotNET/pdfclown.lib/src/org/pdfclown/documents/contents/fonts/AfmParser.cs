//-----------------------------------------------------------------------
// <copyright file="AfmParser.cs" company="">
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
    using System.Collections.Generic;

    using System.Text.RegularExpressions;
    using org.pdfclown.bytes;
    using org.pdfclown.util;

    ///
    /// <summary>
    /// AFM file format parser [AFM:4.1].
    /// </summary>
    ///
    public sealed class AfmParser
    {
        internal AfmParser(IInputStream fontData)
        {
            this.FontData = fontData;

            this.Load();
        }

        private void Load()
        {
            this.Metrics = new FontMetrics();
            this.LoadFontHeader();
            this.LoadCharMetrics();
            this.LoadKerningData();
        }

        ///
        /// <summary>
        /// Loads individual character metrics [AFM:4.1:3,4,4.4,8].
        /// </summary>
        ///
        private void LoadCharMetrics()
        {
            this.GlyphIndexes = new Dictionary<int, int>();
            this.GlyphWidths = new Dictionary<int, int>();

            string line;
            var linePattern = new Regex("C (\\S+) ; WX (\\S+) ; N (\\S+)");
            int implicitCharCode = short.MaxValue;
            while ((line = this.FontData.ReadLine()) != null)
            {
                var lineMatches = linePattern.Matches(line);
                if (lineMatches.Count < 1)
                {
                    if (line.Equals("EndCharMetrics"))
                    {
                        break;
                    }

                    continue;
                }

                var lineMatch = lineMatches[0];

                var charCode = ConvertUtils.ParseIntInvariant(lineMatch.Groups[1].Value);
                var width = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                var charName = lineMatch.Groups[3].Value;
                if (charCode < 0)
                {
                    if (charName == null)
                    {
                        continue;
                    }

                    charCode = ++implicitCharCode;
                }
                var code =
                  ((charName == null) || this.Metrics.IsCustomEncoding)
                    ? charCode
                    : GlyphMapping.NameToCode(charName).Value;

                this.GlyphIndexes[code] = charCode;
                this.GlyphWidths[charCode] = width;
            }
        }

        ///
        /// <summary>
        /// Loads the font header [AFM:4.1:3,4.1-4.4].
        /// </summary>
        ///
        private void LoadFontHeader()
        {
            string line;
            var linePattern = new Regex("(\\S+)\\s+(.+)");
            while ((line = this.FontData.ReadLine()) != null)
            {
                var lineMatches = linePattern.Matches(line);
                if (lineMatches.Count < 1)
                {
                    continue;
                }

                var lineMatch = lineMatches[0];

                var key = lineMatch.Groups[1].Value;
                if (key.Equals("Ascender"))
                {
                    this.Metrics.Ascender = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("CapHeight"))
                {
                    this.Metrics.CapHeight = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("Descender"))
                {
                    this.Metrics.Descender = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("EncodingScheme"))
                {
                    this.Metrics.IsCustomEncoding = lineMatch.Groups[2].Value.Equals("FontSpecific");
                }
                else if (key.Equals("FontBBox"))
                {
                    var coordinates = Regex.Split(lineMatch.Groups[2].Value, "\\s+");
                    this.Metrics.XMin = ConvertUtils.ParseAsIntInvariant(coordinates[0]);
                    this.Metrics.YMin = ConvertUtils.ParseAsIntInvariant(coordinates[1]);
                    this.Metrics.XMax = ConvertUtils.ParseAsIntInvariant(coordinates[2]);
                    this.Metrics.YMax = ConvertUtils.ParseAsIntInvariant(coordinates[3]);
                }
                else if (key.Equals("FontName"))
                {
                    this.Metrics.FontName = lineMatch.Groups[2].Value;
                }
                else if (key.Equals("IsFixedPitch"))
                {
                    this.Metrics.IsFixedPitch = bool.Parse(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("ItalicAngle"))
                {
                    this.Metrics.ItalicAngle = ConvertUtils.ParseFloatInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("StdHW"))
                {
                    this.Metrics.StemH = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("StdVW"))
                {
                    this.Metrics.StemV = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("UnderlinePosition"))
                {
                    this.Metrics.UnderlinePosition = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("UnderlineThickness"))
                {
                    this.Metrics.UnderlineThickness = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("Weight"))
                {
                    this.Metrics.Weight = lineMatch.Groups[2].Value;
                }
                else if (key.Equals("XHeight"))
                {
                    this.Metrics.XHeight = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[2].Value);
                }
                else if (key.Equals("StartCharMetrics"))
                {
                    break;
                }
            }
            if (this.Metrics.Ascender == 0)
            {
                this.Metrics.Ascender = this.Metrics.YMax;
            }
            if (this.Metrics.Descender == 0)
            {
                this.Metrics.Descender = this.Metrics.YMin;
            }
        }

        ///
        /// <summary>
        /// Loads kerning data [AFM:4.1:3,4,4.5,9].
        /// </summary>
        ///
        private void LoadKerningData()
        {
            this.GlyphKernings = new Dictionary<int, int>();

            string line;
            while ((line = this.FontData.ReadLine()) != null)
            {
                if (line.StartsWith("StartKernPairs"))
                {
                    break;
                }
            }

            var linePattern = new Regex("KPX (\\S+) (\\S+) (\\S+)");
            while ((line = this.FontData.ReadLine()) != null)
            {
                var lineMatches = linePattern.Matches(line);
                if (lineMatches.Count < 1)
                {
                    if (line.Equals("EndKernPairs"))
                    {
                        break;
                    }

                    continue;
                }

                var lineMatch = lineMatches[0];

                var code1 = GlyphMapping.NameToCode(lineMatch.Groups[1].Value).Value;
                var code2 = GlyphMapping.NameToCode(lineMatch.Groups[2].Value).Value;
                var pair = code1 << (16 + code2);
                var value = ConvertUtils.ParseAsIntInvariant(lineMatch.Groups[3].Value);

                this.GlyphKernings[pair] = value;
            }
        }

        public IInputStream FontData { get; set; }

        public Dictionary<int, int> GlyphIndexes { get; set; }

        public Dictionary<int, int> GlyphKernings { get; set; }

        public Dictionary<int, int> GlyphWidths { get; set; }

        public FontMetrics Metrics { get; set; }

        ///
        /// <summary>
        /// Font header (Global font information).
        /// </summary>
        ///
        public sealed class FontMetrics
        {
            public int Ascender { get; set; }

            public int CapHeight { get; set; }

            public int Descender { get; set; }

            public string FontName { get; set; }

            public bool IsCustomEncoding { get; set; }

            public bool IsFixedPitch { get; set; }

            public float ItalicAngle { get; set; }

            public int StemH { get; set; }

            public int StemV { get; set; }

            public int UnderlinePosition { get; set; }

            public int UnderlineThickness { get; set; }

            public string Weight { get; set; }

            public int XHeight { get; set; }

            public int XMax { get; set; }

            public int XMin { get; set; }

            public int YMax { get; set; }

            public int YMin { get; set; }
        }
    }
}
