/*
  Copyright 2009-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /**
      <summary>Bidirectional bijective map.</summary>
    */
    public class BiDictionary<TKey, TValue>
      : IDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> dictionary;
        private readonly Dictionary<TValue, TKey> inverseDictionary;

        public BiDictionary(
  )
        {
            this.dictionary = new Dictionary<TKey, TValue>();
            this.inverseDictionary = new Dictionary<TValue, TKey>();
        }

        public BiDictionary(
          int capacity
          )
        {
            this.dictionary = new Dictionary<TKey, TValue>(capacity);
            this.inverseDictionary = new Dictionary<TValue, TKey>(capacity);
        }

        public BiDictionary(
          IDictionary<TKey, TValue> dictionary
          )
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary);
            //TODO: key duplicate collisions to resolve!
            //       inverseDictionary = this.dictionary.ToDictionary(entry => entry.Value, entry => entry.Key);
            this.inverseDictionary = new Dictionary<TValue, TKey>();
            foreach (var entry in this.dictionary)
            { this.inverseDictionary[entry.Value] = entry.Key; }
        }

        public virtual TValue this[
          TKey key
          ]
        {
            get
            {
                /*
                  NOTE: This is an intentional violation of the official .NET Framework Class Library
                  prescription.
                  My loose implementation emphasizes coding smoothness and concision, against ugly
                  TryGetValue() invocations: unfound keys are happily dealt with returning a default (null)
                  value.
                  If the user is interested in verifying whether such result represents a non-existing key
                  or an actual null object, it suffices to query ContainsKey() method.
                */
                TValue value;
                _ = this.dictionary.TryGetValue(key, out value);
                return value;
            }
            set
            {
                TValue oldValue;
                _ = this.dictionary.TryGetValue(key, out oldValue);
                this.dictionary[key] = value; // Sets the entry.
                if (oldValue != null)
                { _ = this.inverseDictionary.Remove(oldValue); }
                this.inverseDictionary[value] = key; // Sets the inverse entry.
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(
  KeyValuePair<TKey, TValue> keyValuePair
  )
        { this.Add(keyValuePair.Key, keyValuePair.Value); }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(
          KeyValuePair<TKey, TValue> keyValuePair
          )
        { return this.dictionary.Contains(keyValuePair); }

        IEnumerator IEnumerable.GetEnumerator(
  )
        { return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator(); }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator(
  )
        { return this.dictionary.GetEnumerator(); }

        public void Add(
  TKey key,
  TValue value
  )
        {
            this.dictionary.Add(key, value); // Adds the entry.
            try
            { this.inverseDictionary.Add(value, key); } // Adds the inverse entry.
            catch (Exception exception)
            {
                _ = this.dictionary.Remove(key); // Reverts the entry addition.
                throw exception;
            }
        }

        public void Clear(
          )
        {
            this.dictionary.Clear();
            this.inverseDictionary.Clear();
        }

        public bool ContainsKey(
          TKey key
          )
        { return this.dictionary.ContainsKey(key); }

        public bool ContainsValue(
TValue value
)
        { return this.inverseDictionary.ContainsKey(value); }

        public void CopyTo(
          KeyValuePair<TKey, TValue>[] keyValuePairs,
          int index
          )
        { throw new NotImplementedException(); }

        public virtual TKey GetKey(
          TValue value
          )
        { TKey key; _ = this.inverseDictionary.TryGetValue(value, out key); return key; }

        public bool Remove(
          TKey key
          )
        {
            TValue value;
            if (!this.dictionary.TryGetValue(key, out value))
            {
                return false;
            }

            _ = this.dictionary.Remove(key);
            _ = this.inverseDictionary.Remove(value);
            return true;
        }

        public bool Remove(
          KeyValuePair<TKey, TValue> keyValuePair
          )
        {
            if (!((ICollection<KeyValuePair<TKey, TValue>>)this.dictionary).Remove(keyValuePair))
            {
                return false;
            }

            _ = this.inverseDictionary.Remove(keyValuePair.Value);
            return true;
        }

        public bool TryGetValue(
          TKey key,
          out TValue value
          )
        { return this.dictionary.TryGetValue(key, out value); }

        public virtual int Count => this.dictionary.Count;

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys => this.dictionary.Keys;

        public ICollection<TValue> Values => this.dictionary.Values;
    }
}