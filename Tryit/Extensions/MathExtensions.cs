using System.Collections.ObjectModel;

namespace System;

/// <summary>
/// Provides extension methods for various numeric types to constrain values within specified minimum and maximum
/// bounds. Also includes methods to check if a value is within a range and to calculate absolute values.
/// </summary>
public static partial class MathExtensions
{
    /// <summary>
    /// Represents an array of characters containing the digits 0-9 and uppercase letters A-Z.
    /// </summary>
    private static readonly char[] charArray = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    /// <summary>
    /// Provides a read-only mapping between integer keys and their corresponding double values.
    /// </summary>
    private static readonly ReadOnlyDictionary<int, double> digitsMaps;

    /// <summary>
    /// Provides a read-only mapping of integer keys to their corresponding decimal digit values.
    /// </summary>
    /// <remarks>This dictionary is intended for scenarios where a fixed association between integer
    /// identifiers and decimal digit counts is required. The contents are immutable and should be accessed in a
    /// thread-safe manner.</remarks>
    private static readonly ReadOnlyDictionary<int, decimal> decimalDigitsMaps;

    /// <summary>
    /// Initializes static data for the MathExtensions class.
    /// </summary>
    /// <remarks>This static constructor precomputes and caches digit maps for use by static members of the
    /// MathExtensions class. It is invoked automatically before any static member is accessed.</remarks>
    static MathExtensions()
    {
        digitsMaps = Enumerable.Range(0, 30).ToReadOnlyDictionary(i => i, i => 1 / Math.Pow(10, i));
        decimalDigitsMaps = Enumerable.Range(0, 30).ToReadOnlyDictionary(i => i, i => (decimal)(1 / Math.Pow(10, i)));
    }

    /// <summary>
    /// Determines whether the specified double-precision floating-point value is considered zero within a given number
    /// of decimal digits of precision.
    /// </summary>
    /// <remarks>This method is useful for determining whether a floating-point value is effectively zero,
    /// accounting for potential rounding errors. The supported digits values are determined by the implementation and
    /// may be limited.</remarks>
    /// <param name="doubleValue">The double-precision floating-point value to evaluate.</param>
    /// <param name="digits">The number of decimal digits to use for the zero comparison. Must be greater than zero.</param>
    /// <returns>true if the value is within the specified precision range of zero; otherwise, false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when digits is less than or equal to zero.</exception>
    /// <exception cref="NotSupportedException">Thrown when the specified digits value is not supported for the comparison.</exception>
    public static bool IsCoerceZore(this double doubleValue, int digits = 6)
    {
        _ = digits <= 0 ? throw new ArgumentOutOfRangeException(nameof(digits)) : 0;

        if (digitsMaps.TryGetValue(digits, out double offset) == false)
        {
            throw new NotSupportedException("invalid digits");
        }
        // 比较 doubleValue 是否在 -offset 和 offset 范围内。
        return -offset <= doubleValue && doubleValue <= offset;
    }

    /// <summary>
    /// Determines whether the specified floating-point value is considered zero within a given number of decimal digits
    /// of precision.
    /// </summary>
    /// <remarks>This method is useful for comparing floating-point values to zero with a configurable
    /// precision, helping to avoid issues with floating-point rounding errors.</remarks>
    /// <param name="floatValue">The floating-point value to evaluate for near-zero equivalence.</param>
    /// <param name="digits">The number of decimal digits to use for the zero comparison. Must be greater than zero.</param>
    /// <returns>true if the value is within the specified precision range of zero; otherwise, false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if digits is less than or equal to zero.</exception>
    /// <exception cref="NotSupportedException">Thrown if the specified digits value is not supported for comparison.</exception>
    public static bool IsCoerceZore(this float floatValue, int digits = 6)
    {
        _ = digits <= 0 ? throw new ArgumentOutOfRangeException(nameof(digits)) : 0;

        if (digitsMaps.TryGetValue(digits, out double offset) == false)
        {
            throw new NotSupportedException("invalid digits");
        }

        // 比较 floatValue 是否在 -offset 和 offset 范围内。
        return -offset <= floatValue && floatValue <= offset;
    }

    /// <summary>
    /// Determines whether the specified decimal value is considered effectively zero within a given number of decimal
    /// places.
    /// </summary>
    /// <remarks>This method is useful for comparing decimal values where small rounding errors may occur,
    /// such as in financial or scientific calculations. The method checks if the value falls within a symmetric range
    /// around zero, determined by the specified number of decimal places.</remarks>
    /// <param name="decimalValue">The decimal value to evaluate for effective zero.</param>
    /// <param name="digits">The number of decimal places to use when determining if the value is effectively zero. Must be greater than
    /// zero. Defaults to 6.</param>
    /// <returns>true if the absolute value of decimalValue is less than or equal to the threshold defined by digits; otherwise,
    /// false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when digits is less than or equal to zero.</exception>
    /// <exception cref="NotSupportedException">Thrown when the specified digits value is not supported.</exception>
    public static bool IsCoerceZore(this decimal decimalValue, int digits = 6)
    {
        _ = digits <= 0 ? throw new ArgumentOutOfRangeException(nameof(digits)) : 0;

        if (decimalDigitsMaps.TryGetValue(digits, out decimal offset) == false)
        {
            throw new NotSupportedException("invalid digits");
        }

        // 比较 decimalValue 是否在 -offset 和 offset 范围内。
        return -offset <= decimalValue && decimalValue <= offset;
    }

    /// <summary>
    /// Determines whether the specified double-precision floating-point value is equal to another value within a
    /// specified number of decimal digits of precision.
    /// </summary>
    /// <remarks>This method is useful for comparing floating-point values where exact equality is not
    /// reliable due to precision limitations. The supported values for digits depend on the implementation and may be
    /// limited.</remarks>
    /// <param name="input">The double-precision floating-point value to compare.</param>
    /// <param name="compare">The value to compare against.</param>
    /// <param name="digits">The number of decimal digits to use for the comparison. Must be greater than 0. The comparison checks if the
    /// values are equal within this precision. The default is 6.</param>
    /// <returns>true if the input value is equal to the compare value within the specified number of decimal digits; otherwise,
    /// false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if digits is less than or equal to 0.</exception>
    /// <exception cref="NotSupportedException">Thrown if the specified digits value is not supported by the implementation.</exception>
    public static bool CoerceEquals(this double input, double compare, int digits = 6)
    {
        _ = digits <= 0 ? throw new ArgumentOutOfRangeException(nameof(digits)) : 0;

        if (digitsMaps.TryGetValue(digits, out double offset) == false)
        {
            throw new NotSupportedException("invalid digits");
        }

        // 比较 input 是否在 compare - offset 和 compare + offset 范围内。
        return compare - offset <= input && input <= compare + offset;
    }

    /// <summary>
    /// Determines whether the specified floating-point value is equal to another value within a given number of decimal
    /// digits of precision.
    /// </summary>
    /// <remarks>This method performs a tolerance-based comparison, allowing for minor differences due to
    /// floating-point precision. The supported range of digits may be limited by the implementation.</remarks>
    /// <param name="input">The floating-point value to compare.</param>
    /// <param name="compare">The value to compare against.</param>
    /// <param name="digits">The number of decimal digits to use for the comparison. Must be greater than zero.</param>
    /// <returns>true if the two values are equal within the specified number of decimal digits; otherwise, false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if digits is less than or equal to zero.</exception>
    /// <exception cref="NotSupportedException">Thrown if the specified number of digits is not supported.</exception>
    public static bool CoerceEquals(this float input, float compare, int digits = 6)
    {
        _ = digits <= 0 ? throw new ArgumentOutOfRangeException(nameof(digits)) : 0;

        if (digitsMaps.TryGetValue(digits, out double offset) == false)
        {
            throw new NotSupportedException("invalid digits");
        }

        // 比较 input 是否在 compare - offset 和 compare + offset 范围内。
        return compare - offset <= input && input <= compare + offset;
    }

    /// <summary>
    /// Determines whether the specified decimal value is equal to a comparison value within a given number of decimal
    /// places.
    /// </summary>
    /// <remarks>This method checks whether the input value falls within the range defined by compare ±
    /// offset, where offset is determined by the number of decimal places specified. This is useful for comparing
    /// decimal values where minor differences due to rounding or precision are acceptable.</remarks>
    /// <param name="input">The decimal value to compare.</param>
    /// <param name="compare">The decimal value to compare against.</param>
    /// <param name="digits">The number of decimal places to use when determining equality. Must be greater than zero.</param>
    /// <returns>true if the input value is within the specified number of decimal places of the comparison value; otherwise,
    /// false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if digits is less than or equal to zero.</exception>
    /// <exception cref="NotSupportedException">Thrown if the specified number of digits is not supported.</exception>
    public static bool CoerceEquals(this decimal input, decimal compare, int digits = 6)
    {
        _ = digits <= 0 ? throw new ArgumentOutOfRangeException(nameof(digits)) : 0;

        if (decimalDigitsMaps.TryGetValue(digits, out decimal offset) == false)
        {
            throw new NotSupportedException("invalid digits");
        }

        // 比较 input 是否在 compare - offset 和 compare + offset 范围内。
        return compare - offset <= input && input <= compare + offset;
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static byte CoerceIn(this byte input, byte minValue, byte maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static short CoerceIn(this short input, short minValue, short maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static int CoerceIn(this int input, int minValue, int maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static long CoerceIn(this long input, long minValue, long maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static float CoerceIn(this float input, float minValue, float maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static double CoerceIn(this double input, double minValue, double maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static decimal CoerceIn(this decimal input, decimal minValue, decimal maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static sbyte CoerceIn(this sbyte input, sbyte minValue, sbyte maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static ushort CoerceIn(this ushort input, ushort minValue, ushort maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static uint CoerceIn(this uint input, uint minValue, uint maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Restricts a byte value to be within the specified minimum and maximum bounds.
    /// </summary>
    /// <remarks>If input is less than minValue, minValue is returned. If input is greater than maxValue,
    /// maxValue is returned. Otherwise, input is returned unchanged.</remarks>
    /// <param name="input">The value to constrain within the specified range.</param>
    /// <param name="minValue">The inclusive lower bound to which the value will be coerced if it is less than this value.</param>
    /// <param name="maxValue">The inclusive upper bound to which the value will be coerced if it is greater than this value.</param>
    /// <returns>A byte value that is no less than minValue and no greater than maxValue.</returns>
    public static ulong CoerceIn(this ulong input, ulong minValue, ulong maxValue)
    {
        return input <= minValue ? minValue : (input >= maxValue ? maxValue : input);
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static byte CoerceAtLeast(this byte input, byte minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static short CoerceAtLeast(this short input, short minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static int CoerceAtLeast(this int input, int minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static long CoerceAtLeast(this long input, long minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static float CoerceAtLeast(this float input, float minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static double CoerceAtLeast(this double input, double minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static decimal CoerceAtLeast(this decimal input, decimal minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static sbyte CoerceAtLeast(this sbyte input, sbyte minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static ushort CoerceAtLeast(this ushort input, ushort minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static uint CoerceAtLeast(this uint input, uint minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is greater than or equal to the specified minimum value; otherwise, returns the
    /// minimum value.
    /// </summary>
    /// <param name="input">The value to compare against the minimum value.</param>
    /// <param name="minValue">The minimum allowable value to which the input will be coerced if it is less than this value.</param>
    /// <returns>The input value if it is greater than or equal to minValue; otherwise, minValue.</returns>
    public static ulong CoerceAtLeast(this ulong input, ulong minValue)
    {
        return input <= minValue ? minValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static byte CoerceAtMost(this byte input, byte maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static short CoerceAtMost(this short input, short maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static int CoerceAtMost(this int input, int maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static long CoerceAtMost(this long input, long maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static float CoerceAtMost(this float input, float maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static double CoerceAtMost(this double input, double maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static decimal CoerceAtMost(this decimal input, decimal maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static sbyte CoerceAtMost(this sbyte input, sbyte maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static ushort CoerceAtMost(this ushort input, ushort maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static uint CoerceAtMost(this uint input, uint maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns the input value if it is less than or equal to the specified maximum value; otherwise, returns the
    /// maximum value.
    /// </summary>
    /// <param name="input">The value to compare against the maximum value.</param>
    /// <param name="maxValue">The maximum allowable value. If the input exceeds this value, the method returns maxValue.</param>
    /// <returns>The input value if it is less than or equal to maxValue; otherwise, maxValue.</returns>
    public static ulong CoerceAtMost(this ulong input, ulong maxValue)
    {
        return input >= maxValue ? maxValue : input;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static byte CoerceWhen(this byte trueValue, bool condition, byte falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static short CoerceWhen(this short trueValue, bool condition, short falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static int CoerceWhen(this int trueValue, bool condition, int falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static long CoerceWhen(this long trueValue, bool condition, long falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static float CoerceWhen(this float trueValue, bool condition, float falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static double CoerceWhen(this double trueValue, bool condition, double falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static decimal CoerceWhen(this decimal trueValue, bool condition, decimal falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static sbyte CoerceWhen(this sbyte trueValue, bool condition, sbyte falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static ushort CoerceWhen(this ushort trueValue, bool condition, ushort falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static uint CoerceWhen(this uint trueValue, bool condition, uint falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Returns one of two input values depending on the specified condition.
    /// </summary>
    /// <param name="trueValue">The byte value to return if <paramref name="condition"/> is <see langword="true"/>.</param>
    /// <param name="condition">A value indicating which byte value to return. If <see langword="true"/>, <paramref name="trueValue"/> is
    /// returned; otherwise, <paramref name="falseValue"/> is returned.</param>
    /// <param name="falseValue">The byte value to return if <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns>The value of <paramref name="trueValue"/> if <paramref name="condition"/> is <see langword="true"/>; otherwise,
    /// the value of <paramref name="falseValue"/>.</returns>
    public static ulong CoerceWhen(this ulong trueValue, bool condition, ulong falseValue)
    {
        return condition ? trueValue : falseValue;
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this double input, double minValue, double maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this float input, float minValue, float maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this decimal input, decimal minValue, decimal maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this int input, int minValue, int maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this short input, short minValue, short maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this byte input, byte minValue, byte maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this sbyte input, sbyte minValue, sbyte maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this ushort input, ushort minValue, ushort maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this uint input, uint minValue, uint maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Determines whether the specified value falls within the given range, with an option to include or exclude the
    /// boundary values.
    /// <para> 1 : <![CDATA[minValue <= input && input <= maxValue]]>   ( includeEquals == <see langword="true"/> ) </para>
    /// <para> 2 : <![CDATA[minValue < input && input < maxValue]]> </para>
    /// </summary>
    /// <remarks>If minValue is greater than maxValue, the method will return false for all input values. This
    /// method is typically used to validate that a value is within an expected numeric interval.</remarks>
    /// <param name="input">The value to test for inclusion within the specified range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false to exclude them. The default is
    /// true.</param>
    /// <returns>true if the value is within the specified range according to the boundary inclusion setting; otherwise, false.</returns>
    public static bool VerifyIn(this ulong input, ulong minValue, ulong maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }

    /// <summary>
    /// Calculates the absolute value of a short integer.
    /// </summary>
    /// <param name="value">The short integer for which the absolute value is calculated.</param>
    /// <returns>The absolute value of the provided short integer.</returns>
    public static short Abs(this short value)
    {
        return Math.Abs(value);
    }

    /// <summary>
    /// Calculates the absolute value of a signed byte.
    /// </summary>
    /// <param name="value">The signed byte for which the absolute value is calculated.</param>
    /// <returns>The absolute value of the provided signed byte.</returns>
    public static sbyte Abs(this sbyte value)
    {
        return Math.Abs(value);
    }

    /// <summary>
    /// Calculates the absolute value of an integer.
    /// </summary>
    /// <param name="value">The integer for which the absolute value is to be calculated.</param>
    /// <returns>The absolute value of the provided integer.</returns>
    public static int Abs(this int value)
    {
        return Math.Abs(value);
    }

    /// <summary>
    /// Calculates the absolute value of a long integer.
    /// </summary>
    /// <param name="value">The long integer for which the absolute value is calculated.</param>
    /// <returns>The absolute value of the provided long integer.</returns>
    public static long Abs(this long value)
    {
        return Math.Abs(value);
    }

    /// <summary>
    /// Calculates the absolute value of a floating-point number.
    /// </summary>
    /// <param name="value">The floating-point number for which the absolute value is calculated.</param>
    /// <returns>The absolute value of the provided floating-point number.</returns>
    public static float Abs(this float value)
    {
        return Math.Abs(value);
    }

    /// <summary>
    /// Calculates the absolute value of a double-precision floating-point number.
    /// </summary>
    /// <param name="value">The number for which the absolute value is to be calculated.</param>
    /// <returns>The absolute value of the provided double number.</returns>
    public static double Abs(this double value)
    {
        return Math.Abs(value);
    }

    /// <summary>
    /// Calculates the absolute value of a decimal number.
    /// </summary>
    /// <param name="value">The decimal number for which the absolute value is calculated.</param>
    /// <returns>The absolute value of the provided decimal number.</returns>
    public static decimal Abs(this decimal value)
    {
        return Math.Abs(value);
    }

    /// <summary>
    /// Rounds a decimal value to a specified number of decimal places using a defined rounding mode.
    /// </summary>
    /// <param name="value">The decimal number to be rounded.</param>
    /// <param name="decimals">Specifies the number of decimal places to round to.</param>
    /// <param name="mode">Determines the rounding strategy to apply when the value is exactly halfway between two others.</param>
    /// <returns>The rounded decimal value based on the provided parameters.</returns>
    public static decimal Round(this decimal value, int decimals = 2, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        return Math.Round(value, decimals, mode);
    }

    /// <summary>
    /// Rounds a double to a specified number of decimal places using a defined rounding mode.
    /// </summary>
    /// <param name="value">The number to be rounded.</param>
    /// <param name="decimals">Specifies how many decimal places to round to.</param>
    /// <param name="mode">Determines the rounding strategy when the value is exactly halfway between two others.</param>
    /// <returns>The rounded double value.</returns>
    public static double Round(this double value, int decimals = 2, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        return Math.Round(value, decimals, mode);
    }

    /// <summary>
    /// Rounds a floating-point number to a specified number of decimal places using a defined rounding mode.
    /// </summary>
    /// <param name="value">The number to be rounded.</param>
    /// <param name="decimals">Specifies how many decimal places to round to.</param>
    /// <param name="mode">Determines the rounding strategy to apply when the number is exactly halfway between two others.</param>
    /// <returns>The rounded floating-point number.</returns>
    public static float Round(this float value, int decimals = 2, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        return (float)Math.Round((double)value, decimals, mode);
    }

    /// <summary>
    /// Converts a non-negative decimal number to its string representation in the specified base.
    /// </summary>
    /// <remarks>The returned string uses uppercase letters A–Z for digit values 10 through 35. The method
    /// does not handle negative numbers.</remarks>
    /// <param name="decimalNumber">The non-negative decimal number to convert.</param>
    /// <param name="targetBase">The base to convert to. Must be in the range 2 to 36, inclusive.</param>
    /// <returns>A string representing the converted number in the specified base.</returns>
    /// <exception cref="ArgumentException">Thrown if targetBase is less than 2 or greater than 36.</exception>
    public static string BaseConversion(long decimalNumber, byte targetBase)
    {
        _ = targetBase is < 2 or > 36 ? throw new ArgumentException("target base must be 2 ~ 36") : 0;

        char[] chars = Pick(decimalNumber, targetBase).Reverse().ToArray();

        return new string(chars);

        static IEnumerable<char> Pick(long decimalNumber, byte targetBase)
        {
            long quotient = decimalNumber;

            while (quotient > 0)
            {
                long remainder = quotient % targetBase;

                quotient /= targetBase;

                yield return charArray[remainder];
            }
        }
    }

    #region number


    /// <summary>
    /// Determines if the provided object is a numeric type and outputs its value as a double.
    /// </summary>
    /// <param name="obj">The object to be checked for numeric type.</param>
    /// <param name="doubleValue">The variable that receives the numeric value if the object is a valid number.</param>
    /// <returns>True if the object is a numeric type; otherwise, false.</returns>
    public static bool IsNumber(this object? obj, out double doubleValue)
    {
        doubleValue = 0d;

        if (obj is null)
        {
            return false;
        }

        if (obj is sbyte @sbyte)
        {
            doubleValue = @sbyte;
            return true;
        }

        if (obj is byte @byte)
        {
            doubleValue = @byte;
            return true;
        }

        if (obj is short @short)
        {
            doubleValue = @short;
            return true;
        }
        if (obj is ushort @ushort)
        {
            doubleValue = @ushort;
            return true;
        }
        if (obj is int @int)
        {
            doubleValue = @int;
            return true;
        }
        if (obj is uint @uint)
        {
            doubleValue = @uint;
            return true;
        }
        if (obj is long @long)
        {
            doubleValue = @long;
            return true;
        }
        if (obj is ulong @ulong)
        {
            doubleValue = @ulong;
            return true;
        }
        if (obj is float @false)
        {
            doubleValue = @false;
            return true;
        }
        if (obj is double @double)
        {
            doubleValue = @double;
            return true;
        }
        if (obj is decimal @decimal)
        {
            doubleValue = (double)@decimal;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the provided value is a numeric type.
    /// </summary>
    /// <param name="target">The value to be evaluated for its numeric type.</param>
    /// <returns>True if the value is a numeric type; otherwise, false.</returns>
    public static bool IsNumber(this object target)
    {
        return target is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    /// <summary>
    /// Defines static Type fields for various numeric data types in C#. These include signed and unsigned integers,
    /// floating-point, and decimal types.
    /// </summary>
    private static readonly Type sbyteType = typeof(sbyte),
        byteType = typeof(byte),
        shortType = typeof(short),
        ushortType = typeof(ushort),
        intType = typeof(int),
        uintType = typeof(uint),
        longType = typeof(long),
        ulongType = typeof(ulong),
        floatType = typeof(float),
        doubleType = typeof(double),
        decimalType = typeof(decimal);

    /// <summary>
    /// Determines if a specified type is a numeric type.
    /// </summary>
    /// <param name="target">The type to check for being a numeric type.</param>
    /// <returns>True if the type is numeric; otherwise, false.</returns>
    public static bool IsNumberType(this Type target)
    {
        return target == sbyteType || target == byteType || target == shortType || target == ushortType || target == intType || target == uintType || target == longType || target == ulongType || target == floatType || target == doubleType || target == decimalType;
    }

    /// <summary>
    /// Checks if the provided value is a numeric type.
    /// </summary>
    /// <typeparam name="T">Represents the type of the value being checked for numeric characteristics.</typeparam>
    /// <param name="target">The value to be evaluated for its numeric type.</param>
    /// <returns>True if the value is a numeric type; otherwise, false.</returns>
    public static bool IsNumberCollection<T>(this T target)
    {
        return target is ICollection<sbyte> or ICollection<byte> or ICollection<short> or ICollection<ushort> or ICollection<int> or ICollection<uint> or ICollection<long> or ICollection<ulong> or ICollection<float> or ICollection<double> or ICollection<decimal>;
    }

    /// <summary>
    /// Defines static Type fields for various numeric data types in C#. These include signed and unsigned integers,
    /// floating-point, and decimal types.
    /// </summary>
    private static readonly Type sbyteCollectionType = typeof(ICollection<sbyte>),
        byteCollectionType = typeof(ICollection<byte>),
        shortCollectionType = typeof(ICollection<short>),
        ushortCollectionType = typeof(ICollection<ushort>),
        intCollectionType = typeof(ICollection<int>),
        uintCollectionType = typeof(ICollection<uint>),
        longCollectionType = typeof(ICollection<long>),
        ulongCollectionType = typeof(ICollection<ulong>),
        floatCollectionType = typeof(ICollection<float>),
        doubleCollectionType = typeof(ICollection<double>),
        decimalCollectionType = typeof(ICollection<decimal>);

    /// <summary>
    /// Determines if a specified type is a numeric type.
    /// </summary>
    /// <param name="target">The type to check for being a numeric type.</param>
    /// <returns>True if the type is numeric; otherwise, false.</returns>
    public static bool IsNumberCollectionType(this Type target)
    {
        return target == sbyteCollectionType
            || target == byteCollectionType
            || target == shortCollectionType
            || target == ushortCollectionType
            || target == intCollectionType
            || target == uintCollectionType
            || target == longCollectionType
            || target == ulongCollectionType
            || target == floatCollectionType
            || target == doubleCollectionType
            || target == decimalCollectionType;
    }

    #endregion
}
