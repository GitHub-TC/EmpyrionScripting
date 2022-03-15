using System;
using System.Collections.Generic;

namespace EmpyrionScripting.Interface
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this T[] aArray, Action<T> aAction)
        {
            if (aArray == null) return null;

            foreach (var item in aArray) aAction(item);
            return aArray;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> aArray, Action<T> aAction)
        {
            if (aArray == null) return null;

            foreach (var item in aArray) aAction(item);
            return aArray;
        }
        public static Dictionary<TKey, TElement> SafeToDictionary<TSource, TKey, TElement>(
              this IEnumerable<TSource> source,
              Func<TSource, TKey> keySelector,
              Func<TSource, TElement> elementSelector,
              IEqualityComparer<TKey> comparer = null)
        {
            var dictionary = new Dictionary<TKey, TElement>(comparer);

            if (source == null)
            {
                return dictionary;
            }

            foreach (TSource element in source)
            {
                dictionary[keySelector(element)] = elementSelector(element);
            }

            return dictionary;
        }
    }
}
