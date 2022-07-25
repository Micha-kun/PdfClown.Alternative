namespace org.pdfclown.samples.cli
{

    using System;
    using System.IO;
    using org.pdfclown.bytes;
    using org.pdfclown.documents.files;

    using org.pdfclown.documents.interaction.annotations;
    using files = org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to extract attachments from a PDF document.</summary>
    */
    public class AttachmentExtractionSample
      : Sample
    {

        private void EvaluateDataFile(
          FileSpecification dataFile
          )
        {
            if (dataFile is FullFileSpecification)
            {
                var embeddedFile = ((FullFileSpecification)dataFile).EmbeddedFile;
                if (embeddedFile != null)
                { this.ExportAttachment(embeddedFile.Data, dataFile.Path); }
            }
        }

        private void ExportAttachment(
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
                var document = file.Document;

                // 2. Extracting attachments...
                // 2.1. Embedded files (document level).
                foreach (var entry in document.Names.EmbeddedFiles)
                { this.EvaluateDataFile(entry.Value); }

                // 2.2. File attachments (page level).
                foreach (var page in document.Pages)
                {
                    foreach (var annotation in page.Annotations)
                    {
                        if (annotation is FileAttachment)
                        { this.EvaluateDataFile(((FileAttachment)annotation).DataFile); }
                    }
                }
            }
        }
    }
}