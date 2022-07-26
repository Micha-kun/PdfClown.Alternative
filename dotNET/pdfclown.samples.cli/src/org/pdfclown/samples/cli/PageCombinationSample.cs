namespace org.pdfclown.samples.cli
{

    using System.Drawing;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.composition;

    using org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to combine multiple pages into single bigger pages (for
      example two A4 modules into one A3 module) using form XObjects [PDF:1.6:4.9].</summary>
      <remarks>Form XObjects are a convenient way to represent contents multiple times on multiple pages
      as templates.</remarks>
    */
    public class PageCombinationSample
      : Sample
    {
        public override void Run(
          )
        {
            // 1. Instantiate the source PDF file!
            var filePath = this.PromptFileChoice("Please select a PDF file to use as source");
            using (var sourceFile = new File(filePath))
            {
                // 2. Instantiate a new PDF file!
                var file = new File();

                // 3. Source page combination into target file.
                var document = file.Document;
                var pages = document.Pages;
                var pageIndex = -1;
                PrimitiveComposer composer = null;
                SizeF targetPageSize = PageFormat.GetSize(PageFormat.SizeEnum.A4);
                foreach (var sourcePage in sourceFile.Document.Pages)
                {
                    pageIndex++;
                    var pageMod = pageIndex % 2;
                    if (pageMod == 0)
                    {
                        if (composer != null)
                        { composer.Flush(); }

                        // Add a page to the target document!
                        var page = new Page(
                          document,
                          PageFormat.GetSize(PageFormat.SizeEnum.A3, PageFormat.OrientationEnum.Landscape)
                          ); // Instantiates the page inside the document context.
                        pages.Add(page); // Puts the page in the pages collection.
                                         // Create a composer for the target content stream!
                        composer = new PrimitiveComposer(page);
                    }

                    // Add the form to the target page!
                    composer.ShowXObject(
                      sourcePage.ToXObject(document), // Converts the source page into a form inside the target document.
                      new PointF(targetPageSize.Width * pageMod, 0),
                      targetPageSize,
                      XAlignmentEnum.Left,
                      YAlignmentEnum.Top,
                      0
                      );
                }
                composer.Flush();

                // 4. Serialize the PDF file!
                _ = this.Serialize(file, "Page combination", "combining multiple pages into single bigger ones", "page combination");
            }
        }
    }
}