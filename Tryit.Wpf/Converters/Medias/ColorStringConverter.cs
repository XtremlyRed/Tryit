using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Converts a string representation of a color into a Color object based on the platform. Throws
/// NotImplementedException for unsupported platforms.
/// </summary>
public class ColorStringConverter : MediaConverter<string, Color>
{
    /// <summary>
    /// Converts a string representation of a color into a Color object based on the platform being used.
    /// </summary>
    /// <param name="from">The string representation of a color to be converted.</param>
    /// <returns>A Color object that corresponds to the provided string representation.</returns>
    /// <exception cref="NotImplementedException">Thrown if the platform is not recognized or supported.</exception>
    protected override Color ConvertFrom(string from)
    {
        return (Color)ColorConverter.ConvertFromString(from);
    }
}
