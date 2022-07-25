/*
  Copyright 2007-2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.contents.objects
{

    using System.Collections.Generic;
    using System.Drawing.Drawing2D;
    using org.pdfclown.objects;

    /**
      <summary>'Move to the start of the next line, offset from the start of the current line' operation
      [PDF:1.6:5.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class TranslateTextRelative
      : Operation
    {
        /**
          <summary>Lead parameter setting.</summary>
        */
        public static readonly string LeadOperatorKeyword = "TD";
        /**
<summary>No side effect.</summary>
*/
        public static readonly string SimpleOperatorKeyword = "Td";

        public TranslateTextRelative(
double offsetX,
double offsetY
) : this(offsetX, offsetY, false)
        { }

        public TranslateTextRelative(
          string @operator,
          IList<PdfDirectObject> operands
          ) : base(@operator, operands)
        { }

        public TranslateTextRelative(
          double offsetX,
          double offsetY,
          bool leadSet
          ) : base(
            leadSet ? LeadOperatorKeyword : SimpleOperatorKeyword,
            PdfReal.Get(offsetX),
            PdfReal.Get(offsetY)
            )
        { }

        public override void Scan(
          ContentScanner.GraphicsState state
          )
        {
            state.Tlm.Multiply(new Matrix(1, 0, 0, 1, (float)this.OffsetX, (float)this.OffsetY));
            state.Tm = state.Tlm.Clone();
            if (this.LeadSet)
            { state.Lead = -this.OffsetY; }
        }

        /**
<summary>Gets/Sets whether this operation, as a side effect, sets the leading parameter in the text state.</summary>
*/
        public bool LeadSet
        {
            get => this.@operator.Equals(LeadOperatorKeyword);
            set => this.@operator = value ? LeadOperatorKeyword : SimpleOperatorKeyword;
        }

        public double OffsetX
        {
            get => ((IPdfNumber)this.operands[0]).RawValue;
            set => this.operands[0] = PdfReal.Get(value);
        }

        public double OffsetY
        {
            get => ((IPdfNumber)this.operands[1]).RawValue;
            set => this.operands[1] = PdfReal.Get(value);
        }
    }
}
