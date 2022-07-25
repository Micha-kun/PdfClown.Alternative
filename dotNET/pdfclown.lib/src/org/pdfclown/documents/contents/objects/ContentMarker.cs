/*
  Copyright 2010-2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.contents.objects
{
    using System;

    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Abstract content marker [PDF:1.6:10.5].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public abstract class ContentMarker
      : Operation,
        IResourceReference<PropertyList>
    {
        protected ContentMarker(
PdfName tag
) : this(tag, null)
        { }

        protected ContentMarker(
          PdfName tag,
          PdfDirectObject properties
          ) : base(null, tag)
        {
            if (properties != null)
            {
                this.operands.Add(properties);
                this.@operator = this.PropertyListOperator;
            }
            else
            { this.@operator = this.SimpleOperator; }
        }

        protected ContentMarker(
          string @operator,
          IList<PdfDirectObject> operands
          ) : base(@operator, operands)
        { }

        protected abstract string PropertyListOperator
        { get; }

        protected abstract string SimpleOperator
        { get; }

        /**
<summary>Gets the private information meaningful to the program (application or plugin extension)
creating the marked content.</summary>
<param name="context">Content context.</param>
*/
        public PropertyList GetProperties(
          IContentContext context
          )
        {
            var properties = this.Properties;
            return (properties is PdfName)
              ? context.Resources.PropertyLists[(PdfName)properties]
              : ((PropertyList)properties);
        }

        public PropertyList GetResource(
  IContentContext context
  )
        { return this.GetProperties(context); }

        public PdfName Name
        {
            get
            {
                var properties = this.Properties;
                return (properties is PdfName) ? ((PdfName)properties) : null;
            }
            set => this.Properties = value;
        }

        /**
          <summary>Gets/Sets the private information meaningful to the program (application or plugin
          extension) creating the marked content. It can be either an inline <see cref="PropertyList"/>
          or the <see cref="PdfName">name</see> of an external PropertyList resource.</summary>
        */
        public object Properties
        {
            get
            {
                var propertiesObject = this.operands[1];
                if (propertiesObject == null)
                {
                    return null;
                }
                else if (propertiesObject is PdfName)
                {
                    return propertiesObject;
                }
                else if (propertiesObject is PdfDictionary)
                {
                    return PropertyList.Wrap(propertiesObject);
                }
                else
                {
                    throw new NotSupportedException($"Property list type unknown: {propertiesObject.GetType().Name}");
                }
            }
            set
            {
                if (value == null)
                {
                    this.@operator = this.SimpleOperator;
                    if (this.operands.Count > 1)
                    { this.operands.RemoveAt(1); }
                }
                else
                {
                    PdfDirectObject operand;
                    if (value is PdfName)
                    { operand = (PdfName)value; }
                    else if (value is PropertyList)
                    { operand = ((PropertyList)value).BaseDataObject; }
                    else
                    {
                        throw new ArgumentException("value MUST be a PdfName or a PropertyList.");
                    }

                    this.@operator = this.PropertyListOperator;
                    if (this.operands.Count > 1)
                    { this.operands[1] = operand; }
                    else
                    { this.operands.Add(operand); }
                }
            }
        }

        /**
          <summary>Gets/Sets the marker indicating the role or significance of the marked content.</summary>
        */
        public PdfName Tag
        {
            get => (PdfName)this.operands[0];
            set => this.operands[0] = value;
        }
    }
}