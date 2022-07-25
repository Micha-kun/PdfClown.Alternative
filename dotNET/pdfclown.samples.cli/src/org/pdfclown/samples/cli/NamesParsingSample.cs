namespace org.pdfclown.samples.cli
{

    using System;
    using org.pdfclown.documents;

    using org.pdfclown.files;
    using org.pdfclown.objects;

    /**
      <summary>This sample demonstrates how to inspect the object names within a PDF document.</summary>
    */
    public class NamesParsingSample
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

                // 2. Named objects extraction.
                var names = document.Names;
                if (!names.Exists())
                { Console.WriteLine("\nNo names dictionary."); }
                else
                {
                    Console.WriteLine($"\nNames dictionary found ({names.DataContainer.Reference})");

                    var namedDestinations = names.Destinations;
                    if (!namedDestinations.Exists())
                    { Console.WriteLine("\nNo named destinations."); }
                    else
                    {
                        Console.WriteLine($"\nNamed destinations found ({namedDestinations.DataContainer.Reference})");

                        // Parsing the named destinations...
                        foreach (var namedDestination in namedDestinations)
                        {
                            var key = namedDestination.Key;
                            var value = namedDestination.Value;

                            Console.WriteLine($"  Destination '{key}' ({value.DataContainer.Reference})");

                            Console.Write("    Target Page: number = ");
                            var pageRef = value.Page;
                            if (pageRef is int) // NOTE: numeric page refs are typical of remote destinations.
                            { Console.WriteLine(((int)pageRef) + 1); }
                            else // NOTE: explicit page refs are typical of local destinations.
                            {
                                var page = (Page)pageRef;
                                Console.WriteLine($"{page.Number}; ID = {((PdfReference)page.BaseObject).Id}");
                            }
                        }

                        Console.WriteLine($"Named destinations count = {namedDestinations.Count}");
                    }
                }
            }
        }
    }
}