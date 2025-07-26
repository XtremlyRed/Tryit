using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Abstract class for converting values between two types using a storage mechanism.
/// </summary>
/// <typeparam name="From">Represents the source type that will be converted from.</typeparam>
/// <typeparam name="To">Represents the target type that will be converted to.</typeparam>
public abstract class MediaConverter<From, To> : IValueConverter
    where From : notnull
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly ConcurrentDictionary<From, To> storages = new();

    /// <summary>
    /// Represents a nullable object property that can hold a null value or any object. It allows for flexible data
    /// handling.
    /// </summary>
    public object? Null { get; set; }

    /// <summary>
    /// Converts a value of type 'From' to a target type 'To' using a storage mechanism.
    /// </summary>
    /// <param name="value">The input value to be converted, which should be of type 'From'.</param>
    /// <param name="targetType">Specifies the type to which the input value should be converted.</param>
    /// <param name="parameter">An optional parameter that can be used to influence the conversion process.</param>
    /// <param name="culture">Provides culture-specific information that may affect the conversion.</param>
    /// <returns>Returns the converted value of type 'To' or null if the conversion fails.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not From fromValue)
        {
            return Null!;
        }

        if (storages.TryGetValue(fromValue, out To? targetValue) == false)
        {
            storages[fromValue] = targetValue = ConvertFrom(fromValue);
        }

        return targetValue!;
    }

    /// <summary>
    /// Converts a value back to its original form based on the target type and other parameters.
    /// </summary>
    /// <param name="value">The value to be converted back to its original form.</param>
    /// <param name="targetType">Specifies the type to which the value should be converted.</param>
    /// <param name="parameter">Provides additional information for the conversion process.</param>
    /// <param name="culture">Indicates the culture-specific information for the conversion.</param>
    /// <returns>This method does not return a value as it always throws an exception.</returns>
    /// <exception cref="NotSupportedException">Thrown because the conversion back is not supported.</exception>
    object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>
    protected abstract To ConvertFrom(From from);
}
