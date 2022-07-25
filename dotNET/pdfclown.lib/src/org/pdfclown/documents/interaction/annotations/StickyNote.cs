/*
  Copyright 2008-2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.annotations
{

    using System.Collections.Generic;
    using System.Drawing;
    using org.pdfclown.objects;

    /**
      <summary>Text annotation [PDF:1.6:8.4.5].</summary>
      <remarks>It represents a sticky note attached to a point in the PDF document.</remarks>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class StickyNote
      : Markup
    {
        private static readonly bool DefaultOpen = false;

        private static readonly Dictionary<IconTypeEnum, PdfName> IconTypeEnumCodes;

        static StickyNote()
        {
            IconTypeEnumCodes = new Dictionary<IconTypeEnum, PdfName>();
            IconTypeEnumCodes[IconTypeEnum.Comment] = PdfName.Comment;
            IconTypeEnumCodes[IconTypeEnum.Help] = PdfName.Help;
            IconTypeEnumCodes[IconTypeEnum.Insert] = PdfName.Insert;
            IconTypeEnumCodes[IconTypeEnum.Key] = PdfName.Key;
            IconTypeEnumCodes[IconTypeEnum.NewParagraph] = PdfName.NewParagraph;
            IconTypeEnumCodes[IconTypeEnum.Note] = PdfName.Note;
            IconTypeEnumCodes[IconTypeEnum.Paragraph] = PdfName.Paragraph;
        }

        internal StickyNote(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public StickyNote(
Page page,
PointF location,
string text
) : base(
page,
PdfName.Text,
new RectangleF(location.X, location.Y, 0, 0),
text
)
        { }

        /**
<summary>Gets the code corresponding to the given value.</summary>
*/
        private static PdfName ToCode(
          IconTypeEnum value
          )
        { return IconTypeEnumCodes[value]; }

        /**
          <summary>Gets the icon type corresponding to the given value.</summary>
        */
        private static IconTypeEnum ToIconTypeEnum(
          PdfName value
          )
        {
            foreach (var iconType in IconTypeEnumCodes)
            {
                if (iconType.Value.Equals(value))
                {
                    return iconType.Key;
                }
            }
            return DefaultIconType;
        }

        /**
<summary>Gets/Sets the icon to be used in displaying the annotation.</summary>
*/
        public IconTypeEnum IconType
        {
            get => ToIconTypeEnum((PdfName)this.BaseDataObject[PdfName.Name]);
            set => this.BaseDataObject[PdfName.Name] = (value != DefaultIconType) ? ToCode(value) : null;
        }

        /**
          <summary>Gets/Sets whether the annotation should initially be displayed open.</summary>
        */
        public bool IsOpen
        {
            get
            {
                var openObject = (PdfBoolean)this.BaseDataObject[PdfName.Open];
                return (openObject != null) ? openObject.BooleanValue : DefaultOpen;
            }
            set => this.BaseDataObject[PdfName.Open] = (value != DefaultOpen) ? PdfBoolean.Get(value) : null;
        }
        /**
  <summary>Icon to be used in displaying the annotation [PDF:1.6:8.4.5].</summary>
*/
        public enum IconTypeEnum
        {
            /**
              <summary>Comment.</summary>
            */
            Comment,
            /**
              <summary>Help.</summary>
            */
            Help,
            /**
              <summary>Insert.</summary>
            */
            Insert,
            /**
              <summary>Key.</summary>
            */
            Key,
            /**
              <summary>New paragraph.</summary>
            */
            NewParagraph,
            /**
              <summary>Note.</summary>
            */
            Note,
            /**
              <summary>Paragraph.</summary>
            */
            Paragraph
        };

        private static readonly IconTypeEnum DefaultIconType = IconTypeEnum.Note;

        //TODO:State and StateModel!!!
    }
}