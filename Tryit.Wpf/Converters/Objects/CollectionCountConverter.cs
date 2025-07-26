using System.Collections;
using System.Globalization;

namespace Tryit.Wpf;

/// <summary>
/// Counts the number of elements in a collection and returns the total count. It handles both ICollection and other
/// IEnumerable types.
/// </summary>
public class CollectionCountConverter : ValueConverterBase<IEnumerable>
{
    /// <summary>
    /// Counts the number of elements in a collection and returns the total count.
    /// </summary>
    /// <param name="items">Represents the collection of elements to be counted.</param>
    /// <param name="targetType">Specifies the type to which the result should be converted.</param>
    /// <param name="parameter">Provides additional information for the conversion process.</param>
    /// <param name="culture">Indicates the culture information for formatting the result.</param>
    /// <returns>Returns the total number of elements in the collection.</returns>
    protected override object? Convert(IEnumerable items, Type targetType, object? parameter, CultureInfo culture)
    {
        if (items is ICollection list)
        {
            return list.Count;
        }

        var count = 0;

        foreach (var item in items!)
        {
            count++;
        }

        return count;
    }
}
