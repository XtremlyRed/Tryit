using System.ComponentModel;

namespace Tryit.Wpf;

/// <summary>
/// Returns the DisplayName from a DisplayNameAttribute if present; otherwise, returns null. Overrides the
/// DisplaySelector.
/// </summary>
public class EnumDisplayNameConverter : EnumConverter<DisplayNameAttribute>
{
    /// <summary>
    /// Overrides the DisplaySelector to return the DisplayName from a DisplayNameAttribute if it exists. Returns null
    /// if the attribute is not present.
    /// </summary>
    protected override Func<DisplayNameAttribute?, string?> DisplaySelector => i => i?.DisplayName;
}
