namespace org.pdfclown.samples.cli
{
    using System.Drawing;
    using System.IO;
    using org.pdfclown.documents;

    using org.pdfclown.documents.contents.composition;
    using files = org.pdfclown.files;
    using fonts = org.pdfclown.documents.contents.fonts;

    /**
      <summary>This sample concentrates on proper fitting of styled text within a given PDF page area (block
      frame), from the beginning of "Alice in Wonderland", Chapter 1 ("Down the Rabbit-Hole").</summary>
    */
    public class TypesettingSample
      : Sample
    {
        private static readonly int Margin_X = 50;
        private static readonly int Margin_Y = 50;

        private void Build(
          Document document
          )
        {
            // Add a page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            var pageSize = page.Size;

            // Create a content composer for the content stream!
            /*
              NOTE: There are several ways to add contents to a content stream:
              - adding content objects directly to the Contents collection;
              - adding content objects through a ContentScanner instance;
              - invoking basic drawing functions through a PrimitiveComposer instance;
              - invoking advanced static-positioning functions through a BlockComposer instance;
              - invoking advanced dynamic-positioning functions through a FlowComposer instance (currently not implemented yet).
            */
            var composer = new PrimitiveComposer(page);
            // Wrap the content composer within a block filter!
            /*
              NOTE: The block filter is a basic typesetter. It exposes higher-level graphical
              functionalities (horizontal/vertical alignment, indentation, paragraph composition etc.)
              leveraging the content composer primitives.
              It's important to note that this is just an intermediate abstraction layer of the typesetting
              stack: further abstract levels could sit upon it, allowing the convenient treatment of
              typographic entities like titles, paragraphs, columns, tables, headers, footers etc.
              When such further abstract levels are available, the final user (developer of consuming
              applications) won't care any more of the details you can see here in the following code lines
              (such as bothering to select the first-letter font...).
            */
            var blockComposer = new BlockComposer(composer);

            _ = composer.BeginLocalState();
            // Define the block frame that will constrain our contents on the page canvas!
            var frame = new RectangleF(
              Margin_X,
              Margin_Y,
      pageSize.Width - (Margin_X * 2),
      pageSize.Height - (Margin_Y * 2)
              );
            // Begin the title block!
            blockComposer.Begin(frame, XAlignmentEnum.Left, YAlignmentEnum.Top);
            var decorativeFont = fonts::Font.Get(
              document,
              this.GetResourcePath($"fonts{Path.DirectorySeparatorChar}Ruritania-Outline.ttf")
              );
            composer.SetFont(decorativeFont, 56);
            _ = blockComposer.ShowText("Chapter 1");
            _ = blockComposer.ShowBreak();
            composer.SetFont(decorativeFont, 32);
            _ = blockComposer.ShowText("Down the Rabbit-Hole");
            // End the title block!
            blockComposer.End();
            // Update the block frame in order to begin after the title!
            frame = new RectangleF(
      blockComposer.BoundBox.X,
      blockComposer.BoundBox.Y + blockComposer.BoundBox.Height,
      blockComposer.BoundBox.Width,
      pageSize.Height - Margin_Y - (blockComposer.BoundBox.Y + blockComposer.BoundBox.Height)
              );
            // Begin the body block!
            blockComposer.Begin(frame, XAlignmentEnum.Justify, YAlignmentEnum.Bottom);
            var bodyFont = fonts::Font.Get(
              document,
              this.GetResourcePath($"fonts{Path.DirectorySeparatorChar}TravelingTypewriter.otf")
              );
            composer.SetFont(bodyFont, 14);
            _ = composer.BeginLocalState();
            composer.SetFont(decorativeFont, 28);
            _ = blockComposer.ShowText("A");
            composer.End();
            _ = blockComposer.ShowText("lice was beginning to get very tired of sitting by her sister on the bank, and of having nothing to do: once or twice she had peeped into the book her sister was reading, but it had no pictures or conversations in it, 'and what is the use of a book,' thought Alice 'without pictures or conversation?'");
            // Define new-paragraph first-line offset!
            var breakSize = new SizeF(24, 8); // Indentation (24pt) and top margin (8pt).
                                              // Begin a new paragraph!
            _ = blockComposer.ShowBreak(breakSize);
            _ = blockComposer.ShowText("So she was considering in her own mind (as well as she could, for the hot day made her feel very sleepy and stupid), whether the pleasure of making a daisy-chain would be worth the trouble of getting up and picking the daisies, when suddenly a White Rabbit with pink eyes ran close by her.");
            // Begin a new paragraph!
            _ = blockComposer.ShowBreak(breakSize);
            _ = blockComposer.ShowText("There was nothing so VERY remarkable in that; nor did Alice think it so VERY much out of the way to hear the Rabbit say to itself, 'Oh dear! Oh dear! I shall be late!' (when she thought it over afterwards, it occurred to her that she ought to have wondered at this, but at the time it all seemed quite natural); but when the Rabbit actually TOOK A WATCH OUT OF ITS WAISTCOAT- POCKET, and looked at it, and then hurried on, Alice started to her feet, for it flashed across her mind that she had never before seen a rabbit with either a waistcoat-pocket, or a watch to take out of it, and burning with curiosity, she ran across the field after it, and fortunately was just in time to see it pop down a large rabbit-hole under the hedge.");
            // End the body block!
            blockComposer.End();
            composer.End();

            _ = composer.BeginLocalState();
            composer.Rotate(
              90,
              new PointF(
                pageSize.Width - 50,
                pageSize.Height - 25
                )
              );
            blockComposer = new BlockComposer(composer);
            blockComposer.Begin(
              new RectangleF(0, 0, 300, 50),
              XAlignmentEnum.Left,
              YAlignmentEnum.Middle
              );
            composer.SetFont(bodyFont, 8);
            _ = blockComposer.ShowText($"Generated by PDF Clown on {System.DateTime.Now}");
            _ = blockComposer.ShowBreak();
            _ = blockComposer.ShowText("For more info, visit http://www.pdfclown.org");
            blockComposer.End();
            composer.End();

            // Flush the contents into the page!
            composer.Flush();
        }

        public override void Run(
          )
        {
            // 1. PDF file instantiation.
            var file = new files::File();
            var document = file.Document;

            // 2. Content creation.
            this.Build(document);

            // 3. Serialize the PDF file!
            _ = this.Serialize(file, "Typesetting", "demonstrating how to add style to contents", "typesetting");
        }
    }
}