/*
  Copyright 2007-2011 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.Drawing;
    using org.pdfclown.bytes;
    using org.pdfclown.objects;

    /**
      <summary>Inline image object [PDF:1.6:4.8.6].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class InlineImage
      : GraphicsObject
    {

        private static readonly string DataOperatorKeyword = "ID";
        public static readonly string BeginOperatorKeyword = BeginInlineImage.OperatorKeyword;
        public static readonly string EndOperatorKeyword = EndInlineImage.OperatorKeyword;

        public InlineImage(
InlineImageHeader header,
InlineImageBody body
)
        {
            this.objects.Add(header);
            this.objects.Add(body);
        }

        public override void WriteTo(
          IOutputStream stream,
          Document context
          )
        {
            stream.Write(BeginOperatorKeyword);
            stream.Write("\n");
            this.Header.WriteTo(stream, context);
            stream.Write(DataOperatorKeyword);
            stream.Write("\n");
            this.Body.WriteTo(stream, context);
            stream.Write("\n");
            stream.Write(EndOperatorKeyword);
        }

        /**
<summary>Gets the image body.</summary>
*/
        public Operation Body => (Operation)this.Objects[1];

        /**
          <summary>Gets the image header.</summary>
        */
        public override Operation Header => (Operation)this.Objects[0];

        /**
          <summary>Gets the image size.</summary>
        */
        public Size Size
        {
            get
            {
                var header = (InlineImageHeader)this.Header;
                return new Size(
                  (int)((IPdfNumber)header[header.ContainsKey(PdfName.W) ? PdfName.W : PdfName.Width]).Value,
                  (int)((IPdfNumber)header[header.ContainsKey(PdfName.H) ? PdfName.H : PdfName.Height]).Value
                  );
            }
        }
    }
}