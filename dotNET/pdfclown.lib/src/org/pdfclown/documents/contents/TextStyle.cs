/*
  Copyright 2010-2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.contents
{
    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.documents.contents.fonts;

    /**
      <summary>Text style.</summary>
    */
    public sealed class TextStyle
    {
        private readonly Color fillColor;
        private readonly ColorSpace fillColorSpace;
        private readonly Font font;
        private readonly double fontSize;
        private readonly TextRenderModeEnum renderMode;
        private readonly double scaleX;
        private readonly double scaleY;
        private readonly Color strokeColor;
        private readonly ColorSpace strokeColorSpace;

        public TextStyle(
  Font font,
  double fontSize,
  TextRenderModeEnum renderMode,
  Color strokeColor,
  ColorSpace strokeColorSpace,
  Color fillColor,
  ColorSpace fillColorSpace,
  double scaleX,
  double scaleY
  )
        {
            this.font = font;
            this.fontSize = fontSize;
            this.renderMode = renderMode;
            this.strokeColor = strokeColor;
            this.strokeColorSpace = strokeColorSpace;
            this.fillColor = fillColor;
            this.fillColorSpace = fillColorSpace;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
        }

        /**
          <exception cref="EncodeException"/>
        */
        public double GetWidth(
          char textChar
          )
        { return this.font.GetWidth(textChar, this.fontSize) * this.scaleX / this.scaleY; }

        public Color FillColor => this.fillColor;

        public ColorSpace FillColorSpace => this.fillColorSpace;

        public Font Font => this.font;

        public double FontSize => this.fontSize;

        public TextRenderModeEnum RenderMode => this.renderMode;

        public double ScaleX => this.scaleX;

        public double ScaleY => this.scaleY;

        public Color StrokeColor => this.strokeColor;

        public ColorSpace StrokeColorSpace => this.strokeColorSpace;
    }
}