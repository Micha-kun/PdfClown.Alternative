namespace org.pdfclown.samples.cli
{

    using System;
    using org.pdfclown.files;
    using org.pdfclown.tools;

    /**
      <summary>This sample demonstrates how to retrieve text content along with its graphic attributes
      (font, font size, text color, text rendering mode, text bounding box, and so on) from a PDF document;
      text is automatically sorted and aggregated.</summary>
    */
    public class AdvancedTextExtractionSample
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

                // 2. Text extraction from the document pages.
                var extractor = new TextExtractor();
                foreach (var page in document.Pages)
                {
                    if (!this.PromptNextPage(page, false))
                    {
                        this.Quit();
                        break;
                    }

                    var textStrings = extractor.Extract(page)[TextExtractor.DefaultArea];
                    foreach (var textString in textStrings)
                    {
                        var textStringBox = textString.Box.Value;
                        Console.WriteLine(
                          $"Text [x:{Math.Round(textStringBox.X)},y:{Math.Round(textStringBox.Y)},w:{Math.Round(textStringBox.Width)},h:{Math.Round(textStringBox.Height)}]: {textString.Text}"
                            );
                    }
                }
            }
        }
    }
}