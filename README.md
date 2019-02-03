# Sharp Collections

A set of specialized high-performance collections, not available in the framework.


## Currently available collections

#### Compact Prefix Tree

[`CompactPrefixTree<TValue>`](examples/CompactPrefixTree.md)

A highly memory-efficient string prefix tree

#### Substring Dictionary

[`SubstringDictionary<TValue>`](examples/SubstringDictionary.md)

A `<substring, TValue>` dictionary for ordinal lookups on substrings, but without any substring allocations

#### Readonly Substring Dictionary

[`ReadonlySubstringDictionary<TValue>`](examples/SubstringDictionary.md)

A readonly variant of `CompactPrefixTree<TValue>` with optimized lookups for substrings with invalid lengths


## License

This library is released under the [BSD-Clause 2 license][license]

## Author

[Miha Zupan](https://github.com/MihaZupan)


[License]: https://raw.githubusercontent.com/MihaZupan/SharpCollections/master/license.txt