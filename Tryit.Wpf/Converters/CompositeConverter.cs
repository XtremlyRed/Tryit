using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace Tryit.Wpf;

/// <summary>
/// CompositeConverter manages a collection of value converters for data transformation. It converts values using
/// multiple converters sequentially.
/// </summary>
[ContentProperty(nameof(Converters))]
[DefaultProperty(nameof(Converters))]
public partial class CompositeConverter : ValueConverterBase<object>
{
    /// <summary>
    /// Provides access to a collection of converters used for type conversion. It allows for customization of data
    /// transformation.
    /// </summary>
    public ConverterCollection Converters { get; }

    /// <summary>
    /// Initializes a new instance of the CompositeConverter class. Sets up a new ConverterCollection for managing
    /// converters.
    /// </summary>
    public CompositeConverter()
    {
        Converters = new ConverterCollection();
    }

    /// <summary>
    /// Converts a value using a series of value converters in a specified target type.
    /// </summary>
    /// <param name="value">The input value to be converted through the converters.</param>
    /// <param name="targetType">Specifies the type to which the input value should be converted.</param>
    /// <param name="parameter">Provides additional data that may influence the conversion process.</param>
    /// <param name="culture">Indicates the cultural information that may affect the conversion.</param>
    /// <returns>The final converted value after processing through all converters.</returns>
    protected sealed override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        object? concurrent = value;

        foreach (IValueConverter item in Converters)
        {
            concurrent = item.Convert(concurrent, targetType, parameter, culture);
        }

        return concurrent!;
    }

    /// <summary>
    /// Converts the input value to an object type, ensuring it is not null.
    /// </summary>
    /// <param name="value">The input can be any object, including null, which is handled by the method.</param>
    /// <returns>Returns the input value cast to an object type, guaranteed to be non-null.</returns>
    protected sealed override object InputConvert(object? value)
    {
        return value!;
    }

    /// <summary>
    /// A collection specifically designed to hold instances of IValueConverter. It provides a way to manage and
    /// manipulate a group of value converters.
    /// </summary>
    public sealed class ConverterCollection : Collection<IValueConverter> { }
}
