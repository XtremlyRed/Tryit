using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Provides a value converter that selects between two values based on a Boolean condition, typically for use in data
/// binding scenarios.
/// </summary>
/// <remarks>The If class is commonly used in XAML-based applications to enable conditional value selection in
/// bindings. By specifying the True and False properties, you can control which value is returned when the bound
/// condition evaluates to true or false. This class implements IValueConverter and is intended for use in frameworks
/// that support dependency properties, such as WPF or Xamarin.Forms.</remarks>
public class If : DependencyObject, IValueConverter
{
    /// <summary>
    /// Gets or sets the value to use when the condition evaluates to true.
    /// </summary>
    public object True
    {
        get => (object)GetValue(TrueProperty);
        set => SetValue(TrueProperty, value);
    }

    /// <summary>
    /// Identifies the True dependency property.
    /// </summary>
    /// <remarks>This field is used to reference the True property in property system operations, such as data
    /// binding or styling, within XAML frameworks that support dependency properties.</remarks>
    public static readonly DependencyProperty TrueProperty =
        DependencyProperty.Register(nameof(True), typeof(object), typeof(If), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the value that represents the 'false' state for the associated property or control.
    /// </summary>
    public object False
    {
        get => (object)GetValue(FalseProperty);
        set => SetValue(FalseProperty, value);
    }

    /// <summary>
    /// Identifies the False dependency property, which represents the content to display when the condition is not met.
    /// </summary>
    /// <remarks>This field is typically used when defining or referencing the False property in XAML or code,
    /// such as when calling methods that require a DependencyProperty identifier.</remarks>
    public static readonly DependencyProperty FalseProperty =
        DependencyProperty.Register(nameof(False), typeof(object), typeof(If), new PropertyMetadata(null));

    /// <summary>
    /// Converts a value to a different type for use in a binding scenario.
    /// </summary>
    /// <remarks>This method is typically called by data binding engines such as WPF or Xamarin.Forms when a
    /// value needs to be converted between the source and target of a binding. Implement this method to provide custom
    /// conversion logic.</remarks>
    /// <param name="value">The value produced by the binding source to be converted.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic. This value can be null.</param>
    /// <param name="culture">The culture to use in the converter. This is typically used to format the result appropriately for the user's
    /// locale.</param>
    /// <returns>A converted value to be passed to the target dependency property. The return value must be of the type specified
    /// by targetType.</returns>
    /// <exception cref="NotImplementedException">The method is not implemented.</exception>
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
        {
            throw new NotSupportedException("invalid binding value type,must System.Boolean");  //
        }

        return boolValue ? True : False;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
