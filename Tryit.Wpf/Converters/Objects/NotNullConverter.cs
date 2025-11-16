using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// NotNullConverter evaluates an input value for nullity, returning true if not null and false otherwise. It also
/// converts input values to non-nullable object types.
/// </summary>
public class NotNullConverter : TrueFalseConverter<object>
{
    /// <summary>
    /// Converts a given value to a boolean based on its nullability.
    /// </summary>
    /// <param name="value">The input value that is evaluated for nullity.</param>
    /// <param name="targetType">Specifies the type to which the value is being converted.</param>
    /// <param name="parameter">An optional parameter that can influence the conversion process.</param>
    /// <param name="culture">Provides culture-specific information for the conversion.</param>
    /// <returns>Returns true if the input value is not null; otherwise, returns false.</returns>
    protected override object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null ? True : False;
    }

    /// <summary>
    /// Converts the input value to a non-nullable object type.
    /// </summary>
    /// <param name="value">The input can be any object, including null.</param>
    /// <returns>Returns the input value as a non-nullable object.</returns>
    protected override object InputConvert(object? value)
    {
        return value!;
    }
}

/// <summary>
/// Provides a markup extension that supplies a value converter which returns a Boolean indicating whether a value is
/// not null.
/// </summary>
/// <remarks>This extension is typically used in XAML to bind to a Boolean property that reflects whether a bound
/// value is not null. It is useful for enabling or disabling UI elements based on the presence of a value.</remarks>
public class NotNullConverterExtension : TrueFalseConverterExtension<NotNullConverter, object> { }
