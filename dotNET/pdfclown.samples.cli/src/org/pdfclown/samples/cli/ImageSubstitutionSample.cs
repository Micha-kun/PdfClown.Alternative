namespace org.pdfclown.samples.cli
{

    using System;
    using System.IO;
    using System.Linq;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.entities;

    using org.pdfclown.documents.contents.xObjects;
    using files = org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to replace images appearing in a PDF document's pages
      through their resource names.</summary>
    */
    public class ImageSubstitutionSample
      : Sample
    {

        private void ReplaceImages(
          Document document
          )
        {
            // Get the image used to replace existing ones!
            var image = Image.Get(this.GetResourcePath($"images{Path.DirectorySeparatorChar}gnu.jpg")); // Image is an abstract entity, as it still has to be included into the pdf document.
                                                                                                        // Add the image to the document!
            var imageXObject = image.ToXObject(document); // XObject (i.e. external object) is, in PDF spec jargon, a reusable object.
                                                          // Looking for images to replace...
            foreach (var page in document.Pages)
            {
                var resources = page.Resources;
                var xObjects = resources.XObjects;
                if (xObjects == null)
                {
                    continue;
                }

                foreach (var xObjectKey in xObjects.Keys.ToList())
                {
                    var xObject = xObjects[xObjectKey];
                    // Is the page's resource an image?
                    if (xObject is ImageXObject)
                    {
                        Console.WriteLine($"Substituting {xObjectKey} image xobject.");
                        xObjects[xObjectKey] = imageXObject;
                    }
                }
            }
        }

        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new files::File(filePath))
            {
                var document = file.Document;

                // 2. Replace the images!
                this.ReplaceImages(document);

                // 3. Serialize the PDF file!
                _ = this.Serialize(file, "Image substitution", "substituting a document's images", "image replacement");
            }
        }
    }
}