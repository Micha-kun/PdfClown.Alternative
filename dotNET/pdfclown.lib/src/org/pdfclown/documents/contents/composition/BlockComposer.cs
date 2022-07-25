//-----------------------------------------------------------------------
// <copyright file="BlockComposer.cs" company="">
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
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using org.pdfclown.documents.contents.objects;
    using org.pdfclown.objects;

    using org.pdfclown.util.math;
    using xObjects = org.pdfclown.documents.contents.xObjects;

    ///
    /// <summary>
    /// <para>Content block composer.</para> <para>It provides content positioning functionalities for page
    /// typesetting.</para>
    /// </summary>
    ///
    /*
      NOTE: BlockComposer is going to become deprecated as soon as DocumentComposer fully supports its
      functionality. Until DocumentComposer's content styles are available, BlockComposer is the only
      way to intertwine block-level formatting with custom low-level operations like graphics
      parameters settings (e.g. text color mixing).
    */
    public sealed class BlockComposer
    {
        /// <summary>
        /// Actual area occupied by the block contents.
        /// </summary>
        private RectangleF boundBox;

        private LocalGraphicsState container;

        private Row currentRow;

        /// <summary>
        /// Area available for the block contents.
        /// </summary>
        private RectangleF frame;
        private double lastFontSize;
        private bool rowEnded;

        public BlockComposer(PrimitiveComposer baseComposer)
        {
            this.BaseComposer = baseComposer;
            this.Scanner = baseComposer.Scanner;
        }

        ///
        /// <summary>
        /// Adds an object to the current row.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="lineAlignment">Object's line alignment.</param>
        ///
        private void AddRowObject(RowObject obj, object lineAlignment)
        {
            this.currentRow.Objects.Add(obj);
            this.currentRow.SpaceCount += obj.SpaceCount;
            this.currentRow.Width += obj.Width;

            if ((lineAlignment is double) || lineAlignment.Equals(LineAlignmentEnum.BaseLine))
            {
                var gap = (lineAlignment is double) ? ((double)lineAlignment) : 0;
                var superGap = obj.BaseLine + gap - this.currentRow.BaseLine;
                if (superGap > 0)
                {
                    this.currentRow.Height += superGap;
                    this.currentRow.BaseLine += superGap;
                }
                var subGap = this.currentRow.BaseLine + (obj.Height - obj.BaseLine) - gap - this.currentRow.Height;
                if (subGap > 0)
                {
                    this.currentRow.Height += subGap;
                }
            }
            else if (obj.Height > this.currentRow.Height)
            {
                this.currentRow.Height = obj.Height;
            }
        }

        ///
        /// <summary>
        /// Begins a content row.
        /// </summary>
        ///
        private void BeginRow()
        {
            this.rowEnded = false;

            var state = this.BaseComposer.State;

            double rowY = this.boundBox.Height;
            if (rowY > 0)
            {
                rowY += this.LineSpace.GetValue((state.Font != null) ? state.Font.GetLineHeight(state.FontSize) : 0);
            }
            this.currentRow = new Row(rowY);
            if (this.XAlignment == XAlignmentEnum.Justify)
            {
                /*
                  TODO: This temporary hack forces PrimitiveComposer.showText() to insert word space
                  adjustments in case of justified text; when the row ends, it has to be updated with the
                  actual adjustment value.
                */
                this.currentRow.WordSpaceAdjustment = this.BaseComposer.Add(new SetWordSpace(.001));
            }
            else if (state.WordSpace != 0)
            {
                this.BaseComposer.SetWordSpace(0);
            }
        }

        private int CountOccurrence(char value, string text)
        {
            var count = 0;
            var fromIndex = 0;
            do
            {
                var foundIndex = text.IndexOf(value, fromIndex);
                if (foundIndex == -1)
                {
                    return count;
                }

                count++;

                fromIndex = foundIndex + 1;
            } while (true);
        }

        ///
        /// <summary>
        /// Ends the content row.
        /// </summary>
        /// <param name="broken">Indicates whether this is the end of a paragraph.</param>
        ///
        private void EndRow(bool broken)
        {
            if (this.rowEnded)
            {
                return;
            }

            this.rowEnded = true;

            var objects = this.currentRow.Objects;
            var objectXOffsets = new double[objects.Count]; // Horizontal object displacements.
            double wordSpace = 0; // Exceeding space among words.
            double rowXOffset = 0; // Horizontal row offset.

            // Horizontal alignment.
            var xAlignment = this.XAlignment;
            switch (xAlignment)
            {
                case XAlignmentEnum.Left:
                    break;
                case XAlignmentEnum.Right:
                    rowXOffset = this.frame.Width - this.currentRow.Width;
                    break;
                case XAlignmentEnum.Center:
                    rowXOffset = (this.frame.Width - this.currentRow.Width) / 2;
                    break;
                case XAlignmentEnum.Justify:
                    if ((this.currentRow.SpaceCount == 0) || broken) // NO spaces.
                    {
                        /* NOTE: This situation equals a simple left alignment. */
                    }
                    else // Spaces exist.
                    {
                        // Calculate the exceeding spacing among the words!
                        wordSpace = (this.frame.Width - this.currentRow.Width) / this.currentRow.SpaceCount;

                        // Define the horizontal offsets for justified alignment.
                        for (int index = 1,
                            count = objects.Count; index < count; index++)
                        {
                            /*
                              NOTE: The offset represents the horizontal justification gap inserted at the left
                              side of each object.
                            */
                            objectXOffsets[index] = objectXOffsets[index - 1] +
                                (objects[index - 1].SpaceCount * wordSpace);
                        }
                    }
                    this.currentRow.WordSpaceAdjustment.Value = wordSpace;
                    break;
            }

            // Vertical alignment and translation.
            for (var index = objects.Count - 1; index >= 0; index--)
            {
                var obj = objects[index];

                // Vertical alignment.
                double objectYOffset = 0;
                LineAlignmentEnum lineAlignment;
                double lineRise;
                var objectLineAlignment = obj.LineAlignment;
                if (objectLineAlignment is double)
                {
                    lineAlignment = LineAlignmentEnum.BaseLine;
                    lineRise = (double)objectLineAlignment;
                }
                else
                {
                    lineAlignment = (LineAlignmentEnum)objectLineAlignment;
                    lineRise = 0;
                }
                switch (lineAlignment)
                {
                    case LineAlignmentEnum.Top:
                        /* NOOP */
                        break;
                    case LineAlignmentEnum.Middle:
                        objectYOffset = (-(this.currentRow.Height - obj.Height)) / 2;
                        break;
                    case LineAlignmentEnum.BaseLine:
                        objectYOffset = -(this.currentRow.BaseLine - obj.BaseLine - lineRise);
                        break;
                    case LineAlignmentEnum.Bottom:
                        objectYOffset = -(this.currentRow.Height - obj.Height);
                        break;
                    default:
                        throw new NotImplementedException($"Line alignment {lineAlignment} unknown.");
                }

                var containedGraphics = obj.Container.Objects;
                // Translation.
                containedGraphics.Insert(
                    0,
                    new ModifyCTM(
                        1,
                        0,
                        0,
                        1,
                        objectXOffsets[index] + rowXOffset, // Horizontal alignment.
                        objectYOffset // Vertical alignment.
                    ));
                // Word spacing.
                if (obj.Type == RowObject.TypeEnum.Text)
                {
                    /*
                      TODO: This temporary hack adjusts the word spacing in case of composite font.
                      When DocumentComposer replaces BlockComposer, all the graphical properties of contents
                      will be declared as styles and their composition will occur as a single pass without such
                      ugly tweakings.
                    */
                    var showTextOperation = (ShowText)((Text)((LocalGraphicsState)containedGraphics[1]).Objects[1]).Objects[
                        1];
                    if (showTextOperation is ShowAdjustedText)
                    {
                        var wordSpaceObject = PdfInteger.Get(
                            (int)Math.Round((-wordSpace) * 1000 * obj.Scale / obj.FontSize));
                        var textParams = (PdfArray)showTextOperation.Operands[0];
                        for (int textParamIndex = 1, textParamsLength = textParams.Count; textParamIndex <
                            textParamsLength; textParamIndex += 2)
                        {
                            textParams[textParamIndex] = wordSpaceObject;
                        }
                    }
                }
            }

            // Update the actual block height!
            this.boundBox.Height = (float)(this.currentRow.Y + this.currentRow.Height);

            // Update the actual block vertical location!
            double yOffset;
            switch (this.YAlignment)
            {
                case YAlignmentEnum.Bottom:
                    yOffset = this.frame.Height - this.boundBox.Height;
                    break;
                case YAlignmentEnum.Middle:
                    yOffset = (this.frame.Height - this.boundBox.Height) / 2;
                    break;
                case YAlignmentEnum.Top:
                default:
                    yOffset = 0;
                    break;
            }
            this.boundBox.Y = (float)(this.frame.Y + yOffset);

            // Discard the current row!
            this.currentRow = null;
        }

        private bool EnsureRow(bool open)
        {
            if (this.container == null)
            {
                return false;
            }

            if (open == (this.currentRow == null))
            {
                if (this.currentRow == null)
                {
                    this.BeginRow();
                }
                else
                {
                    this.EndRow(true);
                }
            }
            return true;
        }

        private object ResolveLineAlignment(object lineAlignment)
        {
            if (!((lineAlignment is LineAlignmentEnum) || (lineAlignment is Length)))
            {
                throw new ArgumentException("MUST be either LineAlignmentEnum or Length.", nameof(lineAlignment));
            }

            if (lineAlignment.Equals(LineAlignmentEnum.Super))
            {
                lineAlignment = new Length(0.33, Length.UnitModeEnum.Relative);
            }
            else if (lineAlignment.Equals(LineAlignmentEnum.Sub))
            {
                lineAlignment = new Length(-0.33, Length.UnitModeEnum.Relative);
            }
            if (lineAlignment is Length)
            {
                if (this.lastFontSize == 0)
                {
                    this.lastFontSize = this.BaseComposer.State.FontSize;
                }
                lineAlignment = ((Length)lineAlignment).GetValue(this.lastFontSize);
            }

            return lineAlignment;
        }

        ///
        /// <summary>
        /// Begins a content block.
        /// </summary>
        /// <param name="frame">Block boundaries.</param>
        /// <param name="xAlignment">Horizontal alignment.</param>
        /// <param name="yAlignment">Vertical alignment.</param>
        ///
        public void Begin(RectangleF frame, XAlignmentEnum xAlignment, YAlignmentEnum yAlignment)
        {
            this.frame = frame;
            this.XAlignment = xAlignment;
            this.YAlignment = yAlignment;
            this.lastFontSize = 0;

            // Open the block local state!
            /*
              NOTE: This device allows a fine-grained control over the block representation.
              It MUST be coupled with a closing statement on block end.
            */
            this.container = this.BaseComposer.BeginLocalState();

            this.boundBox = new RectangleF(frame.X, frame.Y, frame.Width, 0);
        }

        ///
        /// <summary>
        /// Ends the content block.
        /// </summary>
        ///
        public void End()
        {
            // End last row!
            this.EndRow(true);

            // Block translation.
            this.container.Objects
                .Insert(
                    0,
                    new ModifyCTM(
                        1,
                        0,
                        0,
                        1,
                        this.boundBox.X, // Horizontal translation.
                        -this.boundBox.Y // Vertical translation.
                ));

            // Close the block local state!
            this.BaseComposer.End();

            this.container = null;
        }

        ///
        /// <summary>
        /// Ends current paragraph.
        /// </summary>
        ///
        public bool ShowBreak() { return this.ShowBreak(null, null); }

        ///
        /// <summary>
        /// Ends current paragraph, specifying the offset of the next one.
        /// </summary>
        /// <remarks>
        /// This functionality allows higher-level features such as paragraph indentation and margin.
        /// </remarks>
        /// <param name="offset">Relative location of the next paragraph.</param>
        ///
        public bool ShowBreak(SizeF offset) { return this.ShowBreak(offset, null); }

        ///
        /// <summary>
        /// Ends current paragraph, specifying the alignment of the next one.
        /// </summary>
        /// <remarks>This functionality allows higher-level features such as paragraph indentation and margin.</remarks>
        /// <param name="xAlignment">Horizontal alignment.</param>
        ///
        public bool ShowBreak(XAlignmentEnum xAlignment) { return this.ShowBreak(null, xAlignment); }

        ///
        /// <summary>
        /// Ends current paragraph, specifying the offset and alignment of the next one.
        /// </summary>
        /// <remarks>This functionality allows higher-level features such as paragraph indentation and margin.</remarks>
        /// <param name="offset">Relative location of the next paragraph.</param>
        /// <param name="xAlignment">Horizontal alignment.</param>
        ///
        public bool ShowBreak(SizeF? offset, XAlignmentEnum? xAlignment)
        {
            // End previous row!
            if (!this.EnsureRow(false))
            {
                return false;
            }

            if (xAlignment.HasValue)
            {
                this.XAlignment = xAlignment.Value;
            }

            this.BeginRow();

            if (offset.HasValue)
            {
                this.currentRow.Y += offset.Value.Height;
                this.currentRow.Width = offset.Value.Width;
            }
            return true;
        }

        ///
        /// <summary>
        /// Shows text.
        /// </summary>
        /// <remarks>Default line alignment is applied.</remarks>
        /// <param name="text">Text to show.</param>
        /// <returns>Last shown character index.</returns>
        ///
        public int ShowText(string text) { return this.ShowText(text, this.LineAlignment); }

        ///
        /// <summary>Shows text.</summary>
        /// <param name="text">Text to show.</param>
        /// <param name="lineAlignment">Line alignment. It can be:
        ///   <list type="bullet">
        ///     <item><see cref="LineAlignmentEnum"/></item>
        ///     <item><see cref="Length">: arbitrary super-/sub-script, depending on whether the value is
        ///     positive or not.</item>
        ///   </list>
        /// </param>
        /// <returns>Last shown character index.</returns>
        ///
        public int ShowText(string text, object lineAlignment)
        {
            if ((text == null) || !this.EnsureRow(true))
            {
                return 0;
            }

            var state = this.BaseComposer.State;
            var font = state.Font;
            var fontSize = state.FontSize;
            var lineHeight = font.GetLineHeight(fontSize);
            var baseLine = font.GetAscent(fontSize);
            lineAlignment = this.ResolveLineAlignment(lineAlignment);

            var textFitter = new TextFitter(text, 0, font, fontSize, this.Hyphenation, this.HyphenationCharacter);
            var textLength = text.Length;
            var index = 0;

            while (true)
            {
                if (this.currentRow.Width == 0) // Current row has just begun.
                {
                    // Removing leading space...
                    while (true)
                    {
                        if (index == textLength) // Text end reached.
                        {
                            goto endTextShowing;
                        }
                        else if (text[index] != ' ') // No more leading spaces.
                        {
                            break;
                        }

                        index++;
                    }
                }

                if (OperationUtils.Compare(this.currentRow.Y + lineHeight, this.frame.Height) == 1) // Text's height exceeds block's remaining vertical space.
                {
                    // Terminate the current row and exit!
                    this.EndRow(false);
                    goto endTextShowing;
                }

                // Does the text fit?
                if (textFitter.Fit(
                    index,
                    this.frame.Width - this.currentRow.Width, // Remaining row width.
                    this.currentRow.SpaceCount == 0))
                {
                    // Get the fitting text!
                    var textChunk = textFitter.FittedText;
                    var textChunkWidth = textFitter.FittedWidth;
                    var textChunkLocation = new PointF((float)this.currentRow.Width, (float)this.currentRow.Y);

                    // Insert the fitting text!
                    RowObject obj;
                    obj = new RowObject(
                        RowObject.TypeEnum.Text,
                        this.BaseComposer.BeginLocalState(), // Opens the row object's local state.
                        lineHeight,
                        textChunkWidth,
                        this.CountOccurrence(' ', textChunk),
                        lineAlignment,
                        baseLine,
                        state.FontSize,
                        state.Scale);
                    _ = this.BaseComposer.ShowText(textChunk, textChunkLocation);
                    this.BaseComposer.End();  // Closes the row object's local state.
                    this.AddRowObject(obj, lineAlignment);

                    index = textFitter.EndIndex;
                }

                // Evaluating trailing text...
                while (true)
                {
                    if (index == textLength) // Text end reached.
                    {
                        goto endTextShowing;
                    }

                    switch (text[index])
                    {
                        case '\r':
                            break;
                        case '\n':
                            // New paragraph!
                            index++;
                            _ = this.ShowBreak();
                            goto endTrailParsing;
                        default:
                            // New row (within the same paragraph)!
                            this.EndRow(false);
                            this.BeginRow();
                            goto endTrailParsing;
                    }

                    index++;
                }
                endTrailParsing:
                ;
            }
            endTextShowing:
            ;
            if ((index >= 0) && lineAlignment.Equals(LineAlignmentEnum.BaseLine))
            {
                this.lastFontSize = fontSize;
            }

            return (textFitter.EndIndex > -1) ? index : 0;
        }

        ///
        /// <summary>
        /// Shows the specified external object.
        /// </summary>
        /// <remarks>Default line alignment is applied.</remarks>
        /// <param name="xObject">External object.</param>
        /// <param name="size">Size of the external object.</param>
        /// <returns>Whether the external object was successfully shown.</returns>
        ///
        public bool ShowXObject(xObjects::XObject xObject, SizeF? size)
        { return this.ShowXObject(xObject, size, this.LineAlignment); }

        ///
        /// <summary>Shows the specified external object.</summary>
        /// <param name="xObject">External object.</param>
        /// <param name="size">Size of the external object.</param>
        /// <param name="lineAlignment">Line alignment. It can be:
        ///   <list type="bullet">
        ///     <item><see cref="LineAlignmentEnum"/></item>
        ///     <item><see cref="Length">: arbitrary super-/sub-script, depending on whether the value is
        ///     positive or not.</item>
        ///   </list>
        /// </param>
        /// <returns>Whether the external object was successfully shown.</returns>
        ///
        public bool ShowXObject(xObjects::XObject xObject, SizeF? size, object lineAlignment)
        {
            if ((xObject == null) || !this.EnsureRow(true))
            {
                return false;
            }

            if (!size.HasValue)
            {
                size = xObject.Size;
            }
            lineAlignment = this.ResolveLineAlignment(lineAlignment);

            while (true)
            {
                if (OperationUtils.Compare(this.currentRow.Y + size.Value.Height, this.frame.Height) == 1) // Object's height exceeds block's remaining vertical space.
                {
                    // Terminate current row and exit!
                    this.EndRow(false);
                    return false;
                }
                else if (OperationUtils.Compare(this.currentRow.Width + size.Value.Width, this.frame.Width) < 1) // There's room for the object in the current row.
                {
                    var location = new PointF((float)this.currentRow.Width, (float)this.currentRow.Y);
                    RowObject obj;
                    obj = new RowObject(
                        RowObject.TypeEnum.XObject,
                        this.BaseComposer.BeginLocalState(), // Opens the row object's local state.
                        size.Value.Height,
                        size.Value.Width,
                        0,
                        lineAlignment,
                        size.Value.Height,
                        0,
                        0);
                    this.BaseComposer.ShowXObject(xObject, location, size);
                    this.BaseComposer.End(); // Closes the row object's local state.
                    this.AddRowObject(obj, lineAlignment);

                    return true;
                }
                else // There's NOT enough room for the object in the current row.
                {
                    // Go to next row!
                    this.EndRow(false);
                    this.BeginRow();
                }
            }
        }

        ///
        /// <summary>
        /// Gets the base composer.
        /// </summary>
        ///
        /*
          NOTE: In order to provide fine-grained alignment,
          there are 2 postproduction state levels:
            1- row level (see EndRow());
            2- block level (see End()).
        
          NOTE: Graphics instructions' layout follows this scheme (XS-BNF syntax):
            block = { beginLocalState translation parameters rows endLocalState }
            beginLocalState { "q\r" }
            translation = { "1 0 0 1 " number ' ' number "cm\r" }
            parameters = { ... } // Graphics state parameters.
            rows = { row* }
            row = { object* }
            object = { parameters beginLocalState translation content endLocalState }
            content = { ... } // Text, image (and so on) showing operators.
            endLocalState = { "Q\r" }
          NOTE: all the graphics state parameters within a block are block-level ones,
          i.e. they can't be represented inside row's or row object's local state, in order to
          facilitate parameter reuse within the same block.
        */
        public PrimitiveComposer BaseComposer { get; }

        ///
        /// <summary>
        /// Gets the area occupied by the already-placed block contents.
        /// </summary>
        ///
        public RectangleF BoundBox => this.boundBox;

        ///
        /// <summary>
        /// Gets the area where to place the block contents.
        /// </summary>
        ///
        public RectangleF Frame => this.frame;

        ///
        /// <summary>
        /// Gets/Sets whether the hyphenation algorithm has to be applied.
        /// </summary>
        /// <remarks>Initial value: <code>false</code>.</remarks>
        ///
        public bool Hyphenation { get; set; }

        ///
        /// <summary>
        /// Gets/Sets the character shown at the end of the line before a hyphenation break.
        /// </summary>
        /// <remarks>Initial value: hyphen symbol (U+002D, i.e. '-').</remarks>
        ///
        public char HyphenationCharacter { get; set; } = '-';

        ///
        /// <summary>
        /// Gets/Sets the default line alignment.
        /// </summary>
        /// <remarks>Initial value: <see cref="LineAlignmentEnum.BaseLine"/>.</remarks>
        ///
        public LineAlignmentEnum LineAlignment { get; set; } = LineAlignmentEnum.BaseLine;

        ///
        /// <summary>
        /// Gets/Sets the text interline spacing.
        /// </summary>
        /// <remarks>Initial value: 0.</remarks>
        ///
        public Length LineSpace { get; set; } = new Length(.2, Length.UnitModeEnum.Relative);

        ///
        /// <summary>
        /// Gets the content scanner.
        /// </summary>
        ///
        public ContentScanner Scanner { get; }

        ///
        /// <summary>
        /// Gets the horizontal alignment applied to the current content block.
        /// </summary>
        ///
        public XAlignmentEnum XAlignment { get; private set; }

        ///
        /// <summary>
        /// Gets the vertical alignment applied to the current content block.
        /// </summary>
        ///
        public YAlignmentEnum YAlignment { get; private set; }

        private sealed class Row
        {

            internal Row(double y) { this.Y = y; }

            ///
            /// <summary>
            /// Row base line.
            /// </summary>
            ///
            public double BaseLine { get; set; }
            public double Height { get; set; }
            ///
            /// <summary>
            /// Row's objects.
            /// </summary>
            ///
            public List<RowObject> Objects { get; set; } = new List<RowObject>();
            ///
            /// <summary>
            /// Number of space characters.
            /// </summary>
            ///
            public int SpaceCount { get; set; } = 0;
            public double Width { get; set; }
            public SetWordSpace WordSpaceAdjustment { get; set; }
            ///
            /// <summary>
            /// Vertical location relative to the block frame.
            /// </summary>
            ///
            public double Y { get; set; }
        }

        private sealed class RowObject
        {

            internal RowObject(
                TypeEnum type,
                ContainerObject container,
                double height,
                double width,
                int spaceCount,
                object lineAlignment,
                double baseLine,
                double fontSize,
                double scale)
            {
                this.Type = type;
                this.Container = container;
                this.Height = height;
                this.Width = width;
                this.SpaceCount = spaceCount;
                this.LineAlignment = lineAlignment;
                this.BaseLine = baseLine;
                this.FontSize = fontSize;
                this.Scale = scale;
            }

            ///
            /// <summary>
            /// Base line.
            /// </summary>
            ///
            public double BaseLine { get; set; }
            ///
            /// <summary>
            /// Graphics objects container associated to this object.
            /// </summary>
            ///
            public ContainerObject Container { get; set; }
            public double FontSize { get; set; }
            public double Height { get; set; }
            ///
            /// <summary>
            /// Line alignment (can be either LineAlignmentEnum or Double).
            /// </summary>
            ///
            public object LineAlignment { get; set; }
            public double Scale { get; set; }
            public int SpaceCount { get; set; }
            public TypeEnum Type { get; set; }
            public double Width { get; set; }

            public enum TypeEnum
            {
                Text,
                XObject
            }
        }
    }
}
