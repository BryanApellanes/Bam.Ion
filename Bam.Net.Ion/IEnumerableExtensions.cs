using System;
using System.Collections.Generic;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Provides extensions to the `IEnumerable` class.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Iterate over the specified IEnumerable passing each element to the specified action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void Each<T>(this IEnumerable<T> arr, Action<T> action)
        {
            foreach (T item in arr)
            {
                action(item);
            }
        }
    }
}
