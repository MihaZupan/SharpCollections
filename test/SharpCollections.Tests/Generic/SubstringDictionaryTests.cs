using System;
using System.Collections.Generic;
using Xunit;

namespace SharpCollections.Generic
{
    public class SubstringDictionaryTests
    {
        [Fact]
        public void Simple()
        {
            SubstringDictionary<int> dictionary = new SubstringDictionary<int>();

            dictionary.Add("Hello world", 1);

            Assert.False(dictionary.TryGetSubstring("And then he said: 'Hello world'".AsSpan(18, 13), out KeyValuePair<string, int> value));

            Assert.True(dictionary.TryGetSubstring("And then he said: 'Hello world'".AsSpan(19, 11), out value));
            Assert.Equal("Hello world", value.Key);
            Assert.Equal(1, value.Value);
        }
    }
}
