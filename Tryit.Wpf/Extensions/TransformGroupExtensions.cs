using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides extension methods for working with TransformGroup collections, enabling type-based search and conditional
/// addition of child transforms.
/// </summary>
/// <remarks>These extension methods simplify common operations on TransformGroup instances, such as locating or
/// adding transforms of a specific type. All methods are static and intended for use with WPF TransformGroup
/// objects.</remarks>
public static class TransformGroupExtensions
{
    /// <summary>
    /// Attempts to find the index of the first child transform of the specified type in the transform collection.
    /// </summary>
    /// <remarks>This method searches only for the first occurrence of a child transform of the specified
    /// type. If multiple children of type T exist, only the index of the first is returned.</remarks>
    /// <typeparam name="T">The type of transform to search for. Must derive from Transform.</typeparam>
    /// <param name="transformCollection">The TransformGroup to search for a child transform of type T. Cannot be null.</param>
    /// <param name="index">When this method returns, contains the zero-based index of the first child transform of type T if found;
    /// otherwise, -1.</param>
    /// <returns>true if a child transform of type T is found; otherwise, false.</returns>
    public static bool TryIndexOf<T>(this TransformGroup transformCollection, out int index)
        where T : Transform
    {
        for (int i = 0; i < transformCollection.Children.Count; i++)
        {
            if (transformCollection.Children[i] is T)
            {
                index = i;
                return true;
            }
        }
        index = -1;

        return false;
    }

    /// <summary>
    /// Attempts to add a new transform of type <typeparamref name="T"/> to the specified <see cref="TransformGroup"/>
    /// if one does not already exist.
    /// </summary>
    /// <remarks>This method checks whether a transform of type <typeparamref name="T"/> is already present in
    /// the <paramref name="transformCollection"/>. If not, it creates and adds a new instance of <typeparamref
    /// name="T"/>. Only one instance of each transform type can be added using this method.</remarks>
    /// <typeparam name="T">The type of transform to add. Must derive from <see cref="Transform"/> and have a public parameterless
    /// constructor.</typeparam>
    /// <param name="transformCollection">The <see cref="TransformGroup"/> to which the transform will be added.</param>
    /// <returns><see langword="true"/> if a new transform of type <typeparamref name="T"/> was added; otherwise, <see
    /// langword="false"/> if a transform of that type already exists in the collection.</returns>
    public static bool TryAdd<T>(this TransformGroup transformCollection)
        where T : Transform, new()
    {
        for (int i = 0; i < transformCollection.Children.Count; i++)
        {
            if (transformCollection.Children[i] is T)
            {
                return false;
            }
        }

        transformCollection.Children.Add(new T());

        return true;
    }
}
