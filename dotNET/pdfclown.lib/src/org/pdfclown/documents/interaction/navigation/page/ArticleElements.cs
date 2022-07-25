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

namespace org.pdfclown.documents.interaction.navigation.page
{
    using System;
    using System.Collections;

    using System.Collections.Generic;
    using org.pdfclown.objects;
    using org.pdfclown.util;

    /**
      <summary>Article bead [PDF:1.7:8.3.2].</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public sealed class ArticleElements
      : PdfObjectWrapper<PdfDictionary>,
        IList<ArticleElement>
    {

        private ArticleElements(
PdfDirectObject baseObject
) : base(baseObject)
        { }

        public ArticleElement this[
          int index
          ]
        {
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var getter = new ElementGetter(index);
                this.Iterate(getter);
                var bead = getter.Bead;
                if (bead == null)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return ArticleElement.Wrap(bead.Reference);
            }
            set => throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return this.GetEnumerator(); }

        private void Iterate(
          IPredicate predicate
          )
        {
            var firstBead = this.FirstBead;
            var bead = firstBead;
            while (bead != null)
            {
                if (predicate.Evaluate(bead))
                {
                    break;
                }

                bead = (PdfDictionary)bead.Resolve(PdfName.N);
                if (bead == firstBead)
                {
                    break;
                }
            }
        }

        /**
          <summary>Links the given item.</summary>
        */
        private void Link(
          PdfDictionary item,
          PdfDictionary next
          )
        {
            var previous = (PdfDictionary)next.Resolve(PdfName.V);
            if (previous == null)
            { previous = next; }

            item[PdfName.N] = next.Reference;
            next[PdfName.V] = item.Reference;
            if (previous != item)
            {
                item[PdfName.V] = previous.Reference;
                previous[PdfName.N] = item.Reference;
            }
        }

        /**
          <summary>Unlinks the given item.</summary>
          <remarks>It assumes the item is contained in this list.</remarks>
        */
        private void Unlink(
          PdfDictionary item
          )
        {
            var prevBead = (PdfDictionary)item.Resolve(PdfName.V);
            _ = item.Remove(PdfName.V);
            var nextBead = (PdfDictionary)item.Resolve(PdfName.N);
            _ = item.Remove(PdfName.N);
            if (prevBead != item) // Still some elements.
            {
                prevBead[PdfName.N] = nextBead.Reference;
                nextBead[PdfName.V] = prevBead.Reference;
                if (item == this.FirstBead)
                { this.FirstBead = nextBead; }
            }
            else // No more elements.
            { this.FirstBead = null; }
        }

        private PdfDictionary FirstBead
        {
            get => (PdfDictionary)this.BaseDataObject.Resolve(PdfName.F);
            set
            {
                var oldValue = this.FirstBead;
                this.BaseDataObject[PdfName.F] = PdfObject.Unresolve(value);
                if (value != null)
                { value[PdfName.T] = this.BaseObject; }
                if (oldValue != null)
                { _ = oldValue.Remove(PdfName.T); }
            }
        }

        public void Add(
  ArticleElement @object
  )
        {
            var itemBead = @object.BaseDataObject;
            var firstBead = this.FirstBead;
            if (firstBead != null) // Non-empty list.
            { this.Link(itemBead, firstBead); }
            else // Empty list.
            {
                this.FirstBead = itemBead;
                this.Link(itemBead, itemBead);
            }
        }

        public void Clear(
          )
        { throw new NotImplementedException(); }

        public bool Contains(
          ArticleElement @object
          )
        { return this.IndexOf(@object) >= 0; }

        public void CopyTo(
          ArticleElement[] objects,
          int index
          )
        { throw new NotImplementedException(); }

        public IEnumerator<ArticleElement> GetEnumerator(
  )
        { return new Enumerator(this); }

        public int IndexOf(
ArticleElement @object
)
        {
            if (@object == null)
            {
                return -1; // NOTE: By definition, no bead can be null.
            }

            var indexer = new ElementIndexer(@object.BaseDataObject);
            this.Iterate(indexer);
            return indexer.Index;
        }

        public void Insert(
          int index,
          ArticleElement @object
          )
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var getter = new ElementGetter(index);
            this.Iterate(getter);
            var bead = getter.Bead;
            if (bead == null)
            { this.Add(@object); }
            else
            { this.Link(@object.BaseDataObject, bead); }
        }

        public bool Remove(
          ArticleElement @object
          )
        {
            if (!this.Contains(@object))
            {
                return false;
            }

            this.Unlink(@object.BaseDataObject);
            return true;
        }

        public void RemoveAt(
          int index
          )
        { this.Unlink(this[index].BaseDataObject); }

        public static ArticleElements Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new ArticleElements(baseObject) : null; }

        public int Count
        {
            get
            {
                var counter = new ElementCounter();
                this.Iterate(counter);
                return counter.Count;
            }
        }

        public bool IsReadOnly => false;

        private sealed class ElementCounter
  : ElementEvaluator
        {
            public int Count => this.index + 1;
        }

        private class ElementEvaluator
          : IPredicate
        {
            /**
              Current position.
            */
            protected int index = -1;

            public virtual bool Evaluate(
              object @object
              )
            {
                this.index++;
                return false;
            }
        }

        private sealed class ElementGetter
          : ElementEvaluator
        {
            private PdfDictionary bead;
            private readonly int beadIndex;

            public ElementGetter(
              int beadIndex
              )
            { this.beadIndex = beadIndex; }

            public override bool Evaluate(
              object @object
              )
            {
                _ = base.Evaluate(@object);
                if (this.index == this.beadIndex)
                {
                    this.bead = (PdfDictionary)@object;
                    return true;
                }
                return false;
            }

            public PdfDictionary Bead => this.bead;
        }

        private sealed class ElementIndexer
          : ElementEvaluator
        {
            private readonly PdfDictionary searchedBead;

            public ElementIndexer(
              PdfDictionary searchedBead
              )
            { this.searchedBead = searchedBead; }

            public override bool Evaluate(
              object @object
              )
            {
                _ = base.Evaluate(@object);
                return @object.Equals(this.searchedBead);
            }

            public int Index => this.index;
        }

        private class Enumerator
          : IEnumerator<ArticleElement>
        {
            private PdfDirectObject currentObject;
            private readonly PdfDirectObject firstObject;
            private PdfDirectObject nextObject;

            internal Enumerator(
              ArticleElements elements
              )
            { this.nextObject = this.firstObject = elements.BaseDataObject[PdfName.F]; }

            ArticleElement IEnumerator<ArticleElement>.Current => ArticleElement.Wrap(this.currentObject);

            public void Dispose(
              )
            { }

            public bool MoveNext(
              )
            {
                if (this.nextObject == null)
                {
                    return false;
                }

                this.currentObject = this.nextObject;
                this.nextObject = ((PdfDictionary)this.currentObject.Resolve())[PdfName.N];
                if (this.nextObject == this.firstObject) // Looping back.
                { this.nextObject = null; }
                return true;
            }

            public void Reset(
              )
            { throw new NotSupportedException(); }

            public object Current => ((IEnumerator<ArticleElement>)this).Current;
        }
    }
}