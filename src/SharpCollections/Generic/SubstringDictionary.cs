// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpCollections.Generic
{
    public class SubstringDictionary<TValue>
    {
        private readonly Dictionary<int, KeyValuePair<string, TValue>> _dictionary;

        public int Count => _dictionary.Count;

        public SubstringDictionary(int capacity = 0)
        {
            _dictionary = new Dictionary<int, KeyValuePair<string, TValue>>(capacity);
        }
        public SubstringDictionary(ICollection<KeyValuePair<string, TValue>> input)
        {
            if (input == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input);

            _dictionary = new Dictionary<int, KeyValuePair<string, TValue>>(input.Count);
            foreach (var pair in input)
            {
                Add(in pair);
            }
        }

        public TValue this[string key]
        {
            get
            {
                if (TryGetValue(key, out KeyValuePair<string, TValue> pair))
                    return pair.Value;
                throw new KeyNotFoundException();
            }
            set
            {
                var pair = new KeyValuePair<string, TValue>(key, value);
                bool modified = TryInsert(in pair, InsertionBehavior.OverwriteExisting);
                Debug.Assert(modified);
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

        public void Remove(string key)
        {
#if NETCORE
            Remove(key.AsSpan());
#else
            Remove(key, 0, key.Length);
#endif
        }

#if NETCORE
        public void Remove(string text, int offset, int length)
            => Remove(text.AsSpan(offset, length));

        public void Remove(ReadOnlySpan<char> substring)
        {
            int hash = ComputeHash(substring);

            while (true)
            {
                if (_dictionary.TryGetValue(hash, out KeyValuePair<string, TValue> value))
                {
                    if (substring.Equals(value.Key.AsSpan(), StringComparison.Ordinal))
                    {
                        Remove(value.Key);
                        return;
                    }

                    // We have a collision, try looking at the next index (matched in Add)
                    hash++;
                    continue;
                }
                throw new KeyNotFoundException();
            }
        }
#else
        public void Remove(string text, int offset, int length)
        {
            if (text == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.text);
            if (offset < 0 || length < 0 || text.Length < offset + length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.offsetLength, ExceptionReason.InvalidOffsetLength);

            int hash = ComputeHash(text, offset, length) ^ Seed;

            while (true)
            {
                if (_dictionary.TryGetValue(hash, out KeyValuePair<string, TValue> value))
                {
                    if (value.Key.Length == length && text.IndexOf(value.Key, offset, length, StringComparison.Ordinal) == offset)
                    {
                        Remove(value.Key);
                        return;
                    }

                    // We have a collision, try looking at the next index (matched in Add)
                    hash++;
                    continue;
                }
                throw new KeyNotFoundException();
            }
        }
#endif

        public void Add(string key, TValue value)
        {
            var pair = new KeyValuePair<string, TValue>(key, value);
            TryInsert(in pair, InsertionBehavior.ThrowOnExisting);
        }

        public void Add(in KeyValuePair<string, TValue> pair)
            => TryInsert(in pair, InsertionBehavior.ThrowOnExisting);

        public bool TryAdd(string key, TValue value)
        {
            var pair = new KeyValuePair<string, TValue>(key, value);
            return TryInsert(in pair, InsertionBehavior.None);
        }

        public bool TryAdd(in KeyValuePair<string, TValue> pair)
            => TryInsert(in pair, InsertionBehavior.None);

        private bool TryInsert(in KeyValuePair<string, TValue> pair, InsertionBehavior behavior)
        {
            string key = pair.Key;
            if (key.Length == 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.key, ExceptionReason.String_Empty);

#if NETCORE
            int hash = ComputeHash(key.AsSpan());
#else
            int hash = ComputeHash(key, 0, key.Length);
#endif

            while (_dictionary.TryGetValue(hash, out KeyValuePair<string, TValue> collisionPair))
            {
                hash++; // We have a collision, try inserting at the next index (matched in TryGetSubstring)

                // Could also be a duplicate key
                if (key.Equals(collisionPair.Key, StringComparison.Ordinal))
                {
                    if (behavior == InsertionBehavior.None) return false;
                    if (behavior == InsertionBehavior.OverwriteExisting)
                    {
                        _dictionary[hash] = pair;
                        return true;
                    }
                    ThrowHelper.ThrowArgumentException(ExceptionArgument.key, ExceptionReason.DuplicateKey);
                    Debug.Fail("Should throw by now");
                }
            }

            _dictionary.Add(hash, pair);
            return true;
        }

        public bool TryGetValue(string key, out KeyValuePair<string, TValue> value)
        {
#if NETCORE
            return TryGetSubstring(key.AsSpan(), out value);
#else
            return TryGetSubstring(key, 0, key.Length, out value);
#endif
        }

#if NETCORE
        public bool TryGetSubstring(string source, int offset, int count, out KeyValuePair<string, TValue> value)
            => TryGetSubstring(source.AsSpan(offset, count), out value);

        public bool TryGetSubstring(ReadOnlySpan<char> substring, out KeyValuePair<string, TValue> value)
        {
            int hash = ComputeHash(substring);

            while (true)
            {
                if (_dictionary.TryGetValue(hash, out value))
                {
                    if (substring.Equals(value.Key.AsSpan(), StringComparison.Ordinal))
                        return true;

                    // We have a collision, try looking at the next index (matched in Add)
                    hash++;
                    continue;
                }
                return false;
            }
        }
#else
        public bool TryGetSubstring(string text, int offset, int length, out KeyValuePair<string, TValue> value)
        {
            if (text == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.text);
            if (offset < 0 || length < 0 || text.Length < offset + length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.offsetLength, ExceptionReason.InvalidOffsetLength);

            int hash = ComputeHash(text, offset, length) ^ Seed;

            while (true)
            {
                if (_dictionary.TryGetValue(hash, out value))
                {
                    if (value.Key.Length == length && text.IndexOf(value.Key, offset, length, StringComparison.Ordinal) == offset)
                        return true;

                    // We have a collision, try looking at the next index (matched in Add)
                    hash++;
                    continue;
                }
                return false;
            }
        }
#endif


        /// <summary>
        /// Currently a Fowler-Noll-Vo hash in 32-bit space, can be replaced by any other string hash
        /// <para>https://en.wikipedia.org/wiki/Fowler–Noll–Vo_hash_function#FNV_hash_parameters</para>
        /// </summary>
#if NETCORE
        private static int ComputeHash(ReadOnlySpan<char> substring)
        {
            unchecked
            {
                uint hash = 2166136261u; // Offset basis
                for (int i = 0; i < substring.Length; i++)
                {
                    hash = (hash ^ substring[i]) * 16777619u; // Prime
                }
                return (int)hash ^ Seed;
            }
        }
#else
        private static int ComputeHash(string substring, int offset, int count)
        {
            unchecked
            {
                uint hash = 2166136261u; // Offset basis
                for (int i = offset; i < offset + count; i++)
                {
                    hash = (hash ^ substring[i]) * 16777619u; // Prime
                }
                return (int)hash ^ Seed;
            }
        }
#endif

        private static readonly int Seed = new Random().Next(int.MinValue + 10, int.MaxValue - 10);
    }
}
