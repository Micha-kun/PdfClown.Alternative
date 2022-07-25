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


namespace org.pdfclown.objects
{
    using System;
    using System.Text;
    using org.pdfclown.bytes;

    using org.pdfclown.files;
    using org.pdfclown.tokens;

    /**
      <summary>PDF indirect object [PDF:1.6:3.2.9].</summary>
    */
    public class PdfIndirectObject
      : PdfObject,
        IPdfIndirectObject
    {
        private static readonly byte[] BeginIndirectObjectChunk = tokens.Encoding.Pdf.Encode(Symbol.Space + Keyword.BeginIndirectObject + Symbol.LineFeed);
        private static readonly byte[] EndIndirectObjectChunk = tokens.Encoding.Pdf.Encode(Symbol.LineFeed + Keyword.EndIndirectObject + Symbol.LineFeed);

        private PdfDataObject dataObject;
        private File file;
        private bool original;
        private readonly PdfReference reference;

        private bool updateable = true;
        private bool updated;
        private bool virtual_;
        private readonly XRefEntry xrefEntry;

        /**
  <param name="file">Associated file.</param>
  <param name="dataObject">
    <para>Data object associated to the indirect object. It MUST be</para>
    <list type="bullet">
      <item><code>null</code>, if the indirect object is original or free.</item>
      <item>NOT <code>null</code>, if the indirect object is new and in-use.</item>
    </list>
  </param>
  <param name="xrefEntry">Cross-reference entry associated to the indirect object. If the
    indirect object is new, its offset field MUST be set to 0.</param>
*/
        internal PdfIndirectObject(
          File file,
          PdfDataObject dataObject,
          XRefEntry xrefEntry
          )
        {
            this.file = file;
            this.dataObject = this.Include(dataObject);
            this.xrefEntry = xrefEntry;

            this.original = xrefEntry.Offset >= 0;
            this.reference = new PdfReference(this);
        }

        protected internal override bool Virtual
        {
            get => this.virtual_;
            set
            {
                if (this.virtual_ && !value)
                {
                    /*
                      NOTE: When a virtual indirect object becomes concrete it must be registered.
                    */
                    _ = this.file.IndirectObjects.AddVirtual(this);
                    this.virtual_ = false;
                    this.Reference.Update();
                }
                else
                { this.virtual_ = value; }
                this.dataObject.Virtual = this.virtual_;
            }
        }

        internal void DropFile(
  )
        {
            this.Uncompress();
            this.file = null;
        }

        internal void DropOriginal(
          )
        { this.original = false; }

        public override PdfObject Accept(
IVisitor visitor,
object data
)
        { return visitor.Visit(this, data); }

        /**
          <summary>Adds the <see cref="DataObject">data object</see> to the specified object stream
          [PDF:1.6:3.4.6].</summary>
          <param name="objectStream">Target object stream.</param>
         */
        public void Compress(
          ObjectStream objectStream
          )
        {
            // Remove from previous object stream!
            this.Uncompress();

            if ((objectStream != null)
               && this.IsCompressible())
            {
                // Add to the object stream!
                objectStream[this.xrefEntry.Number] = this.DataObject;
                // Update its xref entry!
                this.xrefEntry.Usage = XRefEntry.UsageEnum.InUseCompressed;
                this.xrefEntry.StreamNumber = objectStream.Reference.ObjectNumber;
                this.xrefEntry.Offset = XRefEntry.UndefinedOffset; // Internal object index unknown (to set on object stream serialization -- see ObjectStream).
            }
        }

        public override bool Delete(
          )
        {
            if (this.file != null)
            {
                /*
                  NOTE: It's expected that DropFile() is invoked by IndirectObjects.Remove() method;
                  such an action is delegated because clients may invoke directly Remove() method,
                  skipping this method.
                */
                this.file.IndirectObjects.RemoveAt(this.xrefEntry.Number);
            }
            return true;
        }

        public override int GetHashCode(
          )
        { return this.reference.GetHashCode(); }

        /**
          <summary>Gets whether this object is compressed within an object stream [PDF:1.6:3.4.6].
          </summary>
        */
        public bool IsCompressed(
          )
        { return this.xrefEntry.Usage == XRefEntry.UsageEnum.InUseCompressed; }

        /**
          <summary>Gets whether this object can be compressed within an object stream [PDF:1.6:3.4.6].
          </summary>
        */
        public bool IsCompressible(
          )
        {
            return !this.IsCompressed()
              && this.IsInUse()
              && !((this.DataObject is PdfStream)
                || (this.dataObject is PdfInteger))
              && (this.Reference.GenerationNumber == 0);
        }

        /**
          <summary>Gets whether this object contains a data object.</summary>
        */
        public bool IsInUse(
          )
        { return this.xrefEntry.Usage == XRefEntry.UsageEnum.InUse; }

        /**
          <summary>Gets whether this object comes intact from an existing file.</summary>
        */
        public bool IsOriginal(
          )
        { return this.original; }

        public override PdfObject Swap(
          PdfObject other
          )
        {
            var otherObject = (PdfIndirectObject)other;
            var otherDataObject = otherObject.dataObject;
            // Update the other!
            otherObject.DataObject = this.dataObject;
            // Update this one!
            this.DataObject = otherDataObject;
            return this;
        }

        public override string ToString(
          )
        {
            var buffer = new StringBuilder();
            // Header.
            _ = buffer.Append(this.reference.Id).Append(" obj").Append(Symbol.LineFeed);
            // Body.
            _ = buffer.Append(this.DataObject);
            return buffer.ToString();
        }

        /**
          <summary>Removes the <see cref="DataObject">data object</see> from its object stream [PDF:1.6:3.4.6].</summary>
        */
        public void Uncompress(
          )
        {
            if (!this.IsCompressed())
            {
                return;
            }

            // Remove from its object stream!
            var oldObjectStream = (ObjectStream)this.file.IndirectObjects[this.xrefEntry.StreamNumber].DataObject;
            _ = oldObjectStream.Remove(this.xrefEntry.Number);
            // Update its xref entry!
            this.xrefEntry.Usage = XRefEntry.UsageEnum.InUse;
            this.xrefEntry.StreamNumber = XRefEntry.UndefinedStreamNumber; // No object stream.
            this.xrefEntry.Offset = XRefEntry.UndefinedOffset; // Offset unknown (to set on file serialization -- see CompressedWriter).
        }

        public override void WriteTo(
          IOutputStream stream,
          File context
          )
        {
            // Header.
            stream.Write(this.reference.Id);
            stream.Write(BeginIndirectObjectChunk);
            // Body.
            this.DataObject.WriteTo(stream, context);
            // Tail.
            stream.Write(EndIndirectObjectChunk);
        }

        public override PdfIndirectObject Container => this;

        public PdfDataObject DataObject
        {
            get
            {
                if (this.dataObject == null)
                {
                    switch (this.xrefEntry.Usage)
                    {
                        // Free entry (no data object at all).
                        case XRefEntry.UsageEnum.Free:
                            break;
                        // In-use entry (late-bound data object).
                        case XRefEntry.UsageEnum.InUse:
                            // Get the indirect data object!
                            this.dataObject = this.Include(this.file.Reader.Parser.ParsePdfObject(this.xrefEntry));
                            break;
                        case XRefEntry.UsageEnum.InUseCompressed:
                            // Get the object stream where its data object is stored!
                            var objectStream = (ObjectStream)this.file.IndirectObjects[this.xrefEntry.StreamNumber].DataObject;
                            // Get the indirect data object!
                            this.dataObject = this.Include(objectStream[this.xrefEntry.Number]);
                            break;
                    }
                }
                return this.dataObject;
            }
            set
            {
                if (this.xrefEntry.Generation == XRefEntry.GenerationUnreusable)
                {
                    throw new Exception("Unreusable entry.");
                }

                this.Exclude(this.dataObject);
                this.dataObject = this.Include(value);
                this.xrefEntry.Usage = XRefEntry.UsageEnum.InUse;
                this.Update();
            }
        }

        public override File File => this.file;

        public override PdfIndirectObject IndirectObject => this;

        public override PdfObject Parent
        {
            get => null;  // NOTE: As indirect objects are root objects, no parent can be associated.
            internal set
            {/* NOOP: As indirect objects are root objects, no parent can be associated. */}
        }

        public override PdfReference Reference => this.reference;

        public override bool Updateable
        {
            get => this.updateable;
            set => this.updateable = value;
        }

        public override bool Updated
        {
            get => this.updated;
            protected internal set
            {
                if (value && this.original)
                {
                    /*
                      NOTE: It's expected that DropOriginal() is invoked by IndirectObjects indexer;
                      such an action is delegated because clients may invoke directly the indexer skipping
                      this method.
                    */
                    _ = this.file.IndirectObjects.Update(this);
                }
                this.updated = value;
            }
        }

        public XRefEntry XrefEntry => this.xrefEntry;
    }
}