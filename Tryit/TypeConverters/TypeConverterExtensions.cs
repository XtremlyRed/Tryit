using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Tryit;

/// <summary>
/// Provides extension methods for converting objects to specified types using registered type converters. Supports
/// custom converter registration.
/// </summary>
public static class TypeConverterExtensions
{
    /// <summary>
    /// A private static readonly dictionary that maps types to their corresponding type converters. It uses a
    /// concurrent dictionary for thread-safe operations.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, TypeConverter>> typeConvertMaps = new();

    /// <summary>
    /// Holds an array of string representations of the names of the TypeCode enumeration. It is marked as private and
    /// static.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly string[] typeCodeNames = Enum.GetNames(typeof(TypeCode));

    /// <summary>
    /// Converts an object to a specified type, returning the converted value if successful.
    /// </summary>
    /// <typeparam name="To">Represents the target type to which the object will be converted.</typeparam>
    /// <param name="from">The object that is to be converted to the specified type.</param>
    /// <returns>The converted value of the specified type if the conversion is successful.</returns>
    /// <exception cref="InvalidCastException">Thrown when the conversion to the specified type fails.</exception>
    public static To ConvertTo<To>(this object? from)
    {
        if (from is To to)
        {
            return to;
        }

        if (ConvertTo(from, typeof(To)) is To toValue)
        {
            return toValue;
        }

        throw new InvalidCastException("type conversion unsuccessful");
    }

    /// <summary>
    /// Converts an object to a specified type using available type converters.
    /// </summary>
    /// <param name="from">The object to be converted to another type.</param>
    /// <param name="toType">The target type to which the object should be converted.</param>
    /// <returns>The converted object of the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the object to convert is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a type converter for the specified conversion is not registered.</exception>
    public static object ConvertTo(this object? from, Type toType)
    {
        _ = from ?? throw new ArgumentNullException(nameof(from));

        if (from is IConvertible convertible && typeCodeNames.Contains(toType.Name))
        {
            var converted = convertible.ToType(toType, CultureInfo.CurrentCulture);

            if (converted is not null)
            {
                return converted;
            }
        }

        Type fromType = from.GetType();

        if (typeConvertMaps.TryGetValue(fromType, out ConcurrentDictionary<Type, TypeConverter>? fromTypeConverterStorages) == false)
        {
            typeConvertMaps[fromType] = fromTypeConverterStorages = new ConcurrentDictionary<Type, TypeConverter>();
        }

        if (fromTypeConverterStorages.TryGetValue(toType, out TypeConverter? toTypeConverter) == false)
        {
            toTypeConverter = TypeDescriptor.GetConverter(toType) ?? throw new InvalidOperationException("type converter not registered");

            fromTypeConverterStorages[toType] = toTypeConverter;
        }

        if (toTypeConverter.CanConvertFrom(fromType))
        {
            return toTypeConverter.ConvertFrom(from)!;
        }

        throw new InvalidOperationException($"type converter from {fromType} to {toType} not registered");
    }

    /// <summary>
    /// Registers a converter function for transforming one type to another within a type conversion mapping.
    /// </summary>
    /// <typeparam name="TFrom">Represents the source type that will be converted from.</typeparam>
    /// <typeparam name="TTo">Represents the target type that will be converted to.</typeparam>
    /// <param name="converter">A function that defines how to convert from the source type to the target type.</param>
    public static void AppendConverter<TFrom, TTo>(Func<TFrom, TTo> converter)
    {
        var fromType = typeof(TFrom);
        var toType = typeof(TTo);

        if (typeConvertMaps.TryGetValue(fromType, out ConcurrentDictionary<Type, TypeConverter>? targetTypeConverterMaps) == false)
        {
            typeConvertMaps[fromType] = targetTypeConverterMaps = new ConcurrentDictionary<Type, TypeConverter>();
        }

        targetTypeConverterMaps[toType] = new CustomTypeConverter<TFrom, TTo>(converter);
    }

    /// <summary>
    /// A custom type converter that converts objects from one type to another using a specified conversion function.
    /// </summary>
    /// <typeparam name="TFrom">Represents the source type that can be converted from.</typeparam>
    /// <typeparam name="TTo">Represents the target type that the source type is converted to.</typeparam>
    private class CustomTypeConverter<TFrom, TTo> : TypeConverter
    {
        Func<TFrom, TTo> converter;
        static Type FromType = typeof(TFrom);
        static Type ToType = typeof(TTo);

        /// <summary>
        /// Initializes a custom type converter using a specified conversion function.
        /// </summary>
        /// <param name="converter">The function defines how to convert from one type to another.</param>
        public CustomTypeConverter(Func<TFrom, TTo> converter)
        {
            this.converter = converter;
        }

        /// <summary>
        /// Determines if conversion from a specified type is possible. It checks if the source type matches a
        /// predefined type.
        /// </summary>
        /// <param name="context">Provides context information for the conversion process.</param>
        /// <param name="sourceType">Specifies the type that is being checked for conversion compatibility.</param>
        /// <returns>Returns true if the source type matches the expected type; otherwise, it calls the base implementation.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == FromType)
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts a value from one type to another using a specified converter. If the value is not of the expected
        /// type, it falls back to the base conversion.
        /// </summary>
        /// <param name="context">Provides contextual information about the environment where the conversion is taking place.</param>
        /// <param name="culture">Specifies culture-specific information that may affect the conversion process.</param>
        /// <param name="value">Represents the value to be converted to a different type.</param>
        /// <returns>Returns the converted value or a base conversion result if the value is not of the expected type.</returns>
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is TFrom from)
            {
                return converter(from);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
