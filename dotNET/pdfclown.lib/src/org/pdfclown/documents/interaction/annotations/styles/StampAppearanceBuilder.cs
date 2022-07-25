/*
  Copyright 2015 Stefano Chizzolini. http://www.pdfclown.org

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


namespace org.pdfclown.documents.interaction.annotations.styles
{
    using System.Drawing;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.xObjects;
    using colors = org.pdfclown.documents.contents.colorSpaces;

    using fonts = org.pdfclown.documents.contents.fonts;

    /**
      <summary>Appearance builder for rubber stamp annotations.</summary>
      <seealso cref="org.pdfclown.documents.interaction.annotations.Stamp"/>
    */
    public class StampAppearanceBuilder
    {

        private static readonly Length DefaultBorderRadius = new Length(.05, Length.UnitModeEnum.Relative);
        private static readonly Length DefaultBorderWidth = new Length(.025, Length.UnitModeEnum.Relative);
        private static readonly colors::Color DefaultColor = colors::DeviceRGBColor.Get(System.Drawing.Color.Red);

        private bool borderDoubled = true;

        private readonly Document document;
        private fonts::Font font;
        private string text;
        private readonly TypeEnum type;
        private readonly float width;

        public StampAppearanceBuilder(
  Document document,
  TypeEnum type,
  string text,
  float width,
  fonts::Font font
  )
        {
            this.document = document;
            this.type = type;
            this.width = width;
            this.Text = text;
            this.Font = font;
        }

        public FormXObject Build(
          )
        {
            var isRound = this.type == TypeEnum.Round;
            var isStriped = this.type == TypeEnum.Striped;
            var textScale = .5;
            var borderWidth = this.borderWidth.GetValue(this.width);
            var doubleBorderGap = this.borderDoubled ? borderWidth : 0;
            double fontSize = 10;
            fontSize *= (this.width - (isStriped ? 2 : ((doubleBorderGap * 2) + (borderWidth * (this.borderDoubled ? 1.5 : 1) * 2) + (this.width * (isRound ? .15 : .05))))) / textScale / this.font.GetWidth(this.text, fontSize);
            var height = (float)(isRound ? this.width : ((this.font.GetAscent(fontSize) * 1.2) + (doubleBorderGap * 2) + (borderWidth * (this.borderDoubled ? 1.5 : 1) * 2)));
            var size = new SizeF(this.width, height);

            var appearance = new FormXObject(this.document, size);
            var composer = new PrimitiveComposer(appearance);
            if (this.color != null)
            {
                composer.SetStrokeColor(this.color);
                composer.SetFillColor(this.color);
            }
            composer.SetTextScale(textScale);
            composer.SetFont(this.font, fontSize);
            _ = composer.ShowText(this.text, new PointF(size.Width / 2, (float)((size.Height / 2) - (this.font.GetDescent(fontSize) * .4))), XAlignmentEnum.Center, YAlignmentEnum.Middle, 0);

            var borderRadius = isRound ? 0 : this.borderRadius.GetValue((size.Width + size.Height) / 2);
            var prevBorderBox = appearance.Box;
            for (int borderStep = 0, borderStepLimit = this.borderDoubled ? 2 : 1; borderStep < borderStepLimit; borderStep++)
            {
                if (borderStep == 0)
                { composer.SetLineWidth(borderWidth); }
                else
                { composer.SetLineWidth(composer.State.LineWidth / 2); }

                var lineWidth = (float)((borderStep > 0) ? (composer.State.LineWidth / 2) : borderWidth);
                var marginY = (float)((lineWidth / 2) + ((borderStep > 0) ? (composer.State.LineWidth + doubleBorderGap) : 0));
                var marginX = isStriped ? 0 : marginY;
                var borderBox = new RectangleF(prevBorderBox.X + marginX, prevBorderBox.Y + marginY, prevBorderBox.Width - (marginX * 2), prevBorderBox.Height - (marginY * 2));

                if (isRound)
                { composer.DrawEllipse(borderBox); }
                else
                {
                    if (isStriped)
                    {
                        composer.DrawLine(new PointF(borderBox.Left, borderBox.Top), new PointF(borderBox.Right, borderBox.Top));
                        composer.DrawLine(new PointF(borderBox.Left, borderBox.Bottom), new PointF(borderBox.Right, borderBox.Bottom));
                    }
                    else
                    { composer.DrawRectangle(borderBox, borderRadius * (1 - (.5 * borderStep))); }
                }
                composer.Stroke();
                prevBorderBox = borderBox;
            }
            composer.Flush();
            return appearance;
        }

        public bool BorderDoubled
        {
            set => this.borderDoubled = value;
        }

        public Length BorderRadius
        {
            set => this.borderRadius = value;
        }

        public Length BorderWidth
        {
            set => this.borderWidth = value;
        }

        public colors::Color Color
        {
            set => this.color = value;
        }

        public fonts::Font Font
        {
            set => this.font = value;
        }

        public string Text
        {
            set => this.text = value.ToUpper();
        }

        public enum TypeEnum
        {
            Round,
            Squared,
            Striped
        }

        private Length borderRadius = DefaultBorderRadius;
        private Length borderWidth = DefaultBorderWidth;
        private colors::Color color = DefaultColor;
    }
}
