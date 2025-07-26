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

public class LessThanOrEqualConverterExtension : TrueFalseConverterExtension<LessThanOrEqualConverter, IComparable> { }
