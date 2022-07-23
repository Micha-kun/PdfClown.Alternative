//-----------------------------------------------------------------------
// <copyright file="Buffer.cs" company="">
//     Copyright 2010-2012 Stefano Chizzolini. http://www.pdfclown.org
//     
//     Contributors:
//       * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
//     
//     This file should be part of the source code distribution of "PDF Clown library" (the
//     Program): see the accompanying README files for more info.
//     
//     This Program is free software; you can redistribute it and/or modify it under the terms
//     of the GNU Lesser General Public License as published by the Free Software Foundation;
//     either version 3 of the License, or (at your option) any later version.
//     
//     This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
//     either expressed or implied; without even the implied warranty of MERCHANTABILITY or
//     FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.
//     
//     You should have received a copy of the GNU Lesser General Public License along with this
//     Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).
//     
//     Redistribution and use, with or without modification, are permitted provided that such
//     redistributions retain the above copyright notice, license and disclaimer, along with
//     this list of conditions.
// </copyright>
//-----------------------------------------------------------------------
namespace org.pdfclown.bytes
{
    using System;
    using System.IO;
    using org.pdfclown.bytes.filters;
    using org.pdfclown.objects;
    using org.pdfclown.tokens;
    using org.pdfclown.util;
    using org.pdfclown.util.io;
    using text = System.Text;

    /// 
    /// <summary>
    /// Byte buffer.
    /// </summary>
    /// <remarks>
    /// TODO:IMPL Substitute System.Array static class invocations with System.Buffer static class invocations (better
    /// performance)!!!
    /// </remarks>
    /// 
    public sealed class Buffer : IBuffer
    {
        /// 
        /// <summary>
        /// Default buffer capacity.
        /// </summary>
        /// 
        private const int DefaultCapacity = 1 << 8;

        /// 
        /// <summary>
        /// Inner buffer where data are stored.
        /// </summary>
        /// 
        private byte[] data;

        /// 
        /// <summary>
        /// Number of bytes actually used in the buffer.
        /// </summary>
        /// 
        private int length;

        /// 
        /// <summary>
        /// Pointer position within the buffer.
        /// </summary>
        /// 
        private int position = 0;

        public Buffer() : this(0)
        {
        }

        public Buffer(int capacity)
        {
            if (capacity < 1)
            {
                capacity = DefaultCapacity;
            }

            this.data = new byte[capacity];
            this.length = 0;
        }

        public Buffer(byte[] data)
        {
            this.data = data;
            this.length = data.Length;
        }

        public Buffer(System.IO.Stream data) : this((int)data.Length) { _ = this.Append(data); }

        public Buffer(string data) : this() { _ = this.Append(data); }

        public event EventHandler OnChange;

        /// 
        /// <summary>
        /// Check whether the buffer has sufficient room for adding data.
        /// </summary>
        /// 
        private void EnsureCapacity(int additionalLength)
        {
            var minCapacity = this.length + additionalLength;
            // Is additional data within the buffer capacity?
            if (minCapacity <= this.data.Length)
            {
                return;
            }

            // Additional data exceed buffer capacity.
            // Reallocate the buffer!
            var data = new byte[
        Math.Max(
                this.data.Length << 1, // 1 order of magnitude greater than current capacity.
                minCapacity // Minimum capacity required.
          )
        ];
            Array.Copy(this.data, 0, data, 0, this.length);
            this.data = data;
        }

        private void NotifyChange()
        {
            if (this.Dirty || (this.OnChange == null))
            {
                return;
            }

            this.Dirty = true;
            this.OnChange(this, null);
        }

        public IBuffer Append(
            byte data
)
        {
            this.EnsureCapacity(1);
            this.data[this.length++] = data;
            this.NotifyChange();
            return this;
        }

        public IBuffer Append(byte[] data) { return this.Append(data, 0, data.Length); }

        public IBuffer Append(string data) { return this.Append(Encoding.Pdf.Encode(data)); }

        public IBuffer Append(IInputStream data) { return this.Append(data.ToByteArray(), 0, (int)data.Length); }

        public IBuffer Append(System.IO.Stream data)
        {
            var array = new byte[data.Length];
            data.Position = 0;
            _ = data.Read(array, 0, array.Length);
            return this.Append(array);
        }

        public IBuffer Append(byte[] data, int offset, int length)
        {
            this.EnsureCapacity(length);
            Array.Copy(data, offset, this.data, this.length, length);
            this.length += length;
            this.NotifyChange();
            return this;
        }

        public void Clear() { this.SetLength(0); }

        public IBuffer Clone()
        {
            IBuffer clone = new Buffer(this.Capacity);
            _ = clone.Append(this.data, 0, this.length);
            return clone;
        }

        public void Decode(Filter filter, PdfDictionary parameters)
        {
            this.data = filter.Decode(this.data, 0, this.length, parameters);
            this.length = this.data.Length;
        }

        public void Delete(int index, int length)
        {
            // Shift left the trailing data block to override the deleted data!
            Array.Copy(this.data, index + length, this.data, index, this.length - (index + length));
            this.length -= length;
            this.NotifyChange();
        }

        public void Dispose()
        {
        }

        public byte[] Encode(Filter filter, PdfDictionary parameters)
        { return filter.Encode(this.data, 0, this.length, parameters); }

        public int GetByte(int index) { return this.data[index]; }

        public byte[] GetByteArray(int index, int length)
        {
            var data = new byte[length];
            Array.Copy(this.data, index, data, 0, length);
            return data;
        }

        public string GetString(int index, int length) { return Encoding.Pdf.Decode(this.data, index, length); }

        public void Insert(int index, byte[] data) { this.Insert(index, data, 0, data.Length); }

        public void Insert(int index, string data) { this.Insert(index, Encoding.Pdf.Encode(data)); }

        public void Insert(int index, IInputStream data) { this.Insert(index, data.ToByteArray()); }

        public void Insert(int index, byte[] data, int offset, int length)
        {
            this.EnsureCapacity(length);
            // Shift right the existing data block to make room for new data!
            Array.Copy(this.data, index, this.data, index + length, this.length - index);
            // Insert additional data!
            Array.Copy(data, offset, this.data, index, length);
            this.length += length;
            this.NotifyChange();
        }

        public void Read(byte[] data) { this.Read(data, 0, data.Length); }

        public void Read(byte[] data, int offset, int length)
        {
            Array.Copy(this.data, this.position, data, offset, length);
            this.position += length;
        }

        public int ReadByte()
        {
            if (this.position >= this.data.Length)
            {
                return -1; //TODO:harmonize with other Read*() method EOF exceptions!!!
            }

            return this.data[this.position++];
        }

        public int ReadInt()
        {
            var value = ConvertUtils.ByteArrayToInt(this.data, this.position, this.ByteOrder);
            this.position += sizeof(int);
            return value;
        }

        public int ReadInt(int length)
        {
            var value = ConvertUtils.ByteArrayToNumber(this.data, this.position, length, this.ByteOrder);
            this.position += length;
            return value;
        }

        public string ReadLine()
        {
            if (this.position >= this.data.Length)
            {
                throw new EndOfStreamException();
            }

            var buffer = new text::StringBuilder();
            while (this.position < this.data.Length)
            {
                int c = this.data[this.position++];
                if ((c == '\r') || (c == '\n'))
                {
                    break;
                }

                _ = buffer.Append((char)c);
            }
            return buffer.ToString();
        }

        public short ReadShort()
        {
            var value = (short)ConvertUtils.ByteArrayToNumber(this.data, this.position, sizeof(short), this.ByteOrder);
            this.position += sizeof(short);
            return value;
        }

        public sbyte ReadSignedByte()
        {
            if (this.position >= this.data.Length)
            {
                throw new EndOfStreamException();
            }

            return (sbyte)this.data[this.position++];
        }

        public string ReadString(int length)
        {
            var data = Encoding.Pdf.Decode(this.data, this.position, length);
            this.position += length;
            return data;
        }

        public ushort ReadUnsignedShort()
        {
            var value = (ushort)ConvertUtils.ByteArrayToNumber(this.data, this.position, sizeof(ushort), this.ByteOrder);
            this.position += sizeof(ushort);
            return value;
        }

        public void Replace(int index, byte[] data)
        {
            Array.Copy(data, 0, this.data, index, data.Length);
            this.NotifyChange();
        }

        public void Replace(int index, string data) { this.Replace(index, Encoding.Pdf.Encode(data)); }

        public void Replace(int index, IInputStream data) { this.Replace(index, data.ToByteArray()); }

        public void Replace(int index, byte[] data, int offset, int length)
        {
            Array.Copy(data, offset, this.data, index, data.Length);
            this.NotifyChange();
        }

        public void Seek(long position)
        {
            if (position < 0)
            {
                position = 0;
            }
            else if (position > this.data.Length)
            {
                position = this.data.Length;
            }

            this.position = (int)position;
        }

        public void SetLength(int value)
        {
            this.length = value;
            this.NotifyChange();
        }

        public void Skip(long offset) { this.Seek(this.position + offset); }

        public byte[] ToByteArray()
        {
            var data = new byte[this.length];
            Array.Copy(this.data, 0, data, 0, this.length);
            return data;
        }

        public void Write(byte[] data) { _ = this.Append(data); }

        public void Write(string data) { _ = this.Append(data); }

        public void Write(IInputStream data) { _ = this.Append(data); }

        public void Write(byte[] data, int offset, int length) { _ = this.Append(data, offset, length); }

        public void WriteTo(IOutputStream stream) { stream.Write(this.data, 0, this.length); }

        public ByteOrderEnum ByteOrder { get; set; } = ByteOrderEnum.BigEndian;

        public int Capacity => this.data.Length;

        public bool Dirty { get; set; }

        public long Length => this.length;

        public long Position => this.position;
    }
}
