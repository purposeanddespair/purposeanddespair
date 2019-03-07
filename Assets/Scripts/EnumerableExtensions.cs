using System;
using System.Linq;
using System.Collections.Generic;

namespace NetGame
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Except<T> (this IEnumerable<T> set, T exceptValue)
        {
            return set.Except(new T[] { exceptValue });
        }
    }
}
