/*
  Copyright 2008-2011 Stefano Chizzolini. http://www.pdfclown.org

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
    using org.pdfclown.documents.contents.colorSpaces;

    /**
      <summary>Abstract field appearance style.</summary>
      <remarks>It automates the definition of field appearance, applying a common look.</remarks>
    */
    public abstract class FieldStyle
    {
        private Color backColor = DeviceRGBColor.White;
        private char checkSymbol = (char)52;
        private double fontSize = 10;
        private Color foreColor = DeviceRGBColor.Black;
        private bool graphicsVisibile = false;
        private char radioSymbol = (char)108;

        protected FieldStyle(
  )
        { }

        public abstract void Apply(
Field field
);

        public Color BackColor
        {
            get => this.backColor;
            set => this.backColor = value;
        }

        public char CheckSymbol
        {
            get => this.checkSymbol;
            set => this.checkSymbol = value;
        }

        public double FontSize
        {
            get => this.fontSize;
            set => this.fontSize = value;
        }

        public Color ForeColor
        {
            get => this.foreColor;
            set => this.foreColor = value;
        }

        public bool GraphicsVisibile
        {
            get => this.graphicsVisibile;
            set => this.graphicsVisibile = value;
        }

        public char RadioSymbol
        {
            get => this.radioSymbol;
            set => this.radioSymbol = value;
        }
    }
}