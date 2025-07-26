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

public class GreaterThanConverterExtension : TrueFalseConverterExtension<GreaterThanConverter, IComparable> { }
