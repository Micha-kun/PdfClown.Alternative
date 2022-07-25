namespace org.pdfclown.samples.cli
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents;

    using org.pdfclown.documents.interaction.annotations;
    using org.pdfclown.files;
    using org.pdfclown.tools;
    using org.pdfclown.util.math;
    using org.pdfclown.util.math.geom;

    /**
      <summary>This sample demonstrates how to highlight text matching arbitrary patterns.</summary>
      <remarks>Highlighting is defined through text markup annotations.</remarks>
    */
    public class TextHighlightSample
      : Sample
    {

        public override void Run(
          )
        {
            // 1. Opening the PDF file...
            var filePath = this.PromptFileChoice("Please select a PDF file");
            using (var file = new File(filePath))
            {
                // Define the text pattern to look for!
                var textRegEx = this.PromptChoice("Please enter the pattern to look for: ");
                var pattern = new Regex(textRegEx, RegexOptions.IgnoreCase);

                // 2. Iterating through the document pages...
                var textExtractor = new TextExtractor(true, true);
                foreach (var page in file.Document.Pages)
                {
                    Console.WriteLine($"\nScanning page {page.Number}...\n");

                    // 2.1. Extract the page text!
                    var textStrings = textExtractor.Extract(page);

                    // 2.2. Find the text pattern matches!
                    var matches = pattern.Matches(TextExtractor.ToString(textStrings));

                    // 2.3. Highlight the text pattern matches!
                    textExtractor.Filter(
                      textStrings,
                      new TextHighlighter(page, matches)
                      );
                }

                // 3. Highlighted file serialization.
                _ = this.Serialize(file);
            }
        }

        private class TextHighlighter
          : TextExtractor.IIntervalFilter
        {
            private readonly IEnumerator matchEnumerator;
            private readonly Page page;

            public TextHighlighter(
              Page page,
              MatchCollection matches
              )
            {
                this.page = page;
                this.matchEnumerator = matches.GetEnumerator();
            }

            object IEnumerator.Current => this.Current;

            public void Dispose(
              )
            {/* NOOP */}

            public bool MoveNext(
              )
            { return this.matchEnumerator.MoveNext(); }

            public void Process(
              Interval<int> interval,
              ITextString match
              )
            {
                // Defining the highlight box of the text pattern match...
                IList<Quad> highlightQuads = new List<Quad>();
                /*
                  NOTE: A text pattern match may be split across multiple contiguous lines,
                  so we have to define a distinct highlight box for each text chunk.
                */
                RectangleF? textBox = null;
                foreach (var textChar in match.TextChars)
                {
                    var textCharBox = textChar.Box;
                    if (!textBox.HasValue)
                    { textBox = textCharBox; }
                    else
                    {
                        if (textCharBox.Y > textBox.Value.Bottom)
                        {
                            highlightQuads.Add(Quad.Get(textBox.Value));
                            textBox = textCharBox;
                        }
                        else
                        { textBox = RectangleF.Union(textBox.Value, textCharBox); }
                    }
                }
                highlightQuads.Add(Quad.Get(textBox.Value));
                // Highlight the text pattern match!
                _ = new TextMarkup(this.page, highlightQuads, null, TextMarkup.MarkupTypeEnum.Highlight);
            }

            public void Reset(
              )
            { throw new NotSupportedException(); }

            public Interval<int> Current
            {
                get
                {
                    var current = (Match)this.matchEnumerator.Current;
                    return new Interval<int>(current.Index, current.Index + current.Length);
                }
            }
        }
    }
}