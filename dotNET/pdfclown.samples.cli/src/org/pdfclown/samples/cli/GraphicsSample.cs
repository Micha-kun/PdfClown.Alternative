namespace org.pdfclown.samples.cli
{

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.files;
    using org.pdfclown.util.math.geom;

    using entities = org.pdfclown.documents.contents.entities;
    using fonts = org.pdfclown.documents.contents.fonts;

    /**
      <summary>This sample demonstrates some of the graphics operations available
      through the PrimitiveComposer and BlockComposer classes to compose a PDF document.</summary>
    */
    public class GraphicsSample
      : Sample
    {
        private static readonly DeviceRGBColor BackColor = new DeviceRGBColor(210 / 255d, 232 / 255d, 245 / 255d);
        private static readonly DeviceRGBColor SampleColor = DeviceRGBColor.Get(System.Drawing.Color.Red);

        private void BuildCurvesPage(
          Document document
          )
        {
            // 1. Add the page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            var pageSize = page.Size;

            // 2. Create a content composer for the page!
            var composer = new PrimitiveComposer(page);

            // 3. Drawing the page contents...
            composer.SetFont(
              new fonts::StandardType1Font(
                document,
                fonts::StandardType1Font.FamilyEnum.Courier,
                true,
                false
                ),
              32
              );
            var blockComposer = new BlockComposer(composer);
            blockComposer.Begin(new RectangleF(30, 0, pageSize.Width - 60, 50), XAlignmentEnum.Center, YAlignmentEnum.Middle);
            _ = blockComposer.ShowText("Curves");
            blockComposer.End();

            // 3.1. Arcs.
            {
                float y = 100;
                for (
                  var rowIndex = 0;
                  rowIndex < 4;
                  rowIndex++
                  )
                {
                    var angleStep = 45;
                    var startAngle = 0;
                    var endAngle = angleStep;
                    float x = 100;
                    float diameterX;
                    float diameterY;
                    switch (rowIndex)
                    {
                        case 0:
                        default:
                            diameterX = 40;
                            diameterY = 40;
                            break;
                        case 1:
                            diameterX = 40;
                            diameterY = 20;
                            break;
                        case 2:
                            diameterX = 20;
                            diameterY = 40;
                            break;
                        case 3:
                            diameterX = 40;
                            diameterY = 40;
                            break;
                    }
                    for (
                      int index = 0,
                        length = 360 / angleStep;
                      index < length;
                      index++
                      )
                    {
                        var arcFrame = new RectangleF(x, y, diameterX, diameterY);

                        // Drawing the arc frame...
                        _ = composer.BeginLocalState();
                        composer.SetLineWidth(0.25f);
                        composer.SetLineDash(new LineDash(new double[] { 5 }, 3));
                        composer.DrawRectangle(arcFrame);
                        composer.Stroke();
                        composer.End();

                        // Draw the arc!
                        composer.DrawArc(arcFrame, startAngle, endAngle);
                        composer.Stroke();

                        endAngle += angleStep;
                        switch (rowIndex)
                        {
                            case 3:
                                startAngle += angleStep;
                                break;
                        }

                        x += 50;
                    }

                    y += diameterY + 10;
                }
            }

            // 3.2. Circle.
            {
                var arcFrame = new RectangleF(100, 300, 100, 100);

                // Drawing the circle frame...
                _ = composer.BeginLocalState();
                composer.SetLineWidth(0.25f);
                composer.SetLineDash(new LineDash(new double[] { 5 }, 3));
                composer.DrawRectangle(arcFrame);
                composer.Stroke();
                composer.End();

                // Drawing the circle...
                composer.SetFillColor(DeviceRGBColor.Get(System.Drawing.Color.Red));
                composer.DrawEllipse(arcFrame);
                composer.FillStroke();
            }

            // 3.3. Horizontal ellipse.
            {
                var arcFrame = new RectangleF(210, 300, 100, 50);

                // Drawing the ellipse frame...
                _ = composer.BeginLocalState();
                composer.SetLineWidth(0.25f);
                composer.SetLineDash(new LineDash(new double[] { 5 }, 3));
                composer.DrawRectangle(arcFrame);
                composer.Stroke();
                composer.End();

                // Drawing the ellipse...
                composer.SetFillColor(DeviceRGBColor.Get(System.Drawing.Color.Green));
                composer.DrawEllipse(arcFrame);
                composer.FillStroke();
            }

            // 3.4. Vertical ellipse.
            {
                var arcFrame = new RectangleF(320, 300, 50, 100);

                // Drawing the ellipse frame...
                _ = composer.BeginLocalState();
                composer.SetLineWidth(0.25f);
                composer.SetLineDash(new LineDash(new double[] { 5 }, 3));
                composer.DrawRectangle(arcFrame);
                composer.Stroke();
                composer.End();

                // Drawing the ellipse...
                composer.SetFillColor(DeviceRGBColor.Get(System.Drawing.Color.Blue));
                composer.DrawEllipse(arcFrame);
                composer.FillStroke();
            }

            // 3.5. Spirals.
            {
                float y = 500;
                float spiralWidth = 100;
                composer.SetLineWidth(.5f);
                for (
                  var rowIndex = 0;
                  rowIndex < 3;
                  rowIndex++
                  )
                {
                    float x = 150;
                    var branchWidth = .5f;
                    float branchRatio = 1;
                    for (
                      var spiralIndex = 0;
                      spiralIndex < 4;
                      spiralIndex++
                      )
                    {
                        float spiralTurnsCount;
                        switch (rowIndex)
                        {
                            case 0:
                            default:
                                spiralTurnsCount = spiralWidth / (branchWidth * 8);
                                break;
                            case 1:
                                spiralTurnsCount = (float)(spiralWidth / (branchWidth * 8 * ((spiralIndex * 1.15) + 1)));
                                break;
                        }
                        switch (rowIndex)
                        {
                            case 2:
                                composer.SetLineDash(new LineDash(new double[] { 10, 5 }));
                                composer.SetLineCap(LineCapEnum.Round);
                                break;
                            default:
                                break;
                        }

                        composer.DrawSpiral(
                          new PointF(x, y),
                          0,
                          360 * spiralTurnsCount,
                          branchWidth,
                          branchRatio
                          );
                        composer.Stroke();

                        x += spiralWidth + 10;

                        switch (rowIndex)
                        {
                            case 0:
                            default:
                                branchWidth += 1;
                                break;
                            case 1:
                                branchRatio += .035f;
                                break;
                        }
                        switch (rowIndex)
                        {
                            case 2:
                                composer.SetLineWidth(composer.State.LineWidth + .5f);
                                break;
                        }
                    }

                    y += spiralWidth + 10;
                }
            }

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        private void BuildMiscellaneousPage(
          Document document
          )
        {
            // 1. Add the page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            var pageSize = page.Size;

            // 2. Create a content composer for the page!
            var composer = new PrimitiveComposer(page);

            // 3. Drawing the page contents...
            composer.SetFont(
              new fonts::StandardType1Font(
                document,
                fonts::StandardType1Font.FamilyEnum.Courier,
                true,
                false
                ),
              32
              );
            var blockComposer = new BlockComposer(composer);
            blockComposer.Begin(new RectangleF(30, 0, pageSize.Width - 60, 50), XAlignmentEnum.Center, YAlignmentEnum.Middle);
            _ = blockComposer.ShowText("Miscellaneous");
            blockComposer.End();

            _ = composer.BeginLocalState();
            composer.SetLineJoin(LineJoinEnum.Round);
            composer.SetLineCap(LineCapEnum.Round);

            // 3.1. Polygon.
            composer.DrawPolygon(
              new PointF[]
              {
          new PointF(100,200),
          new PointF(150,150),
          new PointF(200,150),
          new PointF(250,200)
              }
              );

            // 3.2. Polyline.
            composer.DrawPolyline(
              new PointF[]
              {
          new PointF(300,200),
          new PointF(350,150),
          new PointF(400,150),
          new PointF(450,200)
              }
              );

            composer.Stroke();

            // 3.3. Rectangle (both squared and rounded).
            var x = 50;
            var radius = 0;
            while (x < 500)
            {
                if (x > 300)
                {
                    composer.SetLineDash(new LineDash(new double[] { 5 }, 3));
                }

                composer.SetFillColor(new DeviceRGBColor(1, x / 500d, x / 500d));
                composer.DrawRectangle(
                  new RectangleF(x, 250, 150, 100),
                  radius // NOTE: radius parameter determines the rounded angle size.
                  );
                composer.FillStroke();

                x += 175;
                radius += 10;
            }
            composer.End(); // End local state.

            _ = composer.BeginLocalState();
            composer.SetFont(
              composer.State.Font,
              12
              );

            // 3.4. Line cap parameter.
            var y = 400;
            foreach (var lineCap
              in (LineCapEnum[])Enum.GetValues(typeof(LineCapEnum)))
            {
                _ = composer.ShowText(
                  $"{lineCap}:",
                  new PointF(50, y),
                  XAlignmentEnum.Left,
                  YAlignmentEnum.Middle,
                  0
                  );
                composer.SetLineWidth(12);
                composer.SetLineCap(lineCap);
                composer.DrawLine(
                  new PointF(120, y),
                  new PointF(220, y)
                  );
                composer.Stroke();

                _ = composer.BeginLocalState();
                composer.SetLineWidth(1);
                composer.SetStrokeColor(DeviceRGBColor.White);
                composer.SetLineCap(LineCapEnum.Butt);
                composer.DrawLine(
                  new PointF(120, y),
                  new PointF(220, y)
                  );
                composer.Stroke();
                composer.End(); // End local state.

                y += 30;
            }

            // 3.5. Line join parameter.
            y += 50;
            foreach (var lineJoin
              in (LineJoinEnum[])Enum.GetValues(typeof(LineJoinEnum)))
            {
                _ = composer.ShowText(
                  $"{lineJoin}:",
                  new PointF(50, y),
                  XAlignmentEnum.Left,
                  YAlignmentEnum.Middle,
                  0
                  );
                composer.SetLineWidth(12);
                composer.SetLineJoin(lineJoin);
                var points = new PointF[]
                  {
            new PointF(120,y+25),
            new PointF(150,y-25),
            new PointF(180,y+25)
                  };
                composer.DrawPolyline(points);
                composer.Stroke();

                _ = composer.BeginLocalState();
                composer.SetLineWidth(1);
                composer.SetStrokeColor(DeviceRGBColor.White);
                composer.SetLineCap(LineCapEnum.Butt);
                composer.DrawPolyline(points);
                composer.Stroke();
                composer.End(); // End local state.

                y += 50;
            }
            composer.End(); // End local state.

            // 3.6. Clipping.
            /*
              NOTE: Clipping should be conveniently enclosed within a local state
              in order to easily resume the unaltered drawing area after the operation completes.
            */
            _ = composer.BeginLocalState();
            composer.DrawPolygon(
              new PointF[]
              {
          new PointF(220,410),
          new PointF(300,490),
          new PointF(450,360),
          new PointF(430,520),
          new PointF(590,565),
          new PointF(420,595),
          new PointF(460,730),
          new PointF(380,650),
          new PointF(330,765),
          new PointF(310,640),
          new PointF(220,710),
          new PointF(275,570),
          new PointF(170,500),
          new PointF(275,510)
              }
              );
            composer.Clip();
            // Showing a clown image...
            // Instantiate a jpeg image object!
            var image = entities::Image.Get(this.GetResourcePath($"images{System.IO.Path.DirectorySeparatorChar}Clown.jpg")); // Abstract image (entity).
            var imageXObject = image.ToXObject(document);
            // Show the image!
            composer.ShowXObject(
              imageXObject,
              new PointF(170, 320),
              GeomUtils.Scale(imageXObject.Size, new SizeF(450, 0))
              );
            composer.End(); // End local state.

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        private void BuildSimpleTextPage(
          Document document
          )
        {
            // 1. Add the page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            var pageSize = page.Size;

            // 2. Create a content composer for the page!
            var composer = new PrimitiveComposer(page);
            // 3. Inserting contents...
            // Set the font to use!
            composer.SetFont(
              new fonts::StandardType1Font(
                document,
                fonts::StandardType1Font.FamilyEnum.Courier,
                true,
                false
                ),
              32
              );

            var xAlignments = (XAlignmentEnum[])Enum.GetValues(typeof(XAlignmentEnum));
            var yAlignments = (YAlignmentEnum[])Enum.GetValues(typeof(YAlignmentEnum));
            var step = ((int)pageSize.Height) / (((xAlignments.Length - 1) * yAlignments.Length) + 1);

            var blockComposer = new BlockComposer(composer);
            var frame = new RectangleF(
              30,
              0,
              pageSize.Width - 60,
              step / 2
              );
            blockComposer.Begin(frame, XAlignmentEnum.Center, YAlignmentEnum.Middle);
            _ = blockComposer.ShowText("Simple alignment");
            blockComposer.End();

            frame = new RectangleF(
              30,
              pageSize.Height - (step / 2),
              pageSize.Width - 60,
              (step / 2) - 10
              );
            blockComposer.Begin(frame, XAlignmentEnum.Left, YAlignmentEnum.Bottom);
            composer.SetFont(composer.State.Font, 10);
            _ = blockComposer.ShowText(
              "NOTE: showText(...) methods return the actual bounding box of the text shown.\n"
                + "NOTE: The rotation parameter can be freely defined as a floating point value."
              );
            blockComposer.End();

            composer.SetFont(composer.State.Font, 12);
            var x = 30;
            var y = step;
            var alignmentIndex = 0;
            foreach (var xAlignment
              in (XAlignmentEnum[])Enum.GetValues(typeof(XAlignmentEnum)))
            {
                /*
                  NOTE: As text shown through PrimitiveComposer has no bounding box constraining its extension,
                  applying the justified alignment has no effect (it degrades to center alignment);
                  in order to get such an effect, use BlockComposer instead.
                */
                if (xAlignment.Equals(XAlignmentEnum.Justify))
                {
                    continue;
                }

                foreach (var yAlignment
                  in (YAlignmentEnum[])Enum.GetValues(typeof(YAlignmentEnum)))
                {
                    if (alignmentIndex % 2 == 0)
                    {
                        _ = composer.BeginLocalState();
                        composer.SetFillColor(BackColor);
                        composer.DrawRectangle(
                          new RectangleF(
                            0,
                            y - (step / 2),
                            pageSize.Width,
                            step
                            )
                          );
                        composer.Fill();
                        composer.End();
                    }

                    _ = composer.ShowText(
                      $"{xAlignment} {yAlignment}:",
                      new PointF(x, y),
                      XAlignmentEnum.Left,
                      YAlignmentEnum.Middle,
                      0
                      );

                    y += step;
                    alignmentIndex++;
                }
            }

            float rotationStep = 0;
            for (
              var columnIndex = 0;
              columnIndex < 2;
              columnIndex++
              )
            {
                switch (columnIndex)
                {
                    case 0:
                        x = 200;
                        rotationStep = 0;
                        break;
                    case 1:
                        x = (((int)pageSize.Width) / 2) + 100;
                        rotationStep = 360 / (((xAlignments.Length - 1) * yAlignments.Length) - 1);
                        break;
                }
                y = step;
                float rotation = 0;
                foreach (var xAlignment
                  in (XAlignmentEnum[])Enum.GetValues(typeof(XAlignmentEnum)))
                {
                    /*
                      NOTE: As text shown through PrimitiveComposer has no bounding box constraining its extension,
                      applying the justified alignment has no effect (it degrades to center alignment);
                      in order to get such an effect, use BlockComposer instead.
                    */
                    if (xAlignment.Equals(XAlignmentEnum.Justify))
                    {
                        continue;
                    }

                    foreach (var yAlignment
                      in (YAlignmentEnum[])Enum.GetValues(typeof(YAlignmentEnum)))
                    {
                        float startArcAngle = 0;
                        switch (xAlignment)
                        {
                            case XAlignmentEnum.Left:
                                // OK -- NOOP.
                                break;
                            case XAlignmentEnum.Right:
                            case XAlignmentEnum.Center:
                                startArcAngle = 180;
                                break;
                        }

                        composer.DrawArc(
                          new RectangleF(
                            x - 10,
                            y - 10,
                            20,
                            20
                            ),
                          startArcAngle,
                          startArcAngle + rotation
                          );

                        this.DrawText(
                          composer,
                          "PDF Clown",
                          new PointF(x, y),
                          xAlignment,
                          yAlignment,
                          rotation
                          );
                        y += step;
                        rotation += rotationStep;
                    }
                }
            }

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        private void BuildTextBlockPage(
          Document document
          )
        {
            // 1. Add the page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            var pageSize = page.Size;

            // 2. Create a content composer for the page!
            var composer = new PrimitiveComposer(page);

            // 3. Drawing the page contents...
            fonts::Font mainFont = new fonts::StandardType1Font(document, fonts::StandardType1Font.FamilyEnum.Courier, true, false);
            int step;
            var xAlignments = (XAlignmentEnum[])Enum.GetValues(typeof(XAlignmentEnum));
            var yAlignments = (YAlignmentEnum[])Enum.GetValues(typeof(YAlignmentEnum));
            step = ((int)pageSize.Height) / ((xAlignments.Length * yAlignments.Length) + 1);
            var blockComposer = new BlockComposer(composer);
            blockComposer.Begin(
              new RectangleF(
                30,
                0,
                pageSize.Width - 60,
                step * .8f
                ),
              XAlignmentEnum.Center,
              YAlignmentEnum.Middle
              );
            composer.SetFont(mainFont, 32);
            _ = blockComposer.ShowText("Block alignment");
            blockComposer.End();

            // Drawing the text blocks...
            fonts::Font sampleFont = new fonts::StandardType1Font(document, fonts::StandardType1Font.FamilyEnum.Times, false, false);
            var x = 30;
            var y = (int)(step * 1.2);
            foreach (var xAlignment in (XAlignmentEnum[])Enum.GetValues(typeof(XAlignmentEnum)))
            {
                foreach (var yAlignment in (YAlignmentEnum[])Enum.GetValues(typeof(YAlignmentEnum)))
                {
                    composer.SetFont(mainFont, 12);
                    _ = composer.ShowText(
                      $"{xAlignment} {yAlignment}:",
                      new PointF(x, y),
                      XAlignmentEnum.Left,
                      YAlignmentEnum.Middle,
                      0
                      );

                    composer.SetFont(sampleFont, 12);
                    for (var index = 0; index < 2; index++)
                    {
                        int frameX;
                        switch (index)
                        {
                            case 0:
                                frameX = 150;
                                blockComposer.Hyphenation = false;
                                break;
                            case 1:
                                frameX = 360;
                                blockComposer.Hyphenation = true;
                                break;
                            default:
                                throw new Exception();
                        }

                        var frame = new RectangleF(
                          frameX,
                          y - (step * .4f),
                          200,
                          step * .8f
                          );
                        blockComposer.Begin(frame, xAlignment, yAlignment);
                        _ = blockComposer.ShowText(
                          "Demonstrating how to constrain text inside a page area. See the other code samples for more usage tips."
                          );
                        blockComposer.End();

                        _ = composer.BeginLocalState();
                        composer.SetLineWidth(0.2f);
                        composer.SetLineDash(new LineDash(new double[] { 5 }, 5));
                        composer.DrawRectangle(frame);
                        composer.Stroke();
                        composer.End();
                    }

                    y += step;
                }
            }

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        private void BuildTextBlockPage2(
          Document document
          )
        {
            // 1. Add the page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            var pageSize = page.Size;

            // 2. Create a content composer for the page!
            var composer = new PrimitiveComposer(page);

            // 3. Drawing the page contents...
            fonts::Font mainFont = new fonts::StandardType1Font(document, fonts::StandardType1Font.FamilyEnum.Courier, true, false);
            var stepCount = 5;
            var step = ((int)pageSize.Height) / (stepCount + 1);
            var blockComposer = new BlockComposer(composer);
            blockComposer.Begin(
              new RectangleF(
                30,
                0,
                pageSize.Width - 60,
                step * .8f
                ),
              XAlignmentEnum.Center,
              YAlignmentEnum.Middle
              );
            composer.SetFont(mainFont, 32);
            _ = blockComposer.ShowText("Block line alignment");
            blockComposer.End();

            // Drawing the text block...

            fonts::Font sampleFont = new fonts::StandardType1Font(document, fonts::StandardType1Font.FamilyEnum.Times, false, false);
            var sampleImage = entities::Image.Get(this.GetResourcePath($"images{System.IO.Path.DirectorySeparatorChar}gnu.jpg"));
            var sampleImageXObject = sampleImage.ToXObject(document);

            IList<LineAlignmentEnum> lineAlignments = new List<LineAlignmentEnum>((LineAlignmentEnum[])Enum.GetValues(typeof(LineAlignmentEnum)));
            var frameHeight = (pageSize.Height - 130 - (5 * lineAlignments.Count * 2)) / (lineAlignments.Count * 2);
            var frameWidth = (pageSize.Width - 60 - (5 * lineAlignments.Count)) / lineAlignments.Count;
            var imageSize = 7;
            for (int index = 0, length = lineAlignments.Count; index < length; index++)
            {
                var lineAlignment = lineAlignments[index];

                for (int imageIndex = 0, imageLength = lineAlignments.Count; imageIndex < imageLength; imageIndex++)
                {
                    var imageAlignment = lineAlignments[imageIndex];

                    for (int index2 = 0, length2 = 2; index2 < length2; index2++)
                    {
                        var frame = new RectangleF(
                          30 + ((frameWidth + 5) * imageIndex),
                          100 + ((frameHeight + 5) * ((index * 2) + index2)),
                          frameWidth,
                          frameHeight
                          );

                        blockComposer.Begin(frame, XAlignmentEnum.Left, YAlignmentEnum.Top);
                        composer.SetFont(mainFont, 3);
                        _ = blockComposer.ShowText($"Text: {lineAlignment}");
                        _ = blockComposer.ShowBreak();
                        _ = blockComposer.ShowText($"Image: {imageAlignment}");
                        blockComposer.End();

                        blockComposer.Begin(frame, XAlignmentEnum.Left, YAlignmentEnum.Middle);
                        composer.SetFont(sampleFont, 3);
                        _ = blockComposer.ShowText("Previous row boundary.");
                        _ = blockComposer.ShowBreak();
                        composer.SetFont(sampleFont, (index2 == 0) ? 3 : 6);
                        _ = blockComposer.ShowText("Alignment:");
                        composer.SetFont(sampleFont, (index2 == 0) ? 6 : 3);
                        _ = blockComposer.ShowText($" aligned to {lineAlignment} ", lineAlignment);
                        _ = blockComposer.ShowXObject(sampleImageXObject, new SizeF(imageSize, imageSize), imageAlignment);
                        _ = blockComposer.ShowBreak();
                        composer.SetFont(sampleFont, 3);
                        _ = blockComposer.ShowText("Next row boundary.");
                        blockComposer.End();

                        _ = composer.BeginLocalState();
                        composer.SetLineWidth(0.1f);
                        composer.SetLineDash(new LineDash(new double[] { 1, 4 }, 4));
                        composer.DrawRectangle(blockComposer.Frame);
                        composer.Stroke();
                        composer.End();

                        _ = composer.BeginLocalState();
                        composer.SetLineWidth(0.1f);
                        composer.SetLineDash(new LineDash(new double[] { 1 }, 1));
                        composer.DrawRectangle(blockComposer.BoundBox);
                        composer.Stroke();
                        composer.End();
                    }
                }
            }

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        private void BuildTextBlockPage3(
          Document document
          )
        {
            // 1. Add the page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            var pageSize = page.Size;

            // 2. Create a content composer for the page!
            var composer = new PrimitiveComposer(page);

            // 3. Drawing the page contents...
            fonts::Font mainFont = new fonts::StandardType1Font(document, fonts::StandardType1Font.FamilyEnum.Courier, true, false);
            var stepCount = 5;
            var step = ((int)pageSize.Height) / (stepCount + 1);

            // 3.1. Drawing the page title...
            var blockComposer = new BlockComposer(composer);
            blockComposer.Begin(
              new RectangleF(
                30,
                0,
                pageSize.Width - 60,
                step * .8f
                ),
              XAlignmentEnum.Center,
              YAlignmentEnum.Middle
              );
            composer.SetFont(mainFont, 32);
            _ = blockComposer.ShowText("Block line space");
            blockComposer.End();

            // 3.2. Drawing the text blocks...
            fonts::Font sampleFont = new fonts::StandardType1Font(document, fonts::StandardType1Font.FamilyEnum.Times, false, false);
            var x = 30;
            var y = (int)(step * 1.1);
            blockComposer.LineSpace.UnitMode = Length.UnitModeEnum.Relative;
            for (var index = 0; index < stepCount; index++)
            {
                var relativeLineSpace = 0.5f * index;
                blockComposer.LineSpace.Value = relativeLineSpace;

                composer.SetFont(mainFont, 12);
                _ = composer.ShowText(
                  $"{relativeLineSpace}:",
                  new PointF(x, y),
                  XAlignmentEnum.Left,
                  YAlignmentEnum.Middle,
                  0
                  );

                composer.SetFont(sampleFont, 10);
                var frame = new RectangleF(150, y - (step * .4f), 350, step * .9f);
                blockComposer.Begin(frame, XAlignmentEnum.Left, YAlignmentEnum.Top);
                _ = blockComposer.ShowText("Demonstrating how to set the block line space. Line space can be expressed either as an absolute value (in user-space units) or as a relative one (floating-point ratio); in the latter case the base value is represented by the current font's line height (so that, for example, 2 means \"a line space that's twice as the line height\").");
                blockComposer.End();

                _ = composer.BeginLocalState();
                composer.SetLineWidth(0.2);
                composer.SetLineDash(new LineDash(new double[] { 5 }, 5));
                composer.DrawRectangle(frame);
                composer.Stroke();
                composer.End();

                y += step;
            }

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        private void BuildTextBlockPage4(
          Document document
          )
        {
            // 1. Add the page to the document!
            var page = new Page(document); // Instantiates the page inside the document context.
            document.Pages.Add(page); // Puts the page in the pages collection.

            var pageSize = page.Size;

            // 2. Create a content composer for the page!
            var composer = new PrimitiveComposer(page);

            // 3. Drawing the page contents...
            fonts::Font mainFont = new fonts::StandardType1Font(document, fonts::StandardType1Font.FamilyEnum.Courier, true, false);
            var stepCount = 5;
            var step = ((int)pageSize.Height) / (stepCount + 1);
            var blockComposer = new BlockComposer(composer);
            blockComposer.Begin(
              new RectangleF(
                30,
                0,
                pageSize.Width - 60,
                step * .8f
                ),
              XAlignmentEnum.Center,
              YAlignmentEnum.Middle
              );
            composer.SetFont(mainFont, 32);
            _ = blockComposer.ShowText("Unspaced block");
            blockComposer.End();

            // Drawing the text block...

            fonts::Font sampleFont = new fonts::StandardType1Font(document, fonts::StandardType1Font.FamilyEnum.Times, false, false);
            composer.SetFont(sampleFont, 15);

            float topMargin = 100;
            float boxMargin = 30;
            var boxWidth = pageSize.Width - (boxMargin * 2);
            var boxHeight = (pageSize.Height - topMargin - boxMargin - boxMargin) / 2;
            {
                var frame = new RectangleF(
                  boxMargin,
                  topMargin,
                  boxWidth,
                  boxHeight
                  );
                blockComposer.Begin(frame, XAlignmentEnum.Left, YAlignmentEnum.Top);
                // Add text until the frame area is completely filled!
                while (blockComposer.ShowText("DemonstratingHowUnspacedTextIsManagedInCaseOfInsertionInADelimitedPageAreaThroughBlockComposerClass.") > 0)
                {
                    ;
                }

                blockComposer.End();

                _ = composer.BeginLocalState();
                composer.SetLineWidth(0.2);
                composer.SetLineDash(new LineDash(new double[] { 5 }, 5));
                composer.DrawRectangle(frame);
                composer.Stroke();
                composer.End();
            }
            {
                var frame = new RectangleF(
                  boxMargin,
                  topMargin + boxHeight + boxMargin,
                  boxWidth,
                  boxHeight
                  );
                blockComposer.Begin(frame, XAlignmentEnum.Left, YAlignmentEnum.Top);
                // Add text until the frame area is completely filled!
                while (blockComposer.ShowText(" DemonstratingHowUnspacedTextWithLeadingSpaceIsManagedInCaseOfInsertionInADelimitedPageAreaThroughBlockComposerClass.") > 0)
                {
                    ;
                }

                blockComposer.End();

                _ = composer.BeginLocalState();
                composer.SetLineWidth(0.2);
                composer.SetLineDash(new LineDash(new double[] { 5 }, 5));
                composer.DrawRectangle(frame);
                composer.Stroke();
                composer.End();
            }

            // 4. Flush the contents into the page!
            composer.Flush();
        }

        private void DrawCross(
          PrimitiveComposer composer,
          PointF center
          )
        {
            composer.DrawLine(
              new PointF(center.X - 10, center.Y),
              new PointF(center.X + 10, center.Y)
              );
            composer.DrawLine(
              new PointF(center.X, center.Y - 10),
              new PointF(center.X, center.Y + 10)
              );
            composer.Stroke();
        }

        private void DrawFrame(
          PrimitiveComposer composer,
          PointF[] frameVertices
          )
        {
            _ = composer.BeginLocalState();
            composer.SetLineWidth(0.2f);
            composer.SetLineDash(new LineDash(new double[] { 5 }, 5));
            composer.DrawPolygon(frameVertices);
            composer.Stroke();
            composer.End();
        }

        private void DrawText(
          PrimitiveComposer composer,
          string value,
          PointF location,
          XAlignmentEnum xAlignment,
          YAlignmentEnum yAlignment,
          float rotation
          )
        {
            // Show the anchor point!
            this.DrawCross(composer, location);

            _ = composer.BeginLocalState();
            composer.SetFillColor(SampleColor);
            // Show the text onto the page!
            var textFrame = composer.ShowText(
              value,
              location,
              xAlignment,
              yAlignment,
              rotation
              );
            composer.End();

            // Draw the frame binding the shown text!
            this.DrawFrame(
              composer,
              textFrame.Points
              );

            _ = composer.BeginLocalState();
            composer.SetFont(composer.State.Font, 8);
            // Draw the rotation degrees!
            _ = composer.ShowText(
              $"({(int)rotation} degrees)",
              new PointF(
                location.X + 70,
                location.Y
                ),
              XAlignmentEnum.Left,
              YAlignmentEnum.Middle,
              0
              );
            composer.End();
        }

        public override void Run(
          )
        {
            // 1. Instantiate a new PDF file!
            var file = new File();
            var document = file.Document;

            // 2. Insert the contents into the document!
            this.BuildCurvesPage(document);
            this.BuildMiscellaneousPage(document);
            this.BuildSimpleTextPage(document);
            this.BuildTextBlockPage(document);
            this.BuildTextBlockPage2(document);
            this.BuildTextBlockPage3(document);
            this.BuildTextBlockPage4(document);

            // 3. Serialize the PDF file!
            _ = this.Serialize(file, "Composition elements", "applying the composition elements", "graphics, line styles, text alignment, shapes, circles, ellipses, spirals, polygons, rounded rectangles, images, clipping");
        }
    }
}