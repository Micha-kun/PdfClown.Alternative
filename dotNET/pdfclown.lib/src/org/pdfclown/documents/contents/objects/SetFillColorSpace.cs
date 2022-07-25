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
    using org.pdfclown.documents.contents.colorSpaces;

    using org.pdfclown.objects;

    /**
      <summary>'Set the current color space to use for nonstroking operations' operation
      [PDF:1.6:4.5.7].</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public sealed class SetFillColorSpace
      : Operation,
        IResourceReference<ColorSpace>
    {
        public static readonly string OperatorKeyword = "cs";

        public SetFillColorSpace(
PdfName name
) : base(OperatorKeyword, name)
        { }

        public SetFillColorSpace(
          IList<PdfDirectObject> operands
          ) : base(OperatorKeyword, operands)
        { }

        /**
<summary>Gets the <see cref="ColorSpace">color space</see> resource to be set.</summary>
<param name="context">Content context.</param>
*/
        public ColorSpace GetColorSpace(
          IContentContext context
          )
        { return this.GetResource(context); }

        public ColorSpace GetResource(
  IContentContext context
  )
        {
            /*
              NOTE: The names DeviceGray, DeviceRGB, DeviceCMYK, and Pattern always identify
              the corresponding color spaces directly; they never refer to resources in the
              ColorSpace subdictionary [PDF:1.6:4.5.7].
            */
            var name = this.Name;
            if (name.Equals(PdfName.DeviceGray))
            {
                return DeviceGrayColorSpace.Default;
            }
            else if (name.Equals(PdfName.DeviceRGB))
            {
                return DeviceRGBColorSpace.Default;
            }
            else if (name.Equals(PdfName.DeviceCMYK))
            {
                return DeviceCMYKColorSpace.Default;
            }
            else if (name.Equals(PdfName.Pattern))
            {
                return PatternColorSpace.Default;
            }
            else
            {
                return context.Resources.ColorSpaces[name];
            }
        }

        public override void Scan(
          ContentScanner.GraphicsState state
          )
        {
            // 1. Color space.
            state.FillColorSpace = this.GetColorSpace(state.Scanner.ContentContext);

            // 2. Initial color.
            /*
              NOTE: The operation also sets the current nonstroking color
              to its initial value, which depends on the color space [PDF:1.6:4.5.7].
            */
            state.FillColor = state.FillColorSpace.DefaultColor;
        }

        public PdfName Name
        {
            get => (PdfName)this.operands[0];
            set => this.operands[0] = value;
        }
    }
}