using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Represents a markup extension for creating brushes with a specified color and optional opacity. It provides a method
/// to generate a SolidColorBrush.
/// </summary>
public class BrushExtension : MarkupExtension
{
    /// <summary>
    /// Constructor for the BrushExtension class. Initializes a new instance of the BrushExtension.
    /// </summary>
    public BrushExtension() { }

    /// <summary>
    /// Initializes a new instance of the BrushExtension class with a specified color.
    /// </summary>
    /// <param name="color">Specifies the color to be used for the brush.</param>
    public BrushExtension(Color color)
    {
        Color = color;
    }

    /// <summary>
    /// Represents the color property with a default value. It can be used to get or set the color in an object.
    /// </summary>
    public Color Color { get; set; } = default!;

    /// <summary>
    /// Represents the opacity level of a UI element, ranging from 0 (completely transparent) to 1 (completely opaque).
    /// </summary>
    public double Opacity { get; set; } = 1;

    /// <summary>
    /// Provides a SolidColorBrush object based on the specified color and opacity settings.
    /// </summary>
    /// <param name="serviceProvider">Used to obtain services required for providing the value.</param>
    /// <returns>Returns a SolidColorBrush configured with the specified color and opacity.</returns>
    /// <exception cref="NotSupportedException">Thrown if the method is called in an unsupported environment.</exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new SolidColorBrush(Color) { Opacity = Opacity };
    }
}
