using System.ComponentModel;

namespace System;

/// <summary>
/// string extensions
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if a string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The input string to evaluate for null or white-space status.</param>
    /// <returns>Returns true if the string is null, empty, or contains only white-space; otherwise, false.</returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Checks if a given string is neither null nor consists only of white-space characters.
    /// </summary>
    /// <param name="value">The input string to evaluate for null or white-space content.</param>
    /// <returns>Returns true if the string is not null and contains non-white-space characters; otherwise, false.</returns>
    public static bool IsNotNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) == false;
    }

    /// <summary>
    /// Checks if a given string is null or empty.
    /// </summary>
    /// <param name="value">The string to evaluate for null or empty status.</param>
    /// <returns>Returns true if the string is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Checks if a given string is neither null nor empty.
    /// </summary>
    /// <param name="value">The string to be evaluated for null or empty status.</param>
    /// <returns>Returns true if the string has content; otherwise, false.</returns>
    public static bool IsNotNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value) == false;
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
}
