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


namespace org.pdfclown.tools
{
    using System.Collections.Generic;
    using org.pdfclown.documents;
    using org.pdfclown.documents.interaction.annotations;
    using org.pdfclown.objects;

    /**
      <summary>Tool to flatten Acroforms.</summary>
    */
    public sealed class FormFlattener
    {
        private bool hiddenRendered;
        private bool nonPrintableRendered;

        /**
          <summary>Replaces the Acroform fields with their corresponding graphics representation.</summary>
          <param name="document">Document to flatten.</param>
        */
        public void Flatten(
          Document document
          )
        {
            var pageStampers = new Dictionary<Page, PageStamper>();
            var form = document.Form;
            var formFields = form.Fields;
            foreach (var field in formFields.Values)
            {
                foreach (var widget in field.Widgets)
                {
                    var widgetPage = widget.Page;
                    var flags = widget.Flags;
                    // Is the widget to be rendered?
                    if ((((flags & Annotation.FlagsEnum.Hidden) == 0) || this.hiddenRendered)
                      && (((flags & Annotation.FlagsEnum.Print) > 0) || this.nonPrintableRendered))
                    {
                        // Stamping the current state appearance of the widget...
                        var widgetCurrentState = (PdfName)widget.BaseDataObject[PdfName.AS];
                        var widgetCurrentAppearance = widget.Appearance.Normal[widgetCurrentState];
                        if (widgetCurrentAppearance != null)
                        {
                            PageStamper widgetStamper;
                            if (!pageStampers.TryGetValue(widgetPage, out widgetStamper))
                            { pageStampers[widgetPage] = widgetStamper = new PageStamper(widgetPage); }

                            var widgetBox = widget.Box;
                            widgetStamper.Foreground.ShowXObject(widgetCurrentAppearance, widgetBox.Location, widgetBox.Size);
                        }
                    }

                    // Removing the widget from the page annotations...
                    var widgetPageAnnotations = widgetPage.Annotations;
                    _ = widgetPageAnnotations.Remove(widget);
                    if (widgetPageAnnotations.Count == 0)
                    {
                        widgetPage.Annotations = null;
                        _ = widgetPageAnnotations.Delete();
                    }

                    // Removing the field references relating the widget...
                    var fieldPartDictionary = widget.BaseDataObject;
                    while (fieldPartDictionary != null)
                    {
                        var parentFieldPartDictionary = (PdfDictionary)fieldPartDictionary.Resolve(PdfName.Parent);

                        PdfArray kidsArray;
                        if (parentFieldPartDictionary != null)
                        { kidsArray = (PdfArray)parentFieldPartDictionary.Resolve(PdfName.Kids); }
                        else
                        { kidsArray = formFields.BaseDataObject; }

                        _ = kidsArray.Remove(fieldPartDictionary.Reference);
                        _ = fieldPartDictionary.Delete();
                        if (kidsArray.Count > 0)
                        {
                            break;
                        }

                        fieldPartDictionary = parentFieldPartDictionary;
                    }
                }
            }
            if (formFields.Count == 0)
            {
                // Removing the form root...
                document.Form = null;
                _ = form.Delete();
            }
            foreach (var pageStamper in pageStampers.Values)
            { pageStamper.Flush(); }
        }

        /**
          <summary>Gets/Sets whether hidden fields have to be rendered.</summary>
        */
        public bool HiddenRendered
        {
            get => this.hiddenRendered;
            set => this.hiddenRendered = value;
        }

        /**
          <summary>Gets/Sets whether non-printable fields have to be rendered.</summary>
        */
        public bool NonPrintableRendered
        {
            get => this.nonPrintableRendered;
            set => this.nonPrintableRendered = value;
        }
    }
}
