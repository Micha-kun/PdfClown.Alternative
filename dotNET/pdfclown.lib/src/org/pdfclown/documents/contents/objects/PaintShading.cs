/*
  Copyright 2007-2011 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.Collections.Generic;
    using org.pdfclown.objects;

    using colorSpaces = org.pdfclown.documents.contents.colorSpaces;

    /**
      <summary>'Paint the shape and color shading' operation [PDF:1.6:4.6.3].</summary>
    */
    [PDF(VersionEnum.PDF13)]
    public sealed class PaintShading
      : Operation,
        IResourceReference<colorSpaces::Shading>
    {
        public static readonly string OperatorKeyword = "sh";

        public PaintShading(
PdfName name
) : base(OperatorKeyword, name)
        { }

        public PaintShading(
          IList<PdfDirectObject> operands
          ) : base(OperatorKeyword, operands)
        { }

        public colorSpaces::Shading GetResource(
  IContentContext context
  )
        { return context.Resources.Shadings[this.Name]; }

        /**
<summary>Gets the <see cref="colorSpaces::Shading">shading</see> resource to be painted.
</summary>
<param name="context">Content context.</param>
*/
        public colorSpaces::Shading GetShading(
          IContentContext context
          )
        { return this.GetResource(context); }

        public PdfName Name
        {
            get => (PdfName)this.operands[0];
            set => this.operands[0] = value;
        }
    }
}