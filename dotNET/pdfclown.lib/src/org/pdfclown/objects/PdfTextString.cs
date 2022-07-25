/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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


namespace org.pdfclown.objects
{
    using System;

    using org.pdfclown.tokens;

    /**
      <summary>PDF text string object [PDF:1.6:3.8.1].</summary>
      <remarks>Text strings are meaningful only as part of the document hierarchy; they cannot appear
      within content streams. They represent information that is intended to be human-readable.</remarks>
    */
    public sealed class PdfTextString
      : PdfString
    {
        /*
          NOTE: Text strings are string objects encoded in either PdfDocEncoding (superset of the ISO
          Latin 1 encoding [PDF:1.6:D]) or 16-bit big-endian Unicode character encoding (see [UCS:4]).
        */
        public static new readonly PdfTextString Default = new PdfTextString(string.Empty);

        private bool unicoded;

        public PdfTextString(
  byte[] rawValue
  ) : base(rawValue)
        { }

        public PdfTextString(
          string value
          ) : base(value)
        { }

        public PdfTextString(
          byte[] rawValue,
          SerializationModeEnum serializationMode
          ) : base(rawValue, serializationMode)
        { }

        public PdfTextString(
          string value,
          SerializationModeEnum serializationMode
          ) : base(value, serializationMode)
        { }

        public override PdfObject Accept(
IVisitor visitor,
object data
)
        { return visitor.Visit(this, data); }

        /**
<summary>Gets the object equivalent to the given value.</summary>
*/
        public static PdfTextString Get(
          string value
          )
        { return (value != null) ? new PdfTextString(value) : null; }

        public override byte[] RawValue
        {
            protected set
            {
                this.unicoded = (value.Length >= 2) && (value[0] == 254) && (value[1] == 255);
                base.RawValue = value;
            }
        }

        public override object Value
        {
            get
            {
                if ((this.SerializationMode == SerializationModeEnum.Literal) && this.unicoded)
                {
                    var valueBytes = this.RawValue;
                    return Charset.UTF16BE.GetString(valueBytes, 2, valueBytes.Length - 2);
                }
                else
                {
                    // FIXME: proper call to base.StringValue could NOT be done due to an unexpected Mono runtime SIGSEGV (TOO BAD).
                    //          return base.StringValue;
                    return (string)base.Value;
                }
            }
            protected set
            {
                switch (this.SerializationMode)
                {
                    case SerializationModeEnum.Literal:
                        var literalValue = (string)value;
                        var valueBytes = PdfDocEncoding.Get().Encode(literalValue);
                        if (valueBytes == null)
                        {
                            var valueBaseBytes = Charset.UTF16BE.GetBytes(literalValue);
                            // Prepending UTF marker...
                            valueBytes = new byte[valueBaseBytes.Length + 2];
                            valueBytes[0] = 254;
                            valueBytes[1] = 255;
                            Array.Copy(valueBaseBytes, 0, valueBytes, 2, valueBaseBytes.Length);
                        }
                        this.RawValue = valueBytes;
                        break;
                    case SerializationModeEnum.Hex:
                        base.Value = value;
                        break;
                }
            }
        }
    }
}