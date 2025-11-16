using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Converts a collection to a boolean indicating if it is empty or null. Also converts input values to an enumerable
/// format.
/// </summary>
public class NullOrEmptyConverter : TrueFalseConverter<IEnumerable>
{
    /// <summary>
    /// Converts a collection to a boolean indicating whether it is empty or null.
    /// </summary>
    /// <param name="value">Represents the collection to be evaluated for emptiness.</param>
    /// <param name="targetType">Specifies the type to which the value should be converted.</param>
    /// <param name="parameter">Provides additional information for the conversion process.</param>
    /// <param name="culture">Indicates the culture-specific formatting to be applied during conversion.</param>
    /// <returns>Returns true if the collection is empty or null; otherwise, returns false.</returns>
    protected override object? Convert(IEnumerable value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return True;
        }

        if (value is ICollection collection)
        {
            return collection.Count > 0 ? False : True;
        }

        foreach (var _ in value)
        {
            return False;
        }

        return True;
    }

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
}

/// <summary>
/// Provides a markup extension that returns a Boolean value indicating whether a given collection is null or contains
/// no elements.
/// </summary>
/// <remarks>This extension is typically used in XAML to enable conditional logic based on whether a collection is
/// null or empty. It can be useful for controlling UI visibility or enabling/disabling controls depending on the
/// presence of data.</remarks>
public class NullOrEmptyConverterExtension : TrueFalseConverterExtension<NullOrEmptyConverter, IEnumerable> { }
