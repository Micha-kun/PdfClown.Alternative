/*
  Copyright 2011-2015 Stefano Chizzolini. http://www.pdfclown.org

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


namespace org.pdfclown.documents.interchange.metadata
{
    using System.IO;

    using System.Xml;
    using org.pdfclown.objects;

    /**
      <summary>Metadata stream [PDF:1.6:10.2.2].</summary>
    */
    [PDF(VersionEnum.PDF14)]
    public sealed class Metadata
      : PdfObjectWrapper<PdfStream>
    {
        public Metadata(
Document context
) : base(
context,
new PdfStream(
new PdfDictionary(
new PdfName[]
{
              PdfName.Type,
              PdfName.Subtype
},
new PdfDirectObject[]
{
              PdfName.Metadata,
              PdfName.XML
}
))
)
        { }

        public Metadata(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
<summary>Gets/Sets the metadata contents.</summary>
*/
        public XmlDocument Content
        {
            get
            {
                XmlDocument content;
                using (var contentStream = new MemoryStream(this.BaseDataObject.Body.ToByteArray()))
                {
                    if (contentStream.Length > 0)
                    {
                        content = new XmlDocument();
                        content.Load(contentStream);
                    }
                    else
                    { content = null; }
                }
                return content;
            }
            set
            {
                using (var contentStream = new MemoryStream())
                {
                    value.Save(contentStream);

                    var body = this.BaseDataObject.Body;
                    body.Clear();
                    body.Write(contentStream.ToArray());
                }
            }
        }
    }
}