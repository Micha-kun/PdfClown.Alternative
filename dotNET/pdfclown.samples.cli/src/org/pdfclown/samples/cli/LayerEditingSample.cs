namespace org.pdfclown.samples.cli
{

    using System;
    using System.Collections.Generic;

    using org.pdfclown.documents.contents.layers;
    using org.pdfclown.files;

    /**
      <summary>This sample demonstrates how to edit existing layers.</summary>
    */
    public class LayerEditingSample
      : Sample
    {

        private void ShowLayer(
          IUILayerNode layerNode,
          int index,
          string indentation
          )
        { Console.WriteLine($"{indentation}{((layerNode is Layer) ? $"[{index}] " : string.Empty)}\"{layerNode.Title}\" ({layerNode.GetType().Name})"); }

        private void ShowUILayers(
          UILayers uiLayers,
          int level,
          List<Layer> layers
          )
        {
            var indentation = this.GetIndentation(level);
            foreach (var layerNode in uiLayers)
            {
                this.ShowLayer(layerNode, layers.Count, indentation);

                if (layerNode is Layer)
                { layers.Add((Layer)layerNode); }

                this.ShowUILayers(layerNode.Children, level + 1, layers);
            }
        }

        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new File(filePath))
            {
                var document = file.Document;

                // 2. Get the layer definition!
                var layerDefinition = document.Layer;
                if (!layerDefinition.Exists())
                { Console.WriteLine("\nNo layer definition available."); }
                else
                {
                    while (true)
                    {
                        var layers = new List<Layer>();

                        // 3.1. Show structured layers!
                        Console.WriteLine("\nLayer structure:\n");
                        this.ShowUILayers(layerDefinition.UILayers, 0, layers);

                        // 3.2. Show unstructured layers!
                        var hiddenShown = false;
                        foreach (var layer in layerDefinition.Layers) // NOTE: LayerDefinition.Layers comprises all the layers (both structured and unstructured).
                        {
                            if (!layers.Contains(layer))
                            {
                                if (!hiddenShown)
                                {
                                    Console.WriteLine("Hidden layers (not displayed in the viewer panel)");
                                    hiddenShown = true;
                                }

                                this.ShowLayer(layer, layers.Count, " ");
                                layers.Add(layer);
                            }
                        }

                        Console.WriteLine("[Q] Exit");

                        string choice;
                        while (true)
                        {
                            choice = this.PromptChoice("Choose a layer to remove:").ToUpper();
                            if ("Q".Equals(choice))
                            {
                                break;
                            }
                            else
                            {
                                int layerIndex;
                                try
                                { layerIndex = int.Parse(choice); }
                                catch
                                { continue; }
                                if ((layerIndex < 0) || (layerIndex >= layers.Count))
                                {
                                    continue;
                                }

                                Console.WriteLine("\nWhat to do with the contents associated to the removed layer?");
                                var contentRemovalOptions = new Dictionary<string, string>
                {
                  {"0", "Remove layered content"},
                  {"1", "Flatten layered content"}
                };
                                int contentRemovalChoice;
                                try
                                { contentRemovalChoice = int.Parse(this.PromptChoice(contentRemovalOptions)); }
                                catch
                                { contentRemovalChoice = 0; }

                                // 4. Remove the chosen layer!
                                _ = layers[layerIndex].Delete(contentRemovalChoice == 1);
                                break;
                            }
                        }
                        if ("Q".Equals(choice))
                        {
                            break;
                        }
                    }
                    if (file.Updated)
                    { _ = this.Serialize(file, "Layer editing", "removing layers", "layers, optional content"); }
                }
            }
        }
    }
}