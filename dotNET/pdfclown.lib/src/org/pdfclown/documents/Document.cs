/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.layers;
    using org.pdfclown.documents.interaction.forms;
    using org.pdfclown.documents.interaction.navigation.document;
    using org.pdfclown.documents.interaction.viewer;

    using org.pdfclown.documents.interchange.metadata;
    using org.pdfclown.files;
    using org.pdfclown.objects;
    using drawing = System.Drawing;

    /**
      <summary>PDF document [PDF:1.6::3.6.1].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class Document
      : PdfObjectWrapper<PdfDictionary>,
        IAppDataHolder
    {

        private DocumentConfiguration configuration;

        internal Dictionary<PdfReference, object> Cache = new Dictionary<PdfReference, object>();

        internal Document(
  File context
  ) : base(
    context,
    new PdfDictionary(
      new PdfName[1] { PdfName.Type },
      new PdfDirectObject[1] { PdfName.Catalog }
      )
    )
        {
            this.configuration = new DocumentConfiguration(this);

            // Attach the document catalog to the file trailer!
            context.Trailer[PdfName.Root] = this.BaseObject;

            // Pages collection.
            this.Pages = new Pages(this);

            // Default page size.
            this.PageSize = PageFormat.GetSize();

            // Default resources collection.
            this.Resources = new Resources(this);
        }

        internal Document(
          PdfDirectObject baseObject // Catalog.
          ) : base(baseObject)
        { this.configuration = new DocumentConfiguration(this); }

        /**
  <summary>Gets the default media box.</summary>
*/
        private PdfArray MediaBox =>
                /*
NOTE: Document media box MUST be associated with the page-tree root node in order to be
inheritable by all the pages.
*/
                (PdfArray)((PdfDictionary)this.BaseDataObject.Resolve(PdfName.Pages)).Resolve(PdfName.MediaBox);

        public override object Clone(
          Document context
          )
        { throw new NotImplementedException(); }

        /**
          <summary>Deletes the object from this document context.</summary>
        */
        public void Exclude(
          PdfObjectWrapper obj
          )
        {
            if (obj.File != this.File)
            {
                return;
            }

            _ = obj.Delete();
        }

        /**
          <summary>Deletes the objects from this document context.</summary>
        */
        public void Exclude<T>(
          ICollection<T> objs
          ) where T : PdfObjectWrapper
        {
            foreach (var obj in objs)
            { this.Exclude(obj); }
        }

        public AppData GetAppData(
          PdfName appName
          )
        { return this.AppData.Ensure(appName); }

        /**
          <summary>Gets the document size, that is the maximum page dimensions across the whole document.
          </summary>
          <seealso cref="PageSize"/>
        */
        public drawing::SizeF GetSize(
          )
        {
            float height = 0, width = 0;
            foreach (var page in this.Pages)
            {
                var pageSize = page.Size;
                height = Math.Max(height, pageSize.Height);
                width = Math.Max(width, pageSize.Width);
            }
            return new drawing::SizeF(width, height);
        }

        /**
          <summary>Clones the object within this document context.</summary>
        */
        public PdfObjectWrapper Include(
          PdfObjectWrapper obj
          )
        {
            if (obj.File == this.File)
            {
                return obj;
            }

            return (PdfObjectWrapper)obj.Clone(this);
        }

        /**
          <summary>Clones the collection objects within this document context.</summary>
        */
        public ICollection<T> Include<T>(
          ICollection<T> objs
          ) where T : PdfObjectWrapper
        {
            var includedObjects = new List<T>(objs.Count);
            foreach (var obj in objs)
            { includedObjects.Add((T)this.Include(obj)); }

            return includedObjects;
        }

        /**
          <summary>Registers a named object.</summary>
          <param name="name">Object name.</param>
          <param name="object">Named object.</param>
          <returns>Registered named object.</returns>
        */
        public T Register<T>(
          PdfString name,
          T @object
          ) where T : PdfObjectWrapper
        {
            var namedObjects = this.Names.Get(@object.GetType());
            _ = namedObjects.GetType().GetMethod("set_Item", BindingFlags.Public | BindingFlags.Instance).Invoke(namedObjects, new object[] { name, @object });
            return @object;
        }
        public static T Resolve<T>(
PdfDirectObject baseObject
) where T : PdfObjectWrapper
        {
            if (typeof(Destination).IsAssignableFrom(typeof(T)))
            {
                return Destination.Wrap(baseObject) as T;
            }
            else
            {
                throw new NotSupportedException($"Type '{typeof(T).Name}' wrapping is not supported.");
            }
        }

        /**
          <summary>Forces a named base object to be expressed as its corresponding high-level
          representation.</summary>
        */
        public T ResolveName<T>(
          PdfDirectObject namedBaseObject
          ) where T : PdfObjectWrapper
        {
            if (namedBaseObject is PdfString) // Named object.
            {
                return this.Names.Get<T>((PdfString)namedBaseObject);
            }
            else // Explicit object.
            {
                return Resolve<T>(namedBaseObject);
            }
        }

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
            this.Information.ModificationDate = modificationDate;
        }

        /**
<summary>Gets/Sets the document's behavior in response to trigger events.</summary>
*/
        [PDF(VersionEnum.PDF14)]
        public DocumentActions Actions
        {
            get => new DocumentActions(this.BaseDataObject.Get<PdfDictionary>(PdfName.AA));
            set => this.BaseDataObject[PdfName.AA] = PdfObjectWrapper.GetBaseObject(value);
        }

        public AppDataCollection AppData => AppDataCollection.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.PieceInfo), this);

        /**
          <summary>Gets the article threads.</summary>
        */
        [PDF(VersionEnum.PDF11)]
        public Articles Articles
        {
            get => Articles.Wrap(this.BaseDataObject.Get<PdfArray>(PdfName.Threads, false));
            set => this.BaseDataObject[PdfName.Threads] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the bookmark collection.</summary>
        */
        public Bookmarks Bookmarks
        {
            get => Bookmarks.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.Outlines, false));
            set => this.BaseDataObject[PdfName.Outlines] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the configuration of this document.</summary>
        */
        public DocumentConfiguration Configuration
        {
            get => this.configuration;
            set => this.configuration = value;
        }

        /**
          <summary>Gets/Sets the interactive form (AcroForm).</summary>
        */
        [PDF(VersionEnum.PDF12)]
        public Form Form
        {
            get => Form.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.AcroForm));
            set => this.BaseDataObject[PdfName.AcroForm] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets common document metadata.</summary>
        */
        public Information Information
        {
            get => Information.Wrap(this.File.Trailer.Get<PdfDictionary>(PdfName.Info, false));
            set => this.File.Trailer[PdfName.Info] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the optional content properties.</summary>
        */
        [PDF(VersionEnum.PDF15)]
        public LayerDefinition Layer
        {
            get => LayerDefinition.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.OCProperties));
            set
            {
                this.CheckCompatibility("Layer");
                this.BaseDataObject[PdfName.OCProperties] = PdfObjectWrapper.GetBaseObject(value);
            }
        }

        public DateTime? ModificationDate => this.Information.ModificationDate;

        /**
          <summary>Gets/Sets the name dictionary.</summary>
        */
        [PDF(VersionEnum.PDF12)]
        public Names Names
        {
            get => new Names(this.BaseDataObject.Get<PdfDictionary>(PdfName.Names));
            set => this.BaseDataObject[PdfName.Names] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the page label ranges.</summary>
        */
        [PDF(VersionEnum.PDF13)]
        public PageLabels PageLabels
        {
            get => new PageLabels(this.BaseDataObject.Get<PdfDictionary>(PdfName.PageLabels));
            set
            {
                this.CheckCompatibility("PageLabels");
                this.BaseDataObject[PdfName.PageLabels] = PdfObjectWrapper.GetBaseObject(value);
            }
        }

        /**
          <summary>Gets/Sets the page collection.</summary>
        */
        public Pages Pages
        {
            get => new Pages(this.BaseDataObject[PdfName.Pages]);
            set => this.BaseDataObject[PdfName.Pages] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the default page size [PDF:1.6:3.6.2].</summary>
        */
        public drawing::Size? PageSize
        {
            get
            {
                var mediaBox = this.MediaBox;
                return (mediaBox != null)
                  ? (new drawing::Size(
                    (int)((IPdfNumber)mediaBox[2]).RawValue,
                    (int)((IPdfNumber)mediaBox[3]).RawValue
                    ))
                  : ((drawing::Size?)null);
            }
            set
            {
                var mediaBox = this.MediaBox;
                if (mediaBox == null)
                {
                    // Create default media box!
                    mediaBox = new Rectangle(0, 0, 0, 0).BaseDataObject;
                    // Assign the media box to the document!
                    ((PdfDictionary)this.BaseDataObject.Resolve(PdfName.Pages))[PdfName.MediaBox] = mediaBox;
                }
                mediaBox[2] = PdfReal.Get(value.Value.Width);
                mediaBox[3] = PdfReal.Get(value.Value.Height);
            }
        }

        /**
          <summary>Gets/Sets the default resource collection [PDF:1.6:3.6.2].</summary>
          <remarks>The default resource collection is used as last resort by every page that doesn't
          reference one explicitly (and doesn't reference an intermediate one implicitly).</remarks>
        */
        public Resources Resources
        {
            get => Resources.Wrap(((PdfDictionary)this.BaseDataObject.Resolve(PdfName.Pages)).Get<PdfDictionary>(PdfName.Resources));
            set => ((PdfDictionary)this.BaseDataObject.Resolve(PdfName.Pages))[PdfName.Resources] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the version of the PDF specification this document conforms to.</summary>
        */
        [PDF(VersionEnum.PDF14)]
        public pdfclown.Version Version
        {
            get
            {
                /*
                  NOTE: If the header specifies a later version, or if this entry is absent, the document
                  conforms to the version specified in the header.
                */
                var fileVersion = this.File.Version;

                var versionObject = (PdfName)this.BaseDataObject[PdfName.Version];
                if (versionObject == null)
                {
                    return fileVersion;
                }

                var version = pdfclown.Version.Get(versionObject);
                if (this.File.Reader == null)
                {
                    return version;
                }

                return (version.CompareTo(fileVersion) > 0) ? version : fileVersion;
            }
            set => this.BaseDataObject[PdfName.Version] = PdfName.Get(value);
        }

        /**
          <summary>Gets the way the document is to be presented.</summary>
        */
        public ViewerPreferences ViewerPreferences
        {
            get => ViewerPreferences.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.ViewerPreferences));
            set => this.BaseDataObject[PdfName.ViewerPreferences] = PdfObjectWrapper.GetBaseObject(value);
        }
    }
}
