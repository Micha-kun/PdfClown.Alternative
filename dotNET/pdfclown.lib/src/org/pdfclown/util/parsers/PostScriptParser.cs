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


namespace org.pdfclown.util.parsers
{
    using System;
    using System.Text;

    using org.pdfclown.bytes;
    using org.pdfclown.tokens;

    /**
      <summary>PostScript (non-procedural subset) parser [PS].</summary>
    */
    public class PostScriptParser
      : IDisposable
    {

        private IInputStream stream;

        private object token;
        private TokenTypeEnum tokenType;

        public PostScriptParser(
  IInputStream stream
  )
        { this.stream = stream; }

        public PostScriptParser(
          byte[] data
          )
        { this.stream = new org.pdfclown.bytes.Buffer(data); }

        ~PostScriptParser(
          )
        { this.Dispose(false); }

        protected virtual void Dispose(
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

        protected static int GetHex(
int c
)
        {
            if ((c >= '0') && (c <= '9'))
            {
                return c - '0';
            }
            else if ((c >= 'A') && (c <= 'F'))
            {
                return c - 'A' + 10;
            }
            else if ((c >= 'a') && (c <= 'f'))
            {
                return c - 'a' + 10;
            }
            else
            {
                return -1;
            }
        }

        /**
          <summary>Evaluate whether a character is a delimiter.</summary>
        */
        protected static bool IsDelimiter(
          int c
          )
        {
            return (c == Symbol.OpenRoundBracket)
              || (c == Symbol.CloseRoundBracket)
              || (c == Symbol.OpenAngleBracket)
              || (c == Symbol.CloseAngleBracket)
              || (c == Symbol.OpenSquareBracket)
              || (c == Symbol.CloseSquareBracket)
              || (c == Symbol.Slash)
              || (c == Symbol.Percent);
        }

        /**
          <summary>Evaluate whether a character is an EOL marker.</summary>
        */
        protected static bool IsEOL(
          int c
          )
        { return (c == 10) || (c == 13); }

        /**
          <summary>Evaluate whether a character is a white-space.</summary>
        */
        protected static bool IsWhitespace(
          int c
          )
        { return (c == 32) || IsEOL(c) || (c == 0) || (c == 9) || (c == 12); }

        public void Dispose(
  )
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override int GetHashCode(
)
        { return this.stream.GetHashCode(); }

        /**
          <summary>Gets a token after moving to the given offset.</summary>
          <param name="offset">Number of tokens to skip before reaching the intended one.</param>
          <seealso cref="Token"/>
        */
        public object GetToken(
          int offset
          )
        { _ = this.MoveNext(offset); return this.Token; }

        /**
          <summary>Moves the pointer to the next token.</summary>
          <remarks>To properly parse the current token, the pointer MUST be just before its starting
          (leading whitespaces are ignored). When this method terminates, the pointer IS
          at the last byte of the current token.</remarks>
          <returns>Whether a new token was found.</returns>
        */
        public virtual bool MoveNext(
          )
        {
            StringBuilder buffer = null;
            this.token = null;
            int c;

            // Skip leading white-space characters.
            do
            {
                c = this.stream.ReadByte();
                if (c == -1)
                {
                    return false;
                }
            } while (IsWhitespace(c)); // Keep goin' till there's a white-space character...

            // Which character is it?
            switch (c)
            {
                case Symbol.Slash: // Name.
                    this.tokenType = TokenTypeEnum.Name;

                    /*
                      NOTE: As name objects are simple symbols uniquely defined by sequences of characters,
                      the bytes making up the name are never treated as text, so here they are just
                      passed through without unescaping.
                    */
                    buffer = new StringBuilder();
                    while (true)
                    {
                        c = this.stream.ReadByte();
                        if (c == -1)
                        {
                            break; // NOOP.
                        }

                        if (IsDelimiter(c) || IsWhitespace(c))
                        {
                            break;
                        }

                        _ = buffer.Append((char)c);
                    }
                    if (c > -1)
                    { this.stream.Skip(-1); } // Restores the first byte after the current token.
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                case '-':
                case '+': // Number.
                    if (c == '.')
                    { this.tokenType = TokenTypeEnum.Real; }
                    else // Digit or signum.
                    { this.tokenType = TokenTypeEnum.Integer; } // By default (it may be real).

                    // Building the number...
                    buffer = new StringBuilder();
                    while (true)
                    {
                        _ = buffer.Append((char)c);
                        c = this.stream.ReadByte();
                        if (c == -1)
                        {
                            break; // NOOP.
                        }
                        else if (c == '.')
                        {
                            this.tokenType = TokenTypeEnum.Real;
                        }
                        else if ((c < '0') || (c > '9'))
                        {
                            break;
                        }
                    }
                    if (c > -1)
                    { this.stream.Skip(-1); } // Restores the first byte after the current token.
                    break;
                case Symbol.OpenSquareBracket: // Array (begin).
                    this.tokenType = TokenTypeEnum.ArrayBegin;
                    break;
                case Symbol.CloseSquareBracket: // Array (end).
                    this.tokenType = TokenTypeEnum.ArrayEnd;
                    break;
                case Symbol.OpenAngleBracket: // Dictionary (begin) | Hexadecimal string.
                    c = this.stream.ReadByte();
                    if (c == -1)
                    {
                        throw new PostScriptParseException("Isolated opening angle-bracket character.");
                    }
                    // Is it a dictionary (2nd angle bracket)?
                    if (c == Symbol.OpenAngleBracket)
                    {
                        this.tokenType = TokenTypeEnum.DictionaryBegin;
                        break;
                    }

                    // Hexadecimal string (single angle bracket).
                    this.tokenType = TokenTypeEnum.Hex;

                    buffer = new StringBuilder();
                    while (c != Symbol.CloseAngleBracket) // NOT string end.
                    {
                        if (!IsWhitespace(c))
                        { _ = buffer.Append((char)c); }

                        c = this.stream.ReadByte();
                        if (c == -1)
                        {
                            throw new PostScriptParseException("Malformed hex string.");
                        }
                    }
                    break;
                case Symbol.CloseAngleBracket: // Dictionary (end).
                    c = this.stream.ReadByte();
                    if (c == -1)
                    {
                        throw new PostScriptParseException("Malformed dictionary.");
                    }
                    else if (c != Symbol.CloseAngleBracket)
                    {
                        throw new PostScriptParseException("Malformed dictionary.", this);
                    }

                    this.tokenType = TokenTypeEnum.DictionaryEnd;
                    break;
                case Symbol.OpenRoundBracket: // Literal string.
                    this.tokenType = TokenTypeEnum.Literal;

                    buffer = new StringBuilder();
                    var level = 0;
                    while (true)
                    {
                        c = this.stream.ReadByte();
                        if (c == -1)
                        {
                            break;
                        }
                        else if (c == Symbol.OpenRoundBracket)
                        {
                            level++;
                        }
                        else if (c == Symbol.CloseRoundBracket)
                        {
                            level--;
                        }
                        else if (c == '\\')
                        {
                            var lineBreak = false;
                            c = this.stream.ReadByte();
                            switch (c)
                            {
                                case 'n':
                                    c = Symbol.LineFeed;
                                    break;
                                case 'r':
                                    c = Symbol.CarriageReturn;
                                    break;
                                case 't':
                                    c = '\t';
                                    break;
                                case 'b':
                                    c = '\b';
                                    break;
                                case 'f':
                                    c = '\f';
                                    break;
                                case Symbol.OpenRoundBracket:
                                case Symbol.CloseRoundBracket:
                                case '\\':
                                    break;
                                case Symbol.CarriageReturn:
                                    lineBreak = true;
                                    c = this.stream.ReadByte();
                                    if (c != Symbol.LineFeed)
                                    {
                                        this.stream.Skip(-1);
                                    }

                                    break;
                                case Symbol.LineFeed:
                                    lineBreak = true;
                                    break;
                                default:
                                    // Is it outside the octal encoding?
                                    if ((c < '0') || (c > '7'))
                                    {
                                        break;
                                    }

                                    // Octal.
                                    var octal = c - '0';
                                    c = this.stream.ReadByte();
                                    // Octal end?
                                    if ((c < '0') || (c > '7'))
                                    { c = octal; this.stream.Skip(-1); break; }
                                    octal = (octal << 3) + c - '0';
                                    c = this.stream.ReadByte();
                                    // Octal end?
                                    if ((c < '0') || (c > '7'))
                                    { c = octal; this.stream.Skip(-1); break; }
                                    octal = (octal << 3) + c - '0';
                                    c = octal & 0xff;
                                    break;
                            }
                            if (lineBreak)
                            {
                                continue;
                            }

                            if (c == -1)
                            {
                                break;
                            }
                        }
                        else if (c == Symbol.CarriageReturn)
                        {
                            c = this.stream.ReadByte();
                            if (c == -1)
                            {
                                break;
                            }
                            else if (c != Symbol.LineFeed)
                            { c = Symbol.LineFeed; this.stream.Skip(-1); }
                        }
                        if (level == -1)
                        {
                            break;
                        }

                        _ = buffer.Append((char)c);
                    }
                    if (c == -1)
                    {
                        throw new PostScriptParseException("Malformed literal string.");
                    }
                    break;
                case Symbol.Percent: // Comment.
                    this.tokenType = TokenTypeEnum.Comment;

                    buffer = new StringBuilder();
                    while (true)
                    {
                        c = this.stream.ReadByte();
                        if ((c == -1)
                          || IsEOL(c))
                        {
                            break;
                        }

                        _ = buffer.Append((char)c);
                    }
                    break;
                default: // Keyword.
                    this.tokenType = TokenTypeEnum.Keyword;

                    buffer = new StringBuilder();
                    do
                    {
                        _ = buffer.Append((char)c);
                        c = this.stream.ReadByte();
                        if (c == -1)
                        {
                            break;
                        }
                    } while (!IsDelimiter(c) && !IsWhitespace(c));
                    if (c > -1)
                    { this.stream.Skip(-1); } // Restores the first byte after the current token.
                    break;
            }

            if (buffer != null)
            {
                switch (this.tokenType)
                {
                    case TokenTypeEnum.Keyword:
                        this.token = buffer.ToString();
                        switch ((string)this.token)
                        {
                            case Keyword.False:
                            case Keyword.True: // Boolean.
                                this.tokenType = TokenTypeEnum.Boolean;
                                this.token = bool.Parse((string)this.token);
                                break;
                            case Keyword.Null: // Null.
                                this.tokenType = TokenTypeEnum.Null;
                                this.token = null;
                                break;
                        }
                        break;
                    case TokenTypeEnum.Name:
                    case TokenTypeEnum.Literal:
                    case TokenTypeEnum.Hex:
                    case TokenTypeEnum.Comment:
                        this.token = buffer.ToString();
                        break;
                    case TokenTypeEnum.Integer:
                        this.token = ConvertUtils.ParseIntInvariant(buffer.ToString());
                        break;
                    case TokenTypeEnum.Real:
                        this.token = ConvertUtils.ParseDoubleInvariant(buffer.ToString());
                        break;
                }
            }
            return true;
        }

        /**
          <summary>Moves the pointer to the next token.</summary>
          <param name="offset">Number of tokens to skip before reaching the intended one.</param>
        */
        public bool MoveNext(
          int offset
          )
        {
            for (
              var index = 0;
              index < offset;
              index++
              )
            {
                if (!this.MoveNext())
                {
                    return false;
                }
            }
            return true;
        }

        /**
          <summary>Moves the pointer to the given absolute byte position.</summary>
        */
        public void Seek(
          long offset
          )
        { this.stream.Seek(offset); }

        /**
          <summary>Moves the pointer to the given relative byte position.</summary>
        */
        public void Skip(
          long offset
          )
        { this.stream.Skip(offset); }

        /**
          <summary>Moves the pointer after the next end-of-line character sequence (that is just before
          the non-EOL character following the EOL sequence).</summary>
          <returns>Whether the stream can be further read.</returns>
        */
        public bool SkipEOL(
          )
        {
            int c;
            var found = false;
            while (true)
            {
                c = this.stream.ReadByte();
                if (c == -1)
                {
                    return false;
                }
                else if (IsEOL(c))
                { found = true; }
                else if (found) // After EOL.
                {
                    break;
                }
            }
            this.stream.Skip(-1); // Moves back to the first non-EOL character position (ready to read the next token).
            return true;
        }

        /**
          <summary>Moves the pointer after the current whitespace sequence (that is just before the
          non-whitespace character following the whitespace sequence).</summary>
          <returns>Whether the stream can be further read.</returns>
        */
        public bool SkipWhitespace(
          )
        {
            int c;
            do
            {
                c = this.stream.ReadByte();
                if (c == -1)
                {
                    return false;
                }
            } while (IsWhitespace(c)); // Keeps going till there's a whitespace character.
            this.stream.Skip(-1); // Moves back to the first non-whitespace character position (ready to read the next token).
            return true;
        }

        public long Length => this.stream.Length;

        public long Position => this.stream.Position;

        public IInputStream Stream => this.stream;

        /**
          <summary>Gets the currently-parsed token.</summary>
        */
        public object Token
        {
            get => this.token;
            protected set => this.token = value;
        }

        /**
          <summary>Gets the currently-parsed token type.</summary>
        */
        public TokenTypeEnum TokenType
        {
            get => this.tokenType;
            protected set => this.tokenType = value;
        }

        public enum TokenTypeEnum // [PS:3.3].
        {
            Keyword,
            Boolean,
            Integer,
            Real,
            Literal,
            Hex,
            Name,
            Comment,
            ArrayBegin,
            ArrayEnd,
            DictionaryBegin,
            DictionaryEnd,
            Null
        }
    }
}