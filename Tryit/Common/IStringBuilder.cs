using System;
using System.Collections.Generic;
using System.Text;

namespace Tryit;

using System;

/// <summary>
/// Defines a fluent abstraction over <see cref="System.Text.StringBuilder"/>.
/// </summary>
/// <remarks>
/// This contract exposes common mutable string operations while returning <see cref="IStringBuilder"/>
/// from modifying members so calls can be chained together. Implementations are expected to forward
/// operations to an underlying <see cref="StringBuilder"/> instance and preserve the same general
/// behavior, argument validation rules, and formatting semantics provided by the framework type.
/// </remarks>
public interface IStringBuilder : IDisposable
{
    #region
    /// <summary>
    /// Gets or sets the number of characters contained in the current builder.
    /// </summary>
    /// <value>
    /// The current length of the character buffer.
    /// </value>
    int Length { get; set; }

    /// <summary>
    /// Gets or sets the character at the specified zero-based position.
    /// </summary>
    /// <param name="index">The zero-based character position to read or update.</param>
    /// <returns>The character stored at the specified index.</returns>
    char this[int index] { get; set; }

    /// <summary>
    /// Gets the underlying <see cref="System.Text.StringBuilder"/> instance used by the implementation.
    /// </summary>
    /// <value>
    /// The wrapped mutable string buffer.
    /// </value>
    /// <remarks>
    /// This property allows direct access to the backing builder when framework-specific functionality
    /// is required beyond what is exposed through this interface.
    /// </remarks>
    StringBuilder StringBuilder { get; }

    #endregion

    #region   Append
    /// <summary>
    /// Appends the string representation of a Boolean value to the end of the current builder.
    /// </summary>
    /// <param name="value">The Boolean value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(bool value);

    /// <summary>
    /// Appends the string representation of an unsigned byte to the end of the current builder.
    /// </summary>
    /// <param name="value">The byte value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(byte value);

    /// <summary>
    /// Appends a single character to the end of the current builder.
    /// </summary>
    /// <param name="value">The character to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(char value);

    /// <summary>
    /// Appends all characters in the specified array to the end of the current builder.
    /// </summary>
    /// <param name="value">The character array whose contents should be appended.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(char[] value);

    /// <summary>
    /// Appends a range of characters from the specified array to the end of the current builder.
    /// </summary>
    /// <param name="value">The character array that contains the characters to append.</param>
    /// <param name="startIndex">The zero-based starting position in <paramref name="value"/>.</param>
    /// <param name="charCount">The number of characters to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(char[] value, int startIndex, int charCount);

    /// <summary>
    /// Appends the string representation of a decimal value to the end of the current builder.
    /// </summary>
    /// <param name="value">The decimal value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(decimal value);

    /// <summary>
    /// Appends the string representation of a double-precision floating-point value to the end of the current builder.
    /// </summary>
    /// <param name="value">The double value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(double value);

    /// <summary>
    /// Appends the string representation of a 16-bit signed integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The short value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(short value);

    /// <summary>
    /// Appends the string representation of a 32-bit signed integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The integer value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(int value);

    /// <summary>
    /// Appends the string representation of a 64-bit signed integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The long value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(long value);

    /// <summary>
    /// Appends the string representation of an object to the end of the current builder.
    /// </summary>
    /// <param name="value">The object whose string representation should be appended.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(object value);

    /// <summary>
    /// Appends the string representation of an 8-bit signed integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The signed byte value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(sbyte value);

    /// <summary>
    /// Appends the string representation of a single-precision floating-point value to the end of the current builder.
    /// </summary>
    /// <param name="value">The float value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(float value);

    /// <summary>
    /// Appends a string to the end of the current builder.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(string value);

    /// <summary>
    /// Appends a substring of the specified string to the end of the current builder.
    /// </summary>
    /// <param name="value">The source string that contains the substring to append.</param>
    /// <param name="startIndex">The zero-based starting position in <paramref name="value"/>.</param>
    /// <param name="count">The number of characters to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(string value, int startIndex, int count);

    /// <summary>
    /// Appends the string representation of a 16-bit unsigned integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The unsigned short value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(ushort value);

    /// <summary>
    /// Appends the string representation of a 32-bit unsigned integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The unsigned integer value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(uint value);

    /// <summary>
    /// Appends the string representation of a 64-bit unsigned integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The unsigned long value to append.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Append(ulong value);
    #endregion

    #region   AppendLine
    /// <summary>
    /// Appends the default line terminator to the end of the current builder.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    IStringBuilder AppendLine();

    /// <summary>
    /// Appends a string followed by the default line terminator to the end of the current builder.
    /// </summary>
    /// <param name="value">The string to append before the line terminator.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder AppendLine(string value);
    #endregion

    #region   Insert
    /// <summary>
    /// Inserts the string representation of a Boolean value at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The Boolean value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, bool value);

    /// <summary>
    /// Inserts the string representation of an unsigned byte at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The byte value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, byte value);

    /// <summary>
    /// Inserts a single character at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The character to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, char value);

    /// <summary>
    /// Inserts the contents of a character array at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The character array to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, char[] value);

    /// <summary>
    /// Inserts a range of characters from the specified array at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The character array that contains the characters to insert.</param>
    /// <param name="startIndex">The zero-based starting position in <paramref name="value"/>.</param>
    /// <param name="charCount">The number of characters to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, char[] value, int startIndex, int charCount);

    /// <summary>
    /// Inserts the string representation of a decimal value at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The decimal value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, decimal value);

    /// <summary>
    /// Inserts the string representation of a double-precision floating-point value at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The double value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, double value);

    /// <summary>
    /// Inserts the string representation of a 16-bit signed integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The short value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, short value);

    /// <summary>
    /// Inserts the string representation of a 32-bit signed integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The integer value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, int value);

    /// <summary>
    /// Inserts the string representation of a 64-bit signed integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The long value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, long value);

    /// <summary>
    /// Inserts the string representation of an object at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The object whose string representation should be inserted.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, object value);

    /// <summary>
    /// Inserts the string representation of an 8-bit signed integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The signed byte value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, sbyte value);

    /// <summary>
    /// Inserts the string representation of a single-precision floating-point value at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The float value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, float value);

    /// <summary>
    /// Inserts a string at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The string to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, string value);

    /// <summary>
    /// Inserts a specified number of copies of a string at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The string to insert.</param>
    /// <param name="count">The number of times <paramref name="value"/> should be inserted.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, string value, int count);

    /// <summary>
    /// Inserts the string representation of a 16-bit unsigned integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The unsigned short value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, ushort value);

    /// <summary>
    /// Inserts the string representation of a 32-bit unsigned integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The unsigned integer value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, uint value);

    /// <summary>
    /// Inserts the string representation of a 64-bit unsigned integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The unsigned long value to insert.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Insert(int index, ulong value);
    #endregion

    #region   Replace
    /// <summary>
    /// Replaces all occurrences of a specified character with another character.
    /// </summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The replacement character.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Replace(char oldChar, char newChar);

    /// <summary>
    /// Replaces all occurrences of a specified string with another string.
    /// </summary>
    /// <param name="oldValue">The string to replace.</param>
    /// <param name="newValue">The replacement string.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Replace(string oldValue, string newValue);

    /// <summary>
    /// Replaces all occurrences of a specified character within a substring of the current builder.
    /// </summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The replacement character.</param>
    /// <param name="startIndex">The zero-based starting position of the range to search.</param>
    /// <param name="count">The number of characters in the range to inspect.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Replace(char oldChar, char newChar, int startIndex, int count);

    /// <summary>
    /// Replaces all occurrences of a specified string within a substring of the current builder.
    /// </summary>
    /// <param name="oldValue">The string to replace.</param>
    /// <param name="newValue">The replacement string.</param>
    /// <param name="startIndex">The zero-based starting position of the range to search.</param>
    /// <param name="count">The number of characters in the range to inspect.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Replace(string oldValue, string newValue, int startIndex, int count);
    #endregion

    #region   Clear
    /// <summary>
    /// Removes all characters from the current builder.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Clear();
    #endregion

    #region   Remove
    /// <summary>
    /// Removes a range of characters from the current builder.
    /// </summary>
    /// <param name="startIndex">The zero-based starting position of the range to remove.</param>
    /// <param name="length">The number of characters to remove.</param>
    /// <returns>The current builder instance.</returns>
    IStringBuilder Remove(int startIndex, int length);
    #endregion

    #region
    /// <summary>
    /// Converts the current builder contents to a string.
    /// </summary>
    /// <returns>A string containing the current character sequence.</returns>
    string ToString();

    /// <summary>
    /// Converts a substring of the current builder contents to a string.
    /// </summary>
    /// <param name="startIndex">The zero-based starting position of the substring.</param>
    /// <param name="length">The number of characters to include in the resulting string.</param>
    /// <returns>A string containing the requested character range.</returns>
    string ToString(int startIndex, int length);

    #endregion
}
