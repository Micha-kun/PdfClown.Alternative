//-----------------------------------------------------------------------
// <copyright file="IndexedColorSpace.cs" company="">
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
    using org.pdfclown.util;
    using drawing = System.Drawing;

    ///
    /// <summary>
    /// Indexed color space [PDF:1.6:4.5.5].
    /// </summary>
    ///
    [PDF(VersionEnum.PDF11)]
    public sealed class IndexedColorSpace : SpecialColorSpace
    {
        private readonly IDictionary<int, Color> baseColors = new Dictionary<int, Color>();
        private byte[] baseComponentValues;
        private ColorSpace baseSpace;

        //TODO:IMPL new element constructor!

        internal IndexedColorSpace(PdfDirectObject baseObject) : base(baseObject)
        {
        }

        ///
        /// <summary>
        /// Gets the color table.
        /// </summary>
        ///
        private byte[] BaseComponentValues
        {
            get
            {
                if (this.baseComponentValues == null)
                {
                    this.baseComponentValues = ((IDataWrapper)((PdfArray)this.BaseDataObject).Resolve(3)).ToByteArray();
                }
                return this.baseComponentValues;
            }
        }

        public override object Clone(Document context) { throw new NotImplementedException(); }

        ///
        /// <summary>Gets the color corresponding to the specified table index resolved according to
        /// the <see cref="BaseSpace">base space</see>.<summary>
        ///
        public Color GetBaseColor(IndexedColor color)
        {
            var colorIndex = color.Index;
            var baseColor = this.baseColors[colorIndex];
            if (baseColor == null)
            {
                var baseSpace = this.BaseSpace;
                IList<PdfDirectObject> components = new List<PdfDirectObject>();
                var componentCount = baseSpace.ComponentCount;
                var componentValueIndex = colorIndex * componentCount;
                var baseComponentValues = this.BaseComponentValues;
                for (var componentIndex = 0; componentIndex < componentCount; componentIndex++)
                {
                    components.Add(PdfReal.Get((baseComponentValues[componentValueIndex++] & 0xff) / 255d));
                }
                baseColor = baseSpace.GetColor(components, null);
            }
            return baseColor;
        }

        public override Color GetColor(IList<PdfDirectObject> components, IContentContext context)
        { return new IndexedColor(components); }

        public override drawing::Brush GetPaint(Color color)
        { return this.BaseSpace.GetPaint(this.GetBaseColor((IndexedColor)color)); }

        ///
        /// <summary>
        /// Gets the base color space in which the values in the color table are to be interpreted.
        /// </summary>
        ///
        public ColorSpace BaseSpace
        {
            get
            {
                if (this.baseSpace == null)
                {
                    this.baseSpace = ColorSpace.Wrap(((PdfArray)this.BaseDataObject)[1]);
                }
                return this.baseSpace;
            }
        }

        public override int ComponentCount => 1;

        public override Color DefaultColor => IndexedColor.Default;
    }
}
