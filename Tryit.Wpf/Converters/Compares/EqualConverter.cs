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

public class EqualConverterExtension : TrueFalseConverterExtension<EqualConverter, IComparable> { }
