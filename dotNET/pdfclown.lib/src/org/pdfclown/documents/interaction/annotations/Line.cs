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
    using System.Drawing;

    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.objects;

    /**
      <summary>Line annotation [PDF:1.6:8.4.5].</summary>
      <remarks>It displays displays a single straight line on the page.
      When opened, it displays a pop-up window containing the text of the associated note.</remarks>
    */
    [PDF(VersionEnum.PDF13)]
    public sealed class Line
      : Markup
    {
        private static readonly double DefaultLeaderLineExtensionLength = 0;
        private static readonly double DefaultLeaderLineLength = 0;
        private static readonly LineEndStyleEnum DefaultLineEndStyle = LineEndStyleEnum.None;

        internal Line(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public Line(
Page page,
PointF startPoint,
PointF endPoint,
string text,
DeviceRGBColor color
) : base(
page,
PdfName.Line,
new RectangleF(
startPoint.X,
startPoint.Y,
endPoint.X - startPoint.X,
endPoint.Y - startPoint.Y
),
text
)
        {
            this.BaseDataObject[PdfName.L] = new PdfArray(new PdfDirectObject[] { PdfReal.Get(0), PdfReal.Get(0), PdfReal.Get(0), PdfReal.Get(0) });
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.Color = color;
        }

        private PdfArray EnsureLineEndStylesObject(
  )
        {
            var endStylesObject = (PdfArray)this.BaseDataObject[PdfName.LE];
            if (endStylesObject == null)
            {
                this.BaseDataObject[PdfName.LE] = endStylesObject = new PdfArray(
                  new PdfDirectObject[]
                  {
            DefaultLineEndStyle.GetName(),
            DefaultLineEndStyle.GetName()
                  }
                  );
            }
            return endStylesObject;
        }

        /**
<summary>Gets/Sets whether the contents should be shown as a caption.</summary>
*/
        public bool CaptionVisible
        {
            get
            {
                var captionVisibleObject = (PdfBoolean)this.BaseDataObject[PdfName.Cap];
                return (captionVisibleObject != null)
&& captionVisibleObject.BooleanValue;
            }
            set => this.BaseDataObject[PdfName.Cap] = PdfBoolean.Get(value);
        }

        /**
          <summary>Gets/Sets the ending coordinates.</summary>
        */
        public PointF EndPoint
        {
            get
            {
                var coordinatesObject = (PdfArray)this.BaseDataObject[PdfName.L];
                return new PointF(
                  (float)((IPdfNumber)coordinatesObject[2]).RawValue,
                  (float)((IPdfNumber)coordinatesObject[3]).RawValue
                  );
            }
            set
            {
                var coordinatesObject = (PdfArray)this.BaseDataObject[PdfName.L];
                coordinatesObject[2] = PdfReal.Get(value.X);
                coordinatesObject[3] = PdfReal.Get(this.Page.Box.Height - value.Y);
            }
        }

        /**
          <summary>Gets/Sets the style of the ending line ending.</summary>
        */
        public LineEndStyleEnum EndStyle
        {
            get
            {
                var endstylesObject = (PdfArray)this.BaseDataObject[PdfName.LE];
                return (endstylesObject != null)
                  ? LineEndStyleEnumExtension.Get((PdfName)endstylesObject[1])
                  : DefaultLineEndStyle;
            }
            set => this.EnsureLineEndStylesObject()[1] = value.GetName();
        }

        /**
          <summary>Gets/Sets the color with which to fill the interior of the annotation's line endings.</summary>
        */
        public DeviceRGBColor FillColor
        {
            get
            {
                var fillColorObject = (PdfArray)this.BaseDataObject[PdfName.IC];
                if (fillColorObject == null)
                {
                    return null;
                }
                //TODO:use baseObject constructor!!!
                return new DeviceRGBColor(
                  ((IPdfNumber)fillColorObject[0]).RawValue,
                  ((IPdfNumber)fillColorObject[1]).RawValue,
                  ((IPdfNumber)fillColorObject[2]).RawValue
                  );
            }
            set => this.BaseDataObject[PdfName.IC] = (PdfDirectObject)value.BaseDataObject;
        }

        /**
          <summary>Gets/Sets the length of leader line extensions that extend
          in the opposite direction from the leader lines.</summary>
        */
        public double LeaderLineExtensionLength
        {
            get
            {
                var leaderLineExtensionLengthObject = (IPdfNumber)this.BaseDataObject[PdfName.LLE];
                return (leaderLineExtensionLengthObject != null)
                  ? leaderLineExtensionLengthObject.RawValue
                  : DefaultLeaderLineExtensionLength;
            }
            set
            {
                this.BaseDataObject[PdfName.LLE] = PdfReal.Get(value);
                /*
                  NOTE: If leader line extension entry is present, leader line MUST be too.
                */
                if (!this.BaseDataObject.ContainsKey(PdfName.LL))
                { this.LeaderLineLength = DefaultLeaderLineLength; }
            }
        }

        /**
          <summary>Gets/Sets the length of leader lines that extend from each endpoint
          of the line perpendicular to the line itself.</summary>
          <remarks>A positive value means that the leader lines appear in the direction
          that is clockwise when traversing the line from its starting point
          to its ending point; a negative value indicates the opposite direction.</remarks>
        */
        public double LeaderLineLength
        {
            get
            {
                var leaderLineLengthObject = (IPdfNumber)this.BaseDataObject[PdfName.LL];
                return (leaderLineLengthObject != null)
                  ? (-leaderLineLengthObject.RawValue)
                  : DefaultLeaderLineLength;
            }
            set => this.BaseDataObject[PdfName.LL] = PdfReal.Get(-value);
        }

        /**
          <summary>Gets/Sets the starting coordinates.</summary>
        */
        public PointF StartPoint
        {
            get
            {
                var coordinatesObject = (PdfArray)this.BaseDataObject[PdfName.L];
                return new PointF(
                  (float)((IPdfNumber)coordinatesObject[0]).RawValue,
                  (float)((IPdfNumber)coordinatesObject[1]).RawValue
                  );
            }
            set
            {
                var coordinatesObject = (PdfArray)this.BaseDataObject[PdfName.L];
                coordinatesObject[0] = PdfReal.Get(value.X);
                coordinatesObject[1] = PdfReal.Get(this.Page.Box.Height - value.Y);
            }
        }

        /**
          <summary>Gets/Sets the style of the starting line ending.</summary>
        */
        public LineEndStyleEnum StartStyle
        {
            get
            {
                var endstylesObject = (PdfArray)this.BaseDataObject[PdfName.LE];
                return (endstylesObject != null)
                  ? LineEndStyleEnumExtension.Get((PdfName)endstylesObject[0])
                  : DefaultLineEndStyle;
            }
            set => this.EnsureLineEndStylesObject()[0] = value.GetName();
        }
    }
}