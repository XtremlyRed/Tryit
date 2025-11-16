using System.Collections;
using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Converts input values to an enumerable format, handling nulls by returning a default value. Evaluates a collection's
/// emptiness, returning true if it contains elements.
/// </summary>
public class NotNullOrEmptyConverter : TrueFalseConverter<IEnumerable>
{
    /// <summary>
    /// Converts input values to an enumerable format, handling null values appropriately.
    /// </summary>
    /// <param name="value">Accepts an object that may be null or contain data to be converted.</param>
    /// <returns>Returns an enumerable representation of the input value or a default value if the input is null.</returns>
    protected override IEnumerable InputConvert(object? value)
    {
        if (value is null)
        {
            return default!;
        }

        return base.InputConvert(value);
    }

    /// <summary>
    /// Converts a collection to a boolean indicating its emptiness.
    /// </summary>
    /// <param name="value">The collection to be evaluated for its contents.</param>
    /// <param name="targetType">Specifies the type to which the value should be converted.</param>
    /// <param name="parameter">An optional parameter that can influence the conversion process.</param>
    /// <param name="culture">Provides culture-specific information for the conversion.</param>
    /// <returns>Returns true if the collection has elements, otherwise returns false.</returns>
    protected override object? Convert(IEnumerable value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return False;
        }

        if (value is ICollection collection)
        {
            return collection.Count > 0 ? True : False;
        }

        foreach (var _ in value)
        {
            return True;
        }

        return False;
    }
}

/// <summary>
/// Provides a markup extension that returns a Boolean value indicating whether a collection is not null and contains at
/// least one element.
/// </summary>
/// <remarks>This extension is typically used in XAML data binding scenarios to enable or disable UI elements
/// based on whether a collection has items. It evaluates to <see langword="true"/> if the bound collection is not null
/// and contains one or more elements; otherwise, it evaluates to <see langword="false"/>.</remarks>
public class NotNullOrEmptyConverterExtension : TrueFalseConverterExtension<NotNullOrEmptyConverter, IEnumerable> { }
