/*
  Copyright 2010-2012 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.Drawing;

    /**
      <summary>PDF rectangle object [PDF:1.6:3.8.4].</summary>
      <remarks>
        <para>Rectangles are described by two diagonally-opposite corners. Corner pairs which don't
        respect the canonical form (lower-left and upper-right) are automatically normalized to
        provide a consistent representation.</para>
        <para>Coordinates are expressed within the PDF coordinate space (lower-left origin and
        positively-oriented axes).</para>
      </remarks>
    */
    public sealed class Rectangle
      : PdfObjectWrapper<PdfArray>
    {

        private Rectangle(
          PdfDirectObject baseObject
          ) : base(Normalize((PdfArray)baseObject.Resolve()))
        { }

        public Rectangle(
RectangleF rectangle
) : this(
rectangle.Left,
rectangle.Bottom,
rectangle.Width,
rectangle.Height
)
        { }

        public Rectangle(
          PointF lowerLeft,
          PointF upperRight
          ) : this(
            lowerLeft.X,
            upperRight.Y,
            upperRight.X - lowerLeft.X,
            upperRight.Y - lowerLeft.Y
            )
        { }

        public Rectangle(
          double left,
          double top,
          double width,
          double height
          ) : this(
            new PdfArray(
              new PdfDirectObject[]
              {
            PdfReal.Get(left), // Left (X).
            PdfReal.Get(top - height), // Bottom (Y).
            PdfReal.Get(left + width), // Right.
            PdfReal.Get(top) // Top.
              }
              )
            )
        { }

        private static PdfArray Normalize(
  PdfArray rectangle
  )
        {
            if (rectangle[0].CompareTo(rectangle[2]) > 0)
            {
                var leftCoordinate = rectangle[2];
                rectangle[2] = rectangle[0];
                rectangle[0] = leftCoordinate;
            }
            if (rectangle[1].CompareTo(rectangle[3]) > 0)
            {
                var bottomCoordinate = rectangle[3];
                rectangle[3] = rectangle[1];
                rectangle[1] = bottomCoordinate;
            }
            return rectangle;
        }

        public RectangleF ToRectangleF(
          )
        { return new RectangleF((float)this.X, (float)this.Y, (float)this.Width, (float)this.Height); }
        public static Rectangle Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new Rectangle(baseObject) : null; }

        public double Bottom
        {
            get => ((IPdfNumber)this.BaseDataObject[1]).RawValue;
            set => this.BaseDataObject[1] = PdfReal.Get(value);
        }

        public double Height
        {
            get => this.Top - this.Bottom;
            set => this.Bottom = this.Top - value;
        }

        public double Left
        {
            get => ((IPdfNumber)this.BaseDataObject[0]).RawValue;
            set => this.BaseDataObject[0] = PdfReal.Get(value);
        }

        public double Right
        {
            get => ((IPdfNumber)this.BaseDataObject[2]).RawValue;
            set => this.BaseDataObject[2] = PdfReal.Get(value);
        }

        public double Top
        {
            get => ((IPdfNumber)this.BaseDataObject[3]).RawValue;
            set => this.BaseDataObject[3] = PdfReal.Get(value);
        }

        public double Width
        {
            get => this.Right - this.Left;
            set => this.Right = this.Left + value;
        }

        public double X
        {
            get => this.Left;
            set => this.Left = value;
        }

        public double Y
        {
            get => this.Bottom;
            set => this.Bottom = value;
        }
    }
}