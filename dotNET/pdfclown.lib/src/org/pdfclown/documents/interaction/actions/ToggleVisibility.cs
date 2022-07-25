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

namespace org.pdfclown.documents.interaction.actions
{
    using System;
    using System.Collections.Generic;
    using org.pdfclown.documents.interaction.annotations;

    using org.pdfclown.documents.interaction.forms;
    using org.pdfclown.objects;

    /**
      <summary>'Toggle the visibility of one or more annotations on the screen' action [PDF:1.6:8.5.3].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class ToggleVisibility
      : Action
    {

        internal ToggleVisibility(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        /**
<summary>Creates a new action within the given document context.</summary>
*/
        public ToggleVisibility(
          Document context,
          ICollection<PdfObjectWrapper> objects,
          bool visible
          ) : base(context, PdfName.Hide)
        {
            this.Objects = objects;
            this.Visible = visible;
        }

        private void FillObjects(
  PdfDataObject objectObject,
  ICollection<PdfObjectWrapper> objects
  )
        {
            var objectDataObject = PdfObject.Resolve(objectObject);
            if (objectDataObject is PdfArray) // Multiple objects.
            {
                foreach (var itemObject in (PdfArray)objectDataObject)
                { this.FillObjects(itemObject, objects); }
            }
            else // Single object.
            {
                if (objectDataObject is PdfDictionary) // Annotation.
                {
                    objects.Add(
                      Annotation.Wrap((PdfReference)objectObject)
                      );
                }
                else if (objectDataObject is PdfTextString) // Form field (associated to widget annotations).
                {
                    objects.Add(
                      this.Document.Form.Fields[
                        (string)((PdfTextString)objectDataObject).Value
                        ]
                      );
                }
                else // Invalid object type.
                {
                    throw new Exception(
                      $"Invalid 'Hide' action target type ({objectDataObject.GetType().Name}).\nIt should be either an annotation or a form field."
                      );
                }
            }
        }

        /**
<summary>Gets/Sets the annotations (or associated form fields) to be affected.</summary>
*/
        public ICollection<PdfObjectWrapper> Objects
        {
            get
            {
                var objects = new List<PdfObjectWrapper>();
                var objectsObject = this.BaseDataObject[PdfName.T];
                this.FillObjects(objectsObject, objects);
                return objects;
            }
            set
            {
                var objectsDataObject = new PdfArray();
                foreach (var item in value)
                {
                    if (item is Annotation)
                    {
                        objectsDataObject.Add(
                          item.BaseObject
                          );
                    }
                    else if (item is Field)
                    {
                        objectsDataObject.Add(
                          new PdfTextString(((Field)item).FullName)
                          );
                    }
                    else
                    {
                        throw new ArgumentException(
                          $"Invalid 'Hide' action target type ({item.GetType().Name}).\nIt MUST be either an annotation or a form field."
                          );
                    }
                }
                this.BaseDataObject[PdfName.T] = objectsDataObject;
            }
        }

        /**
          <summary>Gets/Sets whether to show the annotations.</summary>
        */
        public bool Visible
        {
            get
            {
                var hideObject = (PdfBoolean)this.BaseDataObject[PdfName.H];
                return (hideObject != null)
&& !hideObject.BooleanValue;
            }
            set => this.BaseDataObject[PdfName.H] = PdfBoolean.Get(!value);
        }
    }
}