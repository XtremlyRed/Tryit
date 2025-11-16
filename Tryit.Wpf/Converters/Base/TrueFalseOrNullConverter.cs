using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// An abstract class that extends TrueFalseConverter and implements IValueConverter, allowing for conversion of true,
/// false, or null values.
/// </summary>
/// <typeparam name="T">Specifies the type of value being converted, enabling flexibility in handling different data types.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TrueFalseOrNullConverter<T> : TrueFalseConverter<T>, IValueConverter
{
    /// <summary>
    /// Represents a nullable object property that can hold a null value or any object. It allows for flexible data
    /// handling.
    /// </summary>
    public object? Null { get; set; }
}

/// <summary>
/// Provides a base markup extension for value converters that handle true, false, or null values in XAML bindings.
/// </summary>
/// <remarks>This class is intended to be used as a base for XAML markup extensions that supply true/false/null
/// value converters. It enables flexible configuration of null handling in data bindings.</remarks>
/// <typeparam name="T">The type of the value converter, which must inherit from TrueFalseOrNullConverter{TP} and have a parameterless
/// constructor.</typeparam>
/// <typeparam name="TP">The type of the parameter accepted by the value converter.</typeparam>
public abstract class TrueFalseOrNullConverterExtension<T, TP> : MarkupExtension
    where T : TrueFalseOrNullConverter<TP>, new()
{
    /// <summary>
    /// Represents a nullable object property that can hold a null value or any object. It allows for flexible data
    /// handling.
    /// </summary>
    public object? Null { get; set; }
}
