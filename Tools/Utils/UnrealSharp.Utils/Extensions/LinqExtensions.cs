using System.Collections;

namespace UnrealSharp.Utils.Extensions;

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
        var i = 0;

        foreach (var element in source)
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
        var i = 0;
        var c = 0;

        foreach (var element in source)
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
        var maxValue = int.MinValue;
        var result = default(T);

        foreach (var item in list)
        {
            var value = projection(item);
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
        return list.Any(v => match(v));
    }

    /// <summary>
    /// Removes the specified match.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list.</param>
    /// <param name="match">The match.</param>
    /// <returns><c>true</c> if remove success, <c>false</c> otherwise.</returns>
    public static bool Remove<T>(this IList<T> list, Predicate<T> match)
    {
        var index = list.IndexOf(match);

        if (index == -1)
        {
            return false;
        }
            
        list.RemoveAt(index);
            
        return true;

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
        return from object? item in enumerable select selector(item);
    }
}