using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Abstract class for converting enum values to their display representations using specified attributes.
/// </summary>
/// <typeparam name="TAttribute">Represents an attribute type used to customize the display of enum values.</typeparam>
public abstract class EnumConverter<TAttribute> : ValueConverterBase<object>
    where TAttribute : Attribute
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static ConcurrentDictionary<Type, Dictionary<int, string>> enumValueMaps = new();

    /// <summary>
    /// An abstract property that returns a function to select a display string based on an optional attribute. The
    /// function takes an attribute and returns a nullable string.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    protected abstract Func<TAttribute?, string?> DisplaySelector { get; }

    /// <summary>
    /// Converts an enum value to its corresponding display string based on attributes or field names.
    /// </summary>
    /// <param name="value">The input value to be converted, which must be an enum type.</param>
    /// <param name="targetType">Specifies the type to which the value is being converted.</param>
    /// <param name="parameter">An optional parameter that can provide additional context for the conversion.</param>
    /// <param name="culture">Indicates the culture information that may affect the conversion process.</param>
    /// <returns>Returns the display string associated with the enum value or an unset value for specific frameworks.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input value is null, not an enum, or its type cannot be determined.</exception>
    /// <exception cref="NotImplementedException">Thrown when the conversion is not implemented for the current framework.</exception>
    protected override object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var valueType = value?.GetType();

        if (value is null || valueType is null || valueType.IsEnum == false)
        {
            throw new InvalidOperationException($"invalid data type, must be : {typeof(Enum)}");
        }

        var valueHashCode = value.GetHashCode();

        if (enumValueMaps.TryGetValue(valueType, out var enumMaps) == false)
        {
            enumValueMaps[valueType] = enumMaps = new Dictionary<int, string>();

            var fields = valueType.GetFields();

            for (int i = 0, length = fields.Length; i < length; i++)
            {
                if (fields[i].IsStatic == false)
                {
                    continue;
                }

                var enumValueHashCode = fields[i].GetValue(null)!.GetHashCode();

                TAttribute? attribute = fields[i].GetCustomAttribute<TAttribute>();

                enumMaps[enumValueHashCode] = (DisplaySelector?.Invoke(attribute)) ?? fields[i].Name;
            }
        }

        if (enumMaps.TryGetValue(valueHashCode, out var display))
        {
            return display;
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
    /// Converts the input value to an object type, ensuring it is not null.
    /// </summary>
    /// <param name="value">The input can be any object, which will be returned as is.</param>
    /// <returns>Returns the input value cast to an object type.</returns>
    protected override object InputConvert(object? value)
    {
        return value!;
    }
}
