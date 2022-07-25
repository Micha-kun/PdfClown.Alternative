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


namespace org.pdfclown.tools
{
    using System.Collections.Generic;
    using System.Drawing;
    using org.pdfclown.bytes;
    using org.pdfclown.documents;
    using org.pdfclown.documents.contents;
    using org.pdfclown.documents.contents.objects;

    using org.pdfclown.files;
    using org.pdfclown.objects;

    /**
      <summary>Tool for page management.</summary>
    */
    public sealed class PageManager
    {

        private Document document;
        private Pages pages;

        public PageManager(
  ) : this(null)
        { }

        public PageManager(
          Document document
          )
        { this.Document = document; }

        /**
  <summary>Gets the data size of the specified object expressed in bytes.</summary>
  <param name="object">Data object whose size has to be calculated.</param>
  <param name="visitedReferences">References to data objects excluded from calculation.
    This set is useful, for example, to avoid recalculating the data size of shared resources.
    During the operation, this set is populated with references to visited data objects.</param>
  <param name="isRoot">Whether this data object represents the page root.</param>
*/
        private static long GetSize(
          PdfDirectObject obj,
          HashSet<PdfReference> visitedReferences,
          bool isRoot
          )
        {
            long dataSize = 0;
            var dataObject = PdfObject.Resolve(obj);

            // 1. Evaluating the current object...
            if (obj is PdfReference)
            {
                var reference = (PdfReference)obj;
                if (visitedReferences.Contains(reference))
                {
                    return 0; // Avoids circular references.
                }

                if ((dataObject is PdfDictionary)
                  && PdfName.Page.Equals(((PdfDictionary)dataObject)[PdfName.Type])
                  && !isRoot)
                {
                    return 0; // Avoids references to other pages.
                }

                _ = visitedReferences.Add(reference);

                // Calculate the data size of the current object!
                IOutputStream buffer = new Buffer();
                reference.IndirectObject.WriteTo(buffer, reference.File);
                dataSize += buffer.Length;
            }

            // 2. Evaluating the current object's children...
            ICollection<PdfDirectObject> values = null;
            if (dataObject is PdfStream)
            { dataObject = ((PdfStream)dataObject).Header; }
            if (dataObject is PdfDictionary)
            { values = ((PdfDictionary)dataObject).Values; }
            else if (dataObject is PdfArray)
            { values = (PdfArray)dataObject; }
            if (values != null)
            {
                // Calculate the data size of the current object's children!
                foreach (var value in values)
                { dataSize += GetSize(value, visitedReferences, false); }
            }
            return dataSize;
        }

        /**
          <summary>Gets whether the specified content stream part is blank.</summary>
          <param name="level">Content stream part to evaluate.</param>
          <param name="contentBox">Area to evaluate within the page.</param>
        */
        private static bool IsBlank(
          ContentScanner level,
          RectangleF contentBox
          )
        {
            if (level == null)
            {
                return true;
            }

            while (level.MoveNext())
            {
                var content = level.Current;
                if (content is ContainerObject)
                {
                    // Scan the inner level!
                    if (!IsBlank(level.ChildLevel, contentBox))
                    {
                        return false;
                    }
                }
                else
                {
                    var contentWrapper = level.CurrentWrapper;
                    if (contentWrapper == null)
                    {
                        continue;
                    }

                    if (contentWrapper.Box.Value.IntersectsWith(contentBox))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /**
<summary>Appends a document to the end of the document.</summary>
<param name="document">Document to be added.</param>
*/
        public void Add(
          Document document
          )
        { this.Add(document.Pages); }

        /**
          <summary>Appends a collection of pages to the end of the document.</summary>
          <param name="pages">Pages to be added.</param>
        */
        public void Add(
          ICollection<Page> pages
          )
        {
            // Add the source pages to the document (deep level)!
            var importedPages = this.document.Include(pages); // NOTE: Alien pages MUST be contextualized (i.e. imported).

            // Add the imported pages to the pages collection (shallow level)!
            this.pages.AddAll(importedPages);
        }

        /**
          <summary>Inserts a document at the specified position in the document.</summary>
          <param name="index">Position at which the document has to be inserted.</param>
          <param name="document">Document to be inserted.</param>
        */
        public void Add(
          int index,
          Document document
          )
        { this.Add(index, document.Pages); }

        /**
          <summary>Inserts a collection of pages at the specified position in the document.</summary>
          <param name="index">Position at which the pages have to be inserted.</param>
          <param name="pages">Pages to be inserted.</param>
        */
        public void Add(
          int index,
          ICollection<Page> pages
          )
        {
            // Add the source pages to the document (deep level)!
            var importedPages = this.document.Include(pages); // NOTE: Alien pages MUST be contextualized (i.e. imported).

            // Add the imported pages to the pages collection (shallow level)!
            if (index >= this.pages.Count)
            { this.pages.AddAll(importedPages); }
            else
            { this.pages.InsertAll(index, importedPages); }
        }

        /**
          <summary>Extracts a page range from the document.</summary>
          <param name="startIndex">The beginning index, inclusive.</param>
          <param name="endIndex">The ending index, exclusive.</param>
          <returns>Extracted page range.</returns>
        */
        public Document Extract(
          int startIndex,
          int endIndex
          )
        {
            var extractedDocument = new File().Document;
            // Add the pages to the target file!
            /*
              NOTE: To be added to an alien document,
              pages MUST be contextualized within it first,
              then added to the target pages collection.
            */
            extractedDocument.Pages.AddAll(
    extractedDocument.Include(
                this.pages.GetSlice(startIndex, endIndex)
                )
              );
            return extractedDocument;
        }
        /*
          NOTE: As you can read on the PDF Clown's User Guide, referential operations on high-level object such as pages
          can be done at two levels:
            1. shallow, involving page references but NOT their data within the document;
            2. deep, involving page data within the document.
          This means that, for example, if you remove a page reference (shallow level) from the pages collection,
          the data of that page (deep level) are still within the document!
        */

        /**
<summary>Gets the data size of the specified page expressed in bytes.</summary>
<param name="page">Page whose data size has to be calculated.</param>
*/
        public static long GetSize(
          Page page
          )
        { return GetSize(page, new HashSet<PdfReference>()); }

        /**
          <summary>Gets the data size of the specified page expressed in bytes.</summary>
          <param name="page">Page whose data size has to be calculated.</param>
          <param name="visitedReferences">References to data objects excluded from calculation.
            This set is useful, for example, to avoid recalculating the data size of shared resources.
            During the operation, this set is populated with references to visited data objects.</param>
        */
        public static long GetSize(
          Page page,
          HashSet<PdfReference> visitedReferences
          )
        { return GetSize(page.BaseObject, visitedReferences, true); }

        /**
          <summary>Gets whether the specified page is blank.</summary>
          <param name="page">Page to evaluate.</param>
        */
        public static bool IsBlank(
          Page page
          )
        { return IsBlank(page, page.Box); }

        /**
          <summary>Gets whether the specified page is blank.</summary>
          <param name="page">Page to evaluate.</param>
          <param name="contentBox">Area to evaluate within the page.</param>
        */
        public static bool IsBlank(
          Page page,
          RectangleF contentBox
          )
        { return IsBlank(new ContentScanner(page), contentBox); }

        /**
          <summary>Moves a page range to a target position within the document.</summary>
          <param name="startIndex">The beginning index, inclusive.</param>
          <param name="endIndex">The ending index, exclusive.</param>
          <param name="targetIndex">The target index.</param>
        */
        public void Move(
          int startIndex,
          int endIndex,
          int targetIndex
          )
        {
            var pageCount = this.pages.Count;

            var movingPages = this.pages.GetSlice(startIndex, endIndex);

            // Temporarily remove the pages from the pages collection!
            /*
              NOTE: Shallow removal (only page references are removed, as their data are kept in the document).
            */
            this.pages.RemoveAll(movingPages);

            // Adjust indexes!
            pageCount -= movingPages.Count;
            if (targetIndex > startIndex)
            { targetIndex -= movingPages.Count; } // Adjusts the target position due to shifting for temporary page removal.

            // Reinsert the pages at the target position!
            /*
              NOTE: Shallow addition (only page references are added, as their data are already in the document).
            */
            if (targetIndex >= pageCount)
            { this.pages.AddAll(movingPages); }
            else
            { this.pages.InsertAll(targetIndex, movingPages); }
        }

        /**
          <summary>Removes a page range from the document.</summary>
          <param name="startIndex">The beginning index, inclusive.</param>
          <param name="endIndex">The ending index, exclusive.</param>
        */
        public void Remove(
          int startIndex,
          int endIndex
          )
        {
            var removingPages = this.pages.GetSlice(startIndex, endIndex);

            // Remove the pages from the pages collection!
            /* NOTE: Shallow removal. */
            this.pages.RemoveAll(removingPages);

            // Remove the pages from the document (decontextualize)!
            /* NOTE: Deep removal. */
            this.document.Exclude(removingPages);
        }

        /**
          <summary>Bursts the document into single-page documents.</summary>
          <returns>Split subdocuments.</returns>
        */
        public IList<Document> Split(
          )
        {
            IList<Document> documents = new List<Document>();
            foreach (var page in this.pages)
            {
                var pageDocument = new File().Document;
                pageDocument.Pages.Add((Page)page.Clone(pageDocument));
                documents.Add(pageDocument);
            }
            return documents;
        }

        /**
          <summary>Splits the document into multiple subdocuments delimited by the specified page indexes.</summary>
          <param name="indexes">Split page indexes.</param>
          <returns>Split subdocuments.</returns>
        */
        public IList<Document> Split(
          params int[] indexes
          )
        {
            IList<Document> documents = new List<Document>();
            var startIndex = 0;
            foreach (var index in indexes)
            {
                documents.Add(this.Extract(startIndex, index));
                startIndex = index;
            }
            documents.Add(this.Extract(startIndex, this.pages.Count));
            return documents;
        }

        /**
          <summary>Splits the document into multiple subdocuments on maximum file size.</summary>
          <param name="maxDataSize">Maximum data size (expressed in bytes) of target files.
            Note that resulting files may be a little bit larger than this value, as file data include (along with actual page data)
            some extra structures such as cross reference tables.</param>
          <returns>Split documents.</returns>
        */
        public IList<Document> Split(
          long maxDataSize
          )
        {
            IList<Document> documents = new List<Document>();
            var startPageIndex = 0;
            long incrementalDataSize = 0;
            var visitedReferences = new HashSet<PdfReference>();
            foreach (var page in this.pages)
            {
                var pageDifferentialDataSize = GetSize(page, visitedReferences);
                incrementalDataSize += pageDifferentialDataSize;
                if (incrementalDataSize > maxDataSize) // Data size limit reached.
                {
                    var endPageIndex = page.Index;

                    // Split the current document page range!
                    documents.Add(this.Extract(startPageIndex, endPageIndex));

                    startPageIndex = endPageIndex;
                    incrementalDataSize = GetSize(page, visitedReferences = new HashSet<PdfReference>());
                }
            }
            // Split the last document page range!
            documents.Add(this.Extract(startPageIndex, this.pages.Count));
            return documents;
        }

        /**
          <summary>Gets/Sets the document being managed.</summary>
        */
        public Document Document
        {
            get => this.document;
            set
            {
                this.document = value;
                this.pages = this.document.Pages;
            }
        }
    }
}