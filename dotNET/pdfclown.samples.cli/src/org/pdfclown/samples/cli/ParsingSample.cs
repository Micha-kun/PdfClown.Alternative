namespace org.pdfclown.samples.cli
{

    using System;
    using System.Collections.Generic;
    using System.Xml;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.objects;
    using org.pdfclown.files;
    using org.pdfclown.objects;
    using io = System.IO;

    /**
      <summary>This sample demonstrates how to inspect the structure of a PDF document.</summary>
      <remarks>This sample is just a limited exercise: see the API documentation
      to exploit all the available access functionalities.</remarks>
    */
    public class ParsingSample
      : Sample
    {

        private int PrintContentObjects(
          IList<ContentObject> objects,
          int index,
          int level
          )
        {
            var indentation = this.GetIndentation(level);
            foreach (var obj in objects)
            {
                /*
                  NOTE: Contents are expressed through both simple operations and composite objects.
                */
                if (obj is Operation)
                { Console.WriteLine($"   {indentation}{++index}: {obj}"); }
                else if (obj is CompositeObject)
                {
                    Console.WriteLine(
                      $"   {indentation}{obj.GetType().Name}\n   {indentation}{{"
                      );
                    index = this.PrintContentObjects(((CompositeObject)obj).Objects, index, level + 1);
                    Console.WriteLine($"   {indentation}}}");
                }
                if (index > 9)
                {
                    break;
                }
            }
            return index;
        }

        private void PrintPageInfo(
          Page page,
          int index
          )
        {
            // 1. Showing basic page information...
            Console.WriteLine($" Index (calculated): {page.Index} (should be {index})");
            Console.WriteLine($" ID: {((PdfReference)page.BaseObject).Id}");
            var pageDictionary = page.BaseDataObject;
            Console.WriteLine(" Dictionary entries:");
            foreach (var entry in pageDictionary)
            { Console.WriteLine($"  {entry.Key.Value} = {entry.Value}"); }

            // 2. Showing page contents information...
            var contents = page.Contents;
            Console.WriteLine($" Content objects count: {contents.Count}");
            Console.WriteLine(" Content head:");
            _ = this.PrintContentObjects(contents, 0, 0);

            // 3. Showing page resources information...

            var resources = page.Resources;
            Console.WriteLine(" Resources:");
            try
            { Console.WriteLine($"  Font count: {resources.Fonts.Count}"); }
            catch { }
            try
            { Console.WriteLine($"  XObjects count: {resources.XObjects.Count}"); }
            catch { }
            try
            { Console.WriteLine($"  ColorSpaces count: {resources.ColorSpaces.Count}"); }
            catch { }
        }

        private string ToString(
          XmlDocument document
          )
        {
            var stringWriter = new io::StringWriter();
            var xmlTextWriter = new XmlTextWriter(stringWriter);
            document.WriteTo(xmlTextWriter);
            return stringWriter.ToString();
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
                // 2.1. Metadata.
                // 2.1.1. Basic metadata.
                Console.WriteLine("\nDocument information:");
                var info = document.Information;
                if (info.Exists())
                {
                    foreach (var infoEntry in info)
                    { Console.WriteLine($"{infoEntry.Key}: {infoEntry.Value}"); }
                }
                else
                { Console.WriteLine("No information available (Info dictionary doesn't exist)."); }

                // 2.1.2. Advanced metadata.
                Console.WriteLine("\nDocument metadata (XMP):");
                var metadata = document.Metadata;
                if (metadata.Exists())
                {
                    try
                    {
                        var metadataContent = metadata.Content;
                        Console.WriteLine(this.ToString(metadataContent));
                    }
                    catch (Exception e)
                    { Console.WriteLine($"Metadata extraction failed: {e.Message}"); }
                }
                else
                { Console.WriteLine("No metadata available (Metadata stream doesn't exist)."); }

                Console.WriteLine("\nIterating through the indirect-object collection (please wait)...");

                // 2.2. Counting the indirect objects, grouping them by type...
                var objCounters = new SortedDictionary<string, int>();
                objCounters["xref free entry"] = 0;
                foreach (var obj in file.IndirectObjects)
                {
                    if (obj.IsInUse()) // In-use entry.
                    {
                        var dataObject = obj.DataObject;
                        var typeName = (dataObject != null) ? dataObject.GetType().Name : "empty entry";
                        if (objCounters.ContainsKey(typeName))
                        { objCounters[typeName]++; }
                        else
                        { objCounters[typeName] = 1; }
                    }
                    else // Free entry.
                    { objCounters["xref free entry"]++; }
                }
                Console.WriteLine("\nIndirect objects partial counts (grouped by PDF object type):");
                foreach (var keyValuePair in objCounters)
                { Console.WriteLine($" {keyValuePair.Key}: {keyValuePair.Value}"); }
                Console.WriteLine($"Indirect objects total count: {file.IndirectObjects.Count}");

                // 2.3. Showing some page information...
                var pages = document.Pages;
                var pageCount = pages.Count;
                Console.WriteLine($"\nPage count: {pageCount}");

                var pageIndex = (int)Math.Round(pageCount / 2d);
                Console.WriteLine("Mid page:");
                this.PrintPageInfo(pages[pageIndex], pageIndex);

                pageIndex++;
                if (pageIndex < pageCount)
                {
                    Console.WriteLine("Next page:");
                    this.PrintPageInfo(pages[pageIndex], pageIndex);
                }
            }
        }
    }
}