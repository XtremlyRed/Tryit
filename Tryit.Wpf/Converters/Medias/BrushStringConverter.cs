namespace Tryit.Wpf;

using System.Windows;
using System.Windows.Media;

/// <summary>
/// Converts a string representation of a color into a Brush object based on the platform. Supports WPF, Avalonia, and
/// MAUI.
/// </summary>
public class BrushStringConverter : MediaConverter<string, Brush>
{
    /// <summary>
    /// A static instance of BrushConverter used for converting string representations of brushes to Brush objects. It
    /// is specific to WPF applications.
    /// </summary>

    static BrushConverter brushConverter = new BrushConverter();

    /// <summary>
    /// Converts a string representation of a color into a Brush object based on the platform.
    /// </summary>
    /// <param name="from">The string representation of a color to be converted into a Brush.</param>
    /// <returns>A Brush object that represents the specified color.</returns>
    protected override Brush ConvertFrom(string from)
    {
        return (Brush)brushConverter.ConvertFrom(from)!;
    }
}
