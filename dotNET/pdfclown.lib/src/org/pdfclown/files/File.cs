/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
    * Kasper Fabaech Brandt (patch contributor [FIX:45], http://sourceforge.net/u/kasperfb/)

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


namespace org.pdfclown.files
{
    using System;
    using System.IO;
    using System.Reflection;
    using org.pdfclown;
    using org.pdfclown.bytes;

    using org.pdfclown.documents;
    using org.pdfclown.objects;
    using org.pdfclown.tokens;

    /**
      <summary>PDF file representation.</summary>
    */
    public sealed class File
      : IDisposable
    {

        private static readonly Random hashCodeGenerator = new Random();

        private Cloner cloner;

        private FileConfiguration configuration;
        private readonly Document document;
        private readonly int hashCode = hashCodeGenerator.Next();
        private readonly IndirectObjects indirectObjects;
        private string path;
        private Reader reader;
        private readonly PdfDictionary trailer;
        private readonly pdfclown.Version version;

        public File(
  )
        {
            this.Initialize();

            this.version = VersionEnum.PDF14.GetVersion();
            this.trailer = this.PrepareTrailer(new PdfDictionary());
            this.indirectObjects = new IndirectObjects(this, null);
            this.document = new Document(this);
        }

        public File(
          string path
          ) : this(
            new bytes.Stream(
              new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read
                )
              )
            )
        { this.path = path; }

        public File(
          byte[] data
          ) : this(new bytes.Buffer(data))
        { }

        public File(
          System.IO.Stream stream
          ) : this(new bytes.Stream(stream))
        { }

        public File(
          IInputStream stream
          )
        {
            this.Initialize();

            this.reader = new Reader(stream, this);
            try // [FIX:45] File constructor didn't dispose reader on error.
            {
                var info = this.reader.ReadInfo();
                this.version = info.Version;
                this.trailer = this.PrepareTrailer(info.Trailer);
                if (this.trailer.ContainsKey(PdfName.Encrypt)) // Encrypted file.
                {
                    throw new NotImplementedException("Encrypted files are currently not supported.");
                }

                this.indirectObjects = new IndirectObjects(this, info.XrefEntries);
                this.document = new Document(this.trailer[PdfName.Root]);
                this.Configuration.XRefMode = PdfName.XRef.Equals(this.trailer[PdfName.Type])
                  ? XRefModeEnum.Compressed
                  : XRefModeEnum.Plain;
            }
            catch (Exception)
            {
                this.reader.Dispose();
                throw;
            }
        }

        ~File(
          )
        { this.Dispose(false); }

        private void Dispose(
  bool disposing
  )
        {
            if (disposing)
            {
                if (this.reader != null)
                {
                    this.reader.Dispose();
                    this.reader = null;

                    /*
                      NOTE: If the temporary file exists (see Save() method), it must overwrite the document file.
                    */
                    if (System.IO.File.Exists(this.TempPath))
                    {
                        System.IO.File.Delete(this.path);
                        System.IO.File.Move(this.TempPath, this.path);
                    }
                }
            }
        }

        private void Initialize(
          )
        { this.configuration = new FileConfiguration(this); }

        private PdfDictionary PrepareTrailer(
          PdfDictionary trailer
          )
        { return (PdfDictionary)new ImplicitContainer(this, trailer).DataObject; }

        private string TempPath => (this.path == null) ? null : $"{this.path}.tmp";

        public void Dispose(
  )
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override int GetHashCode(
          )
        { return this.hashCode; }

        /**
          <summary>Registers an <b>internal data object</b>.</summary>
        */
        public PdfReference Register(
          PdfDataObject obj
          )
        { return this.indirectObjects.Add(obj).Reference; }

        /**
          <summary>Serializes the file to the current file-system path using the <see
          cref="SerializationModeEnum.Standard">standard serialization mode</see>.</summary>
        */
        public void Save(
          )
        { this.Save(SerializationModeEnum.Standard); }

        /**
          <summary>Serializes the file to the current file-system path.</summary>
          <param name="mode">Serialization mode.</param>
        */
        public void Save(
          SerializationModeEnum mode
          )
        {
            if (!System.IO.File.Exists(this.path))
            {
                throw new FileNotFoundException("No valid source path available.");
            }

            /*
              NOTE: The document file cannot be directly overwritten as it's locked for reading by the
              open stream; its update is therefore delayed to its disposal, when the temporary file will
              overwrite it (see Dispose() method).
            */
            this.Save(this.TempPath, mode);
        }

        /**
          <summary>Serializes the file to the specified file system path.</summary>
          <param name="path">Target path.</param>
          <param name="mode">Serialization mode.</param>
        */
        public void Save(
          string path,
          SerializationModeEnum mode
          )
        {
            using (var outputStream = new System.IO.FileStream(path, FileMode.Create, FileAccess.Write))
            { this.Save(new bytes.Stream(outputStream), mode); }
        }

        /**
          <summary>Serializes the file to the specified stream.</summary>
          <remarks>It's caller responsibility to close the stream after this method ends.</remarks>
          <param name="stream">Target stream.</param>
          <param name="mode">Serialization mode.</param>
        */
        public void Save(
          System.IO.Stream stream,
          SerializationModeEnum mode
          )
        { this.Save(new bytes.Stream(stream), mode); }

        /**
          <summary>Serializes the file to the specified stream.</summary>
          <remarks>It's caller responsibility to close the stream after this method ends.</remarks>
          <param name="stream">Target stream.</param>
          <param name="mode">Serialization mode.</param>
        */
        public void Save(
          IOutputStream stream,
          SerializationModeEnum mode
          )
        {
            var information = this.Document.Information;
            if (this.Reader == null)
            {
                information.CreationDate = DateTime.Now;
                try
                {
                    var assemblyTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute))).Title;
                    var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    information.Producer = $"{assemblyTitle} {assemblyVersion}";
                }
                catch
                {/* NOOP */}
            }
            else
            { information.ModificationDate = DateTime.Now; }

            var writer = Writer.Get(this, stream);
            writer.Write(mode);
        }

        /**
          <summary>Unregisters an internal object.</summary>
        */
        public void Unregister(
          PdfReference reference
          )
        { this.indirectObjects.RemoveAt(reference.ObjectNumber); }

        /**
<summary>Gets/Sets the default cloner.</summary>
*/
        public Cloner Cloner
        {
            get
            {
                if (this.cloner == null)
                { this.cloner = new Cloner(this); }

                return this.cloner;
            }
            set => this.cloner = value;
        }

        /**
          <summary>Gets the file configuration.</summary>
        */
        public FileConfiguration Configuration => this.configuration;

        /**
          <summary>Gets the high-level representation of the file content.</summary>
        */
        public Document Document => this.document;

        /**
          <summary>Gets the identifier of this file.</summary>
        */
        public FileIdentifier ID => FileIdentifier.Wrap(this.Trailer[PdfName.ID]);

        /**
          <summary>Gets the indirect objects collection.</summary>
        */
        public IndirectObjects IndirectObjects => this.indirectObjects;

        /**
          <summary>Gets/Sets the file path.</summary>
        */
        public string Path
        {
            get => this.path;
            set => this.path = value;
        }

        /**
          <summary>Gets the data reader backing this file.</summary>
          <returns><code>null</code> in case of newly-created file.</returns>
        */
        public Reader Reader => this.reader;

        /**
          <summary>Gets the file trailer.</summary>
        */
        public PdfDictionary Trailer => this.trailer;

        /**
          <summary>Gets whether the initial state of this file has been modified.</summary>
        */
        public bool Updated => this.indirectObjects.ModifiedObjects.Count > 0;

        /**
          <summary>Gets the file header version [PDF:1.6:3.4.1].</summary>
          <remarks>This property represents just the original file version; to get the actual version,
          use the <see cref="org.pdfclown.documents.Document.Version">Document.Version</see> method.
          </remarks>
        */
        public pdfclown.Version Version => this.version;

        private sealed class ImplicitContainer
  : PdfIndirectObject
        {
            public ImplicitContainer(
              File file,
              PdfDataObject dataObject
              ) : base(file, dataObject, new XRefEntry(int.MinValue, int.MinValue))
            { }
        }
    }
}
