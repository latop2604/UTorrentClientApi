using System;
using System.Collections.Generic;

namespace UTorrent.Api
{
    internal static class ListHelper
    {
        //internal static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> source)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    AddRangeIfNotNull(collection, source);
        //}

        //internal static void AddIfNotNull<T>(this ICollection<T> collection, T item)
        //{
        //    if (collection == null)
        //        throw new ArgumentNullException("collection");

        //    if (item == null)
        //        return;

        //    collection.Add(item);
        //}

        internal static void AddRangeIfNotNull<T>(this ICollection<T> collection, IEnumerable<T> source)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (source == null)
                return;

            foreach (T item in source)
            {
                collection.Add(item);
            }
        }
    }
}
