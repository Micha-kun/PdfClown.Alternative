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

namespace org.pdfclown.documents.contents
{
    using System;

    using System.Collections;
    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Collection of a specific resource type.</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public abstract class ResourceItems<TValue>
      : PdfObjectWrapper<PdfDictionary>,
        IDictionary<PdfName, TValue>
      where TValue : PdfObjectWrapper
    {
        protected ResourceItems(
Document context
) : base(context, new PdfDictionary())
        { }

        internal ResourceItems(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public TValue this[
          PdfName key
          ]
        {
            get => this.Wrap(this.BaseDataObject[key]);
            set => this.BaseDataObject[key] = value.BaseObject;
        }

        void ICollection<KeyValuePair<PdfName, TValue>>.Add(
  KeyValuePair<PdfName, TValue> entry
  )
        { this.Add(entry.Key, entry.Value); }

        bool ICollection<KeyValuePair<PdfName, TValue>>.Contains(
          KeyValuePair<PdfName, TValue> entry
          )
        { return entry.Value.BaseObject.Equals(this.BaseDataObject[entry.Key]); }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<PdfName, TValue>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<PdfName, TValue>> IEnumerable<KeyValuePair<PdfName, TValue>>.GetEnumerator(
  )
        {
            foreach (var key in this.Keys)
            { yield return new KeyValuePair<PdfName, TValue>(key, this[key]); }
        }

        /**
  <summary>Wraps a base object within its corresponding high-level representation.</summary>
*/
        protected abstract TValue Wrap(
          PdfDirectObject baseObject
          );

        public void Add(
  PdfName key,
  TValue value
  )
        { this.BaseDataObject.Add(key, value.BaseObject); }

        public void Clear(
          )
        { this.BaseDataObject.Clear(); }

        public bool ContainsKey(
          PdfName key
          )
        { return this.BaseDataObject.ContainsKey(key); }

        public void CopyTo(
          KeyValuePair<PdfName, TValue>[] entries,
          int index
          )
        { throw new NotImplementedException(); }

        /**
Gets the key associated to a given value.
*/
        public PdfName GetKey(
          TValue value
          )
        { return this.BaseDataObject.GetKey(value.BaseObject); }

        public bool Remove(
          PdfName key
          )
        { return this.BaseDataObject.Remove(key); }

        public bool Remove(
          KeyValuePair<PdfName, TValue> entry
          )
        {
            return this.BaseDataObject.Remove(
              new KeyValuePair<PdfName, PdfDirectObject>(
                entry.Key,
                entry.Value.BaseObject
                )
              );
        }

        public bool TryGetValue(
          PdfName key,
          out TValue value
          )
        { return ((value = this[key]) != null) || this.ContainsKey(key); }

        public int Count => this.BaseDataObject.Count;

        public bool IsReadOnly => false;

        public ICollection<PdfName> Keys => this.BaseDataObject.Keys;

        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> values;
                // Get the low-level objects!
                var valueObjects = this.BaseDataObject.Values;
                // Populating the high-level collection...
                values = new List<TValue>(valueObjects.Count);
                foreach (var valueObject in valueObjects)
                { values.Add(this.Wrap(valueObject)); }
                return values;
            }
        }
    }
}