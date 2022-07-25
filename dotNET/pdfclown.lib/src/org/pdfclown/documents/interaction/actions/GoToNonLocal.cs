/*
  Copyright 2010-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.actions
{
    using org.pdfclown.documents.files;
    using org.pdfclown.documents.interaction.navigation.document;

    using org.pdfclown.objects;

    /**
      <summary>Abstract 'go to non-local destination' action.</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public abstract class GotoNonLocal<T>
      : GoToDestination<T>
      where T : Destination
    {

        protected GotoNonLocal(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        protected GotoNonLocal(
Document context,
PdfName actionType,
FileSpecification destinationFile,
T destination
) : base(context, actionType, destination)
        { this.DestinationFile = destinationFile; }

        /**
<summary>Gets/Sets the file in which the destination is located.</summary>
*/
        public virtual FileSpecification DestinationFile
        {
            get => FileSpecification.Wrap(this.BaseDataObject[PdfName.F]);
            set => this.BaseDataObject[PdfName.F] = (value != null) ? value.BaseObject : null;
        }

        /**
          <summary>Gets/Sets the action options.</summary>
        */
        public OptionsEnum Options
        {
            get
            {
                OptionsEnum options = 0;
                var optionsObject = this.BaseDataObject[PdfName.NewWindow];
                if ((optionsObject != null)
                  && ((PdfBoolean)optionsObject).BooleanValue)
                { options |= OptionsEnum.NewWindow; }
                return options;
            }
            set
            {
                if ((value & OptionsEnum.NewWindow) == OptionsEnum.NewWindow)
                { this.BaseDataObject[PdfName.NewWindow] = PdfBoolean.True; }
                else if ((value & OptionsEnum.SameWindow) == OptionsEnum.SameWindow)
                { this.BaseDataObject[PdfName.NewWindow] = PdfBoolean.False; }
                else
                { _ = this.BaseDataObject.Remove(PdfName.NewWindow); } // NOTE: Forcing the absence of this entry ensures that the viewer application should behave in accordance with the current user preference.
            }
        }
    }
}