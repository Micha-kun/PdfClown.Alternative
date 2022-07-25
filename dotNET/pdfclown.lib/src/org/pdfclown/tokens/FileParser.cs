/*
  Copyright 2011-2015 Stefano Chizzolini. http://www.pdfclown.org

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
    using org.pdfclown.util.parsers;

    /**
      <summary>PDF file parser [PDF:1.7:3.2,3.4].</summary>
    */
    public sealed class FileParser
      : BaseParser
    {

        private static readonly int EOFMarkerChunkSize = 1024; // [PDF:1.6:H.3.18].

        private readonly File file;

        internal FileParser(
  IInputStream stream,
  File file
  ) : base(stream)
        { this.file = file; }

        public override bool MoveNext(
)
        {
            var moved = base.MoveNext();
            if (moved)
            {
                switch (this.TokenType)
                {
                    case TokenTypeEnum.Integer:
                        /*
                          NOTE: We need to verify whether indirect reference pattern is applicable:
                          ref :=  { int int 'R' }
                        */
                        var stream = this.Stream;
                        var baseOffset = stream.Position; // Backs up the recovery position.

                        // 1. Object number.
                        var objectNumber = (int)this.Token;
                        // 2. Generation number.
                        _ = base.MoveNext();
                        if (this.TokenType == TokenTypeEnum.Integer)
                        {
                            var generationNumber = (int)this.Token;
                            // 3. Reference keyword.
                            _ = base.MoveNext();
                            if ((this.TokenType == TokenTypeEnum.Keyword)
                              && this.Token.Equals(Keyword.Reference))
                            { this.Token = new Reference(objectNumber, generationNumber); }
                        }
                        if (!(this.Token is Reference))
                        {
                            // Rollback!
                            stream.Seek(baseOffset);
                            this.Token = objectNumber;
                            this.TokenType = TokenTypeEnum.Integer;
                        }
                        break;
                }
            }
            return moved;
        }

        public override PdfDataObject ParsePdfObject(
          )
        {
            switch (this.TokenType)
            {
                case TokenTypeEnum.Keyword:
                    if (this.Token is Reference)
                    {
                        var reference = (Reference)this.Token;
                        return new PdfReference(reference.ObjectNumber, reference.GenerationNumber, this.file);
                    }
                    break;
            }

            var pdfObject = base.ParsePdfObject();
            if (pdfObject is PdfDictionary)
            {
                var stream = this.Stream;
                var oldOffset = (int)stream.Position;
                _ = this.MoveNext();
                // Is this dictionary the header of a stream object [PDF:1.6:3.2.7]?
                if ((this.TokenType == TokenTypeEnum.Keyword)
                  && this.Token.Equals(Keyword.BeginStream))
                {
                    var streamHeader = (PdfDictionary)pdfObject;

                    // Keep track of current position!
                    /*
                      NOTE: Indirect reference resolution is an outbound call which affects the stream pointer position,
                      so we need to recover our current position after it returns.
                    */
                    var position = stream.Position;
                    // Get the stream length!
                    var length = ((PdfInteger)streamHeader.Resolve(PdfName.Length)).IntValue;
                    // Move to the stream data beginning!
                    stream.Seek(position);
                    _ = this.SkipEOL();

                    // Copy the stream data to the instance!
                    var data = new byte[length];
                    stream.Read(data);

                    _ = this.MoveNext(); // Postcondition (last token should be 'endstream' keyword).

                    object streamType = streamHeader[PdfName.Type];
                    if (PdfName.ObjStm.Equals(streamType)) // Object stream [PDF:1.6:3.4.6].
                    {
                        return new ObjectStream(
                          streamHeader,
                          new bytes.Buffer(data)
                          );
                    }
                    else if (PdfName.XRef.Equals(streamType)) // Cross-reference stream [PDF:1.6:3.4.7].
                    {
                        return new XRefStream(
                          streamHeader,
                          new bytes.Buffer(data)
                          );
                    }
                    else // Generic stream.
                    {
                        return new PdfStream(
                          streamHeader,
                          new bytes.Buffer(data)
                          );
                    }
                }
                else // Stand-alone dictionary.
                { stream.Seek(oldOffset); } // Restores postcondition (last token should be the dictionary end).
            }
            return pdfObject;
        }

        /**
          <summary>Parses the specified PDF indirect object [PDF:1.6:3.2.9].</summary>
          <param name="xrefEntry">Cross-reference entry of the indirect object to parse.</param>
        */
        public PdfDataObject ParsePdfObject(
          XRefEntry xrefEntry
          )
        {
            // Go to the beginning of the indirect object!
            this.Seek(xrefEntry.Offset);
            // Skip the indirect-object header!
            _ = this.MoveNext(4);

            // Empty indirect object?
            if ((this.TokenType == TokenTypeEnum.Keyword)
                && Keyword.EndIndirectObject.Equals(this.Token))
            {
                return null;
            }

            // Get the indirect data object!
            return this.ParsePdfObject();
        }

        /**
          <summary>Retrieves the PDF version of the file [PDF:1.6:3.4.1].</summary>
        */
        public string RetrieveVersion(
          )
        {
            var stream = this.Stream;
            stream.Seek(0);
            var header = stream.ReadString(10);
            if (!header.StartsWith(Keyword.BOF))
            {
                throw new PostScriptParseException("PDF header not found.", this);
            }

            return header.Substring(Keyword.BOF.Length, 3);
        }

        /**
          <summary>Retrieves the starting position of the last xref-table section [PDF:1.6:3.4.4].</summary>
        */
        public long RetrieveXRefOffset(
          )
        {
            // [FIX:69] 'startxref' keyword not found (file was corrupted by alien data in the tail).
            var stream = this.Stream;
            var streamLength = stream.Length;
            var position = streamLength;
            var chunkSize = (int)Math.Min(streamLength, EOFMarkerChunkSize);
            var index = -1;
            while ((index < 0) && (position > 0))
            {
                /*
                  NOTE: This condition prevents the keyword from being split by the chunk boundary.
                */
                if (position < streamLength)
                { position += Keyword.StartXRef.Length; }
                position -= chunkSize;
                if (position < 0)
                { position = 0; }
                stream.Seek(position);

                // Get 'startxref' keyword position!
                index = stream.ReadString(chunkSize).LastIndexOf(Keyword.StartXRef);
            }
            if (index < 0)
            {
                throw new PostScriptParseException("'" + Keyword.StartXRef + "' keyword not found.", this);
            }

            // Go past the 'startxref' keyword!
            stream.Seek(position + index);
            _ = this.MoveNext();

            // Get the xref offset!
            _ = this.MoveNext();
            if (this.TokenType != TokenTypeEnum.Integer)
            {
                throw new PostScriptParseException("'" + Keyword.StartXRef + "' value invalid.", this);
            }

            return (int)this.Token;
        }

        public struct Reference
        {
            public readonly int GenerationNumber;
            public readonly int ObjectNumber;

            internal Reference(
              int objectNumber,
              int generationNumber
              )
            {
                this.ObjectNumber = objectNumber;
                this.GenerationNumber = generationNumber;
            }
        }
    }
}