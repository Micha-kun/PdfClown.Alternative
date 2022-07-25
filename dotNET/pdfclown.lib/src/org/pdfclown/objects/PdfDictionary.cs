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


namespace org.pdfclown.objects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using org.pdfclown.bytes;
    using org.pdfclown.files;
    using org.pdfclown.tokens;
    using text = System.Text;

    /**
      <summary>PDF dictionary object [PDF:1.6:3.2.6].</summary>
    */
    public sealed class PdfDictionary
      : PdfDirectObject,
        IDictionary<PdfName, PdfDirectObject>
    {
        private static readonly byte[] BeginDictionaryChunk = Encoding.Pdf.Encode(Keyword.BeginDictionary);
        private static readonly byte[] EndDictionaryChunk = Encoding.Pdf.Encode(Keyword.EndDictionary);

        private PdfObject parent;
        private bool updateable = true;
        private bool updated;
        private bool virtual_;

        internal IDictionary<PdfName, PdfDirectObject> entries;

        /**
  <summary>Creates a new empty dictionary object with the default initial capacity.</summary>
*/
        public PdfDictionary(
          )
        { this.entries = new Dictionary<PdfName, PdfDirectObject>(); }

        /**
          <summary>Creates a new empty dictionary object with the specified initial capacity.</summary>
          <param name="capacity">Initial capacity.</param>
        */
        public PdfDictionary(
          int capacity
          )
        { this.entries = new Dictionary<PdfName, PdfDirectObject>(capacity); }

        /**
          <summary>Creates a new dictionary object with the specified entries.</summary>
          <param name="objects">Sequence of key/value-paired objects (where key is a <see
          cref="PdfName"/> and value is a <see cref="PdfDirectObject"/>).</param>
        */
        public PdfDictionary(
          params PdfDirectObject[] objects
          ) : this(objects.Length / 2)
        {
            this.Updateable = false;
            for (var index = 0; index < objects.Length;)
            { this[(PdfName)objects[index++]] = objects[index++]; }
            this.Updateable = true;
        }

        /**
          <summary>Creates a new dictionary object with the specified entries.</summary>
          <param name="entries">Map whose entries have to be added to this dictionary.</param>
        */
        public PdfDictionary(
          IDictionary<PdfName, PdfDirectObject> entries
          ) : this(entries.Count)
        {
            this.Updateable = false;
            foreach (var entry in entries)
            { this[entry.Key] = (PdfDirectObject)this.Include(entry.Value); }
            this.Updateable = true;
        }

        /**
          <summary>Creates a new dictionary object with the specified entries.</summary>
          <param name="keys">Entry keys to add to this dictionary.</param>
          <param name="values">Entry values to add to this dictionary; their position and number must
          match the <code>keys</code> argument.</param>
        */
        public PdfDictionary(
          PdfName[] keys,
          PdfDirectObject[] values
          ) : this(values.Length)
        {
            this.Updateable = false;
            for (
              var index = 0;
              index < values.Length;
              index++
              )
            { this[keys[index]] = values[index]; }
            this.Updateable = true;
        }

        public PdfDirectObject this[
          PdfName key
          ]
        {
            get
            {
                /*
                  NOTE: This is an intentional violation of the official .NET Framework Class
                  Library prescription (no exception is thrown anytime a key is not found --
                  a null pointer is returned instead).
                */
                PdfDirectObject value;
                _ = this.entries.TryGetValue(key, out value);
                return value;
            }
            set
            {
                if (value == null)
                { _ = this.Remove(key); }
                else
                {
                    var oldValue = this[key];
                    this.entries[key] = (PdfDirectObject)this.Include(value);
                    this.Exclude(oldValue);
                    this.Update();
                }
            }
        }

        void ICollection<KeyValuePair<PdfName, PdfDirectObject>>.Add(
  KeyValuePair<PdfName, PdfDirectObject> entry
  )
        { this.Add(entry.Key, entry.Value); }

        bool ICollection<KeyValuePair<PdfName, PdfDirectObject>>.Contains(
          KeyValuePair<PdfName, PdfDirectObject> entry
          )
        { return this.entries.Contains(entry); }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<PdfName, PdfDirectObject>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<PdfName, PdfDirectObject>> IEnumerable<KeyValuePair<PdfName, PdfDirectObject>>.GetEnumerator(
  )
        { return this.entries.GetEnumerator(); }

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
  PdfName key,
  PdfDirectObject value
  )
        {
            this.entries.Add(key, (PdfDirectObject)this.Include(value));
            this.Update();
        }

        public void Clear(
          )
        {
            foreach (PdfName key in new List<PdfDirectObject>(this.entries.Keys))
            { _ = this.Remove(key); }
        }

        public override int CompareTo(
          PdfDirectObject obj
          )
        { throw new NotImplementedException(); }

        public bool ContainsKey(
          PdfName key
          )
        { return this.entries.ContainsKey(key); }

        public void CopyTo(
          KeyValuePair<PdfName, PdfDirectObject>[] entries,
          int index
          )
        { throw new NotImplementedException(); }

        public override bool Equals(
          object @object
          )
        {
            return base.Equals(@object)
              || ((@object != null)
                && @object.GetType().Equals(this.GetType())
                && ((PdfDictionary)@object).entries.Equals(this.entries));
        }

        /**
          <summary>Gets the value corresponding to the given key, forcing its instantiation as a direct
          object in case of missing entry.</summary>
          <param name="key">Key whose associated value is to be returned.</param>
        */
        public PdfDirectObject Get<T>(
          PdfName key
          ) where T : PdfDataObject, new()
        { return this.Get<T>(key, true); }

        /**
          <summary>Gets the value corresponding to the given key, forcing its instantiation in case of
          missing entry.</summary>
          <param name="key">Key whose associated value is to be returned.</param>
          <param name="direct">Whether the item has to be instantiated directly within its container
          instead of being referenced through an indirect object.</param>
        */
        public PdfDirectObject Get<T>(
          PdfName key,
          bool direct
          ) where T : PdfDataObject, new()
        {
            var value = this[key];
            if (value == null)
            {
                /*
                  NOTE: The null-object placeholder MUST NOT perturb the existing structure; therefore:
                    - it MUST be marked as virtual in order not to unnecessarily serialize it;
                    - it MUST be put into this dictionary without affecting its update status.
                */
                try
                {
                    value = (PdfDirectObject)this.Include(direct
                      ? ((PdfDataObject)new T())
                      : new PdfIndirectObject(this.File, new T(), new XRefEntry(0, 0)).Reference);
                    this.entries[key] = value;
                    value.Virtual = true;
                }
                catch (Exception e)
                { throw new Exception($"{typeof(T).Name} failed to instantiate.", e); }
            }
            return value;
        }

        public override int GetHashCode(
          )
        { return this.entries.GetHashCode(); }

        /**
          Gets the key associated to the specified value.
        */
        public PdfName GetKey(
          PdfDirectObject value
          )
        {
            /*
              NOTE: Current PdfDictionary implementation doesn't support bidirectional maps, to say that
              the only currently-available way to retrieve a key from a value is to iterate the whole map
              (really poor performance!).
            */
            foreach (var entry in this.entries)
            {
                if (entry.Value.Equals(value))
                {
                    return entry.Key;
                }
            }
            return null;
        }

        public bool Remove(
          PdfName key
          )
        {
            var oldValue = this[key];
            if (this.entries.Remove(key))
            {
                this.Exclude(oldValue);
                this.Update();
                return true;
            }
            return false;
        }

        public bool Remove(
          KeyValuePair<PdfName, PdfDirectObject> entry
          )
        {
            if (entry.Value.Equals(this[entry.Key]))
            {
                return this.Remove(entry.Key);
            }
            else
            {
                return false;
            }
        }

        /**
          <summary>Gets the dereferenced value corresponding to the given key.</summary>
          <remarks>This method takes care to resolve the value returned by <see cref="this[PdfName]">
          this[PdfName]</see>.</remarks>
          <param name="key">Key whose associated value is to be returned.</param>
          <returns>null, if the map contains no mapping for this key.</returns>
        */
        public PdfDataObject Resolve(
          PdfName key
          )
        { return Resolve(this[key]); }

        /**
          <summary>Gets the dereferenced value corresponding to the given key, forcing its instantiation
          in case of missing entry.</summary>
          <remarks>This method takes care to resolve the value returned by <see cref="Get(PdfName)"/>.
          </remarks>
          <param name="key">Key whose associated value is to be returned.</param>
          <returns>null, if the map contains no mapping for this key.</returns>
        */
        public T Resolve<T>(
          PdfName key
          ) where T : PdfDataObject, new()
        { return (T)Resolve(this.Get<T>(key)); }

        public override PdfObject Swap(
          PdfObject other
          )
        {
            var otherDictionary = (PdfDictionary)other;
            var otherEntries = otherDictionary.entries;
            // Update the other!
            otherDictionary.entries = this.entries;
            otherDictionary.Update();
            // Update this one!
            this.entries = otherEntries;
            this.Update();
            return this;
        }

        public override string ToString(
          )
        {
            var buffer = new text::StringBuilder();
            // Begin.
            _ = buffer.Append("<< ");
            // Entries.
            foreach (var entry in this.entries)
            {
                // Entry...
                // ...key.
                _ = buffer.Append(entry.Key.ToString()).Append(" ");
                // ...value.
                _ = buffer.Append(PdfDirectObject.ToString(entry.Value)).Append(" ");
            }
            // End.
            _ = buffer.Append(">>");
            return buffer.ToString();
        }

        public bool TryGetValue(
          PdfName key,
          out PdfDirectObject value
          )
        { return this.entries.TryGetValue(key, out value); }

        public override void WriteTo(
          IOutputStream stream,
          File context
          )
        {
            // Begin.
            stream.Write(BeginDictionaryChunk);
            // Entries.
            foreach (var entry in this.entries)
            {
                var value = entry.Value;
                if ((value != null) && value.Virtual)
                {
                    continue;
                }

                // Entry...
                // ...key.
                entry.Key.WriteTo(stream, context);
                stream.Write(Chunk.Space);
                // ...value.
                PdfDirectObject.WriteTo(stream, context, value);
                stream.Write(Chunk.Space);
            }
            // End.
            stream.Write(EndDictionaryChunk);
        }

        public int Count => this.entries.Count;

        public bool IsReadOnly => false;

        public ICollection<PdfName> Keys => this.entries.Keys;

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

        public ICollection<PdfDirectObject> Values => this.entries.Values;
    }
}