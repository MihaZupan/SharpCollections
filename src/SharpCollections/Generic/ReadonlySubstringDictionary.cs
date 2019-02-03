// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpCollections.Generic
{
    public class ReadonlySubstringDictionary<TValue>
    {
        public readonly int LongestEntry;
        private readonly BitArray _availableLengths;
        private readonly SubstringDictionary<TValue> _dictionary;

        public ReadonlySubstringDictionary(ICollection<KeyValuePair<string, TValue>> input)
        {
            if (input == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input);

            _dictionary = new SubstringDictionary<TValue>(input.Count);

            foreach (var pair in input)
            {
                _dictionary.Add(in pair);
                LongestEntry = Math.Max(LongestEntry, pair.Key.Length);
            }

            _availableLengths = new BitArray(LongestEntry + 1);
            foreach (var pair in input)
                _availableLengths.Set(pair.Key.Length, true);
        }

        public TValue this[string key]
        {
            get
            {
                if (TryGetValue(key, out KeyValuePair<string, TValue> pair))
                    return pair.Value;
                throw new KeyNotFoundException();
            }
        }

#if NETCORE
        public KeyValuePair<string, TValue> this[ReadOnlySpan<char> substring]
        {
            get
            {
                if (TryGetSubstring(substring, out KeyValuePair<string, TValue> pair))
                    return pair;
                throw new KeyNotFoundException();
            }
        }
#endif
        public bool TryGetValue(string key, out KeyValuePair<string, TValue> value)
        {
#if NETCORE
            return TryGetSubstring(key.AsSpan(), out value);
#else
            return TryGetSubstring(key, 0, key.Length, out value);
#endif
        }

        public bool TryGetSubstring(string text, int offset, int length, out KeyValuePair<string, TValue> value)
        {
            value = default;
            if (length < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length, ExceptionReason.NegativeLength);

            if (length > LongestEntry || !_availableLengths.Get(length))
                return false;

#if NETCORE
            return _dictionary.TryGetSubstring(text.AsSpan(offset, length), out value);
#else
            return _dictionary.TryGetSubstring(text, offset, length, out value);
#endif
        }
#if NETCORE
        public bool TryGetSubstring(ReadOnlySpan<char> substring, out KeyValuePair<string, TValue> value)
        {
            value = default;

            if (substring.Length > LongestEntry || !_availableLengths.Get(substring.Length))
                return false;

            return _dictionary.TryGetSubstring(substring, out value);
        }
#endif
    }
}
