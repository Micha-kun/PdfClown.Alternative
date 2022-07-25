/*
  Copyright 2007-2010 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.contents.objects
{
    using System;

    using System.Collections;
    using System.Collections.Generic;
    using org.pdfclown.objects;

    /**
      <summary>Inline image entries (anonymous) operation [PDF:1.6:4.8.6].</summary>
      <remarks>This is a figurative operation necessary to constrain the inline image entries section
      within the content stream model.</remarks>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class InlineImageHeader
      : Operation,
        IDictionary<PdfName, PdfDirectObject>
    {
        // [FIX:0.0.4:2] Null operator.
        public InlineImageHeader(
          IList<PdfDirectObject> operands
          ) : base(string.Empty, operands)
        { }

        public PdfDirectObject this[
          PdfName key
          ]
        {
            get
            {
                /*
                  NOTE: This is an intentional violation of the official .NET Framework Class
                  Library prescription: no exception thrown anytime a key is not found.
                */
                var index = this.GetKeyIndex(key);
                if (index == null)
                {
                    return null;
                }

                return this.operands[index.Value + 1];
            }
            set
            {
                var index = this.GetKeyIndex(key);
                if (index == null)
                {
                    this.operands.Add(key);
                    this.operands.Add(value);
                }
                else
                {
                    this.operands[index.Value] = key;
                    this.operands[index.Value + 1] = value;
                }
            }
        }

        void ICollection<KeyValuePair<PdfName, PdfDirectObject>>.Add(
  KeyValuePair<PdfName, PdfDirectObject> keyValuePair
  )
        { this.Add(keyValuePair.Key, keyValuePair.Value); }

        bool ICollection<KeyValuePair<PdfName, PdfDirectObject>>.Contains(
          KeyValuePair<PdfName, PdfDirectObject> keyValuePair
          )
        { return this[keyValuePair.Key] == keyValuePair.Value; }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<PdfName, PdfDirectObject>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<PdfName, PdfDirectObject>> IEnumerable<KeyValuePair<PdfName, PdfDirectObject>>.GetEnumerator(
  )
        {
            for (
              int index = 0,
                length = this.operands.Count - 1;
              index < length;
              index += 2
              )
            {
                yield return new KeyValuePair<PdfName, PdfDirectObject>(
                  (PdfName)this.operands[index],
                  this.operands[index + 1]
                  );
            }
        }

        private int? GetKeyIndex(
  object key
  )
        {
            for (
              int index = 0,
                length = this.operands.Count - 1;
              index < length;
              index += 2
              )
            {
                if (this.operands[index].Equals(key))
                {
                    return index;
                }
            }
            return null;
        }

        public void Add(
PdfName key,
PdfDirectObject value
)
        {
            if (this.ContainsKey(key))
            {
                throw new ArgumentException($"Key '{key}' already in use.", nameof(key));
            }

            this[key] = value;
        }

        public void Clear(
          )
        { this.operands.Clear(); }

        public bool ContainsKey(
          PdfName key
          )
        { return this.GetKeyIndex(key) != null; }

        public void CopyTo(
          KeyValuePair<PdfName, PdfDirectObject>[] keyValuePairs,
          int index
          )
        { throw new NotImplementedException(); }

        public bool Remove(
          PdfName key
          )
        {
            var index = this.GetKeyIndex(key);
            if (!index.HasValue)
            {
                return false;
            }

            this.operands.RemoveAt(index.Value);
            this.operands.RemoveAt(index.Value);
            return true;
        }

        public bool Remove(
          KeyValuePair<PdfName, PdfDirectObject> keyValuePair
          )
        { throw new NotImplementedException(); }

        public bool TryGetValue(
          PdfName key,
          out PdfDirectObject value
          )
        { throw new NotImplementedException(); }

        public int Count => this.operands.Count / 2;

        public bool IsReadOnly => false;

        public ICollection<PdfName> Keys
        {
            get
            {
                ICollection<PdfName> keys = new List<PdfName>();
                for (
                  int index = 0,
                    length = this.operands.Count - 1;
                  index < length;
                  index += 2
                  )
                { keys.Add((PdfName)this.operands[index]); }
                return keys;
            }
        }

        public ICollection<PdfDirectObject> Values
        {
            get
            {
                ICollection<PdfDirectObject> values = new List<PdfDirectObject>();
                for (
                  int index = 1,
                    length = this.operands.Count - 1;
                  index < length;
                  index += 2
                  )
                { values.Add(this.operands[index]); }
                return values;
            }
        }
    }
}