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

#if NET472 || NET451

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="childStr"></param>
    /// <param name="comparisonType"></param>
    /// <returns></returns>
    public static bool Contains<T>(this string source, string childStr, StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase)
    {
        if (source is null || childStr is null)
        {
            return false;
        }

        return source.IndexOf(childStr, comparisonType) >= 0;
    }

#endif
}
