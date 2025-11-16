using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Converts input objects to strings, returning a default string for null values. Evaluates strings to return true for
/// non-empty and false for empty or whitespace strings.
/// </summary>
public class NotNullOrWhiteSpaceConverter : TrueFalseConverter<string>
{
    /// <summary>
    /// Converts an input object to a string, handling null values by returning a default string.
    /// </summary>
    /// <param name="value">An object that may be null or of another type, which will be converted to a string if not null.</param>
    /// <returns>A string representation of the input object or a default string if the input is null.</returns>
    protected override string InputConvert(object? value)
    {
        if (value is null)
        {
            return default!;
        }

        return base.InputConvert(value);
    }

    /// <summary>
    /// Converts a string to a boolean value based on its content. Returns true for non-empty strings and false for
    /// empty or whitespace strings.
    /// </summary>
    /// <param name="value">The string input that is evaluated for its content.</param>
    /// <param name="targetType">Specifies the type to which the string is being converted.</param>
    /// <param name="parameter">An optional object that can provide additional context for the conversion.</param>
    /// <param name="culture">Indicates the culture-specific formatting to consider during conversion.</param>
    /// <returns>Returns a boolean indicating whether the input string is non-empty.</returns>
    protected override object? Convert(string value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value) == false)
        {
            return True;
        }

        return False;
    }
}

/// <summary>
/// Provides a markup extension that converts a string value to a Boolean based on whether the string is not null,
/// empty, or consists only of white-space characters. Returns <see langword="true"/> if the input string contains
/// non-white-space characters; otherwise, returns <see langword="false"/>.
/// </summary>
/// <remarks>This extension is typically used in XAML bindings to enable or disable UI elements based on whether a
/// string property contains meaningful content. It can be useful for form validation scenarios or conditional UI
/// logic.</remarks>
public class NotNullOrWhiteSpaceConverterExtension : TrueFalseConverterExtension<NotNullOrWhiteSpaceConverter, string> { }
