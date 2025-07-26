using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Converts input values to strings while handling nulls, and checks if a string is null or whitespace to return a
/// boolean value. Handles conversions based on specific conditions.
/// </summary>
public class NullOrWhiteSpaceConverter : TrueFalseConverter<string>
{
    /// <summary>
    /// Converts an input value to a string, handling null values appropriately.
    /// </summary>
    /// <param name="value">The input can be any object, which will be converted to a string if not null.</param>
    /// <returns>Returns a string representation of the input value or a default value if the input is null.</returns>
    protected override string InputConvert(object? value)
    {
        if (value is null)
        {
            return default!;
        }

        return base.InputConvert(value);
    }

    /// <summary>
    /// Converts a string value to an object based on specific conditions. It checks if the string is null or whitespace.
    /// </summary>
    /// <param name="value">The string input that is evaluated for null or whitespace.</param>
    /// <param name="targetType">Specifies the type to which the string value is being converted.</param>
    /// <param name="parameter">An optional object that can provide additional context for the conversion.</param>
    /// <param name="culture">Defines the culture-specific formatting for the conversion process.</param>
    /// <returns>Returns a boolean value indicating whether the string is not null or whitespace.</returns>
    protected override object? Convert(string value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value) == false)
        {
            return False;
        }

        return True;
    }
}

public class NullOrWhiteSpaceConverterExtension : TrueFalseConverterExtension<NullOrWhiteSpaceConverter, string> { }
