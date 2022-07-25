namespace org.pdfclown.samples.cli
{
    using System.Drawing.Imaging;
    using org.pdfclown.files;
    using org.pdfclown.tools;

    /**
      <summary>This sample demonstrates how to render a PDF page as a raster image.<summary>
      <remarks>Note: rendering is currently in pre-alpha stage; therefore this sample is
      nothing but an initial stub (no assumption to work!).</remarks>
    */
    public class RenderingSample
      : Sample
    {
        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new File(filePath))
            {
                var document = file.Document;
                var pages = document.Pages;

                // 2. Page rasterization.
                var pageIndex = this.PromptPageChoice("Select the page to render", pages.Count);
                var page = pages[pageIndex];
                var imageSize = page.Size;
                var renderer = new Renderer();
                var image = renderer.Render(page, imageSize);

                // 3. Save the page image!
                image.Save(this.GetOutputPath("ContentRenderingSample.jpg"), ImageFormat.Jpeg);
            }
        }
    }
}