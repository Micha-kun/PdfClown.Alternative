namespace org.pdfclown.samples.cli
{

    using System;
    using System.Drawing;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.objects;

    using org.pdfclown.files;
    using xObjects = org.pdfclown.documents.contents.xObjects;

    /**
      <summary>This sample demonstrates how to retrieve the precise position (page and coordinates)
      of each image within a PDF document, using the page content scanning functionality.</summary>
      <remarks>This sample leverages the ContentScanner class, a powerful device for accessing
      each single content object within a page.</remarks>
    */
    public class ContentScanningSample
      : Sample
    {

        /**
          <summary>Scans a content level looking for images.</summary>
        */
        /*
          NOTE: Page contents are represented by a sequence of content objects,
          possibly nested into multiple levels.
        */
        private void Scan(
          ContentScanner level,
          Page page
          )
        {
            if (level == null)
            {
                return;
            }

            while (level.MoveNext())
            {
                var current = level.Current;
                if (current is ContainerObject)
                {
                    // Scan the inner level!
                    this.Scan(
                      level.ChildLevel,
                      page
                      );
                }
                else
                {
                    var objectWrapper = level.CurrentWrapper;
                    if (objectWrapper == null)
                    {
                        continue;
                    }

                    /*
                      NOTE: Images can be represented on a page either as
                      external objects (XObject) or inline objects.
                    */
                    SizeF? imageSize = null; // Image native size.
                    if (objectWrapper is ContentScanner.XObjectWrapper)
                    {
                        var xObjectWrapper = (ContentScanner.XObjectWrapper)objectWrapper;
                        var xObject = xObjectWrapper.XObject;
                        // Is the external object an image?
                        if (xObject is xObjects::ImageXObject)
                        {
                            Console.Write(
                              $"External Image '{xObjectWrapper.Name}' ({xObject.BaseObject})" // Image key and indirect reference.
                              );
                            imageSize = xObject.Size; // Image native size.
                        }
                    }
                    else if (objectWrapper is ContentScanner.InlineImageWrapper)
                    {
                        Console.Write("Inline Image");
                        var inlineImage = ((ContentScanner.InlineImageWrapper)objectWrapper).InlineImage;
                        imageSize = inlineImage.Size; // Image native size.
                    }

                    if (imageSize.HasValue)
                    {
                        var box = objectWrapper.Box.Value; // Image position (location and size) on the page.
                        Console.WriteLine(
                          $" on page {page.Number} ({page.BaseObject})" // Page index and indirect reference.
                          );
                        Console.WriteLine("  Coordinates:");
                        Console.WriteLine($"     x: {Math.Round(box.X)}");
                        Console.WriteLine($"     y: {Math.Round(box.Y)}");
                        Console.WriteLine($"     width: {Math.Round(box.Width)} (native: {Math.Round(imageSize.Value.Width)})");
                        Console.WriteLine($"     height: {Math.Round(box.Height)} (native: {Math.Round(imageSize.Value.Height)})");
                    }
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

                // 2. Parsing the document...
                Console.WriteLine("\nLooking for images...");
                foreach (var page in document.Pages)
                {
                    this.Scan(
                      new ContentScanner(page), // Wraps the page contents into the scanner.
                      page
                      );
                }
            }
        }
    }
}