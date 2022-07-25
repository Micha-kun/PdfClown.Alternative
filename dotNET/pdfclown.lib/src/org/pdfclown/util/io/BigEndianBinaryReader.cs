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

namespace org.pdfclown.util.io
{
    using System;
    using System.IO;

    /**
      <summary>Reads primitive data types as binary values using big-endian encoding.</summary>
      <remarks>This implementation was necessary because the official framework's binary reader supports
      only the little-endian encoding.</remarks>
    */
    public class BigEndianBinaryReader
    {

        private bool disposed = false;
        private readonly Stream stream;

        public BigEndianBinaryReader(
  Stream stream
  )
        { this.stream = stream; }

        ~BigEndianBinaryReader(
          )
        { this.Dispose(false); }

        protected virtual void Dispose(
  bool disposing
  )
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            { this.stream.Dispose(); }
            this.disposed = true;
        }

        /**
          <summary>Closes the reader, including the underlying stream.</summary>
        */
        public void Close(
          )
        { this.Dispose(); }

        public void Dispose(
  )
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /**
          <summary>Reads a 2-byte signed integer from the current stream and advances the current position
          of the stream by two bytes.</summary>
        */
        public short ReadInt16(
          )
        { return (short)((this.stream.ReadByte() << 8) | this.stream.ReadByte()); }

        /**
          <summary>Reads a 4-byte signed integer from the current stream and advances the current position
          of the stream by four bytes.</summary>
        */
        public int ReadInt32(
          )
        { return (this.stream.ReadByte() << 24) | (this.stream.ReadByte() << 16) | (this.stream.ReadByte() << 8) | this.stream.ReadByte(); }

        /**
          <summary>Reads a 2-byte unsigned integer from the current stream and advances the position of the
          stream by two bytes.</summary>
        */
        public ushort ReadUInt16(
          )
        { return (ushort)((this.stream.ReadByte() << 8) | this.stream.ReadByte()); }

        /**
          <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the
          stream by four bytes.</summary>
        */
        public uint ReadUInt32(
          )
        { return (uint)((this.stream.ReadByte() << 24) | (this.stream.ReadByte() << 16) | (this.stream.ReadByte() << 8) | this.stream.ReadByte()); }

        /**
<summary>Gets the underlying stream.</summary>
*/
        public Stream BaseStream => this.stream;
    }
}