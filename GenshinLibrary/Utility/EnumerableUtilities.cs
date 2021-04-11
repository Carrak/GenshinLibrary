using System;
using System.Collections.Generic;

namespace GenshinLibrary.Utility
{
    public static class EnumerableUtilities
    {
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

        public static T RandomElement<T>(this IReadOnlyList<T> items, Random random)
        {
            return items[random.Next(0, items.Count)];
        }
    }
}
