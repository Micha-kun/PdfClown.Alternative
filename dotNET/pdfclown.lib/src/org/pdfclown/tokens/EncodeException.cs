/*
  Copyright 2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.tokens
{
    using System;

    /**
      <summary>Exception thrown in case of missing code-to-character mapping.</summary>
    */
    public class EncodeException
      : Exception
    {
        private readonly int index;
        private readonly string text;

        public EncodeException(
  char textChar
) : this(new string(textChar, 1), 0)
        { }

        public EncodeException(
          string text,
          int index
          ) : base($"Missing code mapping for character {(int)text[index]} ('{text[index]}') at position {index} in \"{text}\"")
        {
            this.text = text;
            this.index = index;
        }

        /**
<summary>Gets the position of the missing character in the string to encode.</summary>
*/
        public int Index => this.index;

        /**
          <summary>Gets the string to encode.</summary>
        */
        public string Text => this.text;

        /**
          <summary>Gets the missing character.</summary>
        */
        public char UndefinedChar => this.text[this.index];
    }
}