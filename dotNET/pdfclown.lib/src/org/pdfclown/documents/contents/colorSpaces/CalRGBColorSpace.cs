//-----------------------------------------------------------------------
// <copyright file="CalRGBColorSpace.cs" company="">
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
namespace org.pdfclown.documents.contents.colorSpaces
{
    using System;

    using System.Collections.Generic;
    using org.pdfclown.objects;
    using drawing = System.Drawing;

    ///
    /// <summary>
    /// CIE-based ABC single-transformation-stage color space, where A, B, and C represent calibrated red, green and
    /// blue color values [PDF:1.6:4.5.4].
    /// </summary>
    ///
    [PDF(VersionEnum.PDF11)]
    public sealed class CalRGBColorSpace : CalColorSpace
    {
        //TODO:IMPL new element constructor!

        internal CalRGBColorSpace(PdfDirectObject baseObject) : base(baseObject)
        {
        }

        public override object Clone(Document context)
        { throw new NotImplementedException(); }

        public override Color GetColor(IList<PdfDirectObject> components, IContentContext context)
        { return new CalRGBColor(components); }

        public override drawing::Brush GetPaint(Color color)
        {
            // FIXME: temporary hack
            return new drawing::SolidBrush(drawing::Color.Black);
        }

        public override int ComponentCount => 3;

        public override Color DefaultColor => CalRGBColor.Default;

        public override double[] Gamma
        {
            get
            {
                var gammaObject = (PdfArray)this.Dictionary[PdfName.Gamma];
                return (gammaObject == null)
                    ? (new double[] { 1, 1, 1 })
                    : (new double[]
                    {
                        ((IPdfNumber)gammaObject[0]).RawValue,
                        ((IPdfNumber)gammaObject[1]).RawValue,
                        ((IPdfNumber)gammaObject[2]).RawValue
                    });
            }
        }
    }
}
