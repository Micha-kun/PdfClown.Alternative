/*
  Copyright 2006-2012 Stefano Chizzolini. http://www.pdfclown.org

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
    using org.pdfclown.bytes;
    using org.pdfclown.files;

    /**
      <summary>PDF integer number object [PDF:1.6:3.2.2].</summary>
    */
    public sealed class PdfInteger
      : PdfSimpleObject<int>,
        IPdfNumber
    {
        public static readonly PdfInteger Default = new PdfInteger(0);

        public PdfInteger(
int value
)
        { this.RawValue = value; }

        double IPdfSimpleObject<double>.RawValue => this.RawValue;

        public override PdfObject Accept(
IVisitor visitor,
object data
)
        { return visitor.Visit(this, data); }

        public override int CompareTo(
          PdfDirectObject obj
          )
        { return PdfNumber.Compare(this, obj); }

        public override bool Equals(
          object obj
          )
        { return PdfNumber.Equal(this, obj); }

        /**
<summary>Gets the object equivalent to the given value.</summary>
*/
        public static PdfInteger Get(
          int? value
          )
        { return value.HasValue ? new PdfInteger(value.Value) : null; }

        public override int GetHashCode(
          )
        { return PdfNumber.GetHashCode(this); }

        public override void WriteTo(
          IOutputStream stream,
          File context
          )
        { stream.Write(this.RawValue.ToString()); }

        public double DoubleValue => this.RawValue;

        public float FloatValue => this.RawValue;

        public int IntValue => this.RawValue;
    }
}