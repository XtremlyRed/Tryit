using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Initializes a new instance of the GreaterThanOrEqualConverter class. It sets the comparison mode to
/// GreaterThanOrEqual.
/// </summary>
public class GreaterThanOrEqualConverter : CompareConverter
{
    /// <summary>
    /// Initializes a new instance of the GreaterThanOrEqualConverter class. It sets the comparison mode to
    /// GreaterThanOrEqual.
    /// </summary>
    public GreaterThanOrEqualConverter()
        : base(CompareMode.GreaterThanOrEqual) { }
}

/// <summary>
/// Provides a markup extension that returns a value converter for determining whether a value is greater than or equal
/// to a specified comparison value.
/// </summary>
/// <remarks>This extension is typically used in XAML to enable conditional logic based on value comparisons. It
/// is useful for data binding scenarios where UI elements need to react to values meeting or exceeding a
/// threshold.</remarks>
public class GreaterThanOrEqualConverterExtension : TrueFalseConverterExtension<GreaterThanOrEqualConverter, IComparable> { }
