using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Converts a boolean value to a target type, returning True or False based on the input value.  Handles
/// culture-specific conversions.
/// </summary>
public class BooleanConverter : TrueFalseConverter<bool>
{
    /// <summary>
    /// Converts a boolean value to an object based on its truthiness. Returns a specific object for true or false values.
    /// </summary>
    /// <param name="value">The boolean input that determines which object to return.</param>
    /// <param name="targetType">Specifies the type of the object to be returned based on the conversion.</param>
    /// <param name="parameter">An optional parameter that can influence the conversion process.</param>
    /// <param name="culture">Provides culture-specific information that may affect the conversion.</param>
    /// <returns>Returns an object representing true or false based on the input boolean value.</returns>
    protected override object? Convert(bool value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value ? True : False;
    }
}

public class BooleanConverterExtension : TrueFalseConverterExtension<BooleanConverter, bool> { }
