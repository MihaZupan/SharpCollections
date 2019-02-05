using System;

namespace Benchmark
{
    public static class Helpers
    {
        public static void Shuffle<T>(this T[] array)
        {
            Random rng = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i);
                var t = array[j];
                array[j] = array[i];
                array[i] = t;
            }
        }
    }
}
