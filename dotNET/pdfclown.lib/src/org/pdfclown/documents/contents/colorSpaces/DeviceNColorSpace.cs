//-----------------------------------------------------------------------
// <copyright file="DeviceNColorSpace.cs" company="">
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

    ///
    /// <summary>
    /// Special color space that can contain an arbitrary number of color components [PDF:1.6:4.5.5].
    /// </summary>
    ///
    [PDF(VersionEnum.PDF13)]
    public sealed class DeviceNColorSpace : SpecialDeviceColorSpace
    {
        //TODO:IMPL new element constructor!

        internal DeviceNColorSpace(PdfDirectObject baseObject) : base(baseObject)
        {
        }

        public override object Clone(Document context)
        { throw new NotImplementedException(); }

        public override Color GetColor(IList<PdfDirectObject> components, IContentContext context)
        { return new DeviceNColor(components); }

        public override int ComponentCount => ((PdfArray)((PdfArray)this.BaseDataObject)[1]).Count;

        public override IList<string> ComponentNames
        {
            get
            {
                IList<string> componentNames = new List<string>();
                var namesObject = (PdfArray)((PdfArray)this.BaseDataObject)[1];
                foreach (var nameObject in namesObject)
                {
                    componentNames.Add((string)((PdfName)nameObject).Value);
                }
                return componentNames;
            }
        }

        public override Color DefaultColor
        {
            get
            {
                var components = new double[this.ComponentCount];
                for (int index = 0,
                    length = components.Length; index < length; index++)
                {
                    components[index] = 1;
                }

                return new DeviceNColor(components);
            }
        }
    }
}
