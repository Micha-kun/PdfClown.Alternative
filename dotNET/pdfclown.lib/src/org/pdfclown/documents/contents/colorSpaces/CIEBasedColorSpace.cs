//-----------------------------------------------------------------------
// <copyright file="CIEBasedColorSpace.cs" company="">
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
    using org.pdfclown.objects;

    ///
    /// <summary>
    /// Abstract CIE-based color space [PDF:1.6:4.5.4].
    /// </summary>
    ///
    [PDF(VersionEnum.PDF11)]
    public abstract class CIEBasedColorSpace : ColorSpace
    {
        //TODO:IMPL new element constructor!

        protected CIEBasedColorSpace(PdfDirectObject baseObject) : base(baseObject)
        {
        }

        ///
        /// <summary>
        /// Gets this color space's dictionary.
        /// </summary>
        ///
        protected PdfDictionary Dictionary => (PdfDictionary)((PdfArray)this.BaseDataObject)[1];

        ///
        /// <summary>
        /// Gets the tristimulus value, in the CIE 1931 XYZ space, of the diffuse black point.
        /// </summary>
        ///
        public double[] BlackPoint
        {
            get
            {
                var blackPointObject = (PdfArray)this.Dictionary[PdfName.BlackPoint];
                return (blackPointObject == null)
                    ? (new double[] { 0, 0, 0 })
                    : (new double[]
                    {
                        ((IPdfNumber)blackPointObject[0]).RawValue,
                        ((IPdfNumber)blackPointObject[1]).RawValue,
                        ((IPdfNumber)blackPointObject[2]).RawValue
                    });
            }
        }

        ///
        /// <summary>
        /// Gets the tristimulus value, in the CIE 1931 XYZ space, of the diffuse white point.
        /// </summary>
        ///
        public double[] WhitePoint
        {
            get
            {
                var whitePointObject = (PdfArray)this.Dictionary[PdfName.WhitePoint];
                return new double[]
                {
                    ((IPdfNumber)whitePointObject[0]).RawValue,
                    ((IPdfNumber)whitePointObject[1]).RawValue,
                    ((IPdfNumber)whitePointObject[2]).RawValue
                };
            }
        }
    }
}
