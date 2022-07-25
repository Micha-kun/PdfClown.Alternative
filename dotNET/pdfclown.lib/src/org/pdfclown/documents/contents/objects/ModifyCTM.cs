/*
  Copyright 2007-2012 Stefano Chizzolini. http://www.pdfclown.org

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
      <summary>'Modify the current transformation matrix (CTM) by concatenating the specified matrix'
      operation [PDF:1.6:4.3.3].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class ModifyCTM
      : Operation
    {
        public static readonly string OperatorKeyword = "cm";

        public ModifyCTM(
Matrix value
) : this(
value.Elements[0],
value.Elements[1],
value.Elements[2],
value.Elements[3],
value.Elements[4],
value.Elements[5]
)
        { }

        public ModifyCTM(
          IList<PdfDirectObject> operands
          ) : base(OperatorKeyword, operands)
        { }

        public ModifyCTM(
          double a,
          double b,
          double c,
          double d,
          double e,
          double f
          ) : base(
            OperatorKeyword,
            new List<PdfDirectObject>(
              new PdfDirectObject[]
              {
            PdfReal.Get(a),
            PdfReal.Get(b),
            PdfReal.Get(c),
            PdfReal.Get(d),
            PdfReal.Get(e),
            PdfReal.Get(f)
              }
              )
            )
        { }

        public static ModifyCTM GetResetCTM(
ContentScanner.GraphicsState state
)
        {
            var inverseCtm = state.Ctm.Clone();
            inverseCtm.Invert();
            return new ModifyCTM(
              inverseCtm
              // TODO: inverseCtm is a simplification which assumes an identity initial ctm!
              //        SquareMatrix.get(state.Ctm).solve(
              //          SquareMatrix.get(state.GetInitialCtm())
              //          ).toTransform()
              );
        }

        public override void Scan(
ContentScanner.GraphicsState state
)
        {
            state.Ctm.Multiply(this.Value);

            var context = state.Scanner.RenderContext;
            if (context != null)
            { context.Transform = state.Ctm; }
        }

        public Matrix Value => new Matrix(
                  ((IPdfNumber)this.operands[0]).FloatValue, // a.
                  ((IPdfNumber)this.operands[1]).FloatValue, // b.
                  ((IPdfNumber)this.operands[2]).FloatValue, // c.
                  ((IPdfNumber)this.operands[3]).FloatValue, // d.
                  ((IPdfNumber)this.operands[4]).FloatValue, // e.
                  ((IPdfNumber)this.operands[5]).FloatValue // f.
                  );
    }
}