namespace org.pdfclown.samples.cli
{

    using System.Drawing;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.fonts;

    using org.pdfclown.files;

    /**
      <summary>This sample is a minimalist introduction to the use of PDF Clown.</summary>
    */
    public class HelloWorldSample
      : Sample
    {

        /**
          <summary>Populates a PDF file with contents.</summary>
        */
        private void Populate(
          Document document
          )
        {
            // 1. Add the page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.
            var pageSize = page.Size;

            // 2. Create a content composer for the page!
            var composer = new PrimitiveComposer(page);

            // 3. Inserting contents...
            // Set the font to use!
            composer.SetFont(new StandardType1Font(document, StandardType1Font.FamilyEnum.Courier, true, false), 30);
            // Show the text onto the page (along with its box)!
            /*
              NOTE: PrimitiveComposer's ShowText() method is the most basic way to add text to a page
              -- see BlockComposer for more advanced uses (horizontal and vertical alignment, hyphenation,
              etc.).
            */
            _ = composer.ShowText(
              "Hello World!",
              new PointF(32, 48)
              );

            composer.SetLineWidth(.25);
            composer.SetLineCap(LineCapEnum.Round);
            composer.SetLineDash(new LineDash(new double[] { 5, 10 }));
            composer.SetTextLead(1.2);
            composer.DrawPolygon(
              composer.ShowText(
                "This is a primitive example"
                  + "\nof centered, rotated multi-"
                  + "\nline text."
                  + "\n\nWe recommend you to use"
                  + "\nBlockComposer instead, as it"
                  + "\nautomatically manages text"
                  + "\nwrapping and alignment with-"
                  + "\nin a specified area!",
                new PointF(pageSize.Width / 2, pageSize.Height / 2),
                XAlignmentEnum.Center,
                YAlignmentEnum.Middle,
                15
                ).Points
              );
            composer.Stroke();

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        public override void Run(
          )
        {
            // 1. Instantiate a new PDF file!
            /* NOTE: a File object is the low-level (syntactic) representation of a PDF file. */
            var file = new File();

            // 2. Get its corresponding document!
            /* NOTE: a Document object is the high-level (semantic) representation of a PDF file. */
            var document = file.Document;

            // 3. Insert the contents into the document!
            this.Populate(document);

            // 4. Serialize the PDF file!
            _ = this.Serialize(file, "Hello world", "a simple 'hello world'", "Hello world");
        }
    }
}