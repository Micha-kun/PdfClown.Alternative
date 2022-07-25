namespace org.pdfclown.samples.cli
{

    using System;
    using System.Drawing;
    using System.Linq;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents.composition;

    using org.pdfclown.documents.contents.fonts;
    using org.pdfclown.files;
    using org.pdfclown.tokens;

    /**
      <summary>This sample demonstrates the use of standard Type 1 fonts, which are the 14 built-in fonts
      prescribed by the PDF specification to be shipped along with any conformant PDF viewer.</summary>
      <remarks>In particular, this sample displays the complete glyphset of each standard font,
      iterating through character codes and glyph styles (regular, italic, bold).</remarks>
    */
    public class StandardFontSample
      : Sample
    {
        private static readonly int FontBaseSize = 20;
        private static readonly int Margin = 50;

        private void Populate(
          Document document
          )
        {
            var page = new Page(document);
            document.Pages.Add(page);
            var pageSize = page.Size;

            /*
              NOTE: Default fallback behavior on text encoding mismatch is substitution with default
              character; in this case, we want to force an exception to be thrown so we can explicitly
              handle the issue.
            */
            document.Configuration.EncodingFallback = EncodingFallbackEnum.Exception;

            var composer = new PrimitiveComposer(page);

            int x = Margin, y = Margin;
            var titleFont = new StandardType1Font(
              document,
              StandardType1Font.FamilyEnum.Times,
              true,
              true
              );
            StandardType1Font font = null;
            // Iterating through the standard Type 1 fonts...
            foreach (var fontFamily
              in (StandardType1Font.FamilyEnum[])Enum.GetValues(typeof(StandardType1Font.FamilyEnum)))
            {
                // Iterating through the font styles...
                for (var styleIndex = 0; styleIndex < 4; styleIndex++)
                {
                    /*
                      NOTE: Symbol and Zapf Dingbats are available just as regular fonts (no italic or bold variant).
                    */
                    if ((styleIndex > 0)
                      && ((fontFamily == StandardType1Font.FamilyEnum.Symbol)
                        || (fontFamily == StandardType1Font.FamilyEnum.ZapfDingbats)))
                    {
                        break;
                    }

                    var bold = (styleIndex & 1) > 0;
                    var italic = (styleIndex & 2) > 0;

                    // Define the font used to show its character set!
                    font = new StandardType1Font(document, fontFamily, bold, italic);

                    if (y > pageSize.Height - Margin)
                    {
                        composer.Flush();

                        page = new Page(document);
                        document.Pages.Add(page);
                        pageSize = page.Size;
                        composer = new PrimitiveComposer(page);
                        x = Margin;
                        y = Margin;
                    }

                    if (styleIndex == 0)
                    {
                        composer.DrawLine(
                          new PointF(x, y),
                          new PointF(pageSize.Width - Margin, y)
                          );
                        composer.Stroke();
                        y += 5;
                    }

                    composer.SetFont(
                      titleFont,
                      FontBaseSize * ((styleIndex == 0) ? 1.5f : 1)
                      );
                    _ = composer.ShowText(
                      $"{fontFamily}{(bold ? " bold" : string.Empty)}{(italic ? " italic" : string.Empty)}",
                      new PointF(x, y)
                      );

                    y += 40;
                    // Set the font used to show its character set!
                    composer.SetFont(font, FontBaseSize);
                    // Iterating through the font characters...
                    foreach (var charCode in font.CodePoints.OrderBy(codePoint => codePoint))
                    {
                        if (y > pageSize.Height - Margin)
                        {
                            composer.Flush();

                            page = new Page(document);
                            document.Pages.Add(page);
                            pageSize = page.Size;
                            composer = new PrimitiveComposer(page);
                            x = Margin;
                            y = Margin;

                            composer.SetFont(titleFont, FontBaseSize);
                            _ = composer.ShowText(
                              $"{fontFamily} (continued)",
                              new PointF(pageSize.Width - Margin, y),
                              XAlignmentEnum.Right,
                              YAlignmentEnum.Top,
                              0
                              );
                            composer.SetFont(font, FontBaseSize);
                            y += FontBaseSize * 2;
                        }

                        try
                        {
                            // Show the character!
                            _ = composer.ShowText(
                              new string((char)charCode, 1),
                              new PointF(x, y)
                              );
                            x += FontBaseSize;
                            if (x > pageSize.Width - Margin)
                            { x = Margin; y += 30; }
                        }
                        catch (EncodeException)
                        {
                            /*
                              NOOP -- NOTE: document.Configuration.EncodingFallback allows to customize the
                              behavior in case of missing character: we can alternatively catch an exception, have
                              the character substituted by a default one (typically '?' symbol) or have the
                              character silently excluded.
                            */
                        }
                    }

                    x = Margin;
                    y += Margin;
                }
            }
            composer.Flush();
        }

        public override void Run(
          )
        {
            // 1. PDF file instantiation.
            var file = new File();
            var document = file.Document;

            // 2. Content creation.
            this.Populate(document);

            // 3. Serialize the PDF file!
            _ = this.Serialize(file, "Standard Type 1 fonts", "applying standard Type 1 fonts", "Standard Type1 fonts");
        }
    }
}