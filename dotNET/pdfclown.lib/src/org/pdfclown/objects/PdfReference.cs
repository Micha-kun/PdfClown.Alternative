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
    using org.pdfclown.bytes;
    using org.pdfclown.files;

    using org.pdfclown.tokens;

    /**
      <summary>PDF indirect reference object [PDF:1.6:3.2.9].</summary>
    */
    public sealed class PdfReference
      : PdfDirectObject,
        IPdfIndirectObject
    {
        private const int DelegatedReferenceNumber = -1;

        private readonly File file;

        private readonly int generationNumber;

        private PdfIndirectObject indirectObject;
        private readonly int objectNumber;
        private PdfObject parent;
        private bool updated;

        internal PdfReference(
  PdfIndirectObject indirectObject
  )
        {
            this.objectNumber = DelegatedReferenceNumber;
            this.generationNumber = DelegatedReferenceNumber;

            this.indirectObject = indirectObject;
        }

        internal PdfReference(
          int objectNumber,
          int generationNumber,
          File file
          )
        {
            this.objectNumber = objectNumber;
            this.generationNumber = generationNumber;

            this.file = file;
        }

        protected internal override bool Virtual
        {
            get => (this.IndirectObject != null) && this.indirectObject.Virtual;
            set =>
                /*
NOTE: Fail fast if the referenced indirect object is undefined.
*/
                this.IndirectObject.Virtual = value;
        }

        public override PdfObject Accept(
IVisitor visitor,
object data
)
        { return visitor.Visit(this, data); }

        public override int CompareTo(
          PdfDirectObject obj
          )
        { throw new NotImplementedException(); }

        public override bool Equals(
          object other
          )
        {
            /*
             * NOTE: References are evaluated as "equal" if they are either the same instance or they sport
             * the same identifier within the same file instance.
             */
            if (base.Equals(other))
            {
                return true;
            }
            else if ((other == null)
                || !other.GetType().Equals(this.GetType()))
            {
                return false;
            }

            var otherReference = (PdfReference)other;
            return (otherReference.File == this.File)
                && otherReference.Id.Equals(this.Id);
        }

        public override int GetHashCode(
          )
        {
            /*
              NOTE: Uniqueness should be achieved XORring the (local) reference hash-code with the (global)
              file hash-code.
            */
            return this.Id.GetHashCode() ^ this.File.GetHashCode();
        }

        public override PdfObject Swap(
          PdfObject other
          )
        {
            /*
              NOTE: Fail fast if the referenced indirect object is undefined.
            */
            return this.IndirectObject.Swap(((PdfReference)other).IndirectObject).Reference;
        }

        public override string ToString(
          )
        { return this.IndirectReference; }

        public override void WriteTo(
          IOutputStream stream,
          File context
          )
        { stream.Write(this.IndirectReference); }

        public PdfDataObject DataObject
        {
            get => (this.IndirectObject != null) ? this.indirectObject.DataObject : null;
            set =>
                /*
NOTE: Fail fast if the referenced indirect object is undefined.
*/
                this.IndirectObject.DataObject = value;
        }

        public override File File => (this.file != null) ? this.file : base.File;

        /**
          <summary>Gets the generation number.</summary>
        */
        public int GenerationNumber => (this.generationNumber == DelegatedReferenceNumber) ? this.IndirectObject.XrefEntry.Generation : this.generationNumber;

        /**
          <summary>Gets the object identifier.</summary>
          <remarks>This corresponds to the serialized representation of an object identifier within a PDF file.</remarks>
        */
        public string Id => $"{this.ObjectNumber}{Symbol.Space}{this.GenerationNumber}";

        /**
          <returns><code>null</code>, if the indirect object is undefined.</returns>
        */
        public override PdfIndirectObject IndirectObject
        {
            get
            {
                if (this.indirectObject == null)
                { this.indirectObject = this.file.IndirectObjects[this.objectNumber]; }

                return this.indirectObject;
            }
        }

        /**
          <summary>Gets the object reference.</summary>
          <remarks>This corresponds to the serialized representation of a reference within a PDF file.</remarks>
        */
        public string IndirectReference => $"{this.Id}{Symbol.Space}{Symbol.CapitalR}";

        /**
          <summary>Gets the object number.</summary>
        */
        public int ObjectNumber => (this.objectNumber == DelegatedReferenceNumber) ? this.IndirectObject.XrefEntry.Number : this.objectNumber;

        public override PdfObject Parent
        {
            get => this.parent;
            internal set => this.parent = value;
        }

        public override PdfReference Reference => this;

        public override bool Updateable
        {
            get => (this.IndirectObject != null) && this.indirectObject.Updateable;
            set =>
                /*
NOTE: Fail fast if the referenced indirect object is undefined.
*/
                this.IndirectObject.Updateable = value;
        }

        public override bool Updated
        {
            get => this.updated;
            protected internal set => this.updated = value;
        }
    }
}