//-----------------------------------------------------------------------
// <copyright file="TextFitter.cs" company="">
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
namespace org.pdfclown.documents.contents.composition
{
    using System.Text.RegularExpressions;
    using org.pdfclown.documents.contents.fonts;

    ///
    /// <summary>
    /// Text fitter.
    /// </summary>
    ///
    public sealed class TextFitter
    {
        private static readonly Regex FitPattern = new Regex(@"(\s*)(\S*)", RegexOptions.Compiled);

        internal TextFitter(
            string text,
            double width,
            Font font,
            double fontSize,
            bool hyphenation,
            char hyphenationCharacter)
        {
            this.Text = text;
            this.Width = width;
            this.Font = font;
            this.FontSize = fontSize;
            this.Hyphenation = hyphenation;
            this.HyphenationCharacter = hyphenationCharacter;
        }

        private void Hyphenate(
            bool hyphenation,
            ref int index,
            ref int wordEndIndex,
            double wordWidth,
            out string hyphen)
        {
            /*
              TODO: This hyphenation algorithm is quite primitive (to improve!).
            */
            while (true)
            {
                // Add the current character!
                var textChar = this.Text[wordEndIndex];
                wordWidth = this.Font.GetWidth(textChar, this.FontSize);
                wordEndIndex++;
                this.FittedWidth += wordWidth;
                // Does the fitted text's width exceed the available width?
                if (this.FittedWidth > this.Width)
                {
                    // Remove the current character!
                    this.FittedWidth -= wordWidth;
                    wordEndIndex--;
                    if (hyphenation)
                    {
                        // Is hyphenation to be applied?
                        if (wordEndIndex > index + 4) // Long-enough word chunk.
                        {
                            // Make room for the hyphen character!
                            wordEndIndex--;
                            index = wordEndIndex;
                            textChar = this.Text[wordEndIndex];
                            this.FittedWidth -= this.Font.GetWidth(textChar, this.FontSize);

                            // Add the hyphen character!
                            textChar = this.HyphenationCharacter;
                            this.FittedWidth += this.Font.GetWidth(textChar, this.FontSize);

                            hyphen = textChar.ToString();
                        }
                        else // No hyphenation.
                        {
                            // Removing the current word chunk...
                            while (wordEndIndex > index)
                            {
                                wordEndIndex--;
                                textChar = this.Text[wordEndIndex];
                                this.FittedWidth -= this.Font.GetWidth(textChar, this.FontSize);
                            }

                            hyphen = string.Empty;
                        }
                    }
                    else
                    {
                        index = wordEndIndex;

                        hyphen = string.Empty;
                    }
                    break;
                }
            }
        }

        ///
        /// <summary>
        /// Fits the text inside the specified width.
        /// </summary>
        /// <param name="unspacedFitting">Whether fitting of unspaced text is allowed.</param>
        /// <returns>Whether the operation was successful.</returns>
        ///
        public bool Fit(bool unspacedFitting) { return this.Fit(this.EndIndex + 1, this.Width, unspacedFitting); }

        ///
        /// <summary>
        /// Fits the text inside the specified width.
        /// </summary>
        /// <param name="index">Beginning index, inclusive.</param>
        /// <param name="width">Available width.</param>
        /// <param name="unspacedFitting">Whether fitting of unspaced text is allowed.</param>
        /// <returns>Whether the operation was successful.</returns>
        ///
        public bool Fit(int index, double width, bool unspacedFitting)
        {
            this.BeginIndex = index;
            this.Width = width;

            this.FittedText = null;
            this.FittedWidth = 0;

            var hyphen = string.Empty;

            // Fitting the text within the available width...

            var match = FitPattern.Match(this.Text, this.BeginIndex);
            while (match.Success)
            {
                // Scanning for the presence of a line break...

                var leadingWhitespaceGroup = match.Groups[1];
                /*
                  NOTE: This text fitting algorithm returns everytime it finds a line break character,
                  as it's intended to evaluate the width of just a single line of text at a time.
                */
                for (int spaceIndex = leadingWhitespaceGroup.Index,
                    spaceEnd = leadingWhitespaceGroup.Index + leadingWhitespaceGroup.Length; spaceIndex < spaceEnd; spaceIndex++)
                {
                    switch (this.Text[spaceIndex])
                    {
                        case '\n':
                        case '\r':
                            index = spaceIndex;
                            goto endFitting; // NOTE: I know GOTO is evil, but in this case using it sparingly avoids cumbersome boolean flag checks.
                    }
                }

                var matchGroup = match.Groups[0];
                // Add the current word!
                var wordEndIndex = matchGroup.Index + matchGroup.Length; // Current word's limit.
                var wordWidth = this.Font.GetWidth(matchGroup.Value, this.FontSize); // Current word's width.
                this.FittedWidth += wordWidth;
                // Does the fitted text's width exceed the available width?
                if (this.FittedWidth > width)
                {
                    // Remove the current (unfitting) word!
                    this.FittedWidth -= wordWidth;
                    wordEndIndex = index;
                    if (!this.Hyphenation &&
                        ((wordEndIndex > this.BeginIndex) // There's fitted content.
                        ||
                            !unspacedFitting // There's no fitted content, but unspaced fitting isn't allowed.
                        ||
                            (this.Text[this.BeginIndex] == ' ')) // Unspaced fitting is allowed, but text starts with a space.
                      ) // Enough non-hyphenated text fitted.
                    {
                        goto endFitting;
                    }

                    /*
                      NOTE: We need to hyphenate the current (unfitting) word.
                    */
                    this.Hyphenate(this.Hyphenation, ref index, ref wordEndIndex, wordWidth, out hyphen);

                    break;
                }
                index = wordEndIndex;

                match = match.NextMatch();
            }
            endFitting:
            this.FittedText = $"{this.Text.Substring(this.BeginIndex, index - this.BeginIndex)}{hyphen}";
            this.EndIndex = index;

            return this.FittedWidth > 0;
        }

        ///
        /// <summary>
        /// Gets the begin index of the fitted text inside the available text.
        /// </summary>
        ///
        public int BeginIndex { get; private set; } = 0;

        ///
        /// <summary>
        /// Gets the end index of the fitted text inside the available text.
        /// </summary>
        ///
        public int EndIndex { get; private set; } = -1;

        ///
        /// <summary>
        /// Gets the fitted text.
        /// </summary>
        ///
        public string FittedText { get; private set; }

        ///
        /// <summary>
        /// Gets the fitted text's width.
        /// </summary>
        ///
        public double FittedWidth { get; private set; }

        ///
        /// <summary>
        /// Gets the font used to fit the text.
        /// </summary>
        ///
        public Font Font { get; }

        ///
        /// <summary>
        /// Gets the size of the font used to fit the text.
        /// </summary>
        ///
        public double FontSize { get; }

        ///
        /// <summary>
        /// Gets whether the hyphenation algorithm has to be applied.
        /// </summary>
        ///
        public bool Hyphenation { get; }

        ///
        /// <summary>
        /// Gets/Sets the character shown at the end of the line before a hyphenation break.
        /// </summary>
        ///
        public char HyphenationCharacter { get; }

        ///
        /// <summary>
        /// Gets the available text.
        /// </summary>
        ///
        public string Text { get; }

        ///
        /// <summary>
        /// Gets the available width.
        /// </summary>
        ///
        public double Width { get; private set; }
    }
}
