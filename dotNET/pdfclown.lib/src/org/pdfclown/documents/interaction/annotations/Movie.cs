/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.annotations
{
    using System;
    using System.Drawing;

    using org.pdfclown.objects;
    using multimedia = org.pdfclown.documents.multimedia;

    /**
      <summary>Movie annotation [PDF:1.6:8.4.5].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class Movie
      : Annotation
    {

        internal Movie(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        public Movie(
Page page,
RectangleF box,
string text,
multimedia::Movie content
) : base(page, PdfName.Movie, box, text)
        { this.Content = content; }

        /**
<summary>Gets/Sets the movie to be played.</summary>
*/
        public multimedia::Movie Content
        {
            get => new multimedia::Movie(this.BaseDataObject[PdfName.Movie]);
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("Movie MUST be defined.");
                }

                this.BaseDataObject[PdfName.Movie] = value.BaseObject;
            }
        }
    }
}