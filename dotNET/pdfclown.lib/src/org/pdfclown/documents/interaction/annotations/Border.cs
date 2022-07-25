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

namespace org.pdfclown.documents.interaction.annotations
{
    using System.Collections.Generic;

    using org.pdfclown.documents.contents;
    using org.pdfclown.objects;

    /**
      <summary>Border characteristics [PDF:1.6:8.4.3].</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public sealed class Border
      : PdfObjectWrapper<PdfDictionary>
    {

        private static readonly LineDash DefaultLineDash = new LineDash(new double[] { 3 });
        private static readonly double DefaultWidth = 1;

        private static readonly Dictionary<StyleEnum, PdfName> StyleEnumCodes;

        static Border()
        {
            StyleEnumCodes = new Dictionary<StyleEnum, PdfName>();
            StyleEnumCodes[StyleEnum.Solid] = PdfName.S;
            StyleEnumCodes[StyleEnum.Dashed] = PdfName.D;
            StyleEnumCodes[StyleEnum.Beveled] = PdfName.B;
            StyleEnumCodes[StyleEnum.Inset] = PdfName.I;
            StyleEnumCodes[StyleEnum.Underline] = PdfName.U;
        }

        private Border(
          Document context,
          double width,
          StyleEnum style,
          LineDash pattern
          ) : base(
            context,
            new PdfDictionary(
              new PdfName[]
              {PdfName.Type},
              new PdfDirectObject[]
              {PdfName.Border}
              )
            )
        {
            this.Width = width;
            this.Style = style;
            this.Pattern = pattern;
        }

        internal Border(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
<summary>Creates a non-reusable instance.</summary>
*/
        public Border(
          double width
          ) : this(null, width)
        { }

        /**
          <summary>Creates a non-reusable instance.</summary>
        */
        public Border(
          double width,
          StyleEnum style
          ) : this(null, width, style)
        { }

        /**
          <summary>Creates a non-reusable instance.</summary>
        */
        public Border(
          double width,
          LineDash pattern
          ) : this(null, width, pattern)
        { }

        /**
          <summary>Creates a reusable instance.</summary>
        */
        public Border(
          Document context,
          double width
          ) : this(context, width, DefaultStyle, null)
        { }

        /**
          <summary>Creates a reusable instance.</summary>
        */
        public Border(
          Document context,
          double width,
          StyleEnum style
          ) : this(context, width, style, null)
        { }

        /**
          <summary>Creates a reusable instance.</summary>
        */
        public Border(
          Document context,
          double width,
          LineDash pattern
          ) : this(context, width, StyleEnum.Dashed, pattern)
        { }

        /**
<summary>Gets the code corresponding to the given value.</summary>
*/
        private static PdfName ToCode(
          StyleEnum value
          )
        { return StyleEnumCodes[value]; }

        /**
          <summary>Gets the style corresponding to the given value.</summary>
        */
        private static StyleEnum ToStyleEnum(
          PdfName value
          )
        {
            foreach (var style in StyleEnumCodes)
            {
                if (style.Value.Equals(value))
                {
                    return style.Key;
                }
            }
            return DefaultStyle;
        }

        /**
<summary>Gets/Sets the dash pattern used in case of dashed border.</summary>
*/
        public LineDash Pattern
        {
            get
            {
                var dashObject = (PdfArray)this.BaseDataObject[PdfName.D];
                return (dashObject != null) ? LineDash.Get(dashObject, null) : DefaultLineDash;
            }
            set
            {
                PdfArray dashObject = null;
                if (value != null)
                {
                    dashObject = new PdfArray();
                    foreach (var dashItem in value.DashArray)
                    { dashObject.Add(PdfReal.Get(dashItem)); }
                }
                this.BaseDataObject[PdfName.D] = dashObject;
            }
        }

        /**
          <summary>Gets/Sets the border style.</summary>
        */
        public StyleEnum Style
        {
            get => ToStyleEnum((PdfName)this.BaseDataObject[PdfName.S]);
            set => this.BaseDataObject[PdfName.S] = (value != DefaultStyle) ? ToCode(value) : null;
        }

        /**
          <summary>Gets/Sets the border width in points.</summary>
        */
        public double Width
        {
            get
            {
                var widthObject = (IPdfNumber)this.BaseDataObject[PdfName.W];
                return (widthObject != null) ? widthObject.RawValue : DefaultWidth;
            }
            set => this.BaseDataObject[PdfName.W] = PdfReal.Get(value);
        }
        /**
  <summary>Border style [PDF:1.6:8.4.3].</summary>
*/
        public enum StyleEnum
        {
            /**
              <summary>Solid.</summary>
            */
            Solid,
            /**
              <summary>Dashed.</summary>
            */
            Dashed,
            /**
              <summary>Beveled.</summary>
            */
            Beveled,
            /**
              <summary>Inset.</summary>
            */
            Inset,
            /**
              <summary>Underline.</summary>
            */
            Underline
        };

        private static readonly StyleEnum DefaultStyle = StyleEnum.Solid;
    }
}