/*
  Copyright 2011-2015 Stefano Chizzolini. http://www.pdfclown.org

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


namespace org.pdfclown.documents.contents.layers
{
    using System;

    using System.Collections;
    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Optional content membership [PDF:1.7:4.10.1].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public sealed class LayerMembership
      : LayerEntity
    {

        public static PdfName TypeName = PdfName.OCMD;

        private LayerMembership(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public LayerMembership(
Document context
) : base(context, TypeName)
        { }

        public static new LayerMembership Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new LayerMembership(baseObject) : null; }

        public override LayerEntity Membership => this;

        public override VisibilityExpression VisibilityExpression
        {
            get => VisibilityExpression.Wrap(this.BaseDataObject[PdfName.VE]);
            set => this.BaseDataObject[PdfName.VE] = PdfObjectWrapper.GetBaseObject(value);
        }

        public override IList<Layer> VisibilityMembers
        {
            get => new VisibilityMembersImpl(this);
            set
            {
                var visibilityMembers = this.VisibilityMembers;
                visibilityMembers.Clear();
                foreach (var layer in value)
                { visibilityMembers.Add(layer); }
            }
        }

        public override VisibilityPolicyEnum VisibilityPolicy
        {
            get => VisibilityPolicyEnumExtension.Get((PdfName)this.BaseDataObject[PdfName.P]);
            set => this.BaseDataObject[PdfName.P] = value.GetName();
        }
        /**
  <summary>Layers whose states determine the visibility of content controlled by a membership.</summary>
*/
        private class VisibilityMembersImpl
          : PdfObjectWrapper<PdfDirectObject>,
            IList<Layer>
        {
            private readonly LayerMembership membership;

            internal VisibilityMembersImpl(
              LayerMembership membership
              ) : base(membership.BaseDataObject[PdfName.OCGs])
            { this.membership = membership; }

            public Layer this[
              int index
              ]
            {
                get
                {
                    PdfDataObject baseDataObject = this.BaseDataObject;
                    if (baseDataObject == null) // No layer.
                    {
                        return null;
                    }
                    else if (baseDataObject is PdfDictionary) // Single layer.
                    {
                        if (index != 0)
                        {
                            throw new IndexOutOfRangeException();
                        }

                        return Layer.Wrap(this.BaseObject);
                    }
                    else // Multiple layers.
                    {
                        return Layer.Wrap(((PdfArray)baseDataObject)[index]);
                    }
                }
                set => this.EnsureArray()[index] = value.BaseObject;
            }

            IEnumerator IEnumerable.GetEnumerator(
              )
            { return this.GetEnumerator(); }

            private PdfArray EnsureArray(
              )
            {
                var baseDataObject = this.BaseDataObject;
                if (!(baseDataObject is PdfArray))
                {
                    var array = new PdfArray();
                    if (baseDataObject != null)
                    { array.Add(baseDataObject); }
                    this.BaseObject = baseDataObject = array;
                    this.membership.BaseDataObject[PdfName.OCGs] = this.BaseObject;
                }
                return (PdfArray)baseDataObject;
            }

            public void Add(
              Layer item
              )
            { this.EnsureArray().Add(item.BaseObject); }

            public void Clear(
              )
            { this.EnsureArray().Clear(); }

            public bool Contains(
              Layer item
              )
            {
                PdfDataObject baseDataObject = this.BaseDataObject;
                if (baseDataObject == null) // No layer.
                {
                    return false;
                }
                else if (baseDataObject is PdfDictionary) // Single layer.
                {
                    return item.BaseObject.Equals(this.BaseObject);
                }
                else // Multiple layers.
                {
                    return ((PdfArray)baseDataObject).Contains(item.BaseObject);
                }
            }

            public void CopyTo(
              Layer[] items,
              int index
              )
            { throw new NotImplementedException(); }

            public IEnumerator<Layer> GetEnumerator(
              )
            {
                for (int index = 0, length = this.Count; index < length; index++)
                { yield return this[index]; }
            }

            public int IndexOf(
              Layer item
              )
            {
                PdfDataObject baseDataObject = this.BaseDataObject;
                if (baseDataObject == null) // No layer.
                {
                    return -1;
                }
                else if (baseDataObject is PdfDictionary) // Single layer.
                {
                    return item.BaseObject.Equals(this.BaseObject) ? 0 : (-1);
                }
                else // Multiple layers.
                {
                    return ((PdfArray)baseDataObject).IndexOf(item.BaseObject);
                }
            }

            public void Insert(
              int index,
              Layer item
              )
            { this.EnsureArray().Insert(index, item.BaseObject); }

            public bool Remove(
              Layer item
              )
            { return this.EnsureArray().Remove(item.BaseObject); }

            public void RemoveAt(
              int index
              )
            { this.EnsureArray().RemoveAt(index); }

            public int Count
            {
                get
                {
                    PdfDataObject baseDataObject = this.BaseDataObject;
                    if (baseDataObject == null) // No layer.
                    {
                        return 0;
                    }
                    else if (baseDataObject is PdfDictionary) // Single layer.
                    {
                        return 1;
                    }
                    else // Multiple layers.
                    {
                        return ((PdfArray)baseDataObject).Count;
                    }
                }
            }

            public bool IsReadOnly => false;
        }
    }
}
