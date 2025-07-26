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

public abstract class TrueFalseOrNullConverterExtension<T, TP> : MarkupExtension
    where T : TrueFalseOrNullConverter<TP>, new()
{
    /// <summary>
    /// Represents a nullable object property that can hold a null value or any object. It allows for flexible data
    /// handling.
    /// </summary>
    public object? Null { get; set; }
}
