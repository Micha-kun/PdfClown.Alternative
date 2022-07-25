namespace org.pdfclown.samples.cli
{

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.files;
    using org.pdfclown.documents.interaction.annotations;
    using org.pdfclown.documents.interaction.navigation.document;
    using org.pdfclown.tools;
    using actions = org.pdfclown.documents.interaction.actions;
    using files = org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to inspect the links of a PDF document, retrieving
      their associated text along with its graphic attributes (font, font size, text color,
      text rendering mode, text bounding box...).</summary>
      <remarks>According to PDF spec, page text and links have no mutual relation (contrary to, for
      example, HTML links), so retrieving the text associated to a link is somewhat tricky
      as we have to infer the overlapping areas between links and their corresponding text.</remarks>
    */
    public class LinkParsingSample
      : Sample
    {

        private void PrintAction(
          actions::Action action
          )
        {
            /*
              NOTE: Here we have to deal with reflection as a workaround
              to the lack of type covariance support in C# (so bad -- any better solution?).
            */
            Console.WriteLine($"Action [{action.GetType().Name}] {action.BaseObject}");
            if (action.Is(typeof(actions::GoToDestination<>)))
            {
                if (action.Is(typeof(actions::GotoNonLocal<>)))
                {
                    var destinationFile = (FileSpecification)action.Get("DestinationFile");
                    if (destinationFile != null)
                    { Console.WriteLine($"      Filename: {destinationFile.Path}"); }

                    if (action is actions::GoToEmbedded)
                    {
                        var target = ((actions::GoToEmbedded)action).DestinationPath;
                        Console.WriteLine($"      EmbeddedFilename: {target.EmbeddedFileName} Relation: {target.Relation}");
                    }
                }
                Console.Write("      ");
                this.PrintDestination((Destination)action.Get("Destination"));
            }
            else if (action is actions::GoToURI)
            { Console.WriteLine($"      URI: {((actions::GoToURI)action).URI}"); }
        }

        private void PrintDestination(
          Destination destination
          )
        {
            Console.WriteLine($"{destination.GetType().Name} {destination.BaseObject}");
            Console.Write("        Page ");
            var pageRef = destination.Page;
            if (pageRef is Page)
            {
                var page = (Page)pageRef;
                Console.WriteLine($"{page.Number} [ID: {page.BaseObject}]");
            }
            else
            { Console.WriteLine(((int)pageRef) + 1); }

            var location = destination.Location;
            if (location != null)
            { Console.WriteLine($"        Location {location}"); }

            var zoom = destination.Zoom;
            if (zoom.HasValue)
            { Console.WriteLine($"        Zoom {zoom.Value}"); }
        }

        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new files::File(filePath))
            {
                var document = file.Document;

                // 2. Link extraction from the document pages.
                var extractor = new TextExtractor();
                extractor.AreaTolerance = 2; // 2 pt tolerance on area boundary detection.
                var linkFound = false;
                foreach (var page in document.Pages)
                {
                    if (!this.PromptNextPage(page, !linkFound))
                    {
                        this.Quit();
                        break;
                    }

                    IDictionary<RectangleF?, IList<ITextString>> textStrings = null;
                    linkFound = false;

                    // Get the page annotations!
                    var annotations = page.Annotations;
                    if (!annotations.Exists())
                    {
                        Console.WriteLine("No annotations here.");
                        continue;
                    }

                    // Iterating through the page annotations looking for links...
                    foreach (var annotation in annotations)
                    {
                        if (annotation is Link)
                        {
                            linkFound = true;

                            if (textStrings == null)
                            { textStrings = extractor.Extract(page); }

                            var link = (Link)annotation;
                            var linkBox = link.Box;

                            // Text.
                            /*
                              Extracting text superimposed by the link...
                              NOTE: As links have no strong relation to page text but a weak location correspondence,
                              we have to filter extracted text by link area.
                            */
                            var linkTextBuilder = new StringBuilder();
                            foreach (var linkTextString in extractor.Filter(textStrings, linkBox))
                            { _ = linkTextBuilder.Append(linkTextString.Text); }
                            Console.WriteLine($"Link '{linkTextBuilder}' ");

                            // Position.
                            Console.WriteLine(
                              $"    Position: x:{Math.Round(linkBox.X)},y:{Math.Round(linkBox.Y)},w:{Math.Round(linkBox.Width)},h:{Math.Round(linkBox.Height)}"
                                );

                            // Target.
                            Console.Write("    Target: ");
                            var target = link.Target;
                            if (target is Destination)
                            { this.PrintDestination((Destination)target); }
                            else if (target is actions::Action)
                            { this.PrintAction((actions::Action)target); }
                            else if (target == null)
                            { Console.WriteLine("[not available]"); }
                            else
                            { Console.WriteLine($"[unknown type: {target.GetType().Name}]"); }
                        }
                    }
                    if (!linkFound)
                    {
                        Console.WriteLine("No links here.");
                        continue;
                    }
                }
            }
        }
    }
}