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

namespace org.pdfclown.documents
{
    using org.pdfclown.documents.interaction.actions;
    using org.pdfclown.documents.interaction.navigation.document;
    using org.pdfclown.objects;

    using system = System;

    /**
      <summary>Document actions [PDF:1.6:8.5.2].</summary>
    */
    [PDF(VersionEnum.PDF14)]
    public sealed class DocumentActions
      : PdfObjectWrapper<PdfDictionary>
    {

        internal DocumentActions(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        public DocumentActions(
Document context
) : base(context, new PdfDictionary())
        { }

        /**
<summary>Gets/Sets the action to be performed after printing the document.</summary>
*/
        public Action AfterPrint
        {
            get => Action.Wrap(this.BaseDataObject[PdfName.DP]);
            set => this.BaseDataObject[PdfName.DP] = value.BaseObject;
        }

        /**
          <summary>Gets/Sets the action to be performed after saving the document.</summary>
        */
        public Action AfterSave
        {
            get => Action.Wrap(this.BaseDataObject[PdfName.DS]);
            set => this.BaseDataObject[PdfName.DS] = value.BaseObject;
        }

        /**
          <summary>Gets/Sets the action to be performed before printing the document.</summary>
        */
        public Action BeforePrint
        {
            get => Action.Wrap(this.BaseDataObject[PdfName.WP]);
            set => this.BaseDataObject[PdfName.WP] = value.BaseObject;
        }

        /**
          <summary>Gets/Sets the action to be performed before saving the document.</summary>
        */
        public Action BeforeSave
        {
            get => Action.Wrap(this.BaseDataObject[PdfName.WS]);
            set => this.BaseDataObject[PdfName.WS] = value.BaseObject;
        }

        /**
          <summary>Gets/Sets the action to be performed before closing the document.</summary>
        */
        public Action OnClose
        {
            get => Action.Wrap(this.BaseDataObject[PdfName.DC]);
            set => this.BaseDataObject[PdfName.DC] = value.BaseObject;
        }

        /**
          <summary>Gets/Sets the destination to be displayed or the action to be performed
          after opening the document.</summary>
        */
        public PdfObjectWrapper OnOpen
        {
            get
            {
                var onOpenObject = this.Document.BaseDataObject[PdfName.OpenAction];
                if (onOpenObject is PdfDictionary) // Action (dictionary).
                {
                    return Action.Wrap(onOpenObject);
                }
                else // Destination (array).
                {
                    return Destination.Wrap(onOpenObject);
                }
            }
            set
            {
                if (!((value is Action)
                  || (value is LocalDestination)))
                {
                    throw new system::ArgumentException("Value MUST be either an Action or a LocalDestination.");
                }

                this.Document.BaseDataObject[PdfName.OpenAction] = value.BaseObject;
            }
        }
    }
}