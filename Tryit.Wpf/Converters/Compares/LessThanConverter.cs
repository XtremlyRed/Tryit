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

public class LessThanConverterExtension : TrueFalseConverterExtension<LessThanConverter, IComparable> { }
