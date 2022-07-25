/*
  Copyright 2006-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.navigation.document
{
    using System;
    using org.pdfclown.documents.contents.colorSpaces;
    using org.pdfclown.objects;

    using actions = org.pdfclown.documents.interaction.actions;

    /**
      <summary>Outline item [PDF:1.6:8.2.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class Bookmark
      : PdfObjectWrapper<PdfDictionary>,
        ILink
    {

        internal Bookmark(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        public Bookmark(
Document context,
string title
) : base(context, new PdfDictionary())
        { this.Title = title; }

        public Bookmark(
          Document context,
          string title,
          LocalDestination destination
          ) : this(context, title)
        { this.Destination = destination; }

        public Bookmark(
          Document context,
          string title,
          actions.Action action
          ) : this(context, title)
        { this.Action = action; }

        private actions::Action Action
        {
            get => actions.Action.Wrap(this.BaseDataObject[PdfName.A]);
            set
            {
                if (value == null)
                { _ = this.BaseDataObject.Remove(PdfName.A); }
                else
                {
                    /*
                      NOTE: This entry is not permitted in bookmarks if a 'Dest' entry already exists.
                    */
                    if (this.BaseDataObject.ContainsKey(PdfName.Dest))
                    { _ = this.BaseDataObject.Remove(PdfName.Dest); }

                    this.BaseDataObject[PdfName.A] = value.BaseObject;
                }
            }
        }

        private Destination Destination
        {
            get
            {
                var destinationObject = this.BaseDataObject[PdfName.Dest];
                return (destinationObject != null)
                  ? this.Document.ResolveName<LocalDestination>(destinationObject)
                  : null;
            }
            set
            {
                if (value == null)
                { _ = this.BaseDataObject.Remove(PdfName.Dest); }
                else
                {
                    /*
                      NOTE: This entry is not permitted in bookmarks if an 'A' entry is present.
                    */
                    if (this.BaseDataObject.ContainsKey(PdfName.A))
                    { _ = this.BaseDataObject.Remove(PdfName.A); }

                    this.BaseDataObject[PdfName.Dest] = value.NamedBaseObject;
                }
            }
        }

        /**
<summary>Gets the child bookmarks.</summary>
*/
        public Bookmarks Bookmarks => Bookmarks.Wrap(this.BaseObject);

        /**
          <summary>Gets/Sets the bookmark text color.</summary>
        */
        [PDF(VersionEnum.PDF14)]
        public DeviceRGBColor Color
        {
            get => DeviceRGBColor.Get((PdfArray)this.BaseDataObject[PdfName.C]);
            set
            {
                if (value == null)
                { _ = this.BaseDataObject.Remove(PdfName.C); }
                else
                {
                    this.CheckCompatibility("Color");
                    this.BaseDataObject[PdfName.C] = value.BaseObject;
                }
            }
        }

        /**
          <summary>Gets/Sets whether this bookmark's children are displayed.</summary>
        */
        public bool Expanded
        {
            get
            {
                var countObject = (PdfInteger)this.BaseDataObject[PdfName.Count];

                return (countObject == null)
                  || (countObject.RawValue >= 0);
            }
            set
            {
                var countObject = (PdfInteger)this.BaseDataObject[PdfName.Count];
                if (countObject == null)
                {
                    return;
                }

                /*
                  NOTE: Positive Count entry means open, negative Count entry means closed [PDF:1.6:8.2.2].
                */
                this.BaseDataObject[PdfName.Count] = PdfInteger.Get((value ? 1 : (-1)) * Math.Abs(countObject.IntValue));
            }
        }

        /**
          <summary>Gets/Sets the bookmark flags.</summary>
        */
        [PDF(VersionEnum.PDF14)]
        public FlagsEnum Flags
        {
            get
            {
                var flagsObject = (PdfInteger)this.BaseDataObject[PdfName.F];
                if (flagsObject == null)
                {
                    return 0;
                }

                return (FlagsEnum)Enum.ToObject(
                  typeof(FlagsEnum),
                  flagsObject.RawValue
                  );
            }
            set
            {
                if (value == 0)
                { _ = this.BaseDataObject.Remove(PdfName.F); }
                else
                {
                    this.CheckCompatibility(value);
                    this.BaseDataObject[PdfName.F] = PdfInteger.Get((int)value);
                }
            }
        }

        /**
          <summary>Gets the parent bookmark.</summary>
        */
        public Bookmark Parent
        {
            get
            {
                var reference = (PdfReference)this.BaseDataObject[PdfName.Parent];
                // Is its parent a bookmark?
                /*
                  NOTE: the Title entry can be used as a flag to distinguish bookmark
                  (outline item) dictionaries from outline (root) dictionaries.
                */
                if (((PdfDictionary)reference.DataObject).ContainsKey(PdfName.Title)) // Bookmark.
                {
                    return new Bookmark(reference);
                }
                else // Outline root.
                {
                    return null; // NO parent bookmark.
                }
            }
        }

        public PdfObjectWrapper Target
        {
            get
            {
                if (this.BaseDataObject.ContainsKey(PdfName.Dest))
                {
                    return this.Destination;
                }
                else if (this.BaseDataObject.ContainsKey(PdfName.A))
                {
                    return this.Action;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value is Destination)
                { this.Destination = (Destination)value; }
                else if (value is actions.Action)
                { this.Action = (actions::Action)value; }
                else
                {
                    throw new ArgumentException("It MUST be either a Destination or an Action.");
                }
            }
        }

        /**
          <summary>Gets/Sets the text to be displayed for this bookmark.</summary>
        */
        public string Title
        {
            get => (string)((PdfTextString)this.BaseDataObject[PdfName.Title]).Value;
            set => this.BaseDataObject[PdfName.Title] = new PdfTextString(value);
        }
        /**
  <summary>Bookmark flags [PDF:1.6:8.2.2].</summary>
*/
        [Flags]
        [PDF(VersionEnum.PDF14)]
        public enum FlagsEnum
        {
            /**
              <summary>Display the item in italic.</summary>
            */
            Italic = 0x1,
            /**
              <summary>Display the item in bold.</summary>
            */
            Bold = 0x2
        }
    }
}