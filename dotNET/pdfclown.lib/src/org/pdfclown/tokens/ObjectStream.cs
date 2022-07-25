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


namespace org.pdfclown.tokens
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using org.pdfclown.bytes;
    using org.pdfclown.files;
    using org.pdfclown.objects;

    /**
      <summary>Object stream containing a sequence of PDF objects [PDF:1.6:3.4.6].</summary>
      <remarks>The purpose of object streams is to allow a greater number of PDF objects
      to be compressed, thereby substantially reducing the size of PDF files.
      The objects in the stream are referred to as compressed objects.</remarks>
    */
    public sealed class ObjectStream
      : PdfStream,
        IDictionary<int, PdfDataObject>
    {

        /**
<summary>Compressed objects map.</summary>
<remarks>This map is initially populated with offset values;
when a compressed object is required, its offset is used to retrieve it.</remarks>
*/
        private IDictionary<int, ObjectEntry> entries;
        private FileParser parser;

        public ObjectStream(
  ) : base(new PdfDictionary(new PdfName[] { PdfName.Type }, new PdfDirectObject[] { PdfName.ObjStm }))
        { }

        public ObjectStream(
          PdfDictionary header,
          IBuffer body
          ) : base(header, body)
        { }

        public PdfDataObject this[
          int key
          ]
        {
            get
            {
                var entry = this.Entries[key];
                return (entry != null) ? entry.DataObject : null;
            }
            set => this.Entries[key] = new ObjectEntry(value, this.parser);
        }

        void ICollection<KeyValuePair<int, PdfDataObject>>.Add(
  KeyValuePair<int, PdfDataObject> entry
  )
        { this.Add(entry.Key, entry.Value); }

        bool ICollection<KeyValuePair<int, PdfDataObject>>.Contains(
          KeyValuePair<int, PdfDataObject> entry
          )
        { return ((ICollection<KeyValuePair<int, PdfDataObject>>)this.Entries).Contains(entry); }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<int, PdfDataObject>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<int, PdfDataObject>> IEnumerable<KeyValuePair<int, PdfDataObject>>.GetEnumerator(
  )
        {
            foreach (var key in this.Keys)
            { yield return new KeyValuePair<int, PdfDataObject>(key, this[key]); }
        }

        /**
          <summary>Serializes the object stream entries into the stream body.</summary>
        */
        private void Flush(
          IOutputStream stream
          )
        {
            // 1. Body.
            int dataByteOffset;
            // Serializing the entries into the stream buffer...
            IBuffer indexBuffer = new bytes.Buffer();
            IBuffer dataBuffer = new bytes.Buffer();
            var indirectObjects = this.File.IndirectObjects;
            var objectIndex = -1;
            var context = this.File;
            foreach (var entry in this.Entries)
            {
                var objectNumber = entry.Key;

                // Update the xref entry!
                var xrefEntry = indirectObjects[objectNumber].XrefEntry;
                xrefEntry.Offset = ++objectIndex;

                /*
                  NOTE: The entry offset MUST be updated only after its serialization, in order not to
                  interfere with its possible data-object retrieval from the old serialization.
                */
                var entryValueOffset = (int)dataBuffer.Length;

                // Index.
                _ = indexBuffer
                  .Append(objectNumber.ToString()).Append(Chunk.Space) // Object number.
                  .Append(entryValueOffset.ToString()).Append(Chunk.Space); // Byte offset (relative to the first one).

                // Data.
                entry.Value.DataObject.WriteTo(dataBuffer, context);
                entry.Value.offset = entryValueOffset;
            }

            // Get the stream buffer!
            var body = this.Body;

            // Delete the old entries!
            body.Clear();

            // Add the new entries!
            _ = body.Append(indexBuffer);
            dataByteOffset = (int)body.Length;
            _ = body.Append(dataBuffer);

            // 2. Header.

            var header = this.Header;
            header[PdfName.N] = PdfInteger.Get(this.Entries.Count);
            header[PdfName.First] = PdfInteger.Get(dataByteOffset);
        }

        private IDictionary<int, ObjectEntry> Entries
        {
            get
            {
                if (this.entries == null)
                {
                    this.entries = new Dictionary<int, ObjectEntry>();

                    var body = this.Body;
                    if (body.Length > 0)
                    {
                        this.parser = new FileParser(this.Body, this.File);
                        var baseOffset = ((PdfInteger)this.Header[PdfName.First]).IntValue;
                        for (
                          int index = 0,
                            length = ((PdfInteger)this.Header[PdfName.N]).IntValue;
                          index < length;
                          index++
                          )
                        {
                            var objectNumber = ((PdfInteger)this.parser.ParsePdfObject(1)).IntValue;
                            var objectOffset = baseOffset + ((PdfInteger)this.parser.ParsePdfObject(1)).IntValue;
                            this.entries[objectNumber] = new ObjectEntry(objectOffset, this.parser);
                        }
                    }
                }
                return this.entries;
            }
        }

        public override PdfObject Accept(
IVisitor visitor,
object data
)
        { return visitor.Visit(this, data); }

        public void Add(
  int key,
  PdfDataObject value
  )
        { this.Entries.Add(key, new ObjectEntry(value, this.parser)); }

        public void Clear(
          )
        {
            if (this.entries == null)
            { this.entries = new Dictionary<int, ObjectEntry>(); }
            else
            { this.entries.Clear(); }
        }

        public bool ContainsKey(
          int key
          )
        { return this.Entries.ContainsKey(key); }

        public void CopyTo(
          KeyValuePair<int, PdfDataObject>[] entries,
          int index
          )
        { throw new NotImplementedException(); }

        public bool Remove(
          int key
          )
        { return this.Entries.Remove(key); }

        public bool Remove(
          KeyValuePair<int, PdfDataObject> entry
          )
        {
            PdfDataObject value;
            if (this.TryGetValue(entry.Key, out value)
              && value.Equals(entry.Value))
            {
                return this.Entries.Remove(entry.Key);
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(
          int key,
          out PdfDataObject value
          )
        {
            value = this[key];
            return (value != null)
              || this.ContainsKey(key);
        }

        public override void WriteTo(
          IOutputStream stream,
          File context
          )
        {
            if (this.entries != null)
            { this.Flush(stream); }

            base.WriteTo(stream, context);
        }

        /**
          <summary>Gets/Sets the object stream extended by this one.</summary>
          <remarks>Both streams are considered part of a collection of object streams  whose links form
          a directed acyclic graph.</remarks>
        */
        public ObjectStream BaseStream
        {
            get => (ObjectStream)this.Header.Resolve(PdfName.Extends);
            set => this.Header[PdfName.Extends] = value.Reference;
        }

        public int Count => this.Entries.Count;

        public bool IsReadOnly => false;

        public ICollection<int> Keys => this.Entries.Keys;

        public ICollection<PdfDataObject> Values
        {
            get
            {
                IList<PdfDataObject> values = new List<PdfDataObject>();
                foreach (var key in this.Entries.Keys)
                { values.Add(this[key]); }
                return values;
            }
        }

        private sealed class ObjectEntry
        {

            private readonly FileParser parser;
            internal PdfDataObject dataObject;
            internal int offset;

            private ObjectEntry(
              FileParser parser
              )
            { this.parser = parser; }

            public ObjectEntry(
              int offset,
              FileParser parser
              ) : this(parser)
            {
                this.dataObject = null;
                this.offset = offset;
            }

            public ObjectEntry(
              PdfDataObject dataObject,
              FileParser parser
              ) : this(parser)
            {
                this.dataObject = dataObject;
                this.offset = -1; // Undefined -- to set on stream serialization.
            }

            public PdfDataObject DataObject
            {
                get
                {
                    if (this.dataObject == null)
                    {
                        this.parser.Seek(this.offset);
                        _ = this.parser.MoveNext();
                        this.dataObject = this.parser.ParsePdfObject();
                    }
                    return this.dataObject;
                }
            }
        }
    }
}