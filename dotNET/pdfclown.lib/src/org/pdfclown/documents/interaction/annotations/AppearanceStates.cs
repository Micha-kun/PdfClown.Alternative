/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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
    using System;
    using System.Collections;

    using System.Collections.Generic;
    using org.pdfclown.documents.contents.xObjects;
    using org.pdfclown.objects;

    /**
      <summary>Appearance states [PDF:1.6:8.4.4].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class AppearanceStates
      : PdfObjectWrapper<PdfDataObject>,
        IDictionary<PdfName, FormXObject>
    {
        private readonly Appearance appearance;

        private readonly PdfName statesKey;

        internal AppearanceStates(
  PdfName statesKey,
  Appearance appearance
  ) : base(appearance.BaseDataObject[statesKey])
        {
            this.appearance = appearance;
            this.statesKey = statesKey;
        }

        public FormXObject this[
          PdfName key
          ]
        {
            get
            {
                var baseDataObject = this.BaseDataObject;
                if (baseDataObject == null) // No state.
                {
                    return null;
                }
                else if (key == null)
                {
                    if (baseDataObject is PdfStream) // Single state.
                    {
                        return FormXObject.Wrap(this.BaseObject);
                    }
                    else // Multiple state, but invalid key.
                    {
                        return null;
                    }
                }
                else // Multiple state.
                {
                    return FormXObject.Wrap(((PdfDictionary)baseDataObject)[key]);
                }
            }
            set
            {
                if (key == null) // Single state.
                {
                    this.BaseObject = value.BaseObject;
                    this.appearance.BaseDataObject[this.statesKey] = this.BaseObject;
                }
                else // Multiple state.
                { this.EnsureDictionary()[key] = value.BaseObject; }
            }
        }

        void ICollection<KeyValuePair<PdfName, FormXObject>>.Add(
  KeyValuePair<PdfName, FormXObject> entry
  )
        { this.Add(entry.Key, entry.Value); }

        bool ICollection<KeyValuePair<PdfName, FormXObject>>.Contains(
          KeyValuePair<PdfName, FormXObject> entry
          )
        {
            var baseDataObject = this.BaseDataObject;
            if (baseDataObject == null) // No state.
            {
                return false;
            }
            else if (baseDataObject is PdfStream) // Single state.
            {
                return entry.Value.BaseObject.Equals(this.BaseObject);
            }
            else // Multiple state.
            {
                return entry.Value.Equals(this[entry.Key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<PdfName, FormXObject>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<PdfName, FormXObject>> IEnumerable<KeyValuePair<PdfName, FormXObject>>.GetEnumerator(
  )
        {
            var baseDataObject = this.BaseDataObject;
            if (baseDataObject == null) // No state.
            { /* NOOP. */ }
            else if (baseDataObject is PdfStream) // Single state.
            {
                yield return new KeyValuePair<PdfName, FormXObject>(
                  null,
                  FormXObject.Wrap(this.BaseObject)
                  );
            }
            else // Multiple state.
            {
                foreach (var entry in (PdfDictionary)baseDataObject)
                {
                    yield return new KeyValuePair<PdfName, FormXObject>(
                      entry.Key,
                      FormXObject.Wrap(entry.Value)
                      );
                }
            }
        }

        private PdfDictionary EnsureDictionary(
  )
        {
            var baseDataObject = this.BaseDataObject;
            if (!(baseDataObject is PdfDictionary))
            {
                /*
                  NOTE: Single states are erased as they have no valid key
                  to be consistently integrated within the dictionary.
                */
                this.BaseObject = (PdfDirectObject)(baseDataObject = new PdfDictionary());
                this.appearance.BaseDataObject[this.statesKey] = (PdfDictionary)baseDataObject;
            }
            return (PdfDictionary)baseDataObject;
        }

        //TODO
        /**
          Gets the key associated to a given value.
        */
        //   public PdfName GetKey(
        //     FormXObject value
        //     )
        //   {return BaseDataObject.GetKey(value.BaseObject);}

        public void Add(
  PdfName key,
  FormXObject value
  )
        { this.EnsureDictionary()[key] = value.BaseObject; }

        public void Clear(
          )
        { this.EnsureDictionary().Clear(); }

        public override object Clone(
          Document context
          )
        { throw new NotImplementedException(); } // TODO: verify appearance reference.

        public bool ContainsKey(
          PdfName key
          )
        {
            var baseDataObject = this.BaseDataObject;
            if (baseDataObject == null) // No state.
            {
                return false;
            }
            else if (baseDataObject is PdfStream) // Single state.
            {
                return key == null;
            }
            else // Multiple state.
            {
                return ((PdfDictionary)baseDataObject).ContainsKey(key);
            }
        }

        public void CopyTo(
          KeyValuePair<PdfName, FormXObject>[] entries,
          int index
          )
        { throw new NotImplementedException(); }

        public bool Remove(
          PdfName key
          )
        {
            var baseDataObject = this.BaseDataObject;
            if (baseDataObject == null) // No state.
            {
                return false;
            }
            else if (baseDataObject is PdfStream) // Single state.
            {
                if (key == null)
                {
                    this.BaseObject = null;
                    _ = this.appearance.BaseDataObject.Remove(this.statesKey);
                    return true;
                }
                else // Invalid key.
                {
                    return false;
                }
            }
            else // Multiple state.
            {
                return ((PdfDictionary)baseDataObject).Remove(key);
            }
        }

        public bool Remove(
          KeyValuePair<PdfName, FormXObject> entry
          )
        { throw new NotImplementedException(); }

        public bool TryGetValue(
          PdfName key,
          out FormXObject value
          )
        {
            value = this[key];
            return (value != null) || this.ContainsKey(key);
        }

        /**
<summary>Gets the appearance associated to these states.</summary>
*/
        public Appearance Appearance => this.appearance;

        public int Count
        {
            get
            {
                var baseDataObject = this.BaseDataObject;
                if (baseDataObject == null) // No state.
                {
                    return 0;
                }
                else if (baseDataObject is PdfStream) // Single state.
                {
                    return 1;
                }
                else // Multiple state.
                {
                    return ((PdfDictionary)baseDataObject).Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public ICollection<PdfName> Keys => throw new NotImplementedException();

        public ICollection<FormXObject> Values => throw new NotImplementedException();
    }
}