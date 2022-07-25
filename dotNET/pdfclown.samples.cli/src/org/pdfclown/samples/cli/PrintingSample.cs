namespace org.pdfclown.samples.cli
{

    using System;
    using org.pdfclown.files;

    using org.pdfclown.tools;

    /**
      <summary>This sample demonstrates how to print a PDF document.<summary>
      <remarks>Note: printing is currently in pre-alpha stage; therefore this sample is
      nothing but an initial stub (no assumption to work!).</remarks>
    */
    public class PrintingSample
      : Sample
    {
        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new File(filePath))
            {
                // 2. Printing the document...
                var renderer = new Renderer();
                var silent = false;
                if (renderer.Print(file.Document, silent))
                { Console.WriteLine("Print fulfilled."); }
                else
                { Console.WriteLine("Print discarded."); }
            }
        }
    }
}