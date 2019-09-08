// Copyright (c) 2011 John Sonmez

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Attribution: https://github.com/jsonmez/Defaultable-Dictionary/blob/master/DefaultableDictionary/DefaultableDictionary/DefaultableDictionary.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DefaultableDictionary
{
    public class DefaultableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> dictionary;
        private readonly TValue defaultValue;

        public DefaultableDictionary(IDictionary<TKey, TValue> dictionary, TValue defaultValue)
        {
            this.dictionary = dictionary;
            this.defaultValue = defaultValue;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            dictionary.Add(item);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Remove(item);
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return dictionary.IsReadOnly; }
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!dictionary.TryGetValue(key, out value))
                value = defaultValue;

            return true;
        }

        public TValue this[TKey key]
        {
            get
            {
                try
                {
                    return dictionary[key];
                }
                catch (KeyNotFoundException)
                {
                    return defaultValue;
                }
            }

            set { dictionary[key] = value; }
        }

        public ICollection<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>(dictionary.Values) {defaultValue};
                return values;
            }
        }
    }

    public static class DefaultableDictionaryExtensions
    {
        public static IDictionary<TKey, TValue> WithDefaultValue<TValue, TKey>(this IDictionary<TKey, TValue> dictionary, TValue defaultValue )
        {
            return new DefaultableDictionary<TKey, TValue>(dictionary, defaultValue);
        }
    }
}
