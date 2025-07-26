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

public class GreaterThanOrEqualConverterExtension : TrueFalseConverterExtension<GreaterThanOrEqualConverter, IComparable> { }
