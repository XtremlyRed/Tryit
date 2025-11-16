using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Initializes a new instance of the EqualConverter class, setting the comparison mode to Equal.
/// </summary>
public class EqualConverter : CompareConverter
{
    /// <summary>
    /// Initializes a new instance of the EqualConverter class with a comparison mode set to Equal.
    /// </summary>
    public EqualConverter()
        : base(CompareMode.Equal) { }
}

/// <summary>
/// Provides a markup extension that returns a value indicating whether two objects are equal, for use in XAML data
/// binding scenarios.
/// </summary>
/// <remarks>This extension is typically used in XAML to compare a bound value to a specified value and return a
/// Boolean result. It is useful for enabling or disabling UI elements, triggering visual states, or other scenarios
/// where equality comparison is needed in markup.</remarks>
public class EqualConverterExtension : TrueFalseConverterExtension<EqualConverter, IComparable> { }
