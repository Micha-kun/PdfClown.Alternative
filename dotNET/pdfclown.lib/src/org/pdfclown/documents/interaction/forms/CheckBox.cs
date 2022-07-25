/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.forms
{
    using org.pdfclown.documents.interaction.annotations;

    using org.pdfclown.objects;

    /**
      <summary>Check box field [PDF:1.6:8.6.3].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class CheckBox
      : ButtonField
    {

        internal CheckBox(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        /**
<summary>Creates a new checkbox within the given document context.</summary>
*/
        public CheckBox(
          string name,
          Widget widget,
          bool checked_
          ) : base(name, widget)
        { this.Checked = checked_; }

        public bool Checked
        {
            get
            {
                var value = (PdfName)this.BaseDataObject[PdfName.V];
                return !((value == null) || value.Equals(PdfName.Off));
            }
            set
            {
                var widgetDictionary = this.Widgets[0].BaseDataObject;
                /*
                  NOTE: The appearance for the off state is optional but, if present, MUST be stored in the
                  appearance dictionary under the name Off. The recommended (but NOT required) name for the
                  on state is Yes.
                */
                PdfName baseValue = null;
                if (value)
                {
                    var appearanceDictionary = (PdfDictionary)widgetDictionary.Resolve(PdfName.AP);
                    if (appearanceDictionary != null)
                    {
                        foreach (var appearanceKey in ((PdfDictionary)appearanceDictionary.Resolve(PdfName.N)).Keys)
                        {
                            if (!appearanceKey.Equals(PdfName.Off))
                            {
                                baseValue = appearanceKey;
                                break;
                            }
                        }
                    }
                    else
                    { baseValue = PdfName.Yes; }
                }
                else
                { baseValue = PdfName.Off; }
                this.BaseDataObject[PdfName.V] = baseValue;
                widgetDictionary[PdfName.AS] = baseValue;
            }
        }

        public override object Value
        {
            get => base.Value;
            set => this.Checked = !((value == null) || value.Equals(string.Empty) || value.Equals(PdfName.Off.Value));
        }
    }
}