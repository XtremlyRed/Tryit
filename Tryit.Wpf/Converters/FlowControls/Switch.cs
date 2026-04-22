using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Provides a value converter that selects an output value based on matching input cases, similar to a switch
/// statement. Used to map input values to corresponding outputs in data binding scenarios.
/// </summary>
/// <remarks>The Switch class is typically used in XAML data binding to convert an input value to a specific
/// output by matching it against a collection of cases. If no case matches the input, the DefaultValue is returned.
/// This class is especially useful for scenarios where multiple possible input values need to be mapped to different
/// outputs without writing custom converter logic for each case.</remarks>
[DefaultProperty(nameof(Cases))]
[ContentProperty(nameof(Cases))]
public class Switch : DependencyObject, IValueConverter
{
    /// <summary>
    /// Initializes a new instance of the Switch class with an empty collection of cases.
    /// </summary>
    public Switch()
    {
        Cases = [];
    }

    /// <summary>
    /// Gets the collection of cases associated with this instance.
    /// </summary>
    public Collection<Case> Cases { get; }

    /// <summary>
    /// Gets or sets the default value associated with the property.
    /// </summary>
    /// <remarks>If no explicit value is set, this value will be used as the property's value. The type of the
    /// default value should be compatible with the property's expected type.</remarks>
    public object? DefaultValue
    {
        get => (object?)GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    /// <summary>
    /// Identifies the DefaultValue dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the DefaultValue property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on instances of the Switch class.</remarks>
    public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register(nameof(DefaultValue), typeof(object), typeof(Switch), new PropertyMetadata(null));

    /// <summary>
    /// Converts a value to a corresponding output value based on predefined cases.
    /// </summary>
    /// <remarks>The conversion is performed by comparing the hash code of the input value to the hash codes
    /// of the defined cases. If no match is found, the default value is returned. This method is typically used in data
    /// binding scenarios to map input values to specific outputs.</remarks>
    /// <param name="value">The value to convert. This is compared against the input values of the defined cases.</param>
    /// <param name="targetType">The type of the binding target property. Specifies the type to convert the value to.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion logic. May be null.</param>
    /// <param name="culture">The culture to use in the converter. This is typically used to format values according to cultural conventions.</param>
    /// <returns>The output value associated with the matching case if a match is found; otherwise, the default value.</returns>
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        for (int i = 0; i < Cases.Count; i++)
        {
            if (Cases[i].Input?.GetHashCode() == value?.GetHashCode())
            {
                return Cases[i].Value!;
            }
        }

        return DefaultValue!;
    }

    ///
    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Represents a collection of Case objects that can be individually accessed by index.
    /// </summary>
    /// <remarks>CaseCollection provides collection management functionality for Case instances, including
    /// adding, removing, and enumerating cases. It inherits from Collection{Case}, allowing use of standard collection
    /// operations.</remarks>
    public class CaseCollection : Collection<Case> { }
}

/// <summary>
/// Represents a case condition with an associated input and value, typically used in data binding scenarios to map
/// specific inputs to corresponding values.
/// </summary>
/// <remarks>The Case class is commonly used in conjunction with conditional data binding or value converters in
/// XAML-based applications. It enables the definition of input-value pairs that can be evaluated at runtime to
/// determine which value should be applied based on the current input. Both the Input and Value properties are
/// dependency properties and support data binding, styling, and animation.</remarks>
[DebuggerDisplay("{Input}:{Value}")]
public class Case : DependencyObject
{
    /// <summary>
    /// Gets or sets the current value of the control's content.
    /// </summary>
    public object? Value
    {
        get => (object?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Identifies the Value dependency property for the Case control.
    /// </summary>
    /// <remarks>This field is used when registering and accessing the Value property as a dependency property
    /// in XAML or code. Dependency properties enable styling, data binding, animation, and default value support in
    /// WPF.</remarks>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(Case), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the input value associated with the control.
    /// </summary>
    public object? Input
    {
        get => (object?)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    /// <summary>
    /// Identifies the Input dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the Input property with the Windows Presentation
    /// Foundation (WPF) property system. It is typically used when calling methods such as SetValue or GetValue on
    /// instances of the Case class.</remarks>
    public static readonly DependencyProperty InputProperty = DependencyProperty.Register("Input", typeof(object), typeof(Case), new PropertyMetadata(null));
}
