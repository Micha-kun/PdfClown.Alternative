/*
  Copyright 2007-2015 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.Drawing.Drawing2D;
    using org.pdfclown.bytes;

    /**
      <summary>Composite object. It is made up of multiple content objects.</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public abstract class CompositeObject
      : ContentObject
    {
        protected IList<ContentObject> objects;

        protected CompositeObject(
  )
        { this.objects = new List<ContentObject>(); }

        protected CompositeObject(
          ContentObject obj
          ) : this()
        { this.objects.Add(obj); }

        protected CompositeObject(
          params ContentObject[] objects
          ) : this()
        {
            foreach (var obj in objects)
            { this.objects.Add(obj); }
        }

        protected CompositeObject(
          IList<ContentObject> objects
          )
        { this.objects = objects; }

        /**
  <summary>Creates the rendering object corresponding to this container.</summary>
*/
        protected virtual GraphicsPath CreateRenderObject(
          )
        { return null; }

        /**
          <summary>Renders this container.</summary>
          <param name="state">Graphics state.</param>
          <returns>Whether the rendering has been executed.</returns>
         */
        protected bool Render(
          ContentScanner.GraphicsState state
          )
        {
            var scanner = state.Scanner;
            var context = scanner.RenderContext;
            if (context == null)
            {
                return false;
            }

            // Render the inner elements!
            scanner.ChildLevel.Render(
              context,
              scanner.CanvasSize,
              this.CreateRenderObject()
              );
            return true;
        }

        public override void Scan(
          ContentScanner.GraphicsState state
          )
        {
            var childLevel = state.Scanner.ChildLevel;

            if (!this.Render(state))
            { childLevel.MoveEnd(); } // Forces the current object to its final graphics state.

            childLevel.State.CopyTo(state); // Copies the current object's final graphics state to the current level's.
        }

        public override string ToString(
          )
        { return $"{{{this.GetType().Name} {this.objects}}}"; }

        public override void WriteTo(
          IOutputStream stream,
          Document context
          )
        {
            foreach (var obj in this.objects)
            { obj.WriteTo(stream, context); }
        }

        /**
<summary>Gets/Sets the object header.</summary>
*/
        public virtual Operation Header
        {
            get => null;
            set => throw new NotSupportedException();
        }

        /**
          <summary>Gets the list of inner objects.</summary>
        */
        public IList<ContentObject> Objects => this.objects;
    }
}