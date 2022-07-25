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

namespace org.pdfclown.tools
{
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.objects;
    using org.pdfclown.objects;

    /**
      <summary>Tool for content insertion into existing pages.</summary>
    */
    public sealed class PageStamper
    {

        private PrimitiveComposer background;
        private PrimitiveComposer foreground;
        private Page page;

        public PageStamper(
  ) : this(null)
        { }

        public PageStamper(
          Page page
          )
        { this.Page = page; }

        private PrimitiveComposer CreateFilter(
  )
        {
            return new PrimitiveComposer(
              new ContentScanner(
                Contents.Wrap(
                  this.page.File.Register(new PdfStream()),
                  this.page
                  )
                )
              );
        }

        public void Flush(
)
        {
            // Ensuring that there's room for the new content chunks inside the page's content stream...
            /*
              NOTE: This specialized stamper is optimized for content insertion without modifying
              existing content representations, leveraging the peculiar feature of page structures
              to express their content streams as arrays of data streams.
            */
            PdfArray streams;
            var contentsObject = this.page.BaseDataObject[PdfName.Contents];
            var contentsDataObject = PdfObject.Resolve(contentsObject);
            // Single data stream?
            if (contentsDataObject is PdfStream)
            {
                /*
                  NOTE: Content stream MUST be expressed as an array of data streams in order to host
                  background- and foreground-stamped contents.
                */
                streams = new PdfArray();
                streams.Add(contentsObject);
                this.page.BaseDataObject[PdfName.Contents] = streams;
            }
            else
            { streams = (PdfArray)contentsDataObject; }

            // Background.
            // Serialize the content!
            this.background.Flush();
            // Insert the serialized content into the page's content stream!
            streams.Insert(0, this.background.Scanner.Contents.BaseObject);

            // Foreground.
            // Serialize the content!
            this.foreground.Flush();
            // Append the serialized content into the page's content stream!
            streams.Add(this.foreground.Scanner.Contents.BaseObject);
        }

        public PrimitiveComposer Background => this.background;

        public PrimitiveComposer Foreground => this.foreground;

        public Page Page
        {
            get => this.page;
            set
            {
                this.page = value;
                if (this.page == null)
                {
                    this.background = null;
                    this.foreground = null;
                }
                else
                {
                    // Background.
                    this.background = this.CreateFilter();
                    // Open the background local state!
                    _ = this.background.Add(SaveGraphicsState.Value);
                    // Close the background local state!
                    _ = this.background.Add(RestoreGraphicsState.Value);
                    // Open the middleground local state!
                    _ = this.background.Add(SaveGraphicsState.Value);
                    // Move into the background!
                    _ = this.background.Scanner.Move(1);

                    // Foregrond.
                    this.foreground = this.CreateFilter();
                    // Close the middleground local state!
                    _ = this.foreground.Add(RestoreGraphicsState.Value);
                }
            }
        }
    }
}
