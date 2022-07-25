/*
  Copyright 2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library"
  (the Program): see the accompanying README files for more info.

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
      <summary>A page-piece dictionary used to hold private application data [PDF:1.7:10.4].</summary>
    */
    [PDF(VersionEnum.PDF13)]
    public sealed class AppDataCollection
      : PdfObjectWrapper<PdfDictionary>,
        IDictionary<PdfName, AppData>
    {

        private readonly IAppDataHolder holder;

        private AppDataCollection(
  PdfDirectObject baseObject,
  IAppDataHolder holder
  ) : base(baseObject)
        { this.holder = holder; }

        public AppData this[
          PdfName key
          ]
        {
            get => AppData.Wrap(this.BaseDataObject[key]);
            set => throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return this.GetEnumerator(); }

        public void Add(
  KeyValuePair<PdfName, AppData> item
  )
        { throw new NotSupportedException(); }

        public void Add(
  PdfName key,
  AppData value
  )
        { throw new NotSupportedException(); }

        public void Clear(
          )
        { this.BaseDataObject.Clear(); }

        public bool Contains(
          KeyValuePair<PdfName, AppData> item
          )
        { return item.Value.BaseObject.Equals(this.BaseDataObject[item.Key]); }

        public bool ContainsKey(
          PdfName key
          )
        { return this.BaseDataObject.ContainsKey(key); }

        public void CopyTo(
          KeyValuePair<PdfName, AppData>[] array,
          int arrayIndex
          )
        { throw new NotImplementedException(); }

        public AppData Ensure(
  PdfName key
  )
        {
            var appData = this[key];
            if (appData == null)
            {
                this.BaseDataObject[key] = (appData = new AppData(this.Document)).BaseObject;
                this.holder.Touch(key);
            }
            return appData;
        }

        public IEnumerator<KeyValuePair<PdfName, AppData>> GetEnumerator(
  )
        {
            foreach (var key in this.Keys)
            { yield return new KeyValuePair<PdfName, AppData>(key, this[key]); }
        }

        public bool Remove(
          PdfName key
          )
        { return this.BaseDataObject.Remove(key); }

        public bool Remove(
          KeyValuePair<PdfName, AppData> item
          )
        {
            if (this.Contains(item))
            {
                return this.Remove(item.Key);
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(
          PdfName key,
          out AppData value
          )
        { throw new NotImplementedException(); }
        public static AppDataCollection Wrap(
PdfDirectObject baseObject,
IAppDataHolder holder
)
        { return (baseObject != null) ? new AppDataCollection(baseObject, holder) : null; }

        public int Count => this.BaseDataObject.Count;

        public bool IsReadOnly => false;

        public ICollection<PdfName> Keys => this.BaseDataObject.Keys;

        public ICollection<AppData> Values
        {
            get
            {
                ICollection<AppData> values = new List<AppData>();
                foreach (var valueObject in this.BaseDataObject.Values)
                { values.Add(AppData.Wrap(valueObject)); }
                return values;
            }
        }
    }
}

