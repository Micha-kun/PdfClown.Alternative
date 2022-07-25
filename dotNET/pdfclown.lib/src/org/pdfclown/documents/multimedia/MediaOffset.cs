/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.multimedia
{
    using System;

    using org.pdfclown.objects;

    /**
      <summary>Media offset [PDF:1.7:9.1.5].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public abstract class MediaOffset
      : PdfObjectWrapper<PdfDictionary>
    {

        protected MediaOffset(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        protected MediaOffset(
Document context,
PdfName subtype
) : base(
context,
new PdfDictionary(
new PdfName[]
{
            PdfName.Type,
            PdfName.S
},
new PdfDirectObject[]
{
            PdfName.MediaOffset,
            subtype
}
)
)
        { }

        public static MediaOffset Wrap(
PdfDirectObject baseObject
)
        {
            if (baseObject == null)
            {
                return null;
            }

            var dataObject = (PdfDictionary)baseObject.Resolve();
            var offsetType = (PdfName)dataObject[PdfName.S];
            if ((offsetType == null)
              || (dataObject.ContainsKey(PdfName.Type)
                  && !dataObject[PdfName.Type].Equals(PdfName.MediaOffset)))
            {
                return null;
            }

            if (offsetType.Equals(PdfName.F))
            {
                return new Frame(baseObject);
            }
            else if (offsetType.Equals(PdfName.M))
            {
                return new Marker(baseObject);
            }
            else if (offsetType.Equals(PdfName.T))
            {
                return new Time(baseObject);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /**
<summary>Gets/Sets the offset value.</summary>
*/
        public abstract object Value
        {
            get;
            set;
        }
        /**
  <summary>Media offset frame [PDF:1.7:9.1.5].</summary>
*/
        public sealed class Frame
          : MediaOffset
        {

            internal Frame(
              PdfDirectObject baseObject
              ) : base(baseObject)
            { }
            public Frame(
              Document context,
              int value
              ) : base(context, PdfName.F)
            { this.Value = value; }

            /**
              <summary>Gets/Sets the (zero-based) frame within a media object.</summary>
            */
            public override object Value
            {
                get => ((PdfInteger)this.BaseDataObject[PdfName.F]).IntValue;
                set
                {
                    var intValue = (int)value;
                    if (intValue < 0)
                    {
                        throw new ArgumentException("MUST be non-negative.");
                    }

                    this.BaseDataObject[PdfName.F] = PdfInteger.Get(intValue);
                }
            }
        }

        /**
          <summary>Media offset marker [PDF:1.7:9.1.5].</summary>
        */
        public sealed class Marker
          : MediaOffset
        {

            internal Marker(
              PdfDirectObject baseObject
              ) : base(baseObject)
            { }
            public Marker(
              Document context,
              string value
              ) : base(context, PdfName.M)
            { this.Value = value; }

            /**
              <summary>Gets a named offset within a media object.</summary>
            */
            public override object Value
            {
                get => ((PdfTextString)this.BaseDataObject[PdfName.M]).StringValue;
                set => this.BaseDataObject[PdfName.M] = PdfTextString.Get(value);
            }
        }

        /**
          <summary>Media offset time [PDF:1.7:9.1.5].</summary>
        */
        public sealed class Time
          : MediaOffset
        {

            internal Time(
              PdfDirectObject baseObject
              ) : base(baseObject)
            { }
            public Time(
              Document context,
              double value
              ) : base(context, PdfName.T)
            { this.BaseDataObject[PdfName.T] = new Timespan(value).BaseObject; }

            private Timespan Timespan => new Timespan(this.BaseDataObject[PdfName.T]);

            /**
              <summary>Gets/Sets the temporal offset (in seconds).</summary>
            */
            public override object Value
            {
                get => this.Timespan.Time;
                set => this.Timespan.Time = (double)value;
            }
        }
    }
}