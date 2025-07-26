using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Tryit.Wpf;

/// <summary>
/// Transforms an object into its string representation using a specified converter function. Indicates if the object
/// can be printed.
/// </summary>
public class PrintConverter : IValueConverter
{
    /// <summary>
    /// A static function that takes an object and returns a string. It is marked to never be displayed in the debugger.
    /// </summary>
    [DebuggerBrowsable(Never)]
    static Func<object, string>? func;

    /// <summary>
    /// Sets a converter function that transforms an object into its string representation.
    /// </summary>
    /// <param name="func">The function used to convert an object to a string.</param>
    public static void SetObjectToStringConverter(Func<object, string> func)
    {
        PrintConverter.func = func;
    }

    /// <summary>
    /// Indicates whether the object can be printed. It is a boolean property that can be set or retrieved.
    /// </summary>
    public bool Printable { get; set; }

    /// <summary>
    /// Converts a value to a specified target type, optionally using a parameter and culture information.
    /// </summary>
    /// <param name="value">The input value to be converted, which can be of any type.</param>
    /// <param name="targetType">Specifies the type to which the input value should be converted.</param>
    /// <param name="parameter">An optional parameter that can influence the conversion process.</param>
    /// <param name="culture">Provides culture-specific information that may affect the conversion.</param>
    /// <returns>Returns the original value after processing, potentially modified based on the conversion logic.</returns>
    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (Printable)
        {
            var display = func?.Invoke(value!) ?? value;
            Trace.WriteLine(display);
        }

        return value;
    }

    /// <summary>
    /// Converts a value back to its original form based on the specified target type and culture.
    /// </summary>
    /// <param name="value">The value to be converted back to its original form.</param>
    /// <param name="targetType">Specifies the type to which the value should be converted.</param>
    /// <param name="parameter">Provides additional context or settings for the conversion process.</param>
    /// <param name="culture">Indicates the cultural information that may affect the conversion.</param>
    /// <returns>Returns the converted value, which may be of a different type.</returns>
    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
