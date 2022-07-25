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

namespace org.pdfclown.documents.multimedia
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    using org.pdfclown.documents.interchange.access;
    using org.pdfclown.objects;
    using org.pdfclown.util.math;

    /**
      <summary>Rendition [PDF:1.7:9.1.2].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public abstract class Rendition
      : PdfObjectWrapper<PdfDictionary>,
        IPdfNamedObjectWrapper
    {

        protected Rendition(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        protected Rendition(
Document context,
PdfName subtype
) : base(
context,
new PdfDictionary(
new PdfName[]
{
            PdfName.Type,
            PdfName.S
},
new PdfDirectObject[]
{
            PdfName.Rendition,
            subtype
}
)
)
        { }

        protected override PdfString RetrieveName(
  )
        {
            /*
              NOTE: A rendition dictionary is not required to have a name tree entry. When it does, the
              viewer application should ensure that the name specified in the tree is kept the same as the
              value of the N entry (for example, if the user interface allows the name to be changed).
            */
            return (PdfString)this.BaseDataObject[PdfName.N];
        }

        /**
<summary>Wraps a rendition base object into a rendition object.</summary>
<param name="baseObject">Rendition base object.</param>
<returns>Rendition object associated to the base object.</returns>
*/
        public static Rendition Wrap(
          PdfDirectObject baseObject
          )
        {
            if (baseObject == null)
            {
                return null;
            }

            var subtype = (PdfName)((PdfDictionary)baseObject.Resolve())[PdfName.S];
            if (PdfName.MR.Equals(subtype))
            {
                return new MediaRendition(baseObject);
            }
            else if (PdfName.SR.Equals(subtype))
            {
                return new SelectorRendition(baseObject);
            }
            else
            {
                throw new ArgumentException("It doesn't represent a valid clip object.", nameof(baseObject));
            }
        }

        public PdfString Name => this.RetrieveName();

        public PdfDirectObject NamedBaseObject => this.RetrieveNamedBaseObject();

        /**
<summary>Gets/Sets the preferred options the renderer should attempt to honor without affecting
its viability [PDF:1.7:9.1.1].</summary>
*/
        public Viability Preferences
        {
            get => new Viability(this.BaseDataObject.Get<PdfDictionary>(PdfName.BE));
            set => this.BaseDataObject[PdfName.BE] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the minimum requirements the renderer must honor in order to be considered
          viable [PDF:1.7:9.1.1].</summary>
        */
        public Viability Requirements
        {
            get => new Viability(this.BaseDataObject.Get<PdfDictionary>(PdfName.MH));
            set => this.BaseDataObject[PdfName.MH] = PdfObjectWrapper.GetBaseObject(value);
        }
        /**
  <summary>Rendition viability [PDF:1.7:9.1.2].</summary>
*/
        public class Viability
          : PdfObjectWrapper<PdfDictionary>
        {
            internal Viability(
              PdfDirectObject baseObject
              ) : base(baseObject)
            { }

            private PdfDictionary MediaCriteria => this.BaseDataObject.Resolve<PdfDictionary>(PdfName.C);

            /**
              <summary>Gets whether to hear audio descriptions.</summary>
              <remarks>Equivalent to SMIL's systemAudioDesc attribute.</remarks>
            */
            public bool AudioDescriptionEnabled => (bool)PdfBoolean.GetValue(this.MediaCriteria[PdfName.A]);

            /**
              <summary>Gets whether to hear audio overdubs.</summary>
            */
            public bool AudioOverdubEnabled => (bool)PdfBoolean.GetValue(this.MediaCriteria[PdfName.O]);

            /**
              <summary>Gets the minimum system's bandwidth (in bits per second).</summary>
              <remarks>Equivalent to SMIL's systemBitrate attribute.</remarks>
            */
            public int? Bandwidth => (int?)PdfInteger.GetValue(this.MediaCriteria[PdfName.R]);

            /**
              <summary>Gets the list of supported languages.</summary>
              <remarks>Equivalent to SMIL's systemLanguage attribute.</remarks>
            */
            public IList<LanguageIdentifier> Languages
            {
                get
                {
                    IList<LanguageIdentifier> languages = new List<LanguageIdentifier>();
                    var languagesObject = (PdfArray)this.MediaCriteria[PdfName.L];
                    if (languagesObject != null)
                    {
                        foreach (var languageObject in languagesObject)
                        { languages.Add(LanguageIdentifier.Wrap(languageObject)); }
                    }
                    return languages;
                }
            }

            /**
              <summary>Gets the list of supported viewer applications.</summary>
            */
            public Array<SoftwareIdentifier> Renderers => Array<SoftwareIdentifier>.Wrap<SoftwareIdentifier>(this.MediaCriteria.Get<PdfArray>(PdfName.V));

            /**
              <summary>Gets the minimum screen color depth (in bits per pixel).</summary>
              <remarks>Equivalent to SMIL's systemScreenDepth attribute.</remarks>
            */
            public int? ScreenDepth
            {
                get
                {
                    var screenDepthObject = (PdfDictionary)this.MediaCriteria[PdfName.D];
                    return (screenDepthObject != null) ? ((PdfInteger)screenDepthObject[PdfName.V]).IntValue : ((int?)null);
                }
            }

            /**
              <summary>Gets the minimum screen size (in pixels).</summary>
              <remarks>Equivalent to SMIL's systemScreenSize attribute.</remarks>
            */
            public Size? ScreenSize
            {
                get
                {
                    var screenSizeObject = (PdfDictionary)this.MediaCriteria[PdfName.Z];
                    if (screenSizeObject == null)
                    {
                        return null;
                    }

                    var screenSizeValueObject = (PdfArray)screenSizeObject[PdfName.V];
                    return (screenSizeValueObject != null)
                      ? (new Size(
                        ((PdfInteger)screenSizeValueObject[0]).IntValue,
                        ((PdfInteger)screenSizeValueObject[1]).IntValue
                        ))
                      : ((Size?)null);
                }
            }

            /**
              <summary>Gets whether to see subtitles.</summary>
            */
            public bool SubtitleEnabled => (bool)PdfBoolean.GetValue(this.MediaCriteria[PdfName.S]);

            /**
              <summary>Gets whether to see text captions.</summary>
              <remarks>Equivalent to SMIL's systemCaptions attribute.</remarks>
            */
            public bool TextCaptionEnabled => (bool)PdfBoolean.GetValue(this.MediaCriteria[PdfName.C]);

            /**
              <summary>Gets the PDF version range supported by the viewer application.</summary>
            */
            public Interval<pdfclown.Version> Version
            {
                get
                {
                    var pdfVersionArray = (PdfArray)this.MediaCriteria[PdfName.P];
                    return ((pdfVersionArray != null) && (pdfVersionArray.Count > 0))
                      ? (new Interval<pdfclown.Version>(
                        pdfclown.Version.Get((PdfName)pdfVersionArray[0]),
                        (pdfVersionArray.Count > 1) ? pdfclown.Version.Get((PdfName)pdfVersionArray[1]) : null
                        ))
                      : null;
                }
            }

            //TODO:setters!
        }
    }
}
