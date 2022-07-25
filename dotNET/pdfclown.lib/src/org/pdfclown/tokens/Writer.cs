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


namespace org.pdfclown.tokens
{
    using System;
    using org.pdfclown.bytes;
    using org.pdfclown.files;

    using org.pdfclown.objects;

    /**
      <summary>PDF file writer.</summary>
    */
    public abstract class Writer
    {
        private static readonly byte[] BOFChunk = Encoding.Pdf.Encode(Keyword.BOF);
        private static readonly byte[] EOFChunk = Encoding.Pdf.Encode(Symbol.LineFeed + Keyword.EOF + Symbol.CarriageReturn + Symbol.LineFeed);
        private static readonly byte[] HeaderBinaryHintChunk = new byte[] { (byte)Symbol.LineFeed, (byte)Symbol.Percent, 0x80, 0x80, 0x80, 0x80, (byte)Symbol.LineFeed }; // NOTE: Arbitrary binary characters (code >= 128) for ensuring proper behavior of file transfer applications [PDF:1.6:3.4.1].
        private static readonly byte[] StartXRefChunk = Encoding.Pdf.Encode(Keyword.StartXRef + Symbol.LineFeed);

        protected readonly File file;
        protected readonly IOutputStream stream;

        protected Writer(
  File file,
  IOutputStream stream
  )
        {
            this.file = file;
            this.stream = stream;
        }

        /**
  <summary>Updates the specified trailer.</summary>
  <remarks>This method has to be called just before serializing the trailer object.</remarks>
*/
        protected void UpdateTrailer(
          PdfDictionary trailer,
          IOutputStream stream
          )
        {
            // File identifier update.
            var identifier = FileIdentifier.Wrap(trailer[PdfName.ID]);
            if (identifier == null)
            { trailer[PdfName.ID] = (identifier = new FileIdentifier()).BaseObject; }
            identifier.Update(this);
        }

        /**
          <summary>Serializes the beginning of the file [PDF:1.6:3.4.1].</summary>
        */
        protected void WriteHeader(
          )
        {
            this.stream.Write(BOFChunk);
            this.stream.Write(this.file.Document.Version.ToString()); // NOTE: Document version represents the actual (possibly-overridden) file version.
            this.stream.Write(HeaderBinaryHintChunk);
        }

        /**
          <summary>Serializes the PDF file as incremental update [PDF:1.6:3.4.5].</summary>
        */
        protected abstract void WriteIncremental(
          );

        /**
          <summary>Serializes the PDF file linearized [PDF:1.6:F].</summary>
        */
        protected abstract void WriteLinearized(
          );

        /**
          <summary>Serializes the PDF file compactly [PDF:1.6:3.4].</summary>
        */
        protected abstract void WriteStandard(
          );

        /**
          <summary>Serializes the end of the file [PDF:1.6:3.4.4].</summary>
          <param name="startxref">Byte offset from the beginning of the file to the beginning
            of the last cross-reference section.</param>
        */
        protected void WriteTail(
          long startxref
          )
        {
            this.stream.Write(StartXRefChunk);
            this.stream.Write(startxref.ToString());
            this.stream.Write(EOFChunk);
        }

        /**
<summary>Gets a new writer instance for the specified file.</summary>
<param name="file">File to serialize.</param>
<param name="stream">Target stream.</param>
*/
        public static Writer Get(
          File file,
          IOutputStream stream
          )
        {
            // Which cross-reference table mode?
            switch (file.Configuration.XRefMode)
            {
                case XRefModeEnum.Plain:
                    return new PlainWriter(file, stream);
                case XRefModeEnum.Compressed:
                    return new CompressedWriter(file, stream);
                default:
                    throw new NotSupportedException();
            }
        }

        /**
          <summary>Serializes the <see cref="File">file</see> to the <see cref="Stream">target stream</see>.</summary>
          <param name="mode">Serialization mode.</param>
         */
        public void Write(
          SerializationModeEnum mode
          )
        {
            switch (mode)
            {
                case SerializationModeEnum.Incremental:
                    if (this.file.Reader == null)
                    {
                        goto case SerializationModeEnum.Standard;
                    }

                    this.WriteIncremental();
                    break;
                case SerializationModeEnum.Standard:
                    this.WriteStandard();
                    break;
                case SerializationModeEnum.Linearized:
                    this.WriteLinearized();
                    break;
            }
        }

        /**
<summary>Gets the file to serialize.</summary>
*/
        public File File => this.file;

        /**
          <summary>Gets the target stream.</summary>
        */
        public IOutputStream Stream => this.stream;
    }
}
