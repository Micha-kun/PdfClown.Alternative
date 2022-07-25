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

namespace org.pdfclown.util.math.geom
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /**
      <summary>Quadrilateral shape.</summary>
    */
    public class Quad
    {

        private GraphicsPath path;

        private PointF[] points;

        public Quad(
  params PointF[] points
  )
        { this.Points = points; }

        private GraphicsPath Path
        {
            get
            {
                if (this.path == null)
                {
                    this.path = new GraphicsPath(FillMode.Alternate);
                    this.path.AddPolygon(this.points);
                }
                return this.path;
            }
        }

        public bool Contains(
PointF point
)
        { return this.Path.IsVisible(point); }

        public bool Contains(
          float x,
          float y
          )
        { return this.Path.IsVisible(x, y); }
        public static Quad Get(
RectangleF rectangle
)
        { return new Quad(GetPoints(rectangle)); }

        public RectangleF GetBounds(
          )
        { return this.Path.GetBounds(); }

        public GraphicsPathIterator GetPathIterator(
          )
        { return new GraphicsPathIterator(this.Path); }

        public static PointF[] GetPoints(
          RectangleF rectangle
          )
        {
            var points = new PointF[4];
            points[0] = new PointF(rectangle.Left, rectangle.Top);
            points[1] = new PointF(rectangle.Right, rectangle.Top);
            points[2] = new PointF(rectangle.Right, rectangle.Bottom);
            points[3] = new PointF(rectangle.Left, rectangle.Bottom);
            return points;
        }

        /**
          <summary>Expands the size of this quad stretching around its center.</summary>
          <param name="value">Expansion extent.</param>
          <returns>This quad.</returns>
        */
        public Quad Inflate(
          float value
          )
        { return this.Inflate(value, value); }

        /**
          <summary>Expands the size of this quad stretching around its center.</summary>
          <param name="valueX">Expansion's horizontal extent.</param>
          <param name="valueY">Expansion's vertical extent.</param>
          <returns>This quad.</returns>
        */
        public Quad Inflate(
          float valueX,
          float valueY
          )
        {
            var matrix = new Matrix();
            var oldBounds = this.Path.GetBounds();
            matrix.Translate(-oldBounds.X, -oldBounds.Y);
            this.path.Transform(matrix);
            matrix = new Matrix();
            matrix.Scale(1 + (valueX * 2 / oldBounds.Width), 1 + (valueY * 2 / oldBounds.Height));
            this.path.Transform(matrix);
            var newBounds = this.path.GetBounds();
            matrix = new Matrix();
            matrix.Translate(oldBounds.X - ((newBounds.Width - oldBounds.Width) / 2), oldBounds.Y - ((newBounds.Height - oldBounds.Height) / 2));
            this.path.Transform(matrix);

            this.points = this.path.PathPoints;
            return this;
        }

        public PointF[] Points
        {
            get => this.points;
            set
            {
                if (value.Length != 4)
                {
                    throw new ArgumentException("Cardinality MUST be 4.", nameof(this.points));
                }

                this.points = value;
                this.path = null;
            }
        }
    }
}