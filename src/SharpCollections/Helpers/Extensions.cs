#if NETCORE
using System;
using System.Linq;

namespace SharpCollections.Helpers
{
    internal static class Extensions
    {
        public static bool Equals(this ReadOnlySpan<char> a, ReadOnlySpan<char> b, bool ignoreCase)
        {
            if (ignoreCase) return a.Equals(b, StringComparison.OrdinalIgnoreCase);
            else return a.SequenceEqual(b);
        }
    }
}
#endif
