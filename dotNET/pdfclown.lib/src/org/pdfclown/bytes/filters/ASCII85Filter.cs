//-----------------------------------------------------------------------
// <copyright file="ASCII85Filter.cs" company="">
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
namespace org.pdfclown.bytes.filters
{
    using System;
    using System.IO;
    using System.Text;
    using org.pdfclown.objects;

    /// <summary>
    /// ASCII base-85 filter [PDF:1.6:3.3.2].
    /// </summary>
    [PDF(VersionEnum.PDF10)]
    public sealed class ASCII85Filter : Filter
    {
        private const int AsciiOffset = 33;

        /// <summary>
        /// Add the Prefix and Suffix marks when encoding, and enforce their presence for decoding.
        /// </summary>
        private const bool EnforceMarks = true;

        /// <summary>
        /// Maximum line length for encoded ASCII85 string; set to zero for one unbroken line.
        /// </summary>
        private const int LineLength = 75;
        /// <summary>
        /// Prefix mark that identifies an encoded ASCII85 string.
        /// </summary>
        private const string PrefixMark = "<~";
        /// <summary>
        /// Suffix mark that identifies an encoded ASCII85 string.
        /// </summary>
        private const string SuffixMark = "~>";

        private static readonly uint[] Pow85 = { 85 * 85 * 85 * 85, 85 * 85 * 85, 85 * 85, 85, 1 };

        internal ASCII85Filter()
        {
        }

        private static void AppendChar(
            StringBuilder buffer,
            char data,
            ref int linePos
)
        {
            _ = buffer.Append(data);
            linePos++;
            if ((LineLength > 0) && (linePos >= LineLength))
            {
                linePos = 0;
                _ = buffer.Append('\n');
            }
        }

        private static void AppendString(StringBuilder buffer, string data, ref int linePos)
        {
            if ((LineLength > 0) && (linePos + data.Length > LineLength))
            {
                linePos = 0;
                _ = buffer.Append('\n');
            }
            else
            {
                linePos += data.Length;
            }
            _ = buffer.Append(data);
        }

        private static void DecodeBlock(byte[] decodedBlock, ref uint tuple)
        { DecodeBlock(decodedBlock, decodedBlock.Length, ref tuple); }

        private static void DecodeBlock(byte[] decodedBlock, int count, ref uint tuple)
        {
            for (var i = 0; i < count; i++)
            {
                decodedBlock[i] = (byte)(tuple >> (24 - (i * 8)));
            }
        }

        private static void EncodeBlock(byte[] encodedBlock, StringBuilder buffer, ref uint tuple, ref int linePos)
        { EncodeBlock(encodedBlock, encodedBlock.Length, buffer, ref tuple, ref linePos); }

        private static void EncodeBlock(
            byte[] encodedBlock,
            int count,
            StringBuilder buffer,
            ref uint tuple,
            ref int linePos)
        {
            for (var i = encodedBlock.Length - 1; i >= 0; i--)
            {
                encodedBlock[i] = (byte)((tuple % 85) + AsciiOffset);
                tuple /= 85;
            }

            for (var i = 0; i < count; i++)
            {
                AppendChar(buffer, (char)encodedBlock[i], ref linePos);
            }
        }

        public override byte[] Decode(
            byte[] data,
            int offset,
            int length,
            PdfDictionary parameters
)
        {
            var decodedBlock = new byte[4];
            var encodedBlock = new byte[5];
            uint tuple = 0;

            var dataString = Encoding.ASCII.GetString(data).Trim();

            // Stripping prefix and suffix...
            if (dataString.StartsWith(PrefixMark))
            {
                dataString = dataString.Substring(PrefixMark.Length);
            }
            if (dataString.EndsWith(SuffixMark))
            {
                dataString = dataString.Substring(0, dataString.Length - SuffixMark.Length);
            }

            var stream = new MemoryStream();
            var count = 0;
            foreach (var dataChar in dataString)
            {
                bool processChar;
                switch (dataChar)
                {
                    case 'z':
                        if (count != 0)
                        {
                            throw new Exception("The character 'z' is invalid inside an ASCII85 block.");
                        }

                        decodedBlock[0] = 0;
                        decodedBlock[1] = 0;
                        decodedBlock[2] = 0;
                        decodedBlock[3] = 0;
                        stream.Write(decodedBlock, 0, decodedBlock.Length);
                        processChar = false;
                        break;
                    case '\n':
                    case '\r':
                    case '\t':
                    case '\0':
                    case '\f':
                    case '\b':
                        processChar = false;
                        break;
                    default:
                        if ((dataChar < '!') || (dataChar > 'u'))
                        {
                            throw new Exception(
                                $"Bad character '{dataChar}' found. ASCII85 only allows characters '!' to 'u'.");
                        }

                        processChar = true;
                        break;
                }

                if (processChar)
                {
                    tuple += ((uint)(dataChar - AsciiOffset)) * Pow85[count];
                    count++;
                    if (count == encodedBlock.Length)
                    {
                        DecodeBlock(decodedBlock, ref tuple);
                        stream.Write(decodedBlock, 0, decodedBlock.Length);
                        tuple = 0;
                        count = 0;
                    }
                }
            }

            // Bytes left over at the end?
            if (count != 0)
            {
                if (count == 1)
                {
                    throw new Exception("The last block of ASCII85 data cannot be a single byte.");
                }

                count--;
                tuple += Pow85[count];
                DecodeBlock(decodedBlock, count, ref tuple);
                for (var i = 0; i < count; i++)
                {
                    stream.WriteByte(decodedBlock[i]);
                }
            }

            return stream.ToArray();
        }

        public override byte[] Encode(byte[] data, int offset, int length, PdfDictionary parameters)
        {
            var decodedBlock = new byte[4];
            var encodedBlock = new byte[5];

            var buffer = new StringBuilder(data.Length * (encodedBlock.Length / decodedBlock.Length));
            var linePos = 0;

            if (EnforceMarks)
            {
                AppendString(buffer, PrefixMark, ref linePos);
            }

            var count = 0;
            uint tuple = 0;
            foreach (var dataByte in data)
            {
                if (count >= decodedBlock.Length - 1)
                {
                    tuple |= dataByte;
                    if (tuple == 0)
                    {
                        AppendChar(buffer, 'z', ref linePos);
                    }
                    else
                    {
                        EncodeBlock(encodedBlock, buffer, ref tuple, ref linePos);
                    }
                    tuple = 0;
                    count = 0;
                }
                else
                {
                    tuple |= (uint)(dataByte << (24 - (count * 8)));
                    count++;
                }
            }

            // if we have some bytes left over at the end..
            if (count > 0)
            {
                EncodeBlock(encodedBlock, count + 1, buffer, ref tuple, ref linePos);
            }

            if (EnforceMarks)
            {
                AppendString(buffer, SuffixMark, ref linePos);
            }

            return ASCIIEncoding.UTF8.GetBytes(buffer.ToString());
        }
    }
}
