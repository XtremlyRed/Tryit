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

    protected override InRangeConverter ProvideValue()
    {
        var convert = base.ProvideValue();
        convert.MaxValue = MaxValue;
        convert.MinValue = MinValue;
        convert.IncludeEquals = IncludeEquals;

        return convert;
    }
}
