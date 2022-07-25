namespace org.pdfclown.samples.cli
{

    using System;
    using System.Collections.Generic;
    using System.IO;

    using org.pdfclown.documents;
    using files = org.pdfclown.files;

    /**
      <summary>Abstract sample.</summary>
    */
    public abstract class Sample
    {
        private string inputPath;
        private string outputPath;

        private bool quit;

        private void ApplyDocumentSettings(
  Document document,
  string title,
  string subject,
  string keywords
  )
        {
            if (title == null)
            {
                return;
            }

            // Viewer preferences.
            var view = document.ViewerPreferences;
            view.DocTitleDisplayed = true;

            // Document metadata.
            var info = document.Information;
            info.Author = "Stefano";
            info.CreationDate = DateTime.Now;
            info.Creator = this.GetType().FullName;
            info.Title = $"PDF Clown - {title} sample";
            info.Subject = $"Sample about {subject} using PDF Clown";
            info.Keywords = keywords;
        }

        protected string GetIndentation(
  int level
  )
        { return new string(' ', level); }

        /**
          <summary>Gets the path used to serialize output files.</summary>
          <param name="fileName">Relative output file path.</param>
        */
        protected string GetOutputPath(
          string fileName
          )
        { return $"{this.outputPath}{((fileName != null) ? $"{Path.DirectorySeparatorChar}{fileName}" : string.Empty)}"; }

        /**
          <summary>Gets the path to a sample resource.</summary>
          <param name="resourceName">Relative resource path.</param>
        */
        protected string GetResourcePath(
          string resourceName
          )
        { return $"{this.inputPath}{Path.DirectorySeparatorChar}{resourceName}"; }

        /**
          <summary>Prompts a message to the user.</summary>
          <param name="message">Text to show.</param>
        */
        protected void Prompt(
          string message
          )
        { Utils.Prompt(message); }

        /**
          <summary>Gets the user's choice from the given request.</summary>
          <param name="message">Description of the request to show to the user.</param>
          <returns>User choice.</returns>
        */
        protected string PromptChoice(
          string message
          )
        {
            Console.Write(message);
            try
            { return Console.ReadLine(); }
            catch
            { return null; }
        }

        /**
          <summary>Gets the user's choice from the given options.</summary>
          <param name="options">Available options to show to the user.</param>
          <returns>Chosen option key.</returns>
        */
        protected string PromptChoice(
          IDictionary<string, string> options
          )
        {
            Console.WriteLine();
            foreach (var option in options)
            {
                Console.WriteLine(
                    $"{(option.Key.Equals(string.Empty) ? "ENTER" : $"[{option.Key}]")} {option.Value}"
                    );
            }
            Console.Write("Please select: ");
            return Console.ReadLine();
        }

        protected string PromptFileChoice(
          string inputDescription
          )
        {
            var resourcePath = Path.GetFullPath($"{this.inputPath}pdf");
            Console.WriteLine($"\nAvailable PDF files ({resourcePath}):");

            // Get the list of available PDF files!
            var filePaths = Directory.GetFiles($"{resourcePath}{Path.DirectorySeparatorChar}", "*.pdf");

            // Display files!
            for (var index = 0; index < filePaths.Length; index++)
            { Console.WriteLine($"[{index}] {System.IO.Path.GetFileName(filePaths[index])}"); }

            while (true)
            {
                // Get the user's choice!
                Console.Write($"{inputDescription}: ");
                try
                { return filePaths[int.Parse(Console.ReadLine())]; }
                catch
                {/* NOOP */}
            }
        }

        /**
          <summary>Prompts the user for advancing to the next page.</summary>
          <param name="page">Next page.</param>
          <param name="skip">Whether the prompt has to be skipped.</param>
          <returns>Whether to advance.</returns>
        */
        protected bool PromptNextPage(
          Page page,
          bool skip
          )
        {
            var pageIndex = page.Index;
            if ((pageIndex > 0) && !skip)
            {
                IDictionary<string, string> options = new Dictionary<string, string>();
                options[string.Empty] = "Scan next page";
                options["Q"] = "End scanning";
                if (!this.PromptChoice(options).Equals(string.Empty))
                {
                    return false;
                }
            }

            Console.WriteLine($"\nScanning page {pageIndex + 1}...\n");
            return true;
        }

        /**
          <summary>Prompts the user for a page index to select.</summary>
          <param name="inputDescription">Message prompted to the user.</param>
          <param name="pageCount">Page count.</param>
          <returns>Selected page index.</returns>
        */
        protected int PromptPageChoice(
          string inputDescription,
          int pageCount
          )
        { return this.PromptPageChoice(inputDescription, 0, pageCount); }

        /**
          <summary>Prompts the user for a page index to select.</summary>
          <param name="inputDescription">Message prompted to the user.</param>
          <param name="startIndex">First page index, inclusive.</param>
          <param name="endIndex">Last page index, exclusive.</param>
          <returns>Selected page index.</returns>
        */
        protected int PromptPageChoice(
          string inputDescription,
          int startIndex,
          int endIndex
          )
        {
            int pageIndex;
            try
            { pageIndex = int.Parse(this.PromptChoice($"{inputDescription} [{startIndex + 1}-{endIndex}]: ")) - 1; }
            catch
            { pageIndex = startIndex; }
            if (pageIndex < startIndex)
            { pageIndex = startIndex; }
            else if (pageIndex >= endIndex)
            { pageIndex = endIndex - 1; }

            return pageIndex;
        }

        /**
          <summary>Indicates that the sample was exited before its completion.</summary>
        */
        protected void Quit(
          )
        { this.quit = true; }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">PDF file to serialize.</param>
          <returns>Serialization path.</returns>
        */
        protected string Serialize(
          files::File file
          )
        { return this.Serialize(file, null, null, null); }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">PDF file to serialize.</param>
          <param name="serializationMode">Serialization mode.</param>
          <returns>Serialization path.</returns>
        */
        protected string Serialize(
          files::File file,
          files::SerializationModeEnum? serializationMode
          )
        { return this.Serialize(file, serializationMode, null, null, null); }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">PDF file to serialize.</param>
          <param name="fileName">Output file name.</param>
          <returns>Serialization path.</returns>
        */
        protected string Serialize(
          files::File file,
          string fileName
          )
        { return this.Serialize(file, fileName, null, null); }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">PDF file to serialize.</param>
          <param name="fileName">Output file name.</param>
          <param name="serializationMode">Serialization mode.</param>
          <returns>Serialization path.</returns>
        */
        protected string Serialize(
          files::File file,
          string fileName,
          files::SerializationModeEnum? serializationMode
          )
        { return this.Serialize(file, fileName, serializationMode, null, null, null); }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">PDF file to serialize.</param>
          <param name="title">Document title.</param>
          <param name="subject">Document subject.</param>
          <param name="keywords">Document keywords.</param>
          <returns>Serialization path.</returns>
        */
        protected string Serialize(
          files::File file,
          string title,
          string subject,
          string keywords
          )
        { return this.Serialize(file, null, title, subject, keywords); }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">PDF file to serialize.</param>
          <param name="serializationMode">Serialization mode.</param>
          <param name="title">Document title.</param>
          <param name="subject">Document subject.</param>
          <param name="keywords">Document keywords.</param>
          <returns>Serialization path.</returns>
        */
        protected string Serialize(
          files::File file,
          files::SerializationModeEnum? serializationMode,
          string title,
          string subject,
          string keywords
          )
        { return this.Serialize(file, this.GetType().Name, serializationMode, title, subject, keywords); }

        /**
          <summary>Serializes the given PDF Clown file object.</summary>
          <param name="file">PDF file to serialize.</param>
          <param name="fileName">Output file name.</param>
          <param name="serializationMode">Serialization mode.</param>
          <param name="title">Document title.</param>
          <param name="subject">Document subject.</param>
          <param name="keywords">Document keywords.</param>
          <returns>Serialization path.</returns>
        */
        protected string Serialize(
          files::File file,
          string fileName,
          files::SerializationModeEnum? serializationMode,
          string title,
          string subject,
          string keywords
          )
        {
            this.ApplyDocumentSettings(file.Document, title, subject, keywords);

            Console.WriteLine();

            if (!serializationMode.HasValue)
            {
                if (file.Reader == null) // New file.
                { serializationMode = files::SerializationModeEnum.Standard; }
                else // Existing file.
                {
                    Console.WriteLine("[0] Standard serialization");
                    Console.WriteLine("[1] Incremental update");
                    Console.Write("Please select a serialization mode: ");
                    try
                    { serializationMode = (files::SerializationModeEnum)int.Parse(Console.ReadLine()); }
                    catch
                    { serializationMode = files::SerializationModeEnum.Standard; }
                }
            }

            var outputFilePath = $"{this.outputPath}{fileName}.{serializationMode}.pdf";

            // Save the file!
            /*
              NOTE: You can also save to a generic target stream (see Save() method overloads).
            */
            try
            { file.Save(outputFilePath, serializationMode.Value); }
            catch (Exception e)
            {
                Console.WriteLine($"File writing failed: {e.Message}");
                Console.WriteLine(e.StackTrace);
            }
            Console.WriteLine($"\nOutput: {outputFilePath}");

            return outputFilePath;
        }

        /**
          <summary>Gets the path used to serialize output files.</summary>
        */
        protected string OutputPath => this.GetOutputPath(null);

        internal void Initialize(
  string inputPath,
  string outputPath
  )
        {
            this.inputPath = inputPath;
            this.outputPath = outputPath;
        }

        /**
<summary>Gets whether the sample was exited before its completion.</summary>
*/
        public bool IsQuit(
          )
        { return this.quit; }

        /**
          <summary>Executes the sample.</summary>
          <returns>Whether the sample has been completed.</returns>
        */
        public abstract void Run(
          );
    }
}