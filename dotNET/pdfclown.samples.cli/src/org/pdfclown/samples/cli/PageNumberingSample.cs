namespace org.pdfclown.samples.cli
{
    using System.Drawing;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.fonts;

    using org.pdfclown.files;
    using org.pdfclown.tools;

    /**
      <summary>This sample demonstrates how to stamp the page number on alternated corners
      of an existing PDF document's pages.</summary>
      <remarks>Stamping is just one of the several ways PDF contents can be manipulated using PDF Clown:
      contents can be inserted as (raw) data chunks, mid-level content objects, external forms, etc.</remarks>
    */
    public class PageNumberingSample
      : Sample
    {

        private void Stamp(
          Document document
          )
        {
            // 1. Instantiate the stamper!
            /* NOTE: The PageStamper is optimized for dealing with pages. */
            var stamper = new PageStamper();

            // 2. Numbering each page...
            var font = new StandardType1Font(
              document,
              StandardType1Font.FamilyEnum.Courier,
              true,
              false
              );
            var redColor = DeviceRGBColor.Get(System.Drawing.Color.Red);
            var margin = 32;
            foreach (var page in document.Pages)
            {
                // 2.1. Associate the page to the stamper!
                stamper.Page = page;

                // 2.2. Stamping the page number on the foreground...

                var foreground = stamper.Foreground;

                foreground.SetFont(font, 16);
                foreground.SetFillColor(redColor);

                var pageSize = page.Size;
                var pageNumber = page.Number;
                var pageIsEven = pageNumber % 2 == 0;
                _ = foreground.ShowText(
                  pageNumber.ToString(),
                  new PointF(
                    pageIsEven
                      ? margin
                      : (pageSize.Width - margin),
                    pageSize.Height - margin
                    ),
                  pageIsEven
                    ? XAlignmentEnum.Left
                    : XAlignmentEnum.Right,
                  YAlignmentEnum.Bottom,
                  0
                  );

                // 2.3. End the stamping!
                stamper.Flush();
            }
        }

        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new File(filePath))
            {
                var document = file.Document;

                // 2. Stamp the document!
                this.Stamp(document);

                // 3. Serialize the PDF file!
                _ = this.Serialize(file, "Page numbering", "numbering a document's pages", "page numbering");
            }
        }
    }
}