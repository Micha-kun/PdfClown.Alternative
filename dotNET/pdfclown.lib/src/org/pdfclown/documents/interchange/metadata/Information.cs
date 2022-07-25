/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interchange.metadata
{
    using System;

    using System.Collections;
    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Document information [PDF:1.6:10.2.1].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class Information
      : PdfObjectWrapper<PdfDictionary>,
        IDictionary<PdfName, object>
    {

        private Information(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public Information(
Document context
) : base(context, new PdfDictionary())
        { }

        public object this[
          PdfName key
          ]
        {
            get => PdfSimpleObject<object>.GetValue(this.BaseDataObject[key]);
            set => this.BaseDataObject[key] = PdfSimpleObject<object>.Get(value);
        }

        void ICollection<KeyValuePair<PdfName, object>>.Add(
  KeyValuePair<PdfName, object> entry
  )
        { this.Add(entry.Key, entry.Value); }

        bool ICollection<KeyValuePair<PdfName, object>>.Contains(
          KeyValuePair<PdfName, object> entry
          )
        { return entry.Value.Equals(this[entry.Key]); }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<PdfName, object>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<PdfName, object>> IEnumerable<KeyValuePair<PdfName, object>>.GetEnumerator(
  )
        {
            foreach (var entry in this.BaseDataObject)
            {
                yield return new KeyValuePair<PdfName, object>(
                  entry.Key,
                  PdfSimpleObject<object>.GetValue(entry.Value)
                  );
            }
        }

        public void Add(
  PdfName key,
  object value
  )
        { this.BaseDataObject.Add(key, PdfSimpleObject<object>.Get(value)); }

        public void Clear(
          )
        { this.BaseDataObject.Clear(); }

        public bool ContainsKey(
          PdfName key
          )
        { return this.BaseDataObject.ContainsKey(key); }

        public void CopyTo(
          KeyValuePair<PdfName, object>[] entries,
          int index
          )
        { throw new NotImplementedException(); }

        public bool Remove(
          PdfName key
          )
        { return this.BaseDataObject.Remove(key); }

        public bool Remove(
          KeyValuePair<PdfName, object> entry
          )
        { throw new NotImplementedException(); }

        public bool TryGetValue(
          PdfName key,
          out object value
          )
        {
            PdfDirectObject valueObject;
            if (this.BaseDataObject.TryGetValue(key, out valueObject))
            {
                value = PdfSimpleObject<object>.GetValue(valueObject);
                return true;
            }
            else
            {
                value = null;
            }

            return false;
        }
        public static Information Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new Information(baseObject) : null; }

        public string Author
        {
            get => (string)this[PdfName.Author];
            set => this[PdfName.Author] = value;
        }

        public int Count => this.BaseDataObject.Count;

        public DateTime? CreationDate
        {
            get => (DateTime?)this[PdfName.CreationDate];
            set => this[PdfName.CreationDate] = value;
        }

        public string Creator
        {
            get => (string)this[PdfName.Creator];
            set => this[PdfName.Creator] = value;
        }

        public bool IsReadOnly => false;

        public ICollection<PdfName> Keys => this.BaseDataObject.Keys;

        [PDF(VersionEnum.PDF11)]
        public string Keywords
        {
            get => (string)this[PdfName.Keywords];
            set => this[PdfName.Keywords] = value;
        }

        [PDF(VersionEnum.PDF11)]
        public DateTime? ModificationDate
        {
            get => (DateTime?)this[PdfName.ModDate];
            set => this[PdfName.ModDate] = value;
        }

        public string Producer
        {
            get => (string)this[PdfName.Producer];
            set => this[PdfName.Producer] = value;
        }

        [PDF(VersionEnum.PDF11)]
        public string Subject
        {
            get => (string)this[PdfName.Subject];
            set => this[PdfName.Subject] = value;
        }

        [PDF(VersionEnum.PDF11)]
        public string Title
        {
            get => (string)this[PdfName.Title];
            set => this[PdfName.Title] = value;
        }

        public ICollection<object> Values
        {
            get
            {
                IList<object> values = new List<object>();
                foreach (var item in this.BaseDataObject.Values)
                { values.Add(PdfSimpleObject<object>.GetValue(item)); }
                return values;
            }
        }
    }
}