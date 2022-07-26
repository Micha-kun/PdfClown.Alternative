/*
  Copyright 2010 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.Collections.Generic;
    using System.Drawing;

    using System.Drawing.Imaging;
    using System.Drawing.Printing;
    using System.Windows.Forms;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents;

    /**
      <summary>Tool for rendering <see cref="IContentContext">content contexts</see>.</summary>
    */
    public sealed class Renderer
    {

        /**
<summary>Wraps the specified document into a printable object.</summary>
<param name="document">Document to wrap for printing.</param>
<returns>Printable object.</returns>
*/
        public static PrintDocument GetPrintDocument(
          Document document
          )
        { return new PrintDocument(document.Pages); }

        /**
          <summary>Wraps the specified page collection into a printable object.</summary>
          <param name="pages">Page collection to print.</param>
          <returns>Printable object.</returns>
        */
        public static PrintDocument GetPrintDocument(
          IList<Page> pages
          )
        { return new PrintDocument(pages); }

        /**
<summary>Prints silently the specified document.</summary>
<param name="document">Document to print.</param>
<returns>Whether the print was fulfilled.</returns>
*/
        public bool Print(
          Document document
          )
        { return this.Print(document.Pages); }

        /**
          <summary>Prints silently the specified page collection.</summary>
          <param name="pages">Page collection to print.</param>
          <returns>Whether the print was fulfilled.</returns>
        */
        public bool Print(
          IList<Page> pages
          )
        { return this.Print(pages, true); }

        /**
          <summary>Prints the specified document.</summary>
          <param name="document">Document to print.</param>
          <param name="silent">Whether to avoid showing a print dialog.</param>
          <returns>Whether the print was fulfilled.</returns>
        */
        public bool Print(
          Document document,
          bool silent
          )
        { return this.Print(document.Pages, silent); }

        /**
          <summary>Prints the specified page collection.</summary>
          <param name="pages">Page collection to print.</param>
          <param name="silent">Whether to avoid showing a print dialog.</param>
          <returns>Whether the print was fulfilled.</returns>
        */
        public bool Print(
          IList<Page> pages,
          bool silent
          )
        {
            var printDocument = GetPrintDocument(pages);
            if (!silent)
            {
                var printDialog = new PrintDialog();
                printDialog.Document = printDocument;
                if (printDialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }
            }

            printDocument.Print();
            return true;
        }

        /**
          <summary>Renders the specified contents into an image context.</summary>
          <param name="contents">Source contents.</param>
          <param name="size">Image size expressed in device-space units (that is typically pixels).</param>
          <returns>Image representing the rendered contents.</returns>
         */
        public Image Render(
          Contents contents,
          SizeF size
          )
        { return this.Render(contents, size, null); }

        /**
          <summary>Renders the specified content context into an image context.</summary>
          <param name="contentContext">Source content context.</param>
          <param name="size">Image size expressed in device-space units (that is typically pixels).</param>
          <returns>Image representing the rendered contents.</returns>
         */
        public Image Render(
          IContentContext contentContext,
          SizeF size
          )
        { return this.Render(contentContext, size, null); }

        /**
          <summary>Renders the specified contents into an image context.</summary>
          <param name="contents">Source contents.</param>
          <param name="size">Image size expressed in device-space units (that is typically pixels).</param>
          <param name="area">Content area to render; <code>null</code> corresponds to the entire
           <see cref="IContentContext.Box">content bounding box</see>.</param>
          <returns>Image representing the rendered contents.</returns>
         */
        public Image Render(
          Contents contents,
          SizeF size,
          RectangleF? area
          )
        { return this.Render(contents.ContentContext, size, area); }

        /**
          <summary>Renders the specified content context into an image context.</summary>
          <param name="contentContext">Source content context.</param>
          <param name="size">Image size expressed in device-space units (that is typically pixels).</param>
          <param name="area">Content area to render; <code>null</code> corresponds to the entire
           <see cref="IContentContext.Box">content bounding box</see>.</param>
          <returns>Image representing the rendered contents.</returns>
         */
        public Image Render(
          IContentContext contentContext,
          SizeF size,
          RectangleF? area
          )
        {
            //TODO:area!
            Image image = new Bitmap(
              (int)size.Width,
              (int)size.Height,
              PixelFormat.Format24bppRgb
              );
            contentContext.Render(Graphics.FromImage(image), size);
            return image;
        }
        /**
  <summary>Printable document.</summary>
  <remarks>It wraps a page collection for printing purposes.</remarks>
*/
        public sealed class PrintDocument
          : System.Drawing.Printing.PrintDocument
        {

            private int pageIndex;
            private IList<Page> pages;
            private int pagesCount;

            public PrintDocument(
              IList<Page> pages
              )
            { this.Pages = pages; }

            protected override void OnBeginPrint(
              PrintEventArgs e
              )
            {
                this.pageIndex = -1;

                base.OnBeginPrint(e);
            }

            protected override void OnPrintPage(
              PrintPageEventArgs e
              )
            {
                var printerSettings = e.PageSettings.PrinterSettings;
                switch (printerSettings.PrintRange)
                {
                    case PrintRange.SomePages:
                        if (this.pageIndex < printerSettings.FromPage)
                        { this.pageIndex = printerSettings.FromPage; }
                        else
                        { this.pageIndex++; }

                        e.HasMorePages = this.pageIndex < printerSettings.ToPage;
                        break;
                    default:
                        this.pageIndex++;

                        e.HasMorePages = this.pageIndex + 1 < this.pagesCount;
                        break;
                }

                var page = this.pages[this.pageIndex];
                page.Render(e.Graphics, e.PageBounds.Size);

                base.OnPrintPage(e);
            }

            public IList<Page> Pages
            {
                get => this.pages;
                set
                {
                    this.pages = value;
                    this.pagesCount = this.pages.Count;
                }
            }
        }
    }
}
