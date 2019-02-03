using System;
using System.Collections.Generic;
using Xunit;

namespace SharpCollections.Generic
{
    public class CompactPrefixTreeTests
    {
        [Fact]
        public void Simple()
        {
            var tree = new CompactPrefixTree<int>
            {
                { "Hello", 1 },
                { "Hello world", 2 },
                { "Hello world!", 3 }
            };

            Assert.Equal(1, tree["Hello"]);
            Assert.Throws<KeyNotFoundException>(() => { int num = tree["hello"]; }); // Note the lowercase h

            Assert.Equal(2, tree["Hello world, today is a good day!"]);
            Assert.Equal(3, tree["Hello world!"]);
            Assert.Equal(1, tree["Hello World"]); // Note the capital W
            tree.Add("Hello World", 10);
            Assert.Equal(10, tree["Hello World"]); // Note the capital W

            tree["Hello"] = 5;
            Assert.Equal(5, tree["Hello neighbour"]);

            string text = "Hello world and everyone on it!";

            Assert.True(tree.TryMatch(text, 0, 12, out KeyValuePair<string, int> match));
            Assert.Equal("Hello world", match.Key);
            Assert.Equal(2, match.Value);

            tree.Add("one", 1);
            Assert.True(tree.TryMatch(text.AsSpan(21), out match));
            Assert.Equal("one", match.Key);
        }
    }
}
