using System;
using System.Collections.Generic;

namespace GenshinLibrary.Utility
{
    public static class EnumerableUtilities
    {
        private static readonly Random random = new Random();

        public static bool ContainsDuplicates<T>(this IEnumerable<T> items)
        {
            return ContainsDuplicates(items, EqualityComparer<T>.Default);
        }

        public static bool ContainsDuplicates<T>(this IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
        {
            var set = new HashSet<T>(equalityComparer);

            foreach (var item in items)
            {
                if (!set.Add(item))
                    return true;
            }

            return false;
        }

        public static T RandomElement<T>(this IReadOnlyList<T> items)
        {
            return items[random.Next(0, items.Count)];
        }
    }
}
