namespace org.pdfclown.samples.cli
{
    using System.Drawing;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.entities;
    using org.pdfclown.documents.contents.fonts;

    using org.pdfclown.files;
    using org.pdfclown.util.math.geom;

    /**
      <summary>This sample demonstrates how to show bar codes in a PDF document.</summary>
    */
    public class BarcodeSample
      : Sample
    {
        private const float Margin = 36;

        /**
          <summary>Populates a PDF file with contents.</summary>
        */
        private void Populate(
          Document document
          )
        {
            // Get the abstract barcode entity!
            var barcode = new EAN13Barcode("8012345678901");
            // Create the reusable barcode within the document!
            var barcodeXObject = barcode.ToXObject(document);

            var pages = document.Pages;
            // Page 1.
            {
                var page = new Page(document);
                pages.Add(page);
                var pageSize = page.Size;

                var composer = new PrimitiveComposer(page);
                var blockComposer = new BlockComposer(composer);
                blockComposer.Hyphenation = true;
                blockComposer.Begin(
                  new RectangleF(
                    Margin,
                    Margin,
                    pageSize.Width - (Margin * 2),
                    pageSize.Height - (Margin * 2)
                    ),
                  XAlignmentEnum.Left,
                  YAlignmentEnum.Top
                  );
                var bodyFont = new StandardType1Font(
                  document,
                  StandardType1Font.FamilyEnum.Courier,
                  true,
                  false
                  );
                composer.SetFont(bodyFont, 32);
                _ = blockComposer.ShowText("Barcode sample");
                _ = blockComposer.ShowBreak();
                composer.SetFont(bodyFont, 16);
                _ = blockComposer.ShowText("Showing the EAN-13 Bar Code on different compositions:");
                _ = blockComposer.ShowBreak();
                _ = blockComposer.ShowText("- page 1: on the lower right corner of the page, 100pt wide;");
                _ = blockComposer.ShowBreak();
                _ = blockComposer.ShowText("- page 2: on the middle of the page, 1/3-page wide, 25 degree counterclockwise rotated;");
                _ = blockComposer.ShowBreak();
                _ = blockComposer.ShowText("- page 3: filled page, 90 degree clockwise rotated.");
                _ = blockComposer.ShowBreak();
                blockComposer.End();

                // Show the barcode!
                composer.ShowXObject(
                  barcodeXObject,
                  new PointF(pageSize.Width - Margin, pageSize.Height - Margin),
                  GeomUtils.Scale(barcodeXObject.Size, new SizeF(100, 0)),
                  XAlignmentEnum.Right,
                  YAlignmentEnum.Bottom,
                  0
                  );
                composer.Flush();
            }

            // Page 2.
            {
                var page = new Page(document);
                pages.Add(page);
                var pageSize = page.Size;

                var composer = new PrimitiveComposer(page);
                // Show the barcode!
                composer.ShowXObject(
                  barcodeXObject,
                  new PointF(pageSize.Width / 2, pageSize.Height / 2),
                  GeomUtils.Scale(barcodeXObject.Size, new SizeF(pageSize.Width / 3, 0)),
                  XAlignmentEnum.Center,
                  YAlignmentEnum.Middle,
                  25
                  );
                composer.Flush();
            }

            // Page 3.
            {
                var page = new Page(document);
                pages.Add(page);
                var pageSize = page.Size;

                var composer = new PrimitiveComposer(page);
                // Show the barcode!
                composer.ShowXObject(
                  barcodeXObject,
                  new PointF(pageSize.Width / 2, pageSize.Height / 2),
                  new SizeF(pageSize.Height, pageSize.Width),
                  XAlignmentEnum.Center,
                  YAlignmentEnum.Middle,
                  -90
                  );
                composer.Flush();
            }
        }

        public override void Run(
          )
        {
            // 1. PDF file instantiation.
            var file = new File();
            var document = file.Document;

            // 2. Content creation.
            this.Populate(document);

            // 3. Serialize the PDF file!
            _ = this.Serialize(file, "Barcode", "showing barcodes", "barcodes, creation, EAN13");
        }
    }
}