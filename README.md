# Sharp Collections

A set of specialized high-performance collections, not available in the framework.

Compatible with Net 3.5+, Standard 2.0+ and Core 2.1+.

As the primary focus of this library is performance, not all functionality will be available on legacy platforms.
Newer features (like spans) are available thanks to preprocessor directives (use .Net Core if possible).


## Currently available collections

#### Compact Prefix Tree

[`CompactPrefixTree<TValue>`](examples/CompactPrefixTree.md)

A highly memory-efficient string prefix tree

#### Substring Dictionary

[`SubstringDictionary<TValue>`](examples/SubstringDictionary.md)

A `<substring, TValue>` dictionary for ordinal lookups on substrings, but with 0 substring allocations

#### Readonly Substring Dictionary

[`ReadonlySubstringDictionary<TValue>`](examples/ReadonlySubstringDictionary.md)

A readonly variant of `CompactPrefixTree<TValue>` with optimized lookups for substrings with invalid lengths


## License

This library is released under the [BSD-Clause 2 license][license]

## Author

[Miha Zupan](https://github.com/MihaZupan)


[License]: https://raw.githubusercontent.com/MihaZupan/SharpCollections/master/license.txt