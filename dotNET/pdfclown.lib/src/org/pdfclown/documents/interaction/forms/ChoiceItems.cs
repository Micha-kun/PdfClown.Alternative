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
      <summary>Field options [PDF:1.6:8.6.3].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class ChoiceItems
      : PdfObjectWrapper<PdfArray>,
      IList<ChoiceItem>
    {

        internal ChoiceItems(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public ChoiceItems(
  Document context
  ) : base(context, new PdfArray())
        { }

        public ChoiceItem this[
          int index
          ]
        {
            get => new ChoiceItem(this.BaseDataObject[index], this);
            set
            {
                this.BaseDataObject[index] = value.BaseObject;
                value.Items = this;
            }
        }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<ChoiceItem>)this).GetEnumerator(); }

        IEnumerator<ChoiceItem> IEnumerable<ChoiceItem>.GetEnumerator(
  )
        {
            for (
              int index = 0,
                length = this.Count;
              index < length;
              index++
              )
            { yield return this[index]; }
        }

        public ChoiceItem Add(
string value
)
        {
            var item = new ChoiceItem(value);
            this.Add(item);

            return item;
        }

        public void Add(
  ChoiceItem value
  )
        {
            this.BaseDataObject.Add(value.BaseObject);
            value.Items = this;
        }

        public void Clear(
          )
        { this.BaseDataObject.Clear(); }

        public bool Contains(
          ChoiceItem value
          )
        { return this.BaseDataObject.Contains(value.BaseObject); }

        public void CopyTo(
          ChoiceItem[] values,
          int index
          )
        { throw new NotImplementedException(); }

        public int IndexOf(
  ChoiceItem value
  )
        { return this.BaseDataObject.IndexOf(value.BaseObject); }

        public ChoiceItem Insert(
          int index,
          string value
          )
        {
            var item = new ChoiceItem(value);
            this.Insert(index, item);

            return item;
        }

        public void Insert(
          int index,
          ChoiceItem value
          )
        {
            this.BaseDataObject.Insert(index, value.BaseObject);
            value.Items = this;
        }

        public bool Remove(
          ChoiceItem value
          )
        { return this.BaseDataObject.Remove(value.BaseObject); }

        public void RemoveAt(
          int index
          )
        { this.BaseDataObject.RemoveAt(index); }

        public int Count => this.BaseDataObject.Count;

        public bool IsReadOnly => false;
    }
}