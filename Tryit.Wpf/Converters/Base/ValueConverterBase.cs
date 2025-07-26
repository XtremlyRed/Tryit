using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// An abstract class for converting values between different types, implementing IValueConverter.
/// </summary>
/// <typeparam name="Input">Represents the type of the value to be converted.</typeparam>
/// <typeparam name="InputParameter">Represents the type of the parameter used during the conversion process.</typeparam>
public abstract partial class ValueConverterBase<Input, InputParameter> : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Converts an input value to a specified target type using optional parameters and culture information.
    /// </summary>
    /// <param name="value">The input data that needs to be converted to a different type.</param>
    /// <param name="targetType">The type to which the input value should be converted.</param>
    /// <param name="parameter">Optional additional information that may influence the conversion process.</param>
    /// <param name="culture">Specifies the cultural context for the conversion, affecting formatting and parsing.</param>
    /// <returns>The converted value, which may be null if the conversion fails.</returns>
    protected abstract object? Convert(Input value, Type targetType, InputParameter? parameter, CultureInfo culture);

    /// <summary>
    /// Converts a value to a target type using specified parameters and culture information.
    /// </summary>
    /// <param name="value">The value to be converted to the target type.</param>
    /// <param name="targetType">The type that the value should be converted to.</param>
    /// <param name="parameter">Additional information that may influence the conversion process.</param>
    /// <param name="culture">Cultural information that may affect the conversion format.</param>
    /// <returns>The converted value based on the provided inputs.</returns>
    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var targetValue = InputConvert(value);
        var targetParameter = InputParameterConvert(parameter);
        return this.Convert(targetValue, targetType, targetParameter, culture);
    }

    /// <summary>
    /// Converts an object to an Input type, throwing an exception if the conversion fails.
    /// </summary>
    /// <param name="value">An object that is expected to be of Input type for successful conversion.</param>
    /// <returns>The converted Input object if the input value is of the correct type.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided object is not of Input type.</exception>
    protected virtual Input InputConvert(object? value)
    {
        if (value is not Input targetValue)
        {
            throw new ArgumentException($"current value type is not {typeof(Input).FullName}");
        }

        return targetValue;
    }

    /// <summary>
    /// Converts an object to an InputParameter type, throwing an exception if the conversion fails.
    /// </summary>
    /// <param name="value">An object that is expected to be of InputParameter type for successful conversion.</param>
    /// <returns>The converted InputParameter if the input value is of the correct type.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided value is not of InputParameter type.</exception>
    protected virtual InputParameter InputParameterConvert(object? value)
    {
        if (value is not InputParameter targetValue)
        {
            throw new ArgumentException($"current value type is not {typeof(InputParameter).FullName}");
        }

        return targetValue;
    }

    /// <summary>
    /// Converts a value back to a specified type. This method is not supported and will always throw an exception.
    /// </summary>
    /// <param name="value">The value to be converted back to the target type.</param>
    /// <param name="targetType">The type to which the value should be converted.</param>
    /// <param name="parameter">An optional parameter that can influence the conversion process.</param>
    /// <param name="culture">Specifies the culture information that may affect the conversion.</param>
    /// <returns>This method does not return a value as it always throws an exception.</returns>
    /// <exception cref="NotSupportedException">Thrown because the conversion operation is not supported.</exception>
    protected virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Converts a value back from a target type to its original form. It utilizes the provided context for the
    /// conversion process.
    /// </summary>
    /// <param name="value">The value to be converted back to its original form.</param>
    /// <param name="targetType">Specifies the type to which the value should be converted.</param>
    /// <param name="parameter">Provides additional information that may influence the conversion process.</param>
    /// <param name="culture">Indicates the culture-specific information that may affect the conversion.</param>
    /// <returns>Returns the converted value in its original form or null if the conversion fails.</returns>
    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ConvertBack(value, targetType, parameter, culture);
    }

    /// <summary>
    /// Returns the current instance of the object.
    /// </summary>
    /// <param name="serviceProvider">Provides access to services that can be used to retrieve additional information or functionality.</param>
    /// <returns>Returns the instance of the object itself.</returns>
    public
#if __WPF__ || __AVALONIA__
    override
#endif

    object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

/// <summary>
/// An abstract class that serves as a base for value converters, allowing conversion between types. It implements the
/// IValueConverter interface.
/// </summary>
/// <typeparam name="T">Represents the type of value being converted.</typeparam>
public abstract class ValueConverterBase<T> : ValueConverterBase<T, object>, IValueConverter { }
