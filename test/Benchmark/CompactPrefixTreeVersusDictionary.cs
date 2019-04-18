using BenchmarkDotNet.Attributes;
using SharpCollections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class CompactPrefixTreeVersusDictionary
    {
        private static readonly string[] Words;
        private static readonly KeyValuePair<string, int>[] Pairs;
        private static readonly Dictionary<string, int> Dictionary_Ordinal;
        private static readonly Dictionary<string, int> Dictionary_IgnoreCase;
        private static readonly CompactPrefixTree<int> PrefixTree;
        private static readonly CompactPrefixTree<int> PrefixTree_IgnoreCase;

        static CompactPrefixTreeVersusDictionary()
        {
            string wordsPath = @"C:\MihaZupan\SharpCollections\test\Benchmark\words.txt";
            Words = File.ReadAllLines(wordsPath);
            for (int i = 0; i < 5; i++) Words.Shuffle(12345 + i);

            int count = 0;
            Pairs = Words.Select(w => new KeyValuePair<string, int>(w, w.Length)).ToArray();
            Pairs = Pairs.Where(_ => ++count % 1000 == 0).ToArray();

            PrefixTree = new CompactPrefixTree<int>(Pairs, ignoreCase: false);
            PrefixTree_IgnoreCase = new CompactPrefixTree<int>(Pairs, ignoreCase: true);
            Dictionary_Ordinal = new Dictionary<string, int>(Pairs, StringComparer.Ordinal);
            Dictionary_IgnoreCase = new Dictionary<string, int>(Pairs, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < 5; i++) Words.Shuffle(12345 + i);
        }

        [Benchmark]
        public void Init_PrefixTree()
        {
            new CompactPrefixTree<int>(Pairs, ignoreCase: false);
        }
        [Benchmark]
        public void Init_PrefixTree_IgnoreCase()
        {
            new CompactPrefixTree<int>(Pairs, ignoreCase: true);
        }
        [Benchmark]
        public void Init_Dictionary_Ordinal()
        {
            new Dictionary<string, int>(Pairs, StringComparer.Ordinal);
        }
        [Benchmark]
        public void Init_Dictionary_OrdinalIgnoreCase()
        {
            new Dictionary<string, int>(Pairs, StringComparer.OrdinalIgnoreCase);
        }

        [Benchmark]
        public void Lookup_PrefixTree()
        {
            for (int i = 0; i < Words.Length; i++)
            {
                PrefixTree.TryMatchExact(Words[i], out _);
            }
        }
        [Benchmark]
        public void Lookup_PrefixTree_IgnoreCase()
        {
            for (int i = 0; i < Words.Length; i++)
            {
                PrefixTree_IgnoreCase.TryMatchExact(Words[i], out _);
            }
        }
        [Benchmark]
        public void Lookup_Dictionary_Ordinal()
        {
            for (int i = 0; i < Words.Length; i++)
            {
                Dictionary_Ordinal.TryGetValue(Words[i], out _);
            }
        }
        [Benchmark]
        public void Lookup_Dictionary_OrdinalIgnoreCase()
        {
            for (int i = 0; i < Words.Length; i++)
            {
                Dictionary_IgnoreCase.TryGetValue(Words[i], out _);
            }
        }
    }
}
