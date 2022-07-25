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

namespace org.pdfclown.documents.contents
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Text;
    using org.pdfclown.documents.contents.objects;
    using org.pdfclown.objects;
    using colors = org.pdfclown.documents.contents.colorSpaces;
    using fonts = org.pdfclown.documents.contents.fonts;
    using xObjects = org.pdfclown.documents.contents.xObjects;

    /**
      <summary>Content objects scanner.</summary>
      <remarks>
        <para>It wraps the <see cref="Contents">content objects collection</see> to scan its graphics state
        through a forward cursor.</para>
        <para>Scanning is performed at an arbitrary deepness, according to the content objects nesting:
        each depth level corresponds to a scan level so that at any time it's possible to seamlessly
        navigate across the levels (see <see cref="ParentLevel"/>, <see cref="ChildLevel"/>).</para>
      </remarks>
    */
    public sealed class ContentScanner
    {

        private static readonly int StartIndex = -1;

        /**
          <summary>Size of the graphics canvas.</summary>
          <remarks>According to the current processing (whether it is device-independent scanning or
          device-based rendering), it may be expressed, respectively, in user-space units or in
          device-space units.</remarks>
        */
        private SizeF canvasSize;

        /**
Child level.
*/
        private ContentScanner childLevel;
        /**
          Content objects collection.
        */
        private readonly Contents contents;
        /**
          <summary>Device-independent size of the graphics canvas.</summary>
        */
        private SizeF contextSize;
        /**
          Current object index at this level.
        */
        private int index = 0;
        /**
          Object collection at this level.
        */
        private readonly IList<ContentObject> objects;
        /**
          Parent level.
        */
        private readonly ContentScanner parentLevel;

        /**
          Rendering context.
        */
        private Graphics renderContext;
        /**
          Rendering object.
        */
        private GraphicsPath renderObject;
        /**
          Current graphics state.
        */
        private GraphicsState state;

        /**
          <summary>Instantiates a child-level content scanner.</summary>
          <param name="parentLevel">Parent scan level.</param>
        */
        private ContentScanner(
          ContentScanner parentLevel
          )
        {
            this.parentLevel = parentLevel;
            this.contents = parentLevel.contents;
            this.objects = ((CompositeObject)parentLevel.Current).Objects;

            this.canvasSize = this.contextSize = parentLevel.contextSize;

            this.MoveStart();
        }

        /**
  <summary>Instantiates a top-level content scanner.</summary>
  <param name="contents">Content objects collection to scan.</param>
*/
        public ContentScanner(
          Contents contents
          )
        {
            this.parentLevel = null;
            this.objects = this.contents = contents;

            this.canvasSize = this.contextSize = contents.ContentContext.Box.Size;

            this.MoveStart();
        }

        /**
          <summary>Instantiates a top-level content scanner.</summary>
          <param name="contentContext">Content context containing the content objects collection to scan.</param>
        */
        public ContentScanner(
          IContentContext contentContext
          ) : this(contentContext.Contents)
        { }

        /**
          <summary>Instantiates a child-level content scanner for <see cref="org.pdfclown.documents.contents.xObjects.FormXObject">external form</see>.</summary>
          <param name="formXObject">External form.</param>
          <param name="parentLevel">Parent scan level.</param>
        */
        public ContentScanner(
          xObjects::FormXObject formXObject,
          ContentScanner parentLevel
          )
        {
            this.parentLevel = parentLevel;
            this.objects = this.contents = formXObject.Contents;

            this.canvasSize = this.contextSize = parentLevel.contextSize;

            this.OnStart += delegate (
              ContentScanner scanner
              )
            {
                // Adjust the initial graphics state to the external form context!
                scanner.State.Ctm.Multiply(formXObject.Matrix);
                /*
                  TODO: On rendering, clip according to the form dictionary's BBox entry!
                */
            };
            this.MoveStart();
        }
        /**
  <summary>Handles the scan start notification.</summary>
  <param name="scanner">Content scanner started.</param>
*/
        public delegate void OnStartEventHandler(
          ContentScanner scanner
          );

        /**
  <summary>Notifies the scan start.</summary>
*/
        public event OnStartEventHandler OnStart;

        /**
  <summary>Synchronizes the scanner state.</summary>
*/
        private void Refresh(
          )
        {
            if (this.Current is CompositeObject)
            { this.childLevel = new ContentScanner(this); }
            else
            { this.childLevel = null; }
        }

#pragma warning disable 0628
        /**
          <summary>Notifies the scan start to listeners.</summary>
        */
        protected void NotifyStart(
          )
        {
            if (this.OnStart != null)
            { this.OnStart(this); }
        }
#pragma warning restore 0628

        /**
          <summary>Inserts a content object at the current position.</summary>
        */
        public void Insert(
          ContentObject obj
          )
        {
            if (this.index == -1)
            { this.index = 0; }

            this.objects.Insert(this.index, obj);
            this.Refresh();
        }

        /**
          <summary>Inserts content objects at the current position.</summary>
          <remarks>After the insertion is complete, the lastly-inserted content object is at the current position.</remarks>
        */
        public void Insert<T>(
          ICollection<T> objects
          ) where T : ContentObject
        {
            var index = 0;
            var count = objects.Count;
            foreach (ContentObject obj in objects)
            {
                this.Insert(obj);

                if (++index < count)
                { _ = this.MoveNext(); }
            }
        }

        /**
          <summary>Gets whether this level is the root of the hierarchy.</summary>
        */
        public bool IsRootLevel(
          )
        { return this.parentLevel == null; }

        /**
          <summary>Moves to the object at the given position.</summary>
          <param name="index">New position.</param>
          <returns>Whether the object was successfully reached.</returns>
        */
        public bool Move(
          int index
          )
        {
            if (this.index > index)
            { this.MoveStart(); }

            while ((this.index < index)
              && this.MoveNext())
            {
                ;
            }

            return this.Current != null;
        }

        /**
          <summary>Moves after the last object.</summary>
        */
        public void MoveEnd(
          )
        { _ = this.MoveLast(); _ = this.MoveNext(); }

        /**
          <summary>Moves to the first object.</summary>
          <returns>Whether the first object was successfully reached.</returns>
        */
        public bool MoveFirst(
          )
        { this.MoveStart(); return this.MoveNext(); }

        /**
          <summary>Moves to the last object.</summary>
          <returns>Whether the last object was successfully reached.</returns>
        */
        public bool MoveLast(
          )
        {
            var lastIndex = this.objects.Count - 1;
            while (this.index < lastIndex)
            { _ = this.MoveNext(); }

            return this.Current != null;
        }

        /**
          <summary>Moves to the next object.</summary>
          <returns>Whether the next object was successfully reached.</returns>
        */
        public bool MoveNext(
          )
        {
            // Scanning the current graphics state...
            var currentObject = this.Current;
            if (currentObject != null)
            { currentObject.Scan(this.state); }

            // Moving to the next object...
            if (this.index < this.objects.Count)
            { this.index++; this.Refresh(); }

            return this.Current != null;
        }

        /**
          <summary>Moves before the first object.</summary>
        */
        public void MoveStart(
          )
        {
            this.index = StartIndex;
            if (this.state == null)
            {
                if (this.parentLevel == null)
                { this.state = new GraphicsState(this); }
                else
                { this.state = this.parentLevel.state.Clone(this); }
            }
            else
            {
                if (this.parentLevel == null)
                { this.state.Initialize(); }
                else
                { this.parentLevel.state.CopyTo(this.state); }
            }

            this.NotifyStart();

            this.Refresh();
        }

        /**
          <summary>Removes the content object at the current position.</summary>
          <returns>Removed object.</returns>
        */
        public ContentObject Remove(
          )
        {
            var removedObject = this.Current;
            this.objects.RemoveAt(this.index);
            this.Refresh();

            return removedObject;
        }

        /**
          <summary>Renders the contents into the specified context.</summary>
          <param name="renderContext">Rendering context.</param>
          <param name="renderSize">Rendering canvas size.</param>
        */
        public void Render(
          Graphics renderContext,
          SizeF renderSize
          )
        { this.Render(renderContext, renderSize, null); }

        /**
          <summary>Renders the contents into the specified object.</summary>
          <param name="renderContext">Rendering context.</param>
          <param name="renderSize">Rendering canvas size.</param>
          <param name="renderObject">Rendering object.</param>
        */
        public void Render(
          Graphics renderContext,
          SizeF renderSize,
          GraphicsPath renderObject
          )
        {
            if (this.IsRootLevel())
            {
                // Initialize the context!
                renderContext.TextRenderingHint = TextRenderingHint.AntiAlias;
                renderContext.SmoothingMode = SmoothingMode.HighQuality;

                // Paint the canvas background!
                renderContext.Clear(Color.White);
            }

            try
            {
                this.renderContext = renderContext;
                this.canvasSize = renderSize;
                this.renderObject = renderObject;

                // Scan this level for rendering!
                this.MoveStart();
                while (this.MoveNext())
                {
                    ;
                }
            }
            finally
            {
                this.renderContext = null;
                this.canvasSize = this.contextSize;
                this.renderObject = null;
            }
        }

        /**
<summary>Gets the size of the current imageable area.</summary>
<remarks>It can be either the user-space area (dry scanning) or the device-space area (wet
scanning).</remarks>
*/
        public SizeF CanvasSize => this.canvasSize;

        /**
          <summary>Gets the current child scan level.</summary>
        */
        public ContentScanner ChildLevel => this.childLevel;

        /**
          <summary>Gets the content context associated to the content objects collection.</summary>
        */
        public IContentContext ContentContext => this.contents.ContentContext;

        /**
          <summary>Gets the content objects collection this scanner is inspecting.</summary>
        */
        public Contents Contents => this.contents;

        /**
          <summary>Gets the size of the current imageable area in user-space units.</summary>
        */
        public SizeF ContextSize => this.contextSize;

        /**
          <summary>Gets/Sets the current content object.</summary>
        */
        public ContentObject Current
        {
            get
            {
                if ((this.index < 0) || (this.index >= this.objects.Count))
                {
                    return null;
                }

                return this.objects[this.index];
            }
            set
            {
                this.objects[this.index] = value;
                this.Refresh();
            }
        }

        /**
          <summary>Gets the current content object's information.</summary>
        */
        public GraphicsObjectWrapper CurrentWrapper => GraphicsObjectWrapper.Get(this);

        /**
          <summary>Gets the current position.</summary>
        */
        public int Index => this.index;

        /**
          <summary>Gets the current parent object.</summary>
        */
        public CompositeObject Parent => (this.parentLevel == null) ? null : ((CompositeObject)this.parentLevel.Current);

        /**
          <summary>Gets the parent scan level.</summary>
        */
        public ContentScanner ParentLevel => this.parentLevel;

        /**
          <summary>Gets the rendering context.</summary>
          <returns><code>null</code> in case of dry scanning.</returns>
        */
        public Graphics RenderContext => this.renderContext;

        /**
          <summary>Gets the rendering object.</summary>
          <returns><code>null</code> in case of scanning outside a shape.</returns>
        */
        public GraphicsPath RenderObject => this.renderObject;

        /**
          <summary>Gets the root scan level.</summary>
        */
        public ContentScanner RootLevel
        {
            get
            {
                var level = this;
                while (true)
                {
                    var parentLevel = level.ParentLevel;
                    if (parentLevel == null)
                    {
                        return level;
                    }

                    level = parentLevel;
                }
            }
        }

        /**
          <summary>Gets the current graphics state applied to the current content object.</summary>
        */
        public GraphicsState State => this.state;

        /**
  <summary>Graphics state [PDF:1.6:4.3].</summary>
*/
        public sealed class GraphicsState
          : ICloneable
        {
            private IList<BlendModeEnum> blendMode;
            private double charSpace;
            private Matrix ctm;
            private colors::Color fillColor;
            private colors::ColorSpace fillColorSpace;
            private fonts::Font font;
            private double fontSize;
            private double lead;
            private LineCapEnum lineCap;
            private LineDash lineDash;
            private LineJoinEnum lineJoin;
            private double lineWidth;
            private double miterLimit;
            private TextRenderModeEnum renderMode;
            private double rise;
            private double scale;

            private ContentScanner scanner;
            private colors::Color strokeColor;
            private colors::ColorSpace strokeColorSpace;
            private Matrix tlm;
            private Matrix tm;
            private double wordSpace;

            internal GraphicsState(
  ContentScanner scanner
  )
            {
                this.scanner = scanner;
                this.Initialize();
            }

            internal GraphicsState Clone(
  ContentScanner scanner
  )
            {
                var state = (GraphicsState)this.Clone();
                state.scanner = scanner;
                return state;
            }

            internal void Initialize(
              )
            {
                // State parameters initialization.
                this.blendMode = ExtGState.DefaultBlendMode;
                this.charSpace = 0;
                this.ctm = this.GetInitialCtm();
                this.fillColor = colors::DeviceGrayColor.Default;
                this.fillColorSpace = colors::DeviceGrayColorSpace.Default;
                this.font = null;
                this.fontSize = 0;
                this.lead = 0;
                this.lineCap = LineCapEnum.Butt;
                this.lineDash = new LineDash();
                this.lineJoin = LineJoinEnum.Miter;
                this.lineWidth = 1;
                this.miterLimit = 10;
                this.renderMode = TextRenderModeEnum.Fill;
                this.rise = 0;
                this.scale = 1;
                this.strokeColor = colors::DeviceGrayColor.Default;
                this.strokeColorSpace = colors::DeviceGrayColorSpace.Default;
                this.tlm = new Matrix();
                this.tm = new Matrix();
                this.wordSpace = 0;

                // Rendering context initialization.
                var renderContext = this.Scanner.RenderContext;
                if (renderContext != null)
                { renderContext.Transform = this.ctm; }
            }

            /**
<summary>Gets a deep copy of the graphics state object.</summary>
*/
            public object Clone(
              )
            {
                GraphicsState clone;
                // Shallow copy.
                clone = (GraphicsState)this.MemberwiseClone();

                // Deep copy.
                /* NOTE: Mutable objects are to be cloned. */
                clone.ctm = this.ctm.Clone();
                clone.tlm = this.tlm.Clone();
                clone.tm = this.tm.Clone();
                return clone;
            }

            /**
              <summary>Copies this graphics state into the specified one.</summary>
              <param name="state">Target graphics state object.</param>
            */
            public void CopyTo(
              GraphicsState state
              )
            {
                state.blendMode = this.blendMode;
                state.charSpace = this.charSpace;
                state.ctm = this.ctm.Clone();
                state.fillColor = this.fillColor;
                state.fillColorSpace = this.fillColorSpace;
                state.font = this.font;
                state.fontSize = this.fontSize;
                state.lead = this.lead;
                state.lineCap = this.lineCap;
                state.lineDash = this.lineDash;
                state.lineJoin = this.lineJoin;
                state.lineWidth = this.lineWidth;
                state.miterLimit = this.miterLimit;
                state.renderMode = this.renderMode;
                state.rise = this.rise;
                state.scale = this.scale;
                state.strokeColor = this.strokeColor;
                state.strokeColorSpace = this.strokeColorSpace;
                //TODO:temporary hack (define TextState for textual parameters!)...
                if (state.scanner.Parent is Text)
                {
                    state.tlm = this.tlm.Clone();
                    state.tm = this.tm.Clone();
                }
                else
                {
                    state.tlm = new Matrix();
                    state.tm = new Matrix();
                }
                state.wordSpace = this.wordSpace;
            }

            /**
              <summary>Gets the initial current transformation matrix.</summary>
            */
            public Matrix GetInitialCtm(
              )
            {
                Matrix initialCtm;
                if (this.Scanner.RenderContext == null) // Device-independent.
                {
                    initialCtm = new Matrix(); // Identity.
                }
                else // Device-dependent.
                {
                    var contentContext = this.Scanner.ContentContext;
                    var canvasSize = this.Scanner.CanvasSize;

                    // Axes orientation.
                    var rotation = contentContext.Rotation;
                    switch (rotation)
                    {
                        case RotationEnum.Downward:
                            initialCtm = new Matrix(1, 0, 0, -1, 0, canvasSize.Height);
                            break;
                        case RotationEnum.Leftward:
                            initialCtm = new Matrix(0, 1, 1, 0, 0, 0);
                            break;
                        case RotationEnum.Upward:
                            initialCtm = new Matrix(-1, 0, 0, 1, canvasSize.Width, 0);
                            break;
                        case RotationEnum.Rightward:
                            initialCtm = new Matrix(0, -1, -1, 0, canvasSize.Width, canvasSize.Height);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    // Scaling.
                    var contentBox = contentContext.Box;
                    var rotatedCanvasSize = rotation.Transform(canvasSize);
                    initialCtm.Scale(
                      rotatedCanvasSize.Width / contentBox.Width,
                      rotatedCanvasSize.Height / contentBox.Height
                      );

                    // Origin alignment.
                    initialCtm.Translate(-contentBox.Left, -contentBox.Top); //TODO: verify minimum coordinates!
                }
                return initialCtm;
            }

            /**
              <summary>Gets the text-to-device space transformation matrix [PDF:1.6:5.3.3].</summary>
              <param name="topDown">Whether the y-axis orientation has to be adjusted to common top-down
              orientation rather than standard PDF coordinate system (bottom-up).</param>
            */
            public Matrix GetTextToDeviceMatrix(
              bool topDown
              )
            {
                /*
                  NOTE: The text rendering matrix (trm) is obtained from the concatenation of the current
                  transformation matrix (ctm) and the text matrix (tm).
                */
                var matrix = this.GetUserToDeviceMatrix(topDown);
                matrix.Multiply(this.tm);
                return matrix;
            }

            /**
              <summary>Gets the user-to-device space transformation matrix [PDF:1.6:4.2.3].</summary>
              <param name="topDown">Whether the y-axis orientation has to be adjusted to common top-down
              orientation rather than standard PDF coordinate system (bottom-up).</param>
            */
            public Matrix GetUserToDeviceMatrix(
              bool topDown
              )
            {
                if (topDown)
                {
                    var matrix = new Matrix(1, 0, 0, -1, 0, this.scanner.CanvasSize.Height);
                    matrix.Multiply(this.ctm);
                    return matrix;
                }
                else
                {
                    return this.ctm.Clone();
                }
            }

            /**
              <summary>Gets/Sets the current blend mode to be used in the transparent imaging model
              [PDF:1.6:5.2.1].</summary>
              <remarks>The application should use the first blend mode in the list that it recognizes.
              </remarks>
            */
            public IList<BlendModeEnum> BlendMode
            {
                get => this.blendMode;
                set => this.blendMode = value;
            }

            /**
              <summary>Gets/Sets the current character spacing [PDF:1.6:5.2.1].</summary>
            */
            public double CharSpace
            {
                get => this.charSpace;
                set => this.charSpace = value;
            }

            /**
              <summary>Gets/Sets the current transformation matrix.</summary>
            */
            public Matrix Ctm
            {
                get => this.ctm;
                set => this.ctm = value;
            }

            /**
              <summary>Gets/Sets the current color for nonstroking operations [PDF:1.6:4.5.1].</summary>
            */
            public colors::Color FillColor
            {
                get => this.fillColor;
                set => this.fillColor = value;
            }

            /**
              <summary>Gets/Sets the current color space for nonstroking operations [PDF:1.6:4.5.1].</summary>
            */
            public colors::ColorSpace FillColorSpace
            {
                get => this.fillColorSpace;
                set => this.fillColorSpace = value;
            }

            /**
              <summary>Gets/Sets the current font [PDF:1.6:5.2].</summary>
            */
            public fonts::Font Font
            {
                get => this.font;
                set => this.font = value;
            }

            /**
              <summary>Gets/Sets the current font size [PDF:1.6:5.2].</summary>
            */
            public double FontSize
            {
                get => this.fontSize;
                set => this.fontSize = value;
            }

            /**
              <summary>Gets/Sets the current leading [PDF:1.6:5.2.4].</summary>
            */
            public double Lead
            {
                get => this.lead;
                set => this.lead = value;
            }

            /**
              <summary>Gets/Sets the current line cap style [PDF:1.6:4.3.2].</summary>
            */
            public LineCapEnum LineCap
            {
                get => this.lineCap;
                set => this.lineCap = value;
            }

            /**
              <summary>Gets/Sets the current line dash pattern [PDF:1.6:4.3.2].</summary>
            */
            public LineDash LineDash
            {
                get => this.lineDash;
                set => this.lineDash = value;
            }

            /**
              <summary>Gets/Sets the current line join style [PDF:1.6:4.3.2].</summary>
            */
            public LineJoinEnum LineJoin
            {
                get => this.lineJoin;
                set => this.lineJoin = value;
            }

            /**
              <summary>Gets/Sets the current line width [PDF:1.6:4.3.2].</summary>
            */
            public double LineWidth
            {
                get => this.lineWidth;
                set => this.lineWidth = value;
            }

            /**
              <summary>Gets/Sets the current miter limit [PDF:1.6:4.3.2].</summary>
            */
            public double MiterLimit
            {
                get => this.miterLimit;
                set => this.miterLimit = value;
            }

            /**
              <summary>Gets/Sets the current text rendering mode [PDF:1.6:5.2.5].</summary>
            */
            public TextRenderModeEnum RenderMode
            {
                get => this.renderMode;
                set => this.renderMode = value;
            }

            /**
              <summary>Gets/Sets the current text rise [PDF:1.6:5.2.6].</summary>
            */
            public double Rise
            {
                get => this.rise;
                set => this.rise = value;
            }

            /**
              <summary>Gets/Sets the current horizontal scaling [PDF:1.6:5.2.3], normalized to 1.</summary>
            */
            public double Scale
            {
                get => this.scale;
                set => this.scale = value;
            }

            /**
              <summary>Gets the scanner associated to this state.</summary>
            */
            public ContentScanner Scanner => this.scanner;

            /**
              <summary>Gets/Sets the current color for stroking operations [PDF:1.6:4.5.1].</summary>
            */
            public colors::Color StrokeColor
            {
                get => this.strokeColor;
                set => this.strokeColor = value;
            }

            /**
              <summary>Gets/Sets the current color space for stroking operations [PDF:1.6:4.5.1].</summary>
            */
            public colors::ColorSpace StrokeColorSpace
            {
                get => this.strokeColorSpace;
                set => this.strokeColorSpace = value;
            }

            /**
              <summary>Gets/Sets the current text line matrix [PDF:1.6:5.3].</summary>
            */
            public Matrix Tlm
            {
                get => this.tlm;
                set => this.tlm = value;
            }

            /**
              <summary>Gets/Sets the current text matrix [PDF:1.6:5.3].</summary>
            */
            public Matrix Tm
            {
                get => this.tm;
                set => this.tm = value;
            }

            /**
              <summary>Gets/Sets the current word spacing [PDF:1.6:5.2.2].</summary>
            */
            public double WordSpace
            {
                get => this.wordSpace;
                set => this.wordSpace = value;
            }
        }

        public abstract class GraphicsObjectWrapper
        {

            protected RectangleF? box;

            internal static GraphicsObjectWrapper Get(
  ContentScanner scanner
  )
            {
                var obj = scanner.Current;
                if (obj is ShowText)
                {
                    return new TextStringWrapper(scanner);
                }
                else if (obj is Text)
                {
                    return new TextWrapper(scanner);
                }
                else if (obj is XObject)
                {
                    return new XObjectWrapper(scanner);
                }
                else if (obj is InlineImage)
                {
                    return new InlineImageWrapper(scanner);
                }
                else
                {
                    return null;
                }
            }

            /**
<summary>Gets the object's bounding box.</summary>
*/
            public virtual RectangleF? Box => this.box;
        }

        /**
          <summary>Object information.</summary>
          <remarks>
            <para>This class provides derivative (higher-level) information
            about the currently scanned object.</para>
          </remarks>
        */
        public abstract class GraphicsObjectWrapper<TDataObject>
          : GraphicsObjectWrapper
          where TDataObject : ContentObject
        {
            private readonly TDataObject baseDataObject;

            protected GraphicsObjectWrapper(
  TDataObject baseDataObject
  )
            { this.baseDataObject = baseDataObject; }

            /**
<summary>Gets the underlying data object.</summary>
*/
            public TDataObject BaseDataObject => this.baseDataObject;
        }

        /**
          <summary>Inline image information.</summary>
        */
        public sealed class InlineImageWrapper
          : GraphicsObjectWrapper<InlineImage>
        {
            internal InlineImageWrapper(
              ContentScanner scanner
              ) : base((InlineImage)scanner.Current)
            {
                var ctm = scanner.State.Ctm;
                this.box = new RectangleF(
                  ctm.Elements[4],
                  scanner.ContextSize.Height - ctm.Elements[5],
                  ctm.Elements[0],
                  Math.Abs(ctm.Elements[3])
                  );
            }

            /**
              <summary>Gets the inline image.</summary>
            */
            public InlineImage InlineImage => this.BaseDataObject;
        }

        /**
          <summary>Text information.</summary>
        */
        public sealed class TextWrapper
          : GraphicsObjectWrapper<Text>
        {
            private readonly List<TextStringWrapper> textStrings;

            internal TextWrapper(
              ContentScanner scanner
              ) : base((Text)scanner.Current)
            {
                this.textStrings = new List<TextStringWrapper>();
                this.Extract(scanner.ChildLevel);
            }

            private void Extract(
              ContentScanner level
              )
            {
                if (level == null)
                {
                    return;
                }

                while (level.MoveNext())
                {
                    var content = level.Current;
                    if (content is ShowText)
                    { this.textStrings.Add((TextStringWrapper)level.CurrentWrapper); }
                    else if (content is ContainerObject)
                    { this.Extract(level.ChildLevel); }
                }
            }

            public override string ToString(
              )
            { return this.Text; }

            public override RectangleF? Box
            {
                get
                {
                    if (this.box == null)
                    {
                        foreach (var textString in this.textStrings)
                        {
                            if (!this.box.HasValue)
                            { this.box = textString.Box; }
                            else
                            { this.box = RectangleF.Union(this.box.Value, textString.Box.Value); }
                        }
                    }
                    return this.box;
                }
            }

            public string Text
            {
                get
                {
                    var textBuilder = new StringBuilder();
                    foreach (var textString in this.textStrings)
                    { _ = textBuilder.Append(textString.Text); }
                    return textBuilder.ToString();
                }
            }

            /**
              <summary>Gets the text strings.</summary>
            */
            public List<TextStringWrapper> TextStrings => this.textStrings;
        }

        /**
          <summary>Text string information.</summary>
        */
        public sealed class TextStringWrapper
          : GraphicsObjectWrapper<ShowText>,
            ITextString
        {

            private readonly TextStyle style;
            private readonly List<TextChar> textChars;

            internal TextStringWrapper(
              ContentScanner scanner
              ) : base((ShowText)scanner.Current)
            {
                this.textChars = new List<TextChar>();
                var state = scanner.State;
                this.style = new TextStyle(
                  state.Font,
                  state.FontSize * state.Tm.Elements[3],
                  state.RenderMode,
                  state.StrokeColor,
                  state.StrokeColorSpace,
                  state.FillColor,
                  state.FillColorSpace,
                  state.Scale * state.Tm.Elements[0],
                  state.Tm.Elements[3]
                  );
                this.BaseDataObject.Scan(
                  state,
                  new ShowTextScanner(this)
                  );
            }

            public override string ToString(
              )
            { return this.Text; }

            public override RectangleF? Box
            {
                get
                {
                    if (this.box == null)
                    {
                        foreach (var textChar in this.textChars)
                        {
                            if (!this.box.HasValue)
                            { this.box = textChar.Box; }
                            else
                            { this.box = RectangleF.Union(this.box.Value, textChar.Box); }
                        }
                    }
                    return this.box;
                }
            }

            /**
              <summary>Gets the text style.</summary>
            */
            public TextStyle Style => this.style;

            public string Text
            {
                get
                {
                    var textBuilder = new StringBuilder();
                    foreach (var textChar in this.textChars)
                    { _ = textBuilder.Append(textChar); }
                    return textBuilder.ToString();
                }
            }

            public List<TextChar> TextChars => this.textChars;

            private class ShowTextScanner
              : ShowText.IScanner
            {
                private readonly TextStringWrapper wrapper;

                internal ShowTextScanner(
                  TextStringWrapper wrapper
                  )
                { this.wrapper = wrapper; }

                public void ScanChar(
                  char textChar,
                  RectangleF textCharBox
                  )
                {
                    this.wrapper.textChars.Add(
                      new TextChar(
                        textChar,
                        textCharBox,
                        this.wrapper.style,
                        false
                        )
                      );
                }
            }
        }

        /**
          <summary>External object information.</summary>
        */
        public sealed class XObjectWrapper
          : GraphicsObjectWrapper<XObject>
        {
            private readonly PdfName name;
            private readonly xObjects::XObject xObject;

            internal XObjectWrapper(
              ContentScanner scanner
              ) : base((XObject)scanner.Current)
            {
                var ctm = scanner.State.Ctm;
                this.box = new RectangleF(
                  ctm.Elements[4],
                  scanner.ContextSize.Height - ctm.Elements[5],
                  ctm.Elements[0],
                  Math.Abs(ctm.Elements[3])
                  );
                this.name = this.BaseDataObject.Name;
                this.xObject = this.BaseDataObject.GetResource(scanner.ContentContext);
            }

            /**
              <summary>Gets the corresponding resource key.</summary>
            */
            public PdfName Name => this.name;

            /**
              <summary>Gets the external object.</summary>
            */
            public xObjects::XObject XObject => this.xObject;
        }
    }
}