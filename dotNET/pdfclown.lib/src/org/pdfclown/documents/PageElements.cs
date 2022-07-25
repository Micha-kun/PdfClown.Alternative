/*
  Copyright 2012-2015 Stefano Chizzolini. http://www.pdfclown.org

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
    using System;

    using org.pdfclown.objects;

    /**
      <summary>Page elements.</summary>
    */
    public abstract class PageElements<TItem>
      : Array<TItem>
      where TItem : PdfObjectWrapper<PdfDictionary>
    {
        private readonly Page page;

        internal PageElements(
  PdfDirectObject baseObject,
  Page page
  ) : base(baseObject)
        { this.page = page; }

        internal PageElements(
          IWrapper<TItem> itemWrapper,
          PdfDirectObject baseObject,
          Page page
        ) : base(itemWrapper, baseObject)
        { this.page = page; }

        private void DoAdd(
  TItem @object
  )
        {
            // Link the element to its page!
            @object.BaseDataObject[PdfName.P] = this.page.BaseObject;
        }

        private void DoRemove(
          TItem @object
          )
        {
            // Unlink the element from its page!
            _ = @object.BaseDataObject.Remove(PdfName.P);
        }

        public override void Add(
TItem @object
)
        {
            this.DoAdd(@object);
            base.Add(@object);
        }

        public override object Clone(
          Document context
          )
        { throw new NotSupportedException(); }

        public override void Insert(
          int index,
          TItem @object
          )
        {
            this.DoAdd(@object);
            base.Insert(index, @object);
        }

        public override bool Remove(
          TItem @object
          )
        {
            if (!base.Remove(@object))
            {
                return false;
            }

            this.DoRemove(@object);
            return true;
        }

        public override void RemoveAt(
          int index
          )
        {
            var @object = this[index];
            base.RemoveAt(index);
            this.DoRemove(@object);
        }

        /**
          <summary>Gets the page associated to these elements.</summary>
        */
        public Page Page => this.page;
    }
}