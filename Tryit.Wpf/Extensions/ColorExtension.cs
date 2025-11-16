namespace Tryit.Wpf;

/// <summary>
/// Represents a color with RGBA channels and provides a method to create a color object based on the platform.
/// Supports WPF, Avalonia, and MAUI.
/// </summary>
public class ColorExtension : MarkupExtension
{
    /// <summary>
    /// Constructor for the ColorExtension class. Initializes a new instance of the ColorExtension.
    /// </summary>
    public ColorExtension() { }

    /// <summary>
    /// Constructs a color with specified alpha, red, green, and blue components.
    /// </summary>
    /// <param name="a">Defines the transparency level of the color.</param>
    /// <param name="r">Specifies the intensity of the red component.</param>
    /// <param name="g">Specifies the intensity of the green component.</param>
    /// <param name="b">Specifies the intensity of the blue component.</param>
    public ColorExtension(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Represents the red component of a color in the range of 0 to 255. It can be accessed or modified through its
    /// getter and setter.
    /// </summary>
    public byte R { get; set; }

    /// <summary>
    /// Represents the green component of a color in the RGB color model. It is stored as a byte value.
    /// </summary>
    public byte G { get; set; }

    /// <summary>
    /// Represents a byte property named B. It can be accessed and modified through its getter and setter.
    /// </summary>
    public byte B { get; set; }

    /// <summary>
    /// A property of type byte named A, initialized to the value 0xff. It can be accessed and modified publicly.
    /// </summary>
    public byte A { get; set; } = 0xff;

    /// <summary>
    /// Generates a Color object based on the provided color component values.
    /// </summary>
    /// <param name="serviceProvider">Supplies services needed for the value generation process.</param>
    /// <returns>Returns a Color object initialized with specified alpha and RGB values.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Color()
        {
            A = A,
            R = R,
            G = G,
            B = B,
        };
    }
}
