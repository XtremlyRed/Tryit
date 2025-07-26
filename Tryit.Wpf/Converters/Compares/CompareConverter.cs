using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Abstract class for converting comparable values to boolean based on specified comparison mode. It supports various
/// comparison operations.
/// </summary>
public abstract class CompareConverter : TrueFalseConverter<IComparable>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private readonly CompareMode compareMode;

    /// <summary>
    /// Initializes a new instance of the CompareConverter class with a specified mode for comparison.
    /// </summary>
    /// <param name="compareMode">Specifies the mode used for comparison operations.</param>
    protected CompareConverter(CompareMode compareMode)
    {
        this.compareMode = compareMode;
    }

    /// <summary>
    /// Represents an input value that can be compared. It can hold any object that implements the IComparable interface.
    /// </summary>
    public IComparable? Input { get; set; }

    /// <summary>
    /// Converts a comparable value to a boolean based on a matching condition.
    /// </summary>
    /// <param name="value">The input value that needs to be compared against a predefined input.</param>
    /// <param name="targetType">Specifies the type to which the value should be converted.</param>
    /// <param name="parameter">An optional parameter that may influence the conversion process.</param>
    /// <param name="culture">Defines the culture-specific formatting for the conversion.</param>
    /// <returns>Returns true if the condition is met, otherwise returns false.</returns>
    protected override object? Convert(IComparable value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool condiction = Match(value, Input, compareMode);

        return condiction ? True : False;
    }

    /// <summary>
    /// Compares two values based on a specified comparison mode and returns a boolean result.
    /// </summary>
    /// <param name="value">The value to be compared against another value.</param>
    /// <param name="comparer">The value used for comparison with the first value.</param>
    /// <param name="compareMode">Specifies the type of comparison to perform between the two values.</param>
    /// <returns>A boolean indicating whether the comparison condition is met.</returns>
    public static bool Match(IComparable? value, IComparable? comparer, CompareMode? compareMode)
    {
        return compareMode switch
        {
            CompareMode.Equal => value!.CompareTo(comparer) == 0,
            CompareMode.NotEqual => value!.CompareTo(comparer) != 0,
            CompareMode.GreaterThan => value!.CompareTo(comparer) > 0,
            CompareMode.GreaterThanOrEqual => value!.CompareTo(comparer) >= 0,
            CompareMode.LessThan => value!.CompareTo(comparer) < 0,
            CompareMode.LessThanOrEqual => value!.CompareTo(comparer) <= 0,
            _ => true,
        };
    }
}

/// <summary>
/// Defines comparison modes for evaluating values, including equal, not equal, greater than, less than, and their
/// inclusive counterparts.
/// </summary>
public enum CompareMode
{
    /// <summary>
    /// Checks if two objects are equal. Returns a boolean indicating whether the objects are the same.
    /// </summary>
    Equal,

    /// <summary>
    /// Checks if two values are not equal. Returns true if they are different, false if they are the same.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Checks if a value is greater than another value. Useful for comparisons in conditional statements.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Checks if a value is greater than or equal to another value. Returns a boolean result based on the comparison.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Compares two values and returns true if the first value is less than the second value. Useful for sorting and
    /// conditional checks.
    /// </summary>
    LessThan,

    /// <summary>
    /// Compares two values and returns true if the first value is less than or equal to the second value.
    /// </summary>
    LessThanOrEqual,
}
