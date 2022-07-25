/*
  Copyright 2006-2012 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.Text;
    using org.pdfclown.bytes;
    using org.pdfclown.files;
    using org.pdfclown.objects;

    /**
      <summary>PDF file writer implementing classic cross-reference table [PDF:1.6:3.4.3].</summary>
    */
    internal sealed class PlainWriter
      : Writer
    {

        private const string XRefGenerationFormat = "00000";
        private const string XRefOffsetFormat = "0000000000";

        private static readonly byte[] TrailerChunk = Encoding.Pdf.Encode(Keyword.Trailer + Symbol.LineFeed);
        private static readonly string XRefChunk = Keyword.XRef + Symbol.LineFeed;
        private static readonly string XRefEOLChunk = $"{Symbol.CarriageReturn}{Symbol.LineFeed}";

        internal PlainWriter(
File file,
IOutputStream stream
) : base(file, stream)
        { }

        private StringBuilder AppendXRefEntry(
  StringBuilder xrefBuilder,
  PdfReference reference,
  long offset
  )
        {
            string usage;
            switch (reference.IndirectObject.XrefEntry.Usage)
            {
                case XRefEntry.UsageEnum.Free:
                    usage = Keyword.FreeXrefEntry;
                    break;
                case XRefEntry.UsageEnum.InUse:
                    usage = Keyword.InUseXrefEntry;
                    break;
                default: // Should NEVER happen.
                    throw new NotSupportedException();
            }
            return xrefBuilder.Append(offset.ToString(XRefOffsetFormat)).Append(Symbol.Space)
              .Append(reference.GenerationNumber.ToString(XRefGenerationFormat)).Append(Symbol.Space)
              .Append(usage).Append(XRefEOLChunk);
        }

        /**
          <summary>Appends the cross-reference subsection to the specified builder.</summary>
          <param name="xrefBuilder">Target builder.</param>
          <param name="firstObjectNumber">Object number of the first object in the subsection.</param>
          <param name="entryCount">Number of entries in the subsection.</param>
          <param name="xrefSubBuilder">Cross-reference subsection entries.</param>
        */
        private StringBuilder AppendXRefSubsection(
          StringBuilder xrefBuilder,
          int firstObjectNumber,
          int entryCount,
          StringBuilder xrefSubBuilder
          )
        { return this.AppendXRefSubsectionIndexer(xrefBuilder, firstObjectNumber, entryCount).Append(xrefSubBuilder); }

        /**
          <summary>Appends the cross-reference subsection indexer to the specified builder.</summary>
          <param name="xrefBuilder">Target builder.</param>
          <param name="firstObjectNumber">Object number of the first object in the subsection.</param>
          <param name="entryCount">Number of entries in the subsection.</param>
        */
        private StringBuilder AppendXRefSubsectionIndexer(
          StringBuilder xrefBuilder,
          int firstObjectNumber,
          int entryCount
          )
        { return xrefBuilder.Append(firstObjectNumber).Append(Symbol.Space).Append(entryCount).Append(Symbol.LineFeed); }

        /**
          <summary>Serializes the file trailer [PDF:1.6:3.4.4].</summary>
          <param name="startxref">Byte offset from the beginning of the file to the beginning
            of the last cross-reference section.</param>
          <param name="xrefSize">Total number of entries in the file's cross-reference table,
            as defined by the combination of the original section and all update sections.</param>
          <param name="parser">File parser.</param>
        */
        private void WriteTrailer(
          long startxref,
          int xrefSize,
          FileParser parser
          )
        {
            // 1. Header.
            this.stream.Write(TrailerChunk);

            // 2. Body.
            // Update its entries:
            var trailer = this.file.Trailer;
            this.UpdateTrailer(trailer, this.stream);
            // * Size
            trailer[PdfName.Size] = PdfInteger.Get(xrefSize);
            // * Prev
            if (parser == null)
            { _ = trailer.Remove(PdfName.Prev); } // [FIX:0.0.4:5] It (wrongly) kept the 'Prev' entry of multiple-section xref tables.
            else
            { trailer[PdfName.Prev] = PdfInteger.Get((int)parser.RetrieveXRefOffset()); }
            // Serialize its contents!
            trailer.WriteTo(this.stream, this.file);
            this.stream.Write(Chunk.LineFeed);

            // 3. Tail.
            this.WriteTail(startxref);
        }

        protected override void WriteIncremental(
)
        {
            // 1. Original content (head, body and previous trailer).
            var parser = this.file.Reader.Parser;
            this.stream.Write(parser.Stream);

            // 2. Body update (modified indirect objects insertion).
            var xrefSize = this.file.IndirectObjects.Count;
            var xrefBuilder = new StringBuilder(XRefChunk);
            /*
              NOTE: Incremental xref table comprises multiple sections
              each one composed by multiple subsections; this update
              adds a new section.
            */
            var xrefSubBuilder = new StringBuilder(); // Xref-table subsection builder.
            var xrefSubCount = 0; // Xref-table subsection counter.
            var prevKey = 0; // Previous-entry object number.
            foreach (
              var indirectObjectEntry
                in this.file.IndirectObjects.ModifiedObjects
              )
            {
                // Is the object in the current subsection?
                /*
                  NOTE: To belong to the current subsection, the object entry MUST be contiguous with the
                  previous (condition 1) or the iteration has to have been just started (condition 2).
                */
                if ((indirectObjectEntry.Key - prevKey == 1)
                  || (prevKey == 0)) // Current subsection continues.
                { xrefSubCount++; }
                else // Current subsection terminates.
                {
                    // End current subsection!
                    _ = this.AppendXRefSubsection(
                      xrefBuilder,
                      prevKey - xrefSubCount + 1,
                      xrefSubCount,
                      xrefSubBuilder
                      );

                    // Begin next subsection!
                    xrefSubBuilder.Length = 0;
                    xrefSubCount = 1;
                }

                prevKey = indirectObjectEntry.Key;

                // Current entry insertion.
                if (indirectObjectEntry.Value.IsInUse()) // In-use entry.
                {
                    // Add in-use entry!
                    _ = this.AppendXRefEntry(
                      xrefSubBuilder,
                      indirectObjectEntry.Value.Reference,
                      this.stream.Length
                      );
                    // Add in-use entry content!
                    indirectObjectEntry.Value.WriteTo(this.stream, this.file);
                }
                else // Free entry.
                {
                    // Add free entry!
                    /*
                      NOTE: We purposely neglect the linked list of free entries (see IndirectObjects.remove(int)),
                      so that this entry links directly back to object number 0, having a generation number of 65535
                      (not reusable) [PDF:1.6:3.4.3].
                    */
                    _ = this.AppendXRefEntry(
                      xrefSubBuilder,
                      indirectObjectEntry.Value.Reference,
                      0
                      );
                }
            }
            // End last subsection!
            _ = this.AppendXRefSubsection(
              xrefBuilder,
              prevKey - xrefSubCount + 1,
              xrefSubCount,
              xrefSubBuilder
              );

            // 3. XRef-table last section.
            var startxref = this.stream.Length;
            this.stream.Write(xrefBuilder.ToString());

            // 4. Trailer.
            this.WriteTrailer(startxref, xrefSize, parser);
        }

        protected override void WriteLinearized(
          )
        { throw new NotImplementedException(); }

        protected override void WriteStandard(
          )
        {
            // 1. Header [PDF:1.6:3.4.1].
            this.WriteHeader();

            // 2. Body [PDF:1.6:3.4.2].
            var xrefSize = this.file.IndirectObjects.Count;
            var xrefBuilder = new StringBuilder(XRefChunk);
            /*
              NOTE: A standard xref table comprises just one section composed by just one subsection.
              NOTE: As xref-table free entries MUST be arrayed as a linked list,
              it's needed to cache intermingled in-use entries in order to properly render
              the object number of the next free entry inside the previous one.
            */
            _ = this.AppendXRefSubsectionIndexer(xrefBuilder, 0, xrefSize);

            var xrefInUseBlockBuilder = new StringBuilder();
            var indirectObjects = this.file.IndirectObjects;
            var freeReference = indirectObjects[0].Reference; // Initialized to the first free entry.
            for (
              var index = 1;
              index < xrefSize;
              index++
              )
            {
                // Current entry insertion.
                var indirectObject = indirectObjects[index];
                if (indirectObject.IsInUse()) // In-use entry.
                {
                    // Add in-use entry!
                    _ = this.AppendXRefEntry(
                      xrefInUseBlockBuilder,
                      indirectObject.Reference,
                      this.stream.Length
                      );
                    // Add in-use entry content!
                    indirectObject.WriteTo(this.stream, this.file);
                }
                else // Free entry.
                {
                    // Add free entry!
                    _ = this.AppendXRefEntry(
                      xrefBuilder,
                      freeReference,
                      index
                      );

                    // End current block!
                    _ = xrefBuilder.Append(xrefInUseBlockBuilder);

                    // Initialize next block!
                    xrefInUseBlockBuilder.Length = 0;
                    freeReference = indirectObject.Reference;
                }
            }
            // Add last free entry!
            _ = this.AppendXRefEntry(
              xrefBuilder,
              freeReference,
              0
              );

            // End last block!
            _ = xrefBuilder.Append(xrefInUseBlockBuilder);

            // 3. XRef table (unique section) [PDF:1.6:3.4.3].
            var startxref = this.stream.Length;
            this.stream.Write(xrefBuilder.ToString());

            // 4. Trailer [PDF:1.6:3.4.4].
            this.WriteTrailer(startxref, xrefSize, null);
        }
    }
}