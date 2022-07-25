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

    using org.pdfclown.objects;

    /**
      <summary>'Set the line dash pattern' operation [PDF:1.6:4.3.3].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class SetLineDash
      : Operation
    {
        public static readonly string OperatorKeyword = "d";

        public SetLineDash(
LineDash lineDash
) : base(OperatorKeyword, (PdfDirectObject)new PdfArray())
        { this.Value = lineDash; }

        public SetLineDash(
          IList<PdfDirectObject> operands
          ) : base(OperatorKeyword, operands)
        { }

        public override void Scan(
ContentScanner.GraphicsState state
)
        { state.LineDash = this.Value; }

        public LineDash Value
        {
            get => LineDash.Get((PdfArray)this.operands[0], (IPdfNumber)this.operands[1]);
            set
            {
                this.operands.Clear();
                // 1. Dash array.
                var dashArray = value.DashArray;
                var baseDashArray = new PdfArray(dashArray.Length);
                foreach (var dashItem in dashArray)
                { baseDashArray.Add(PdfReal.Get(dashItem)); }
                this.operands.Add(baseDashArray);
                // 2. Dash phase.
                this.operands.Add(PdfReal.Get(value.DashPhase));
            }
        }
    }
}