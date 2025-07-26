using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Converts values between types using specified functions and culture information. Supports two conversion methods.
/// </summary>
public class FuncConverter : IValueConverter
{
    /// <summary>
    /// A private readonly function that converts an object based on specified type, additional object, and culture
    /// information. It is not visible in the debugger.
    /// </summary>
    [DebuggerBrowsable(Never)]
    private readonly Func<object, Type, object, CultureInfo, object>? converter1;

    /// <summary>
    /// A private readonly function that converts an object based on a specified type and additional parameters. It can
    /// be null.
    /// </summary>
    [DebuggerBrowsable(Never)]
    private readonly Func<object, Type, object, object>? converter2;

    /// <summary>
    /// Initializes a new instance of a converter that transforms an object based on a specified type and culture
    /// information.
    /// </summary>
    /// <param name="converter1">A function that takes an object, a type, another object, and culture information to produce a converted result.</param>
    public FuncConverter(Func<object, Type, object, CultureInfo, object> converter1)
    {
        this.converter1 = converter1;
    }

    /// <summary>
    /// Initializes a new instance of the FuncConverter class with a specified conversion function.
    /// </summary>
    /// <param name="converter2">The conversion function that takes an object, a Type, and another object to produce a converted result.</param>
    public FuncConverter(Func<object, Type, object, object> converter2)
    {
        this.converter2 = converter2;
    }

    /// <summary>
    /// Converts a value to a specified target type using optional parameters and culture information.
    /// </summary>
    /// <param name="value">The input value to be converted to the target type.</param>
    /// <param name="targetType">Specifies the type to which the input value should be converted.</param>
    /// <param name="parameter">An optional parameter that can influence the conversion process.</param>
    /// <param name="culture">Provides culture-specific information for the conversion.</param>
    /// <returns>Returns the converted value or an unset value based on the context.</returns>
    /// <exception cref="NotImplementedException">Thrown when no suitable conversion method is implemented for the provided parameters.</exception>
    object IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (converter1 is not null)
        {
            return converter1(value!, targetType, parameter!, culture);
        }

        if (converter2 is not null)
        {
            return converter2(value!, targetType, parameter!);
        }
#if __WPF__
        return DependencyProperty.UnsetValue;
#elif __AVALONIA__
        return AvaloniaProperty.UnsetValue;
#elif __MAUI__
        return BindableProperty.UnsetValue;
#endif

        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a value back to its original form based on the target type and additional parameters.
    /// </summary>
    /// <param name="value">The value to be converted back to its original form.</param>
    /// <param name="targetType">Specifies the type to which the value should be converted.</param>
    /// <param name="parameter">Provides additional context or instructions for the conversion process.</param>
    /// <param name="culture">Indicates the cultural information that may affect the conversion.</param>
    /// <returns>Returns the converted value in the specified target type.</returns>
    /// <exception cref="NotImplementedException">Thrown when the conversion logic has not been implemented.</exception>
    object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a function that takes an object, type, and culture info into a FuncConverter instance.
    /// </summary>
    /// <param name="converter1">The function that processes an object based on its type and culture information.</param>

    public static implicit operator FuncConverter(Func<object, Type, object, CultureInfo, object> converter1)
    {
        _ = converter1 ?? throw new ArgumentNullException(nameof(converter1));

        return new FuncConverter(converter1);
    }

    /// <summary>
    /// Converts a function that takes three parameters into a FuncConverter instance. It throws an exception if the
    /// provided function is null.
    /// </summary>
    /// <param name="converter2">The function that processes an object, a type, and another object to produce a result.</param>

    public static implicit operator FuncConverter(Func<object, Type, object, object> converter2)
    {
        _ = converter2 ?? throw new ArgumentNullException(nameof(converter2));

        return new FuncConverter(converter2);
    }

    /// <summary>
    /// Creates a new value converter using a specified conversion function.
    /// </summary>
    /// <param name="converter2">The conversion function that defines how to transform values.</param>
    /// <returns>Returns an instance of a value converter based on the provided function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the conversion function is null.</exception>
    public static IValueConverter Create(Func<object, Type, object, object> converter2)
    {
        _ = converter2 ?? throw new ArgumentNullException(nameof(converter2));

        return new FuncConverter(converter2);
    }

    /// <summary>
    /// Creates a new value converter using a specified conversion function.
    /// </summary>
    /// <param name="converter1">The function that defines how to convert values between types.</param>
    /// <returns>Returns an instance of a value converter that utilizes the provided conversion function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the conversion function is null.</exception>
    public static IValueConverter Create(Func<object, Type, object, CultureInfo, object> converter1)
    {
        _ = converter1 ?? throw new ArgumentNullException(nameof(converter1));

        return new FuncConverter(converter1);
    }
}
