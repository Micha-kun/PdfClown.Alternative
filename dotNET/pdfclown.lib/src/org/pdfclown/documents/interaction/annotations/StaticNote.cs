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
    using System.Drawing;

    using org.pdfclown.objects;
    using org.pdfclown.util;

    /**
      <summary>Free text annotation [PDF:1.6:8.4.5].</summary>
      <remarks>It displays text directly on the page. Unlike an ordinary text annotation, a free text
      annotation has no open or closed state; instead of being displayed in a pop-up window, the text
      is always visible.</remarks>
    */
    [PDF(VersionEnum.PDF13)]
    public sealed class StaticNote
      : Markup
    {

        private static readonly JustificationEnum DefaultJustification = JustificationEnum.Left;
        private static readonly LineEndStyleEnum DefaultLineEndStyle = LineEndStyleEnum.None;

        internal StaticNote(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public StaticNote(
Page page,
RectangleF box,
string text
) : base(page, PdfName.FreeText, box, text)
        { }

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

        private TypeEnum? Type
        {
            get => StaticNoteTypeEnumExtension.Get(this.TypeBase);
            set => this.TypeBase = value.HasValue ? value.Value.GetName() : null;
        }

        /**
<summary>Gets/Sets the border effect.</summary>
*/
        [PDF(VersionEnum.PDF16)]
        public BorderEffect BorderEffect
        {
            get => new BorderEffect(this.BaseDataObject.Get<PdfDictionary>(PdfName.BE));
            set => this.BaseDataObject[PdfName.BE] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the justification to be used in displaying the annotation's text.</summary>
        */
        public JustificationEnum Justification
        {
            get => JustificationEnumExtension.Get((PdfInteger)this.BaseDataObject[PdfName.Q]);
            set => this.BaseDataObject[PdfName.Q] = (value != DefaultJustification) ? value.GetCode() : null;
        }

        /**
          <summary>Gets/Sets the callout line attached to the free text annotation.</summary>
        */
        public CalloutLine Line
        {
            get
            {
                var calloutCalloutLine = (PdfArray)this.BaseDataObject[PdfName.CL];
                return (calloutCalloutLine != null) ? new CalloutLine(calloutCalloutLine) : null;
            }
            set
            {
                this.BaseDataObject[PdfName.CL] = PdfObjectWrapper.GetBaseObject(value);
                if (value != null)
                {
                    /*
                      NOTE: To ensure the callout would be properly rendered, we have to declare the
                      corresponding intent.
                    */
                    this.Type = TypeEnum.Callout;
                }
            }
        }

        /**
          <summary>Gets/Sets the style of the ending line ending.</summary>
        */
        public LineEndStyleEnum LineEndStyle
        {
            get
            {
                var endstylesObject = (PdfArray)this.BaseDataObject[PdfName.LE];
                return (endstylesObject != null) ? LineEndStyleEnumExtension.Get((PdfName)endstylesObject[1]) : DefaultLineEndStyle;
            }
            set => this.EnsureLineEndStylesObject()[1] = value.GetName();
        }

        /**
          <summary>Gets/Sets the style of the starting line ending.</summary>
        */
        public LineEndStyleEnum LineStartStyle
        {
            get
            {
                var endstylesObject = (PdfArray)this.BaseDataObject[PdfName.LE];
                return (endstylesObject != null) ? LineEndStyleEnumExtension.Get((PdfName)endstylesObject[0]) : DefaultLineEndStyle;
            }
            set => this.EnsureLineEndStylesObject()[0] = value.GetName();
        }

        /**
          <summary>Popups not supported.</summary>
        */
        public override Popup Popup
        {
            set => throw new NotSupportedException();
        }
        /**
  <summary>Callout line [PDF:1.6:8.4.5].</summary>
*/
        public class CalloutLine
          : PdfObjectWrapper<PdfArray>
        {
            private readonly Page page;

            internal CalloutLine(
              PdfDirectObject baseObject
              ) : base(baseObject)
            { }

            public CalloutLine(
              Page page,
              PointF start,
              PointF end
              ) : this(page, start, null, end)
            { }

            public CalloutLine(
              Page page,
              PointF start,
              PointF? knee,
              PointF end
              ) : base(new PdfArray())
            {
                this.page = page;
                var baseDataObject = this.BaseDataObject;
                double pageHeight = page.Box.Height;
                baseDataObject.Add(PdfReal.Get(start.X));
                baseDataObject.Add(PdfReal.Get(pageHeight - start.Y));
                if (knee.HasValue)
                {
                    baseDataObject.Add(PdfReal.Get(knee.Value.X));
                    baseDataObject.Add(PdfReal.Get(pageHeight - knee.Value.Y));
                }
                baseDataObject.Add(PdfReal.Get(end.X));
                baseDataObject.Add(PdfReal.Get(pageHeight - end.Y));
            }

            public PointF End
            {
                get
                {
                    var coordinates = this.BaseDataObject;
                    if (coordinates.Count < 6)
                    {
                        return new PointF(
                          (float)((IPdfNumber)coordinates[2]).RawValue,
                          (float)(this.page.Box.Height - ((IPdfNumber)coordinates[3]).RawValue)
                          );
                    }
                    else
                    {
                        return new PointF(
                          (float)((IPdfNumber)coordinates[4]).RawValue,
                          (float)(this.page.Box.Height - ((IPdfNumber)coordinates[5]).RawValue)
                          );
                    }
                }
            }

            public PointF? Knee
            {
                get
                {
                    var coordinates = this.BaseDataObject;
                    if (coordinates.Count < 6)
                    {
                        return null;
                    }

                    return new PointF(
                      (float)((IPdfNumber)coordinates[2]).RawValue,
                      (float)(this.page.Box.Height - ((IPdfNumber)coordinates[3]).RawValue)
                      );
                }
            }

            public PointF Start
            {
                get
                {
                    var coordinates = this.BaseDataObject;

                    return new PointF(
                      (float)((IPdfNumber)coordinates[0]).RawValue,
                      (float)(this.page.Box.Height - ((IPdfNumber)coordinates[1]).RawValue)
                      );
                }
            }
        }

        /**
          <summary>Note type [PDF:1.6:8.4.5].</summary>
        */
        public enum TypeEnum
        {
            /**
              Callout.
            */
            Callout,
            /**
              Typewriter.
            */
            TypeWriter
        }
    }

    internal static class StaticNoteTypeEnumExtension
    {
        private static readonly BiDictionary<StaticNote.TypeEnum, PdfName> codes;

        static StaticNoteTypeEnumExtension()
        {
            codes = new BiDictionary<StaticNote.TypeEnum, PdfName>();
            codes[StaticNote.TypeEnum.Callout] = PdfName.FreeTextCallout;
            codes[StaticNote.TypeEnum.TypeWriter] = PdfName.FreeTextTypeWriter;
        }

        public static StaticNote.TypeEnum? Get(
          PdfName name
          )
        {
            if (name == null)
            {
                return null;
            }

            StaticNote.TypeEnum? type = codes.GetKey(name);
            if (!type.HasValue)
            {
                throw new NotSupportedException($"Type unknown: {name}");
            }

            return type.Value;
        }

        public static PdfName GetName(
          this StaticNote.TypeEnum type
          )
        { return codes[type]; }
    }
}