using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Tryit.Internals;

/// <summary>
/// Provides an <see cref="IStringBuilder"/> implementation that delegates all operations to an underlying
/// <see cref="System.Text.StringBuilder"/> instance.
/// </summary>
/// <remarks>
/// This adapter exposes a fluent API by returning the current <see cref="IStringBuilder"/> instance from
/// mutating operations, making it suitable for chained string construction scenarios while preserving the
/// behavior of the wrapped framework type.
/// </remarks>
class StringBuilderAdapter : IStringBuilder
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal SimpleObjectPool<IStringBuilder>? pool;

    /// <summary>
    /// Stores the underlying mutable character buffer used by this adapter.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly StringBuilder stringBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringBuilderAdapter"/> class with an empty backing builder.
    /// </summary>
    public StringBuilderAdapter() => stringBuilder = new StringBuilder();

    /// <summary>
    /// Gets the underlying <see cref="System.Text.StringBuilder"/> instance.
    /// </summary>
    /// <value>
    /// The wrapped <see cref="StringBuilder"/> used to perform all string operations.
    /// </value>
    public StringBuilder StringBuilder => stringBuilder;

    /// <summary>
    /// Gets or sets the number of characters contained in the current builder.
    /// </summary>
    /// <value>
    /// The current length of the underlying character buffer.
    /// </value>
    public int Length
    {
        get => stringBuilder.Length;
        set => stringBuilder.Length = value;
    }

    /// <summary>
    /// Gets or sets the character at the specified zero-based position.
    /// </summary>
    /// <param name="index">The zero-based index of the character to retrieve or update.</param>
    /// <returns>The character located at the specified index.</returns>
    public char this[int index]
    {
        get => stringBuilder[index];
        set => stringBuilder[index] = value;
    }

    /// <summary>
    /// Appends a string to the end of the current builder.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(string value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends a string followed by the default line terminator to the end of the current builder.
    /// </summary>
    /// <param name="value">The string to append before the line terminator.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder AppendLine(string value)
    {
        stringBuilder.AppendLine(value);
        return this;
    }

    /// <summary>
    /// Removes all characters from the current builder.
    /// </summary>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Clear()
    {
        stringBuilder.Clear();
        return this;
    }

    /// <summary>
    /// Removes a range of characters from the current builder.
    /// </summary>
    /// <param name="startIndex">The zero-based starting position of the range to remove.</param>
    /// <param name="length">The number of characters to remove.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Remove(int startIndex, int length)
    {
        stringBuilder.Remove(startIndex, length);
        return this;
    }

    /// <summary>
    /// Inserts a string at the specified zero-based position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The string to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, string value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Replaces all occurrences of a specified string with another string.
    /// </summary>
    /// <param name="oldValue">The string to replace.</param>
    /// <param name="newValue">The replacement string.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Replace(string oldValue, string newValue)
    {
        stringBuilder.Replace(oldValue, newValue);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a Boolean value to the end of the current builder.
    /// </summary>
    /// <param name="value">The Boolean value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(bool value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of an unsigned byte to the end of the current builder.
    /// </summary>
    /// <param name="value">The byte value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(byte value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends a single character to the end of the current builder.
    /// </summary>
    /// <param name="value">The character to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(char value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends all characters in the specified array to the end of the current builder.
    /// </summary>
    /// <param name="value">The character array whose contents should be appended.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(char[] value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends a range of characters from the specified array to the end of the current builder.
    /// </summary>
    /// <param name="value">The character array containing the characters to append.</param>
    /// <param name="startIndex">The zero-based starting position in <paramref name="value"/>.</param>
    /// <param name="charCount">The number of characters to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(char[] value, int startIndex, int charCount)
    {
        stringBuilder.Append(value, startIndex, charCount);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a decimal value to the end of the current builder.
    /// </summary>
    /// <param name="value">The decimal value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(decimal value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a double-precision floating-point value to the end of the current builder.
    /// </summary>
    /// <param name="value">The double value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(double value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a 16-bit signed integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The short value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(short value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a 32-bit signed integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The integer value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(int value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a 64-bit signed integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The long value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(long value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of an object to the end of the current builder.
    /// </summary>
    /// <param name="value">The object whose string representation should be appended.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(object value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of an 8-bit signed integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The signed byte value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(sbyte value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a single-precision floating-point value to the end of the current builder.
    /// </summary>
    /// <param name="value">The float value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(float value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a 16-bit unsigned integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The unsigned short value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(ushort value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a 32-bit unsigned integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The unsigned integer value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(uint value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends the string representation of a 64-bit unsigned integer to the end of the current builder.
    /// </summary>
    /// <param name="value">The unsigned long value to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(ulong value)
    {
        stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    /// Appends a substring of the specified string to the end of the current builder.
    /// </summary>
    /// <param name="value">The source string containing the substring to append.</param>
    /// <param name="startIndex">The zero-based starting position in <paramref name="value"/>.</param>
    /// <param name="count">The number of characters to append.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Append(string value, int startIndex, int count)
    {
        stringBuilder.Append(value, startIndex, count);
        return this;
    }

    /// <summary>
    /// Appends the default line terminator to the end of the current builder.
    /// </summary>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder AppendLine()
    {
        stringBuilder.AppendLine();
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a Boolean value at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The Boolean value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, bool value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of an unsigned byte at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The byte value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, byte value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts a single character at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The character to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, char value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts all characters in the specified array at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The character array to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, char[] value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts a range of characters from the specified array at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The character array containing the characters to insert.</param>
    /// <param name="startIndex">The zero-based starting position in <paramref name="value"/>.</param>
    /// <param name="charCount">The number of characters to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, char[] value, int startIndex, int charCount)
    {
        stringBuilder.Insert(index, value, startIndex, charCount);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a decimal value at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The decimal value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, decimal value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a double-precision floating-point value at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The double value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, double value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a 16-bit signed integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The short value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, short value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a 32-bit signed integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The integer value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, int value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a 64-bit signed integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The long value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, long value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of an object at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The object whose string representation should be inserted.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, object value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of an 8-bit signed integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The signed byte value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, sbyte value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a single-precision floating-point value at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The float value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, float value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts a specified number of copies of a string at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The string to insert.</param>
    /// <param name="count">The number of times <paramref name="value"/> should be inserted.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, string value, int count)
    {
        stringBuilder.Insert(index, value, count);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a 16-bit unsigned integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The unsigned short value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, ushort value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a 32-bit unsigned integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The unsigned integer value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, uint value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Inserts the string representation of a 64-bit unsigned integer at the specified position.
    /// </summary>
    /// <param name="index">The zero-based position at which insertion begins.</param>
    /// <param name="value">The unsigned long value to insert.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Insert(int index, ulong value)
    {
        stringBuilder.Insert(index, value);
        return this;
    }

    /// <summary>
    /// Replaces all occurrences of a specified character with another character.
    /// </summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The replacement character.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Replace(char oldChar, char newChar)
    {
        stringBuilder.Replace(oldChar, newChar);
        return this;
    }

    /// <summary>
    /// Replaces all occurrences of a specified character within a defined substring range.
    /// </summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The replacement character.</param>
    /// <param name="startIndex">The zero-based starting position of the range to inspect.</param>
    /// <param name="count">The number of characters to include in the replacement operation.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
    {
        stringBuilder.Replace(oldChar, newChar, startIndex, count);
        return this;
    }

    /// <summary>
    /// Replaces all occurrences of a specified string within a defined substring range.
    /// </summary>
    /// <param name="oldValue">The string to replace.</param>
    /// <param name="newValue">The replacement string.</param>
    /// <param name="startIndex">The zero-based starting position of the range to inspect.</param>
    /// <param name="count">The number of characters to include in the replacement operation.</param>
    /// <returns>The current adapter instance for fluent chaining.</returns>
    public IStringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
    {
        stringBuilder.Replace(oldValue, newValue, startIndex, count);
        return this;
    }

    /// <summary>
    /// Converts a range of characters in the current builder to a new string instance.
    /// </summary>
    /// <param name="startIndex">The zero-based starting position of the substring.</param>
    /// <param name="length">The number of characters to include in the resulting string.</param>
    /// <returns>A string containing the requested range of characters.</returns>
    public string ToString(int startIndex, int length) => stringBuilder.ToString(startIndex, length);

    /// <summary>
    /// Converts the entire contents of the current builder to a string.
    /// </summary>
    /// <returns>A string containing all characters in the underlying builder.</returns>
    public override string ToString() => stringBuilder.ToString();

    void IDisposable.Dispose()
    {
        Interlocked.Exchange(ref pool, null)?.Return(this);
    }
}
