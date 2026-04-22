using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Provides extension methods for managing and querying transform collections on FrameworkElement instances, enabling
/// convenient addition and indexing of specific transform types within the RenderTransform property.
/// </summary>
/// <remarks>These extension methods simplify working with the RenderTransform property by allowing developers to
/// append new transforms or locate existing ones of a specified type. The methods handle cases where the
/// RenderTransform is not already a TransformGroup, ensuring transforms are managed consistently. Use these methods to
/// build or inspect complex transformation chains on UI elements in a type-safe manner.</remarks>
public static class VisualTransformExtensions
{
    /// <summary>
    /// Attempts to append a new transform of type T to the specified FrameworkElement and returns the index of the
    /// appended transform.
    /// </summary>
    /// <typeparam name="T">The type of Transform to append. Must have a parameterless constructor.</typeparam>
    /// <param name="element">The FrameworkElement to which the new transform will be appended.</param>
    /// <param name="index">When this method returns, contains the zero-based index at which the new transform was appended, or -1 if the
    /// operation failed.</param>
    /// <returns>true if the transform was successfully appended; otherwise, false.</returns>
    public static bool TryAppend<T>(this FrameworkElement element, out int index)
        where T : Transform, new()
    {
        return TryAppend<T>(element, new T(), out index);
    }

    /// <summary>
    /// Attempts to append a new transform of the specified type to the given FrameworkElement using a factory function.
    /// </summary>
    /// <typeparam name="T">The type of Transform to append. Must derive from Transform.</typeparam>
    /// <param name="element">The FrameworkElement to which the transform will be appended.</param>
    /// <param name="transformFunc">A function that creates an instance of the transform to append. The function is invoked only if the append
    /// operation proceeds.</param>
    /// <param name="index">When this method returns, contains the zero-based index at which the transform was appended, or -1 if the
    /// operation failed.</param>
    /// <returns>true if the transform was successfully appended; otherwise, false.</returns>
    public static bool TryAppend<T>(this FrameworkElement element, Func<T> transformFunc, out int index)
        where T : Transform
    {
        return TryAppend<T>(element, transformFunc(), out index);
    }

    /// <summary>
    /// Attempts to append a transform of type T to the RenderTransform of the specified FrameworkElement, or returns
    /// the index of an existing transform of type T if one is already present.
    /// </summary>
    /// <remarks>If the element's RenderTransform is not a TransformGroup, it will be replaced with a new
    /// TransformGroup containing the original transform (if any) and the new transform. This method always returns true
    /// unless an unexpected error occurs.</remarks>
    /// <typeparam name="T">The type of transform to append. Must derive from Transform.</typeparam>
    /// <param name="element">The FrameworkElement whose RenderTransform will be checked or modified.</param>
    /// <param name="transform">The transform to append if a transform of type T does not already exist.</param>
    /// <param name="index">When this method returns, contains the zero-based index of the existing or newly added transform of type T
    /// within the RenderTransform collection.</param>
    /// <returns>true if a transform of type T exists or was successfully appended; otherwise, false.</returns>
    public static bool TryAppend<T>(this FrameworkElement element, T transform, out int index)
        where T : Transform
    {
        if (element.RenderTransform is not TransformGroup transformGroup)
        {
            transformGroup = new TransformGroup();

            if (element.RenderTransform is not null)
            {
                transformGroup.Children.Add(element.RenderTransform);
            }

            element.RenderTransform = transformGroup;
        }

        for (int i = 0, length = transformGroup.Children.Count; i < length; i++)
        {
            if (transformGroup.Children[i] is T)
            {
                index = i;

                return true;
            }
        }

        index = transformGroup.Children.Count;

        transformGroup.Children.Add(transform);

        return true;
    }

    /// <summary>
    /// Attempts to find the index of the first child transform of the specified type in the element's RenderTransform
    /// collection.
    /// </summary>
    /// <remarks>Use this method to determine the position of a specific type of transform within a
    /// FrameworkElement's RenderTransform collection. The method only searches direct children of the
    /// TransformGroup.</remarks>
    /// <typeparam name="T">The type of transform to search for within the RenderTransform collection.</typeparam>
    /// <param name="element">The FrameworkElement whose RenderTransform collection is searched.</param>
    /// <param name="index">When this method returns, contains the zero-based index of the first child transform of type T if found;
    /// otherwise, -1.</param>
    /// <returns>true if a child transform of type T is found; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if the element's RenderTransform is not a TransformGroup.</exception>
    public static bool TryIndex<T>(this FrameworkElement element, out int index)
    {
        if (element.RenderTransform is not TransformGroup transformGroup)
        {
            throw new ArgumentException("invalid transfrom collection");
        }
        for (int i = 0, length = transformGroup.Children.Count; i < length; i++)
        {
            if (transformGroup.Children[i] is T)
            {
                index = i;

                return true;
            }
        }

        index = -1;
        return false;
    }
}
