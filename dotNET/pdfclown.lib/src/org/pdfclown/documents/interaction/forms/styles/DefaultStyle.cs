/*
  Copyright 2008-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

namespace org.pdfclown.documents.interaction.forms.styles
{
    using System.Drawing;
    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.fonts;
    using org.pdfclown.documents.contents.xObjects;
    using org.pdfclown.objects;

    /**
      <summary>Default field appearance style.</summary>
    */
    public sealed class DefaultStyle
      : FieldStyle
    {
        public DefaultStyle(
)
        { this.BackColor = new DeviceRGBColor(.9, .9, .9); }

        private void Apply(
          CheckBox field
          )
        {
            var document = field.Document;
            foreach (var widget in field.Widgets)
            {
                var widgetDataObject = widget.BaseDataObject;
                widgetDataObject[PdfName.DA] = new PdfString("/ZaDb 0 Tf 0 0 0 rg");
                widgetDataObject[PdfName.MK] = new PdfDictionary(
                  new PdfName[]
                  {
              PdfName.BG,
              PdfName.BC,
              PdfName.CA
                  },
                  new PdfDirectObject[]
                  {
              new PdfArray(new PdfDirectObject[]{PdfReal.Get(0.9412), PdfReal.Get(0.9412), PdfReal.Get(0.9412)}),
              new PdfArray(new PdfDirectObject[]{PdfInteger.Default, PdfInteger.Default, PdfInteger.Default}),
              new PdfString("4")
                  }
                  );
                widgetDataObject[PdfName.BS] = new PdfDictionary(
                  new PdfName[]
                  {
              PdfName.W,
              PdfName.S
                  },
                  new PdfDirectObject[]
                  {
              PdfReal.Get(0.8),
              PdfName.S
                  }
                  );
                widgetDataObject[PdfName.H] = PdfName.P;

                var appearance = widget.Appearance;
                var normalAppearance = appearance.Normal;
                var size = widget.Box.Size;
                var onState = new FormXObject(document, size);
                normalAppearance[PdfName.Yes] = onState;

                //TODO:verify!!!
                //   appearance.getRollover().put(PdfName.Yes,onState);
                //   appearance.getDown().put(PdfName.Yes,onState);
                //   appearance.getRollover().put(PdfName.Off,offState);
                //   appearance.getDown().put(PdfName.Off,offState);

                float lineWidth = 1;
                var frame = new RectangleF(lineWidth / 2, lineWidth / 2, size.Width - lineWidth, size.Height - lineWidth);
                {
                    var composer = new PrimitiveComposer(onState);

                    if (this.GraphicsVisibile)
                    {
                        _ = composer.BeginLocalState();
                        composer.SetLineWidth(lineWidth);
                        composer.SetFillColor(this.BackColor);
                        composer.SetStrokeColor(this.ForeColor);
                        composer.DrawRectangle(frame, 5);
                        composer.FillStroke();
                        composer.End();
                    }

                    var blockComposer = new BlockComposer(composer);
                    blockComposer.Begin(frame, XAlignmentEnum.Center, YAlignmentEnum.Middle);
                    composer.SetFillColor(this.ForeColor);
                    composer.SetFont(
                      new StandardType1Font(
                        document,
                        StandardType1Font.FamilyEnum.ZapfDingbats,
                        true,
                        false
                        ),
                      size.Height * 0.8
                      );
                    _ = blockComposer.ShowText(new string(new char[] { this.CheckSymbol }));
                    blockComposer.End();

                    composer.Flush();
                }

                var offState = new FormXObject(document, size);
                normalAppearance[PdfName.Off] = offState;
                if (this.GraphicsVisibile)
                {
                    var composer = new PrimitiveComposer(offState);

                    _ = composer.BeginLocalState();
                    composer.SetLineWidth(lineWidth);
                    composer.SetFillColor(this.BackColor);
                    composer.SetStrokeColor(this.ForeColor);
                    composer.DrawRectangle(frame, 5);
                    composer.FillStroke();
                    composer.End();

                    composer.Flush();
                }
            }
        }

        private void Apply(
          RadioButton field
          )
        {
            var document = field.Document;
            foreach (var widget in field.Widgets)
            {
                var widgetDataObject = widget.BaseDataObject;
                widgetDataObject[PdfName.DA] = new PdfString("/ZaDb 0 Tf 0 0 0 rg");
                widgetDataObject[PdfName.MK] = new PdfDictionary(
                  new PdfName[]
                  {
              PdfName.BG,
              PdfName.BC,
              PdfName.CA
                  },
                  new PdfDirectObject[]
                  {
              new PdfArray(new PdfDirectObject[]{PdfReal.Get(0.9412), PdfReal.Get(0.9412), PdfReal.Get(0.9412)}),
              new PdfArray(new PdfDirectObject[]{PdfInteger.Default, PdfInteger.Default, PdfInteger.Default}),
              new PdfString("l")
                  }
                  );
                widgetDataObject[PdfName.BS] = new PdfDictionary(
                  new PdfName[]
                  {
              PdfName.W,
              PdfName.S
                  },
                  new PdfDirectObject[]
                  {
              PdfReal.Get(0.8),
              PdfName.S
                  }
                  );
                widgetDataObject[PdfName.H] = PdfName.P;

                var appearance = widget.Appearance;
                var normalAppearance = appearance.Normal;
                var onState = normalAppearance[new PdfName(widget.Value)];

                //TODO:verify!!!
                //   appearance.getRollover().put(new PdfName(...),onState);
                //   appearance.getDown().put(new PdfName(...),onState);
                //   appearance.getRollover().put(PdfName.Off,offState);
                //   appearance.getDown().put(PdfName.Off,offState);

                var size = widget.Box.Size;
                float lineWidth = 1;
                var frame = new RectangleF(lineWidth / 2, lineWidth / 2, size.Width - lineWidth, size.Height - lineWidth);
                {
                    var composer = new PrimitiveComposer(onState);

                    if (this.GraphicsVisibile)
                    {
                        _ = composer.BeginLocalState();
                        composer.SetLineWidth(lineWidth);
                        composer.SetFillColor(this.BackColor);
                        composer.SetStrokeColor(this.ForeColor);
                        composer.DrawEllipse(frame);
                        composer.FillStroke();
                        composer.End();
                    }

                    var blockComposer = new BlockComposer(composer);
                    blockComposer.Begin(frame, XAlignmentEnum.Center, YAlignmentEnum.Middle);
                    composer.SetFillColor(this.ForeColor);
                    composer.SetFont(
                      new StandardType1Font(
                        document,
                        StandardType1Font.FamilyEnum.ZapfDingbats,
                        true,
                        false
                        ),
                      size.Height * 0.8
                      );
                    _ = blockComposer.ShowText(new string(new char[] { this.RadioSymbol }));
                    blockComposer.End();

                    composer.Flush();
                }

                var offState = new FormXObject(document, size);
                normalAppearance[PdfName.Off] = offState;
                if (this.GraphicsVisibile)
                {
                    var composer = new PrimitiveComposer(offState);

                    _ = composer.BeginLocalState();
                    composer.SetLineWidth(lineWidth);
                    composer.SetFillColor(this.BackColor);
                    composer.SetStrokeColor(this.ForeColor);
                    composer.DrawEllipse(frame);
                    composer.FillStroke();
                    composer.End();

                    composer.Flush();
                }
            }
        }

        private void Apply(
          PushButton field
          )
        {
            var document = field.Document;
            var widget = field.Widgets[0];

            var appearance = widget.Appearance;
            FormXObject normalAppearanceState;
            var size = widget.Box.Size;
            normalAppearanceState = new FormXObject(document, size);
            var composer = new PrimitiveComposer(normalAppearanceState);

            float lineWidth = 1;
            var frame = new RectangleF(lineWidth / 2, lineWidth / 2, size.Width - lineWidth, size.Height - lineWidth);
            if (this.GraphicsVisibile)
            {
                _ = composer.BeginLocalState();
                composer.SetLineWidth(lineWidth);
                composer.SetFillColor(this.BackColor);
                composer.SetStrokeColor(this.ForeColor);
                composer.DrawRectangle(frame, 5);
                composer.FillStroke();
                composer.End();
            }

            var title = (string)field.Value;
            if (title != null)
            {
                var blockComposer = new BlockComposer(composer);
                blockComposer.Begin(frame, XAlignmentEnum.Center, YAlignmentEnum.Middle);
                composer.SetFillColor(this.ForeColor);
                composer.SetFont(
                  new StandardType1Font(
                    document,
                    StandardType1Font.FamilyEnum.Helvetica,
                    true,
                    false
                    ),
                  size.Height * 0.5
                  );
                _ = blockComposer.ShowText(title);
                blockComposer.End();
            }

            composer.Flush();
            appearance.Normal[null] = normalAppearanceState;
        }

        private void Apply(
          TextField field
          )
        {
            var document = field.Document;
            var widget = field.Widgets[0];

            var appearance = widget.Appearance;
            widget.BaseDataObject[PdfName.DA] = new PdfString($"/Helv {this.FontSize} Tf 0 0 0 rg");

            FormXObject normalAppearanceState;
            var size = widget.Box.Size;
            normalAppearanceState = new FormXObject(document, size);
            var composer = new PrimitiveComposer(normalAppearanceState);

            float lineWidth = 1;
            var frame = new RectangleF(lineWidth / 2, lineWidth / 2, size.Width - lineWidth, size.Height - lineWidth);
            if (this.GraphicsVisibile)
            {
                _ = composer.BeginLocalState();
                composer.SetLineWidth(lineWidth);
                composer.SetFillColor(this.BackColor);
                composer.SetStrokeColor(this.ForeColor);
                composer.DrawRectangle(frame, 5);
                composer.FillStroke();
                composer.End();
            }

            _ = composer.BeginMarkedContent(PdfName.Tx);
            composer.SetFont(
              new StandardType1Font(
                document,
                StandardType1Font.FamilyEnum.Helvetica,
                false,
                false
                ),
              this.FontSize
              );
            _ = composer.ShowText(
              (string)field.Value,
              new PointF(0, size.Height / 2),
              XAlignmentEnum.Left,
              YAlignmentEnum.Middle,
              0
              );
            composer.End();

            composer.Flush();
            appearance.Normal[null] = normalAppearanceState;
        }

        private void Apply(
          ComboBox field
          )
        {
            var document = field.Document;
            var widget = field.Widgets[0];

            var appearance = widget.Appearance;
            widget.BaseDataObject[PdfName.DA] = new PdfString($"/Helv {this.FontSize} Tf 0 0 0 rg");

            FormXObject normalAppearanceState;
            var size = widget.Box.Size;
            normalAppearanceState = new FormXObject(document, size);
            var composer = new PrimitiveComposer(normalAppearanceState);

            float lineWidth = 1;
            var frame = new RectangleF(lineWidth / 2, lineWidth / 2, size.Width - lineWidth, size.Height - lineWidth);
            if (this.GraphicsVisibile)
            {
                _ = composer.BeginLocalState();
                composer.SetLineWidth(lineWidth);
                composer.SetFillColor(this.BackColor);
                composer.SetStrokeColor(this.ForeColor);
                composer.DrawRectangle(frame, 5);
                composer.FillStroke();
                composer.End();
            }

            _ = composer.BeginMarkedContent(PdfName.Tx);
            composer.SetFont(
              new StandardType1Font(
                document,
                StandardType1Font.FamilyEnum.Helvetica,
                false,
                false
                ),
              this.FontSize
              );
            _ = composer.ShowText(
              (string)field.Value,
              new PointF(0, size.Height / 2),
              XAlignmentEnum.Left,
              YAlignmentEnum.Middle,
              0
              );
            composer.End();

            composer.Flush();
            appearance.Normal[null] = normalAppearanceState;
        }

        private void Apply(
          ListBox field
          )
        {
            var document = field.Document;
            var widget = field.Widgets[0];

            var appearance = widget.Appearance;
            var widgetDataObject = widget.BaseDataObject;
            widgetDataObject[PdfName.DA] = new PdfString($"/Helv {this.FontSize} Tf 0 0 0 rg");
            widgetDataObject[PdfName.MK] = new PdfDictionary(
              new PdfName[]
              {
            PdfName.BG,
            PdfName.BC
              },
              new PdfDirectObject[]
              {
            new PdfArray(new PdfDirectObject[]{PdfReal.Get(.9), PdfReal.Get(.9), PdfReal.Get(.9)}),
            new PdfArray(new PdfDirectObject[]{PdfInteger.Default, PdfInteger.Default, PdfInteger.Default})
              }
              );

            FormXObject normalAppearanceState;
            var size = widget.Box.Size;
            normalAppearanceState = new FormXObject(document, size);
            var composer = new PrimitiveComposer(normalAppearanceState);

            float lineWidth = 1;
            var frame = new RectangleF(lineWidth / 2, lineWidth / 2, size.Width - lineWidth, size.Height - lineWidth);
            if (this.GraphicsVisibile)
            {
                _ = composer.BeginLocalState();
                composer.SetLineWidth(lineWidth);
                composer.SetFillColor(this.BackColor);
                composer.SetStrokeColor(this.ForeColor);
                composer.DrawRectangle(frame, 5);
                composer.FillStroke();
                composer.End();
            }

            _ = composer.BeginLocalState();
            if (this.GraphicsVisibile)
            {
                composer.DrawRectangle(frame, 5);
                composer.Clip(); // Ensures that the visible content is clipped within the rounded frame.
            }
            _ = composer.BeginMarkedContent(PdfName.Tx);
            composer.SetFont(
              new StandardType1Font(
                document,
                StandardType1Font.FamilyEnum.Helvetica,
                false,
                false
                ),
              this.FontSize
              );
            double y = 3;
            foreach (var item in field.Items)
            {
                _ = composer.ShowText(
                  item.Text,
                  new PointF(0, (float)y)
                  );
                y += this.FontSize * 1.175;
                if (y > size.Height)
                {
                    break;
                }
            }
            composer.End();
            composer.End();

            composer.Flush();
            appearance.Normal[null] = normalAppearanceState;
        }

        public override void Apply(
Field field
)
        {
            if (field is PushButton)
            { this.Apply((PushButton)field); }
            else if (field is CheckBox)
            { this.Apply((CheckBox)field); }
            else if (field is TextField)
            { this.Apply((TextField)field); }
            else if (field is ComboBox)
            { this.Apply((ComboBox)field); }
            else if (field is ListBox)
            { this.Apply((ListBox)field); }
            else if (field is RadioButton)
            { this.Apply((RadioButton)field); }
        }
    }
}