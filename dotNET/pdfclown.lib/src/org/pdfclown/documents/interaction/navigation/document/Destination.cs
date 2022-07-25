/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
    * Furkan Duman (bug reporter [FIX:66], https://sourceforge.net/u/fduman/)

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

namespace org.pdfclown.documents.interaction.navigation.document
{
    using System;
    using System.Drawing;

    using org.pdfclown.objects;
    using org.pdfclown.util;

    /**
      <summary>Interaction target [PDF:1.6:8.2.1].</summary>
      <remarks>
        It represents a particular view of a document, consisting of the following items:
        <list type="bullet">
          <item>the page of the document to be displayed;</item>
          <item>the location of the document window on that page;</item>
          <item>the magnification (zoom) factor to use when displaying the page.</item>
        </list>
      </remarks>
    */
    [PDF(VersionEnum.PDF10)]
    public abstract class Destination
      : PdfObjectWrapper<PdfArray>,
        IPdfNamedObjectWrapper
    {

        protected Destination(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
<summary>Creates a new destination within the given document context.</summary>
<param name="context">Document context.</param>
<param name="page">Page reference. It may be either a <see cref="Page"/> or a page index (int).
</param>
<param name="mode">Destination mode.</param>
<param name="location">Destination location.</param>
<param name="zoom">Magnification factor to use when displaying the page.</param>
*/
        protected Destination(
          Document context,
          object page,
          ModeEnum mode,
          object location,
          double? zoom
          ) : base(context, new PdfArray(null, null))
        {
            this.Page = page;
            this.Mode = mode;
            this.Location = location;
            this.Zoom = zoom;
        }

        /**
<summary>Wraps a destination base object into a destination object.</summary>
<param name="baseObject">Destination base object.</param>
<returns>Destination object associated to the base object.</returns>
*/
        public static Destination Wrap(
          PdfDirectObject baseObject
          )
        {
            if (baseObject == null)
            {
                return null;
            }

            var dataObject = (PdfArray)baseObject.Resolve();
            var pageObject = dataObject[0];
            if (pageObject is PdfReference)
            {
                return new LocalDestination(baseObject);
            }
            else if (pageObject is PdfInteger)
            {
                return new RemoteDestination(baseObject);
            }
            else
            {
                throw new ArgumentException("Not a valid destination object.", nameof(baseObject));
            }
        }

        /**
<summary>Gets/Sets the page location.</summary>
*/
        public object Location
        {
            get
            {
                // [FIX:66] Invalid cast exception on number unboxing.
                switch (this.Mode)
                {
                    case ModeEnum.FitBoundingBoxHorizontal:
                    case ModeEnum.FitBoundingBoxVertical:
                    case ModeEnum.FitHorizontal:
                    case ModeEnum.FitVertical:
                        return Convert.ToSingle(PdfSimpleObject<object>.GetValue(this.BaseDataObject[2], double.NaN));
                    case ModeEnum.FitRectangle:
                        var left = Convert.ToSingle(PdfSimpleObject<object>.GetValue(this.BaseDataObject[2], double.NaN));
                        var top = Convert.ToSingle(PdfSimpleObject<object>.GetValue(this.BaseDataObject[5], double.NaN));
                        var width = Convert.ToSingle(PdfSimpleObject<object>.GetValue(this.BaseDataObject[4], double.NaN)) - left;
                        var height = Convert.ToSingle(PdfSimpleObject<object>.GetValue(this.BaseDataObject[3], double.NaN)) - top;
                        return new RectangleF(left, top, width, height);
                    case ModeEnum.XYZ:
                        return new PointF(
                          Convert.ToSingle(PdfSimpleObject<object>.GetValue(this.BaseDataObject[2], double.NaN)),
                          Convert.ToSingle(PdfSimpleObject<object>.GetValue(this.BaseDataObject[3], double.NaN))
                          );
                    default:
                        return null;
                }
            }
            set
            {
                var baseDataObject = this.BaseDataObject;
                switch (this.Mode)
                {
                    case ModeEnum.FitBoundingBoxHorizontal:
                    case ModeEnum.FitBoundingBoxVertical:
                    case ModeEnum.FitHorizontal:
                    case ModeEnum.FitVertical:
                        baseDataObject[2] = PdfReal.Get(Convert.ToDouble(value));
                        break;
                    case ModeEnum.FitRectangle:
                        var rectangle = (RectangleF)value;
                        baseDataObject[2] = PdfReal.Get(rectangle.X);
                        baseDataObject[3] = PdfReal.Get(rectangle.Y);
                        baseDataObject[4] = PdfReal.Get(rectangle.Right);
                        baseDataObject[5] = PdfReal.Get(rectangle.Bottom);
                        break;
                    case ModeEnum.XYZ:
                        var point = (PointF)value;
                        baseDataObject[2] = PdfReal.Get(point.X);
                        baseDataObject[3] = PdfReal.Get(point.Y);
                        break;
                    default:
                        /* NOOP */
                        break;
                }
            }
        }

        /**
          <summary>Gets the destination mode.</summary>
        */
        public ModeEnum Mode
        {
            get => ModeEnumExtension.Get((PdfName)this.BaseDataObject[1]).Value;
            set
            {
                var baseDataObject = this.BaseDataObject;

                baseDataObject[1] = value.GetName();

                // Adjusting parameter list...
                int parametersCount;
                switch (value)
                {
                    case ModeEnum.Fit:
                    case ModeEnum.FitBoundingBox:
                        parametersCount = 2;
                        break;
                    case ModeEnum.FitBoundingBoxHorizontal:
                    case ModeEnum.FitBoundingBoxVertical:
                    case ModeEnum.FitHorizontal:
                    case ModeEnum.FitVertical:
                        parametersCount = 3;
                        break;
                    case ModeEnum.XYZ:
                        parametersCount = 5;
                        break;
                    case ModeEnum.FitRectangle:
                        parametersCount = 6;
                        break;
                    default:
                        throw new NotSupportedException($"Mode unknown: {value}");
                }
                while (baseDataObject.Count < parametersCount)
                { baseDataObject.Add(null); }
                while (baseDataObject.Count > parametersCount)
                { baseDataObject.RemoveAt(baseDataObject.Count - 1); }
            }
        }

        public PdfString Name => this.RetrieveName();

        public PdfDirectObject NamedBaseObject => this.RetrieveNamedBaseObject();

        /**
          <summary>Gets/Sets the target page reference.</summary>
        */
        public abstract object Page
        {
            get;
            set;
        }

        /**
          <summary>Gets the magnification factor to use when displaying the page.</summary>
        */
        public double? Zoom
        {
            get
            {
                switch (this.Mode)
                {
                    case ModeEnum.XYZ:
                        return PdfSimpleObject<object>.GetDoubleValue(this.BaseDataObject[4]);
                    default:
                        return null;
                }
            }
            set
            {
                switch (this.Mode)
                {
                    case ModeEnum.XYZ:
                        this.BaseDataObject[4] = PdfReal.Get(value);
                        break;
                    default:
                        /* NOOP */
                        break;
                }
            }
        }
        /**
  <summary>Destination mode [PDF:1.6:8.2.1].</summary>
*/
        public enum ModeEnum
        {
            /**
              <summary>Display the page at the given upper-left position,
              applying the given magnification.</summary>
              <remarks>
                View parameters:
                <list type="number">
                  <item>left coordinate</item>
                  <item>top coordinate</item>
                  <item>zoom</item>
                </list>
              </remarks>
            */
            XYZ,
            /**
              <summary>Display the page with its contents magnified just enough to fit
              the entire page within the window both horizontally and vertically.</summary>
              <remarks>No view parameters.</remarks>
            */
            Fit,
            /**
              <summary>Display the page with the vertical coordinate <code>top</code> positioned
              at the top edge of the window and the contents of the page magnified
              just enough to fit the entire width of the page within the window.</summary>
              <remarks>
                View parameters:
                <list type="number">
                  <item>top coordinate</item>
                </list>
              </remarks>
            */
            FitHorizontal,
            /**
              <summary>Display the page with the horizontal coordinate <code>left</code> positioned
              at the left edge of the window and the contents of the page magnified
              just enough to fit the entire height of the page within the window.</summary>
              <remarks>
                View parameters:
                <list type="number">
                  <item>left coordinate</item>
                </list>
              </remarks>
            */
            FitVertical,
            /**
              <summary>Display the page with its contents magnified just enough to fit
              the rectangle specified by the given coordinates entirely
              within the window both horizontally and vertically.</summary>
              <remarks>
                View parameters:
                <list type="number">
                  <item>left coordinate</item>
                  <item>bottom coordinate</item>
                  <item>right coordinate</item>
                  <item>top coordinate</item>
                </list>
              </remarks>
            */
            FitRectangle,
            /**
              <summary>Display the page with its contents magnified just enough to fit
              its bounding box entirely within the window both horizontally and vertically.</summary>
              <remarks>No view parameters.</remarks>
            */
            FitBoundingBox,
            /**
              <summary>Display the page with the vertical coordinate <code>top</code> positioned
              at the top edge of the window and the contents of the page magnified
              just enough to fit the entire width of its bounding box within the window.</summary>
              <remarks>
                View parameters:
                <list type="number">
                  <item>top coordinate</item>
                </list>
              </remarks>
            */
            FitBoundingBoxHorizontal,
            /**
              <summary>Display the page with the horizontal coordinate <code>left</code> positioned
              at the left edge of the window and the contents of the page magnified
              just enough to fit the entire height of its bounding box within the window.</summary>
              <remarks>
                View parameters:
                <list type="number">
                  <item>left coordinate</item>
                </list>
              </remarks>
            */
            FitBoundingBoxVertical
        }
    }

    internal static class ModeEnumExtension
    {
        private static readonly BiDictionary<Destination.ModeEnum, PdfName> codes;

        static ModeEnumExtension()
        {
            codes = new BiDictionary<Destination.ModeEnum, PdfName>();
            codes[Destination.ModeEnum.Fit] = PdfName.Fit;
            codes[Destination.ModeEnum.FitBoundingBox] = PdfName.FitB;
            codes[Destination.ModeEnum.FitBoundingBoxHorizontal] = PdfName.FitBH;
            codes[Destination.ModeEnum.FitBoundingBoxVertical] = PdfName.FitBV;
            codes[Destination.ModeEnum.FitHorizontal] = PdfName.FitH;
            codes[Destination.ModeEnum.FitRectangle] = PdfName.FitR;
            codes[Destination.ModeEnum.FitVertical] = PdfName.FitV;
            codes[Destination.ModeEnum.XYZ] = PdfName.XYZ;
        }

        public static Destination.ModeEnum? Get(
          PdfName name
          )
        {
            if (name == null)
            {
                return null;
            }

            Destination.ModeEnum? mode = codes.GetKey(name);
            if (!mode.HasValue)
            {
                throw new NotSupportedException($"Mode unknown: {name}");
            }

            return mode;
        }

        public static PdfName GetName(
          this Destination.ModeEnum mode
          )
        { return codes[mode]; }
    }
}