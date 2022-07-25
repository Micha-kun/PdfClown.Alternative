namespace org.pdfclown.samples.cli
{
    using System.Drawing;
    using System.IO;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.composition;

    using org.pdfclown.documents.contents.fonts;
    using entities = org.pdfclown.documents.contents.entities;
    using files = org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to embed an image object within a PDF content
      stream.</summary>
      <remarks>
        <para>Inline objects should be used sparingly, as they easily clutter content
        streams.</para>
        <para>The alternative (and preferred) way to insert an image object is via external
        object (XObject); its main advantage is to allow content reuse.</para>
      </remarks>
    */
    public class InlineObjectSample
      : Sample
    {
        private const float Margin = 36;

        private void Populate(
          Document document
          )
        {
            var page = new Page(document);
            document.Pages.Add(page);
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
              XAlignmentEnum.Justify,
              YAlignmentEnum.Top
              );
            var bodyFont = new StandardType1Font(
              document,
              StandardType1Font.FamilyEnum.Courier,
              true,
              false
              );
            composer.SetFont(bodyFont, 32);
            _ = blockComposer.ShowText("Inline image sample");
            _ = blockComposer.ShowBreak();
            composer.SetFont(bodyFont, 16);
            _ = blockComposer.ShowText("Showing the GNU logo as an inline image within the page content stream.");
            blockComposer.End();
            // Showing the 'GNU' image...

            // Instantiate a jpeg image object!
            var image = entities::Image.Get(this.GetResourcePath($"images{Path.DirectorySeparatorChar}gnu.jpg")); // Abstract image (entity).
                                                                                                                  // Set the position of the image in the page!
            composer.ApplyMatrix(200, 0, 0, 200, (pageSize.Width - 200) / 2, (pageSize.Height - 200) / 2);
            // Show the image!
            _ = image.ToInlineObject(composer); // Transforms the image entity into an inline image within the page.
            composer.Flush();
        }

        public override void Run(
          )
        {
            // 1. PDF file instantiation.
            var file = new files::File();
            var document = file.Document;

            // 2. Content creation.
            this.Populate(document);

            // 3. Serialize the PDF file!
            _ = this.Serialize(file, "Inline image", "embedding an image within a content stream", "inline image");
        }
    }
}