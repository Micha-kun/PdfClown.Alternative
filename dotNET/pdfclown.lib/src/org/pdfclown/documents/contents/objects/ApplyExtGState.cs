/*
  Copyright 2009-2012 Stefano Chizzolini. http://www.pdfclown.org

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

    /**
      <summary>'Set the specified graphics state parameters' operation [PDF:1.6:4.3.3].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class ApplyExtGState
      : Operation,
        IResourceReference<ExtGState>
    {
        public static readonly string OperatorKeyword = "gs";

        public ApplyExtGState(
PdfName name
) : base(OperatorKeyword, name)
        { }

        public ApplyExtGState(
          IList<PdfDirectObject> operands
          ) : base(OperatorKeyword, operands)
        { }

        /**
<summary>Gets the <see cref="ExtGState">graphics state parameters</see> resource to be set.
</summary>
<param name="context">Content context.</param>
*/
        public ExtGState GetExtGState(
          IContentContext context
          )
        { return this.GetResource(context); }

        public ExtGState GetResource(
  IContentContext context
  )
        { return context.Resources.ExtGStates[this.Name]; }

        public override void Scan(
          ContentScanner.GraphicsState state
          )
        {
            var extGState = this.GetExtGState(state.Scanner.ContentContext);
            extGState.ApplyTo(state);
        }

        public PdfName Name
        {
            get => (PdfName)this.operands[0];
            set => this.operands[0] = value;
        }
    }
}
