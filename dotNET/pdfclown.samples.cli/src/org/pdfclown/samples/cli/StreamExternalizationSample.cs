namespace org.pdfclown.samples.cli
{

    using System.IO;
    using org.pdfclown.documents.files;
    using org.pdfclown.objects;

    using files = org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to move stream data outside PDF files and keep external
      references to them; it demonstrates also the inverse process (reimporting stream data from
      external files).</summary>
      <remarks>Note that, due to security concerns, external streams are a discouraged feature which
      is often unsupported on third-party viewers and disabled by default on recent Adobe Acrobat
      versions; in the latter case, in order to bypass restrictions and allow access to external
      streams, users have to enable Enhanced Security from the Preferences dialog, specifying
      privileged locations.</remarks>
    */
    public class StreamExternalizationSample
      : Sample
    {
        public override void Run(
          )
        {
            // 1. Externalizing the streams...
            string externalizedFilePath;
            // 1.1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new files::File(filePath))
            {
                var document = file.Document;
                /*
                  NOTE: As we are going to export streams using paths relative to the output path, it's
                  necessary to ensure they are properly resolved (otherwise they will be written relative to
                  the current user directory).
                */
                file.Path = this.OutputPath;

                // 1.2. Iterating through the indirect objects to externalize streams...
                var filenameIndex = 0;
                foreach (var indirectObject in file.IndirectObjects)
                {
                    var dataObject = indirectObject.DataObject;
                    if (dataObject is PdfStream)
                    {
                        var stream = (PdfStream)dataObject;
                        if (stream.DataFile == null) // Internal stream to externalize.
                        {
                            stream.SetDataFile(
                              FileSpecification.Get(
                                document,
                                $"{this.GetType().Name}-external{filenameIndex++}"
                                ),
                              true // Forces the stream data to be transferred to the external location.
                              );
                        }
                    }
                }

                // 1.3. Serialize the PDF file!
                externalizedFilePath = this.Serialize(file, files::SerializationModeEnum.Standard);
            }

            // 2. Reimporting the externalized streams...

            // 2.1. Opening the PDF file...
            using (var file = new files::File(externalizedFilePath))
            {
                // 2.2. Iterating through the indirect objects to internalize streams...
                foreach (var indirectObject in file.IndirectObjects)
                {
                    var dataObject = indirectObject.DataObject;
                    if (dataObject is PdfStream)
                    {
                        var stream = (PdfStream)dataObject;
                        if (stream.DataFile != null) // External stream to internalize.
                        {
                            stream.SetDataFile(
                              null,
                              true // Forces the stream data to be transferred to the internal location.
                              );
                        }
                    }
                }

                // 2.3. Serialize the PDF file!
                var externalizedFileName = Path.GetFileNameWithoutExtension(externalizedFilePath);
                var internalizedFilePath = $"{externalizedFileName}-reimported.pdf";
                _ = this.Serialize(file, internalizedFilePath, files::SerializationModeEnum.Standard);
            }
        }
    }
}