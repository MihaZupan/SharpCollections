# Compact Prefix Tree

Supports fast shortest, exact and longest prefix queries, while consuming less memory than traditional tries.

All operations return the matched key as well, which is useful when you do a lot of lookups on substrings.
It saves you the substring allocation for the lookup/use after the lookup.

Supports three main query operations: `TryMatchShortest`, `TryMatchExact`, `TryMatchLongest`

All these methods also have (offset, length), (offset) and (`ReadOnlySpan<char>`) overloads.

```csharp
using SharpCollections.Generic;

CompactPrefixTree<int> tree = new CompactPrefixTree<int>
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
```