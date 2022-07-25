/*
  Copyright 2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library"
  (the Program): see the accompanying README files for more info.

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


namespace org.pdfclown.documents.contents.layers
{

    using org.pdfclown.objects;

    /**
      <summary>UI collection of related layers [PDF:1.7:4.10.3].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public sealed class LayerCollection
      : UILayers,
        IUILayerNode
    {

        private LayerCollection(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public LayerCollection(
Document context,
string title
) : base(context)
        { this.Title = title; }

        UILayers IUILayerNode.Children => this;

        public override string ToString(
)
        { return this.Title; }
        public static new LayerCollection Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new LayerCollection(baseObject) : null; }

        public string Title
        {
            get
            {
                if (this.BaseDataObject.Count == 0)
                {
                    return null;
                }

                var firstObject = this.BaseDataObject[0];
                return (firstObject is PdfString) ? ((PdfString)firstObject).StringValue : null;
            }
            set
            {
                var titleObject = PdfTextString.Get(value);
                var baseDataObject = this.BaseDataObject;
                var firstObject = (baseDataObject.Count == 0) ? null : baseDataObject[0];
                if (firstObject is PdfString)
                {
                    if (titleObject != null)
                    { baseDataObject[0] = titleObject; }
                    else
                    { baseDataObject.RemoveAt(0); }
                }
                else if (titleObject != null)
                { baseDataObject.Insert(0, titleObject); }
            }
        }
    }
}

