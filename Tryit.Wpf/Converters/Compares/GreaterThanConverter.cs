using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Initializes a new instance of GreaterThanConverter, setting the comparison mode to GreaterThan.
/// </summary>
public class GreaterThanConverter : CompareConverter
{
    /// <summary>
    /// Initializes a new instance of the GreaterThanConverter class. It sets the comparison mode to GreaterThan.
    /// </summary>
    public GreaterThanConverter()
        : base(CompareMode.GreaterThan) { }
}

/// <summary>
/// Provides a markup extension that returns a value indicating whether the input is greater than a specified comparison
/// value.
/// </summary>
/// <remarks>This extension is typically used in XAML to perform greater-than comparisons in data bindings. It
/// supports values that implement the IComparable interface. The result is determined by comparing the bound value to
/// the specified comparison value using the IComparable.CompareTo method.</remarks>
public class GreaterThanConverterExtension : TrueFalseConverterExtension<GreaterThanConverter, IComparable> { }
