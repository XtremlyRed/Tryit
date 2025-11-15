using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Linq;

/// <summary>
/// Contains extension methods for working with collections, including checks for null or emptiness, filtering,
/// indexing, and pagination. Also provides methods for iterating with actions and converting collections to read-only
/// types.
/// </summary>
public static partial class EnumerableExtensions
{
    /// <summary>
    /// Checks if a collection is null or has no elements.
    /// </summary>
    /// <param name="source">The collection to be checked for null or emptiness.</param>
    /// <returns>True if the collection is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty(this IEnumerable? source)
    {
        if (source is null)
        {
            return true;
        }

        if (source is Array array)
        {
            return array.Length == 0;
        }
        else if (source is ICollection collection2)
        {
            return collection2.Count == 0;
        }

        foreach (object? _ in source)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a collection is not null and contains elements.
    /// </summary>
    /// <param name="source">The collection to be checked for null or empty status.</param>
    /// <returns>True if the collection is not null and has at least one element; otherwise, false.</returns>
    public static bool IsNotNullOrEmpty(this IEnumerable? source)
    {
        if (source is null)
        {
            return false;
        }

        if (source is ICollection collection2)
        {
            return collection2.Count != 0;
        }
        foreach (object? _ in source)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a given collection is null or empty.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the collection being checked.</typeparam>
    /// <param name="source">The collection to evaluate for null or emptiness.</param>
    /// <returns>True if the collection is null or contains no elements; otherwise, false.</returns>
    public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource>? source)
    {
        if (source is null)
        {
            return true;
        }
        if (source is IList<TSource> array)
        {
            return array.Count == 0;
        }
        else if (source is IReadOnlyCollection<TSource> @readonly)
        {
            return @readonly.Count == 0;
        }
        else if (source is ICollection<TSource> collection)
        {
            return collection.Count == 0;
        }
        else if (source is ICollection collection2)
        {
            return collection2.Count == 0;
        }
        else
        {
            return source.Any() == false;
        }
    }

    /// <summary>
    /// Checks if a collection is not null and contains elements.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the collection being checked for null or emptiness.</typeparam>
    /// <param name="source">The collection to be evaluated for null or empty status.</param>
    /// <returns>True if the collection is not null and contains at least one element; otherwise, false.</returns>
    public static bool IsNotNullOrEmpty<TSource>(this IEnumerable<TSource>? source)
    {
        if (source is null)
        {
            return false;
        }
        if (source is IList<TSource> array)
        {
            return array.Count != 0;
        }
        else if (source is IReadOnlyCollection<TSource> @readonly)
        {
            return @readonly.Count != 0;
        }
        else if (source is ICollection<TSource> collection)
        {
            return collection.Count != 0;
        }
        else if (source is ICollection collection2)
        {
            return collection2.Count != 0;
        }
        else
        {
            return source.Any();
        }
    }

    /// <summary>
    /// Filters a collection based on a condition and a specified filter function.
    /// </summary>
    /// <typeparam name="Target">Represents the type of elements in the collection being processed.</typeparam>
    /// <param name="source">The collection of elements to be filtered based on the provided condition.</param>
    /// <param name="condition">Determines whether the filtering should be applied to the collection.</param>
    /// <param name="filter">A function that defines the criteria for filtering the elements in the collection.</param>
    /// <returns>Returns the original collection or a filtered collection based on the condition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the collection or filter function is null.</exception>
    public static IEnumerable<Target> WhereIf<Target>(this IEnumerable<Target> source, bool condition, Func<Target, bool> filter)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = filter ?? throw new ArgumentNullException(nameof(filter));

        if (condition)
        {
            return source.Where(filter);
        }

        return source;
    }

    /// <summary>
    /// Finds the index of the first element in a collection that matches a specified condition.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the collection being searched.</typeparam>
    /// <param name="source">The collection of elements to search through for a matching condition.</param>
    /// <param name="filter">A function that defines the condition to evaluate each element against.</param>
    /// <returns>The zero-based index of the first matching element, or -1 if no match is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the collection or the filter function is null.</exception>
    public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> filter)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = filter ?? throw new ArgumentNullException(nameof(filter));

        if (source is IList<TSource> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (filter(array[i]))
                {
                    return i;
                }
            }
        }
        else if (source is IReadOnlyList<TSource> @readonly)
        {
            for (int i = 0; i < @readonly.Count; i++)
            {
                if (filter(@readonly[i]))
                {
                    return i;
                }
            }
        }
        else
        {
            var index = 0;

            foreach (TSource? item in source)
            {
                if (filter(item))
                {
                    return index;
                }

                index++;
            }
        }

        return -1;
    }

    /// <summary>
    /// Attempts to find the index of the first element in a collection that matches a specified condition.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the collection being searched.</typeparam>
    /// <param name="source">The collection of elements to search through for a matching condition.</param>
    /// <param name="filter">A function that defines the condition to be matched by the elements in the collection.</param>
    /// <param name="index">Outputs the index of the first matching element if found, otherwise -1.</param>
    /// <returns>True if a matching element is found; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the collection or the filter function is null.</exception>
    public static bool TryIndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> filter, out int index)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = filter ?? throw new ArgumentNullException(nameof(filter));

        if (source is IList<TSource> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (filter(array[i]))
                {
                    index = i;
                    return true;
                }
            }
        }
        else if (source is IReadOnlyList<TSource> @readonly)
        {
            for (int i = 0; i < @readonly.Count; i++)
            {
                if (filter(@readonly[i]))
                {
                    index = i;
                    return true;
                }
            }
        }
        else
        {
            var forIndex = 0;
            index = -1;

            foreach (TSource? item in source)
            {
                if (filter(item))
                {
                    index = forIndex;
                    return true;
                }

                forIndex++;
            }
        }

        index = -1;
        return false;
    }

    /// <summary>
    /// Finds the indices of elements in a collection that match a specified condition.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the source collection.</typeparam>
    /// <param name="source">The collection of elements to search through for matching conditions.</param>
    /// <param name="filter">A function that defines the condition to evaluate each element against.</param>
    /// <returns>An enumerable collection of indices where the elements match the specified condition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the collection or the filter function is null.</exception>
    public static IEnumerable<int> IndexOfMany<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> filter)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = filter ?? throw new ArgumentNullException(nameof(filter));

        if (source is IList<TSource> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (filter(array[i]))
                {
                    yield return i;
                }
            }
        }
        else if (source is IReadOnlyList<TSource> @readonly)
        {
            for (int i = 0; i < @readonly.Count; i++)
            {
                if (filter(@readonly[i]))
                {
                    yield return i;
                }
            }
        }
        else
        {
            var index = 0;

            foreach (TSource? item in source)
            {
                if (filter(item))
                {
                    yield return index;
                }

                index++;
            }
        }
    }

    /// <summary>
    /// Generates a paginated subset of a collection based on the specified page index and size.
    /// </summary>
    /// <typeparam name="Target">Represents the type of elements in the collection being paginated.</typeparam>
    /// <param name="source">The collection from which a subset is to be extracted.</param>
    /// <param name="pageIndex">Indicates the specific page of results to retrieve, starting from 1.</param>
    /// <param name="pageSize">Specifies the number of items to include in each page of results.</param>
    /// <returns>A collection containing the items for the specified page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the collection to paginate is null.</exception>
    public static IEnumerable<Target> Paginate<Target>(this IEnumerable<Target> source, int pageIndex, int pageSize)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));

        return source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Iterates over each element in a collection and applies a specified action to each element.
    /// </summary>
    /// <typeparam name="Target">Represents the type of elements in the collection being processed.</typeparam>
    /// <param name="source">The collection of elements to iterate over and apply the action to.</param>
    /// <param name="action">The operation to perform on each element of the collection.</param>
    /// <returns>The original collection after applying the action to each element.</returns>
    [DebuggerNonUserCode]
    public static IEnumerable<Target> ForEach<Target>(this IEnumerable<Target> source, Action<Target> action)
    {
        if (source is null || action is null)
        {
            return source!;
        }

        if (source is IList<Target> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i]);
            }
        }
        else if (source is IReadOnlyList<Target> @readonly)
        {
            for (int i = 0; i < @readonly.Count; i++)
            {
                action(@readonly[i]);
            }
        }
        else
        {
            foreach (Target item in source)
            {
                action(item);
            }
        }

        return source;
    }

    /// <summary>
    /// Iterates over a collection and applies a specified action to each element along with its index.
    /// </summary>
    /// <typeparam name="Target">Represents the type of elements in the collection being processed.</typeparam>
    /// <param name="source">The collection of elements to iterate over and apply the action to.</param>
    /// <param name="action">The operation to perform on each element and its index during iteration.</param>
    [DebuggerNonUserCode]
    public static IEnumerable<Target> ForEach<Target>(this IEnumerable<Target> source, Action<Target, int> action)
    {
        if (source is null || action is null)
        {
            return source!;
        }

        if (source is IList<Target> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }
        else if (source is IReadOnlyList<Target> @readonly)
        {
            for (int i = 0; i < @readonly.Count; i++)
            {
                action(@readonly[i], i);
            }
        }
        else
        {
            int index = 0;
            foreach (Target item in source)
            {
                index++;
                action(item, index);
            }
        }

        return source;
    }

    /// <summary>
    /// Executes an asynchronous action for each element in a collection and returns the original collection.
    /// </summary>
    /// <typeparam name="Target">Represents the type of elements in the collection being processed.</typeparam>
    /// <param name="source">The collection of elements to iterate over and apply the asynchronous action.</param>
    /// <param name="action">An asynchronous function that is applied to each element in the collection.</param>
    /// <returns>The original collection after all actions have been executed.</returns>
    [DebuggerNonUserCode]
    public static async Task<IEnumerable<Target>> ForEachAsync<Target>(this IEnumerable<Target> source, Func<Target, Task> action)
    {
        if (source is null || action is null)
        {
            return source!;
        }

        if (source is IList<Target> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                await action(list[i]);
            }
        }
        else if (source is IReadOnlyList<Target> @readonly)
        {
            for (int i = 0; i < @readonly.Count; i++)
            {
                await action(@readonly[i]);
            }
        }
        else
        {
            foreach (Target item in source)
            {
                await action(item);
            }
        }

        return source;
    }

    /// <summary>
    /// Executes an asynchronous action for each element in a collection and returns the original collection.
    /// </summary>
    /// <typeparam name="Target">Represents the type of elements in the collection being processed.</typeparam>
    /// <param name="source">The collection of elements to iterate over and apply the action to.</param>
    /// <param name="action">An asynchronous function that takes an element and its index to perform an operation.</param>
    /// <returns>The original collection after all actions have been executed.</returns>
    [DebuggerNonUserCode]
    public static async Task<IEnumerable<Target>> ForEachAsync<Target>(this IEnumerable<Target> source, Func<Target, int, Task> action)
    {
        if (source is null || action is null)
        {
            return source!;
        }

        if (source is IList<Target> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                await action(array[i], i);
            }
        }
        else if (source is IReadOnlyList<Target> @readonly)
        {
            for (int i = 0; i < @readonly.Count; i++)
            {
                await action(@readonly[i], i);
            }
        }
        else
        {
            int index = 0;
            foreach (Target item in source)
            {
                index++;
                await action(item, index);
            }
        }

        return source;
    }

    /// <summary>
    /// Iterates over each element in a collection and applies a specified action to each element.
    /// </summary>
    /// <param name="source">The collection of elements to be processed by the action.</param>
    /// <param name="action">The operation to perform on each element of the collection.</param>
    public static IEnumerable ForEach(this IEnumerable source, Action<object?> action)
    {
        if (source is null || action is null)
        {
            return source!;
        }

        if (source is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i]);
            }
        }
        else
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        return source;
    }

    /// <summary>
    /// Iterates over a collection and applies a specified action to each element along with its index.
    /// </summary>
    /// <param name="source">The collection of elements to be processed by the action.</param>
    /// <param name="action">A function that takes an element and its index to perform a specified operation.</param>
    public static void ForEach(this IEnumerable source, Action<object?, int> action)
    {
        if (source is null || action is null)
        {
            return;
        }

        if (source is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }

            return;
        }
        var index = 0;
        foreach (var item in source)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// Executes an asynchronous action for each element in a collection.
    /// </summary>
    /// <param name="source">The collection of elements to iterate over.</param>
    /// <param name="action">An asynchronous function to execute for each element in the collection.</param>
    /// <returns>The original collection after all actions have been executed.</returns>
    public static async Task<IEnumerable> ForEachAsync(this IEnumerable source, Func<object?, Task> action)
    {
        if (source is null || action is null)
        {
            return source!;
        }

        if (source is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                await action(list[i]);
            }
        }
        else
        {
            foreach (var item in source)
            {
                await action(item);
            }
        }

        return source;
    }

    /// <summary>
    /// Executes an asynchronous action for each element in a collection, passing the element and its index to the action.
    /// </summary>
    /// <param name="source">The collection of elements to iterate over and perform actions on.</param>
    /// <param name="action">An asynchronous function that processes each element along with its index.</param>
    /// <returns>This method does not return a value.</returns>
    public static async Task ForEachAsync(this IEnumerable source, Func<object?, int, Task> action)
    {
        if (source is null || action is null)
        {
            return;
        }

        if (source is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                await action(list[i], i);
            }

            return;
        }
        var index = 0;
        foreach (var item in source)
        {
            await action(item, index++);
        }
    }

    /// <summary>
    /// Combines elements of a collection into a single string, separated by a specified symbol.
    /// </summary>
    /// <typeparam name="T">Represents the type of elements in the collection being joined.</typeparam>
    /// <param name="source">The collection of elements to be concatenated into a string.</param>
    /// <param name="intervalSymbol">The string used to separate the elements in the resulting concatenated string.</param>
    /// <returns>A single string that contains all elements from the collection, separated by the specified symbol.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the collection or the separator string is null.</exception>
    public static string Join<T>(this IEnumerable<T> source, string intervalSymbol = ",")
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = intervalSymbol ?? throw new ArgumentNullException(nameof(intervalSymbol));

        return string.Join(intervalSymbol, source);
    }

    /// <summary>
    /// Combines elements from a collection into a single string, using a specified interval symbol.
    /// </summary>
    /// <typeparam name="T">Represents the type of elements in the collection being processed.</typeparam>
    /// <param name="source">The collection of elements to be joined into a string.</param>
    /// <param name="selector">A function that transforms each element of the collection into a string.</param>
    /// <param name="intervalSymbol">The string used to separate the elements in the resulting joined string.</param>
    /// <returns>A single string that contains all the transformed elements separated by the specified interval symbol.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the collection, selector function, or interval symbol is null.</exception>
    public static string Join<T>(this IEnumerable<T> source, Func<T, string> selector, string intervalSymbol = ",")
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = selector ?? throw new ArgumentNullException(nameof(selector));
        _ = intervalSymbol ?? throw new ArgumentNullException(nameof(intervalSymbol));

        return string.Join(intervalSymbol, source.Select(selector));
    }

#if !NET6_0_OR_GREATER

    /// <summary>
    /// Divides a collection into smaller chunks of a specified size.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the collection being chunked.</typeparam>
    /// <param name="targets">The collection to be divided into smaller segments.</param>
    /// <param name="segmentSize">Specifies the maximum number of elements in each chunk.</param>
    /// <returns>An enumerable collection of chunks, each containing up to the specified number of elements.</returns>
    public static IEnumerable<IEnumerable<TSource>> Chunk<TSource>(this IEnumerable<TSource> targets, int segmentSize)
    {
        if (targets is null || segmentSize < 1)
        {
            yield break;
        }

        using var enumer = targets.GetEnumerator();

        while (enumer.MoveNext())
        {
            yield return InnerChunk<TSource>(enumer, segmentSize);
        }

        static IEnumerable<T> InnerChunk<T>(IEnumerator<T> enumerator, int sengmetSize)
        {
            int index = 0;

            do
            {
                yield return enumerator.Current;

                if (++index >= sengmetSize)
                {
                    yield break;
                }
            } while (enumerator.MoveNext());
        }
    }

    /// <summary>
    /// Finds the element in a collection that has the maximum value based on a specified key selector.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the source collection.</typeparam>
    /// <typeparam name="TKey">Represents the type of the key used for comparison to determine the maximum element.</typeparam>
    /// <param name="source">The collection of elements to search for the maximum value.</param>
    /// <param name="keySelector">A function that extracts the key from each element for comparison.</param>
    /// <param name="comparer">An optional comparer to define how the keys are compared.</param>
    /// <returns>The element with the maximum key value or null if the collection is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source collection or key selector function is null.</exception>
    public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = keySelector ?? throw new ArgumentNullException(nameof(keySelector));

        comparer ??= Comparer<TKey>.Default;

        using IEnumerator<TSource> e = source.GetEnumerator();

        if (e.MoveNext() == false)
        {
            throw new ArgumentNullException("source contains no elements");
        }

        TSource value = e.Current;
        TKey key = keySelector(value);

        if (default(TKey) is null)
        {
            if (key is null)
            {
                TSource firstValue = value;

                do
                {
                    if (e.MoveNext() == false)
                    {
                        return firstValue;
                    }

                    value = e.Current;
                    key = keySelector(value);
                } while (key is null);
            }

            while (e.MoveNext())
            {
                TSource nextValue = e.Current;
                TKey nextKey = keySelector(nextValue);
                if (nextKey is not null && comparer.Compare(nextKey, key) > 0)
                {
                    key = nextKey;
                    value = nextValue;
                }
            }
        }
        else
        {
            if (comparer == Comparer<TKey>.Default)
            {
                while (e.MoveNext())
                {
                    TSource nextValue = e.Current;
                    TKey nextKey = keySelector(nextValue);
                    if (Comparer<TKey>.Default.Compare(nextKey, key) > 0)
                    {
                        key = nextKey;
                        value = nextValue;
                    }
                }
            }
            else
            {
                while (e.MoveNext())
                {
                    TSource nextValue = e.Current;
                    TKey nextKey = keySelector(nextValue);
                    if (comparer.Compare(nextKey, key) > 0)
                    {
                        key = nextKey;
                        value = nextValue;
                    }
                }
            }
        }

        return value;
    }

    /// <summary>
    /// Finds the minimum element in a sequence based on a specified key selector and optional comparer.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of elements in the source collection.</typeparam>
    /// <typeparam name="TKey">Represents the type of the key used for comparison.</typeparam>
    /// <param name="source">The collection of elements to search for the minimum value.</param>
    /// <param name="keySelector">A function to extract the key from each element for comparison.</param>
    /// <param name="comparer">An optional comparer to define how the keys are compared.</param>
    /// <returns>The minimum element from the source based on the specified key, or null if the source is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source or key selector is null.</exception>
    public static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = keySelector ?? throw new ArgumentNullException(nameof(keySelector));

        comparer ??= Comparer<TKey>.Default;

        using IEnumerator<TSource> e = source.GetEnumerator();

        if (e.MoveNext() == false)
        {
            throw new ArgumentNullException("source contains no elements");
        }

        TSource value = e.Current;
        TKey key = keySelector(value);

        if (default(TKey) is null)
        {
            if (key is null)
            {
                TSource firstValue = value;

                do
                {
                    if (!e.MoveNext())
                    {
                        return firstValue;
                    }

                    value = e.Current;
                    key = keySelector(value);
                } while (key is null);
            }

            while (e.MoveNext())
            {
                TSource nextValue = e.Current;
                TKey nextKey = keySelector(nextValue);
                if (nextKey is not null && comparer.Compare(nextKey, key) < 0)
                {
                    key = nextKey;
                    value = nextValue;
                }
            }
        }
        else
        {
            if (comparer == Comparer<TKey>.Default)
            {
                while (e.MoveNext())
                {
                    TSource nextValue = e.Current;
                    TKey nextKey = keySelector(nextValue);
                    if (Comparer<TKey>.Default.Compare(nextKey, key) < 0)
                    {
                        key = nextKey;
                        value = nextValue;
                    }
                }
            }
            else
            {
                while (e.MoveNext())
                {
                    TSource nextValue = e.Current;
                    TKey nextKey = keySelector(nextValue);
                    if (comparer.Compare(nextKey, key) < 0)
                    {
                        key = nextKey;
                        value = nextValue;
                    }
                }
            }
        }

        return value;
    }
#endif

    /// <summary>
    /// Clears all items from a collection that supports producer-consumer operations.
    /// </summary>
    /// <typeparam name="T">Represents the type of elements contained in the collection.</typeparam>
    /// <param name="source">The collection from which all items will be removed.</param>
    public static void Clear<T>(this IProducerConsumerCollection<T> source)
    {
        if (source is null || source.Count == 0)
        {
            return;
        }

        while (source.TryTake(out _)) { }
    }

    /// <summary>
    /// Adds multiple elements to a collection if the collection and elements are not null or empty.
    /// </summary>
    /// <typeparam name="Target">Specifies the type of elements that will be added to the collection.</typeparam>
    /// <param name="sources">Represents the collection to which elements will be added.</param>
    /// <param name="targets">Contains the elements that will be added to the collection.</param>
    public static void AddRange<Target>(this Collection<Target> sources, params Target[] targets)
    {
        if (sources is null || targets is null || targets.Length == 0)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            sources.Add(targets[i]);
        }
    }

    /// <summary>
    /// Adds a range of items from a collection of targets to a specified collection of sources.
    /// </summary>
    /// <typeparam name="Target">Represents the type of elements being added to the collection.</typeparam>
    /// <param name="sources">The collection that will receive the new items from another collection.</param>
    /// <param name="targets">The collection containing the items to be added to the source collection.</param>
    public static void AddRange<Target>(this Collection<Target> sources, IEnumerable<Target> targets)
    {
        if (sources is null || targets is null)
        {
            return;
        }

        foreach (var item in targets)
        {
            sources.Add(item);
        }
    }

    /// <summary>
    /// Converts a given enumerable collection into a read-only list.
    /// </summary>
    /// <typeparam name="T">Represents the type of elements in the collection being converted.</typeparam>
    /// <param name="source">The collection of elements to be converted into a read-only list.</param>
    /// <returns>A read-only list containing the elements from the provided collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided collection is null.</exception>
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));

        if (source is IList<T> list2)
        {
            //
            return new ReadOnlyCollection<T>(list2);
        }

        return new ReadOnlyCollection<T>(source.ToArray());
    }

    /// <summary>
    /// Converts a mutable dictionary into a read-only dictionary.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of keys in the dictionary, which must not be null.</typeparam>
    /// <typeparam name="TValue">Specifies the type of values in the dictionary.</typeparam>
    /// <param name="dict">The mutable dictionary to be converted into a read-only version.</param>
    /// <returns>A read-only dictionary containing the same key-value pairs as the input dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided dictionary is null.</exception>
    public static IReadOnlyDictionary<TKey, TValue> ToReadOnlayDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        where TKey : notnull
    {
        _ = dict ?? throw new ArgumentNullException(nameof(dict));

        if (dict is IReadOnlyDictionary<TKey, TValue> read)
        {
            //
            return read;
        }

        return new ReadOnlyDictionary<TKey, TValue>(dict);
    }
}
