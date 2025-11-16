using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Provides a markup extension that retrieves a resource by key from the application resource dictionary at XAML parse
/// time.
/// </summary>
/// <remarks>Use ResourceExtension in XAML to reference resources dynamically by key. If the specified resource
/// key is not found or is null, the extension returns DependencyProperty.UnsetValue, allowing the property system to
/// handle missing resources according to its default behavior. This extension is typically used to support resource
/// lookups in custom markup scenarios.</remarks>
public class ResourceExtension : MarkupExtension
{
    /// <summary>
    /// Initializes a new instance of the ResourceExtension class.
    /// </summary>
    public ResourceExtension() { }

    /// <summary>
    /// Initializes a new instance of the ResourceExtension class with the specified resource key.
    /// </summary>
    /// <param name="resourceKey">The key that identifies the resource to be referenced. Can be null to indicate no specific resource.</param>
    public ResourceExtension(string? resourceKey)
    {
        ResourceKey = resourceKey;
    }

    /// <summary>
    /// Gets or sets the key that identifies the associated resource.
    /// </summary>
    public string? ResourceKey { get; set; }

    /// <summary>
    /// Returns the object that should be set on the property where this markup extension is applied, based on the
    /// specified resource key.
    /// </summary>
    /// <remarks>If the resource key is null, empty, or consists only of white-space characters, or if the
    /// resource cannot be found, this method returns <see cref="DependencyProperty.UnsetValue"/>. This allows the
    /// property system to handle the missing resource according to its default behavior.</remarks>
    /// <param name="serviceProvider">An object that can provide services for the markup extension. This parameter is typically provided by the XAML
    /// infrastructure and may be used to access contextual information.</param>
    /// <returns>The resource object associated with the specified resource key if found; otherwise, <see
    /// cref="DependencyProperty.UnsetValue"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(ResourceKey) == false)
        {
            object? resource = Application.Current.TryFindResource(ResourceKey);

            if (resource is not null)
            {
                return resource;
            }
        }

        return DependencyProperty.UnsetValue;
    }
}
