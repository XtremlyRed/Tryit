using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Converts a value to a boolean based on whether it is null. Returns True for null values and False otherwise. Also
/// ensures the input value is non-null when converted to an object.
/// </summary>
public class NullConverter : TrueFalseConverter<object>
{
    /// <summary>
    /// Converts a given value to a boolean based on its nullity.
    /// </summary>
    /// <param name="value">The input value that is evaluated for nullity.</param>
    /// <param name="targetType">Specifies the type to which the value should be converted.</param>
    /// <param name="parameter">An optional parameter that can influence the conversion process.</param>
    /// <param name="culture">Indicates the culture information that may affect the conversion.</param>
    /// <returns>Returns True if the input value is null; otherwise, returns False.</returns>
    protected override object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? True : False;
    }

    /// <summary>
    /// Converts the input value to an object type, ensuring it is not null.
    /// </summary>
    /// <param name="value">The input can be any object, including null, which will be handled by the conversion process.</param>
    /// <returns>Returns the input value as an object, guaranteed to be non-null.</returns>
    protected override object InputConvert(object? value)
    {
        return value!;
    }
}

public class NullConverterExtension : TrueFalseConverterExtension<NullConverter, object> { }
