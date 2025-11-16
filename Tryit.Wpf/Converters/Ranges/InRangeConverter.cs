using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Tryit.Wpf;

/// <summary>
/// InRangeConverter checks if a numeric value is within a specified range defined by MinValue and MaxValue. It can
/// include equality in comparisons.
/// </summary>
public class InRangeConverter : TrueFalseConverter<object>
{
    /// <summary>
    /// Represents the minimum value for a certain range or calculation. It can be both retrieved and modified.
    /// </summary>
    public double MinValue { get; set; }

    /// <summary>
    /// Represents the maximum value that can be assigned. It is a double type property with both get and set accessors.
    /// </summary>
    public double MaxValue { get; set; }

    /// <summary>
    /// Indicates whether equality comparisons should be included.
    /// </summary>
    public bool IncludeEquals { get; set; }

    /// <summary>
    /// Converts a numeric value to a boolean indicating if it falls within specified minimum and maximum bounds.
    /// </summary>
    /// <param name="value">The numeric value to be evaluated against the defined range.</param>
    /// <param name="targetType">Specifies the type to which the value is being converted.</param>
    /// <param name="parameter">An optional parameter that can be used for additional conversion settings.</param>
    /// <param name="culture">Defines the culture-specific formatting for the conversion process.</param>
    /// <returns>Returns true if the value is within the specified range; otherwise, returns false.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided value is not a valid numeric type.</exception>
    protected override object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
        {
            var dValue = System.Convert.ToDouble(value);

            if (IncludeEquals && dValue.CompareTo(MinValue) >= 0 && MaxValue.CompareTo(dValue) >= 0)
            {
                return True;
            }
            else if (dValue.CompareTo(MinValue) > 0 && MaxValue.CompareTo(dValue) > 0)
            {
                return True;
            }

            return False;
        }

        throw new ArgumentException("invalid number type");
    }
}

/// <summary>
/// Provides a markup extension that supplies an <see cref="InRangeConverter"/> configured to determine whether a value
/// falls within a specified numeric range. Enables declarative range checking in XAML data binding scenarios.
/// </summary>
/// <remarks>Use <see cref="InRangeConverterExtension"/> in XAML to create a value converter that evaluates
/// whether a bound value is within the range defined by <see cref="MinValue"/> and <see cref="MaxValue"/>. The <see
/// cref="IncludeEquals"/> property controls whether the range comparison includes the boundary values. This extension
/// is typically used to simplify range validation logic in UI bindings.</remarks>
public class InRangeConverterExtension : TrueFalseConverterExtension<InRangeConverter, object>
{
    /// <summary>
    /// Represents the minimum value for a certain range or calculation. It can be both retrieved and modified.
    /// </summary>
    public double MinValue { get; set; }

    /// <summary>
    /// Represents the maximum value that can be assigned. It is a double type property with both get and set accessors.
    /// </summary>
    public double MaxValue { get; set; }

    /// <summary>
    /// Indicates whether equality comparisons should be included. It is a boolean property that can be set to true or
    /// false.
    /// </summary>
    public bool IncludeEquals { get; set; }

    /// <summary>
    /// Creates and returns a configured instance of the range converter for use in value conversion scenarios.
    /// </summary>
    /// <remarks>This method is typically called by XAML infrastructure to provide a value converter instance
    /// with the appropriate configuration. The returned converter reflects the current property values of the
    /// containing markup extension.</remarks>
    /// <returns>A configured <see cref="InRangeConverter"/> instance with the current <see cref="MaxValue"/>, <see
    /// cref="MinValue"/>, and <see cref="IncludeEquals"/> settings applied.</returns>
    protected override InRangeConverter ProvideValue()
    {
        var convert = base.ProvideValue();
        convert.MaxValue = MaxValue;
        convert.MinValue = MinValue;
        convert.IncludeEquals = IncludeEquals;

        return convert;
    }
}
