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

namespace org.pdfclown.documents.interaction.annotations
{
    using System.Collections;
    using System.Collections.Generic;

    using org.pdfclown.documents.interaction.actions;
    using org.pdfclown.objects;
    using system = System;

    /**
      <summary>Annotation actions [PDF:1.6:8.5.2].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public abstract class AnnotationActions
      : PdfObjectWrapper<PdfDictionary>,
        IDictionary<PdfName, Action>
    {
        private readonly Annotation parent;

        internal AnnotationActions(
          Annotation parent,
          PdfDirectObject baseObject
          ) : base(baseObject)
        { this.parent = parent; }

        public AnnotationActions(
  Annotation parent
  ) : base(parent.Document, new PdfDictionary())
        { this.parent = parent; }

        public Action this[
          PdfName key
          ]
        {
            get => Action.Wrap(this.BaseDataObject[key]);
            set => this.BaseDataObject[key] = (value != null) ? value.BaseObject : null;
        }

        void ICollection<KeyValuePair<PdfName, Action>>.Add(
  KeyValuePair<PdfName, Action> entry
  )
        { this.Add(entry.Key, entry.Value); }

        bool ICollection<KeyValuePair<PdfName, Action>>.Contains(
          KeyValuePair<PdfName, Action> entry
          )
        { return entry.Value.BaseObject.Equals(this.BaseDataObject[entry.Key]); }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<PdfName, Action>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<PdfName, Action>> IEnumerable<KeyValuePair<PdfName, Action>>.GetEnumerator(
  )
        {
            foreach (var key in this.Keys)
            { yield return new KeyValuePair<PdfName, Action>(key, this[key]); }
        }

        public void Add(
  PdfName key,
  Action value
  )
        { this.BaseDataObject.Add(key, value.BaseObject); }

        public void Clear(
          )
        {
            this.BaseDataObject.Clear();
            this.OnActivate = null;
        }

        public override object Clone(
Document context
)
        { throw new system::NotImplementedException(); } // TODO: verify parent reference.

        public bool ContainsKey(
          PdfName key
          )
        {
            return this.BaseDataObject.ContainsKey(key)
              || (PdfName.A.Equals(key) && this.parent.BaseDataObject.ContainsKey(key));
        }

        public void CopyTo(
          KeyValuePair<PdfName, Action>[] entries,
          int index
          )
        { throw new system::NotImplementedException(); }

        public bool Remove(
          PdfName key
          )
        {
            if (PdfName.A.Equals(key) && this.parent.BaseDataObject.ContainsKey(key))
            {
                this.OnActivate = null;
                return true;
            }
            else
            {
                return this.BaseDataObject.Remove(key);
            }
        }

        public bool Remove(
          KeyValuePair<PdfName, Action> entry
          )
        {
            return this.BaseDataObject.Remove(
              new KeyValuePair<PdfName, PdfDirectObject>(
                entry.Key,
                entry.Value.BaseObject
                )
              );
        }

        public bool TryGetValue(
          PdfName key,
          out Action value
          )
        {
            value = this[key];
            if (value == null)
            {
                return this.ContainsKey(key);
            }
            else
            {
                return true;
            }
        }

        public int Count => this.BaseDataObject.Count + (this.parent.BaseDataObject.ContainsKey(PdfName.A) ? 1 : 0);

        public bool IsReadOnly => false;

        public ICollection<PdfName> Keys => this.BaseDataObject.Keys;

        /**
          <summary>Gets/Sets the action to be performed when the annotation is activated.</summary>
        */
        public Action OnActivate
        {
            get => this.parent.Action;
            set => this.parent.Action = value;
        }

        /**
          <summary>Gets/Sets the action to be performed when the cursor enters the annotation's active area.</summary>
        */
        public Action OnEnter
        {
            get => this[PdfName.E];
            set => this[PdfName.E] = value;
        }

        /**
          <summary>Gets/Sets the action to be performed when the cursor exits the annotation's active area.</summary>
        */
        public Action OnExit
        {
            get => this[PdfName.X];
            set => this[PdfName.X] = value;
        }

        /**
          <summary>Gets/Sets the action to be performed when the mouse button is pressed
          inside the annotation's active area.</summary>
        */
        public Action OnMouseDown
        {
            get => this[PdfName.D];
            set => this[PdfName.D] = value;
        }

        /**
          <summary>Gets/Sets the action to be performed when the mouse button is released
          inside the annotation's active area.</summary>
        */
        public Action OnMouseUp
        {
            get => this[PdfName.U];
            set => this[PdfName.U] = value;
        }

        /**
          <summary>Gets/Sets the action to be performed when the page containing the annotation is closed.</summary>
        */
        public Action OnPageClose
        {
            get => this[PdfName.PC];
            set => this[PdfName.PC] = value;
        }

        /**
          <summary>Gets/Sets the action to be performed when the page containing the annotation
          is no longer visible in the viewer application's user interface.</summary>
        */
        public Action OnPageInvisible
        {
            get => this[PdfName.PI];
            set => this[PdfName.PI] = value;
        }

        /**
          <summary>Gets/Sets the action to be performed when the page containing the annotation is opened.</summary>
        */
        public Action OnPageOpen
        {
            get => this[PdfName.PO];
            set => this[PdfName.PO] = value;
        }

        /**
          <summary>Gets/Sets the action to be performed when the page containing the annotation
          becomes visible in the viewer application's user interface.</summary>
        */
        public Action OnPageVisible
        {
            get => this[PdfName.PV];
            set => this[PdfName.PV] = value;
        }

        public ICollection<Action> Values
        {
            get
            {
                List<Action> values;
                var objs = this.BaseDataObject.Values;
                values = new List<Action>(objs.Count);
                foreach (var obj in objs)
                { values.Add(Action.Wrap(obj)); }
                var action = this.OnActivate;
                if (action != null)
                { values.Add(action); }
                return values;
            }
        }
    }
}