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

namespace org.pdfclown.documents.contents
{
    using System.Collections.Generic;

    using org.pdfclown.documents.contents.fonts;
    using org.pdfclown.objects;

    /**
      <summary>Graphics state parameters [PDF:1.6:4.3.4].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class ExtGState
      : PdfObjectWrapper<PdfDictionary>
    {
        internal static readonly IList<BlendModeEnum> DefaultBlendMode = new BlendModeEnum[0];

        internal ExtGState(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public ExtGState(
Document context
) : base(context, new PdfDictionary())
        { }

        public void ApplyTo(
          ContentScanner.GraphicsState state
          )
        {
            foreach (var parameterName in this.BaseDataObject.Keys)
            {
                if (parameterName.Equals(PdfName.Font))
                {
                    state.Font = this.Font;
                    state.FontSize = this.FontSize.Value;
                }
                else if (parameterName.Equals(PdfName.LC))
                { state.LineCap = this.LineCap.Value; }
                else if (parameterName.Equals(PdfName.D))
                { state.LineDash = this.LineDash; }
                else if (parameterName.Equals(PdfName.LJ))
                { state.LineJoin = this.LineJoin.Value; }
                else if (parameterName.Equals(PdfName.LW))
                { state.LineWidth = this.LineWidth.Value; }
                else if (parameterName.Equals(PdfName.ML))
                { state.MiterLimit = this.MiterLimit.Value; }
                else if (parameterName.Equals(PdfName.BM))
                { state.BlendMode = this.BlendMode; }
                //TODO:extend supported parameters!!!
            }
        }

        /**
<summary>Wraps the specified base object into a graphics state parameter dictionary object.
</summary>
<param name="baseObject">Base object of a graphics state parameter dictionary object.</param>
<returns>Graphics state parameter dictionary object corresponding to the base object.</returns>
*/
        public static ExtGState Wrap(
          PdfDirectObject baseObject
          )
        { return (baseObject != null) ? new ExtGState(baseObject) : null; }

        /**
<summary>Gets/Sets whether the current soft mask and alpha constant are to be interpreted as
shape values instead of opacity values.</summary>
*/
        [PDF(VersionEnum.PDF14)]
        public bool AlphaShape
        {
            get => (bool)PdfSimpleObject<object>.GetValue(this.BaseDataObject[PdfName.AIS], false);
            set => this.BaseDataObject[PdfName.AIS] = PdfBoolean.Get(value);
        }

        /**
          <summary>Gets/Sets the blend mode to be used in the transparent imaging model [PDF:1.7:7.2.4].
          </summary>
        */
        [PDF(VersionEnum.PDF14)]
        public IList<BlendModeEnum> BlendMode
        {
            get
            {
                var blendModeObject = this.BaseDataObject[PdfName.BM];
                if (blendModeObject == null)
                {
                    return DefaultBlendMode;
                }

                IList<BlendModeEnum> blendMode = new List<BlendModeEnum>();
                if (blendModeObject is PdfName)
                { blendMode.Add(BlendModeEnumExtension.Get((PdfName)blendModeObject).Value); }
                else // MUST be an array.
                {
                    foreach (var alternateBlendModeObject in (PdfArray)blendModeObject)
                    { blendMode.Add(BlendModeEnumExtension.Get((PdfName)alternateBlendModeObject).Value); }
                }
                return blendMode;
            }
            set
            {
                PdfDirectObject blendModeObject;
                if ((value == null) || (value.Count == 0))
                { blendModeObject = null; }
                else if (value.Count == 1)
                { blendModeObject = value[0].GetName(); }
                else
                {
                    var blendModeArray = new PdfArray();
                    foreach (var blendMode in value)
                    { blendModeArray.Add(blendMode.GetName()); }
                    blendModeObject = blendModeArray;
                }
                this.BaseDataObject[PdfName.BM] = blendModeObject;
            }
        }

        /**
          <summary>Gets/Sets the nonstroking alpha constant, specifying the constant shape or constant
          opacity value to be used for nonstroking operations in the transparent imaging model
          [PDF:1.7:7.2.6].</summary>
        */
        [PDF(VersionEnum.PDF14)]
        public double? FillAlpha
        {
            get => (double?)PdfSimpleObject<PdfObject>.GetValue(this.BaseDataObject[PdfName.ca]);
            set => this.BaseDataObject[PdfName.ca] = PdfReal.Get(value);
        }

        [PDF(VersionEnum.PDF13)]
        public Font Font
        {
            get
            {
                var fontObject = (PdfArray)this.BaseDataObject[PdfName.Font];
                return (fontObject != null) ? Font.Wrap(fontObject[0]) : null;
            }
            set
            {
                var fontObject = (PdfArray)this.BaseDataObject[PdfName.Font];
                if (fontObject == null)
                { fontObject = new PdfArray(PdfObjectWrapper.GetBaseObject(value), PdfInteger.Default); }
                else
                { fontObject[0] = PdfObjectWrapper.GetBaseObject(value); }
                this.BaseDataObject[PdfName.Font] = fontObject;
            }
        }

        [PDF(VersionEnum.PDF13)]
        public double? FontSize
        {
            get
            {
                var fontObject = (PdfArray)this.BaseDataObject[PdfName.Font];
                return (fontObject != null) ? ((IPdfNumber)fontObject[1]).RawValue : ((double?)null);
            }
            set
            {
                var fontObject = (PdfArray)this.BaseDataObject[PdfName.Font];
                if (fontObject == null)
                { fontObject = new PdfArray(null, PdfReal.Get(value)); }
                else
                { fontObject[1] = PdfReal.Get(value); }
                this.BaseDataObject[PdfName.Font] = fontObject;
            }
        }

        [PDF(VersionEnum.PDF13)]
        public LineCapEnum? LineCap
        {
            get
            {
                var lineCapObject = (PdfInteger)this.BaseDataObject[PdfName.LC];
                return (lineCapObject != null) ? ((LineCapEnum)lineCapObject.RawValue) : ((LineCapEnum?)null);
            }
            set => this.BaseDataObject[PdfName.LC] = value.HasValue ? PdfInteger.Get(value.Value) : null;
        }

        [PDF(VersionEnum.PDF13)]
        public LineDash LineDash
        {
            get
            {
                var lineDashObject = (PdfArray)this.BaseDataObject[PdfName.D];
                return (lineDashObject != null) ? LineDash.Get((PdfArray)lineDashObject[0], (IPdfNumber)lineDashObject[1]) : null;
            }
            set
            {
                var lineDashObject = new PdfArray();
                var dashArrayObject = new PdfArray();
                foreach (var dashArrayItem in value.DashArray)
                { dashArrayObject.Add(PdfReal.Get(dashArrayItem)); }
                lineDashObject.Add(dashArrayObject);
                lineDashObject.Add(PdfReal.Get(value.DashPhase));
                this.BaseDataObject[PdfName.D] = lineDashObject;
            }
        }

        [PDF(VersionEnum.PDF13)]
        public LineJoinEnum? LineJoin
        {
            get
            {
                var lineJoinObject = (PdfInteger)this.BaseDataObject[PdfName.LJ];
                return (lineJoinObject != null) ? ((LineJoinEnum)lineJoinObject.RawValue) : ((LineJoinEnum?)null);
            }
            set => this.BaseDataObject[PdfName.LJ] = value.HasValue ? PdfInteger.Get(value.Value) : null;
        }

        [PDF(VersionEnum.PDF13)]
        public double? LineWidth
        {
            get
            {
                var lineWidthObject = (IPdfNumber)this.BaseDataObject[PdfName.LW];
                return (lineWidthObject != null) ? lineWidthObject.RawValue : ((double?)null);
            }
            set => this.BaseDataObject[PdfName.LW] = PdfReal.Get(value);
        }

        [PDF(VersionEnum.PDF13)]
        public double? MiterLimit
        {
            get
            {
                var miterLimitObject = (IPdfNumber)this.BaseDataObject[PdfName.ML];
                return (miterLimitObject != null) ? miterLimitObject.RawValue : ((double?)null);
            }
            set => this.BaseDataObject[PdfName.ML] = PdfReal.Get(value);
        }

        /**
          <summary>Gets/Sets the stroking alpha constant, specifying the constant shape or constant
          opacity value to be used for stroking operations in the transparent imaging model
          [PDF:1.7:7.2.6].</summary>
        */
        [PDF(VersionEnum.PDF14)]
        public double? StrokeAlpha
        {
            get => (double?)PdfSimpleObject<PdfObject>.GetValue(this.BaseDataObject[PdfName.CA]);
            set => this.BaseDataObject[PdfName.CA] = PdfReal.Get(value);
        }
    }
}