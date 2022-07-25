/*
  Copyright 2008-2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.annotations
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.documents.contents.composition;

    using org.pdfclown.documents.contents.xObjects;
    using org.pdfclown.objects;
    using org.pdfclown.util.math.geom;

    /**
      <summary>Text markup annotation [PDF:1.6:8.4.5].</summary>
      <remarks>It displays highlights, underlines, strikeouts, or jagged ("squiggly") underlines in
      the text of a document.</remarks>
    */
    [PDF(VersionEnum.PDF13)]
    public sealed class TextMarkup
      : Markup
    {

        private static readonly PdfName HighlightExtGStateName = new PdfName("highlight");

        private static readonly Dictionary<MarkupTypeEnum, PdfName> MarkupTypeEnumCodes;

        static TextMarkup()
        {
            MarkupTypeEnumCodes = new Dictionary<MarkupTypeEnum, PdfName>();
            MarkupTypeEnumCodes[MarkupTypeEnum.Highlight] = PdfName.Highlight;
            MarkupTypeEnumCodes[MarkupTypeEnum.Squiggly] = PdfName.Squiggly;
            MarkupTypeEnumCodes[MarkupTypeEnum.StrikeOut] = PdfName.StrikeOut;
            MarkupTypeEnumCodes[MarkupTypeEnum.Underline] = PdfName.Underline;
        }

        internal TextMarkup(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
<summary>Creates a new text markup on the specified page, making it printable by default.
</summary>
<param name="page">Page to annotate.</param>
<param name="markupBox">Quadrilateral encompassing a word or group of contiguous words in the
text underlying the annotation.</param>
<param name="text">Annotation text.</param>
<param name="markupType">Markup type.</param>
*/
        public TextMarkup(
          Page page,
          Quad markupBox,
          string text,
          MarkupTypeEnum markupType
          ) : this(page, new List<Quad> { markupBox }, text, markupType)
        { }

        /**
          <summary>Creates a new text markup on the specified page, making it printable by default.
          </summary>
          <param name="page">Page to annotate.</param>
          <param name="markupBoxes">Quadrilaterals encompassing a word or group of contiguous words in
          the text underlying the annotation.</param>
          <param name="text">Annotation text.</param>
          <param name="markupType">Markup type.</param>
        */
        public TextMarkup(
          Page page,
          IList<Quad> markupBoxes,
          string text,
          MarkupTypeEnum markupType
          ) : base(
            page,
            ToCode(markupType),
            markupBoxes[0].GetBounds(),
            text
            )
        {
            this.MarkupType = markupType;
            this.MarkupBoxes = markupBoxes;
            this.Printable = true;
        }

        private static float GetMarkupBoxMargin(
  float boxHeight
  )
        { return boxHeight * .25f; }

        /*
  TODO: refresh should happen just before serialization, on document event (e.g. OnWrite())
*/
        private void RefreshAppearance(
          )
        {
            FormXObject normalAppearance;
            var box = org.pdfclown.objects.Rectangle.Wrap(this.BaseDataObject[PdfName.Rect]).ToRectangleF();
            var normalAppearances = this.Appearance.Normal;
            normalAppearance = normalAppearances[null];
            if (normalAppearance != null)
            {
                normalAppearance.Box = box;
                normalAppearance.BaseDataObject.Body.SetLength(0);
            }
            else
            { normalAppearances[null] = normalAppearance = new FormXObject(this.Document, box); }

            var composer = new PrimitiveComposer(normalAppearance);
            var yOffset = box.Height - this.Page.Box.Height;
            var markupType = this.MarkupType;
            switch (markupType)
            {
                case MarkupTypeEnum.Highlight:
                    ExtGState defaultExtGState;
                    var extGStates = normalAppearance.Resources.ExtGStates;
                    defaultExtGState = extGStates[HighlightExtGStateName];
                    if (defaultExtGState == null)
                    {
                        if (extGStates.Count > 0)
                        { extGStates.Clear(); }

                        extGStates[HighlightExtGStateName] = defaultExtGState = new ExtGState(this.Document);
                        defaultExtGState.AlphaShape = false;
                        defaultExtGState.BlendMode = new List<BlendModeEnum>(new BlendModeEnum[] { BlendModeEnum.Multiply });
                    }

                    composer.ApplyState(defaultExtGState);
                    composer.SetFillColor(this.Color);
                    foreach (var markupBox in this.MarkupBoxes)
                    {
                        var points = markupBox.Points;
                        var markupBoxHeight = points[3].Y - points[0].Y;
                        var markupBoxMargin = GetMarkupBoxMargin(markupBoxHeight);
                        composer.DrawCurve(
                          new PointF(points[3].X, points[3].Y + yOffset),
                          new PointF(points[0].X, points[0].Y + yOffset),
                          new PointF(points[3].X - markupBoxMargin, points[3].Y - markupBoxMargin + yOffset),
                          new PointF(points[0].X - markupBoxMargin, points[0].Y + markupBoxMargin + yOffset)
                          );
                        composer.DrawLine(
                          new PointF(points[1].X, points[1].Y + yOffset)
                          );
                        composer.DrawCurve(
                          new PointF(points[2].X, points[2].Y + yOffset),
                          new PointF(points[1].X + markupBoxMargin, points[1].Y + markupBoxMargin + yOffset),
                          new PointF(points[2].X + markupBoxMargin, points[2].Y - markupBoxMargin + yOffset)
                          );
                        composer.Fill();
                    }
                    break;
                case MarkupTypeEnum.Squiggly:
                    composer.SetStrokeColor(this.Color);
                    composer.SetLineCap(LineCapEnum.Round);
                    composer.SetLineJoin(LineJoinEnum.Round);
                    foreach (var markupBox in this.MarkupBoxes)
                    {
                        var points = markupBox.Points;
                        var markupBoxHeight = points[3].Y - points[0].Y;
                        var lineWidth = markupBoxHeight * .05f;
                        var step = markupBoxHeight * .125f;
                        var boxXOffset = points[3].X;
                        var boxYOffset = points[3].Y + yOffset - lineWidth;
                        var phase = false;
                        composer.SetLineWidth(lineWidth);
                        for (float x = 0, xEnd = points[2].X - boxXOffset; (x < xEnd) || !phase; x += step)
                        {
                            var point = new PointF(x + boxXOffset, (phase ? (-step) : 0) + boxYOffset);
                            if (x == 0)
                            { composer.StartPath(point); }
                            else
                            { composer.DrawLine(point); }
                            phase = !phase;
                        }
                    }
                    composer.Stroke();
                    break;
                case MarkupTypeEnum.StrikeOut:
                case MarkupTypeEnum.Underline:
                    composer.SetStrokeColor(this.Color);
                    float lineYRatio;
                    switch (markupType)
                    {
                        case MarkupTypeEnum.StrikeOut:
                            lineYRatio = .5f;
                            break;
                        case MarkupTypeEnum.Underline:
                            lineYRatio = .9f;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    foreach (var markupBox in this.MarkupBoxes)
                    {
                        var points = markupBox.Points;
                        var markupBoxHeight = points[3].Y - points[0].Y;
                        var boxYOffset = (markupBoxHeight * lineYRatio) + yOffset;
                        composer.SetLineWidth(markupBoxHeight * .065);
                        composer.DrawLine(
                          new PointF(points[3].X, points[0].Y + boxYOffset),
                          new PointF(points[2].X, points[1].Y + boxYOffset)
                          );
                    }
                    composer.Stroke();
                    break;
                default:
                    throw new NotImplementedException();
            }
            composer.Flush();
        }

        /**
<summary>Gets the code corresponding to the given value.</summary>
*/
        private static PdfName ToCode(
          MarkupTypeEnum value
          )
        { return MarkupTypeEnumCodes[value]; }

        /**
          <summary>Gets the markup type corresponding to the given value.</summary>
        */
        private static MarkupTypeEnum ToMarkupTypeEnum(
          PdfName value
          )
        {
            foreach (var markupType in MarkupTypeEnumCodes)
            {
                if (markupType.Value.Equals(value))
                {
                    return markupType.Key;
                }
            }
            throw new Exception("Invalid markup type.");
        }

        public override DeviceColor Color
        {
            set
            {
                base.Color = value;
                if (this.Appearance.Normal[null] != null)
                { this.RefreshAppearance(); }
            }
        }

        /**
          <summary>Gets/Sets the quadrilaterals encompassing a word or group of contiguous words in the
          text underlying the annotation.</summary>
        */
        public IList<Quad> MarkupBoxes
        {
            get
            {
                IList<Quad> markupBoxes = new List<Quad>();
                var quadPointsObject = (PdfArray)this.BaseDataObject[PdfName.QuadPoints];
                if (quadPointsObject != null)
                {
                    var pageHeight = this.Page.Box.Height;
                    for (
                      int index = 0,
                        length = quadPointsObject.Count;
                      index < length;
                      index += 8
                      )
                    {
                        /*
                          NOTE: Despite the spec prescription, Point 3 and Point 4 MUST be inverted.
                        */
                        markupBoxes.Add(
                          new Quad(
                            new PointF(
                              ((IPdfNumber)quadPointsObject[index]).FloatValue,
                              pageHeight - ((IPdfNumber)quadPointsObject[index + 1]).FloatValue
                              ),
                            new PointF(
                              ((IPdfNumber)quadPointsObject[index + 2]).FloatValue,
                              pageHeight - ((IPdfNumber)quadPointsObject[index + 3]).FloatValue
                              ),
                            new PointF(
                              ((IPdfNumber)quadPointsObject[index + 6]).FloatValue,
                              pageHeight - ((IPdfNumber)quadPointsObject[index + 7]).FloatValue
                              ),
                            new PointF(
                              ((IPdfNumber)quadPointsObject[index + 4]).FloatValue,
                              pageHeight - ((IPdfNumber)quadPointsObject[index + 5]).FloatValue
                              )
                            )
                          );
                    }
                }
                return markupBoxes;
            }
            set
            {
                var quadPointsObject = new PdfArray();
                var pageHeight = this.Page.Box.Height;
                var box = RectangleF.Empty;
                foreach (var markupBox in value)
                {
                    /*
                      NOTE: Despite the spec prescription, Point 3 and Point 4 MUST be inverted.
                    */
                    var markupBoxPoints = markupBox.Points;
                    quadPointsObject.Add(PdfReal.Get(markupBoxPoints[0].X)); // x1.
                    quadPointsObject.Add(PdfReal.Get(pageHeight - markupBoxPoints[0].Y)); // y1.
                    quadPointsObject.Add(PdfReal.Get(markupBoxPoints[1].X)); // x2.
                    quadPointsObject.Add(PdfReal.Get(pageHeight - markupBoxPoints[1].Y)); // y2.
                    quadPointsObject.Add(PdfReal.Get(markupBoxPoints[3].X)); // x4.
                    quadPointsObject.Add(PdfReal.Get(pageHeight - markupBoxPoints[3].Y)); // y4.
                    quadPointsObject.Add(PdfReal.Get(markupBoxPoints[2].X)); // x3.
                    quadPointsObject.Add(PdfReal.Get(pageHeight - markupBoxPoints[2].Y)); // y3.
                    if (box.IsEmpty)
                    { box = markupBox.GetBounds(); }
                    else
                    { box = RectangleF.Union(box, markupBox.GetBounds()); }
                }
                this.BaseDataObject[PdfName.QuadPoints] = quadPointsObject;

                /*
                  NOTE: Box width is expanded to make room for end decorations (e.g. rounded highlight caps).
                */
                var markupBoxMargin = GetMarkupBoxMargin(box.Height);
                box.X -= markupBoxMargin;
                box.Width += markupBoxMargin * 2;
                this.Box = box;

                this.RefreshAppearance();
            }
        }

        /**
          <summary>Gets/Sets the markup type.</summary>
        */
        public MarkupTypeEnum MarkupType
        {
            get => ToMarkupTypeEnum((PdfName)this.BaseDataObject[PdfName.Subtype]);
            set
            {
                this.BaseDataObject[PdfName.Subtype] = ToCode(value);
                switch (value)
                {
                    case MarkupTypeEnum.Highlight:
                        this.Color = new DeviceRGBColor(1, 1, 0);
                        break;
                    case MarkupTypeEnum.Squiggly:
                        this.Color = new DeviceRGBColor(1, 0, 0);
                        break;
                    default:
                        this.Color = new DeviceRGBColor(0, 0, 0);
                        break;
                }
            }
        }
        /**
  <summary>Markup type [PDF:1.6:8.4.5].</summary>
*/
        public enum MarkupTypeEnum
        {
            /**
              <summary>Highlight.</summary>
            */
            [PDF(VersionEnum.PDF13)]
            Highlight,
            /**
              <summary>Squiggly.</summary>
            */
            [PDF(VersionEnum.PDF14)]
            Squiggly,
            /**
              <summary>StrikeOut.</summary>
            */
            [PDF(VersionEnum.PDF13)]
            StrikeOut,
            /**
              <summary>Underline.</summary>
            */
            [PDF(VersionEnum.PDF13)]
            Underline
        };
    }
}