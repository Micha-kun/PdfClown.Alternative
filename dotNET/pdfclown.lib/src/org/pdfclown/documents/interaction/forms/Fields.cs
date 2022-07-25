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
    using System;

    using System.Collections;
    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Interactive form fields [PDF:1.6:8.6.1].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class Fields
      : PdfObjectWrapper<PdfArray>,
        IDictionary<string, Field>
    {

        internal Fields(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        public Fields(
Document context
) : base(context, new PdfArray())
        { }

        public Field this[
          string key
          ]
        {
            get
            {
                /*
                  TODO: It is possible for different field dictionaries to have the SAME fully qualified field
                  name if they are descendants of a common ancestor with that name and have no
                  partial field names (T entries) of their own. Such field dictionaries are different
                  representations of the same underlying field; they should differ only in properties
                  that specify their visual appearance. In particular, field dictionaries with the same
                  fully qualified field name must have the same field type (FT), value (V), and default
                  value (DV).
                 */
                PdfReference valueFieldReference = null;
                var partialNamesIterator = key.Split('.').GetEnumerator();
                var fieldObjectsIterator = this.BaseDataObject.GetEnumerator();
                while (partialNamesIterator.MoveNext())
                {
                    var partialName = (string)partialNamesIterator.Current;
                    valueFieldReference = null;
                    while ((fieldObjectsIterator != null)
                      && fieldObjectsIterator.MoveNext())
                    {
                        var fieldReference = (PdfReference)fieldObjectsIterator.Current;
                        var fieldDictionary = (PdfDictionary)fieldReference.DataObject;
                        var fieldName = (PdfTextString)fieldDictionary[PdfName.T];
                        if ((fieldName != null) && fieldName.Value.Equals(partialName))
                        {
                            valueFieldReference = fieldReference;
                            var kidFieldObjects = (PdfArray)fieldDictionary.Resolve(PdfName.Kids);
                            fieldObjectsIterator = (kidFieldObjects == null) ? null : kidFieldObjects.GetEnumerator();
                            break;
                        }
                    }
                    if (valueFieldReference == null)
                    {
                        break;
                    }
                }
                return Field.Wrap(valueFieldReference);
            }
            set => throw new NotImplementedException();/*
                TODO:put the field into the correct position, based on the full name (key)!!!
                */
        }

        void ICollection<KeyValuePair<string, Field>>.Add(
  KeyValuePair<string, Field> entry
  )
        { this.Add(entry.Key, entry.Value); }

        bool ICollection<KeyValuePair<string, Field>>.Contains(
          KeyValuePair<string, Field> entry
          )
        { throw new NotImplementedException(); }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<string, Field>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<string, Field>> IEnumerable<KeyValuePair<string, Field>>.GetEnumerator(
  )
        { throw new NotImplementedException(); }

        private void RetrieveValues(
  PdfArray fieldObjects,
  IList<Field> values
  )
        {
            foreach (var fieldObject in fieldObjects)
            {
                var fieldReference = (PdfReference)fieldObject;
                var kidReferences = (PdfArray)((PdfDictionary)fieldReference.DataObject).Resolve(PdfName.Kids);
                PdfDictionary kidObject;
                if (kidReferences == null)
                { kidObject = null; }
                else
                { kidObject = (PdfDictionary)((PdfReference)kidReferences[0]).DataObject; }
                // Terminal field?
                if ((kidObject == null) // Merged single widget annotation.
                  || (!kidObject.ContainsKey(PdfName.FT) // Multiple widget annotations.
                    && kidObject.ContainsKey(PdfName.Subtype)
                    && kidObject[PdfName.Subtype].Equals(PdfName.Widget)))
                { values.Add(Field.Wrap(fieldReference)); }
                else // Non-terminal field.
                { this.RetrieveValues(kidReferences, values); }
            }
        }

        public void Add(
Field value
)
        { this.BaseDataObject.Add(value.BaseObject); }

        public void Add(
  string key,
  Field value
  )
        { throw new NotImplementedException(); }

        public void Clear(
          )
        { this.BaseDataObject.Clear(); }

        public bool ContainsKey(
          string key
          )
        //TODO: avoid getter (use raw matching).
        { return this[key] != null; }

        public void CopyTo(
          KeyValuePair<string, Field>[] entries,
          int index
          )
        { throw new NotImplementedException(); }

        public bool Remove(
          string key
          )
        {
            var field = this[key];
            if (field == null)
            {
                return false;
            }

            PdfArray fieldObjects;
            var fieldParentReference = (PdfReference)field.BaseDataObject[PdfName.Parent];
            if (fieldParentReference == null)
            { fieldObjects = this.BaseDataObject; }
            else
            { fieldObjects = (PdfArray)((PdfDictionary)fieldParentReference.DataObject).Resolve(PdfName.Kids); }
            return fieldObjects.Remove(field.BaseObject);
        }

        public bool Remove(
          KeyValuePair<string, Field> entry
          )
        { throw new NotImplementedException(); }

        public bool TryGetValue(
          string key,
          out Field value
          )
        {
            value = this[key];
            return (value != null)
              || this.ContainsKey(key);
        }

        public int Count => this.Values.Count;

        public bool IsReadOnly => false;

        public ICollection<string> Keys => throw new NotImplementedException();//TODO: retrieve all the full names (keys)!!!

        public ICollection<Field> Values
        {
            get
            {
                IList<Field> values = new List<Field>();
                this.RetrieveValues(this.BaseDataObject, values);
                return values;
            }
        }
    }
}