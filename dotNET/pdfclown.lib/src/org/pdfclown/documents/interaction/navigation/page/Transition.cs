/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.navigation.page
{

    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Visual transition to use when moving to a page during a presentation [PDF:1.6:8.3.3].</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public sealed class Transition
      : PdfObjectWrapper<PdfDictionary>
    {
        private static readonly double DefaultDuration = 1;
        private static readonly double DefaultScale = 1;

        private static readonly Dictionary<DirectionEnum, PdfDirectObject> DirectionEnumCodes;
        private static readonly Dictionary<OrientationEnum, PdfName> OrientationEnumCodes;
        private static readonly Dictionary<PageDirectionEnum, PdfName> PageDirectionEnumCodes;
        private static readonly Dictionary<StyleEnum, PdfName> StyleEnumCodes;

        static Transition()
        {
            //TODO: transfer to extension methods!
            DirectionEnumCodes = new Dictionary<DirectionEnum, PdfDirectObject>();
            DirectionEnumCodes[DirectionEnum.LeftToRight] = PdfInteger.Get(0);
            DirectionEnumCodes[DirectionEnum.BottomToTop] = PdfInteger.Get(90);
            DirectionEnumCodes[DirectionEnum.RightToLeft] = PdfInteger.Get(180);
            DirectionEnumCodes[DirectionEnum.TopToBottom] = PdfInteger.Get(270);
            DirectionEnumCodes[DirectionEnum.TopLeftToBottomRight] = PdfInteger.Get(315);
            DirectionEnumCodes[DirectionEnum.None] = PdfName.None;

            OrientationEnumCodes = new Dictionary<OrientationEnum, PdfName>();
            OrientationEnumCodes[OrientationEnum.Horizontal] = PdfName.H;
            OrientationEnumCodes[OrientationEnum.Vertical] = PdfName.V;

            PageDirectionEnumCodes = new Dictionary<PageDirectionEnum, PdfName>();
            PageDirectionEnumCodes[PageDirectionEnum.Inward] = PdfName.I;
            PageDirectionEnumCodes[PageDirectionEnum.Outward] = PdfName.O;

            StyleEnumCodes = new Dictionary<StyleEnum, PdfName>();
            StyleEnumCodes[StyleEnum.Split] = PdfName.Split;
            StyleEnumCodes[StyleEnum.Blinds] = PdfName.Blinds;
            StyleEnumCodes[StyleEnum.Box] = PdfName.Box;
            StyleEnumCodes[StyleEnum.Wipe] = PdfName.Wipe;
            StyleEnumCodes[StyleEnum.Dissolve] = PdfName.Dissolve;
            StyleEnumCodes[StyleEnum.Glitter] = PdfName.Glitter;
            StyleEnumCodes[StyleEnum.Replace] = PdfName.R;
            StyleEnumCodes[StyleEnum.Fly] = PdfName.Fly;
            StyleEnumCodes[StyleEnum.Push] = PdfName.Push;
            StyleEnumCodes[StyleEnum.Cover] = PdfName.Cover;
            StyleEnumCodes[StyleEnum.Uncover] = PdfName.Uncover;
            StyleEnumCodes[StyleEnum.Fade] = PdfName.Fade;
        }

        private Transition(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
<summary>Creates a new action within the given document context.</summary>
*/
        public Transition(
          Document context
          ) : base(
            context,
            new PdfDictionary(
              new PdfName[] { PdfName.Type },
              new PdfDirectObject[] { PdfName.Trans }
              )
            )
        { }

        public Transition(
          Document context,
          StyleEnum style
          ) : this(
            context,
            style,
            DefaultDuration,
            DefaultOrientation,
            DefaultPageDirection,
            DefaultDirection,
            DefaultScale
            )
        { }

        public Transition(
          Document context,
          StyleEnum style,
          double duration
          ) : this(
            context,
            style,
            duration,
            DefaultOrientation,
            DefaultPageDirection,
            DefaultDirection,
            DefaultScale
            )
        { }

        public Transition(
          Document context,
          StyleEnum style,
          double duration,
          OrientationEnum orientation,
          PageDirectionEnum pageDirection,
          DirectionEnum direction,
          double scale
          ) : this(context)
        {
            this.Style = style;
            this.Duration = duration;
            this.Orientation = orientation;
            this.PageDirection = pageDirection;
            this.Direction = direction;
            this.Scale = scale;
        }

        /**
  <summary>Gets the code corresponding to the given value.</summary>
*/
        private static PdfDirectObject ToCode(
          DirectionEnum value
          )
        { return DirectionEnumCodes[value]; }

        /**
          <summary>Gets the code corresponding to the given value.</summary>
        */
        private static PdfName ToCode(
          OrientationEnum value
          )
        { return OrientationEnumCodes[value]; }

        /**
          <summary>Gets the code corresponding to the given value.</summary>
        */
        private static PdfName ToCode(
          PageDirectionEnum value
          )
        { return PageDirectionEnumCodes[value]; }

        /**
          <summary>Gets the code corresponding to the given value.</summary>
        */
        private static PdfName ToCode(
          StyleEnum value
          )
        { return StyleEnumCodes[value]; }

        /**
          <summary>Gets the direction corresponding to the given value.</summary>
        */
        private static DirectionEnum ToDirectionEnum(
          PdfDirectObject value
          )
        {
            foreach (var direction in DirectionEnumCodes)
            {
                if (direction.Value.Equals(value))
                {
                    return direction.Key;
                }
            }
            return DefaultDirection;
        }

        /**
          <summary>Gets the orientation corresponding to the given value.</summary>
        */
        private static OrientationEnum ToOrientationEnum(
          PdfName value
          )
        {
            foreach (var orientation in OrientationEnumCodes)
            {
                if (orientation.Value.Equals(value))
                {
                    return orientation.Key;
                }
            }
            return DefaultOrientation;
        }

        /**
          <summary>Gets the page direction corresponding to the given value.</summary>
        */
        private static PageDirectionEnum ToPageDirectionEnum(
          PdfName value
          )
        {
            foreach (var direction in PageDirectionEnumCodes)
            {
                if (direction.Value.Equals(value))
                {
                    return direction.Key;
                }
            }
            return DefaultPageDirection;
        }

        /**
          <summary>Gets the style corresponding to the given value.</summary>
        */
        private static StyleEnum ToStyleEnum(
          PdfName value
          )
        {
            foreach (var style in StyleEnumCodes)
            {
                if (style.Value.Equals(value))
                {
                    return style.Key;
                }
            }
            return DefaultStyle;
        }

        public static Transition Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new Transition(baseObject) : null; }

        /**
<summary>Gets/Sets the transition direction.</summary>
*/
        public DirectionEnum Direction
        {
            get => ToDirectionEnum(this.BaseDataObject[PdfName.Di]);
            set
            {
                if (value == DefaultDirection)
                { _ = this.BaseDataObject.Remove(PdfName.Di); }
                else
                { this.BaseDataObject[PdfName.Di] = ToCode(value); }
            }
        }

        /**
          <summary>Gets/Sets the duration of the transition effect, in seconds.</summary>
        */
        public double Duration
        {
            get
            {
                var durationObject = (IPdfNumber)this.BaseDataObject[PdfName.D];
                return (durationObject == null)
                  ? DefaultDuration
                  : durationObject.RawValue;
            }
            set
            {
                if (value == DefaultDuration)
                { _ = this.BaseDataObject.Remove(PdfName.D); }
                else
                { this.BaseDataObject[PdfName.D] = PdfReal.Get(value); }
            }
        }

        /**
          <summary>Gets/Sets the transition orientation.</summary>
        */
        public OrientationEnum Orientation
        {
            get => ToOrientationEnum((PdfName)this.BaseDataObject[PdfName.Dm]);
            set
            {
                if (value == DefaultOrientation)
                { _ = this.BaseDataObject.Remove(PdfName.Dm); }
                else
                { this.BaseDataObject[PdfName.Dm] = ToCode(value); }
            }
        }

        /**
          <summary>Gets/Sets the transition direction on page.</summary>
        */
        public PageDirectionEnum PageDirection
        {
            get => ToPageDirectionEnum((PdfName)this.BaseDataObject[PdfName.M]);
            set
            {
                if (value == DefaultPageDirection)
                { _ = this.BaseDataObject.Remove(PdfName.M); }
                else
                { this.BaseDataObject[PdfName.M] = ToCode(value); }
            }
        }

        /**
          <summary>Gets/Sets the scale at which the changes are drawn.</summary>
        */
        [PDF(VersionEnum.PDF15)]
        public double Scale
        {
            get
            {
                var scaleObject = (IPdfNumber)this.BaseDataObject[PdfName.SS];
                return (scaleObject == null)
                  ? DefaultScale
                  : scaleObject.RawValue;
            }
            set
            {
                if (value == DefaultScale)
                { _ = this.BaseDataObject.Remove(PdfName.SS); }
                else
                { this.BaseDataObject[PdfName.SS] = PdfReal.Get(value); }
            }
        }

        /**
          <summary>Gets/Sets the transition style.</summary>
        */
        public StyleEnum Style
        {
            get => ToStyleEnum((PdfName)this.BaseDataObject[PdfName.S]);
            set
            {
                if (value == DefaultStyle)
                { _ = this.BaseDataObject.Remove(PdfName.S); }
                else
                { this.BaseDataObject[PdfName.S] = ToCode(value); }
            }
        }
        /**
  <summary>Transition direction (counterclockwise) [PDF:1.6:8.3.3].</summary>
*/
        public enum DirectionEnum
        {
            /**
              <summary>Left to right.</summary>
            */
            LeftToRight,
            /**
              <summary>Bottom to top.</summary>
            */
            BottomToTop,
            /**
              <summary>Right to left.</summary>
            */
            RightToLeft,
            /**
              <summary>Top to bottom.</summary>
            */
            TopToBottom,
            /**
              <summary>Top-left to bottom-right.</summary>
            */
            TopLeftToBottomRight,
            /**
              <summary>None.</summary>
            */
            None
        };

        /**
          <summary>Transition orientation [PDF:1.6:8.3.3].</summary>
        */
        public enum OrientationEnum
        {
            /**
              <summary>Horizontal.</summary>
            */
            Horizontal,
            /**
              <summary>Vertical.</summary>
            */
            Vertical
        };

        /**
          <summary>Transition direction on page [PDF:1.6:8.3.3].</summary>
        */
        public enum PageDirectionEnum
        {
            /**
              <summary>Inward (from the edges of the page).</summary>
            */
            Inward,
            /**
              <summary>Outward (from the center of the page).</summary>
            */
            Outward
        };

        /**
          <summary>Transition style [PDF:1.6:8.3.3].</summary>
        */
        public enum StyleEnum
        {
            /**
              <summary>Two lines sweep across the screen, revealing the page.</summary>
            */
            Split,
            /**
              <summary>Multiple lines sweep across the screen, revealing the page.</summary>
            */
            Blinds,
            /**
              <summary>A rectangular box sweeps between the edges of the page and the center.</summary>
            */
            Box,
            /**
              <summary>A single line sweeps across the screen from one edge to the other.</summary>
            */
            Wipe,
            /**
              <summary>The old page dissolves gradually.</summary>
            */
            Dissolve,
            /**
              <summary>The old page dissolves gradually sweeping across the page in a wide band
              moving from one side of the screen to the other.</summary>
            */
            Glitter,
            /**
              <summary>No transition.</summary>
            */
            Replace,
            /**
              <summary>Changes are flown across the screen.</summary>
            */
            [PDF(VersionEnum.PDF15)]
            Fly,
            /**
              <summary>The page slides in, pushing away the old one.</summary>
            */
            [PDF(VersionEnum.PDF15)]
            Push,
            /**
              <summary>The page slides on to the screen, covering the old one.</summary>
            */
            [PDF(VersionEnum.PDF15)]
            Cover,
            /**
              <summary>The old page slides off the screen, uncovering the new one.</summary>
            */
            [PDF(VersionEnum.PDF15)]
            Uncover,
            /**
              <summary>The new page reveals gradually.</summary>
            */
            [PDF(VersionEnum.PDF15)]
            Fade
        };

        private static readonly DirectionEnum DefaultDirection = DirectionEnum.LeftToRight;
        private static readonly OrientationEnum DefaultOrientation = OrientationEnum.Horizontal;
        private static readonly PageDirectionEnum DefaultPageDirection = PageDirectionEnum.Inward;
        private static readonly StyleEnum DefaultStyle = StyleEnum.Replace;
    }
}