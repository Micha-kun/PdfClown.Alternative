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

namespace org.pdfclown.documents.interaction.forms
{
    using org.pdfclown.documents.contents;

    using org.pdfclown.objects;

    /**
      <summary>Interactive form (AcroForm) [PDF:1.6:8.6.1].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class Form
      : PdfObjectWrapper<PdfDictionary>
    {

        private Form(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public Form(
Document context
) : base(
context,
new PdfDictionary(
new PdfName[]
{PdfName.Fields},
new PdfDirectObject[]
{new PdfArray()}
)
)
        { }

        public static Form Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new Form(baseObject) : null; }

        /**
<summary>Gets/Sets the fields collection.</summary>
*/
        public Fields Fields
        {
            get => new Fields(this.BaseDataObject.Get<PdfArray>(PdfName.Fields));
            set => this.BaseDataObject[PdfName.Fields] = value.BaseObject;
        }

        /**
          <summary>Gets/Sets the default resources used by fields.</summary>
        */
        public Resources Resources
        {
            get => Resources.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.DR));
            set => this.BaseDataObject[PdfName.DR] = value.BaseObject;
        }
    }
}