/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.multimedia
{
    using System;

    using org.pdfclown.objects;

    /**
      <summary>Media clip section [PDF:1.7:9.1.3].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public sealed class MediaClipSection
      : MediaClip
    {

        internal MediaClipSection(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        public MediaClipSection(
Document context
) : base(context, PdfName.MCS)
        { }

        public override PdfObjectWrapper Data
        {
            get => MediaClip.Wrap(this.BaseDataObject[PdfName.D]).Data;
            set => throw new NotImplementedException();
        }
    }
}