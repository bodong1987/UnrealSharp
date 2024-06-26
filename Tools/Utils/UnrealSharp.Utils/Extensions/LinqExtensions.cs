using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace UnrealSharp.Utils.Extensions
{
    /// <summary>
    /// Class LinqExtensions.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>System.Int32.</returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            int i = 0;

            foreach (TSource element in source)
            {
                if (predicate(element))
                    return i;

                ++i;
            }

            return -1;
        }

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>System.Int32.</returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, int startIndex, Predicate<TSource> predicate)
        {
            int i = 0;
            int c = 0;

            foreach (TSource element in source)
            {
                if (c < startIndex)
                {
                    ++c;
                    ++i;
                    continue;
                }

                if (predicate(element))
                    return i;

                ++i;
            }

            return -1;
        }

        /// <summary>
        /// Finds the maximum element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>T.</returns>
        /// <exception cref="InvalidOperationException">Empty List</exception>
        public static T? MaxElement<T>(this IEnumerable<T> list, Converter<T, int> projection)
        {
            if (list.Count() == 0)
            {
                throw new InvalidOperationException("Empty List");
            }

            int maxValue = int.MinValue;
            T? result = default(T);

            foreach (T item in list)
            {
                int value = projection(item);
                if (value > maxValue)
                {
                    maxValue = value;
                    result = item;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether [contains] [the specified match].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="match">The match.</param>
        /// <returns><c>true</c> if [contains] [the specified match]; otherwise, <c>false</c>.</returns>
        public static bool Contains<T>(this IEnumerable<T> list, Predicate<T> match)
        {
            foreach (var v in list)
            {
                if (match(v))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the specified match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="match">The match.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool Remove<T>(this IList<T> list, Predicate<T> match)
        {
            int Index = list.IndexOf(match);

            if (Index != -1)
            {
                list.RemoveAt(Index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Selects the specified selector.
        /// </summary>
        /// <typeparam name="TRet">The type of the t ret.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>IEnumerable&lt;TRet&gt;.</returns>
        public static IEnumerable<TRet> Select<TRet>(this IEnumerable enumerable, Func<object, TRet> selector)
        {
            foreach (object item in enumerable)
            {
                yield return selector(item);
            }
        }
    }

}
