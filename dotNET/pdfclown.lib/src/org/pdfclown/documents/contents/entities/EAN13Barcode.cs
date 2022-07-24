//-----------------------------------------------------------------------
// <copyright file="EAN13Barcode.cs" company="">
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
namespace org.pdfclown.documents.contents.entities
{
    using System;
    using System.Drawing;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.objects;

    using fonts = org.pdfclown.documents.contents.fonts;
    using xObjects = org.pdfclown.documents.contents.xObjects;

    ///
    /// <summary>
    /// <para>The EAN-13 Bar Code Symbol shall be made up as follows, reading from left to right:</para> <list
    /// type="number"><item>A left Quiet Zone</item><item>A normal Guard Bar Pattern (Left Guard)</item><item>Six symbol
    /// characters from number sets A and B (Left Half)</item><item>A center Guard Bar Pattern (Center
    /// Guard)</item><item>Six symbol characters from number set C (Right Half)</item><item>A normal Guard Bar Pattern
    /// (Right Guard)</item><item>A right Quiet Zone</item></list> <para>The rightmost symbol character shall encode the
    /// Check Digit.</para>
    /// </summary>
    /// <see href="http://en.wikipedia.org/wiki/EAN13">EAN-13 Bar Code</see> object [GS1:7.1:5.1.1.3.1].
    /// 
    public sealed class EAN13Barcode : Barcode
    {
        /// <summary>
        /// Bar full height.
        /// </summary>
        private static readonly int BarHeight;
        /*
          NOTE: Conventional terms used within this implementation:
            * module: smallest encoding unit (either a bar (dark module) or a space (light module);
            * element: sequence of omogeneous modules (either all bars or all spaces);
            * symbol character: code digit, whose encoding is made up of 4 elements encompassing 7 modules;
            * number set: symbol character encoding, representing the codomain of the digit domain
              (i.e. [0-9]).
        */

        /// <summary>
        /// Symbol Character Encodation (Number Set A, odd parity) [GS1:7.1:5.1.1.2.1]. NOTE: Number Set B uses the same
        /// patterns (though at inverted parity, i.e. even), whilst Number Set C (even parity) mirrors Number Set B.
        /// </summary>
        private static readonly int[][] DigitElementWidths =
        {
            new int[] { 3, 2, 1, 1 }, // 0
            new int[] { 2, 2, 2, 1 }, // 1
            new int[] { 2, 1, 2, 2 }, // 2
            new int[] { 1, 4, 1, 1 }, // 3
            new int[] { 1, 1, 3, 2 }, // 4
            new int[] { 1, 2, 3, 1 }, // 5
            new int[] { 1, 1, 1, 4 }, // 6
            new int[] { 1, 3, 1, 2 }, // 7
            new int[] { 1, 2, 1, 3 }, // 8
            new int[] { 3, 1, 1, 2 }  // 9
        };

        /// <summary>
        /// Digit glyph width.
        /// </summary>
        private static readonly int DigitGlyphWidth;
        /// <summary>
        /// Digit glyph horizontal positions.
        /// </summary>
        private static readonly double[] DigitGlyphXs;

        /// <summary>
        /// Digit box height.
        /// </summary>
        private static readonly int DigitHeight;
        /// <summary>
        /// Digit box width.
        /// </summary>
        private static readonly int DigitWidth;

        /// <summary>
        /// Bar elements count.
        /// </summary>
        private static readonly int ElementCount;

        /// <summary>
        /// Guard bar index positions.
        /// </summary>
        private static readonly int[] GuardBarIndexes =
        {
            0,
            2, // Left Guard.
            28,
            30, // Center Guard.
            56,
            58 // Right Guard.
        };
        /// <summary>
        /// Left Half of an EAN-13 Bar Code Symbol. Since the EAN-13 Bar Code Symbol comprises only 12 symbol characters
        /// but encodes 13 digits of data (including the Check Digit), the value of the additional digit (leading digit,
        /// implicitly encoded), which is the character in the leftmost position in the data string, shall be encoded by
        /// the variable parity mix of number sets A and B for the six symbol characters in the left half of the symbol.
        ///
        /// </summary>
        private static readonly int[][] LeftHalfNumberSets =
        {
            new int[] { NumberSet_A, NumberSet_A, NumberSet_A, NumberSet_A, NumberSet_A, NumberSet_A }, // 0
            new int[] { NumberSet_A, NumberSet_A, NumberSet_B, NumberSet_A, NumberSet_B, NumberSet_B }, // 1
            new int[] { NumberSet_A, NumberSet_A, NumberSet_B, NumberSet_B, NumberSet_A, NumberSet_B }, // 2
            new int[] { NumberSet_A, NumberSet_A, NumberSet_B, NumberSet_B, NumberSet_B, NumberSet_A }, // 3
            new int[] { NumberSet_A, NumberSet_B, NumberSet_A, NumberSet_A, NumberSet_B, NumberSet_B }, // 4
            new int[] { NumberSet_A, NumberSet_B, NumberSet_B, NumberSet_A, NumberSet_A, NumberSet_B }, // 5
            new int[] { NumberSet_A, NumberSet_B, NumberSet_B, NumberSet_B, NumberSet_A, NumberSet_A }, // 6
            new int[] { NumberSet_A, NumberSet_B, NumberSet_A, NumberSet_B, NumberSet_A, NumberSet_B }, // 7
            new int[] { NumberSet_A, NumberSet_B, NumberSet_A, NumberSet_B, NumberSet_B, NumberSet_A }, // 8
            new int[] { NumberSet_A, NumberSet_B, NumberSet_B, NumberSet_A, NumberSet_B, NumberSet_A }  // 9
        };

        private static readonly int NumberSet_A = 0;
        private static readonly int NumberSet_B = 1;

        static EAN13Barcode()
        {
            /*
              Digit metrics.
            */

            var digitElementWidths = DigitElementWidths[0];

            ElementCount
              = 3 // Left Guard.
                +
                (digitElementWidths.Length * 6) // Left Half.
                +
                5 // Center Guard.
                +
                (digitElementWidths.Length * 6) // Right Half.
                +
                3; // Right Guard.

            var digitWidth = 0;
            foreach (var digitElementWidth in digitElementWidths)
            {
                digitWidth += digitElementWidth;
            }
            DigitWidth = digitWidth;
            DigitHeight = DigitWidth + 2;
            DigitGlyphWidth = DigitWidth - 1;
            BarHeight = DigitHeight * 4;

            /*
              Digit glyph horizontal positions.
            */

            double[] elementWidths =
            {
                DigitWidth,
                3,
                DigitWidth,
                DigitWidth,
                DigitWidth,
                DigitWidth,
                DigitWidth,
                DigitWidth,
                5,
                DigitWidth,
                DigitWidth,
                DigitWidth,
                DigitWidth,
                DigitWidth,
                DigitWidth,
                3
            };
            int[] digitIndexes = { 0, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14 };
            DigitGlyphXs = new double[13];
            var digitXIndex = 0;
            for (int index = 0,
                length = elementWidths.Length; index < length; index++)
            {
                if (index < digitIndexes[digitXIndex])
                {
                    DigitGlyphXs[digitXIndex] += elementWidths[index];
                }
                else
                {
                    DigitGlyphXs[digitXIndex] += elementWidths[index] / 2;
                    digitXIndex++;
                    if (digitXIndex >= DigitGlyphXs.Length)
                    {
                        break;
                    }

                    DigitGlyphXs[digitXIndex] = DigitGlyphXs[digitXIndex - 1] + (elementWidths[index] / 2);
                }
            }
        }


        public EAN13Barcode(string code) : base(code)
        {
        }

        ///
        /// <summary>
        /// Gets the code elements widths.
        /// </summary>
        ///
        private int[] GetElementWidths()
        {
            // 1. Digit-codes-to-digit-IDs transformation.
            /* NOTE: Leveraging the ASCII charset sequence. */
            var digits = new int[this.code.Length];
            for (var index = 0; index < digits.Length; index++)
            {
                digits[index] = this.code[index] - '0';
            }

            // 2. Element widths calculation.
            var elementWidths = new int[ElementCount];
            var elementIndex = 0;

            // Left Guard Bar Pattern (3 elements).
            elementWidths[elementIndex++] = 1;
            elementWidths[elementIndex++] = 1;
            elementWidths[elementIndex++] = 1;

            var digitIndex = 0;

            // Left Half (6 digits, 4 elements each).
            var leftHalfNumberSets = LeftHalfNumberSets[digits[digitIndex++]]; // Gets the left-half number set encoding sequence based on the leading digit.
            do
            {
                var digitElementWidths = DigitElementWidths[digits[digitIndex]];
                // Number Set A encoding to apply?
                if (leftHalfNumberSets[digitIndex - 1] == NumberSet_A) // Number Set A encoding.
                {
                    elementWidths[elementIndex++] = digitElementWidths[0];
                    elementWidths[elementIndex++] = digitElementWidths[1];
                    elementWidths[elementIndex++] = digitElementWidths[2];
                    elementWidths[elementIndex++] = digitElementWidths[3];
                }
                else // Number Set B encoding.
                {
                    elementWidths[elementIndex++] = digitElementWidths[3];
                    elementWidths[elementIndex++] = digitElementWidths[2];
                    elementWidths[elementIndex++] = digitElementWidths[1];
                    elementWidths[elementIndex++] = digitElementWidths[0];
                }
            } while (digitIndex++ < leftHalfNumberSets.Length);

            // Center Guard Bar Pattern (5 elements).
            elementWidths[elementIndex++] = 1;
            elementWidths[elementIndex++] = 1;
            elementWidths[elementIndex++] = 1;
            elementWidths[elementIndex++] = 1;
            elementWidths[elementIndex++] = 1;

            // Right Half (6 digits, 4 elements each).
            do
            {
                var digitElementWidths = DigitElementWidths[digits[digitIndex]];
                // NOTE: Number Set C encoding.
                elementWidths[elementIndex++] = digitElementWidths[0];
                elementWidths[elementIndex++] = digitElementWidths[1];
                elementWidths[elementIndex++] = digitElementWidths[2];
                elementWidths[elementIndex++] = digitElementWidths[3];
            } while (digitIndex++ < 12);

            // Right Guard Bar Pattern (3 elements).
            elementWidths[elementIndex++] = 1;
            elementWidths[elementIndex++] = 1;
            elementWidths[elementIndex++] = 1;

            return elementWidths;
        }

        ///
        /// <summary>
        /// Gets the barcode's graphical size.
        /// </summary>
        ///
        private Size Size => new Size(
            (DigitWidth * 13) // Digits.
                    +
                (3 * 2) // Left and right guards.
                    +
                5, // Center guard.
            BarHeight // Non-guard bar.
                    +
                DigitHeight // Digit.
                  );

        public override ContentObject ToInlineObject(PrimitiveComposer composer)
        {
            ContentObject barcodeObject = composer.BeginLocalState();
            fonts::Font font = new fonts::StandardType1Font(
                composer.Scanner.Contents.Document,
                fonts::StandardType1Font.FamilyEnum.Helvetica,
                false,
                false);
            var fontSize = DigitGlyphWidth / font.GetWidth(this.code.Substring(0, 1), 1);

            // 1. Bars.

            double elementX = DigitWidth;
            var elementWidths = this.GetElementWidths();

            double guardBarIndentY = DigitHeight / 2;
            var isBar = true;
            for (var elementIndex = 0; elementIndex < elementWidths.Length; elementIndex++)
            {
                double elementWidth = elementWidths[elementIndex];
                // Dark element?
                /*
                  NOTE: EAN symbol elements alternate bars to spaces.
                */
                if (isBar)
                {
                    composer.DrawRectangle(
                        new RectangleF(
                            (float)elementX,
                            0,
                            (float)elementWidth,
                            (float)(BarHeight +
                                (
                          // Guard bar?
                          (Array.BinarySearch<int>(GuardBarIndexes, elementIndex) >= 0)
                                    ? guardBarIndentY // Guard bar.
                                    : 0 // Symbol character.
                          ))));
                }

                elementX += elementWidth;
                isBar = !isBar;
            }
            composer.Fill();

            // 2. Digits.

            composer.SetFont(font, fontSize);
            var digitY = BarHeight + ((DigitHeight - font.GetAscent(fontSize)) / 2);
            // Showing the digits...
            for (var digitIndex = 0; digitIndex < 13; digitIndex++)
            {
                var digit = this.code.Substring(digitIndex, 1);
                var pX = DigitGlyphXs[digitIndex] // Digit position.
                  -
                    (font.GetWidth(digit, fontSize) / 2); // Centering.
                // Show the current digit!
                _ = composer.ShowText(digit, new PointF((float)pX, (float)digitY));
            }
            composer.End();
            return barcodeObject;
        }

        public override xObjects::XObject ToXObject(Document context)
        {
            var xObject = new xObjects::FormXObject(context, this.Size);
            var composer = new PrimitiveComposer(xObject);
            _ = this.ToInlineObject(composer);
            composer.Flush();
            return xObject;
        }
    }
}
