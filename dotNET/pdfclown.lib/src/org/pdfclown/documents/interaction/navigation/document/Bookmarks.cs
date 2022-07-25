/*
  Copyright 2006-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.navigation.document
{
    using System;

    using System.Collections;
    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Collection of bookmarks [PDF:1.6:8.2.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class Bookmarks
      : PdfObjectWrapper<PdfDictionary>,
        IList<Bookmark>
    {

        private Bookmarks(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public Bookmarks(
Document context
) : base(
context,
new PdfDictionary(
new PdfName[2]
{
            PdfName.Type,
            PdfName.Count
},
new PdfDirectObject[2]
{
            PdfName.Outlines,
            PdfInteger.Default
}
)
)
        { }

        public Bookmark this[
          int index
          ]
        {
            get
            {
                var bookmarkObject = (PdfReference)this.BaseDataObject[PdfName.First];
                while (index > 0)
                {
                    bookmarkObject = (PdfReference)((PdfDictionary)bookmarkObject.DataObject)[PdfName.Next];
                    // Did we go past the collection range?
                    if (bookmarkObject == null)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    index--;
                }

                return new Bookmark(bookmarkObject);
            }
            set => throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<Bookmark>)this).GetEnumerator(); }

        IEnumerator<Bookmark> IEnumerable<Bookmark>.GetEnumerator(
  )
        {
            var bookmarkObject = this.BaseDataObject[PdfName.First];
            if (bookmarkObject == null)
            {
                yield break;
            }

            do
            {
                yield return new Bookmark(bookmarkObject);

                bookmarkObject = ((PdfDictionary)bookmarkObject.Resolve())[PdfName.Next];
            } while (bookmarkObject != null);
        }

        /**
  <summary>Gets the count object, forcing its creation if it doesn't
  exist.</summary>
*/
        private PdfInteger EnsureCountObject(
          )
        {
            /*
              NOTE: The Count entry may be absent [PDF:1.6:8.2.2].
            */
            var countObject = (PdfInteger)this.BaseDataObject[PdfName.Count];
            if (countObject == null)
            { this.BaseDataObject[PdfName.Count] = countObject = PdfInteger.Default; }

            return countObject;
        }

        public void Add(
  Bookmark bookmark
  )
        {
            /*
              NOTE: Bookmarks imported from alien PDF files MUST be cloned
              before being added.
            */
            bookmark.BaseDataObject[PdfName.Parent] = this.BaseObject;

            var countObject = this.EnsureCountObject();
            // Is it the first bookmark?
            if ((int)countObject.Value == 0) // First bookmark.
            {
                this.BaseDataObject[PdfName.Last]
                  = this.BaseDataObject[PdfName.First]
                  = bookmark.BaseObject;
                this.BaseDataObject[PdfName.Count] = PdfInteger.Get(countObject.IntValue + 1);
            }
            else // Non-first bookmark.
            {
                var oldLastBookmarkReference = (PdfReference)this.BaseDataObject[PdfName.Last];
                this.BaseDataObject[PdfName.Last] // Added bookmark is the last in the collection...
                  = ((PdfDictionary)oldLastBookmarkReference.DataObject)[PdfName.Next] // ...and the next of the previously-last bookmark.
                  = bookmark.BaseObject;
                bookmark.BaseDataObject[PdfName.Prev] = oldLastBookmarkReference;

                /*
                  NOTE: The Count entry is a relative number (whose sign represents
                  the node open state).
                */
                this.BaseDataObject[PdfName.Count] = PdfInteger.Get(countObject.IntValue + Math.Sign(countObject.IntValue));
            }
        }

        public void Clear(
          )
        { throw new NotImplementedException(); }

        public bool Contains(
          Bookmark bookmark
          )
        { throw new NotImplementedException(); }

        public void CopyTo(
          Bookmark[] bookmarks,
          int index
          )
        { throw new NotImplementedException(); }

        public int IndexOf(
Bookmark bookmark
)
        { throw new NotImplementedException(); }

        public void Insert(
          int index,
          Bookmark bookmark
          )
        { throw new NotImplementedException(); }

        public bool Remove(
          Bookmark bookmark
          )
        { throw new NotImplementedException(); }

        public void RemoveAt(
          int index
          )
        { throw new NotImplementedException(); }
        public static Bookmarks Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new Bookmarks(baseObject) : null; }

        public int Count
        {
            get
            {
                /*
                  NOTE: The Count entry may be absent [PDF:1.6:8.2.2].
                */
                var countObject = (PdfInteger)this.BaseDataObject[PdfName.Count];
                if (countObject == null)
                {
                    return 0;
                }

                return countObject.RawValue;
            }
        }

        public bool IsReadOnly => false;
    }
}