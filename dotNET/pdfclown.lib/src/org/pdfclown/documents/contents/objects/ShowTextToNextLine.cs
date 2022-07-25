/*
  Copyright 2009-2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.contents.objects
{

    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>'Move to the next line and show a text string' operation [PDF:1.6:5.3.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class ShowTextToNextLine
      : ShowText
    {
        /**
<summary>Specifies no text state parameter
(just uses the current settings).</summary>
*/
        public static readonly string SimpleOperatorKeyword = "'";
        /**
          <summary>Specifies the word spacing and the character spacing
          (setting the corresponding parameters in the text state).</summary>
        */
        public static readonly string SpaceOperatorKeyword = "''";

        /**
<param name="text">Text encoded using current font's encoding.</param>
*/
        public ShowTextToNextLine(
          byte[] text
          ) : base(
            SimpleOperatorKeyword,
            new PdfByteString(text)
            )
        { }

        public ShowTextToNextLine(
          string @operator,
          IList<PdfDirectObject> operands
          ) : base(
            @operator,
            operands
            )
        { }

        /**
          <param name="text">Text encoded using current font's encoding.</param>
          <param name="wordSpace">Word spacing.</param>
          <param name="charSpace">Character spacing.</param>
        */
        public ShowTextToNextLine(
          byte[] text,
          double wordSpace,
          double charSpace
          ) : base(
            SpaceOperatorKeyword,
            PdfReal.Get(wordSpace),
            PdfReal.Get(charSpace),
            new PdfByteString(text)
            )
        { }

        private void EnsureSpaceOperation(
  )
        {
            if (this.@operator.Equals(SimpleOperatorKeyword))
            {
                this.@operator = SpaceOperatorKeyword;
                this.operands.Insert(0, PdfReal.Get(0));
                this.operands.Insert(1, PdfReal.Get(0));
            }
        }

        /**
<summary>Gets/Sets the character spacing.</summary>
*/
        public double? CharSpace
        {
            get
            {
                if (this.@operator.Equals(SimpleOperatorKeyword))
                {
                    return null;
                }
                else
                {
                    return ((IPdfNumber)this.operands[1]).RawValue;
                }
            }
            set
            {
                this.EnsureSpaceOperation();
                this.operands[1] = PdfReal.Get(value.Value);
            }
        }

        public override byte[] Text
        {
            get => ((PdfString)this.operands[
                  this.@operator.Equals(SimpleOperatorKeyword) ? 0 : 2
                  ]).RawValue;
            set => this.operands[
                  this.@operator.Equals(SimpleOperatorKeyword) ? 0 : 2
                  ] = new PdfByteString(value);
        }

        /**
          <summary>Gets/Sets the word spacing.</summary>
        */
        public double? WordSpace
        {
            get
            {
                if (this.@operator.Equals(SimpleOperatorKeyword))
                {
                    return null;
                }
                else
                {
                    return ((IPdfNumber)this.operands[0]).RawValue;
                }
            }
            set
            {
                this.EnsureSpaceOperation();
                this.operands[0] = PdfReal.Get(value.Value);
            }
        }
    }
}