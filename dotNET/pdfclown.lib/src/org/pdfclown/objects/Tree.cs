/*
  Copyright 2007-2015 Stefano Chizzolini. http://www.pdfclown.org

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
    using System.Collections;

    using System.Collections.Generic;
    using org.pdfclown.documents;

    /**
      <summary>Abstract tree [PDF:1.7:3.8.5].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public abstract class Tree<TKey, TValue>
      : PdfObjectWrapper<PdfDictionary>,
        IDictionary<TKey, TValue>
      where TKey : PdfDirectObject, IPdfSimpleObject
      where TValue : PdfObjectWrapper
    {

        /**
Minimum number of items in non-root nodes.
Note that the tree (high) order is assumed twice as much (<see cref="Children.Info.Info(int, int)"/>.
*/
        private static readonly int TreeLowOrder = 5;

        private PdfName pairsKey;

        protected Tree(
  Document context
  ) : base(context, new PdfDictionary())
        { this.Initialize(); }

        protected Tree(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { this.Initialize(); }

        public virtual TValue this[
          TKey key
          ]
        {
            get
            {
                var parent = this.BaseDataObject;
                while (true)
                {
                    var children = Children.Get(parent, this.pairsKey);
                    if (children.IsLeaf()) // Leaf node.
                    {
                        int low = 0, high = children.Items.Count - children.Info.ItemCount;
                        while (true)
                        {
                            if (low > high)
                            {
                                return null;
                            }

                            int mid = (mid = (low + high) / 2) - (mid % 2);
                            var comparison = key.CompareTo(children.Items[mid]);
                            if (comparison < 0)
                            { high = mid - 2; }
                            else if (comparison > 0)
                            { low = mid + 2; }
                            else
                            {
                                // We got it!
                                return this.WrapValue(children.Items[mid + 1]);
                            }
                        }
                    }
                    else // Intermediate node.
                    {
                        int low = 0, high = children.Items.Count - children.Info.ItemCount;
                        while (true)
                        {
                            if (low > high)
                            {
                                return null;
                            }

                            var mid = (low + high) / 2;
                            var kid = (PdfDictionary)children.Items.Resolve(mid);
                            var limits = (PdfArray)kid.Resolve(PdfName.Limits);
                            if (key.CompareTo(limits[0]) < 0)
                            { high = mid - 1; }
                            else if (key.CompareTo(limits[1]) > 0)
                            { low = mid + 1; }
                            else
                            {
                                // Go down one level!
                                parent = kid;
                                break;
                            }
                        }
                    }
                }
            }
            set => this.Add(key, value, true);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(
  KeyValuePair<TKey, TValue> keyValuePair
  )
        { this.Add(keyValuePair.Key, keyValuePair.Value); }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(
          KeyValuePair<TKey, TValue> keyValuePair
          )
        { return keyValuePair.Value.Equals(this[keyValuePair.Key]); }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return this.GetEnumerator(); }

        /**
  <summary>Adds an entry into the tree.</summary>
  <param name="key">New entry's key.</param>
  <param name="value">New entry's value.</param>
  <param name="overwrite">Whether the entry is allowed to replace an existing one having the same
  key.</param>
*/
        private void Add(
          TKey key,
          TValue value,
          bool overwrite
          )
        {
            // Get the root node!
            var root = this.BaseDataObject;

            // Ensuring the root node isn't full...

            var rootChildren = Children.Get(root, this.pairsKey);
            if (rootChildren.IsFull())
            {
                // Transfer the root contents into the new leaf!
                var leaf = (PdfDictionary)new PdfDictionary().Swap(root);
                var rootChildrenObject = new PdfArray(new PdfDirectObject[] { this.File.Register(leaf) });
                root[PdfName.Kids] = rootChildrenObject;
                // Split the leaf!
                this.SplitFullNode(
                  rootChildrenObject,
                  0, // Old root's position within new root's kids.
                  rootChildren.TypeName
                  );
            }

            // Set the entry under the root node!
            this.Add(key, value, overwrite, root);
        }

        /**
          <summary>Adds an entry under the given tree node.</summary>
          <param name="key">New entry's key.</param>
          <param name="value">New entry's value.</param>
          <param name="overwrite">Whether the entry is allowed to replace an existing one having the same
          key.</param>
          <param name="nodeReference">Current node reference.</param>
        */
        private void Add(
          TKey key,
          TValue value,
          bool overwrite,
          PdfDictionary node
          )
        {
            var children = Children.Get(node, this.pairsKey);
            if (children.IsLeaf()) // Leaf node.
            {
                var childrenSize = children.Items.Count;
                int low = 0, high = childrenSize - children.Info.ItemCount;
                while (true)
                {
                    if (low > high)
                    {
                        // Insert the entry!
                        children.Items.Insert(low, key);
                        children.Items.Insert(++low, value.BaseObject);
                        break;
                    }

                    int mid = (mid = (low + high) / 2) - (mid % 2);
                    if (mid >= childrenSize)
                    {
                        // Append the entry!
                        children.Items.Add(key);
                        children.Items.Add(value.BaseObject);
                        break;
                    }

                    var comparison = key.CompareTo(children.Items[mid]);
                    if (comparison < 0) // Before.
                    { high = mid - 2; }
                    else if (comparison > 0) // After.
                    { low = mid + 2; }
                    else // Matching entry.
                    {
                        if (!overwrite)
                        {
                            throw new ArgumentException($"Key '{key}' already exists.", nameof(key));
                        }

                        // Overwrite the entry!
                        children.Items[mid] = key;
                        children.Items[++mid] = value.BaseObject;
                        break;
                    }
                }

                // Update the key limits!
                this.UpdateNodeLimits(children);
            }
            else // Intermediate node.
            {
                int low = 0, high = children.Items.Count - children.Info.ItemCount;
                while (true)
                {
                    var matched = false;
                    var mid = (low + high) / 2;
                    var kidReference = (PdfReference)children.Items[mid];
                    var kid = (PdfDictionary)kidReference.DataObject;
                    var limits = (PdfArray)kid.Resolve(PdfName.Limits);
                    if (key.CompareTo(limits[0]) < 0) // Before the lower limit.
                    { high = mid - 1; }
                    else if (key.CompareTo(limits[1]) > 0) // After the upper limit.
                    { low = mid + 1; }
                    else // Limit range matched.
                    { matched = true; }

                    if (matched // Limit range matched.
                      || (low > high)) // No limit range match.
                    {
                        var kidChildren = Children.Get(kid, this.pairsKey);
                        if (kidChildren.IsFull())
                        {
                            // Split the node!
                            this.SplitFullNode(
                              children.Items,
                              mid,
                              kidChildren.TypeName
                              );
                            // Is the key before the split node?
                            if (key.CompareTo(((PdfArray)kid.Resolve(PdfName.Limits))[0]) < 0)
                            {
                                kidReference = (PdfReference)children.Items[mid];
                                kid = (PdfDictionary)kidReference.DataObject;
                            }
                        }

                        this.Add(key, value, overwrite, kid);
                        // Update the key limits!
                        this.UpdateNodeLimits(children);
                        break;
                    }
                }
            }
        }

        /**
          <summary>Removes all the given node's children.</summary>
          <remarks>
            <para>As this method doesn't apply balancing, it's suitable for clearing root nodes only.
            </para>
            <para>Removal affects only tree nodes: referenced objects are preserved to avoid inadvertently
            breaking possible references to them from somewhere else.</para>
          </remarks>
          <param name="node">Current node.</param>
        */
        private void Clear(
          PdfDictionary node
          )
        {
            var children = Children.Get(node, this.pairsKey);
            if (!children.IsLeaf())
            {
                foreach (var child in children.Items)
                {
                    this.Clear((PdfDictionary)child.Resolve());
                    this.File.Unregister((PdfReference)child);
                }
                node[this.pairsKey] = node[children.TypeName];
                _ = node.Remove(children.TypeName); // Recycles the array as the intermediate node transforms to leaf.
            }
            children.Items.Clear();
            _ = node.Remove(PdfName.Limits);
        }

        private void Fill<TObject>(
          IFiller<TObject> filler,
          PdfDictionary node
          )
        {
            var kidsObject = (PdfArray)node.Resolve(PdfName.Kids);
            if (kidsObject == null) // Leaf node.
            {
                var namesObject = (PdfArray)node.Resolve(this.pairsKey);
                for (
                  int index = 0,
                    length = namesObject.Count;
                  index < length;
                  index += 2
                  )
                { filler.Add(namesObject, index); }
            }
            else // Intermediate node.
            {
                foreach (var kidObject in kidsObject)
                { this.Fill(filler, (PdfDictionary)kidObject.Resolve()); }
            }
        }

        /**
          <summary>Gets the given node's entries count.</summary>
          <param name="node">Current node.</param>
        */
        private int GetCount(
          PdfDictionary node
          )
        {
            var children = (PdfArray)node.Resolve(this.pairsKey);
            if (children != null) // Leaf node.
            { return children.Count / 2; }
            else // Intermediate node.
            {
                children = (PdfArray)node.Resolve(PdfName.Kids);
                var count = 0;
                foreach (var child in children)
                { count += this.GetCount((PdfDictionary)child.Resolve()); }
                return count;
            }
        }

        private void Initialize(
          )
        {
            this.pairsKey = this.PairsKey;

            var baseDataObject = this.BaseDataObject;
            if (baseDataObject.Count == 0)
            {
                baseDataObject.Updateable = false;
                baseDataObject[this.pairsKey] = new PdfArray(); // NOTE: Initial root is by definition a leaf node.
                baseDataObject.Updateable = true;
            }
        }

        /**
          <summary>Splits a full node.</summary>
          <remarks>A new node is inserted at the full node's position, receiving the lower half of its
          children.</remarks>
          <param name="nodes">Parent nodes.</param>
          <param name="fullNodeIndex">Full node's position among the parent nodes.</param>
          <param name="childrenTypeName">Full node's children type.</param>
        */
        private void SplitFullNode(
          PdfArray nodes,
          int fullNodeIndex,
          PdfName childrenTypeName
          )
        {
            // Get the full node!
            var fullNode = (PdfDictionary)nodes.Resolve(fullNodeIndex);
            var fullNodeChildren = (PdfArray)fullNode.Resolve(childrenTypeName);

            // Create a new (sibling) node!
            var newNode = new PdfDictionary();
            var newNodeChildren = new PdfArray();
            newNode[childrenTypeName] = newNodeChildren;
            // Insert the new node just before the full!
            nodes.Insert(fullNodeIndex, this.File.Register(newNode)); // NOTE: Nodes MUST be indirect objects.

            // Transferring exceeding children to the new node...
            for (int index = 0, length = Children.InfoImpl.Get(childrenTypeName).MinCount; index < length; index++)
            {
                var removedChild = fullNodeChildren[0];
                fullNodeChildren.RemoveAt(0);
                newNodeChildren.Add(removedChild);
            }

            // Update the key limits!
            this.UpdateNodeLimits(newNode, newNodeChildren, childrenTypeName);
            this.UpdateNodeLimits(fullNode, fullNodeChildren, childrenTypeName);
        }

        /**
          <summary>Sets the key limits of the given node.</summary>
          <param name="children">Node children.</param>
        */
        private void UpdateNodeLimits(
          Children children
          )
        { this.UpdateNodeLimits(children.Parent, children.Items, children.TypeName); }

        /**
          <summary>Sets the key limits of the given node.</summary>
          <param name="node">Node to update.</param>
          <param name="children">Node children.</param>
          <param name="childrenTypeName">Node's children type.</param>
        */
        private void UpdateNodeLimits(
          PdfDictionary node,
          PdfArray children,
          PdfName childrenTypeName
          )
        {
            // Root node?
            if (node == this.BaseDataObject)
            {
                return; // NOTE: Root nodes DO NOT specify limits.
            }

            PdfDirectObject lowLimit, highLimit;
            if (childrenTypeName.Equals(PdfName.Kids))
            {
                lowLimit = ((PdfArray)((PdfDictionary)children.Resolve(0)).Resolve(PdfName.Limits))[0];
                highLimit = ((PdfArray)((PdfDictionary)children.Resolve(children.Count - 1)).Resolve(PdfName.Limits))[1];
            }
            else if (childrenTypeName.Equals(this.pairsKey))
            {
                lowLimit = children[0];
                highLimit = children[children.Count - 2];
            }
            else // NOTE: Should NEVER happen.
            {
                throw new NotSupportedException($"{childrenTypeName} is NOT a supported child type.");
            }

            var limits = (PdfArray)node[PdfName.Limits];
            if (limits != null)
            {
                limits[0] = lowLimit;
                limits[1] = highLimit;
            }
            else
            {
                node[PdfName.Limits] = new PdfArray(
                  new PdfDirectObject[]
                  {
            lowLimit,
            highLimit
                  }
                  );
            }
        }

        /**
          <summary>Wraps a base object within its corresponding high-level representation.</summary>
        */
        protected abstract TValue WrapValue(
          PdfDirectObject baseObject
          );

        /**
  <summary>Gets the name of the key-value pairs entries.</summary>
*/
        protected abstract PdfName PairsKey
        {
            get;
        }

        public virtual void Add(
  TKey key,
  TValue value
  )
        { this.Add(key, value, false); }

        public virtual void Clear(
          )
        { this.Clear(this.BaseDataObject); }

        public virtual bool ContainsKey(
          TKey key
          )
        {
            /*
              NOTE: Here we assume that any named entry has a non-null value.
            */
            return this[key] != null;
        }

        public virtual void CopyTo(
          KeyValuePair<TKey, TValue>[] keyValuePairs,
          int index
          )
        { throw new NotImplementedException(); }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator(
  )
        { return new Enumerator(this); }

        /**
Gets the key associated to the specified value.
*/
        public TKey GetKey(
          TValue value
          )
        {
            /*
              NOTE: Current implementation doesn't support bidirectional maps, to say that the only
              currently-available way to retrieve a key from a value is to iterate the whole map (really
              poor performance!).
            */
            foreach (var entry in this)
            {
                if (entry.Value.Equals(value))
                {
                    return entry.Key;
                }
            }
            return null;
        }

        public virtual bool Remove(
          TKey key
          )
        {
            var node = this.BaseDataObject;
            var nodeReferenceStack = new Stack<PdfReference>();
            while (true)
            {
                var nodeChildren = Children.Get(node, this.pairsKey);
                if (nodeChildren.IsLeaf()) // Leaf node.
                {
                    int low = 0, high = nodeChildren.Items.Count - nodeChildren.Info.ItemCount;
                    while (true)
                    {
                        if (low > high) // No match.
                        {
                            return false;
                        }

                        int mid = (mid = (low + high) / 2) - (mid % 2);
                        var comparison = key.CompareTo(nodeChildren.Items[mid]);
                        if (comparison < 0) // Key before.
                        { high = mid - 2; }
                        else if (comparison > 0) // Key after.
                        { low = mid + 2; }
                        else // Key matched.
                        {
                            // We got it!
                            nodeChildren.Items.RemoveAt(mid + 1); // Removes value.
                            nodeChildren.Items.RemoveAt(mid); // Removes key.
                            if ((mid == 0) || (mid == nodeChildren.Items.Count)) // Limits changed.
                            {
                                // Update key limits!
                                this.UpdateNodeLimits(nodeChildren);

                                // Updating key limits on ascendants...
                                var rootReference = (PdfReference)this.BaseObject;
                                PdfReference nodeReference;
                                while ((nodeReferenceStack.Count > 0) && !(nodeReference = nodeReferenceStack.Pop()).Equals(rootReference))
                                {
                                    var parentChildren = (PdfArray)nodeReference.Parent;
                                    var nodeIndex = parentChildren.IndexOf(nodeReference);
                                    if ((nodeIndex == 0) || (nodeIndex == parentChildren.Count - 1))
                                    {
                                        var parent = (PdfDictionary)parentChildren.Parent;
                                        this.UpdateNodeLimits(parent, parentChildren, PdfName.Kids);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            return true;
                        }
                    }
                }
                else // Intermediate node.
                {
                    int low = 0, high = nodeChildren.Items.Count - nodeChildren.Info.ItemCount;
                    while (true)
                    {
                        if (low > high) // Outside the limit range.
                        {
                            return false;
                        }

                        var mid = (low + high) / 2;
                        var kidReference = (PdfReference)nodeChildren.Items[mid];
                        var kid = (PdfDictionary)kidReference.DataObject;
                        var limits = (PdfArray)kid.Resolve(PdfName.Limits);
                        if (key.CompareTo(limits[0]) < 0) // Before the lower limit.
                        { high = mid - 1; }
                        else if (key.CompareTo(limits[1]) > 0) // After the upper limit.
                        { low = mid + 1; }
                        else // Limit range matched.
                        {
                            var kidChildren = Children.Get(kid, this.pairsKey);
                            if (kidChildren.IsUndersized())
                            {
                                /*
                                  NOTE: Rebalancing is required as minimum node size invariant is violated.
                                */
                                PdfDictionary leftSibling = null;
                                Children leftSiblingChildren = null;
                                if (mid > 0)
                                {
                                    leftSibling = (PdfDictionary)nodeChildren.Items.Resolve(mid - 1);
                                    leftSiblingChildren = Children.Get(leftSibling, this.pairsKey);
                                }
                                PdfDictionary rightSibling = null;
                                Children rightSiblingChildren = null;
                                if (mid < nodeChildren.Items.Count - 1)
                                {
                                    rightSibling = (PdfDictionary)nodeChildren.Items.Resolve(mid + 1);
                                    rightSiblingChildren = Children.Get(rightSibling, this.pairsKey);
                                }

                                if ((leftSiblingChildren != null) && !leftSiblingChildren.IsUndersized())
                                {
                                    // Move the last child subtree of the left sibling to be the first child subtree of the kid!
                                    for (int index = 0, endIndex = leftSiblingChildren.Info.ItemCount; index < endIndex; index++)
                                    {
                                        var itemIndex = leftSiblingChildren.Items.Count - 1;
                                        var item = leftSiblingChildren.Items[itemIndex];
                                        leftSiblingChildren.Items.RemoveAt(itemIndex);
                                        kidChildren.Items.Insert(0, item);
                                    }
                                    // Update left sibling's key limits!
                                    this.UpdateNodeLimits(leftSiblingChildren);
                                }
                                else if ((rightSiblingChildren != null) && !rightSiblingChildren.IsUndersized())
                                {
                                    // Move the first child subtree of the right sibling to be the last child subtree of the kid!
                                    for (int index = 0, endIndex = rightSiblingChildren.Info.ItemCount; index < endIndex; index++)
                                    {
                                        var itemIndex = 0;
                                        var item = rightSiblingChildren.Items[itemIndex];
                                        rightSiblingChildren.Items.RemoveAt(itemIndex);
                                        kidChildren.Items.Add(item);
                                    }
                                    // Update right sibling's key limits!
                                    this.UpdateNodeLimits(rightSiblingChildren);
                                }
                                else
                                {
                                    if (leftSibling != null)
                                    {
                                        // Merging with the left sibling...
                                        for (var index = leftSiblingChildren.Items.Count; index-- > 0;)
                                        {
                                            var item = leftSiblingChildren.Items[index];
                                            leftSiblingChildren.Items.RemoveAt(index);
                                            kidChildren.Items.Insert(0, item);
                                        }
                                        nodeChildren.Items.RemoveAt(mid - 1);
                                        _ = leftSibling.Delete();
                                    }
                                    else if (rightSibling != null)
                                    {
                                        // Merging with the right sibling...
                                        for (var index = rightSiblingChildren.Items.Count; index-- > 0;)
                                        {
                                            var itemIndex = 0;
                                            var item = rightSiblingChildren.Items[itemIndex];
                                            rightSiblingChildren.Items.RemoveAt(itemIndex);
                                            kidChildren.Items.Add(item);
                                        }
                                        nodeChildren.Items.RemoveAt(mid + 1);
                                        _ = rightSibling.Delete();
                                    }
                                    if (nodeChildren.Items.Count == 1)
                                    {
                                        // Collapsing node...
                                        // Remove the lonely intermediate node from the parent!
                                        nodeChildren.Items.RemoveAt(0);
                                        if (node == this.BaseDataObject) // Root node [FIX:50].
                                        {
                                            /*
                                              NOTE: In case of root collapse, Kids entry must be converted to
                                              key-value-pairs entry, as no more intermediate nodes are available.
                                            */
                                            node[this.pairsKey] = node[PdfName.Kids];
                                            _ = node.Remove(PdfName.Kids);
                                            nodeChildren.TypeName = this.pairsKey;
                                        }
                                        // Populate the parent with the lonely intermediate node's children!
                                        for (var index = kidChildren.Items.Count; index-- > 0;)
                                        {
                                            const int RemovedItemIndex = 0;
                                            var item = kidChildren.Items[RemovedItemIndex];
                                            kidChildren.Items.RemoveAt(RemovedItemIndex);
                                            nodeChildren.Items.Add(item);
                                        }
                                        _ = kid.Delete();
                                        kid = node;
                                        kidReference = kid.Reference;
                                        kidChildren = nodeChildren;
                                    }
                                }
                                // Update key limits!
                                this.UpdateNodeLimits(kidChildren);
                            }
                            // Go down one level!
                            nodeReferenceStack.Push(kidReference);
                            node = kid;
                            break;
                        }
                    }
                }
            }
        }

        public virtual bool Remove(
          KeyValuePair<TKey, TValue> keyValuePair
          )
        { throw new NotSupportedException(); }

        public virtual bool TryGetValue(
          TKey key,
          out TValue value
          )
        {
            value = this[key];
            return value != null;
        }

        public virtual int Count => this.GetCount(this.BaseDataObject);

        public virtual bool IsReadOnly => false;

        public virtual ICollection<TKey> Keys
        {
            get
            {
                var filler = new KeysFiller();
                this.Fill(filler, this.BaseDataObject);

                return filler.Collection;
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            {
                var filler = new ValuesFiller(this);
                this.Fill(filler, this.BaseDataObject);
                return filler.Collection;
            }
        }
        /*
          NOTE: This implementation is an adaptation of the B-tree algorithm described in "Introduction
          to Algorithms" [1], 2nd ed (Cormen, Leiserson, Rivest, Stein) published by MIT Press/McGraw-Hill.
          PDF trees represent a special subset of B-trees whereas actual keys are concentrated in leaf
          nodes and proxied by boundary limits across their paths. This simplifies some handling but
          requires keeping node limits updated whenever a change occurs in the leaf nodes composition.

          [1] http://en.wikipedia.org/wiki/Introduction_to_Algorithms
        */
        /**
  Node children.
*/
        private sealed class Children
        {

            private InfoImpl info;
            private PdfArray items;
            private readonly PdfDictionary parent;
            private PdfName typeName;

            private Children(
              PdfDictionary parent,
              PdfName typeName
              )
            {
                this.parent = parent;
                this.TypeName = typeName;
            }

            /**
              <summary>Gets the given node's children.</summary>
              <param name="node">Parent node.</param>
              <param name="pairs">Pairs key.</param>
            */
            public static Children Get(
              PdfDictionary node,
              PdfName pairsKey
              )
            {
                PdfName childrenTypeName;
                if (node.ContainsKey(PdfName.Kids))
                { childrenTypeName = PdfName.Kids; }
                else if (node.ContainsKey(pairsKey))
                { childrenTypeName = pairsKey; }
                else
                {
                    throw new Exception("Malformed tree node.");
                }

                return new Children(node, childrenTypeName);
            }

            /**
              <summary>Gets whether the collection size has reached its maximum.</summary>
            */
            public bool IsFull(
              )
            { return this.Items.Count >= this.Info.MaxCount; }

            /**
              <summary>Gets whether this collection represents a leaf node.</summary>
            */
            public bool IsLeaf(
              )
            { return !this.TypeName.Equals(PdfName.Kids); }

            /**
              <summary>Gets whether the collection size is more than its maximum.</summary>
            */
            public bool IsOversized(
              )
            { return this.Items.Count > this.Info.MaxCount; }

            /**
              <summary>Gets whether the collection size is less than its minimum.</summary>
            */
            public bool IsUndersized(
              )
            { return this.Items.Count < this.Info.MinCount; }

            /**
              <summary>Gets whether the collection size is within the order limits.</summary>
            */
            public bool IsValid(
              )
            { return !(this.IsUndersized() || this.IsOversized()); }

            /**
              <summary>Gets the node's children info.</summary>
            */
            public InfoImpl Info => this.info;

            /**
              <summary>Gets the node's children collection.</summary>
            */
            public PdfArray Items => this.items;

            /**
              <summary>Gets the node.</summary>
            */
            public PdfDictionary Parent => this.parent;

            /**
              <summary>Gets/Sets the node's children type.</summary>
            */
            public PdfName TypeName
            {
                get => this.typeName;
                set
                {
                    this.typeName = value;
                    this.items = (PdfArray)this.parent.Resolve(this.typeName);
                    this.info = InfoImpl.Get(this.typeName);
                }
            }

            public sealed class InfoImpl
            {
                private static readonly InfoImpl KidsInfo = new InfoImpl(1, TreeLowOrder);
                private static readonly InfoImpl PairsInfo = new InfoImpl(2, TreeLowOrder); // NOTE: Paired children are combinations of 2 contiguous items.

                /** Number of (contiguous) children defining an item. */
                public int ItemCount;
                /** Maximum number of children. */
                public int MaxCount;
                /** Minimum number of children. */
                public int MinCount;

                public InfoImpl(
                  int itemCount,
                  int lowOrder
                  )
                {
                    this.ItemCount = itemCount;
                    this.MinCount = itemCount * lowOrder;
                    this.MaxCount = this.MinCount * 2;
                }

                public static InfoImpl Get(
                  PdfName typeName
                  )
                { return typeName.Equals(PdfName.Kids) ? KidsInfo : PairsInfo; }
            }
        }

        private class Enumerator
          : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            /**
              <summary>Current container.</summary>
            */
            private PdfIndirectObject container;
            /**
<summary>Current named object.</summary>
*/
            private KeyValuePair<TKey, TValue>? current;

            /**
              <summary>Current child tree nodes.</summary>
            */
            private PdfArray kids;

            /**
              <summary>Current level index.</summary>
            */
            private int levelIndex = 0;
            /**
              <summary>Stacked levels.</summary>
            */
            private readonly Stack<object[]> levels = new Stack<object[]>();
            /**
              <summary>Current names.</summary>
            */
            private PdfArray names;

            /**
              <summary>Name tree.</summary>
            */
            private readonly Tree<TKey, TValue> tree;

            internal Enumerator(
  Tree<TKey, TValue> tree
  )
            {
                this.tree = tree;

                this.container = tree.Container;
                var rootNode = tree.BaseDataObject;
                var kidsObject = rootNode[PdfName.Kids];
                if (kidsObject == null) // Leaf node.
                {
                    var namesObject = rootNode[tree.pairsKey];
                    if (namesObject is PdfReference)
                    { this.container = ((PdfReference)namesObject).IndirectObject; }
                    this.names = (PdfArray)namesObject.Resolve();
                }
                else // Intermediate node.
                {
                    if (kidsObject is PdfReference)
                    { this.container = ((PdfReference)kidsObject).IndirectObject; }
                    this.kids = (PdfArray)kidsObject.Resolve();
                }
            }

            KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => this.current.Value;

            private KeyValuePair<TKey, TValue>? GetNext(
  )
            {
                /*
                  NOTE: Algorithm:
                  1. [Vertical, down] We have to go downward the name tree till we reach
                  a names collection (leaf node).
                  2. [Horizontal] Then we iterate across the names collection.
                  3. [Vertical, up] When leaf-nodes scan is complete, we go upward solving
                  parent nodes, repeating step 1.
                */
                while (true)
                {
                    if (this.names == null)
                    {
                        if ((this.kids == null)
                          || (this.kids.Count == this.levelIndex)) // Kids subtree complete.
                        {
                            if (this.levels.Count == 0)
                            {
                                return null;
                            }

                            // 3. Go upward one level.
                            // Restore current level!
                            var level = this.levels.Pop();
                            this.container = (PdfIndirectObject)level[0];
                            this.kids = (PdfArray)level[1];
                            this.levelIndex = ((int)level[2]) + 1; // Next node (partially scanned level).
                        }
                        else // Kids subtree incomplete.
                        {
                            // 1. Go downward one level.
                            // Save current level!
                            this.levels.Push(new object[] { this.container, this.kids, this.levelIndex });

                            // Move downward!
                            var kidReference = (PdfReference)this.kids[this.levelIndex];
                            this.container = kidReference.IndirectObject;
                            var kid = (PdfDictionary)kidReference.DataObject;
                            var kidsObject = kid[PdfName.Kids];
                            if (kidsObject == null) // Leaf node.
                            {
                                var namesObject = kid[this.tree.pairsKey];
                                if (namesObject is PdfReference)
                                { this.container = ((PdfReference)namesObject).IndirectObject; }
                                this.names = (PdfArray)namesObject.Resolve();
                                this.kids = null;
                            }
                            else // Intermediate node.
                            {
                                if (kidsObject is PdfReference)
                                { this.container = ((PdfReference)kidsObject).IndirectObject; }
                                this.kids = (PdfArray)kidsObject.Resolve();
                            }
                            this.levelIndex = 0; // First node (new level).
                        }
                    }
                    else
                    {
                        if (this.names.Count == this.levelIndex) // Names complete.
                        { this.names = null; }
                        else // Names incomplete.
                        {
                            // 2. Object found.
                            var key = (TKey)this.names[this.levelIndex];
                            var value = this.tree.WrapValue(this.names[this.levelIndex + 1]);
                            this.levelIndex += 2;

                            return new KeyValuePair<TKey, TValue>(key, value);
                        }
                    }
                }
            }

            public void Dispose(
  )
            { }

            public bool MoveNext(
              )
            { return (this.current = this.GetNext()) != null; }

            public void Reset(
              )
            { throw new NotSupportedException(); }

            public object Current => ((IEnumerator<KeyValuePair<TKey, TValue>>)this).Current;
        }

        private interface IFiller<TObject>
        {
            void Add(
              PdfArray names,
              int offset
              );

            ICollection<TObject> Collection
            {
                get;
            }
        }

        private class KeysFiller
          : IFiller<TKey>
        {
            private readonly ICollection<TKey> keys = new List<TKey>();

            public void Add(
              PdfArray names,
              int offset
              )
            { this.keys.Add((TKey)names[offset]); }

            public ICollection<TKey> Collection => this.keys;
        }

        private class ValuesFiller
          : IFiller<TValue>
        {
            private readonly Tree<TKey, TValue> tree;
            private readonly ICollection<TValue> values = new List<TValue>();

            internal ValuesFiller(
              Tree<TKey, TValue> tree
              )
            { this.tree = tree; }

            public void Add(
              PdfArray names,
              int offset
              )
            { this.values.Add(this.tree.WrapValue(names[offset + 1])); }

            public ICollection<TValue> Collection => this.values;
        }
    }
}