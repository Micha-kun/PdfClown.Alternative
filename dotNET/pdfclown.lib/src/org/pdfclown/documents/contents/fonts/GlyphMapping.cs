//-----------------------------------------------------------------------
// <copyright file="GlyphMapping.cs" company="">
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
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    ///
    /// <summary>Adobe standard glyph mapping (unicode-encoding against glyph-naming)
    /// [PDF:1.6:D;AGL:2.0].</summary>
    ///
    internal class GlyphMapping
    {
        private static readonly Dictionary<string, int> codes = new Dictionary<string, int>();

        static GlyphMapping() { Load(); }

        ///
        /// <summary>Loads the glyph list mapping character names to character codes (unicode
        /// encoding).</summary>
        ///
        private static void Load()
        {
            StreamReader glyphListStream = null;
            try
            {
                // Open the glyph list!
                /*
                  NOTE: The Adobe Glyph List [AGL:2.0] represents the reference name-to-unicode map
                  for consumer applications.
                */
                glyphListStream = new StreamReader(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("fonts.AGL20"));

                // Parsing the glyph list...
                string line;
                var linePattern = new Regex("^(\\w+);([A-F0-9]+)$");
                while ((line = glyphListStream.ReadLine()) != null)
                {
                    var lineMatches = linePattern.Matches(line);
                    if (lineMatches.Count < 1)
                    {
                        continue;
                    }

                    var lineMatch = lineMatches[0];

                    var name = lineMatch.Groups[1].Value;
                    var code = int.Parse(lineMatch.Groups[2].Value, NumberStyles.HexNumber);

                    // Associate the character name with its corresponding character code!
                    codes[name] = code;
                }
            }
            finally
            {
                if (glyphListStream != null)
                {
                    glyphListStream.Close();
                }
            }
        }

        public static int? NameToCode(string name)
        {
            int code;
            return codes.TryGetValue(name, out code) ? code : ((int?)null);
        }
    }
}
