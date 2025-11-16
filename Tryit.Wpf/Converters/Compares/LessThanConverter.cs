using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Initializes a new instance of LessThanConverter, setting the comparison mode to LessThan.
/// </summary>
public class LessThanConverter : CompareConverter
{
    /// <summary>
    /// Initializes a LessThanConverter with a comparison mode set to LessThan.
    /// </summary>
    public LessThanConverter()
        : base(CompareMode.LessThan) { }
}

/// <summary>
/// Provides a markup extension that returns a value based on whether the input is less than a specified comparison
/// value.
/// </summary>
/// <remarks>This extension is typically used in XAML to perform value comparisons in data bindings. It evaluates
/// whether the bound value is less than the provided comparison value and returns a corresponding result, such as a
/// Boolean or a custom value. The comparison uses the IComparable interface, so both values must be
/// comparable.</remarks>
public class LessThanConverterExtension : TrueFalseConverterExtension<LessThanConverter, IComparable> { }
