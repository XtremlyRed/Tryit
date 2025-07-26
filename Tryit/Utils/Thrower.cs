using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tryit;

/// <summary>
/// Contains static methods to validate strings and objects for null or empty values, throwing exceptions when invalid.
/// Captures context for debugging.
/// </summary>
public static class Thrower
{
    /// <summary>
    /// Checks if a string is null or empty and throws an exception if it is.
    /// </summary>
    /// <param name="string">The string to be checked for null or empty status.</param>
    /// <param name="argumentName">An optional name for the argument being checked, used in the exception message if provided.</param>
    /// <param name="caller">The name of the method that called this function, automatically captured for debugging.</param>
    /// <param name="callerFileName">The file path of the source code where the call originated, used in the exception message.</param>
    /// <param name="callerLineNumner">The line number in the source code where the call was made, included in the exception message.</param>
    /// <exception cref="ArgumentException">Thrown when the string is null or empty, indicating the issue with the specified argument.</exception>
    public static void IsNullOrEmpty(
        string? @string,
        string? argumentName = null,
        [CallerMemberName] string? caller = null,
        [CallerFilePath] string? callerFileName = null,
        [CallerLineNumber] int? callerLineNumner = null
    )
    {
        if (string.IsNullOrEmpty(@string) == false)
        {
            return;
        }

        var argu = string.IsNullOrWhiteSpace(argumentName) ? caller : argumentName;

        const string nullOeEmptyMessage = "{0} is null or empty in file {1} at line {2}.";

        throw new ArgumentException(string.Format(nullOeEmptyMessage, argu, callerFileName, callerLineNumner));
    }

    /// <summary>
    /// Checks if a string is null or consists only of white-space characters. Throws an exception if the string is
    /// invalid.
    /// </summary>
    /// <param name="string">The string to be checked for null or white-space content.</param>
    /// <param name="argumentName">An optional name for the argument being validated, used in the exception message if provided.</param>
    /// <param name="caller">The name of the method that called this validation, automatically captured for debugging purposes.</param>
    /// <param name="callerFileName">The file path of the source code where the validation was called, used in the exception message.</param>
    /// <param name="callerLineNumner">The line number in the source code where the validation was invoked, included in the exception message.</param>
    /// <exception cref="ArgumentException">Thrown when the string is null or white-space, indicating the issue with the specified argument.</exception>
    public static void IsNullOrWhiteSpace(
        string? @string,
        string? argumentName = null,
        [CallerMemberName] string? caller = null,
        [CallerFilePath] string? callerFileName = null,
        [CallerLineNumber] int? callerLineNumner = null
    )
    {
        if (string.IsNullOrWhiteSpace(@string) == false)
        {
            return;
        }

        var argu = string.IsNullOrWhiteSpace(argumentName) ? caller : argumentName;

        const string nullOeEmptyMessage = "{0} is null or empty in file {1} at line {2}.";

        throw new ArgumentException(string.Format(nullOeEmptyMessage, argu, callerFileName, callerLineNumner));
    }

    /// <summary>
    /// Checks if the provided object is null and throws an exception if it is not. It also captures the context of the
    /// call.
    /// </summary>
    /// <typeparam name="T">Represents a reference type that is being checked for null.</typeparam>
    /// <param name="object">The object to be checked for nullity.</param>
    /// <param name="argumentName">An optional name for the object, used in the exception message if provided.</param>
    /// <param name="callerFileName">The file name from which the method was called, used for debugging purposes.</param>
    /// <param name="callerLineNumner">The line number in the file where the method was called, aiding in identifying the source of the call.</param>
    /// <exception cref="ArgumentException">Thrown when the object is not null, indicating a violation of the null check.</exception>
    public static void IsNull<T>(T? @object, string? argumentName = null, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int? callerLineNumner = null)
        where T : class
    {
        if (@object is not null)
        {
            return;
        }

        var argu = string.IsNullOrWhiteSpace(argumentName) ? "object" : argumentName;

        const string nullOeEmptyMessage = "{0}:{1} is null in file {1} at line {2}.";

        throw new ArgumentException(string.Format(nullOeEmptyMessage, argu, callerFileName, callerLineNumner));
    }
}
