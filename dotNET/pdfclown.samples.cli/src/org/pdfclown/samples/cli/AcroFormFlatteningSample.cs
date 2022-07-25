namespace org.pdfclown.samples.cli
{
    using org.pdfclown.files;
    using org.pdfclown.tools;

    /**
      <summary>This sample demonstrates how to flatten the AcroForm fields of a PDF document.</summary>
    */
    public class AcroFormFlatteningSample
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

                // 2. Flatten the form!
                var formFlattener = new FormFlattener();
                formFlattener.Flatten(document);

                // 3. Serialize the PDF file!
                _ = this.Serialize(file);
            }
        }
    }
}