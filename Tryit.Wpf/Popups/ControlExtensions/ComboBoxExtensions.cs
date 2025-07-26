using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using static System.Reflection.BindingFlags;

namespace Tryit.Wpf;

/// <summary>
/// Provides extension methods for ComboBox to manage enumeration values and types. Allows setting and getting enum
/// values and types.
/// </summary>
public static class ComboBoxExtensions
{
    /// <summary>
    /// Retrieves the value of an enumeration from a specified combo box.
    /// </summary>
    /// <param name="comboBox">The combo box from which the enumeration value is obtained.</param>
    /// <returns>Returns the enumeration value associated with the combo box, or null if not found.</returns>
    public static object? GetEnumValue(ComboBox comboBox)
    {
        return (object?)comboBox.GetValue(EnumValueProperty);
    }

    /// <summary>
    /// Sets the value of an enumeration property for a specified combo box.
    /// </summary>
    /// <param name="comboBox">The control that displays a list of options for the user to select from.</param>
    /// <param name="value">The value to be assigned to the enumeration property of the combo box.</param>
    public static void SetEnumValue(ComboBox comboBox, object? value)
    {
        comboBox.SetValue(EnumValueProperty, value);
    }

    /// <summary>
    /// Registers an attached dependency property named 'EnumValue' for ComboBoxExtensions. It updates the ComboBox when
    /// the property value changes.
    /// </summary>
    public static readonly DependencyProperty EnumValueProperty = DependencyProperty.RegisterAttached(
        "EnumValue",
        typeof(object),
        typeof(ComboBoxExtensions),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (s, e) =>
            {
                if (s is ComboBox comboBox && e.NewValue?.GetHashCode() != e.OldValue?.GetHashCode())
                {
                    Initialize(comboBox, null!, e.NewValue, null);
                }
            }
        )
    );

    /// <summary>
    /// Retrieves the enum type associated with a specified ComboBox.
    /// </summary>
    /// <param name="obj">The ComboBox from which the enum type is being retrieved.</param>
    /// <returns>Returns the enum type as a Type object.</returns>
    public static Type GetEnumType(ComboBox obj)
    {
        return (Type)obj.GetValue(EnumTypeProperty);
    }

    /// <summary>
    /// Sets the enum type for a ComboBox control.
    /// </summary>
    /// <param name="obj">The ComboBox control that will have its enum type set.</param>
    /// <param name="value">The type of the enum that will be assigned to the ComboBox.</param>
    public static void SetEnumType(ComboBox obj, Type value)
    {
        obj.SetValue(EnumTypeProperty, value);
    }

    /// <summary>
    /// Registers an attached property for a ComboBox to specify an enum type. It initializes the ComboBox when the enum
    /// type changes.
    /// </summary>
    public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.RegisterAttached(
        "EnumType",
        typeof(Type),
        typeof(ComboBoxExtensions),
        new PropertyMetadata(
            null,
            (s, e) =>
            {
                if (s is not ComboBox comboBox)
                {
                    return;
                }

                if (e.NewValue is Type enumType)
                {
                    Initialize(comboBox, enumType, null, null);
                }

                if (e.OldValue is not Type oldEnumType)
                {
                    comboBox.SelectionChanged += ComboBox_SelectionChanged;
                }
            }
        )
    );

    /// <summary>
    /// Retrieves a collection of ignored items from a specified user interface element.
    /// </summary>
    /// <param name="comboBox">The user interface element from which to extract the ignored items.</param>
    /// <returns>A collection of ignored items associated with the specified user interface element.</returns>
    public static IEnumerable GetIgnores(ComboBox comboBox)
    {
        return (IEnumerable)comboBox.GetValue(IgnoresProperty);
    }

    /// <summary>
    /// Sets the value of a property related to ignored items in a specified control.
    /// </summary>
    /// <param name="comboBox">The control that will have its property updated with the provided value.</param>
    /// <param name="value">The collection of items that will be set as ignored for the specified control.</param>
    public static void SetIgnores(ComboBox comboBox, IEnumerable value)
    {
        comboBox.SetValue(IgnoresProperty, value);
    }

    /// <summary>
    /// Defines an attached property named 'Ignores' for ComboBox, allowing the association of an IEnumerable collection. It
    /// initializes the ComboBox when the property changes.
    /// </summary>
    public static readonly DependencyProperty IgnoresProperty = DependencyProperty.RegisterAttached(
        "Ignores",
        typeof(IEnumerable),
        typeof(ComboBoxExtensions),
        new PropertyMetadata(
            null,
            (s, e) =>
            {
                if (s is ComboBox comboBox)
                {
                    Initialize(comboBox, null!, null, e.NewValue as IEnumerable);
                }
            }
        )
    );

    /// <summary>
    /// Initializes a ComboBox with items based on a specified enumeration type and handles selection and ignoring certain
    /// values.
    /// </summary>
    /// <param name="comboBox">The control that will be populated with items derived from the enumeration.</param>
    /// <param name="enumType">Specifies the type of enumeration used to generate the items for the ComboBox.</param>
    /// <param name="enumValue">Sets the selected item in the ComboBox based on the provided enumeration value.</param>
    /// <param name="ignores">Contains values that should be excluded from the ComboBox items.</param>
    private static void Initialize(ComboBox comboBox, Type enumType, object? enumValue, IEnumerable? ignores)
    {
        if (enumType is not null)
        {
            comboBox.Items.Clear();

            var sourceItems = enumType
                .GetFields(Public | Static)
                .Where(x => x.IsStatic && x.IsPublic)
                .Where(x => x is not null)
                .Select(x => new
                {
                    IsBrowsable = x.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true,
                    DisplayName = x.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? x.Name,
                    Value = x.GetValue(null),
                })
                .Where(x => x.IsBrowsable)
                .Select(x => new DisplayItem(x.Value!, x.DisplayName))
                .ToArray();

            comboBox.DisplayMemberPath = nameof(DisplayItem.DisplayName);

            comboBox.ItemsSource = new ObservableCollection<DisplayItem>(sourceItems);
        }

        if (ignores is not null && comboBox.ItemsSource is IList<DisplayItem> ignoreDisplayItems)
        {
            foreach (var ignore in ignores)
            {
                if (ignore is null || ignore.GetType() != GetEnumType(comboBox))
                {
                    continue;
                }

                var hashCode = ignore?.GetHashCode();

                for (int i = ignoreDisplayItems.Count - 1; i >= 0; i--)
                {
                    if (ignoreDisplayItems[i].GetHashCode() == hashCode)
                    {
                        ignoreDisplayItems.RemoveAt(i);
                    }
                }
            }
        }

        if (enumValue is not null)
        {
            int hashCode = enumValue.GetHashCode();

            if (comboBox.SelectedItem is DisplayItem item && item.GetHashCode() == hashCode)
            {
                return;
            }

            if (comboBox.ItemsSource is IList<DisplayItem> items)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].GetHashCode() == hashCode)
                    {
                        comboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles the selection change event for a ComboBox, updating a property based on the selected item.
    /// </summary>
    /// <param name="sender">Represents the source of the event, typically the ComboBox that triggered the selection change.</param>
    /// <param name="e">Contains the event data related to the selection change, providing information about the new and old selections.</param>
    private static void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox || comboBox.SelectedItem is not DisplayItem item || comboBox.ItemsSource is not IList<DisplayItem> items)
        {
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == item)
            {
                comboBox.SetCurrentValue(ComboBoxExtensions.EnumValueProperty, item.Value);

                break;
            }
        }
    }

    /// <summary>
    /// Represents an item with a value and a display name. It includes methods for equality comparison and hash code
    /// generation.
    /// </summary>
    public class DisplayItem
    {
        [DBA(Never)]
        int hashCode;

        /// <summary>
        /// Initializes a new instance of the DisplayItem class with a specified value and display name.
        /// </summary>
        /// <param name="value">Represents the underlying data associated with the display item.</param>
        /// <param name="displayName">Represents the name that will be shown for the display item.</param>
        public DisplayItem(object value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
            hashCode = Value?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Represents a value that can be of any type, allowing for null. It provides a way to access the underlying data.
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Represents the display name of an object. It is a read-only property.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Returns the string representation of the object, which is the value of DisplayName.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => DisplayName;

        /// <summary>
        /// Calculates and returns the hash code for the current instance.
        /// </summary>
        /// <returns>An integer representing the hash code.</returns>
        public override int GetHashCode() => hashCode;

        /// <summary>
        /// Compares two DisplayItem instances for equality based on their hash codes. Returns false if either instance is
        /// null.
        /// </summary>
        /// <param name="left">Represents the first instance to compare for equality.</param>
        /// <param name="right">Represents the second instance to compare for equality.</param>
        /// <returns>Returns true if both instances have the same hash code; otherwise, false.</returns>
        public static bool operator ==(DisplayItem left, DisplayItem right)
        {
            if (left is null || right is null)
            {
                return false;
            }
            return left.GetHashCode() == right.GetHashCode();
        }

        /// <summary>
        /// Compares two DisplayItem instances for inequality.
        /// </summary>
        /// <param name="left">The first instance to compare for inequality.</param>
        /// <param name="right">The second instance to compare for inequality.</param>
        /// <returns>True if the instances are not equal; otherwise, false.</returns>
        public static bool operator !=(DisplayItem left, DisplayItem right)
        {
            return (left == right) == false;
        }

        /// <summary>
        /// Compares the current instance with another object for equality. Returns true if the objects are considered
        /// equal.
        /// </summary>
        /// <param name="obj">An object to compare with the current instance for equality.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is DisplayItem item && this == item;
        }
    }
}
