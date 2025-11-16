using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Initializes a LessThanOrEqualConverter instance, setting the comparison mode to LessThanOrEqual.
/// </summary>
public class LessThanOrEqualConverter : CompareConverter
{
    /// <summary>
    /// Initializes a new instance of the LessThanOrEqualConverter class. It sets the comparison mode to LessThanOrEqual.
    /// </summary>
    public LessThanOrEqualConverter()
        : base(CompareMode.LessThanOrEqual) { }
}

/// <summary>
/// Provides a markup extension that supplies a value converter for determining whether a value is less than or equal to
/// a comparison value.
/// </summary>
/// <remarks>This extension is typically used in XAML to enable conditional logic based on value comparisons. It
/// returns <see langword="true"/> if the input value is less than or equal to the specified comparison value;
/// otherwise, it returns <see langword="false"/>.</remarks>
public class LessThanOrEqualConverterExtension : TrueFalseConverterExtension<LessThanOrEqualConverter, IComparable> { }
