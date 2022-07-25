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
    using System.Collections.Generic;

    using System.Linq;
    using org.pdfclown.objects;
    using org.pdfclown.util;

    /**
      <summary>Optional content configuration [PDF:1.7:4.10.3].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public sealed class LayerConfiguration
      : PdfObjectWrapper<PdfDictionary>,
        ILayerConfiguration
    {

        public LayerConfiguration(
Document context
) : base(context, new PdfDictionary())
        { }

        public LayerConfiguration(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
  <summary>Gets the collection of the layer objects whose state is set to OFF.</summary>
*/
        private PdfArray OffLayersObject => this.BaseDataObject.Resolve<PdfArray>(PdfName.OFF);

        /**
          <summary>Gets the collection of the layer objects whose state is set to ON.</summary>
        */
        private PdfArray OnLayersObject => this.BaseDataObject.Resolve<PdfArray>(PdfName.ON);

        internal bool IsVisible(
  Layer layer
  )
        {
            var visible = this.Visible;
            if (!visible.HasValue || visible.Value)
            {
                return !this.OffLayersObject.Contains(layer.BaseObject);
            }
            else
            {
                return this.OnLayersObject.Contains(layer.BaseObject);
            }
        }

        /**
          <summary>Sets the usage application for the specified factors.</summary>
          <param name="event">Situation in which this usage application should be used. May be
            <see cref="PdfName.View">View</see>, <see cref="PdfName.Print">Print</see> or <see
            cref="PdfName.Export">Export</see>.</param>
          <param name="category">Layer usage entry to consider when managing the states of the layer.
          </param>
          <param name="layer">Layer which should have its state automatically managed based on its usage
            information.</param>
          <param name="retain">Whether this usage application has to be kept or removed.</param>
        */
        internal void SetUsageApplication(
          PdfName @event,
          PdfName category,
          Layer layer,
          bool retain
          )
        {
            var matched = false;
            var usages = this.BaseDataObject.Resolve<PdfArray>(PdfName.AS);
            foreach (var usage in usages)
            {
                var usageDictionary = (PdfDictionary)usage;
                if (usageDictionary[PdfName.Event].Equals(@event)
                  && ((PdfArray)usageDictionary[PdfName.Category]).Contains(category))
                {
                    var usageLayers = usageDictionary.Resolve<PdfArray>(PdfName.OCGs);
                    if (usageLayers.Contains(layer.BaseObject))
                    {
                        if (!retain)
                        { _ = usageLayers.Remove(layer.BaseObject); }
                    }
                    else
                    {
                        if (retain)
                        { usageLayers.Add(layer.BaseObject); }
                    }
                    matched = true;
                }
            }
            if (!matched && retain)
            {
                var usageDictionary = new PdfDictionary();
                usageDictionary[PdfName.Event] = @event;
                usageDictionary.Resolve<PdfArray>(PdfName.Category).Add(category);
                usageDictionary.Resolve<PdfArray>(PdfName.OCGs).Add(layer.BaseObject);
                usages.Add(usageDictionary);
            }
        }

        internal void SetVisible(
          Layer layer,
          bool value
          )
        {
            var layerObject = layer.BaseObject;
            var offLayersObject = this.OffLayersObject;
            var onLayersObject = this.OnLayersObject;
            var visible = this.Visible;
            if (!visible.HasValue)
            {
                if (value && !onLayersObject.Contains(layerObject))
                {
                    onLayersObject.Add(layerObject);
                    _ = offLayersObject.Remove(layerObject);
                }
                else if (!value && !offLayersObject.Contains(layerObject))
                {
                    offLayersObject.Add(layerObject);
                    _ = onLayersObject.Remove(layerObject);
                }
            }
            else if (!visible.Value)
            {
                if (value && !onLayersObject.Contains(layerObject))
                { onLayersObject.Add(layerObject); }
            }
            else
            {
                if (!value && !offLayersObject.Contains(layerObject))
                { offLayersObject.Add(layerObject); }
            }
        }

        public static LayerConfiguration Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new LayerConfiguration(baseObject) : null; }

        public string Creator
        {
            get => (string)PdfSimpleObject<object>.GetValue(this.BaseDataObject[PdfName.Creator]);
            set => this.BaseDataObject[PdfName.Creator] = PdfTextString.Get(value);
        }

        public ISet<PdfName> Intents
        {
            get
            {
                ISet<PdfName> intents = new HashSet<PdfName>();
                var intentObject = this.BaseDataObject.Resolve(PdfName.Intent);
                if (intentObject != null)
                {
                    if (intentObject is PdfArray) // Multiple intents.
                    {
                        foreach (var intentItem in (PdfArray)intentObject)
                        { _ = intents.Add((PdfName)intentItem); }
                    }
                    else // Single intent.
                    { _ = intents.Add((PdfName)intentObject); }
                }
                else
                { _ = intents.Add(IntentEnum.View.Name()); }
                return intents;
            }
            set
            {
                PdfDirectObject intentObject = null;
                if ((value != null)
                  && (value.Count > 0))
                {
                    if (value.Count == 1) // Single intent.
                    {
                        intentObject = value.First();
                        if (intentObject.Equals(IntentEnum.View.Name())) // Default.
                        { intentObject = null; }
                    }
                    else // Multiple intents.
                    {
                        var intentArray = new PdfArray();
                        foreach (var valueItem in value)
                        { intentArray.Add(valueItem); }
                    }
                }
                this.BaseDataObject[PdfName.Intent] = intentObject;
            }
        }

        public Array<OptionGroup> OptionGroups => Array<OptionGroup>.Wrap<OptionGroup>(this.BaseDataObject.Get<PdfArray>(PdfName.RBGroups));

        public string Title
        {
            get => (string)PdfSimpleObject<object>.GetValue(this.BaseDataObject[PdfName.Name]);
            set => this.BaseDataObject[PdfName.Name] = PdfTextString.Get(value);
        }

        public UILayers UILayers => UILayers.Wrap(this.BaseDataObject.Get<PdfArray>(PdfName.Order));

        public UIModeEnum UIMode
        {
            get => UIModeEnumExtension.Get((PdfName)this.BaseDataObject[PdfName.ListMode]);
            set => this.BaseDataObject[PdfName.ListMode] = value.GetName();
        }

        public bool? Visible
        {
            get => BaseStateEnumExtension.Get((PdfName)this.BaseDataObject[PdfName.BaseState]).IsEnabled();
            set
            {
                /*
                  NOTE: Base state can be altered only in case of alternate configuration; default ones MUST
                  be set to default state (that is ON).
                */
                if (!(this.BaseObject.Parent is PdfDictionary)) // Not the default configuration?
                { this.BaseDataObject[PdfName.BaseState] = BaseStateEnumExtension.Get(value).GetName(); }
            }
        }
        /**
  <summary>Base state used to initialize the states of all the layers in a document when this
  configuration is applied.</summary>
*/
        internal enum BaseStateEnum
        {
            /**
              <summary>All the layers are visible.</summary>
            */
            On,
            /**
              <summary>All the layers are invisible.</summary>
            */
            Off,
            /**
              <summary>All the layers are left unchanged.</summary>
            */
            Unchanged
        }
    }

    internal static class BaseStateEnumExtension
    {
        private static readonly BiDictionary<LayerConfiguration.BaseStateEnum, PdfName> codes;

        static BaseStateEnumExtension()
        {
            codes = new BiDictionary<LayerConfiguration.BaseStateEnum, PdfName>();
            codes[LayerConfiguration.BaseStateEnum.On] = PdfName.ON;
            codes[LayerConfiguration.BaseStateEnum.Off] = PdfName.OFF;
            codes[LayerConfiguration.BaseStateEnum.Unchanged] = PdfName.Unchanged;
        }

        public static LayerConfiguration.BaseStateEnum Get(
          PdfName name
          )
        {
            if (name == null)
            {
                return LayerConfiguration.BaseStateEnum.On;
            }

            LayerConfiguration.BaseStateEnum? baseState = codes.GetKey(name);
            if (!baseState.HasValue)
            {
                throw new NotSupportedException($"Base state unknown: {name}");
            }

            return baseState.Value;
        }

        public static LayerConfiguration.BaseStateEnum Get(
          bool? enabled
          )
        { return enabled.HasValue ? (enabled.Value ? LayerConfiguration.BaseStateEnum.On : LayerConfiguration.BaseStateEnum.Off) : LayerConfiguration.BaseStateEnum.Unchanged; }

        public static PdfName GetName(
          this LayerConfiguration.BaseStateEnum baseState
          )
        { return codes[baseState]; }

        public static bool? IsEnabled(
          this LayerConfiguration.BaseStateEnum baseState
          )
        {
            switch (baseState)
            {
                case LayerConfiguration.BaseStateEnum.On:
                    return true;
                case LayerConfiguration.BaseStateEnum.Off:
                    return false;
                case LayerConfiguration.BaseStateEnum.Unchanged:
                    return null;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
