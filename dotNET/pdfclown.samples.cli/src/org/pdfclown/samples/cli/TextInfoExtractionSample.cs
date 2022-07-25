namespace org.pdfclown.samples.cli
{

    using System;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.objects;

    using org.pdfclown.files;
    using org.pdfclown.tools;

    /**
      <summary>This sample demonstrates how to retrieve text content along with its graphic attributes
      (font, font size, text color, text rendering mode, text bounding box...) from a PDF document;
      it also generates a document version decorated by text bounding boxes.</summary>
    */
    public class TextInfoExtractionSample
      : Sample
    {
        private readonly DeviceRGBColor[] textCharBoxColors = new DeviceRGBColor[]
          {
        new DeviceRGBColor(200 / 255d, 100 / 255d, 100 / 255d),
        new DeviceRGBColor(100 / 255d, 200 / 255d, 100 / 255d),
        new DeviceRGBColor(100 / 255d, 100 / 255d, 200 / 255d)
          };
        private readonly DeviceRGBColor textStringBoxColor = DeviceRGBColor.Black;

        /**
          <summary>Scans a content level looking for text.</summary>
        */
        /*
          NOTE: Page contents are represented by a sequence of content objects,
          possibly nested into multiple levels.
        */
        private void Extract(
          ContentScanner level,
          PrimitiveComposer composer
          )
        {
            if (level == null)
            {
                return;
            }

            while (level.MoveNext())
            {
                var content = level.Current;
                if (content is Text)
                {
                    var text = (ContentScanner.TextWrapper)level.CurrentWrapper;
                    var colorIndex = 0;
                    foreach (var textString in text.TextStrings)
                    {
                        var textStringBox = textString.Box.Value;
                        Console.WriteLine(
                          $"Text [x:{Math.Round(textStringBox.X)},y:{Math.Round(textStringBox.Y)},w:{Math.Round(textStringBox.Width)},h:{Math.Round(textStringBox.Height)}] [font size:{Math.Round(textString.Style.FontSize)}]: {textString.Text}"
                            );

                        // Drawing text character bounding boxes...
                        colorIndex = (colorIndex + 1) % this.textCharBoxColors.Length;
                        composer.SetStrokeColor(this.textCharBoxColors[colorIndex]);
                        foreach (var textChar in textString.TextChars)
                        {
                            /*
                              NOTE: You can get further text information
                              (font, font size, text color, text rendering mode)
                              through textChar.style.
                            */
                            composer.DrawRectangle(textChar.Box);
                            composer.Stroke();
                        }

                        // Drawing text string bounding box...
                        _ = composer.BeginLocalState();
                        composer.SetLineDash(new LineDash(new double[] { 5 }));
                        composer.SetStrokeColor(this.textStringBoxColor);
                        composer.DrawRectangle(textString.Box.Value);
                        composer.Stroke();
                        composer.End();
                    }
                }
                else if (content is XObject)
                {
                    // Scan the external level!
                    this.Extract(
                      ((XObject)content).GetScanner(level),
                      composer
                      );
                }
                else if (content is ContainerObject)
                {
                    // Scan the inner level!
                    this.Extract(
                      level.ChildLevel,
                      composer
                      );
                }
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

                var stamper = new PageStamper(); // NOTE: Page stamper is used to draw contents on existing pages.

                // 2. Iterating through the document pages...
                foreach (var page in document.Pages)
                {
                    Console.WriteLine($"\nScanning page {page.Number}...\n");

                    stamper.Page = page;

                    this.Extract(
                      new ContentScanner(page), // Wraps the page contents into a scanner.
                      stamper.Foreground
                      );

                    stamper.Flush();
                }

                // 3. Decorated version serialization.
                _ = this.Serialize(file);
            }
        }
    }
}