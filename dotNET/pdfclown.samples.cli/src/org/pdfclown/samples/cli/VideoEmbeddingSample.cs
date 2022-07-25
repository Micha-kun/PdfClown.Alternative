namespace org.pdfclown.samples.cli
{
    using System.Drawing;
    using org.pdfclown.documents;

    using org.pdfclown.documents.interaction.annotations;
    using org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to insert screen annotations to display media clips inside
      a PDF document.</summary>
    */
    public class VideoEmbeddingSample
      : Sample
    {
        public override void Run(
          )
        {
            // 1. Instantiate the PDF file!
            var file = new File();
            var document = file.Document;

            // 2. Insert a new page!
            var page = new Page(document);
            document.Pages.Add(page);

            // 3. Insert a video into the page!
            _ = new Screen(
              page,
              new RectangleF(10, 10, 320, 180),
              "PJ Harvey - Dress (part)",
              this.GetResourcePath($"video{System.IO.Path.DirectorySeparatorChar}pj_clip.mp4"),
              "video/mp4"
              );

            // 4. Serialize the PDF file!
            _ = this.Serialize(file, "Video embedding", "inserting screen annotations to display media clips inside a PDF document", "video embedding");
        }
    }
}