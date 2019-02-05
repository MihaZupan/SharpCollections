using BenchmarkDotNet.Attributes;
using SharpCollections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Benchmark
{
    public class PrefixTreeTesting
    {
        private static readonly string[] SortedWords;
        private static readonly string[] MixedWords;

        private static readonly KeyValuePair<string, int>[] SortedPairs;
        private static readonly KeyValuePair<string, int>[] MixedPairs;

        private static readonly CompactPrefixTree<int> SortedPrefixTree;
        private static readonly CompactPrefixTree<int> MixedPrefixTree;

        static PrefixTreeTesting()
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

            for (int i = 0; i < 5; i++) MixedWords.Shuffle();
        }

        [Benchmark]
        public void Test()
        {
            for (int i = 0; i < MixedWords.Length; i++)
            {
                SortedPrefixTree.TryMatchLongest(MixedWords[i], out _);
            }
        }
    }
}
