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

    using org.pdfclown.objects;

    /**
      <summary>Timespan [PDF:1.7:9.1.5].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    internal sealed class Timespan
      : PdfObjectWrapper<PdfDictionary>
    {

        internal Timespan(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        public Timespan(
double time
) : base(
new PdfDictionary(
new PdfName[]
{
            PdfName.Type,
            PdfName.S
},
new PdfDirectObject[]
{
            PdfName.Timespan,
            PdfName.S
}
)
)
        { this.Time = time; }

        /**
<summary>Gets/Sets the temporal offset (in seconds).</summary>
*/
        public double Time
        {
            get => ((IPdfNumber)this.BaseDataObject[PdfName.V]).DoubleValue;
            set => this.BaseDataObject[PdfName.V] = PdfReal.Get(value);
        }
    }
}