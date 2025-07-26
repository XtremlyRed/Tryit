using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Initializes a new instance of NotEqualConverter, setting the comparison mode to NotEqual.
/// </summary>
public class NotEqualConverter : CompareConverter
{
    /// <summary>
    /// Initializes a NotEqualConverter with a comparison mode set to NotEqual. This sets up the converter for not-equal
    /// comparisons.
    /// </summary>
    public NotEqualConverter()
        : base(CompareMode.NotEqual) { }
}

/// <summary>
/// This extension allows the NotEqualConverter to be used in XAML with a True/False binding, where it will return
/// </summary>
public class NotEqualConverterExtension : TrueFalseConverterExtension<NotEqualConverter, IComparable> { }
