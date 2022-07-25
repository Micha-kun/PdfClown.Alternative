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
    using org.pdfclown.documents.contents.xObjects;
    using org.pdfclown.documents.files;
    using org.pdfclown.objects;

    using org.pdfclown.util;

    /**
      <summary>Media clip data [PDF:1.7:9.1.3].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public sealed class MediaClipData
      : MediaClip
    {

        internal MediaClipData(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public MediaClipData(
PdfObjectWrapper data,
string mimeType
) : base(data.Document, PdfName.MCD)
        {
            this.Data = data;
            this.MimeType = mimeType;
            this.TempFilePermission = TempFilePermissionEnum.Always;
        }

        public override PdfObjectWrapper Data
        {
            get
            {
                var dataObject = this.BaseDataObject[PdfName.D];
                if (dataObject == null)
                {
                    return null;
                }

                if (dataObject.Resolve() is PdfStream)
                {
                    return FormXObject.Wrap(dataObject);
                }
                else
                {
                    return FileSpecification.Wrap(dataObject);
                }
            }
            set => this.BaseDataObject[PdfName.D] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the MIME type of data [RFC 2045].</summary>
        */
        public string MimeType
        {
            get => (string)PdfString.GetValue(this.BaseDataObject[PdfName.CT]);
            set => this.BaseDataObject[PdfName.CT] = (value != null) ? new PdfString(value) : null;
        }

        /**
          <summary>Gets/Sets the player rules for playing this media.</summary>
        */
        public MediaPlayers Players
        {
            get => MediaPlayers.Wrap(this.BaseDataObject.Get<PdfDictionary>(PdfName.PL));
            set => this.BaseDataObject[PdfName.PL] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the preferred options the renderer should attempt to honor without affecting its
          viability.</summary>
        */
        public Viability Preferences
        {
            get => new Viability(this.BaseDataObject.Get<PdfDictionary>(PdfName.BE));
            set => this.BaseDataObject[PdfName.BE] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the minimum requirements the renderer must honor in order to be considered viable.
          </summary>
        */
        public Viability Requirements
        {
            get => new Viability(this.BaseDataObject.Get<PdfDictionary>(PdfName.MH));
            set => this.BaseDataObject[PdfName.MH] = PdfObjectWrapper.GetBaseObject(value);
        }

        /**
          <summary>Gets/Sets the circumstance under which it is acceptable to write a temporary file in order
          to play this media clip.</summary>
        */
        public TempFilePermissionEnum? TempFilePermission
        {
            get => TempFilePermissionEnumExtension.Get((PdfString)this.BaseDataObject.Resolve<PdfDictionary>(PdfName.P)[PdfName.TF]);
            set => this.BaseDataObject.Resolve<PdfDictionary>(PdfName.P)[PdfName.TF] = value.HasValue ? value.Value.GetCode() : null;
        }
        /**
  <summary>Circumstance under which it is acceptable to write a temporary file in order to play
  a media clip.</summary>
*/
        public enum TempFilePermissionEnum
        {
            /**
              <summary>Never allowed.</summary>
            */
            Never,
            /**
              <summary>Allowed only if the document permissions allow content extraction.</summary>
            */
            ContentExtraction,
            /**
              <summary>Allowed only if the document permissions allow content extraction, including for
              accessibility purposes.</summary>
            */
            Accessibility,
            /**
              <summary>Always allowed.</summary>
            */
            Always
        }

        /**
          <summary>Media clip data viability.</summary>
        */
        public class Viability
          : PdfObjectWrapper<PdfDictionary>
        {
            internal Viability(
              PdfDirectObject baseObject
              ) : base(baseObject)
            { }

            /**
              <summary>Gets the absolute URL to be used as the base URL in resolving any relative URLs
              found within the media data.</summary>
            */
            public Uri BaseURL
            {
                get
                {
                    var baseURLObject = (PdfString)this.BaseDataObject[PdfName.BU];
                    return (baseURLObject != null) ? new Uri(baseURLObject.StringValue) : null;
                }
                set => this.BaseDataObject[PdfName.BU] = (value != null) ? new PdfString(value.ToString()) : null;
            }
        }
    }

    internal static class TempFilePermissionEnumExtension
    {
        private static readonly BiDictionary<MediaClipData.TempFilePermissionEnum, PdfString> codes;

        static TempFilePermissionEnumExtension(
          )
        {
            codes = new BiDictionary<MediaClipData.TempFilePermissionEnum, PdfString>();
            codes[MediaClipData.TempFilePermissionEnum.Never] = new PdfString("TEMPNEVER");
            codes[MediaClipData.TempFilePermissionEnum.ContentExtraction] = new PdfString("TEMPEXTRACT");
            codes[MediaClipData.TempFilePermissionEnum.Accessibility] = new PdfString("TEMPACCESS");
            codes[MediaClipData.TempFilePermissionEnum.Always] = new PdfString("TEMPALWAYS");
        }

        public static MediaClipData.TempFilePermissionEnum? Get(
          PdfString code
          )
        {
            if (code == null)
            {
                return null;
            }

            MediaClipData.TempFilePermissionEnum? tempFilePermission = codes.GetKey(code);
            if (!tempFilePermission.HasValue)
            {
                throw new NotSupportedException($"Operation unknown: {code}");
            }

            return tempFilePermission;
        }

        public static PdfString GetCode(
          this MediaClipData.TempFilePermissionEnum tempFilePermission
          )
        { return codes[tempFilePermission]; }
    }
}