/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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


namespace org.pdfclown.objects
{
    using System;
    using System.IO;
    using org.pdfclown.bytes;

    using org.pdfclown.util;
    using tokens = org.pdfclown.tokens;

    /**
      <summary>PDF string object [PDF:1.6:3.2.3].</summary>
      <remarks>
        <para>A string object consists of a series of bytes.</para>
        <para>String objects can be serialized in two ways:</para>
        <list type="bullet">
          <item>as a sequence of literal characters (plain form)</item>
          <item>as a sequence of hexadecimal digits (hexadecimal form)</item>
        </list>
      </remarks>
    */
    public class PdfString
      : PdfSimpleObject<byte[]>,
        IDataWrapper
    {

        private const byte BackspaceCode = 8;
        private const byte CarriageReturnCode = 13;
        private const byte FormFeedCode = 12;

        private const byte HexLeftDelimiterCode = 60;
        private const byte HexRightDelimiterCode = 62;
        private const byte HorizontalTabCode = 9;
        private const byte LineFeedCode = 10;
        private const byte LiteralEscapeCode = 92;
        private const byte LiteralLeftDelimiterCode = 40;
        private const byte LiteralRightDelimiterCode = 41;

        public static readonly PdfString Default = new PdfString(string.Empty);

        protected PdfString(
          )
        { }

        public PdfString(
  byte[] rawValue
  )
        { this.RawValue = rawValue; }

        public PdfString(
          string value
          )
        { this.Value = value; }

        public PdfString(
          byte[] rawValue,
          SerializationModeEnum serializationMode
          )
        {
            this.SerializationMode = serializationMode;
            this.RawValue = rawValue;
        }

        public PdfString(
          string value,
          SerializationModeEnum serializationMode
          )
        {
            this.SerializationMode = serializationMode;
            this.Value = value;
        }

        public override PdfObject Accept(
IVisitor visitor,
object data
)
        { return visitor.Visit(this, data); }

        public override int CompareTo(
          PdfDirectObject obj
          )
        {
            if (!(obj is PdfString))
            {
                throw new ArgumentException("Object MUST be a PdfString");
            }

            return string.CompareOrdinal(this.StringValue, ((PdfString)obj).StringValue);
        }

        public byte[] ToByteArray(
          )
        { return (byte[])this.RawValue.Clone(); }

        public override string ToString(
          )
        {
            switch (this.serializationMode)
            {
                case SerializationModeEnum.Hex:
                    return $"<{base.ToString()}>";
                case SerializationModeEnum.Literal:
                    return $"({base.ToString()})";
                default:
                    throw new NotImplementedException();
            }
        }

        public override void WriteTo(
          IOutputStream stream,
          files.File context
          )
        {
            var buffer = new MemoryStream();
            var rawValue = this.RawValue;
            switch (this.serializationMode)
            {
                case SerializationModeEnum.Literal:
                    buffer.WriteByte(LiteralLeftDelimiterCode);
                    /*
                      NOTE: Literal lexical conventions prescribe that the following reserved characters
                      are to be escaped when placed inside string character sequences:
                        - \n Line feed (LF)
                        - \r Carriage return (CR)
                        - \t Horizontal tab (HT)
                        - \b Backspace (BS)
                        - \f Form feed (FF)
                        - \( Left parenthesis
                        - \) Right parenthesis
                        - \\ Backslash
                    */
                    for (
                      var index = 0;
                      index < rawValue.Length;
                      index++
                      )
                    {
                        var valueByte = rawValue[index];
                        switch (valueByte)
                        {
                            case LineFeedCode:
                                buffer.WriteByte(LiteralEscapeCode);
                                valueByte = 110;
                                break;
                            case CarriageReturnCode:
                                buffer.WriteByte(LiteralEscapeCode);
                                valueByte = 114;
                                break;
                            case HorizontalTabCode:
                                buffer.WriteByte(LiteralEscapeCode);
                                valueByte = 116;
                                break;
                            case BackspaceCode:
                                buffer.WriteByte(LiteralEscapeCode);
                                valueByte = 98;
                                break;
                            case FormFeedCode:
                                buffer.WriteByte(LiteralEscapeCode);
                                valueByte = 102;
                                break;
                            case LiteralLeftDelimiterCode:
                            case LiteralRightDelimiterCode:
                            case LiteralEscapeCode:
                                buffer.WriteByte(LiteralEscapeCode);
                                break;
                        }
                        buffer.WriteByte(valueByte);
                    }
                    buffer.WriteByte(LiteralRightDelimiterCode);
                    break;
                case SerializationModeEnum.Hex:
                    buffer.WriteByte(HexLeftDelimiterCode);
                    var value = tokens::Encoding.Pdf.Encode(ConvertUtils.ByteArrayToHex(rawValue));
                    buffer.Write(value, 0, value.Length);
                    buffer.WriteByte(HexRightDelimiterCode);
                    break;
                default:
                    throw new NotImplementedException();
            }
            stream.Write(buffer.ToArray());
        }

        /**
          <summary>Gets/Sets the serialization mode.</summary>
        */
        public virtual SerializationModeEnum SerializationMode
        {
            get => this.serializationMode;
            set => this.serializationMode = value;
        }

        public string StringValue => (string)this.Value;

        public override object Value
        {
            get
            {
                switch (this.serializationMode)
                {
                    case SerializationModeEnum.Literal:
                        return tokens::Encoding.Pdf.Decode(this.RawValue);
                    case SerializationModeEnum.Hex:
                        return ConvertUtils.ByteArrayToHex(this.RawValue);
                    default:
                        throw new NotImplementedException($"{this.serializationMode} serialization mode is not implemented.");
                }
            }
            protected set
            {
                switch (this.serializationMode)
                {
                    case SerializationModeEnum.Literal:
                        this.RawValue = tokens::Encoding.Pdf.Encode((string)value);
                        break;
                    case SerializationModeEnum.Hex:
                        this.RawValue = ConvertUtils.HexToByteArray((string)value);
                        break;
                    default:
                        throw new NotImplementedException($"{this.serializationMode} serialization mode is not implemented.");
                }
            }
        }
        /*
          NOTE: String objects are internally represented as unescaped sequences of bytes.
          Escaping is applied on serialization only.
        */
        /**
  <summary>String serialization mode.</summary>
*/
        public enum SerializationModeEnum
        {
            /**
              Plain form.
            */
            Literal,
            /**
              Hexadecimal form.
            */
            Hex
        };

        private SerializationModeEnum serializationMode = SerializationModeEnum.Literal;
    }
}