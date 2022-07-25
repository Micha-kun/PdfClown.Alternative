/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

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


namespace org.pdfclown.documents.files
{
    using System;
    using System.Net;
    using org.pdfclown.files;
    using org.pdfclown.objects;

    using org.pdfclown.util;
    using bytes = org.pdfclown.bytes;

    /**
      <summary>Extended reference to the contents of another file [PDF:1.6:3.10.2].</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public sealed class FullFileSpecification
      : FileSpecification
    {

        internal FullFileSpecification(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        internal FullFileSpecification(
Document context,
string path
) : base(
context,
new PdfDictionary(
new PdfName[]
{PdfName.Type},
new PdfDirectObject[]
{PdfName.Filespec}
)
)
        { this.SetPath(path); }

        internal FullFileSpecification(
          EmbeddedFile embeddedFile,
          string filename
          ) : this(embeddedFile.Document, filename)
        { this.EmbeddedFile = embeddedFile; }

        internal FullFileSpecification(
          Document context,
          Uri url
          ) : this(context, url.ToString())
        { this.FileSystem = StandardFileSystemEnum.URL; }

        /**
          <summary>Gets the related files associated to the given key.</summary>
        */
        private RelatedFiles GetDependencies(
          PdfName key
          )
        {
            var dependenciesObject = (PdfDictionary)this.BaseDictionary[PdfName.RF];
            if (dependenciesObject == null)
            {
                return null;
            }

            return RelatedFiles.Wrap(dependenciesObject[key]);
        }

        /**
          <summary>Gets the embedded file associated to the given key.</summary>
        */
        private EmbeddedFile GetEmbeddedFile(
          PdfName key
          )
        {
            var embeddedFilesObject = (PdfDictionary)this.BaseDictionary[PdfName.EF];
            if (embeddedFilesObject == null)
            {
                return null;
            }

            return EmbeddedFile.Wrap(embeddedFilesObject[key]);
        }

        /**
          <summary>Gets the path associated to the given key.</summary>
        */
        private string GetPath(
          PdfName key
          )
        { return (string)PdfSimpleObject<object>.GetValue(this.BaseDictionary[key]); }

        /**
          <see cref="GetDependencies(PdfName)"/>
        */
        private void SetDependencies(
          PdfName key,
          RelatedFiles value
          )
        {
            var dependenciesObject = (PdfDictionary)this.BaseDictionary[PdfName.RF];
            if (dependenciesObject == null)
            { this.BaseDictionary[PdfName.RF] = dependenciesObject = new PdfDictionary(); }

            dependenciesObject[key] = value.BaseObject;
        }

        /**
          <see cref="GetEmbeddedFile(PdfName)"/>
        */
        private void SetEmbeddedFile(
          PdfName key,
          EmbeddedFile value
          )
        {
            var embeddedFilesObject = (PdfDictionary)this.BaseDictionary[PdfName.EF];
            if (embeddedFilesObject == null)
            { this.BaseDictionary[PdfName.EF] = embeddedFilesObject = new PdfDictionary(); }

            embeddedFilesObject[key] = value.BaseObject;
        }

        /**
          <see cref="GetPath(PdfName)"/>
        */
        private void SetPath(
          PdfName key,
          string value
          )
        { this.BaseDictionary[key] = new PdfString(value); }

        private PdfDictionary BaseDictionary => (PdfDictionary)this.BaseDataObject;

        public override bytes::IInputStream GetInputStream(
          )
        {
            if (PdfName.URL.Equals(this.BaseDictionary[PdfName.FS])) // Remote resource [PDF:1.7:3.10.4].
            {
                Uri fileUrl;
                try
                { fileUrl = new Uri(this.Path); }
                catch (Exception e)
                { throw new Exception($"Failed to instantiate URL for {this.Path}", e); }
                var webClient = new WebClient();
                try
                { return new bytes::Buffer(webClient.OpenRead(fileUrl)); }
                catch (Exception e)
                { throw new Exception($"Failed to open input stream for {this.Path}", e); }
            }
            else // Local resource [PDF:1.7:3.10.1].
            {
                return base.GetInputStream();
            }
        }

        public override bytes::IOutputStream GetOutputStream(
          )
        {
            if (PdfName.URL.Equals(this.BaseDictionary[PdfName.FS])) // Remote resource [PDF:1.7:3.10.4].
            {
                Uri fileUrl;
                try
                { fileUrl = new Uri(this.Path); }
                catch (Exception e)
                { throw new Exception($"Failed to instantiate URL for {this.Path}", e); }
                var webClient = new WebClient();
                try
                { return new bytes::Stream(webClient.OpenWrite(fileUrl)); }
                catch (Exception e)
                { throw new Exception($"Failed to open output stream for {this.Path}", e); }
            }
            else // Local resource [PDF:1.7:3.10.1].
            {
                return base.GetOutputStream();
            }
        }

        public void SetPath(
          string value
          )
        { this.SetPath(PdfName.F, value); }

        /**
<summary>Gets/Sets the related files.</summary>
*/
        public RelatedFiles Dependencies
        {
            get => this.GetDependencies(PdfName.F);
            set => this.SetDependencies(PdfName.F, value);
        }

        /**
          <summary>Gets/Sets the description of the file.</summary>
        */
        public string Description
        {
            get => (string)PdfSimpleObject<object>.GetValue(this.BaseDictionary[PdfName.Desc]);
            set => this.BaseDictionary[PdfName.Desc] = new PdfTextString(value);
        }

        /**
          <summary>Gets/Sets the embedded file corresponding to this file.</summary>
        */
        public EmbeddedFile EmbeddedFile
        {
            get => this.GetEmbeddedFile(PdfName.F);
            set => this.SetEmbeddedFile(PdfName.F, value);
        }

        /**
          <summary>Gets/Sets the file system to be used to interpret this file specification.</summary>
          <returns>Either <see cref="StandardFileSystemEnum"/> (standard file system) or
          <see cref="string"/> (custom file system).</returns>
        */
        public object FileSystem
        {
            get
            {
                var fileSystemObject = (PdfName)this.BaseDictionary[PdfName.FS];
                var standardFileSystem = StandardFileSystemEnumExtension.Get(fileSystemObject);
                return standardFileSystem.HasValue ? standardFileSystem.Value : fileSystemObject.Value;
            }
            set
            {
                PdfName fileSystemObject;
                if (value is StandardFileSystemEnum)
                { fileSystemObject = ((StandardFileSystemEnum)value).GetCode(); }
                else if (value is string)
                { fileSystemObject = new PdfName((string)value); }
                else
                {
                    throw new ArgumentException("MUST be either StandardFileSystemEnum (standard file system) or String (custom file system)");
                }

                this.BaseDictionary[PdfName.FS] = fileSystemObject;
            }
        }

        /**
          <summary>Gets/Sets the identifier of the file.</summary>
        */
        public FileIdentifier ID
        {
            get => FileIdentifier.Wrap(this.BaseDictionary[PdfName.ID]);
            set => this.BaseDictionary[PdfName.ID] = value.BaseObject;
        }

        public override string Path => this.GetPath(PdfName.F);

        /**
          <summary>Gets/Sets whether the referenced file is volatile (changes frequently with time).
          </summary>
        */
        public bool Volatile
        {
            get => (bool)PdfSimpleObject<object>.GetValue(this.BaseDictionary[PdfName.V], false);
            set => this.BaseDictionary[PdfName.V] = PdfBoolean.Get(value);
        }
        /**
  <summary>Standard file system.</summary>
*/
        public enum StandardFileSystemEnum
        {
            /**
              <summary>Generic platform file system.</summary>
            */
            Native,
            /**
              <summary>Uniform resource locator.</summary>
            */
            URL
        }
    }

    internal static class StandardFileSystemEnumExtension
    {
        private static readonly BiDictionary<FullFileSpecification.StandardFileSystemEnum, PdfName> codes;

        static StandardFileSystemEnumExtension(
          )
        {
            codes = new BiDictionary<FullFileSpecification.StandardFileSystemEnum, PdfName>();
            codes[FullFileSpecification.StandardFileSystemEnum.Native] = null;
            codes[FullFileSpecification.StandardFileSystemEnum.URL] = PdfName.URL;
        }

        public static FullFileSpecification.StandardFileSystemEnum? Get(
          PdfName code
          )
        { return codes.GetKey(code); }

        public static PdfName GetCode(
          this FullFileSpecification.StandardFileSystemEnum standardFileSystem
          )
        { return codes[standardFileSystem]; }
    }
}