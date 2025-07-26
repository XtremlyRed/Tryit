using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Locates a visual element using a specified token. Returns a string that represents the located visual element.
/// </summary>
public interface IViewLocator
{
    /// <summary>
    /// Locates a visual element based on a specified token.
    /// </summary>
    /// <param name="viewToken">The token used to identify the visual element to locate.</param>
    /// <returns>Returns a string representing the located visual element.</returns>
    Visual Locate(string viewToken);
}

/// <summary>
/// Manages the view locator for the application, enabling custom strategies for view resolution. It allows setting a
/// locator directly or via a function.
/// </summary>
public static class ViewLocator
{
    /// <summary>
    /// Holds a reference to an optional IViewLocator instance. It is marked as internal and static, indicating it is
    /// accessible within the same assembly.
    /// </summary>
    [DBA(Never)]
    internal static IViewLocator? viewLocator;

    /// <summary>
    /// Sets the view locator for the application, allowing for custom view resolution.
    /// </summary>
    /// <param name="viewLocator">The custom view resolution strategy to be used by the application.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided view resolution strategy is null.</exception>
    public static void SetViewLocator(IViewLocator viewLocator)
    {
        ViewLocator.viewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));
    }

    /// <summary>
    /// Sets a custom view locator for resolving views based on string keys. It initializes the view locator with the
    /// provided function.
    /// </summary>
    /// <param name="viewLocatorFunc">The function used to locate views based on a string identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided function for locating views is null.</exception>
    public static void SetViewLocator(Func<string, Visual> viewLocatorFunc)
    {
        _ = viewLocatorFunc ?? throw new ArgumentNullException(nameof(viewLocator));

        ViewLocator.viewLocator = new _ViewLocator(viewLocatorFunc);
    }

    /// <summary>
    /// Locates visual elements using a function that maps string tokens to Visual objects. Initializes with a function
    /// that cannot be null.
    /// </summary>
    internal class _ViewLocator : IViewLocator
    {
        private readonly Func<string, Visual> func;

        /// <summary>
        /// Initializes a new instance of the ViewLocator class using a function that maps a string to a Visual object.
        /// </summary>
        /// <param name="func">The function that converts a string input into a Visual output.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided function is null.</exception>
        public _ViewLocator(Func<string, Visual> func)
        {
            this.func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Locates a visual element based on a provided token.
        /// </summary>
        /// <param name="viewToken">The token used to identify the specific visual element.</param>
        /// <returns>Returns the visual element associated with the given token.</returns>
        public Visual Locate(string viewToken)
        {
            return func(viewToken);
        }
    }
}
