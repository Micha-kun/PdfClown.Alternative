/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

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
    using System;
    using System.Reflection;
    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.documents.contents.fonts;

    using org.pdfclown.documents.contents.xObjects;
    using org.pdfclown.objects;

    /**
      <summary>Resources collection [PDF:1.6:3.7.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class Resources
      : PdfObjectWrapper<PdfDictionary>,
        ICompositeDictionary<PdfName>
    {

        private Resources(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public Resources(
Document context
) : base(context, new PdfDictionary())
        { }

        public PdfObjectWrapper Get(
  Type type
  )
        {
            if (typeof(ColorSpace).IsAssignableFrom(type))
            {
                return this.ColorSpaces;
            }
            else if (typeof(ExtGState).IsAssignableFrom(type))
            {
                return this.ExtGStates;
            }
            else if (typeof(Font).IsAssignableFrom(type))
            {
                return this.Fonts;
            }
            else if (typeof(Pattern).IsAssignableFrom(type))
            {
                return this.Patterns;
            }
            else if (typeof(PropertyList).IsAssignableFrom(type))
            {
                return this.PropertyLists;
            }
            else if (typeof(Shading).IsAssignableFrom(type))
            {
                return this.Shadings;
            }
            else if (typeof(XObject).IsAssignableFrom(type))
            {
                return this.XObjects;
            }
            else
            {
                throw new ArgumentException($"{type.Name} does NOT represent a valid resource class.");
            }
        }

        public T Get<T>(
          PdfName key
          ) where T : PdfObjectWrapper
        {
            var resources = this.Get(typeof(T));
            return (T)resources.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance).GetValue(resources, new object[] { key });
        }
        public static Resources Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new Resources(baseObject) : null; }

        public ColorSpaceResources ColorSpaces
        {
            get => new ColorSpaceResources(this.BaseDataObject.Get<PdfDictionary>(PdfName.ColorSpace));
            set => this.BaseDataObject[PdfName.ColorSpace] = value.BaseObject;
        }

        public ExtGStateResources ExtGStates
        {
            get => new ExtGStateResources(this.BaseDataObject.Get<PdfDictionary>(PdfName.ExtGState));
            set => this.BaseDataObject[PdfName.ExtGState] = value.BaseObject;
        }

        public FontResources Fonts
        {
            get => new FontResources(this.BaseDataObject.Get<PdfDictionary>(PdfName.Font));
            set => this.BaseDataObject[PdfName.Font] = value.BaseObject;
        }

        public PatternResources Patterns
        {
            get => new PatternResources(this.BaseDataObject.Get<PdfDictionary>(PdfName.Pattern));
            set => this.BaseDataObject[PdfName.Pattern] = value.BaseObject;
        }

        [PDF(VersionEnum.PDF12)]
        public PropertyListResources PropertyLists
        {
            get => new PropertyListResources(this.BaseDataObject.Get<PdfDictionary>(PdfName.Properties));
            set
            {
                this.CheckCompatibility("PropertyLists");
                this.BaseDataObject[PdfName.Properties] = value.BaseObject;
            }
        }

        [PDF(VersionEnum.PDF13)]
        public ShadingResources Shadings
        {
            get => new ShadingResources(this.BaseDataObject.Get<PdfDictionary>(PdfName.Shading));
            set => this.BaseDataObject[PdfName.Shading] = value.BaseObject;
        }

        public XObjectResources XObjects
        {
            get => new XObjectResources(this.BaseDataObject.Get<PdfDictionary>(PdfName.XObject));
            set => this.BaseDataObject[PdfName.XObject] = value.BaseObject;
        }
    }
}