using System.Globalization;

namespace Tryit.Wpf;

/// <summary>
/// Converts a numeric value to a boolean based on whether it falls within a specified range. It includes options for
/// minimum and maximum values.
/// </summary>
public class OutOfRangeConverter : TrueFalseConverter<object>
{
    /// <summary>
    /// Represents the minimum value for a certain range or set of data. It can be both retrieved and modified.
    /// </summary>
    public double MinValue { get; set; }

    /// <summary>
    /// Represents the maximum value that can be assigned. It is a double type property with both get and set accessors.
    /// </summary>
    public double MaxValue { get; set; }

    /// <summary>
    /// Indicates whether equality comparisons should be included. It is a boolean property that can be set or retrieved.
    /// </summary>
    public bool IncludeEquals { get; set; }

    /// <summary>
    /// Converts a numeric value to a boolean indicating if it falls within specified bounds.
    /// </summary>
    /// <param name="value">The numeric value to be evaluated against the defined range.</param>
    /// <param name="targetType">Specifies the type to which the value is being converted.</param>
    /// <param name="parameter">An optional parameter that can be used for additional conversion logic.</param>
    /// <param name="culture">Defines the culture-specific formatting for the conversion process.</param>
    /// <returns>Returns true if the value is within the specified range, otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided value is not a valid numeric type.</exception>
    protected override object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
        {
            var dValue = System.Convert.ToDouble(value);

            if (IncludeEquals && dValue.CompareTo(MinValue) <= 0 && MaxValue.CompareTo(dValue) >= 0)
            {
                return True;
            }
            else if (dValue.CompareTo(MinValue) < 0 && MaxValue.CompareTo(dValue) > 0)
            {
                return True;
            }

            return False;
        }

        throw new ArgumentException("invalid number type");
    }
}

/// <summary>
///
/// </summary>
public class OutOfRangeConverterExtension : TrueFalseConverterExtension<OutOfRangeConverter, object>
{
    /// <summary>
    /// 指定范围的最小值。用于判断数值是否在指定范围之外。
    /// </summary>
    public double MinValue { get; set; }

    /// <summary>
    /// 指定范围的最大值。用于判断数值是否在指定范围之外。
    /// </summary>
    public double MaxValue { get; set; }

    /// <summary>
    /// 指定是否包含等于最小值或最大值的情况在范围判断中。
    /// </summary>
    public bool IncludeEquals { get; set; }

    /// <summary>
    /// 创建并配置 <see cref="OutOfRangeConverter"/> 实例，将扩展属性赋值到转换器。
    /// </summary>
    /// <returns>配置好的 <see cref="OutOfRangeConverter"/> 实例。</returns>
    protected override OutOfRangeConverter ProvideValue()
    {
        var convert = base.ProvideValue();
        convert.MaxValue = MaxValue;
        convert.MinValue = MinValue;
        convert.IncludeEquals = IncludeEquals;

        return convert;
    }
}
