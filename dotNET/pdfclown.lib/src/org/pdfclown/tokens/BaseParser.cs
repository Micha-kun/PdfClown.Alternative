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
    using org.pdfclown.objects;

    using org.pdfclown.util.parsers;

    /**
      <summary>Base PDF parser [PDF:1.7:3.2].</summary>
    */
    public class BaseParser
      : PostScriptParser
    {
        protected BaseParser(
IInputStream stream
) : base(stream)
        { }

        protected BaseParser(
          byte[] data
          ) : base(data)
        { }

        public override bool MoveNext(
)
        {
            bool moved;
            while (moved = base.MoveNext())
            {
                var tokenType = this.TokenType;
                if (tokenType == TokenTypeEnum.Comment)
                {
                    continue; // Comments are ignored.
                }

                if (tokenType == TokenTypeEnum.Literal)
                {
                    var literalToken = (string)this.Token;
                    if (literalToken.StartsWith(Keyword.DatePrefix)) // Date.
                    {
                        /*
                          NOTE: Dates are a weak extension to the PostScript language.
                        */
                        try
                        { this.Token = PdfDate.ToDate(literalToken); }
                        catch (ParseException)
                        {/* NOOP: gently degrade to a common literal. */}
                    }
                }
                break;
            }
            return moved;
        }

        /**
          <summary>Parses the current PDF object [PDF:1.6:3.2].</summary>
        */
        public virtual PdfDataObject ParsePdfObject(
          )
        {
            switch (this.TokenType)
            {
                case TokenTypeEnum.Integer:
                    return PdfInteger.Get((int)this.Token);
                case TokenTypeEnum.Name:
                    return new PdfName((string)this.Token, true);
                case TokenTypeEnum.DictionaryBegin:
                    var dictionary = new PdfDictionary();
                    dictionary.Updateable = false;
                    while (true)
                    {
                        // Key.
                        _ = this.MoveNext();
                        if (this.TokenType == TokenTypeEnum.DictionaryEnd)
                        {
                            break;
                        }

                        var key = (PdfName)this.ParsePdfObject();
                        // Value.
                        _ = this.MoveNext();
                        var value = (PdfDirectObject)this.ParsePdfObject();
                        // Add the current entry to the dictionary!
                        dictionary[key] = value;
                    }
                    dictionary.Updateable = true;
                    return dictionary;
                case TokenTypeEnum.ArrayBegin:
                    var array = new PdfArray();
                    array.Updateable = false;
                    while (true)
                    {
                        // Value.
                        _ = this.MoveNext();
                        if (this.TokenType == TokenTypeEnum.ArrayEnd)
                        {
                            break;
                        }
                        // Add the current item to the array!
                        array.Add((PdfDirectObject)this.ParsePdfObject());
                    }
                    array.Updateable = true;
                    return array;
                case TokenTypeEnum.Literal:
                    if (this.Token is DateTime)
                    {
                        return PdfDate.Get((DateTime)this.Token);
                    }
                    else
                    {
                        return new PdfTextString(
                          Encoding.Pdf.Encode((string)this.Token)
                          );
                    }

                case TokenTypeEnum.Hex:
                    return new PdfTextString(
                      (string)this.Token,
                      PdfString.SerializationModeEnum.Hex
                      );
                case TokenTypeEnum.Real:
                    return PdfReal.Get((double)this.Token);
                case TokenTypeEnum.Boolean:
                    return PdfBoolean.Get((bool)this.Token);
                case TokenTypeEnum.Null:
                    return null;
                default:
                    throw new PostScriptParseException($"Unknown type beginning: '{this.Token}'", this);
            }
        }

        /**
          <summary>Parses a PDF object after moving to the given token offset.</summary>
          <param name="offset">Number of tokens to skip before reaching the intended one.</param>
          <seealso cref="ParsePdfObject()"/>
        */
        public PdfDataObject ParsePdfObject(
          int offset
          )
        {
            _ = this.MoveNext(offset);
            return this.ParsePdfObject();
        }
    }
}

