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


namespace org.pdfclown.tools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using System.Drawing;
    using System.Text;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.objects;
    using org.pdfclown.util.math;

    /**
      <summary>Tool for extracting text from <see cref="IContentContext">content contexts</see>.</summary>
    */
    public sealed class TextExtractor
    {

        public static readonly RectangleF DefaultArea = new RectangleF(0, 0, 0, 0);

        private AreaModeEnum areaMode = AreaModeEnum.Containment;
        private List<RectangleF> areas;
        private float areaTolerance = 0;
        private bool dehyphenated;
        private bool sorted;

        public TextExtractor(
  ) : this(true, false)
        { }

        public TextExtractor(
          bool sorted,
          bool dehyphenated
          ) : this(null, sorted, dehyphenated)
        { }

        public TextExtractor(
          IList<RectangleF> areas,
          bool sorted,
          bool dehyphenated
          )
        {
            this.Areas = areas;
            this.Dehyphenated = dehyphenated;
            this.Sorted = sorted;
        }

        /**
  <summary>Scans a content level looking for text.</summary>
*/
        private void Extract(
          ContentScanner level,
          IList<ContentScanner.TextStringWrapper> extractedTextStrings
          )
        {
            if (level == null)
            {
                return;
            }

            while (level.MoveNext())
            {
                var content = level.Current;
                if (content is Text)
                {
                    // Collect the text strings!
                    foreach (var textString in ((ContentScanner.TextWrapper)level.CurrentWrapper).TextStrings)
                    {
                        if (textString.TextChars.Count > 0)
                        { extractedTextStrings.Add(textString); }
                    }
                }
                else if (content is XObject)
                {
                    // Scan the external level!
                    this.Extract(
                      ((XObject)content).GetScanner(level),
                      extractedTextStrings
                      );
                }
                else if (content is ContainerObject)
                {
                    // Scan the inner level!
                    this.Extract(
                      level.ChildLevel,
                      extractedTextStrings
                      );
                }
            }
        }

        /**
          <summary>Sorts the extracted text strings.</summary>
          <remarks>Sorting implies text position ordering, integration and aggregation.</remarks>
          <param name="rawTextStrings">Source (lower-level) text strings.</param>
          <param name="textStrings">Target (higher-level) text strings.</param>
        */
        private void Sort(
          List<ContentScanner.TextStringWrapper> rawTextStrings,
          List<ITextString> textStrings
          )
        {
            // Sorting the source text strings...

            var positionComparator = new TextStringPositionComparer<ContentScanner.TextStringWrapper>();
            rawTextStrings.Sort(positionComparator);

            // Aggregating and integrating the source text strings into the target ones...
            TextString textString = null;
            TextStyle textStyle = null;
            TextChar previousTextChar = null;
            var dehyphenating = false;
            foreach (var rawTextString in rawTextStrings)
            {
                /*
                  NOTE: Contents on the same line are grouped together within the same text string.
                */
                // Add a new text string in case of new line!
                if ((textString != null)
                  && (textString.TextChars.Count > 0)
                  && !TextStringPositionComparer<ITextString>.IsOnTheSameLine(textString.Box.Value, rawTextString.Box.Value))
                {
                    if (this.dehyphenated
                      && (previousTextChar.Value == '-')) // Hyphened word.
                    {
                        _ = textString.TextChars.Remove(previousTextChar);
                        dehyphenating = true;
                    }
                    else // Full word.
                    {
                        // Add synthesized space character!
                        textString.TextChars.Add(
                          new TextChar(
                            ' ',
                            new RectangleF(
                              previousTextChar.Box.Right,
                              previousTextChar.Box.Top,
                              0,
                              previousTextChar.Box.Height
                              ),
                            textStyle,
                            true
                            )
                          );
                        textString = null;
                        dehyphenating = false;
                    }
                    previousTextChar = null;
                }
                if (textString == null)
                { textStrings.Add(textString = new TextString()); }

                textStyle = rawTextString.Style;
                var spaceWidth = textStyle.GetWidth(' ') * .5;
                foreach (var textChar in rawTextString.TextChars)
                {
                    if (previousTextChar != null)
                    {
                        /*
                          NOTE: PDF files may have text contents omitting space characters,
                          so they must be inferred and synthesized, marking them as virtual
                          in order to allow the user to distinguish between original contents
                          and augmented ones.
                        */
                        if (!textChar.Contains(' ')
                          && !previousTextChar.Contains(' '))
                        {
                            var charSpace = textChar.Box.X - previousTextChar.Box.Right;
                            if (charSpace > spaceWidth)
                            {
                                // Add synthesized space character!
                                textString.TextChars.Add(
                                  previousTextChar = new TextChar(
                                    ' ',
                                    new RectangleF(
                                      previousTextChar.Box.Right,
                                      textChar.Box.Y,
                                      charSpace,
                                      textChar.Box.Height
                                      ),
                                    textStyle,
                                    true
                                    )
                                  );
                            }
                        }
                        else if (dehyphenating
                          && previousTextChar.Contains(' '))
                        {
                            textStrings.Add(textString = new TextString());
                            dehyphenating = false;
                        }
                    }
                    textString.TextChars.Add(previousTextChar = textChar);
                }
            }
        }

        /**
          <summary>Extracts text strings from the specified content context.</summary>
          <param name="contentContext">Source content context.</param>
        */
        public IDictionary<RectangleF?, IList<ITextString>> Extract(
          IContentContext contentContext
          )
        {
            IDictionary<RectangleF?, IList<ITextString>> extractedTextStrings;
            var textStrings = new List<ITextString>();
            // 1. Extract the source text strings!
            var rawTextStrings = new List<ContentScanner.TextStringWrapper>();
            this.Extract(
              new ContentScanner(contentContext),
              rawTextStrings
              );

            // 2. Sort the target text strings!
            if (this.sorted)
            { this.Sort(rawTextStrings, textStrings); }
            else
            {
                foreach (var rawTextString in rawTextStrings)
                { textStrings.Add(rawTextString); }
            }

            // 3. Filter the target text strings!
            if (this.areas.Count == 0)
            {
                extractedTextStrings = new Dictionary<RectangleF?, IList<ITextString>>();
                extractedTextStrings[DefaultArea] = textStrings;
            }
            else
            { extractedTextStrings = this.Filter(textStrings, this.areas.ToArray()); }
            return extractedTextStrings;
        }

        /**
          <summary>Extracts text strings from the specified contents.</summary>
          <param name="contents">Source contents.</param>
        */
        public IDictionary<RectangleF?, IList<ITextString>> Extract(
          Contents contents
          )
        { return this.Extract(contents.ContentContext); }

        /**
          <summary>Gets the text strings matching the specified intervals.</summary>
          <param name="textStrings">Text strings to filter.</param>
          <param name="intervals">Text intervals to match. They MUST be ordered and not overlapping.</param>
          <returns>A list of text strings corresponding to the specified intervals.</returns>
        */
        public IList<ITextString> Filter(
          IDictionary<RectangleF?, IList<ITextString>> textStrings,
          IList<Interval<int>> intervals
          )
        {
            var filter = new IntervalFilter(intervals);
            this.Filter(textStrings, filter);
            return filter.TextStrings;
        }

        /**
          <summary>Processes the text strings matching the specified filter.</summary>
          <param name="textStrings">Text strings to filter.</param>
          <param name="filter">Matching processor.</param>
        */
        public void Filter(
          IDictionary<RectangleF?, IList<ITextString>> textStrings,
          IIntervalFilter filter
          )
        {
            var textStringsIterator = textStrings.Values.GetEnumerator();
            if (!textStringsIterator.MoveNext())
            {
                return;
            }

            var areaTextStringsIterator = textStringsIterator.Current.GetEnumerator();
            if (!areaTextStringsIterator.MoveNext())
            {
                return;
            }

            IList<TextChar> textChars = areaTextStringsIterator.Current.TextChars;
            var baseTextCharIndex = 0;
            while (filter.MoveNext())
            {
                var interval = filter.Current;
                var match = new TextString();
                var matchStartIndex = interval.Low;
                var matchEndIndex = interval.High;
                while (matchStartIndex > baseTextCharIndex + textChars.Count)
                {
                    baseTextCharIndex += textChars.Count;
                    if (!areaTextStringsIterator.MoveNext())
                    { areaTextStringsIterator = textStringsIterator.Current.GetEnumerator(); _ = areaTextStringsIterator.MoveNext(); }
                    textChars = areaTextStringsIterator.Current.TextChars;
                }
                var textCharIndex = matchStartIndex - baseTextCharIndex;

                while (baseTextCharIndex + textCharIndex < matchEndIndex)
                {
                    if (textCharIndex == textChars.Count)
                    {
                        baseTextCharIndex += textChars.Count;
                        if (!areaTextStringsIterator.MoveNext())
                        { areaTextStringsIterator = textStringsIterator.Current.GetEnumerator(); _ = areaTextStringsIterator.MoveNext(); }
                        textChars = areaTextStringsIterator.Current.TextChars;
                        textCharIndex = 0;
                    }
                    match.TextChars.Add(textChars[textCharIndex++]);
                }
                filter.Process(interval, match);
            }
        }

        /**
          <summary>Gets the text strings matching the specified area.</summary>
          <param name="textStrings">Text strings to filter, grouped by source area.</param>
          <param name="area">Graphic area which text strings have to be matched to.</param>
        */
        public IList<ITextString> Filter(
          IDictionary<RectangleF?, IList<ITextString>> textStrings,
          RectangleF area
          )
        { return this.Filter(textStrings, new RectangleF[] { area })[area]; }

        /**
          <summary>Gets the text strings matching the specified areas.</summary>
          <param name="textStrings">Text strings to filter, grouped by source area.</param>
          <param name="areas">Graphic areas which text strings have to be matched to.</param>
        */
        public IDictionary<RectangleF?, IList<ITextString>> Filter(
          IDictionary<RectangleF?, IList<ITextString>> textStrings,
          params RectangleF[] areas
          )
        {
            IDictionary<RectangleF?, IList<ITextString>> filteredTextStrings = null;
            foreach (var areaTextStrings in textStrings.Values)
            {
                var filteredAreasTextStrings = this.Filter(areaTextStrings, areas);
                if (filteredTextStrings == null)
                { filteredTextStrings = filteredAreasTextStrings; }
                else
                {
                    foreach (var filteredAreaTextStringsEntry in filteredAreasTextStrings)
                    {
                        var filteredTextStringsList = filteredTextStrings[filteredAreaTextStringsEntry.Key];
                        foreach (var filteredAreaTextString in filteredAreaTextStringsEntry.Value)
                        { filteredTextStringsList.Add(filteredAreaTextString); }
                    }
                }
            }
            return filteredTextStrings;
        }

        /**
          <summary>Gets the text strings matching the specified area.</summary>
          <param name="textStrings">Text strings to filter.</param>
          <param name="area">Graphic area which text strings have to be matched to.</param>
        */
        public IList<ITextString> Filter(
          IList<ITextString> textStrings,
          RectangleF area
          )
        { return this.Filter(textStrings, new RectangleF[] { area })[area]; }

        /**
          <summary>Gets the text strings matching the specified areas.</summary>
          <param name="textStrings">Text strings to filter.</param>
          <param name="areas">Graphic areas which text strings have to be matched to.</param>
        */
        public IDictionary<RectangleF?, IList<ITextString>> Filter(
          IList<ITextString> textStrings,
          params RectangleF[] areas
          )
        {
            IDictionary<RectangleF?, IList<ITextString>> filteredAreasTextStrings = new Dictionary<RectangleF?, IList<ITextString>>();
            foreach (var area in areas)
            {
                IList<ITextString> filteredAreaTextStrings = new List<ITextString>();
                filteredAreasTextStrings[area] = filteredAreaTextStrings;
                var toleratedArea = (this.areaTolerance != 0)
                  ? new RectangleF(
                    area.X - this.areaTolerance,
                    area.Y - this.areaTolerance,
                    area.Width + (this.areaTolerance * 2),
                    area.Height + (this.areaTolerance * 2)
                    )
                  : area;
                foreach (var textString in textStrings)
                {
                    var textStringBox = textString.Box;
                    if (toleratedArea.IntersectsWith(textStringBox.Value))
                    {
                        var filteredTextString = new TextString();
                        var filteredTextStringChars = filteredTextString.TextChars;
                        foreach (var textChar in textString.TextChars)
                        {
                            var textCharBox = textChar.Box;
                            if (((this.areaMode == AreaModeEnum.Containment) && toleratedArea.Contains(textCharBox))
                              || ((this.areaMode == AreaModeEnum.Intersection) && toleratedArea.IntersectsWith(textCharBox)))
                            { filteredTextStringChars.Add(textChar); }
                        }
                        if (filteredTextStringChars.Count > 0)
                        { filteredAreaTextStrings.Add(filteredTextString); }
                    }
                }
            }
            return filteredAreasTextStrings;
        }

        /**
<summary>Converts text information into plain text.</summary>
<param name="textStrings">Text information to convert.</param>
<returns>Plain text.</returns>
*/
        public static string ToString(
          IDictionary<RectangleF?, IList<ITextString>> textStrings
          )
        { return ToString(textStrings, string.Empty, string.Empty); }

        /**
          <summary>Converts text information into plain text.</summary>
          <param name="textStrings">Text information to convert.</param>
          <param name="lineSeparator">Separator to apply on line break.</param>
          <param name="areaSeparator">Separator to apply on area break.</param>
          <returns>Plain text.</returns>
        */
        public static string ToString(
          IDictionary<RectangleF?, IList<ITextString>> textStrings,
          string lineSeparator,
          string areaSeparator
          )
        {
            var textBuilder = new StringBuilder();
            foreach (var areaTextStrings in textStrings.Values)
            {
                if (textBuilder.Length > 0)
                { _ = textBuilder.Append(areaSeparator); }

                foreach (var textString in areaTextStrings)
                { _ = textBuilder.Append(textString.Text).Append(lineSeparator); }
            }
            return textBuilder.ToString();
        }

        /**
<summary>Gets the text-to-area matching mode.</summary>
*/
        public AreaModeEnum AreaMode
        {
            get => this.areaMode;
            set => this.areaMode = value;
        }

        /**
          <summary>Gets the graphic areas whose text has to be extracted.</summary>
        */
        public IList<RectangleF> Areas
        {
            get => this.areas;
            set => this.areas = (value == null) ? new List<RectangleF>() : new List<RectangleF>(value);
        }

        /**
          <summary>Gets the admitted outer area (in points) for containment matching purposes.</summary>
          <remarks>This measure is useful to ensure that text whose boxes overlap with the area bounds
          is not excluded from the match.</remarks>
        */
        public float AreaTolerance
        {
            get => this.areaTolerance;
            set => this.areaTolerance = value;
        }

        /**
          <summary>Gets/Sets whether the text strings have to be dehyphenated.</summary>
        */
        public bool Dehyphenated
        {
            get => this.dehyphenated;
            set
            {
                this.dehyphenated = value;
                if (this.dehyphenated)
                { this.Sorted = true; }
            }
        }

        /**
          <summary>Gets/Sets whether the text strings have to be sorted.</summary>
        */
        public bool Sorted
        {
            get => this.sorted;
            set
            {
                this.sorted = value;
                if (!this.sorted)
                { this.Dehyphenated = false; }
            }
        }
        /**
  <summary>Text-to-area matching mode.</summary>
*/
        public enum AreaModeEnum
        {
            /**
              <summary>Text string must be contained by the area.</summary>
            */
            Containment,
            /**
              <summary>Text string must intersect the area.</summary>
            */
            Intersection
        }

        /**
          <summary>Text filter by interval.</summary>
          <remarks>Iterated intervals MUST be ordered.</remarks>
        */
        public interface IIntervalFilter
          : IEnumerator<Interval<int>>
        {
            /**
              <summary>Notifies current matching.</summary>
              <param name="interval">Current interval.</param>
              <param name="match">Text string matching the current interval.</param>
            */
            void Process(
              Interval<int> interval,
              ITextString match
              );
        }

        private class IntervalFilter
          : IIntervalFilter
        {
            private int index = 0;
            private readonly IList<Interval<int>> intervals;

            private readonly IList<ITextString> textStrings = new List<ITextString>();

            public IntervalFilter(
              IList<Interval<int>> intervals
              )
            { this.intervals = intervals; }

            object IEnumerator.Current => this.Current;

            public void Dispose(
              )
            {/* NOOP */}

            public bool MoveNext(
              )
            { return ++this.index < this.intervals.Count; }

            public void Process(
              Interval<int> interval,
              ITextString match
              )
            { this.textStrings.Add(match); }

            public void Reset(
              )
            { throw new NotSupportedException(); }

            public Interval<int> Current => this.intervals[this.index];

            public IList<ITextString> TextStrings => this.textStrings;
        }

        /**
          <summary>Text string.</summary>
          <remarks>This is typically used to assemble contiguous raw text strings
          laying on the same line.</remarks>
        */
        private class TextString
          : ITextString
        {
            private readonly List<TextChar> textChars = new List<TextChar>();

            public override string ToString(
              )
            { return this.Text; }

            public RectangleF? Box
            {
                get
                {
                    RectangleF? box = null;
                    foreach (var textChar in this.textChars)
                    {
                        if (!box.HasValue)
                        { box = (RectangleF?)textChar.Box; }
                        else
                        { box = RectangleF.Union(box.Value, textChar.Box); }
                    }
                    return box;
                }
            }

            public string Text
            {
                get
                {
                    var textBuilder = new StringBuilder();
                    foreach (var textChar in this.textChars)
                    { _ = textBuilder.Append(textChar); }
                    return textBuilder.ToString();
                }
            }

            public List<TextChar> TextChars => this.textChars;
        }

        /**
          <summary>Text string position comparer.</summary>
        */
        private class TextStringPositionComparer<T>
          : IComparer<T>
          where T : ITextString
        {

            public int Compare(
T textString1,
T textString2
)
            {
                var box1 = textString1.Box.Value;
                var box2 = textString2.Box.Value;
                if (IsOnTheSameLine(box1, box2))
                {
                    /*
                      [FIX:55:0.1.3] In order not to violate the transitive condition, equivalence on x-axis
                      MUST fall back on y-axis comparison.
                    */
                    var xCompare = box1.X.CompareTo(box2.X);
                    if (xCompare != 0)
                    {
                        return xCompare;
                    }
                }
                return box1.Y.CompareTo(box2.Y);
            }
            /**
  <summary>Gets whether the specified boxes lay on the same text line.</summary>
*/
            public static bool IsOnTheSameLine(
              RectangleF box1,
              RectangleF box2
              )
            {
                /*
                  NOTE: In order to consider the two boxes being on the same line,
                  we apply a simple rule of thumb: at least 25% of a box's height MUST
                  lay on the horizontal projection of the other one.
                */
                double minHeight = Math.Min(box1.Height, box2.Height);
                var yThreshold = minHeight * .75;
                return ((box1.Y > box2.Y - yThreshold)
                    && (box1.Y < box2.Bottom + yThreshold - minHeight))
                  || ((box2.Y > box1.Y - yThreshold)
                    && (box2.Y < box1.Bottom + yThreshold - minHeight));
            }
        }
    }
}
