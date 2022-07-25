/*
  Copyright 2009-2015 Stefano Chizzolini. http://www.pdfclown.org

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

  redistributions retain the above copyright notice, license and disclaimer, along with
  Redistribution and use, with or without modification, are permitted provided that such
  this list of conditions.
*/

namespace org.pdfclown.documents.contents.objects
{
    using System;

    using System.Collections.Generic;
    using System.IO;
    using org.pdfclown.objects;

    /**
      <summary>'Show one or more text strings, allowing individual glyph positioning'
      operation [PDF:1.6:5.3.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class ShowAdjustedText
      : ShowText
    {
        public static readonly string OperatorKeyword = "TJ";

        internal ShowAdjustedText(
          IList<PdfDirectObject> operands
          ) : base(OperatorKeyword, operands)
        { }

        /**
<param name="value">Each element can be either a byte array (encoded text) or a number.
If the element is a byte array (encoded text), this operator shows the text glyphs.
If it is a number (glyph adjustment), the operator adjusts the next glyph position by that amount.</param>
*/
        public ShowAdjustedText(
          IList<object> value
          ) : base(OperatorKeyword, (PdfDirectObject)new PdfArray())
        { this.Value = value; }

        public override byte[] Text
        {
            get
            {
                var textStream = new MemoryStream();
                foreach (var element in (PdfArray)this.operands[0])
                {
                    if (element is PdfString)
                    {
                        var elementValue = ((PdfString)element).RawValue;
                        textStream.Write(elementValue, 0, elementValue.Length);
                    }
                }
                return textStream.ToArray();
            }
            set => this.Value = new List<object> { value };
        }

        public override IList<object> Value
        {
            get
            {
                var value = new List<object>();
                foreach (var element in (PdfArray)this.operands[0])
                {
                    //TODO:horrible workaround to the lack of generic covariance...
                    if (element is IPdfNumber)
                    {
                        value.Add(
                          ((IPdfNumber)element).RawValue
                          );
                    }
                    else if (element is PdfString)
                    {
                        value.Add(
                          ((PdfString)element).RawValue
                          );
                    }
                    else
                    {
                        throw new NotSupportedException($"Element type {element.GetType().Name} not supported.");
                    }
                }
                return value;
            }
            set
            {
                var elements = (PdfArray)this.operands[0];
                elements.Clear();
                var textItemExpected = true;
                foreach (var valueItem in value)
                {
                    PdfDirectObject element;
                    if (textItemExpected)
                    { element = new PdfByteString((byte[])valueItem); }
                    else
                    { element = PdfInteger.Get(Convert.ToInt32(valueItem)); }
                    elements.Add(element);

                    textItemExpected = !textItemExpected;
                }
            }
        }
    }
}