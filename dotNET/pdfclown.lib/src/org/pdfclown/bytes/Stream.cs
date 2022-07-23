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

namespace org.pdfclown.bytes
{
    using System;
    using System.IO;
    using org.pdfclown.tokens;
    using org.pdfclown.util;
    using org.pdfclown.util.io;
    using text = System.Text;

    /**
      <summary>Generic stream.</summary>
    */
    public sealed class Stream
    : IInputStream,
      IOutputStream
    {

        private ByteOrderEnum byteOrder = ByteOrderEnum.BigEndian;
        private System.IO.Stream stream;

        public Stream(
  System.IO.Stream stream
  )
        { this.stream = stream; }

        ~Stream(
          )
        { this.Dispose(false); }

        private void Dispose(
  bool disposing
  )
        {
            if (disposing)
            {
                if (this.stream != null)
                {
                    this.stream.Dispose();
                    this.stream = null;
                }
            }
        }

        public void Clear(
  )
        { this.stream.SetLength(0); }

        public void Dispose(
)
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override int GetHashCode(
      )
        { return this.stream.GetHashCode(); }

        public void Read(
      byte[] data
      )
        { _ = this.stream.Read(data, 0, data.Length); }

        public void Read(
          byte[] data,
          int offset,
          int count
          )
        { _ = this.stream.Read(data, offset, count); }

        public int ReadByte(
          )
        { return this.stream.ReadByte(); }

        public int ReadInt(
          )
        {
            var data = new byte[sizeof(int)];
            this.Read(data);
            return ConvertUtils.ByteArrayToInt(data, 0, this.byteOrder);
        }

        public int ReadInt(
          int length
          )
        {
            var data = new byte[length];
            this.Read(data);
            return ConvertUtils.ByteArrayToNumber(data, 0, length, this.byteOrder);
        }

        public string ReadLine(
          )
        {
            var buffer = new text::StringBuilder();
            while (true)
            {
                var c = this.stream.ReadByte();
                if (c == -1)
                {
                    if (buffer.Length == 0)
                    {
                        return null;
                    }
                    else
                    {
                        break;
                    }
                }
                else if ((c == '\r')
          || (c == '\n'))
                {
                    break;
                }

                _ = buffer.Append((char)c);
            }
            return buffer.ToString();
        }

        public short ReadShort(
          )
        {
            var data = new byte[sizeof(short)];
            this.Read(data);
            return (short)ConvertUtils.ByteArrayToNumber(data, 0, data.Length, this.byteOrder);
        }

        public sbyte ReadSignedByte(
          )
        { throw new NotImplementedException(); }

        public string ReadString(
          int length
          )
        {
            var buffer = new text::StringBuilder();
            int c;

            while (length-- > 0)
            {
                c = this.stream.ReadByte();
                if (c == -1)
                {
                    break;
                }

                _ = buffer.Append((char)c);
            }

            return buffer.ToString();
        }

        public ushort ReadUnsignedShort(
          )
        {
            var data = new byte[sizeof(ushort)];
            this.Read(data);
            return (ushort)ConvertUtils.ByteArrayToNumber(data, 0, data.Length, this.byteOrder);
        }

        public void Seek(
          long offset
          )
        { _ = this.stream.Seek(offset, SeekOrigin.Begin); }

        public void Skip(
          long offset
          )
        { _ = this.stream.Seek(offset, SeekOrigin.Current); }

        public byte[] ToByteArray(
  )
        {
            var data = new byte[this.stream.Length];
            this.stream.Position = 0;
            _ = this.stream.Read(data, 0, data.Length);
            return data;
        }

        public void Write(
          byte[] data
          )
        { this.stream.Write(data, 0, data.Length); }

        public void Write(
          string data
          )
        { this.Write(Encoding.Pdf.Encode(data)); }

        public void Write(
          IInputStream data
          )
        {
            // TODO:IMPL bufferize!!!
            var baseData = new byte[data.Length];
            // Force the source pointer to the BOF (as we must copy the entire content)!
            data.Seek(0);
            // Read source content!
            data.Read(baseData, 0, baseData.Length);
            // Write target content!
            this.Write(baseData);
        }

        public void Write(
          byte[] data,
          int offset,
          int length
          )
        { this.stream.Write(data, offset, length); }

        public ByteOrderEnum ByteOrder
        {
            get => this.byteOrder;
            set => this.byteOrder = value;
        }

        public long Length => this.stream.Length;

        public long Position
        {
            get => this.stream.Position;
            set => this.stream.Position = value;
        }
    }
}
