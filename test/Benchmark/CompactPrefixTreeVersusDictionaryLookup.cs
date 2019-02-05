using BenchmarkDotNet.Attributes;
using SharpCollections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class CompactPrefixTreeVersusDictionaryLookup
    {
        private static readonly string[] SortedWords;
        private static readonly string[] MixedWords;

        private static readonly KeyValuePair<string, int>[] SortedPairs;
        private static readonly KeyValuePair<string, int>[] MixedPairs;

        private static readonly Dictionary<string, int> SortedDictionary;
        private static readonly Dictionary<string, int> MixedDictionary;
        private static readonly CompactPrefixTree<int> SortedPrefixTree;
        private static readonly CompactPrefixTree<int> MixedPrefixTree;

        static CompactPrefixTreeVersusDictionaryLookup()
        {
            string wordsPath = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(typeof(CompactPrefixTreeVersusDictionaryLookup).Assembly.Location),
                    "../../../../../../../words.txt"));

            SortedWords = File.ReadAllLines(wordsPath);
            Array.Sort(SortedWords, StringComparer.Ordinal);

            MixedWords = (string[])SortedWords.Clone();
            for (int i = 0; i < 5; i++) MixedWords.Shuffle();

            int count = 0;
            SortedPairs = SortedWords.Select(w => new KeyValuePair<string, int>(w, w.Length)).ToArray();
            SortedPairs = SortedPairs.Where(_ => ++count % 100 == 0).ToArray();
            count = 0;
            MixedPairs = MixedWords.Select(w => new KeyValuePair<string, int>(w, w.Length)).ToArray();
            MixedPairs = MixedPairs.Where(_ => ++count % 100 == 0).ToArray();

            SortedPrefixTree = new CompactPrefixTree<int>(SortedPairs);
            MixedPrefixTree = new CompactPrefixTree<int>(MixedPairs);
            SortedDictionary = new Dictionary<string, int>(SortedPairs, StringComparer.Ordinal);
            MixedDictionary = new Dictionary<string, int>(MixedPairs, StringComparer.Ordinal);

            for (int i = 0; i < 5; i++) MixedWords.Shuffle();
        }

        // Init speed
#if false

        [Benchmark(Baseline = true)]
        public void OrdinalDictionaryInit_Ctor_Sorted()
        {
            _ = new Dictionary<string, int>(SortedPairs, StringComparer.Ordinal);
        }

        [Benchmark]
        public void OrdinalDictionaryInit_Ctor_Mixed()
        {
            _ = new Dictionary<string, int>(MixedPairs, StringComparer.Ordinal);
        }

        [Benchmark]
        public void OrdinalDictionaryInit_Loop_Sorted()
        {
            var dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < SortedPairs.Length; i++)
            {
                var pair = SortedPairs[i];
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        [Benchmark]
        public void OrdinalDictionaryInit_Loop_Mixed()
        {
            var dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < MixedPairs.Length; i++)
            {
                var pair = MixedPairs[i];
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        [Benchmark]
        public void DictionaryInit_Ctor_Sorted()
        {
            _ = new Dictionary<string, int>(SortedPairs);
        }

        [Benchmark]
        public void DictionaryInit_Ctor_Mixed()
        {
            _ = new Dictionary<string, int>(MixedPairs);
        }

        [Benchmark]
        public void DictionaryInit_Loop_Sorted()
        {
            var dictionary = new Dictionary<string, int>();
            for (int i = 0; i < SortedPairs.Length; i++)
            {
                var pair = SortedPairs[i];
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        [Benchmark]
        public void DictionaryInit_Loop_Mixed()
        {
            var dictionary = new Dictionary<string, int>();
            for (int i = 0; i < MixedPairs.Length; i++)
            {
                var pair = MixedPairs[i];
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        [Benchmark]
        public void PrefixTreeInit_Ctor_Sorted()
        {
            _ = new CompactPrefixTree<int>(SortedPairs);
        }

        [Benchmark]
        public void PrefixTreeInit_Ctor_Mixed()
        {
            _ = new CompactPrefixTree<int>(MixedPairs);
        }

        [Benchmark]
        public void PrefixTreeInit_Loop_Sorted()
        {
            var tree = new CompactPrefixTree<int>();
            for (int i = 0; i < SortedPairs.Length; i++)
            {
                var pair = SortedPairs[i];
                tree.Add(pair.Key, pair.Value);
            }
        }

        [Benchmark]
        public void PrefixTreeInit_Loop_Mixed()
        {
            var tree = new CompactPrefixTree<int>();
            for (int i = 0; i < MixedPairs.Length; i++)
            {
                var pair = MixedPairs[i];
                tree.Add(pair.Key, pair.Value);
            }
        }

#else

        // Lookup speed

        [Benchmark(Baseline = true)]
        public void SortedOrdinalDict_SortedLookup()
        {
            for (int i = 0; i < SortedWords.Length; i++)
            {
                SortedDictionary.TryGetValue(SortedWords[i], out _);
            }
        }

        [Benchmark]
        public void SortedOrdinalDict_MixedLookup()
        {
            for (int i = 0; i < MixedWords.Length; i++)
            {
                SortedDictionary.TryGetValue(MixedWords[i], out _);
            }
        }

        [Benchmark]
        public void MixedOrdinalDict_SortedLookup()
        {
            for (int i = 0; i < SortedWords.Length; i++)
            {
                MixedDictionary.TryGetValue(SortedWords[i], out _);
            }
        }

        [Benchmark]
        public void MixedOrdinalDict_MixedLookup()
        {
            for (int i = 0; i < MixedWords.Length; i++)
            {
                MixedDictionary.TryGetValue(MixedWords[i], out _);
            }
        }

        [Benchmark]
        public void SortedPrefixTree_SortedLookup()
        {
            for (int i = 0; i < SortedWords.Length; i++)
            {
                SortedPrefixTree.TryMatchExact(SortedWords[i], out _);
            }
        }

        [Benchmark]
        public void SortedPrefixTree_MixedLookup()
        {
            for (int i = 0; i < MixedWords.Length; i++)
            {
                SortedPrefixTree.TryMatchExact(MixedWords[i], out _);
            }
        }

        [Benchmark]
        public void MixedPrefixTree_SortedLookup()
        {
            for (int i = 0; i < SortedWords.Length; i++)
            {
                MixedPrefixTree.TryMatchExact(SortedWords[i], out _);
            }
        }

        [Benchmark]
        public void MixedPrefixTree_MixedLookup()
        {
            for (int i = 0; i < MixedWords.Length; i++)
            {
                MixedPrefixTree.TryMatchExact(MixedWords[i], out _);
            }
        }
#endif
    }
}
