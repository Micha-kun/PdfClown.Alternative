/*
  Copyright 2011-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
    * Stephen Cleary (bug reporter [FIX:51], https://sourceforge.net/u/stephencleary/)

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


namespace org.pdfclown.documents.contents.tokens
{
    using System.Collections.Generic;
    using org.pdfclown.documents.contents.objects;
    using org.pdfclown.objects;
    using org.pdfclown.tokens;

    using org.pdfclown.util.parsers;
    using bytes = org.pdfclown.bytes;
    using sysIO = System.IO;

    /**
      <summary>Content stream parser [PDF:1.6:3.7.1].</summary>
    */
    public sealed class ContentParser
      : BaseParser
    {
        internal ContentParser(
bytes::IInputStream stream
) : base(stream)
        { }

        public ContentParser(
          byte[] data
          ) : base(data)
        { }

        private InlineImage ParseInlineImage(
  )
        {
            /*
              NOTE: Inline images use a peculiar syntax that's an exception to the usual rule
              that the data in a content stream is interpreted according to the standard PDF syntax
              for objects.
            */
            InlineImageHeader header;
            var operands = new List<PdfDirectObject>();
            // Parsing the image entries...
            while (this.MoveNext()
              && (this.TokenType != TokenTypeEnum.Keyword)) // Not keyword (i.e. end at image data beginning (ID operator)).
            { operands.Add((PdfDirectObject)this.ParsePdfObject()); }
            header = new InlineImageHeader(operands);

            InlineImageBody body;
            // [FIX:51,74] Wrong 'EI' token handling on inline image parsing.
            var stream = this.Stream;
            _ = stream.ReadByte(); // Should be the whitespace following the 'ID' token.
            var data = new bytes::Buffer();
            var endChunkBuffer = new sysIO::MemoryStream(3);
            var endChunkIndex = -1;
            while (true)
            {
                var curByte = stream.ReadByte();
                if (curByte == -1)
                {
                    throw new PostScriptParseException("No 'EI' token found to close inline image data stream.");
                }

                if (endChunkIndex == -1)
                {
                    if (IsWhitespace(curByte))
                    {
                        /*
                          NOTE: Whitespace characters may announce the beginning of the end image operator.
                        */
                        endChunkBuffer.WriteByte((byte)curByte);
                        endChunkIndex++;
                    }
                    else
                    { _ = data.Append((byte)curByte); }
                }
                else if ((endChunkIndex == 0) && IsWhitespace(curByte))
                {
                    /*
                      NOTE: Only the last whitespace character may announce the beginning of the end image
                      operator.
                    */
                    _ = data.Append(endChunkBuffer.ToArray());
                    endChunkBuffer.SetLength(0);
                    endChunkBuffer.WriteByte((byte)curByte);
                }
                else if (((endChunkIndex == 0) && (curByte == 'E'))
                  || ((endChunkIndex == 1) && (curByte == 'I')))
                {
                    /*
                      NOTE: End image operator characters.
                    */
                    endChunkBuffer.WriteByte((byte)curByte);
                    endChunkIndex++;
                }
                else if ((endChunkIndex == 2) && IsWhitespace(curByte))
                {
                    /*
 NOTE: The whitespace character after the end image operator completes the pattern.
*/
                    break;
                }
                else
                {
                    if (endChunkIndex > -1)
                    {
                        _ = data.Append(endChunkBuffer.ToArray());
                        endChunkBuffer.SetLength(0);
                        endChunkIndex = -1;
                    }
                    _ = data.Append((byte)curByte);
                }
            }
            body = new InlineImageBody(data);

            return new InlineImage(
              header,
              body
              );
        }

        private Path ParsePath(
          Operation beginOperation
          )
        {
            /*
              NOTE: Paths do not have an explicit end operation, so we must infer it
              looking for the first non-painting operation.
            */
            IList<ContentObject> operations = new List<ContentObject>();
            operations.Add(beginOperation);
            var position = this.Position;
            var closeable = false;
            while (this.MoveNext())
            {
                var operation = this.ParseOperation();
                // Multiple-operation graphics object closeable?
                if (operation is PaintPath) // Painting operation.
                { closeable = true; }
                else if (closeable) // Past end (first non-painting operation).
                {
                    this.Seek(position); // Rolls back to the last path-related operation.

                    break;
                }

                operations.Add(operation);
                position = this.Position;
            }
            return new Path(operations);
        }

        /**
<summary>Parses the next content object [PDF:1.6:4.1].</summary>
*/
        public ContentObject ParseContentObject(
          )
        {
            var operation = this.ParseOperation();
            if (operation is PaintXObject) // External object.
            {
                return new XObject((PaintXObject)operation);
            }
            else if (operation is PaintShading) // Shading.
            {
                return new Shading((PaintShading)operation);
            }
            else if ((operation is BeginSubpath)
              || (operation is DrawRectangle)) // Path.
            {
                return this.ParsePath(operation);
            }
            else if (operation is BeginText) // Text.
            {
                return new Text(
                  this.ParseContentObjects()
                  );
            }
            else if (operation is SaveGraphicsState) // Local graphics state.
            {
                return new LocalGraphicsState(
                  this.ParseContentObjects()
                  );
            }
            else if (operation is BeginMarkedContent) // Marked-content sequence.
            {
                return new MarkedContent(
                  (BeginMarkedContent)operation,
                  this.ParseContentObjects()
                  );
            }
            else if (operation is BeginInlineImage) // Inline image.
            {
                return this.ParseInlineImage();
            }
            else // Single operation.
            {
                return operation;
            }
        }

        /**
          <summary>Parses the next content objects.</summary>
        */
        public IList<ContentObject> ParseContentObjects(
          )
        {
            var contentObjects = new List<ContentObject>();
            while (this.MoveNext())
            {
                var contentObject = this.ParseContentObject();
                // Multiple-operation graphics object end?
                if ((contentObject is EndText) // Text.
                  || (contentObject is RestoreGraphicsState) // Local graphics state.
                  || (contentObject is EndMarkedContent) // End marked-content sequence.
                  || (contentObject is EndInlineImage)) // Inline image.
                {
                    return contentObjects;
                }

                contentObjects.Add(contentObject);
            }
            return contentObjects;
        }

        /**
          <summary>Parses the next operation.</summary>
        */
        public Operation ParseOperation(
          )
        {
            string @operator = null;
            var operands = new List<PdfDirectObject>();
            // Parsing the operation parts...
            do
            {
                switch (this.TokenType)
                {
                    case TokenTypeEnum.Keyword:
                        @operator = (string)this.Token;
                        break;
                    default:
                        operands.Add((PdfDirectObject)this.ParsePdfObject());
                        break;
                }
            } while ((@operator == null) && this.MoveNext());
            return Operation.Get(@operator, operands);
        }

        public override PdfDataObject ParsePdfObject(
          )
        {
            switch (this.TokenType)
            {
                case TokenTypeEnum.Literal:
                    if (this.Token is string)
                    {
                        return new PdfByteString(Encoding.Pdf.Encode((string)this.Token));
                    }

                    break;
                case TokenTypeEnum.Hex:
                    return new PdfByteString((string)this.Token);
            }
            return base.ParsePdfObject();
        }
    }
}