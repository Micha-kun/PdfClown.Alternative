/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
    * Andreas Pinter (bug reporter [FIX:53], https://sourceforge.net/u/drunal/)

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


namespace org.pdfclown.documents
{
    using System;
    using System.Collections.Generic;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.objects;
    using org.pdfclown.documents.interaction.navigation.page;
    using org.pdfclown.documents.interchange.metadata;

    using org.pdfclown.objects;
    using drawing = System.Drawing;
    using xObjects = org.pdfclown.documents.contents.xObjects;

    /**
      <summary>Document page [PDF:1.6:3.6.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public class Page
      : PdfObjectWrapper<PdfDictionary>,
        IContentContext
    {
        /*
          NOTE: Inheritable attributes are NOT early-collected, as they are NOT part
          of the explicit representation of a page. They are retrieved every time
          clients call.
        */
        #region types
        /**
          <summary>Annotations tab order [PDF:1.6:3.6.2].</summary>
        */
        [PDF(VersionEnum.PDF15)]
        public enum TabOrderEnum
        {
            /**
              <summary>Row order.</summary>
            */
            Row,
            /**
              <summary>Column order.</summary>
            */
            Column,
            /**
              <summary>Structure order.</summary>
            */
            Structure
        };
        #endregion

        #region static
        #region fields
        public static readonly ISet<PdfName> InheritableAttributeKeys;

        private static readonly Dictionary<TabOrderEnum, PdfName> TabOrderEnumCodes;
        #endregion

        #region constructors
        static Page()
        {
            InheritableAttributeKeys = new HashSet<PdfName>();
            _ = InheritableAttributeKeys.Add(PdfName.Resources);
            _ = InheritableAttributeKeys.Add(PdfName.MediaBox);
            _ = InheritableAttributeKeys.Add(PdfName.CropBox);
            _ = InheritableAttributeKeys.Add(PdfName.Rotate);

            TabOrderEnumCodes = new Dictionary<TabOrderEnum, PdfName>();
            TabOrderEnumCodes[TabOrderEnum.Row] = PdfName.R;
            TabOrderEnumCodes[TabOrderEnum.Column] = PdfName.C;
            TabOrderEnumCodes[TabOrderEnum.Structure] = PdfName.S;
        }
        #endregion

        #region interface
        #region public
        /**
          <summary>Gets the attribute value corresponding to the specified key, possibly recurring to
          its ancestor nodes in the page tree.</summary>
          <param name="pageObject">Page object.</param>
          <param name="key">Attribute key.</param>
        */
        public static PdfDirectObject GetInheritableAttribute(
          PdfDictionary pageObject,
          PdfName key
          )
        {
            /*
              NOTE: It moves upward until it finds the inherited attribute.
            */
            var dictionary = pageObject;
            while (true)
            {
                var entry = dictionary[key];
                if (entry != null)
                {
                    return entry;
                }

                dictionary = (PdfDictionary)dictionary.Resolve(PdfName.Parent);
                if (dictionary == null)
                {
                    // Isn't the page attached to the page tree?
                    /* NOTE: This condition is illegal. */
                    if (pageObject[PdfName.Parent] == null)
                    {
                        throw new Exception("Inheritable attributes unreachable: Page objects MUST be inserted into their document's Pages collection before being used.");
                    }

                    return null;
                }
            }
        }

        public static Page Wrap(
          PdfDirectObject baseObject
          )
        { return baseObject == null ? null : new Page(baseObject); }
        #endregion

        #region private
        /**
          <summary>Gets the code corresponding to the given value.</summary>
        */
        private static PdfName ToCode(
          TabOrderEnum value
          )
        { return TabOrderEnumCodes[value]; }

        /**
          <summary>Gets the tab order corresponding to the given value.</summary>
        */
        private static TabOrderEnum ToTabOrderEnum(
          PdfName value
          )
        {
            foreach (var tabOrder in TabOrderEnumCodes)
            {
                if (tabOrder.Value.Equals(value))
                {
                    return tabOrder.Key;
                }
            }
            return TabOrderEnum.Row;
        }
        #endregion
        #endregion
        #endregion

        #region dynamic
        #region constructors
        /**
          <summary>Creates a new page within the specified document context, using the default size.
          </summary>
          <param name="context">Document where to place this page.</param>
        */
        public Page(
          Document context
          ) : this(context, null)
        { }

        /**
          <summary>Creates a new page within the specified document context.</summary>
          <param name="context">Document where to place this page.</param>
          <param name="size">Page size. In case of <code>null</code>, uses the default size.</param>
        */
        public Page(
          Document context,
          drawing::SizeF? size
          ) : base(
            context,
            new PdfDictionary(
              new PdfName[]
              {
            PdfName.Type,
            PdfName.Contents
              },
              new PdfDirectObject[]
              {
            PdfName.Page,
            context.File.Register(new PdfStream())
              }
              )
            )
        {
            if (size.HasValue)
            { this.Size = size.Value; }
        }

        private Page(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        #endregion

        #region interface
        #region public
        /**
          <summary>Gets/Sets the page's behavior in response to trigger events.</summary>
        */
        [PDF(VersionEnum.PDF12)]
        public PageActions Actions
        {
            get => new PageActions(this.BaseDataObject.Get<PdfDictionary>(PdfName.AA));
            set => this.BaseDataObject[PdfName.AA] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the annotations associated to the page.</summary>
        */
        public PageAnnotations Annotations
        {
            get => new PageAnnotations(this.BaseDataObject.Get<PdfArray>(PdfName.Annots), this);
            set => this.BaseDataObject[PdfName.Annots] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the extent of the page's meaningful content (including potential white space)
          as intended by the page's creator [PDF:1.7:10.10.1].</summary>
          <seealso cref="CropBox"/>
        */
        [PDF(VersionEnum.PDF13)]
        public drawing::RectangleF? ArtBox
        {
            get
            {
                /*
                  NOTE: The default value is the page's crop box.
                */
                var artBoxObject = this.GetInheritableAttribute(PdfName.ArtBox);
                return artBoxObject != null ? Rectangle.Wrap(artBoxObject).ToRectangleF() : this.CropBox;
            }
            set => this.BaseDataObject[PdfName.ArtBox] = value.HasValue ? new Rectangle(value.Value).BaseDataObject : null;
        }

        /**
          <summary>Gets the page article beads.</summary>
        */
        public PageArticleElements ArticleElements => new PageArticleElements(this.BaseDataObject.Get<PdfArray>(PdfName.B), this);

        /**
          <summary>Gets/Sets the region to which the contents of the page should be clipped when output
          in a production environment [PDF:1.7:10.10.1].</summary>
          <remarks>
            <para>This may include any extra bleed area needed to accommodate the physical limitations of
            cutting, folding, and trimming equipment. The actual printed page may include printing marks
            that fall outside the bleed box.</para>
          </remarks>
          <seealso cref="CropBox"/>
        */
        [PDF(VersionEnum.PDF13)]
        public drawing::RectangleF? BleedBox
        {
            get
            {
                /*
                  NOTE: The default value is the page's crop box.
                */
                var bleedBoxObject = this.GetInheritableAttribute(PdfName.BleedBox);
                return bleedBoxObject != null ? Rectangle.Wrap(bleedBoxObject).ToRectangleF() : this.CropBox;
            }
            set => this.BaseDataObject[PdfName.BleedBox] = value.HasValue ? new Rectangle(value.Value).BaseDataObject : null;
        }

        /**
          <summary>Gets/Sets the region to which the contents of the page are to be clipped (cropped)
          when displayed or printed [PDF:1.7:10.10.1].</summary>
          <remarks>
            <para>Unlike the other boxes, the crop box has no defined meaning in terms of physical page
            geometry or intended use; it merely imposes clipping on the page contents. However, in the
            absence of additional information, the crop box determines how the page's contents are to be
            positioned on the output medium.</para>
          </remarks>
          <seealso cref="Box"/>
        */
        public drawing::RectangleF? CropBox
        {
            get
            {
                /*
                  NOTE: The default value is the page's media box.
                */
                var cropBoxObject = this.GetInheritableAttribute(PdfName.CropBox);
                return cropBoxObject != null ? Rectangle.Wrap(cropBoxObject).ToRectangleF() : this.Box;
            }
            set => this.BaseDataObject[PdfName.CropBox] = value.HasValue ? new Rectangle(value.Value).BaseDataObject : null;
        }

        /**
          <summary>Gets/Sets the page's display duration.</summary>
          <remarks>
            <para>The page's display duration (also called its advance timing)
            is the maximum length of time, in seconds, that the page is displayed
            during presentations before the viewer application automatically advances
            to the next page.</para>
            <para>By default, the viewer does not advance automatically.</para>
          </remarks>
        */
        [PDF(VersionEnum.PDF11)]
        public double Duration
        {
            get
            {
                var durationObject = (IPdfNumber)this.BaseDataObject[PdfName.Dur];
                return durationObject == null ? 0 : durationObject.RawValue;
            }
            set => this.BaseDataObject[PdfName.Dur] = value > 0 ? PdfReal.Get(value) : null;
        }

        /**
          <summary>Gets the index of this page.</summary>
        */
        public int Index
        {
            get
            {
                /*
                  NOTE: We'll scan sequentially each page-tree level above this page object
                  collecting page counts. At each level we'll scan the kids array from the
                  lower-indexed item to the ancestor of this page object at that level.
                */
                var ancestorKidReference = (PdfReference)this.BaseObject;
                var parentReference = (PdfReference)this.BaseDataObject[PdfName.Parent];
                var parent = (PdfDictionary)parentReference.DataObject;
                var kids = (PdfArray)parent.Resolve(PdfName.Kids);
                var index = 0;
                for (
                  var i = 0;
                  true;
                  i++
                  )
                {
                    var kidReference = (PdfReference)kids[i];
                    // Is the current-level counting complete?
                    // NOTE: It's complete when it reaches the ancestor at this level.
                    if (kidReference.Equals(ancestorKidReference)) // Ancestor node.
                    {
                        // Does the current level correspond to the page-tree root node?
                        if (!parent.ContainsKey(PdfName.Parent))
                        {
                            // We reached the top: counting's finished.
                            return index;
                        }
                        // Set the ancestor at the next level!
                        ancestorKidReference = parentReference;
                        // Move up one level!
                        parentReference = (PdfReference)parent[PdfName.Parent];
                        parent = (PdfDictionary)parentReference.DataObject;
                        kids = (PdfArray)parent.Resolve(PdfName.Kids);
                        i = -1;
                    }
                    else // Intermediate node.
                    {
                        var kid = (PdfDictionary)kidReference.DataObject;
                        if (kid[PdfName.Type].Equals(PdfName.Page))
                        {
                            index++;
                        }
                        else
                        {
                            index += ((PdfInteger)kid[PdfName.Count]).RawValue;
                        }
                    }
                }
            }
        }

        /**
          <summary>Gets the page number.</summary>
        */
        public int Number => this.Index + 1;

        /**
          <summary>Gets/Sets the page size.</summary>
        */
        public drawing::SizeF Size
        {
            get => this.Box.Size;
            set
            {
                drawing::RectangleF box;
                try
                { box = this.Box; }
                catch
                { box = new drawing::RectangleF(0, 0, 0, 0); }
                box.Size = value;
                this.Box = box;
            }
        }

        /**
          <summary>Gets/Sets the tab order to be used for annotations on the page.</summary>
        */
        [PDF(VersionEnum.PDF15)]
        public TabOrderEnum TabOrder
        {
            get => ToTabOrderEnum((PdfName)this.BaseDataObject[PdfName.Tabs]);
            set => this.BaseDataObject[PdfName.Tabs] = ToCode(value);
        }

        /**
          <summary>Gets the transition effect to be used
          when displaying the page during presentations.</summary>
        */
        [PDF(VersionEnum.PDF11)]
        public Transition Transition
        {
            get => Transition.Wrap(this.BaseDataObject[PdfName.Trans]);
            set => this.BaseDataObject[PdfName.Trans] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the intended dimensions of the finished page after trimming
          [PDF:1.7:10.10.1].</summary>
          <remarks>
            <para>It may be smaller than the media box to allow for production-related content, such as
            printing instructions, cut marks, or color bars.</para>
          </remarks>
          <seealso cref="CropBox"/>
        */
        [PDF(VersionEnum.PDF13)]
        public drawing::RectangleF? TrimBox
        {
            get
            {
                /*
                  NOTE: The default value is the page's crop box.
                */
                var trimBoxObject = this.GetInheritableAttribute(PdfName.TrimBox);
                return trimBoxObject != null ? Rectangle.Wrap(trimBoxObject).ToRectangleF() : this.CropBox;
            }
            set => this.BaseDataObject[PdfName.TrimBox] = value.HasValue ? new Rectangle(value.Value).BaseDataObject : null;
        }

        #region IContentContext
        public drawing::RectangleF Box
        {
            get => Rectangle.Wrap(this.GetInheritableAttribute(PdfName.MediaBox)).ToRectangleF();
            set => this.BaseDataObject[PdfName.MediaBox] = new Rectangle(value).BaseDataObject;
        }

        public Contents Contents
        {
            get
            {
                var contentsObject = this.BaseDataObject[PdfName.Contents];
                if (contentsObject == null)
                { this.BaseDataObject[PdfName.Contents] = contentsObject = this.File.Register(new PdfStream()); }
                return Contents.Wrap(contentsObject, this);
            }
        }

        public void Render(
          drawing::Graphics context,
          drawing::SizeF size
          )
        {
            var scanner = new ContentScanner(this.Contents);
            scanner.Render(context, size);
        }

        public Resources Resources
        {
            get
            {
                var resources = Resources.Wrap(this.GetInheritableAttribute(PdfName.Resources));
                return resources != null ? resources : Resources.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.Resources));
            }
        }

        public RotationEnum Rotation
        {
            get => RotationEnumExtension.Get((IPdfNumber)this.GetInheritableAttribute(PdfName.Rotate));
            set => this.BaseDataObject[PdfName.Rotate] = PdfInteger.Get((int)value);
        }

        #region IAppDataHolder
        public AppDataCollection AppData => AppDataCollection.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.PieceInfo), this);

        public AppData GetAppData(
          PdfName appName
          )
        { return this.AppData.Ensure(appName); }

        public DateTime? ModificationDate => (DateTime)PdfSimpleObject<object>.GetValue(this.BaseDataObject[PdfName.LastModified]);

        public void Touch(
          PdfName appName
          )
        { this.Touch(appName, DateTime.Now); }

        public void Touch(
          PdfName appName,
          DateTime modificationDate
          )
        {
            this.GetAppData(appName).ModificationDate = modificationDate;
            this.BaseDataObject[PdfName.LastModified] = new PdfDate(modificationDate);
        }
        #endregion

        #region IContentEntity
        public ContentObject ToInlineObject(
          PrimitiveComposer composer
          )
        { throw new NotImplementedException(); }

        public xObjects::XObject ToXObject(
          Document context
          )
        {
            xObjects::FormXObject form;
            {
                form = new xObjects::FormXObject(context, this.Box);
                form.Resources = (Resources)(
                  context == this.Document  // [FIX:53] Ambiguous context identity.
                    ? this.Resources // Same document: reuses the existing resources.
                    : this.Resources.Clone(context) // Alien document: clones the resources.
                  );

                // Body (contents).
                {
                    var formBody = form.BaseDataObject.Body;
                    var contentsDataObject = this.BaseDataObject.Resolve(PdfName.Contents);
                    if (contentsDataObject is PdfStream)
                    { _ = formBody.Append(((PdfStream)contentsDataObject).Body); }
                    else
                    {
                        foreach (var contentStreamObject in (PdfArray)contentsDataObject)
                        { _ = formBody.Append(((PdfStream)contentStreamObject.Resolve()).Body); }
                    }
                }
            }
            return form;
        }
        #endregion
        #endregion
        #endregion

        #region private
        private PdfDirectObject GetInheritableAttribute(
          PdfName key
          )
        { return GetInheritableAttribute(this.BaseDataObject, key); }
        #endregion
        #endregion
        #endregion
    }
}
