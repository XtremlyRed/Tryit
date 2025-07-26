using System.ComponentModel;

namespace Tryit.Wpf;

/// <summary>
/// Retrieves the Description property from a DescriptionAttribute instance. Returns null if the instance is null.
/// </summary>
public class EnumDescriptionConverter : EnumConverter<DescriptionAttribute>
{
    /// <summary>
    /// Returns a function that retrieves the Description property from a DescriptionAttribute instance. If the instance is
    /// null, it returns null.
    /// </summary>
    protected override Func<DescriptionAttribute?, string?> DisplaySelector => i => i?.Description;
}
