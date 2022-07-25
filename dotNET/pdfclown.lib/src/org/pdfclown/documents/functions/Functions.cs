/*
  Copyright 2010-2011 Stefano Chizzolini. http://www.pdfclown.org

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


namespace org.pdfclown.documents.functions
{
    using System;

    using System.Collections;
    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>List of 1-input functions combined in a <see cref="Parent">stitching function</see> [PDF:1.6:3.9.3].</summary>
    */
    [PDF(VersionEnum.PDF13)]
    public sealed class Functions
      : PdfObjectWrapper<PdfArray>,
        IList<Function>
    {
        /**
<summary>Parent function.</summary>
*/
        private readonly Type3Function parent;

        internal Functions(
  PdfDirectObject baseObject,
  Type3Function parent
  ) : base(baseObject)
        { this.parent = parent; }

        public Function this[
          int index
          ]
        {
            get => Function.Wrap(this.BaseDataObject[index]);
            set
            {
                this.Validate(value);
                this.BaseDataObject[index] = value.BaseObject;
            }
        }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<Function>)this).GetEnumerator(); }

        IEnumerator<Function> IEnumerable<Function>.GetEnumerator(
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

        /**
  <summary>Checks whether the specified function is valid for insertion.</summary>
  <param name="value">Function to validate.</param>
*/
        private void Validate(
          Function value
          )
        {
            if (value.InputCount != 1)
            {
                throw new ArgumentException("value parameter MUST be 1-input function.");
            }
        }

        public void Add(
  Function value
  )
        {
            this.Validate(value);
            this.BaseDataObject.Add(value.BaseObject);
        }

        public void Clear(
          )
        { this.BaseDataObject.Clear(); }

        public override object Clone(
Document context
)
        { return new NotImplementedException(); }

        public bool Contains(
          Function value
          )
        { return this.BaseDataObject.Contains(value.BaseObject); }

        public void CopyTo(
          Function[] values,
          int index
          )
        { throw new NotImplementedException(); }

        public int IndexOf(
  Function value
  )
        { return this.BaseDataObject.IndexOf(value.BaseObject); }

        public void Insert(
          int index,
          Function value
          )
        {
            this.Validate(value);
            this.BaseDataObject.Insert(index, value.BaseObject);
        }

        public bool Remove(
          Function value
          )
        { return this.BaseDataObject.Remove(value.BaseObject); }

        public void RemoveAt(
          int index
          )
        { this.BaseDataObject.RemoveAt(index); }

        public int Count => this.BaseDataObject.Count;

        public bool IsReadOnly => false;

        /**
          <summary>Gets the parent stitching function.</summary>
        */
        public Type3Function Parent => this.parent;
    }
}