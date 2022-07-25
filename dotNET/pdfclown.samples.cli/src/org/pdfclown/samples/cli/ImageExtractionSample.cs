namespace org.pdfclown.samples.cli
{

    using System;
    using System.IO;
    using org.pdfclown.bytes;

    using org.pdfclown.objects;
    using files = org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to extract XObject images from a PDF document.</summary>
      <remarks>
        <para>Inline images are ignored.</para>
        <para>XObject images other than JPEG aren't currently supported for handling.</para>
      </remarks>
    */
    public class ImageExtractionSample
      : Sample
    {

        private void ExportImage(
          IBuffer data,
          string filename
          )
        {
            var outputPath = this.GetOutputPath(filename);
            FileStream outputStream;
            try
            { outputStream = new FileStream(outputPath, FileMode.CreateNew); }
            catch (Exception e)
            { throw new Exception($"{outputPath} file couldn't be created.", e); }

            try
            {
                var writer = new BinaryWriter(outputStream);
                writer.Write(data.ToByteArray());
                writer.Close();
                outputStream.Close();
            }
            catch (Exception e)
            { throw new Exception($"{outputPath} file writing has failed.", e); }

            Console.WriteLine($"Output: {outputPath}");
        }

        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new files::File(filePath))
            {
                // 2. Iterating through the indirect object collection...
                var index = 0;
                foreach (var indirectObject in file.IndirectObjects)
                {
                    // Get the data object associated to the indirect object!
                    var dataObject = indirectObject.DataObject;
                    // Is this data object a stream?
                    if (dataObject is PdfStream)
                    {
                        var header = ((PdfStream)dataObject).Header;
                        // Is this stream an image?
                        if (header.ContainsKey(PdfName.Type)
                          && header[PdfName.Type].Equals(PdfName.XObject)
                          && header[PdfName.Subtype].Equals(PdfName.Image))
                        {
                            // Which kind of image?
                            if (header[PdfName.Filter].Equals(PdfName.DCTDecode)) // JPEG image.
                            {
                                // Get the image data (keeping it encoded)!
                                var body = ((PdfStream)dataObject).GetBody(false);
                                // Export the image!
                                this.ExportImage(
                                  body,
                                  $"ImageExtractionSample_{index++}.jpg"
                                  );
                            }
                            else // Unsupported image.
                            { Console.WriteLine($"Image XObject {indirectObject.Reference} couldn't be extracted (filter: {header[PdfName.Filter]})"); }
                        }
                    }
                }
            }
        }
    }
}