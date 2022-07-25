namespace org.pdfclown.samples.cli
{

    using System;
    using System.Collections.Generic;
    using org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to inspect the AcroForm fields of a PDF document.</summary>
    */
    public class AcroFormParsingSample
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

                // 2. Get the acroform!
                var form = document.Form;
                if (!form.Exists())
                { Console.WriteLine("\nNo acroform available (AcroForm dictionary not found)."); }
                else
                {
                    Console.WriteLine("\nIterating through the fields collection...\n");

                    // 3. Showing the acroform fields...
                    var objCounters = new Dictionary<string, int>();
                    foreach (var field in form.Fields.Values)
                    {
                        Console.WriteLine($"* Field '{field.FullName}' ({field.BaseObject})");

                        var typeName = field.GetType().Name;
                        Console.WriteLine($"    Type: {typeName}");
                        Console.WriteLine($"    Value: {field.Value}");
                        Console.WriteLine($"    Data: {field.BaseDataObject}");

                        var widgetIndex = 0;
                        foreach (var widget in field.Widgets)
                        {
                            Console.WriteLine($"    Widget {++widgetIndex}:");
                            var widgetPage = widget.Page;
                            Console.WriteLine($"      Page: {((widgetPage == null) ? "undefined" : $"{widgetPage.Number} ({widgetPage.BaseObject})")}");

                            var widgetBox = widget.Box;
                            Console.WriteLine($"      Coordinates: {{x:{Math.Round(widgetBox.X)}; y:{Math.Round(widgetBox.Y)}; width:{Math.Round(widgetBox.Width)}; height:{Math.Round(widgetBox.Height)}}}");
                        }

                        objCounters[typeName] = (objCounters.ContainsKey(typeName) ? objCounters[typeName] : 0) + 1;
                    }

                    var fieldCount = form.Fields.Count;
                    if (fieldCount == 0)
                    { Console.WriteLine("No field available."); }
                    else
                    {
                        Console.WriteLine("\nFields partial counts (grouped by type):");
                        foreach (var entry in objCounters)
                        { Console.WriteLine($" {entry.Key}: {entry.Value}"); }
                        Console.WriteLine($"Fields total count: {fieldCount}");
                    }
                }
            }
        }
    }
}