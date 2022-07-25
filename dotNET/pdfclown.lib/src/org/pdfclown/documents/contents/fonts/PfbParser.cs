/*
  Copyright 2009-2011 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.Collections.Generic;

    using System.Text.RegularExpressions;
    using org.pdfclown.bytes;
    using org.pdfclown.util;

    /**
      <summary>Type 1 font parser.</summary>
    */
    internal sealed class PfbParser
    {
        private readonly IInputStream stream;

        internal PfbParser(
  IInputStream stream
  )
        { this.stream = stream; }

        /**
<summary>Parses the character-code-to-unicode mapping [PDF:1.6:5.9.1].</summary>
*/
        public Dictionary<ByteArray, int> Parse(
          )
        {
            var codes = new Dictionary<ByteArray, int>();

            string line;
            var linePattern = new Regex("(\\S+)\\s+(.+)");
            while ((line = this.stream.ReadLine()) != null)
            {
                var lineMatches = linePattern.Matches(line);
                if (lineMatches.Count < 1)
                {
                    continue;
                }

                var lineMatch = lineMatches[0];

                var key = lineMatch.Groups[1].Value;
                if (key.Equals("/Encoding"))
                {
                    // Skip to the encoding array entries!
                    _ = this.stream.ReadLine();
                    string encodingLine;
                    var encodingLinePattern = new Regex("dup (\\S+) (\\S+) put");
                    while ((encodingLine = this.stream.ReadLine()) != null)
                    {
                        var encodingLineMatches = encodingLinePattern.Matches(encodingLine);
                        if (encodingLineMatches.Count < 1)
                        {
                            break;
                        }

                        var encodingLineMatch = encodingLineMatches[0];
                        var inputCode = new byte[] { (byte)int.Parse(encodingLineMatch.Groups[1].Value) };
                        var name = encodingLineMatch.Groups[2].Value.Substring(1);
                        codes[new ByteArray(inputCode)] = GlyphMapping.NameToCode(name).Value;
                    }
                    break;
                }
            }
            return codes;
        }
    }
}
