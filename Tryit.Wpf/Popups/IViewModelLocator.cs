using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Maps view types to their corresponding view model types and provides functions to locate view models. It also
/// manages an attached property for automatic view model assignment.
/// </summary>
public static class ViewModelLocator
{
    /// <summary>
    /// A static function that maps a view type to its corresponding view model type. It is initialized with a default
    /// mapping function.
    /// </summary>
    [DBA(Never)]
    internal static Func<Type, Type>? viewModelTypeLocator = DefaultViewTypeToViewModel;

    /// <summary>
    /// A static function that takes a Type as input and returns an object, used for locating view models. It is marked
    /// as internal and can be null.
    /// </summary>
    [DBA(Never)]
    internal static Func<Type, object>? viewModelLocator;

    /// <summary>
    /// Sets a function to locate view model types based on their associated types.
    /// </summary>
    /// <param name="viewModelTypeLocator">A function that maps a type to its corresponding view model type.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided function for locating view model types is null.</exception>
    public static void SetViewModelTypeLocator(Func<Type, Type> viewModelTypeLocator)
    {
        ViewModelLocator.viewModelTypeLocator = viewModelTypeLocator ?? throw new ArgumentNullException(nameof(viewModelTypeLocator));
    }

    /// <summary>
    /// Sets a function to locate view models by their type.
    /// </summary>
    /// <param name="viewModelLocator">The function used to retrieve view models based on their type.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided function for locating view models is null.</exception>
    public static void SetViewModelLocator(Func<Type, object> viewModelLocator)
    {
        ViewModelLocator.viewModelLocator = viewModelLocator ?? throw new ArgumentNullException(nameof(viewModelLocator));
    }

    /// <summary>
    /// Sets a property on a dependency object to indicate whether it is automatically aware of changes.
    /// </summary>
    /// <param name="obj">The dependency object that will have its property set.</param>
    /// <param name="value">A boolean indicating whether the object should be automatically aware of changes.</param>
    public static void SetAutoAware(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoAwareProperty, value);
    }

    /// <summary>
    /// Registers an attached property named 'AutoAware' of type bool. It sets the DataContext of a FrameworkElement to a
    /// ViewModel when AutoAware is true.
    /// </summary>
    public static readonly DependencyProperty AutoAwareProperty = DependencyProperty.RegisterAttached(
        "AutoAware",
        typeof(bool),
        typeof(ViewModelLocator),
        new PropertyMetadata(
            false,
            (s, e) =>
            {
                if (s is Visual visual && e.NewValue is bool autoAware && autoAware)
                {
                    if (viewModelLocator is null)
                    {
                        throw new InvalidOperationException("invalid view model locator");
                    }

                    var viewModelType = viewModelTypeLocator(visual.GetType());

                    var viewModel = viewModelLocator(viewModelType);

                    if (viewModel is not null && s is FrameworkElement element)
                    {
                        element.DataContext = viewModel;
                    }
                }
            }
        )
    );

    /// <summary>
    /// Converts a view type to its corresponding view model type by modifying the namespace and suffix.
    /// </summary>
    /// <param name="viewType">Represents the type of the view that is being converted to a view model.</param>
    /// <returns>Returns the type of the corresponding view model.</returns>
    private static Type DefaultViewTypeToViewModel(Type viewType)
    {
        var viewName = viewType.FullName;
        viewName = viewName?.Replace(".Views.", ".ViewModels.");
        var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
        var suffix = viewName != null && viewName.EndsWith("View") ? "Model" : "ViewModel";
        var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
        return Type.GetType(viewModelName)!;
    }
}
