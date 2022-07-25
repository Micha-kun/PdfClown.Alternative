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

namespace org.pdfclown.documents.interaction.actions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using System.Collections.ObjectModel;
    using org.pdfclown.documents.contents.layers;
    using org.pdfclown.objects;
    using org.pdfclown.util;

    /**
      <summary>'Set the state of one or more optional content groups' action [PDF:1.6:8.5.3].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public sealed class SetLayerState
      : Action
    {

        internal SetLayerState(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
<summary>Creates a new action within the given document context.</summary>
*/
        public SetLayerState(
          Document context
          ) : this(context, (LayerState)null)
        { }

        /**
          <summary>Creates a new action within the given document context.</summary>
        */
        public SetLayerState(
          Document context,
          params LayerState[] states
          ) : base(context, PdfName.SetOCGState)
        {
            this.States = new LayerStates();
            if ((states != null) && (states.Length > 0))
            {
                var layerStates = this.States;
                foreach (var state in states)
                { layerStates.Add(state); }
            }
        }

        public LayerStates States
        {
            get => new LayerStates(this.BaseDataObject[PdfName.State]);
            set => this.BaseDataObject[PdfName.State] = value.BaseObject;
        }

        public enum StateModeEnum
        {
            On,
            Off,
            Toggle
        }

        public class LayerState
        {

            private LayerStates baseStates;

            private readonly LayersImpl layers;
            private StateModeEnum mode;

            internal LayerState(
              StateModeEnum mode,
              LayersImpl layers,
              LayerStates baseStates
              )
            {
                this.mode = mode;
                this.layers = layers;
                this.layers.parentState = this;
                this.Attach(baseStates);
            }

            public LayerState(
              StateModeEnum mode,
              params Layer[] layers
              ) : this(mode, new LayersImpl(), null)
            {
                foreach (var layer in layers)
                { this.layers.Add(layer); }
            }

            internal void Attach(
              LayerStates baseStates
              )
            { this.baseStates = baseStates; }

            internal void Detach(
              )
            { this.baseStates = null; }

            public override bool Equals(
              object obj
              )
            {
                if (!(obj is LayerState))
                {
                    return false;
                }

                var state = (LayerState)obj;
                if (!state.Mode.Equals(this.Mode)
                  || (state.Layers.Count != this.Layers.Count))
                {
                    return false;
                }

                var layerIterator = this.Layers.GetEnumerator();
                var stateLayerIterator = state.Layers.GetEnumerator();
                while (layerIterator.MoveNext())
                {
                    _ = stateLayerIterator.MoveNext();
                    if (!layerIterator.Current.Equals(stateLayerIterator.Current))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode(
              )
            { return this.mode.GetHashCode() ^ this.layers.Count; }

            public IList<Layer> Layers => this.layers;

            public StateModeEnum Mode
            {
                get => this.mode;
                set
                {
                    this.mode = value;

                    if (this.baseStates != null)
                    {
                        var baseIndex = this.baseStates.GetBaseIndex(this);
                        this.baseStates.BaseDataObject[baseIndex] = value.GetName();
                    }
                }
            }

            internal class LayersImpl
              : Collection<Layer>
            {
                internal LayerState parentState;

                private LayerStates BaseStates => (this.parentState != null) ? this.parentState.baseStates : null;

                protected override void ClearItems(
                  )
                {
                    // Low-level definition.
                    var baseStates = this.BaseStates;
                    if (baseStates != null)
                    {
                        var itemIndex = baseStates.GetBaseIndex(this.parentState)
                          + 1; // Name object offset.
                        for (var count = this.Count; count > 0; count--)
                        { baseStates.BaseDataObject.RemoveAt(itemIndex); }
                    }
                    // High-level definition.
                    base.ClearItems();
                }

                protected override void InsertItem(
                  int index,
                  Layer item
                  )
                {
                    // High-level definition.
                    base.InsertItem(index, item);
                    // Low-level definition.
                    var baseStates = this.BaseStates;
                    if (baseStates != null)
                    {
                        var baseIndex = baseStates.GetBaseIndex(this.parentState);
                        var itemIndex = baseIndex
                          + 1 // Name object offset.
                          + index; // Layer object offset.
                        baseStates.BaseDataObject[itemIndex] = item.BaseObject;
                    }
                }

                protected override void RemoveItem(
                  int index
                  )
                {
                    // High-level definition.
                    base.RemoveItem(index);
                    // Low-level definition.
                    var baseStates = this.BaseStates;
                    if (baseStates != null)
                    {
                        var baseIndex = baseStates.GetBaseIndex(this.parentState);
                        var itemIndex = baseIndex
                          + 1 // Name object offset.
                          + index; // Layer object offset.
                        baseStates.BaseDataObject.RemoveAt(itemIndex);
                    }
                }

                protected override void SetItem(
                  int index,
                  Layer item
                  )
                {
                    this.RemoveItem(index);
                    this.InsertItem(index, item);
                }
            }
        }

        public class LayerStates
          : PdfObjectWrapper<PdfArray>,
            IList<LayerState>
        {
            private IList<LayerState> items;

            internal LayerStates(
              PdfDirectObject baseObject
              ) : base(baseObject)
            { this.Initialize(); }

            public LayerStates(
              ) : base(new PdfArray())
            { }

            public LayerState this[
              int index
              ]
            {
                get => this.items[index];
                set
                {
                    this.RemoveAt(index);
                    this.Insert(index, value);
                }
            }

            IEnumerator IEnumerable.GetEnumerator(
  )
            { return this.GetEnumerator(); }

            private void Initialize(
              )
            {
                this.items = new List<LayerState>();
                var baseDataObject = this.BaseDataObject;
                StateModeEnum? mode = null;
                LayerState.LayersImpl layers = null;
                for (
                  int baseIndex = 0,
                    baseCount = baseDataObject.Count;
                  baseIndex < baseCount;
                  baseIndex++
                  )
                {
                    var baseObject = baseDataObject[baseIndex];
                    if (baseObject is PdfName)
                    {
                        if (mode.HasValue)
                        { this.items.Add(new LayerState(mode.Value, layers, this)); }
                        mode = StateModeEnumExtension.Get((PdfName)baseObject);
                        layers = new LayerState.LayersImpl();
                    }
                    else
                    { layers.Add(Layer.Wrap(baseObject)); }
                }
                if (mode.HasValue)
                { this.items.Add(new LayerState(mode.Value, layers, this)); }
            }

            /**
              <summary>Gets the position of the initial base item corresponding to the specified layer
              state index.</summary>
              <param name="index">Layer state index.</param>
              <returns>-1, in case <code>index</code> is outside the available range.</returns>
            */
            internal int GetBaseIndex(
              int index
              )
            {
                var baseIndex = -1;
                var baseDataObject = this.BaseDataObject;
                var layerStateIndex = -1;
                for (
                  int baseItemIndex = 0,
                    baseItemCount = baseDataObject.Count;
                  baseItemIndex < baseItemCount;
                  baseItemIndex++
                  )
                {
                    if (baseDataObject[baseItemIndex] is PdfName)
                    {
                        layerStateIndex++;
                        if (layerStateIndex == index)
                        {
                            baseIndex = baseItemIndex;
                            break;
                        }
                    }
                }
                return baseIndex;
            }

            /**
              <summary>Gets the position of the initial base item corresponding to the specified layer
              state.</summary>
              <param name="item">Layer state.</param>
              <returns>-1, in case <code>item</code> has no match.</returns>
            */
            internal int GetBaseIndex(
              LayerState item
              )
            {
                var baseIndex = -1;
                var baseDataObject = this.BaseDataObject;
                for (
                  int baseItemIndex = 0,
                    baseItemCount = baseDataObject.Count;
                  baseItemIndex < baseItemCount;
                  baseItemIndex++
                  )
                {
                    var baseItem = baseDataObject[baseItemIndex];
                    if ((baseItem is PdfName)
                      && baseItem.Equals(item.Mode.GetName()))
                    {
                        foreach (var layer in item.Layers)
                        {
                            if (++baseItemIndex >= baseItemCount)
                            {
                                break;
                            }

                            baseItem = baseDataObject[baseItemIndex];
                            if ((baseItem is PdfName)
                              || !baseItem.Equals(layer.BaseObject))
                            {
                                break;
                            }
                        }
                    }
                }
                return baseIndex;
            }

            public void Add(
  LayerState item
  )
            {
                var baseDataObject = this.BaseDataObject;
                // Low-level definition.
                baseDataObject.Add(item.Mode.GetName());
                foreach (var layer in item.Layers)
                { baseDataObject.Add(layer.BaseObject); }
                // High-level definition.
                this.items.Add(item);
                item.Attach(this);
            }

            public void Clear(
              )
            {
                // Low-level definition.
                this.BaseDataObject.Clear();
                // High-level definition.
                foreach (var item in this.items)
                { item.Detach(); }
                this.items.Clear();
            }

            public bool Contains(
              LayerState item
              )
            { return this.items.Contains(item); }

            public void CopyTo(
              LayerState[] items,
              int index
              )
            { throw new NotImplementedException(); }

            public IEnumerator<LayerState> GetEnumerator(
  )
            { return this.items.GetEnumerator(); }

            public int IndexOf(
  LayerState item
  )
            { return this.items.IndexOf(item); }

            public void Insert(
              int index,
              LayerState item
              )
            {
                var baseIndex = this.GetBaseIndex(index);
                if (baseIndex == -1)
                { this.Add(item); }
                else
                {
                    var baseDataObject = this.BaseDataObject;
                    // Low-level definition.
                    baseDataObject.Insert(baseIndex++, item.Mode.GetName());
                    foreach (var layer in item.Layers)
                    { baseDataObject.Insert(baseIndex++, layer.BaseObject); }
                    // High-level definition.
                    this.items.Insert(index, item);
                    item.Attach(this);
                }
            }

            public bool Remove(
              LayerState item
              )
            {
                var index = this.IndexOf(item);
                if (index == -1)
                {
                    return false;
                }

                this.RemoveAt(index);
                return true;
            }

            public void RemoveAt(
              int index
              )
            {
                LayerState layerState;
                // Low-level definition.

                var baseIndex = this.GetBaseIndex(index);
                if (baseIndex == -1)
                {
                    throw new IndexOutOfRangeException();
                }

                var baseDataObject = this.BaseDataObject;
                var done = false;
                for (var baseCount = baseDataObject.Count; baseIndex < baseCount;)
                {
                    if (baseDataObject[baseIndex] is PdfName)
                    {
                        if (done)
                        {
                            break;
                        }

                        done = true;
                    }
                    baseDataObject.RemoveAt(baseIndex);
                }
                // High-level definition.

                layerState = this.items[index];
                this.items.RemoveAt(index);
                layerState.Detach();
            }

            public int Count => this.items.Count;

            public bool IsReadOnly => false;
        }
    }

    internal static class StateModeEnumExtension
    {
        private static readonly BiDictionary<SetLayerState.StateModeEnum, PdfName> codes;

        static StateModeEnumExtension()
        {
            codes = new BiDictionary<SetLayerState.StateModeEnum, PdfName>();
            codes[SetLayerState.StateModeEnum.On] = PdfName.ON;
            codes[SetLayerState.StateModeEnum.Off] = PdfName.OFF;
            codes[SetLayerState.StateModeEnum.Toggle] = PdfName.Toggle;
        }

        public static SetLayerState.StateModeEnum Get(
          PdfName name
          )
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            SetLayerState.StateModeEnum? stateMode = codes.GetKey(name);
            if (!stateMode.HasValue)
            {
                throw new NotSupportedException($"State mode unknown: {name}");
            }

            return stateMode.Value;
        }

        public static PdfName GetName(
          this SetLayerState.StateModeEnum stateMode
          )
        { return codes[stateMode]; }
    }
}