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
    using System.Collections.Generic;
    using org.pdfclown.bytes;
    using org.pdfclown.files;
    using org.pdfclown.objects;
    using org.pdfclown.util.parsers;

    /**
      <summary>PDF file reader.</summary>
    */
    public sealed class Reader
      : IDisposable
    {

        private FileParser parser;

        internal Reader(
  IInputStream stream,
  File file
  )
        { this.parser = new FileParser(stream, file); }

        ~Reader(
          )
        { this.Dispose(false); }

        private void Dispose(
  bool disposing
  )
        {
            if (disposing)
            {
                if (this.parser != null)
                {
                    this.parser.Dispose();
                    this.parser = null;
                }
            }
        }

        public void Dispose(
  )
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override int GetHashCode(
)
        { return this.parser.GetHashCode(); }

        /**
          <summary>Retrieves the file information.</summary>
        */
        public FileInfo ReadInfo(
          )
        {
            //TODO:hybrid xref table/stream
            var version = pdfclown.Version.Get(this.parser.RetrieveVersion());
            PdfDictionary trailer = null;
            var xrefEntries = new SortedDictionary<int, XRefEntry>();
            var sectionOffset = this.parser.RetrieveXRefOffset();
            while (sectionOffset > -1)
            {
                // Move to the start of the xref section!
                this.parser.Seek(sectionOffset);

                PdfDictionary sectionTrailer;
                if (this.parser.GetToken(1).Equals(Keyword.XRef)) // XRef-table section.
                {
                    // Looping sequentially across the subsections inside the current xref-table section...
                    while (true)
                    {
                        /*
                          NOTE: Each iteration of this block represents the scanning of one subsection.
                          We get its bounds (first and last object numbers within its range) and then collect
                          its entries.
                        */
                        // 1. First object number.
                        _ = this.parser.MoveNext();
                        if ((this.parser.TokenType == PostScriptParser.TokenTypeEnum.Keyword)
                            && this.parser.Token.Equals(Keyword.Trailer)) // XRef-table section ended.
                        {
                            break;
                        }
                        else if (this.parser.TokenType != PostScriptParser.TokenTypeEnum.Integer)
                        {
                            throw new PostScriptParseException("Neither object number of the first object in this xref subsection nor end of xref section found.", this.parser);
                        }

                        // Get the object number of the first object in this xref-table subsection!
                        var startObjectNumber = (int)this.parser.Token;

                        // 2. Last object number.
                        _ = this.parser.MoveNext();
                        if (this.parser.TokenType != PostScriptParser.TokenTypeEnum.Integer)
                        {
                            throw new PostScriptParseException("Number of entries in this xref subsection not found.", this.parser);
                        }

                        // Get the object number of the last object in this xref-table subsection!
                        var endObjectNumber = ((int)this.parser.Token) + startObjectNumber;

                        // 3. XRef-table subsection entries.
                        for (
                          var index = startObjectNumber;
                          index < endObjectNumber;
                          index++
                          )
                        {
                            if (xrefEntries.ContainsKey(index)) // Already-defined entry.
                            {
                                // Skip to the next entry!
                                _ = this.parser.MoveNext(3);
                                continue;
                            }

                            // Get the indirect object offset!
                            var offset = (int)this.parser.GetToken(1);
                            // Get the object generation number!
                            var generation = (int)this.parser.GetToken(1);
                            // Get the usage tag!
                            XRefEntry.UsageEnum usage;
                            var usageToken = (string)this.parser.GetToken(1);
                            if (usageToken.Equals(Keyword.InUseXrefEntry))
                            {
                                usage = XRefEntry.UsageEnum.InUse;
                            }
                            else if (usageToken.Equals(Keyword.FreeXrefEntry))
                            {
                                usage = XRefEntry.UsageEnum.Free;
                            }
                            else
                            {
                                throw new PostScriptParseException("Invalid xref entry.", this.parser);
                            }

                            // Define entry!
                            xrefEntries[index] = new XRefEntry(
                              index,
                              generation,
                              offset,
                              usage
                              );
                        }
                    }

                    // Get the previous trailer!
                    sectionTrailer = (PdfDictionary)this.parser.ParsePdfObject(1);
                }
                else // XRef-stream section.
                {
                    var stream = (XRefStream)this.parser.ParsePdfObject(3); // Gets the xref stream skipping the indirect-object header.
                                                                            // XRef-stream subsection entries.
                    foreach (var xrefEntry in stream.Values)
                    {
                        if (xrefEntries.ContainsKey(xrefEntry.Number)) // Already-defined entry.
                        {
                            continue;
                        }

                        // Define entry!
                        xrefEntries[xrefEntry.Number] = xrefEntry;
                    }

                    // Get the previous trailer!
                    sectionTrailer = stream.Header;
                }

                if (trailer == null)
                { trailer = sectionTrailer; }

                // Get the previous xref-table section's offset!
                var prevXRefOffset = (PdfInteger)sectionTrailer[PdfName.Prev];
                sectionOffset = (prevXRefOffset != null) ? prevXRefOffset.IntValue : (-1);
            }
            return new FileInfo(version, trailer, xrefEntries);
        }

        public FileParser Parser => this.parser;

        public sealed class FileInfo
        {
            private readonly PdfDictionary trailer;
            private readonly pdfclown.Version version;
            private readonly SortedDictionary<int, XRefEntry> xrefEntries;

            internal FileInfo(
              pdfclown.Version version,
              PdfDictionary trailer,
              SortedDictionary<int, XRefEntry> xrefEntries
              )
            {
                this.version = version;
                this.trailer = trailer;
                this.xrefEntries = xrefEntries;
            }

            public PdfDictionary Trailer => this.trailer;

            public pdfclown.Version Version => this.version;

            public SortedDictionary<int, XRefEntry> XrefEntries => this.xrefEntries;
        }
    }
}
