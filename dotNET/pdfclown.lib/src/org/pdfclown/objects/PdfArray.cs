/*
  Copyright 2006-2013 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.objects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using org.pdfclown.bytes;

    using org.pdfclown.files;
    using org.pdfclown.tokens;
    using org.pdfclown.util.collections.generic;
    using text = System.Text;

    /**
      <summary>PDF array object, that is a one-dimensional collection of (possibly-heterogeneous)
      objects arranged sequentially [PDF:1.7:3.2.5].</summary>
    */
    public sealed class PdfArray
      : PdfDirectObject,
        IList<PdfDirectObject>
    {
        private static readonly byte[] BeginArrayChunk = Encoding.Pdf.Encode(Keyword.BeginArray);
        private static readonly byte[] EndArrayChunk = Encoding.Pdf.Encode(Keyword.EndArray);

        private PdfObject parent;
        private bool updateable = true;
        private bool updated;
        private bool virtual_;

        internal List<PdfDirectObject> items;

        public PdfArray(
  ) : this(10)
        { }

        public PdfArray(
          int capacity
          )
        { this.items = new List<PdfDirectObject>(capacity); }

        public PdfArray(
          params PdfDirectObject[] items
          ) : this(items.Length)
        {
            this.Updateable = false;
            this.AddAll(items);
            this.Updateable = true;
        }

        public PdfArray(
          IList<PdfDirectObject> items
          ) : this(items.Count)
        {
            this.Updateable = false;
            this.AddAll(items);
            this.Updateable = true;
        }

        public PdfDirectObject this[
          int index
          ]
        {
            get => this.items[index];
            set
            {
                var oldItem = this.items[index];
                this.items[index] = (PdfDirectObject)this.Include(value);
                this.Exclude(oldItem);
                this.Update();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return this.GetEnumerator(); }

        protected internal override bool Virtual
        {
            get => this.virtual_;
            set => this.virtual_ = value;
        }

        public override PdfObject Accept(
IVisitor visitor,
object data
)
        { return visitor.Visit(this, data); }

        public void Add(
  PdfDirectObject item
  )
        {
            this.items.Add((PdfDirectObject)this.Include(item));
            this.Update();
        }

        public void Clear(
          )
        {
            while (this.items.Count > 0)
            { this.RemoveAt(0); }
        }

        public override int CompareTo(
          PdfDirectObject obj
          )
        { throw new NotImplementedException(); }

        public bool Contains(
          PdfDirectObject item
          )
        { return this.items.Contains(item); }

        public void CopyTo(
          PdfDirectObject[] items,
          int index
          )
        { this.items.CopyTo(items, index); }

        public override bool Equals(
          object @object
          )
        {
            return base.Equals(@object)
              || ((@object != null)
                && @object.GetType().Equals(this.GetType())
                && ((PdfArray)@object).items.Equals(this.items));
        }

        /**
          <summary>Gets the value corresponding to the given index, forcing its instantiation as a direct
          object in case of missing entry.</summary>
          <param name="index">Index of the item to return.</param>
          <param name="itemClass">Class to use for instantiating the item in case of missing entry.</param>
        */
        public PdfDirectObject Get<T>(
          int index
          ) where T : PdfDataObject, new()
        { return this.Get<T>(index, true); }

        /**
          <summary>Gets the value corresponding to the given index, forcing its instantiation in case
          of missing entry.</summary>
          <param name="index">Index of the item to return.</param>
          <param name="direct">Whether the item has to be instantiated directly within its container
          instead of being referenced through an indirect object.</param>
        */
        public PdfDirectObject Get<T>(
          int index,
          bool direct
          ) where T : PdfDataObject, new()
        {
            PdfDirectObject item;
            if ((index == this.Count)
              || ((item = this[index]) == null)
              || !item.Resolve().GetType().Equals(typeof(T)))
            {
                /*
                  NOTE: The null-object placeholder MUST NOT perturb the existing structure; therefore:
                    - it MUST be marked as virtual in order not to unnecessarily serialize it;
                    - it MUST be put into this array without affecting its update status.
                */
                try
                {
                    item = (PdfDirectObject)this.Include(direct
                      ? ((PdfDataObject)new T())
                      : new PdfIndirectObject(this.File, new T(), new XRefEntry(0, 0)).Reference);
                    if (index == this.Count)
                    { this.items.Add(item); }
                    else if (item == null)
                    { this.items[index] = item; }
                    else
                    { this.items.Insert(index, item); }
                    item.Virtual = true;
                }
                catch (Exception e)
                { throw new Exception($"{typeof(T).Name} failed to instantiate.", e); }
            }
            return item;
        }

        public IEnumerator<PdfDirectObject> GetEnumerator(
  )
        { return this.items.GetEnumerator(); }

        public override int GetHashCode(
          )
        { return this.items.GetHashCode(); }

        public int IndexOf(
  PdfDirectObject item
  )
        { return this.items.IndexOf(item); }

        public void Insert(
          int index,
          PdfDirectObject item
          )
        {
            this.items.Insert(index, (PdfDirectObject)this.Include(item));
            this.Update();
        }

        public bool Remove(
          PdfDirectObject item
          )
        {
            if (!this.items.Remove(item))
            {
                return false;
            }

            this.Exclude(item);
            this.Update();
            return true;
        }

        public void RemoveAt(
          int index
          )
        {
            var oldItem = this.items[index];
            this.items.RemoveAt(index);
            this.Exclude(oldItem);
            this.Update();
        }

        /**
          <summary>Gets the dereferenced value corresponding to the given index.</summary>
          <remarks>This method takes care to resolve the value returned by
          <see cref="this[int]">this[int]</see>.</remarks>
          <param name="index">Index of the item to return.</param>
        */
        public PdfDataObject Resolve(
          int index
          )
        { return Resolve(this[index]); }

        /**
          <summary>Gets the dereferenced value corresponding to the given index, forcing its
          instantiation in case of missing entry.</summary>
          <remarks>This method takes care to resolve the value returned by
          <see cref="Get<T>">Get<T></see>.</remarks>
          <param name="index">Index of the item to return.</param>
        */
        public T Resolve<T>(
          int index
          ) where T : PdfDataObject, new()
        { return (T)Resolve(this.Get<T>(index)); }

        public override PdfObject Swap(
          PdfObject other
          )
        {
            var otherArray = (PdfArray)other;
            var otherItems = otherArray.items;
            // Update the other!
            otherArray.items = this.items;
            otherArray.Update();
            // Update this one!
            this.items = otherItems;
            this.Update();
            return this;
        }

        public override string ToString(
          )
        {
            var buffer = new text::StringBuilder();
            // Begin.
            _ = buffer.Append("[ ");
            // Elements.
            foreach (var item in this.items)
            { _ = buffer.Append(PdfDirectObject.ToString(item)).Append(" "); }
            // End.
            _ = buffer.Append("]");
            return buffer.ToString();
        }

        public override void WriteTo(
          IOutputStream stream,
          File context
          )
        {
            // Begin.
            stream.Write(BeginArrayChunk);
            // Elements.
            foreach (var item in this.items)
            {
                if ((item != null) && item.Virtual)
                {
                    continue;
                }

                PdfDirectObject.WriteTo(stream, context, item);
                stream.Write(Chunk.Space);
            }
            // End.
            stream.Write(EndArrayChunk);
        }

        public int Count => this.items.Count;

        public bool IsReadOnly => false;

        public override PdfObject Parent
        {
            get => this.parent;
            internal set => this.parent = value;
        }

        public override bool Updateable
        {
            get => this.updateable;
            set => this.updateable = value;
        }

        public override bool Updated
        {
            get => this.updated;
            protected internal set => this.updated = value;
        }
    }
}