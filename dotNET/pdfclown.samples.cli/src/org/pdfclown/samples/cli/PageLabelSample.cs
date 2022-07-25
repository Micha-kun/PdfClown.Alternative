namespace org.pdfclown.samples.cli
{

    using System;
    using org.pdfclown.documents.interaction.navigation.page;

    using org.pdfclown.files;
    using org.pdfclown.objects;

    /**
      <summary>This sample demonstrates how to define, read and modify page labels.</summary>
    */
    public class PageLabelSample
      : Sample
    {
        public override void Run(
          )
        {
            string outputFilePath;
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new File(filePath))
            {
                var document = file.Document;

                // 2. Defining the page labels...
                var pageLabels = document.PageLabels;
                pageLabels.Clear();
                /*
                  NOTE: This sample applies labels to arbitrary page ranges: no sensible connection with their
                  actual content has therefore to be expected.
                */
                var pageCount = document.Pages.Count;
                pageLabels[new PdfInteger(0)] = new PageLabel(document, "Introduction ", PageLabel.NumberStyleEnum.UCaseRomanNumber, 5);
                if (pageCount > 3)
                { pageLabels[new PdfInteger(3)] = new PageLabel(document, PageLabel.NumberStyleEnum.UCaseLetter); }
                if (pageCount > 6)
                { pageLabels[new PdfInteger(6)] = new PageLabel(document, "Contents ", PageLabel.NumberStyleEnum.ArabicNumber, 0); }

                // 3. Serialize the PDF file!
                outputFilePath = this.Serialize(file, "Page labelling", "labelling a document's pages", "page labels");
            }
            using (var file = new File(outputFilePath))
            {
                foreach (var entry in file.Document.PageLabels)
                {
                    Console.WriteLine($"Page label {entry.Value.BaseObject}");
                    Console.WriteLine($"    Initial page: {entry.Key.IntValue + 1}");
                    Console.WriteLine($"    Prefix: {entry.Value.Prefix}");
                    Console.WriteLine($"    Number style: {entry.Value.NumberStyle}");
                    Console.WriteLine($"    Number base: {entry.Value.NumberBase}");
                }
            }
        }
    }
}