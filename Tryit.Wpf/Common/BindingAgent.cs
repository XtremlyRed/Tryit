using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Represents an object that provides a data context for data binding operations within an element and its child
/// elements.
/// </summary>
/// <remarks>The BindingAgent class derives from Freezable to support cloning, change notification, and other
/// features useful in data binding scenarios. It is typically used to set or manage the data context for a subtree of
/// elements, enabling flexible data binding in user interfaces. The data context set by this class is inherited by
/// child elements unless they explicitly set their own data context.</remarks>
public class BindingAgent : Freezable
{
    /// <summary>
    /// Creates a new instance of the BindingAgent class.
    /// </summary>
    /// <remarks>This method is called by the Freezable base class to create a new instance of the derived
    /// BindingAgent class. It is typically not called directly from user code.</remarks>
    /// <returns>A new instance of BindingAgent.</returns>
    protected override Freezable CreateInstanceCore()
    {
        return new BindingAgent();
    }

    /// <summary>
    /// Gets or sets the data context for the element.
    /// </summary>
    /// <remarks>The data context provides a data source for data binding operations. Setting this property
    /// affects the data context for the element and all its child elements, unless they have their own data context
    /// set. This property is commonly used in data binding scenarios to supply the source object for bindings within
    /// the element's scope.</remarks>
    public object DataContext
    {
        get => GetValue(DataContextProperty);
        set => SetValue(DataContextProperty, value);
    }

    /// <summary>
    /// Identifies the DataContext dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the DataContext property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when adding property metadata or working
    /// with property system services such as data binding and styling.</remarks>
    public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), typeof(BindingAgent), new PropertyMetadata(null));
}
