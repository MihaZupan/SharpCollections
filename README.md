# Sharp Collections [![Build Status](https://travis-ci.org/MihaZupan/SharpCollections.svg?branch=master)](https://travis-ci.org/MihaZupan/SharpCollections) [![Build status](https://ci.appveyor.com/api/projects/status/uwno7633b39ikdvn/branch/master?svg=true)](https://ci.appveyor.com/project/MihaZupan/sharpcollections/branch/master) [![Coverage Status](https://coveralls.io/repos/github/MihaZupan/SharpCollections/badge.svg?branch=master)](https://coveralls.io/github/MihaZupan/SharpCollections?branch=master) [![NuGet](https://img.shields.io/nuget/v/SharpCollections.svg)](https://www.nuget.org/packages/SharpCollections/) [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)][PayPalMe]

A set of specialized high-performance data structures, not available in the framework.

Compatible with Framework 3.5+, Standard 2.0+ and Core 2.1+.

Newer features (like spans) are available thanks to preprocessor directives.

Support for legacy platforms (<= Net 4.6) will be dropped, should they prove to complicate development.


## Currently available

#### Compact Prefix Tree

[`CompactPrefixTree<TValue>`](examples/CompactPrefixTree.md)

Available from Framework 3.5.

A highly memory-efficient and GC-friendly string prefix tree.

Used in the amazing [Markdig library](https://github.com/lunet-io/markdig)

#### Work Scheduler

[`WorkScheduler<T>`](examples/WorkScheduler.md)

Available from Standard 2.0.

A helper class that helps schedule work on your `TaskScheduler` of choice (`ThreadPool` by default).

Work is scheduled in parallel, but only one item from each bucket at a time.

#### Binary Heap

[`BinaryHeap<T>`](examples/BinaryHeap.md)

Available from Framework 3.5.

A simple generic binary heap implementation for types that implement `IComparable<T>`.


## License

This library is released under the [BSD-Clause 2 license][license].

TL;DR: Use however you want as long as you give attribution

## Donate

If you find yourself using this library, please consider donating!
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)][PayPalMe]

## Author

[Miha Zupan](https://github.com/MihaZupan)


[License]: https://raw.githubusercontent.com/MihaZupan/SharpCollections/master/license.txt
[PayPalMe]: https://www.paypal.me/MihaZupanSLO