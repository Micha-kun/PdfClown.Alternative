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
    using System.Drawing;

    using org.pdfclown.documents.contents.xObjects;
    using org.pdfclown.objects;
    using org.pdfclown.util;

    /**
      <summary>Widget annotation [PDF:1.6:8.4.5].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class Widget
      : Annotation
    {

        private static readonly Dictionary<HighlightModeEnum, PdfName> HighlightModeEnumCodes;

        static Widget()
        {
            HighlightModeEnumCodes = new Dictionary<HighlightModeEnum, PdfName>();
            HighlightModeEnumCodes[HighlightModeEnum.None] = PdfName.N;
            HighlightModeEnumCodes[HighlightModeEnum.Invert] = PdfName.I;
            HighlightModeEnumCodes[HighlightModeEnum.Outline] = PdfName.O;
            HighlightModeEnumCodes[HighlightModeEnum.Push] = PdfName.P;
            HighlightModeEnumCodes[HighlightModeEnum.Toggle] = PdfName.T;
        }

        internal Widget(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
<summary>Creates a new generic widget.</summary>
*/
        public Widget(
          Page page,
          RectangleF box
          ) : base(page, PdfName.Widget, box, null)
        { this.Flags = EnumUtils.Mask(this.Flags, FlagsEnum.Print, true); }

        /**
          <summary>Creates a new dual-state widget (required by <see
          cref="org.pdfclown.documents.forms.RadioButton"/> fields).</summary>
        */
        public Widget(
          Page page,
          RectangleF box,
          string name
          ) : this(page, box)
        {
            // Initialize the on-state appearance!
            /*
              NOTE: This is necessary to keep the reference to the on-state name.
            */
            var appearance = new Appearance(page.Document);
            this.Appearance = appearance;
            var normalAppearance = appearance.Normal;
            normalAppearance[new PdfName(name)] = new FormXObject(page.Document, box.Size);
        }

        /**
<summary>Gets the code corresponding to the given value.</summary>
*/
        private static PdfName ToCode(
          HighlightModeEnum value
          )
        { return HighlightModeEnumCodes[value]; }

        /**
          <summary>Gets the highlighting mode corresponding to the given value.</summary>
        */
        private static HighlightModeEnum ToHighlightModeEnum(
          PdfName value
          )
        {
            foreach (var mode in HighlightModeEnumCodes)
            {
                if (mode.Value.Equals(value))
                {
                    return mode.Key;
                }
            }
            return HighlightModeEnum.Invert;
        }

        public override AnnotationActions Actions
        {
            get => new WidgetActions(this, this.BaseDataObject.Get<PdfDictionary>(PdfName.AA));
            set => base.Actions = value;
        }

        /**
          <summary>Gets/Sets the annotation's appearance characteristics to be used for its visual
          presentation on the page.</summary>
        */
        public AppearanceCharacteristics AppearanceCharacteristics
        {
            get => AppearanceCharacteristics.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.MK));
            set => this.BaseDataObject[PdfName.MK] = value.BaseObject;
        }

        /**
          <summary>Gets/Sets the annotation's highlighting mode, the visual effect to be used when the
          mouse button is pressed or held down inside its active area.</summary>
        */
        public HighlightModeEnum HighlightMode
        {
            get => ToHighlightModeEnum((PdfName)this.BaseDataObject[PdfName.H]);
            set => this.BaseDataObject[PdfName.H] = ToCode(value);
        }

        /**
          <summary>Gets the widget value (applicable to dual-state widgets only). It corresponds to the
          on-state appearance of this widget.</summary>
        */
        public string Value
        {
            get
            {
                foreach (var normalAppearanceEntry in this.Appearance.Normal)
                {
                    var key = normalAppearanceEntry.Key;
                    if (!key.Equals(PdfName.Off)) // 'On' state.
                    {
                        return (string)key.Value;
                    }
                }
                return null; // NOTE: It MUST NOT happen (on-state should always be defined).
            }
        }
        /**
  <summary>Highlighting mode [PDF:1.6:8.4.5].</summary>
*/
        public enum HighlightModeEnum
        {
            /**
              <summary>No highlighting.</summary>
            */
            None,
            /**
              <summary>Invert the contents of the annotation rectangle.</summary>
            */
            Invert,
            /**
              <summary>Invert the annotation's border.</summary>
            */
            Outline,
            /**
              <summary>Display the annotation's down appearance.</summary>
            */
            Push,
            /**
              <summary>Same as Push (which is preferred).</summary>
            */
            Toggle
        };
    }
}