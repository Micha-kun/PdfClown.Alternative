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

    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Optional content properties [PDF:1.7:4.10.3].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public sealed class LayerDefinition
      : PdfObjectWrapper<PdfDictionary>,
        ILayerConfiguration
    {

        private LayerDefinition(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public LayerDefinition(
Document context
) : base(context, new PdfDictionary())
        { }

        public static LayerDefinition Wrap(
PdfDirectObject baseObject
)
        { return (baseObject != null) ? new LayerDefinition(baseObject) : null; }

        /**
<summary>Gets the layer configurations used under particular circumstances.</summary>
*/
        public Array<LayerConfiguration> AlternateConfigurations
        {
            get => Array<LayerConfiguration>.Wrap<LayerConfiguration>(this.BaseDataObject.Get<PdfArray>(PdfName.Configs));
            set => this.BaseDataObject[PdfName.Configs] = value.BaseObject;
        }

        public string Creator
        {
            get => this.DefaultConfiguration.Creator;
            set => this.DefaultConfiguration.Creator = value;
        }

        /**
          <summary>Gets the default layer configuration, that is the initial state of the optional
          content groups when a document is first opened.</summary>
        */
        public LayerConfiguration DefaultConfiguration
        {
            get => LayerConfiguration.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.D));
            set => this.BaseDataObject[PdfName.D] = value.BaseObject;
        }

        public ISet<PdfName> Intents
        {
            get => this.DefaultConfiguration.Intents;
            set => this.DefaultConfiguration.Intents = value;
        }

        /**
          <summary>Gets the collection of all the layers existing in the document.</summary>
        */
        public Layers Layers => Layers.Wrap(this.BaseDataObject.Get<PdfArray>(PdfName.OCGs));

        public Array<OptionGroup> OptionGroups => this.DefaultConfiguration.OptionGroups;

        public string Title
        {
            get => this.DefaultConfiguration.Title;
            set => this.DefaultConfiguration.Title = value;
        }

        public UILayers UILayers => this.DefaultConfiguration.UILayers;

        public UIModeEnum UIMode
        {
            get => this.DefaultConfiguration.UIMode;
            set => this.DefaultConfiguration.UIMode = value;
        }

        public bool? Visible
        {
            get => this.DefaultConfiguration.Visible;
            set => this.DefaultConfiguration.Visible = value;
        }
    }
}

