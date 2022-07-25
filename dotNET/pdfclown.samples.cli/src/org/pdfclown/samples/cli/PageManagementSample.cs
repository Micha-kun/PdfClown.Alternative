namespace org.pdfclown.samples.cli
{

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text;
    using org.pdfclown.files;
    using org.pdfclown.objects;
    using org.pdfclown.tools;
    using io = System.IO;

    /**
      <summary>This sample demonstrates how to manipulate the pages collection within
      a PDF document, to perform page data size calculations, additions, movements,
      removals, extractions and splits of groups of pages.</summary>
    */
    public class PageManagementSample
      : Sample
    {

        private ActionEnum PromptAction(
          )
        {
            var actions = (ActionEnum[])Enum.GetValues(typeof(ActionEnum));
            IDictionary<string, string> options = new Dictionary<string, string>();
            for (
              int actionIndex = 0,
                actionsLength = actions.Length;
              actionIndex < actionsLength;
              actionIndex++
              )
            { options[actionIndex.ToString()] = actions[actionIndex].GetDescription(); }

            try
            { return actions[int.Parse(this.PromptChoice(options))]; }
            catch
            { return actions[0]; }
        }

        /**
          <summary>Serializes the specified PDF file.</summary>
          <param name="file">File to serialize.</param>
          <param name="action">Generator.</param>
        */
        private void Serialize(
          File file,
          ActionEnum action
          )
        { this.Serialize(file, action, null); }

        /**
          <summary>Serializes the specified PDF file.</summary>
          <param name="file">File to serialize.</param>
          <param name="action">Generator.</param>
          <param name="index">File index.</param>
        */
        private void Serialize(
          File file,
          ActionEnum action,
          int? index
          )
        {
            _ = this.Serialize(
              file,
              $"{this.GetType().Name}_{action}{(index.HasValue ? $".{index.Value}" : string.Empty)}",
              null,
              action.ToString(),
              "managing document pages",
              action.ToString()
              );
        }

        public override void Run(
          )
        {
            var action = this.PromptAction();

            // Opening the PDF file...
            var mainFilePath = this.PromptFileChoice("Please select a PDF file");
            using (var mainFile = new File(mainFilePath))
            {
                var mainDocument = mainFile.Document;
                var mainPages = mainDocument.Pages;
                var mainPagesCount = mainPages.Count;

                switch (action)
                {
                    case ActionEnum.PageDataSizeCalculation:
                        Console.WriteLine("\nThis algorithm calculates the data size (expressed in bytes) of the selected document's pages.");
                        Console.WriteLine("Legend:");
                        Console.WriteLine(" * full: page data size encompassing all its dependencies (like shared resources) -- this is the size of the page when extracted as a single-page document;");
                        Console.WriteLine(" * differential: additional page data size -- this is the extra-content that's not shared with previous pages;");
                        Console.WriteLine(" * incremental: data size of the page sublist encompassing all the previous pages and the current one.\n");

                        // Calculating the page data sizes...
                        var visitedReferences = new HashSet<PdfReference>();
                        long incrementalDataSize = 0;
                        foreach (var page in mainPages)
                        {
                            var pageFullDataSize = PageManager.GetSize(page);
                            var pageDifferentialDataSize = PageManager.GetSize(page, visitedReferences);
                            incrementalDataSize += pageDifferentialDataSize;

                            Console.WriteLine(
                              $"Page {page.Number}: {pageFullDataSize} (full); {pageDifferentialDataSize} (differential); {incrementalDataSize} (incremental)"
                              );
                        }
                        break;
                    case ActionEnum.BlankPageDetection:
                        Console.WriteLine(
                          "\nThis algorithm makes a simple guess about whether a page should be considered empty:"
                          + "\nit evaluates the middle portion (70%) of a page assuming that possible contents"
                          + "\noutside this area would NOT qualify as actual (informative) content (such as"
                          + "\nredundant patterns like footers and headers). Obviously, this assumption may need"
                          + "\nsome fine-tuning as each document features its own layout ratios. Alternatively,"
                          + "\nan adaptive algorithm should automatically evaluate the content role based on its"
                          + "\ntypographic attributes in relation to the other contents existing in the same page"
                          + "\nor document.\n");
                        var blankPageCount = 0;
                        foreach (var page in mainPages)
                        {
                            var pageBox = page.Box;
                            var margin = new SizeF(pageBox.Width * .15f, pageBox.Height * .15f);
                            var contentBox = new RectangleF(margin.Width, margin.Height, pageBox.Width - (margin.Width * 2), pageBox.Height - (margin.Height * 2));
                            if (PageManager.IsBlank(page, contentBox))
                            {
                                blankPageCount++;
                                Console.WriteLine($"Page {page.Number} is blank");
                            }
                        }
                        Console.WriteLine((blankPageCount > 0) ? $"Blank pages detected: {blankPageCount} of {mainPages.Count}" : "No blank pages detected.");
                        break;
                    case ActionEnum.PageAddition:
                    {
                        // Opening the source file...
                        var sourceFilePath = this.PromptFileChoice("Select the source PDF file");
                        using (var sourceFile = new File(sourceFilePath))
                        {
                            // Source page collection.
                            var sourcePages = sourceFile.Document.Pages;
                            // Source page count.
                            var sourcePagesCount = sourcePages.Count;

                            // First page to add.
                            var fromSourcePageIndex = this.PromptPageChoice("Select the start source page to add", sourcePagesCount);
                            // Last page to add.
                            var toSourcePageIndex = this.PromptPageChoice("Select the end source page to add", fromSourcePageIndex, sourcePagesCount) + 1;
                            // Target position.
                            var targetPageIndex = this.PromptPageChoice("Select the position where to insert the source pages", mainPagesCount + 1);

                            // Add the chosen page range to the main document!
                            new PageManager(mainDocument).Add(
                              targetPageIndex,
                              sourcePages.GetSlice(
                                fromSourcePageIndex,
                                toSourcePageIndex
                                )
                              );
                        }
                        // Serialize the main file!
                        this.Serialize(mainFile, action);
                    }
                    break;
                    case ActionEnum.PageMovement:
                    {
                        // First page to move.
                        var fromSourcePageIndex = this.PromptPageChoice("Select the start page to move", mainPagesCount);
                        // Last page to move.
                        var toSourcePageIndex = this.PromptPageChoice("Select the end page to move", fromSourcePageIndex, mainPagesCount) + 1;
                        // Target position.
                        var targetPageIndex = this.PromptPageChoice("Select the position where to insert the pages", mainPagesCount + 1);

                        // Move the chosen page range!
                        new PageManager(mainDocument).Move(
                          fromSourcePageIndex,
                          toSourcePageIndex,
                          targetPageIndex
                          );

                        // Serialize the main file!
                        this.Serialize(mainFile, action);
                    }
                    break;
                    case ActionEnum.PageRemoval:
                    {
                        // First page to remove.
                        var fromPageIndex = this.PromptPageChoice("Select the start page to remove", mainPagesCount);
                        // Last page to remove.
                        var toPageIndex = this.PromptPageChoice("Select the end page to remove", fromPageIndex, mainPagesCount) + 1;

                        // Remove the chosen page range!
                        new PageManager(mainDocument).Remove(
                          fromPageIndex,
                          toPageIndex
                          );

                        // Serialize the main file!
                        this.Serialize(mainFile, action);
                    }
                    break;
                    case ActionEnum.PageExtraction:
                    {
                        // First page to extract.
                        var fromPageIndex = this.PromptPageChoice("Select the start page", mainPagesCount);
                        // Last page to extract.
                        var toPageIndex = this.PromptPageChoice("Select the end page", fromPageIndex, mainPagesCount) + 1;

                        // Extract the chosen page range!
                        var targetDocument = new PageManager(mainDocument).Extract(
                          fromPageIndex,
                          toPageIndex
                          );

                        // Serialize the target file!
                        this.Serialize(targetDocument.File, action);
                    }
                    break;
                    case ActionEnum.DocumentMerge:
                    {
                        // Opening the source file...
                        var sourceFilePath = this.PromptFileChoice("Select the source PDF file");
                        using (var sourceFile = new File(sourceFilePath))
                        {
                            // Append the chosen source document to the main document!
                            new PageManager(mainDocument).Add(sourceFile.Document);
                        }
                        // Serialize the main file!
                        this.Serialize(mainFile, action);
                    }
                    break;
                    case ActionEnum.DocumentBurst:
                    {
                        // Split the document into single-page documents!
                        var splitDocuments = new PageManager(mainDocument).Split();

                        // Serialize the split files!
                        var index = 0;
                        foreach (var splitDocument in splitDocuments)
                        { this.Serialize(splitDocument.File, action, ++index); }
                    }
                    break;
                    case ActionEnum.DocumentSplitByPageIndex:
                    {
                        // Number of splits to apply to the source document.
                        int splitCount;
                        try
                        { splitCount = int.Parse(this.PromptChoice("Number of split positions: ")); }
                        catch
                        { splitCount = 0; }

                        // Split positions within the source document.
                        var splitIndexes = new int[splitCount];
                        var prevSplitIndex = 0;
                        for (var index = 0; index < splitCount; index++)
                        {
                            var splitIndex = this.PromptPageChoice($"Position {index + 1} of {splitCount}", prevSplitIndex + 1, mainPagesCount);
                            splitIndexes[index] = splitIndex;
                            prevSplitIndex = splitIndex;
                        }

                        // Split the document at the chosen positions!
                        var splitDocuments = new PageManager(mainDocument).Split(splitIndexes);

                        // Serialize the split files!
                        {
                            var index = 0;
                            foreach (var splitDocument in splitDocuments)
                            { this.Serialize(splitDocument.File, action, ++index); }
                        }
                    }
                    break;
                    case ActionEnum.DocumentSplitOnMaximumFileSize:
                    {
                        // Maximum file size.
                        long maxDataSize;
                        var mainFileSize = new io::FileInfo(mainFilePath).Length;
                        int kbMaxDataSize;
                        do
                        {
                            try
                            { kbMaxDataSize = int.Parse(this.PromptChoice("Max file size (KB): ")); }
                            catch
                            { kbMaxDataSize = 0; }
                        } while (kbMaxDataSize == 0);
                        maxDataSize = kbMaxDataSize << 10;
                        if (maxDataSize > mainFileSize)
                        { maxDataSize = mainFileSize; }

                        // Split the document on maximum file size!
                        var splitDocuments = new PageManager(mainDocument).Split(maxDataSize);

                        // Serialize the split files!

                        var index = 0;
                        foreach (var splitDocument in splitDocuments)
                        { this.Serialize(splitDocument.File, action, ++index); }
                    }
                    break;
                }
            }
        }

        internal enum ActionEnum
        {
            PageDataSizeCalculation,
            BlankPageDetection,
            PageAddition,
            PageMovement,
            PageRemoval,
            PageExtraction,
            DocumentMerge,
            DocumentBurst,
            DocumentSplitByPageIndex,
            DocumentSplitOnMaximumFileSize
        }
    }

    internal static class ActionEnumExtension
    {
        public static string GetDescription(
          this PageManagementSample.ActionEnum value
          )
        {
            var builder = new StringBuilder();
            foreach (var c in value.ToString())
            {
                if (char.IsUpper(c) && (builder.Length > 0))
                { _ = builder.Append(" "); }

                _ = builder.Append(c);
            }
            return builder.ToString();
        }
    }
}