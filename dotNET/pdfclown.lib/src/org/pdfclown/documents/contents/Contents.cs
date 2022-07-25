/*
  Copyright 2007-2015 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.IO;
    using System.Text;
    using org.pdfclown.documents.contents.objects;

    using org.pdfclown.documents.contents.tokens;
    using org.pdfclown.objects;
    using org.pdfclown.util.io;
    using bytes = org.pdfclown.bytes;

    /**
      <summary>Content stream [PDF:1.6:3.7.1].</summary>
      <remarks>During its loading, this content stream is parsed and its instructions
      are exposed as a list; in case of modifications, it's user responsability
      to call the <see cref="Flush()"/> method in order to serialize back the instructions
      into this content stream.</remarks>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class Contents
      : PdfObjectWrapper<PdfDataObject>,
        IList<ContentObject>
    {

        private readonly IContentContext contentContext;

        private IList<ContentObject> items;

        private Contents(
  PdfDirectObject baseObject,
  IContentContext contentContext
  ) : base(baseObject)
        {
            this.contentContext = contentContext;
            this.Load();
        }

        public ContentObject this[
          int index
          ]
        {
            get => this.items[index];
            set => this.items[index] = value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return this.GetEnumerator(); }

        private void Load(
  )
        {
            var parser = new ContentParser(new ContentStream(this.BaseDataObject));
            this.items = parser.ParseContentObjects();
        }

        public void Add(
  ContentObject obj
  )
        { this.items.Add(obj); }

        public void Clear(
          )
        { this.items.Clear(); }

        public override object Clone(
Document context
)
        { throw new NotSupportedException(); }

        public bool Contains(
          ContentObject obj
          )
        { return this.items.Contains(obj); }

        public void CopyTo(
          ContentObject[] objs,
          int index
          )
        { this.items.CopyTo(objs, index); }

        /**
          <summary>Serializes the contents into the content stream.</summary>
        */
        public void Flush(
          )
        {
            PdfStream stream;
            var baseDataObject = this.BaseDataObject;
            // Are contents just a single stream object?
            if (baseDataObject is PdfStream) // Single stream.
            { stream = (PdfStream)baseDataObject; }
            else // Array of streams.
            {
                var streams = (PdfArray)baseDataObject;
                // No stream available?
                if (streams.Count == 0) // No stream.
                {
                    // Add first stream!
                    stream = new PdfStream();
                    streams.Add( // Inserts the new stream into the content stream.
                      this.File.Register(stream) // Inserts the new stream into the file.
                      );
                }
                else // Streams exist.
                {
                    // Eliminating exceeding streams...
                    /*
                      NOTE: Applications that consume or produce PDF files are not required to preserve
                      the existing structure of the Contents array [PDF:1.6:3.6.2].
                    */
                    while (streams.Count > 1)
                    {
                        this.File.Unregister((PdfReference)streams[1]); // Removes the exceeding stream from the file.
                        streams.RemoveAt(1); // Removes the exceeding stream from the content stream.
                    }
                    stream = (PdfStream)streams.Resolve(0);
                }
            }

            // Get the stream buffer!
            var buffer = stream.Body;
            // Delete old contents from the stream buffer!
            buffer.Clear();
            // Serializing the new contents into the stream buffer...
            var context = this.Document;
            foreach (var item in this.items)
            { item.WriteTo(buffer, context); }
        }

        public IEnumerator<ContentObject> GetEnumerator(
  )
        { return this.items.GetEnumerator(); }

        public int IndexOf(
  ContentObject obj
  )
        { return this.items.IndexOf(obj); }

        public void Insert(
          int index,
          ContentObject obj
          )
        { this.items.Insert(index, obj); }

        public bool Remove(
          ContentObject obj
          )
        { return this.items.Remove(obj); }

        public void RemoveAt(
          int index
          )
        { this.items.RemoveAt(index); }

        public static Contents Wrap(
PdfDirectObject baseObject,
IContentContext contentContext
)
        { return (baseObject != null) ? new Contents(baseObject, contentContext) : null; }

        public IContentContext ContentContext => this.contentContext;

        public int Count => this.items.Count;

        public bool IsReadOnly => false;
        /**
  <summary>Content stream wrapper.</summary>
*/
        private class ContentStream
          : bytes::IInputStream
        {
            private readonly PdfDataObject baseDataObject;

            /**
              Current stream base position (cumulative size of preceding streams).
            */
            private long basePosition;
            /**
              Current stream.
            */
            private bytes::IInputStream stream;
            /**
              Current stream index.
            */
            private int streamIndex = -1;

            public ContentStream(
              PdfDataObject baseDataObject
              )
            {
                this.baseDataObject = baseDataObject;
                _ = this.MoveNextStream();
            }

            /**
              <summary>Ensures stream availability, moving to the next stream in case the current one has
              run out of data.</summary>
            */
            private void EnsureStream(
              )
            {
                if (((this.stream == null)
                    || (this.stream.Position >= this.stream.Length))
                  && !this.MoveNextStream())
                {
                    throw new EndOfStreamException();
                }
            }

            private bool MoveNextStream(
              )
            {
                // Is the content stream just a single stream?
                /*
                  NOTE: A content stream may be made up of multiple streams [PDF:1.6:3.6.2].
                */
                if (this.baseDataObject is PdfStream) // Single stream.
                {
                    if (this.streamIndex < 1)
                    {
                        this.streamIndex++;

                        this.basePosition = (this.streamIndex == 0)
                          ? 0
                          : (this.basePosition + this.stream.Length);

                        this.stream = (this.streamIndex < 1)
                          ? ((PdfStream)this.baseDataObject).Body
                          : null;
                    }
                }
                else // Multiple streams.
                {
                    var streams = (PdfArray)this.baseDataObject;
                    if (this.streamIndex < streams.Count)
                    {
                        this.streamIndex++;

                        this.basePosition = (this.streamIndex == 0)
                          ? 0
                          : (this.basePosition + this.stream.Length);

                        this.stream = (this.streamIndex < streams.Count)
                          ? ((PdfStream)streams.Resolve(this.streamIndex)).Body
                          : null;
                    }
                }
                if (this.stream == null)
                {
                    return false;
                }

                this.stream.Seek(0);
                return true;
            }

            private bool MovePreviousStream(
              )
            {
                if (this.streamIndex == 0)
                {
                    this.streamIndex--;
                    this.stream = null;
                }
                if (this.streamIndex == -1)
                {
                    return false;
                }

                this.streamIndex--;
                /* NOTE: A content stream may be made up of multiple streams [PDF:1.6:3.6.2]. */
                // Is the content stream just a single stream?
                if (this.baseDataObject is PdfStream) // Single stream.
                {
                    this.stream = ((PdfStream)this.baseDataObject).Body;
                    this.basePosition = 0;
                }
                else // Array of streams.
                {
                    var streams = (PdfArray)this.baseDataObject;

                    this.stream = ((PdfStream)((PdfReference)streams[this.streamIndex]).DataObject).Body;
                    this.basePosition -= this.stream.Length;
                }

                return true;
            }

            public void Dispose(
              )
            {/* NOOP */}

            public void Read(
              byte[] data
              )
            { this.Read(data, 0, data.Length); }

            public void Read(
              byte[] data,
              int offset,
              int length
              )
            {
                while (length > 0)
                {
                    this.EnsureStream();
                    var readLength = Math.Min(length, (int)(this.stream.Length - this.stream.Position));
                    this.stream.Read(data, offset, readLength);
                    offset += readLength;
                    length -= readLength;
                }
            }

            public int ReadByte(
              )
            {
                //TODO:harmonize with other Read*() method EOF exceptions!!!
                try
                { this.EnsureStream(); }
                catch (EndOfStreamException)
                { return -1; }
                return this.stream.ReadByte();
            }

            public int ReadInt(
              )
            { throw new NotImplementedException(); }

            public int ReadInt(
              int length
              )
            { throw new NotImplementedException(); }

            public string ReadLine(
              )
            { throw new NotImplementedException(); }

            public short ReadShort(
              )
            { throw new NotImplementedException(); }

            public sbyte ReadSignedByte(
              )
            { throw new NotImplementedException(); }

            public string ReadString(
              int length
              )
            {
                var builder = new StringBuilder();
                while (length > 0)
                {
                    this.EnsureStream();
                    var readLength = Math.Min(length, (int)(this.stream.Length - this.stream.Position));
                    _ = builder.Append(this.stream.ReadString(readLength));
                    length -= readLength;
                }
                return builder.ToString();
            }

            public ushort ReadUnsignedShort(
              )
            { throw new NotImplementedException(); }

            public void Seek(
              long position
              )
            {
                if (position < 0)
                {
                    throw new ArgumentException("Negative positions cannot be sought.");
                }

                while (true)
                {
                    if (position < this.basePosition) //Before current stream.
                    { _ = this.MovePreviousStream(); }
                    else if (position > this.basePosition + this.stream.Length) // After current stream.
                    {
                        if (!this.MoveNextStream())
                        {
                            throw new EndOfStreamException();
                        }
                    }
                    else // At current stream.
                    {
                        this.stream.Seek(position - this.basePosition);
                        break;
                    }
                }
            }

            public void Skip(
              long offset
              )
            { this.Seek(this.Position + offset); }

            public byte[] ToByteArray(
              )
            { throw new NotImplementedException(); }

            public ByteOrderEnum ByteOrder
            {
                get => this.stream.ByteOrder;
                set => throw new NotSupportedException();
            }

            public long Length
            {
                get
                {
                    if (this.baseDataObject is PdfStream) // Single stream.
                    {
                        return ((PdfStream)this.baseDataObject).Body.Length;
                    }
                    else // Array of streams.
                    {
                        long length = 0;
                        foreach (var stream in (PdfArray)this.baseDataObject)
                        { length += ((PdfStream)((PdfReference)stream).DataObject).Body.Length; }
                        return length;
                    }
                }
            }

            public long Position => this.basePosition + this.stream.Position;
        }
    }
}
