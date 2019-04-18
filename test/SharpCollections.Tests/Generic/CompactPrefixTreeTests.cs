using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SharpCollections.Generic
{
    public class CompactPrefixTreeTests
    {
        [Fact]
        public void ImplementsInterfaces()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "Hello", 1 },
                { "World", 2 }
            };

            var dictionary = tree as IReadOnlyDictionary<string, int>;
            var list = tree as IReadOnlyList<KeyValuePair<string, int>>;
            Assert.NotNull(dictionary);
            Assert.NotNull(list);

            Assert.Equal(dictionary.Keys, new[] { "Hello", "World" });
            Assert.Equal(dictionary.Values, new[] { 1, 2 });

            Assert.True(dictionary.ContainsKey("Hello"));
            Assert.False(dictionary.ContainsKey("Foo"));

            Assert.True(dictionary.TryGetValue("Hello", out int value));
            Assert.Equal(1, value);

            Assert.Equal(list.Count, dictionary.Count);
            Assert.Equal(new KeyValuePair<string, int>("Hello", 1), list[0]);
            Assert.Equal(new KeyValuePair<string, int>("World", 2), list[1]);

            Assert.Equal(list, new[] { new KeyValuePair<string, int>("Hello", 1), new KeyValuePair<string, int>("World", 2) });
            Assert.Equal(dictionary.ToDictionary(p => p.Key, p => p.Value), new Dictionary<string, int>() { { "Hello", 1 }, { "World", 2 } });

            using (var e = tree.GetEnumerator())
            {
                Assert.True(e.MoveNext());
                Assert.Equal(1, e.Current.Value);
                Assert.True(e.MoveNext());
                Assert.Equal(2, e.Current.Value);
                Assert.False(e.MoveNext());
                Assert.Throws<IndexOutOfRangeException>(() => { var element = e.Current; });
                e.Reset();
                Assert.True(e.MoveNext());
                Assert.Equal(1, e.Current.Value);
            }
        }

        [Fact]
        public void ValidatesInput()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>();

            Assert.Throws<ArgumentNullException>(() => { new CompactPrefixTree<int>(null); });

            Assert.Throws<ArgumentNullException>(() => { tree[null] = 123; });
            Assert.Throws<ArgumentNullException>(() => { int num = tree[null]; });
            Assert.Throws<ArgumentNullException>(() => { tree.Add(null, 123); });
            Assert.Throws<ArgumentNullException>(() => { tree.TryAdd(null, 123); });
            Assert.Throws<ArgumentNullException>(() => { tree.TryMatchShortest(null, out _); });
            Assert.Throws<ArgumentNullException>(() => { tree.TryMatchExact(null, out _); });
            Assert.Throws<ArgumentNullException>(() => { tree.TryMatchLongest(null, out _); });
            Assert.Throws<ArgumentNullException>(() => { tree.TryGetValue(null, out _); });
            Assert.Throws<ArgumentNullException>(() => { tree.ContainsKey(null); });

            Assert.Throws<ArgumentOutOfRangeException>(() => { tree[""] = 123; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.Add("", 123); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryAdd("", 123); });

            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchShortest("Foo", -1, 0, out _); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchShortest("Foo", 0, -1, out _); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchShortest("Foo", 3, 3, out _); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchExact("Foo", -1, 0, out _); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchExact("Foo", 0, -1, out _); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchExact("Foo", 3, 3, out _); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchLongest("Foo", -1, 0, out _); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchLongest("Foo", 0, -1, out _); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TryMatchLongest("Foo", 3, 3, out _); });

            Assert.Throws<IndexOutOfRangeException>(() => { var match = tree[-1]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var match = tree[5]; });

            tree.Capacity = 0;
            tree.TreeCapacity = 0;
            tree.Add("Hello", 123);
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.Capacity = 0; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { tree.TreeCapacity = 0; });
        }

        [Fact]
        public void DoesOrdinalComparrison()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "Hello", 1 }
            };

            Assert.Equal(1, tree["Hello"]);
            Assert.Throws<KeyNotFoundException>(() => { int num = tree["hello"]; });
        }

        [Fact]
        public void SupportsUnicode()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "ümlaüt", 12345 }
            };
            Assert.False(tree.ContainsKey("umlaüt"));
            Assert.True(tree.ContainsKey("ümlaüt"));
            Assert.False(tree.ContainsKey("Ümlaüt"));

            // We don't support Right-To-Left, don't push it
        }

        [Fact]
        public void ModifiesValue()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "Hello", 1 }
            };

            Assert.Equal(1, tree["Hello"]);

            tree["Hello"] = 2;
            Assert.Equal(2, tree["Hello"]);
        }
        
        [Fact]
        public void MatchesLongest()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "foo", 1 }, { "foo bar", 2 }
            };

            Assert.True(tree.TryMatchLongest("foo bar", out KeyValuePair<string, int> match));
            Assert.Equal(match, new KeyValuePair<string, int>("foo bar", 2));

            Assert.True(tree.TryMatchLongest("foo ba", out match));
            Assert.Equal(match, new KeyValuePair<string, int>("foo", 1));

            Assert.True(tree.TryMatchLongest("foo", out match));
            Assert.Equal(match, new KeyValuePair<string, int>("foo", 1));

            Assert.False(tree.TryMatchLongest("fo", out match));
        }

        [Fact]
        public void MatchesExact()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "foo", 1 }, { "foo bar", 2 }
            };

            Assert.True(tree.TryMatchExact("foo bar", out KeyValuePair<string, int> match));
            Assert.Equal(match, new KeyValuePair<string, int>("foo bar", 2));

            Assert.False(tree.TryMatchExact("foo ba", out match));

            Assert.True(tree.TryMatchExact("foo", out match));
            Assert.Equal(match, new KeyValuePair<string, int>("foo", 1));

            Assert.False(tree.TryMatchExact("fo", out match));
        }

        [Fact]
        public void MatchesShortest()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "foo", 1 }, { "foo bar", 2 }
            };

            Assert.True(tree.TryMatchShortest("foo bar", out KeyValuePair<string, int> match));
            Assert.Equal(match, new KeyValuePair<string, int>("foo", 1));

            Assert.True(tree.TryMatchShortest("foo ba", out match));
            Assert.Equal(match, new KeyValuePair<string, int>("foo", 1));

            Assert.True(tree.TryMatchShortest("foo", out match));
            Assert.Equal(match, new KeyValuePair<string, int>("foo", 1));

            Assert.False(tree.TryMatchShortest("fo", out match));
        }

        [Fact]
        public void SetsCorrectCapacity()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>();
            Assert.Equal(0, tree.Capacity);
            Assert.Equal(0, tree.TreeCapacity);

            tree = new CompactPrefixTree<int>
            {
                { "foo", 1 }, { "foo bar", 2 }, { "bar", 3 }, { "Bar", 4 }, { "foobar", 5 }
            };
            Assert.Equal(5, tree.Count);
            Assert.Equal(8, tree.Capacity); // Grows by powers of 2
            Assert.Equal(7, tree.TreeSize); // 'f', 'fo', 'foo', 'foo bar', 'foobar', 'bar', 'Bar'
            Assert.Equal(12, tree.TreeCapacity); // 1, 3, 6, 12
            // 1 => 3 happens because internally when adding "foo bar", we call Ensure(1 + 2) to accomodate two leaf nodes
            // After that the capacity is doubled
            Assert.Equal(2, tree.ChildrenCount); // only "foo bar" and "foobar" will force a child (ChildrenCount is double the amount of actual children entries)
            Assert.Equal(2, tree.ChildrenCapacity);

            tree = new CompactPrefixTree<int>(new[]
            {
                new KeyValuePair<string, int>("foo", 1),
                new KeyValuePair<string, int>("foo bar", 2),
                new KeyValuePair<string, int>("bar", 3),
                new KeyValuePair<string, int>("Bar", 4),
                new KeyValuePair<string, int>("foobar", 5)
            });
            Assert.Equal(5, tree.Count);
            Assert.Equal(5, tree.Capacity); // Set correctly since it was known at construction-time
            Assert.Equal(7, tree.TreeSize);
            Assert.Equal(10, tree.TreeCapacity); // Set to 2 * input.Count in the constructor
            Assert.Equal(2, tree.ChildrenCount);
            Assert.Equal(10, tree.ChildrenCapacity); // Initially set to 2 * input.Count
        }

        [Fact]
        public void MimicsDictionaryExceptionsBehavior()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>();

            Assert.Throws<KeyNotFoundException>(() => { int num = tree["foo"]; });
            Assert.Throws<KeyNotFoundException>(() => { int num = dictionary["foo"]; });

            tree["foo"] = 1;
            Assert.Equal(1, tree["foo"]);

            dictionary["foo"] = 1;
            Assert.Equal(1, dictionary["foo"]);

            tree["foo"] = 2;
            Assert.Equal(2, tree["foo"]);

            dictionary["foo"] = 2;
            Assert.Equal(2, dictionary["foo"]);

            Assert.Throws<ArgumentException>(() => { tree.Add("foo", 1); });
            Assert.Equal(2, tree["foo"]);

            Assert.Throws<ArgumentException>(() => { dictionary.Add("foo", 1); });
            Assert.Equal(2, dictionary["foo"]);
        }

        [Fact]
        public void AcceptsSubstrings()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "Hell", 1 },
                { "Hello", 2 },
                { "Hello world", 3 },
                { "Hello world!", 4 },
                { "wor", 5 },
                { "world", 6 },
                { " ", 7 }
            };

            //             0         1
            //             012345678901
            string text = "Hello world!";

            Assert.False(tree.TryMatchExact(text, 0, 3, out var match));

            Assert.True(tree.TryMatchExact(text, 0, 4, out match));
            Assert.Equal("Hell", match.Key);
            Assert.True(tree.TryMatchExact(text, 0, 5, out match));
            Assert.Equal("Hello", match.Key);
            Assert.True(tree.TryMatchShortest(text, 5, 5, out match));
            Assert.Equal(" ", match.Key);
            Assert.True(tree.TryMatchShortest(text, 0, 5, out match));
            Assert.Equal("Hell", match.Key);
            Assert.True(tree.TryMatchLongest(text, 0, 5, out match));
            Assert.Equal("Hello", match.Key);
            Assert.True(tree.TryMatchShortest(text, out match));
            Assert.Equal("Hell", match.Key);
            Assert.True(tree.TryMatchLongest(text, 6, out match));
            Assert.Equal("world", match.Key);
            Assert.True(tree.TryMatchShortest(text, 6, out match));
            Assert.Equal("wor", match.Key);
            Assert.True(tree.TryMatchLongest(text, 6, 4, out match));
            Assert.Equal("wor", match.Key);
            Assert.True(tree.TryMatchLongest(text, 0, text.Length, out match));
            Assert.Equal("Hello world!", match.Key);
            Assert.True(tree.TryMatchLongest(text, 0, text.Length - 1, out match));
            Assert.Equal("Hello world", match.Key);
            Assert.True(tree.TryMatchLongest(text, 0, text.Length - 2, out match));
            Assert.Equal("Hello", match.Key);

            Assert.False(tree.TryMatchExact(text, 6, out match));
            tree.Add("world!", 7);
            Assert.True(tree.TryMatchExact(text, 6, out match));
            Assert.Equal("world!", match.Key);
        }

        [Fact]
        public void AcceptsSpans()
        {
#if NETCORE
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>
            {
                { "Hell", 1 },
                { "Hello", 2 },
                { "Hello world", 3 },
                { "Hello world!", 4 },
                { "wor", 5 },
                { "world", 6 },
                { " ", 7 }
            };

            //             0         1
            //             012345678901
            string text = "Hello world!";

            Assert.False(tree.TryMatchExact(text.AsSpan(0, 3), out var match));

            Assert.True(tree.TryMatchExact(text.AsSpan(0, 4), out match));
            Assert.Equal("Hell", match.Key);
            Assert.True(tree.TryMatchExact(text.AsSpan(0, 5), out match));
            Assert.Equal("Hello", match.Key);
            Assert.True(tree.TryMatchShortest(text.AsSpan(5, 5), out match));
            Assert.Equal(" ", match.Key);
            Assert.True(tree.TryMatchShortest(text.AsSpan(0, 5), out match));
            Assert.Equal("Hell", match.Key);
            Assert.True(tree.TryMatchLongest(text.AsSpan(0, 5), out match));
            Assert.Equal("Hello", match.Key);
            Assert.True(tree.TryMatchShortest(text, out match));
            Assert.Equal("Hell", match.Key);
            Assert.True(tree.TryMatchLongest(text.AsSpan(6), out match));
            Assert.Equal("world", match.Key);
            Assert.True(tree.TryMatchShortest(text.AsSpan(6), out match));
            Assert.Equal("wor", match.Key);
            Assert.True(tree.TryMatchLongest(text.AsSpan(6, 4), out match));
            Assert.Equal("wor", match.Key);
            Assert.True(tree.TryMatchLongest(text.AsSpan(), out match));
            Assert.Equal("Hello world!", match.Key);
            Assert.True(tree.TryMatchLongest(text.AsSpan(0, text.Length - 1), out match));
            Assert.Equal("Hello world", match.Key);
            Assert.True(tree.TryMatchLongest(text.AsSpan(0, text.Length - 2), out match));
            Assert.Equal("Hello", match.Key);

            Assert.False(tree.TryMatchExact(text.AsSpan(6), out match));
            Assert.Throws<KeyNotFoundException>(() => { match = tree[text.AsSpan(6)]; });
            tree.Add("world!", 7);
            Assert.True(tree.TryMatchExact(text.AsSpan(6), out match));
            Assert.Equal("world!", match.Key);
            match = tree[text.AsSpan(6)];
            Assert.Equal("world!", match.Key);
#endif
        }

        [Fact]
        public void SupportsCaseInsensitivity()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>(ignoreCase: true)
            {
                { "Hell", 1 },
                { "Hello", 2 },
                { "Hello world", 3 },
                { "Hello world!", 4 },
                { "wor", 5 },
                { "world", 6 },
                { " ", 7 }
            };

            //             0         1
            //             012345678901
            string text = "HeLLo woRld!";

            Assert.True(tree.TryMatchLongest(text, out var match));
            Assert.Equal("Hello world!", match.Key);
            Assert.True(tree.TryMatchLongest(text, 6, out match));
            Assert.Equal("world", match.Key);
            Assert.True(tree.ContainsKey("hello"));
            int value = tree["wOr"];
            Assert.Equal(5, value);

#if NETCORE

            Assert.False(tree.TryMatchExact(text.AsSpan(0, 3), out match));

            Assert.True(tree.TryMatchExact(text.AsSpan(0, 4), out match));
            Assert.Equal("Hell", match.Key);
            Assert.True(tree.TryMatchExact(text.AsSpan(0, 5), out match));
            Assert.Equal("Hello", match.Key);

            Assert.False(tree.TryMatchExact(text.AsSpan(6), out match));
            Assert.Throws<KeyNotFoundException>(() => { match = tree[text.AsSpan(6)]; });
            tree.Add("world!", 7);
            Assert.True(tree.TryMatchExact(text.AsSpan(6), out match));
            Assert.Equal("world!", match.Key);
            match = tree[text.AsSpan(6)];
            Assert.Equal("world!", match.Key);
#endif
        }

        [Fact]
        public void UsesAllRelevantCodePaths()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>()
            {
                { "A", 1 }, { "Abc", 2 }, { "Aeiou", 3 },
                { "fooob", 4 }, { "foobar1", 5 }, { "foobar2", 6 }
            };

            Assert.False(tree.ContainsKey("a"));
            Assert.True(tree.ContainsKey("A"));
            Assert.False(tree.TryGetValue("a", out _));
            Assert.True(tree.TryGetValue("A", out _));
            Assert.False(tree.TryMatchShortest("a", out _));
            Assert.True(tree.TryMatchShortest("A", out _));
            Assert.False(tree.TryMatchExact("a", out _));
            Assert.True(tree.TryMatchExact("A", out _));
            Assert.False(tree.TryMatchLongest("a", out _));
            Assert.True(tree.TryMatchLongest("A", out _));

            Assert.True(tree.TryMatchLongest("Aeiou and something", out KeyValuePair<string, int> match));
            Assert.Equal(3, match.Value);

            Assert.True(tree.TryMatchExact("Abc", out match));
            Assert.Equal(2, match.Value);

            Assert.True(tree.TryMatchShortest("Aeiou and something", out match));
            Assert.Equal("A", match.Key);
            Assert.Equal(1, match.Value);

            Assert.True(tree.TryMatchLongest("foobar123", out match));
            Assert.Equal("foobar1", match.Key);
            Assert.Equal(5, match.Value);

            Assert.False(tree.TryMatchExact("foobar123", out match));

            Assert.True(tree.TryMatchShortest("foobar123", out match));
            Assert.Equal("foobar1", match.Key);
            Assert.Equal(5, match.Value);

            tree.Add("full", 10);

            Assert.True(tree.TryMatchLongest("fullbar123", out match));
            Assert.Equal("full", match.Key);
            Assert.Equal(10, match.Value);

            Assert.False(tree.TryMatchExact("fullbar123", out match));

            Assert.True(tree.TryMatchShortest("fullbar123", out match));
            Assert.Equal("full", match.Key);
            Assert.Equal(10, match.Value);

            tree.Add(new KeyValuePair<string, int>("Hello", 321));
            Assert.True(tree.ContainsKey("Hello"));
            tree.TryAdd(new KeyValuePair<string, int>("Hello", 123));
            Assert.Equal(321, tree["Hello"]);

            tree.Add("Hell", 123);
            Assert.Equal(321, tree["Hello"]);
            Assert.Equal(123, tree["Hell"]);

            tree.Add("aa", 2);
            tree.Add("ab", 3);
            tree.Add("ac", 4);
            tree.Add("ad", 5);
            tree.Add("ae", 6);

            Assert.False(tree.TryMatchLongest("af", out match));
            Assert.False(tree.TryMatchShortest("af", out match));

            Assert.Equal(5, tree["ad"]);
            Assert.Equal(6, tree["ae"]);

            tree.TryAdd("af", 7);
            Assert.Equal(7, tree["af"]);

            Assert.False(tree.ContainsKey("ag"));

            Assert.False(tree.TryAdd("A", 2));

            tree.Add("something", 123);
            tree.Add("soms", 789);
            tree.Add("some", 456);

            Assert.False(tree.ContainsKey("son"));
            Assert.True(tree.ContainsKey("soms"));
        }

        [Fact]
        public void ExampleTest()
        {
            CompactPrefixTree<int> tree = new CompactPrefixTree<int>(ignoreCase: false)
            {
                { "Hell", 1 },
                { "Hello", 2 },
                { "Hello world", 3 },
                { "Hello world!", 4 },
                { "world", 5 }
            };

            if (tree.TryMatchLongest("Hello everyone!", out KeyValuePair<string, int> match))
                Console.WriteLine("Matched {0}: {1}", match.Key, match.Value); // Hello, 2

            if (tree.TryMatchExact("Hello ", out match))
                Console.WriteLine("This is not gonna happen");

            if (tree.TryMatchLongest("Hello ", out match))
                Console.WriteLine("But this will: " + match.Key); // Hello

            if (tree.TryMatchShortest("Hello ", out match))
                Console.WriteLine("So will this: " + match.Key); // Hell

            // You can read/set/add values the same way you would in a dictionary
            tree["world"] = 123;
            tree["second world"] = 321;
            Console.WriteLine(tree["world"]);

            // Or add them explicitly
            tree.Add("Foo", 1); // Will throw if already present
            tree.TryAdd("Bar", 2); // Will return false if already present

            // You can access key/value pairs by index
            match = tree[3]; // Hello world!, 4

            // Only downside of this data structure is not being able to remove elements
            // tree.Remove("key");
        }
    }
}
