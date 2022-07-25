namespace org.pdfclown.samples.cli
{

    using System;
    using System.Drawing;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.composition;

    using org.pdfclown.documents.contents.fonts;
    using org.pdfclown.files;

    /**
      <summary>This sample generates a series of PDF pages from the default page formats available,
      varying both in size and orientation.</summary>
    */
    public class PageFormatSample
      : Sample
    {

        private void Populate(
          Document document
          )
        {
            var bodyFont = new StandardType1Font(
              document,
              StandardType1Font.FamilyEnum.Courier,
              true,
              false
              );

            var pages = document.Pages;
            var pageFormats = (PageFormat.SizeEnum[])Enum.GetValues(typeof(PageFormat.SizeEnum));
            var pageOrientations = (PageFormat.OrientationEnum[])Enum.GetValues(typeof(PageFormat.OrientationEnum));
            foreach (var pageFormat in pageFormats)
            {
                foreach (var pageOrientation in pageOrientations)
                {
                    // Add a page to the document!
                    var page = new Page(
                      document,
                      PageFormat.GetSize(
                        pageFormat,
                        pageOrientation
                        )
                      ); // Instantiates the page inside the document context.
                    pages.Add(page); // Puts the page in the pages collection.

                    // Drawing the text label on the page...
                    var pageSize = page.Size;
                    var composer = new PrimitiveComposer(page);
                    composer.SetFont(bodyFont, 32);
                    _ = composer.ShowText(
                      $"{pageFormat} ({pageOrientation})", // Text.
                      new PointF(
                        pageSize.Width / 2,
                        pageSize.Height / 2
                        ), // Location: page center.
                      XAlignmentEnum.Center, // Places the text on horizontal center of the location.
                      YAlignmentEnum.Middle, // Places the text on vertical middle of the location.
                      45 // Rotates the text 45 degrees counterclockwise.
                      );
                    composer.Flush();
                }
            }
        }

        public override void Run(
          )
        {
            // 1. PDF file instantiation.
            var file = new File();
            var document = file.Document;

            // 2. Populate the document!
            this.Populate(document);

            // 3. Serialize the PDF file!
            _ = this.Serialize(file, "Page Format", "page formats", "page formats");
        }
    }
}