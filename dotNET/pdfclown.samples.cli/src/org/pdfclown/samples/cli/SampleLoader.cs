namespace org.pdfclown.samples.cli
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;

    /**
      <summary>Command-line sample loader.</summary>
    */
    public static class SampleLoader
    {
        private static readonly string ClassName = typeof(SampleLoader).FullName;

        private static readonly string Properties_InputPath = $"{ClassName}.inputPath";
        private static readonly string Properties_OutputPath = $"{ClassName}.outputPath";

        private static readonly string QuitChoiceSymbol = "Q";

        private static void Run(string inputPath, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                _ = Directory.CreateDirectory(outputPath);
            }

            while (true)
            {
                // Get the current assembly!
                var assembly = Assembly.GetExecutingAssembly();
                // Get all the types inside the current assembly!
                var types = new List<Type>(assembly.GetTypes());
                types.Sort(new TypeComparer());

                Console.WriteLine("\nAvailable samples:");
                // Instantiate the list of available samples!
                var sampleTypes = new List<Type>();
                // Picking available samples...
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Sample)))
                    {
                        sampleTypes.Add(type);
                        Console.WriteLine($"[{sampleTypes.IndexOf(type)}] {type.Name}");
                    }
                }
                Console.WriteLine($"[{QuitChoiceSymbol}] (Quit)");

                // Getting the user's choice...
                Type sampleType = null;
                do
                {
                    Console.Write("Please select a sample: ");
                    try
                    {
                        var choice = Console.ReadLine();
                        if (choice.ToUpper().Equals(QuitChoiceSymbol)) // Quit.
                        {
                            return;
                        }

                        sampleType = sampleTypes[int.Parse(choice)];
                    }
                    catch
                    {/* NOOP */
                    }
                } while (sampleType == null);

                Console.WriteLine($"\n{sampleType.Name} running...");

                // Instantiate the sample!
                var sample = (Sample)Activator.CreateInstance(sampleType);
                sample.Initialize(inputPath, outputPath);

                // Run the sample!
                try
                {
                    sample.Run();
                    if (!sample.IsQuit())
                    {
                        Utils.Prompt("Sample finished.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception happened while running the sample:");
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public static void Main(
            string[] args
)
        {
            Console.WriteLine("\nSampleLoader running...");
            var pdfClownAssembly = Assembly.GetAssembly(typeof(Engine));
            Console.WriteLine(
                $"\n{((AssemblyTitleAttribute)pdfClownAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title} version {pdfClownAssembly.GetName().Version}");

            Run(
                ConfigurationManager.AppSettings.Get(Properties_InputPath),
                ConfigurationManager.AppSettings.Get(Properties_OutputPath));

            Console.WriteLine("\nSampleLoader finished.\n");
        }

        private class TypeComparer : IComparer<Type>
        {
            public int Compare(Type type1, Type type2) { return type1.Name.CompareTo(type2.Name); }
        }
    }
}
