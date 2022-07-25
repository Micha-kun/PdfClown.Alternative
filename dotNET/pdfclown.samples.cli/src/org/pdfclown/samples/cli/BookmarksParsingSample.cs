namespace org.pdfclown.samples.cli
{

    using System;
    using org.pdfclown.documents;
    using org.pdfclown.documents.files;
    using org.pdfclown.documents.interaction.navigation.document;
    using org.pdfclown.files;

    using actions = org.pdfclown.documents.interaction.actions;

    /**
      <summary>This sample demonstrates how to inspect the bookmarks of a PDF document.</summary>
    */
    public class BookmarksParsingSample
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

        private void PrintBookmarks(
          Bookmarks bookmarks
          )
        {
            if (bookmarks == null)
            {
                return;
            }

            foreach (var bookmark in bookmarks)
            {
                // Show current bookmark!
                Console.WriteLine($"Bookmark '{bookmark.Title}'");
                Console.Write("    Target: ");
                var target = bookmark.Target;
                if (target is Destination)
                { this.PrintDestination((Destination)target); }
                else if (target is actions::Action)
                { this.PrintAction((actions::Action)target); }
                else if (target == null)
                { Console.WriteLine("[not available]"); }
                else
                { Console.WriteLine($"[unknown type: {target.GetType().Name}]"); }

                // Show child bookmarks!
                this.PrintBookmarks(bookmark.Bookmarks);
            }
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
            using (var file = new File(filePath))
            {
                var document = file.Document;

                // 2. Get the bookmarks collection!
                var bookmarks = document.Bookmarks;
                if (!bookmarks.Exists())
                { Console.WriteLine("\nNo bookmark available (Outline dictionary not found)."); }
                else
                {
                    Console.WriteLine("\nIterating through the bookmarks collection (please wait)...\n");
                    // 3. Show the bookmarks!
                    this.PrintBookmarks(bookmarks);
                }
            }
        }
    }
}