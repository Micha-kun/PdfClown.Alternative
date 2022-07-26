/*
  Copyright 2013-2015 Stefano Chizzolini. http://www.pdfclown.org

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
    using System;
    using System.Drawing;

    using org.pdfclown.objects;
    using org.pdfclown.util;

    /**
      <summary>Markup annotation [PDF:1.6:8.4.5].</summary>
      <remarks>It represents text-based annotations used primarily to mark up documents.</remarks>
    */
    [PDF(VersionEnum.PDF11)]
    public abstract class Markup
      : Annotation
    {

        protected Markup(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        protected Markup(
Page page,
PdfName subtype,
RectangleF box,
string text
) : base(page, subtype, box, text)
        { this.CreationDate = DateTime.Now; }

        protected PdfName TypeBase
        {
            get => (PdfName)this.BaseDataObject[PdfName.IT];
            set => this.BaseDataObject[PdfName.IT] = value;
        }

        /**
<summary>Gets/Sets the constant opacity value to be used in painting the annotation.</summary>
<remarks>This value applies to all visible elements of the annotation (including its background
and border) but not to the popup window that appears when the annotation is opened.</remarks>
*/
        [PDF(VersionEnum.PDF14)]
        public virtual double Alpha
        {
            get => (double)PdfSimpleObject<object>.GetValue(this.BaseDataObject[PdfName.CA], 1d);
            set => this.BaseDataObject[PdfName.CA] = PdfReal.Get(value);
        }

        /**
          <summary>Gets/Sets the annotation editor. It is displayed as a text label in the title bar of
          the annotation's pop-up window when open and active. By convention, it identifies the user who
          added the annotation.</summary>
        */
        [PDF(VersionEnum.PDF11)]
        public virtual string Author
        {
            get => (string)PdfSimpleObject<object>.GetValue(this.BaseDataObject[PdfName.T]);
            set
            {
                this.BaseDataObject[PdfName.T] = PdfTextString.Get(value);
                this.ModificationDate = DateTime.Now;
            }
        }

        /**
          <summary>Gets/Sets the date and time when the annotation was created.</summary>
        */
        [PDF(VersionEnum.PDF15)]
        public virtual DateTime? CreationDate
        {
            get
            {
                var creationDateObject = this.BaseDataObject[PdfName.CreationDate];
                return (creationDateObject is PdfDate) ? ((DateTime?)((PdfDate)creationDateObject).Value) : null;
            }
            set => this.BaseDataObject[PdfName.CreationDate] = PdfDate.Get(value);
        }

        /**
          <summary>Gets/Sets the annotation that this one is in reply to. Both annotations must be on the
          same page of the document.</summary>
          <remarks>The relationship between the two annotations is specified by the
          <see cref="ReplyType"/> property.</remarks>
        */
        [PDF(VersionEnum.PDF15)]
        public virtual Annotation InReplyTo
        {
            get => Annotation.Wrap(this.BaseDataObject[PdfName.IRT]);
            set => this.BaseDataObject[PdfName.IRT] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the pop-up annotation associated with this one.</summary>
          <exception cref="InvalidOperationException">If pop-up annotations can't be associated with
          this markup.</exception>
        */
        [PDF(VersionEnum.PDF13)]
        public virtual Popup Popup
        {
            get => (Popup)Annotation.Wrap(this.BaseDataObject[PdfName.Popup]);
            set
            {
                value.Markup = this;
                this.BaseDataObject[PdfName.Popup] = value.BaseObject;
            }
        }

        /**
          <summary>Gets/Sets the relationship between this annotation and one specified by
          <see cref="InReplyTo"/> property.</summary>
        */
        [PDF(VersionEnum.PDF16)]
        public virtual ReplyTypeEnum ReplyType
        {
            get => ReplyTypeEnumExtension.Get((PdfName)this.BaseDataObject[PdfName.RT]).Value;
            set => this.BaseDataObject[PdfName.RT] = value.GetCode();
        }

        /**
          <summary>Gets/Sets the annotation subject.</summary>
        */
        [PDF(VersionEnum.PDF15)]
        public virtual string Subject
        {
            get => (string)PdfSimpleObject<object>.GetValue(this.BaseDataObject[PdfName.Subj]);
            set
            {
                this.BaseDataObject[PdfName.Subj] = PdfTextString.Get(value);
                this.ModificationDate = DateTime.Now;
            }
        }
        /**
  <summary>Annotation relationship [PDF:1.6:8.4.5].</summary>
*/
        [PDF(VersionEnum.PDF16)]
        public enum ReplyTypeEnum
        {
            Thread,
            Group
        }
    }

    internal static class ReplyTypeEnumExtension
    {
        private static readonly BiDictionary<Markup.ReplyTypeEnum, PdfName> codes;

        static ReplyTypeEnumExtension()
        {
            codes = new BiDictionary<Markup.ReplyTypeEnum, PdfName>();
            codes[Markup.ReplyTypeEnum.Thread] = PdfName.R;
            codes[Markup.ReplyTypeEnum.Group] = PdfName.Group;
        }

        public static Markup.ReplyTypeEnum? Get(
          PdfName name
          )
        {
            if (name == null)
            {
                return Markup.ReplyTypeEnum.Thread;
            }

            return codes.GetKey(name);
        }

        public static PdfName GetCode(
          this Markup.ReplyTypeEnum replyType
          )
        { return codes[replyType]; }
    }
}