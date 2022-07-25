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

namespace org.pdfclown.files
{
    using System;
    using System.Collections;

    using System.Collections.Generic;
    using System.Linq;
    using org.pdfclown.objects;
    using org.pdfclown.tokens;

    /**
      <summary>Collection of the <b>alive indirect objects</b> available inside the
      file.</summary>
      <remarks>
        <para>According to the PDF spec, <i>indirect object entries may be free
        (no data object associated) or in-use (data object associated)</i>.</para>
        <para>We can effectively subdivide indirect objects in two possibly-overriding
        collections: the <b>original indirect objects</b> (coming from the associated
        preexisting file) and the <b>newly-registered indirect objects</b> (coming
        from new data objects or original indirect objects manipulated during the
        current session).</para>
        <para><i>To ensure that the modifications applied to an original indirect object
        are committed to being persistent</i> is critical that the modified original
        indirect object is newly-registered (practically overriding the original
        indirect object).</para>
        <para><b>Alive indirect objects</b> encompass all the newly-registered ones plus
        not-overridden original ones.</para>
      </remarks>
    */
    public sealed class IndirectObjects
      : IList<PdfIndirectObject>
    {
        /**
<summary>Associated file.</summary>
*/
        private readonly File file;

        /**
          <summary>Map of matching references of imported indirect objects.</summary>
          <remarks>
            <para>This collection is used to prevent duplications among imported
            indirect objects.</para>
            <para><code>Key</code> is the external indirect object hashcode, <code>Value</code> is the
            matching internal indirect object.</para>
          </remarks>
        */
        private readonly Dictionary<int, PdfIndirectObject> importedObjects = new Dictionary<int, PdfIndirectObject>();

        /**
          <summary>Object counter.</summary>
        */
        private int lastObjectNumber;
        /**
          <summary>Collection of newly-registered indirect objects.</summary>
        */
        private readonly SortedDictionary<int, PdfIndirectObject> modifiedObjects = new SortedDictionary<int, PdfIndirectObject>();
        /**
          <summary>Collection of instantiated original indirect objects.</summary>
          <remarks>This collection is used as a cache to avoid unconsistent parsing duplications.</remarks>
        */
        private readonly SortedDictionary<int, PdfIndirectObject> wokenObjects = new SortedDictionary<int, PdfIndirectObject>();
        /**
          <summary>Offsets of the original indirect objects inside the associated file
          (to say: implicit collection of the original indirect objects).</summary>
          <remarks>This information is vital to randomly retrieve the indirect-object
          persistent representation inside the associated file.</remarks>
        */
        private readonly SortedDictionary<int, XRefEntry> xrefEntries;

        internal IndirectObjects(
  File file,
  SortedDictionary<int, XRefEntry> xrefEntries
  )
        {
            this.file = file;
            this.xrefEntries = xrefEntries;
            if (this.xrefEntries == null) // No original indirect objects.
            {
                // Register the leading free-object!
                /*
                  NOTE: Mandatory head of the linked list of free objects
                  at object number 0 [PDF:1.6:3.4.3].
                */
                this.lastObjectNumber = 0;
                this.modifiedObjects[this.lastObjectNumber] = new PdfIndirectObject(
                  this.file,
                  null,
                  new XRefEntry(
                    this.lastObjectNumber,
                    XRefEntry.GenerationUnreusable,
                    0,
                    XRefEntry.UsageEnum.Free
                    )
                  );
            }
            else
            {
                // Adjust the object counter!
                this.lastObjectNumber = xrefEntries.Keys.Last();
            }
        }

        public PdfIndirectObject this[
          int index
          ]
        {
            get
            {
                if ((index < 0) || (index >= this.Count))
                {
                    /*
     NOTE: An indirect reference to an undefined object is not an error; it is simply treated
     as a reference to the null object [PDF:1.7:3.2.9] [FIX:59].
   */
                    return null;
                }

                PdfIndirectObject obj;
                if (!this.modifiedObjects.TryGetValue(index, out obj))
                {
                    if (!this.wokenObjects.TryGetValue(index, out obj))
                    {
                        XRefEntry xrefEntry;
                        if (!this.xrefEntries.TryGetValue(index, out xrefEntry))
                        {
                            /*
                              NOTE: The cross-reference table (comprising the original cross-reference section and
                              all update sections) MUST contain one entry for each object number from 0 to the
                              maximum object number used in the file, even if one or more of the object numbers in
                              this range do not actually occur in the file. However, for resilience purposes
                              missing entries are treated as free ones.
                            */
                            this.xrefEntries[index] = xrefEntry = new XRefEntry(
                              index,
                              XRefEntry.GenerationUnreusable,
                              0,
                              XRefEntry.UsageEnum.Free
                              );
                        }

                        // Awake the object!
                        /*
                          NOTE: This operation allows to keep a consistent state across the whole session,
                          avoiding multiple incoherent instantiations of the same original indirect object.
                        */
                        this.wokenObjects[index] = obj = new PdfIndirectObject(this.file, null, xrefEntry);
                    }
                }
                return obj;
            }
            set => throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return this.GetEnumerator(); }

        internal PdfIndirectObject AddVirtual(
  PdfIndirectObject obj
  )
        {
            // Update the reference of the object!
            var xref = obj.XrefEntry;
            xref.Number = ++this.lastObjectNumber;
            xref.Generation = 0;
            // Register the object!
            this.modifiedObjects[this.lastObjectNumber] = obj;
            return obj;
        }

        internal PdfIndirectObject Update(
          PdfIndirectObject obj
          )
        {
            var index = obj.Reference.ObjectNumber;

            // Get the old indirect object to be replaced!
            var old = this[index];
            if (old != obj)
            { old.DropFile(); } // Disconnect the old indirect object.

            // Insert the new indirect object into the modified objects collection!
            this.modifiedObjects[index] = obj;
            // Remove old indirect object from cache!
            _ = this.wokenObjects.Remove(index);
            // Mark the new indirect object as modified!
            obj.DropOriginal();

            return old;
        }

        internal SortedDictionary<int, PdfIndirectObject> ModifiedObjects => this.modifiedObjects;

        /**
<summary>Registers an <i>internal</i> data object.</summary>
<remarks>To register an external indirect object, use <see
cref="AddExternal(PdfIndirectObject)"/>.</remarks>
<returns>Indirect object corresponding to the registered data object.</returns>
*/
        public PdfIndirectObject Add(
          PdfDataObject obj
          )
        {
            // Register a new indirect object wrapping the data object inside!
            var indirectObject = new PdfIndirectObject(
              this.file,
              obj,
              new XRefEntry(++this.lastObjectNumber, 0)
              );
            this.modifiedObjects[this.lastObjectNumber] = indirectObject;
            return indirectObject;
        }

        /**
  <summary>Registers an <i>external</i> indirect object.</summary>
  <remarks>
    <para>External indirect objects come from alien PDF files; therefore, this is a powerful way
    to import contents from a file into another one.</para>
    <para>To register and get an external indirect object, use <see
    cref="AddExternal(PdfIndirectObject)"/></para>
  </remarks>
  <returns>Whether the indirect object was successfully registered.</returns>
*/
        public void Add(
          PdfIndirectObject obj
          )
        { _ = this.AddExternal(obj); }

        /**
          <summary>Registers an <i>external</i> indirect object.</summary>
          <remarks>
            <para>External indirect objects come from alien PDF files; therefore, this is a powerful way
            to import contents from a file into another one.</para>
            <para>To register an internal data object, use <see cref="Add(PdfDataObject)"/>.</para>
          </remarks>
          <param nme="obj">External indirect object to import.</param>
          <returns>Indirect object imported from the external indirect object.</returns>
        */
        public PdfIndirectObject AddExternal(
          PdfIndirectObject obj
          )
        { return this.AddExternal(obj, this.file.Cloner); }

        /**
          <summary>Registers an <i>external</i> indirect object.</summary>
          <remarks>
            <para>External indirect objects come from alien PDF files; therefore, this is a powerful way
            to import contents from a file into another one.</para>
            <para>To register an internal data object, use <see cref="Add(PdfDataObject)"/>.</para>
          </remarks>
          <param nme="obj">External indirect object to import.</param>
          <param nme="cloner">Import rules.</param>
          <returns>Indirect object imported from the external indirect object.</returns>
        */
        public PdfIndirectObject AddExternal(
          PdfIndirectObject obj,
          Cloner cloner
          )
        {
            if (cloner.Context != this.file)
            {
                throw new ArgumentException("cloner file context incompatible");
            }

            PdfIndirectObject indirectObject;
            // Hasn't the external indirect object been imported yet?
            if (!this.importedObjects.TryGetValue(obj.GetHashCode(), out indirectObject))
            {
                // Keep track of the imported indirect object!
                this.importedObjects.Add(
                  obj.GetHashCode(),
                  indirectObject = this.Add((PdfDataObject)null) // [DEV:AP] Circular reference issue solved.
                  );
                indirectObject.DataObject = (PdfDataObject)obj.DataObject.Accept(cloner, null);
            }
            return indirectObject;
        }

        public void Clear(
          )
        {
            for (int index = 0, length = this.Count; index < length; index++)
            { this.RemoveAt(index); }
        }

        public bool Contains(
          PdfIndirectObject obj
          )
        { return (obj != null) && (this[obj.Reference.ObjectNumber] == obj); }

        public void CopyTo(
          PdfIndirectObject[] objs,
          int index
          )
        { throw new NotSupportedException(); }

        public IEnumerator<PdfIndirectObject> GetEnumerator(
  )
        {
            for (var index = 0; index < this.Count; index++)
            { yield return this[index]; }
        }

        public int IndexOf(
  PdfIndirectObject obj
  )
        {
            // Is this indirect object associated to this file?
            if (obj.File != this.file)
            {
                return -1;
            }

            return obj.Reference.ObjectNumber;
        }

        public void Insert(
          int index,
          PdfIndirectObject obj
          )
        { throw new NotSupportedException(); }

        public bool IsEmpty(
          )
        {
            /*
              NOTE: Indirect objects' semantics imply that the collection is considered empty when no
              in-use object is available.
            */
            foreach (var obj in this)
            {
                if (obj.IsInUse())
                {
                    return false;
                }
            }
            return true;
        }

        public bool Remove(
          PdfIndirectObject obj
          )
        {
            if (!this.Contains(obj))
            {
                return false;
            }

            this.RemoveAt(obj.Reference.ObjectNumber);
            return true;
        }

        public void RemoveAt(
          int index
          )
        {
            if (this[index].IsInUse())
            {
                /*
                  NOTE: Acrobat 6.0 and later (PDF 1.5+) DO NOT use the free list to recycle object numbers;
                  new objects are assigned new numbers [PDF:1.6:H.3:16].
                  According to such an implementation note, we simply mark the removed object as 'not-reusable'
                  newly-freed entry, neglecting both to add it to the linked list of free entries and to
                  increment by 1 its generation number.
                */
                _ = this.Update(
                  new PdfIndirectObject(
                    this.file,
                    null,
                    new XRefEntry(
                      index,
                      XRefEntry.GenerationUnreusable,
                      0,
                      XRefEntry.UsageEnum.Free
                      )
                    )
                  );
            }
        }

        /**
          <summary>Gets the number of entries available (both in-use and free) in the
          collection.</summary>
          <returns>The number of entries available in the collection.</returns>
        */
        public int Count => this.lastObjectNumber + 1;

        /**
          <summary>Gets the file associated to this collection.</summary>
        */
        public File File => this.file;

        public bool IsReadOnly => false;
    }
}