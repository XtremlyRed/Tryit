using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

#pragma warning disable CS9113

namespace Tryit.Wpf;

/// <summary>
/// A ComboBox specialized for binding to enumeration types.
/// Set <see cref="EnumType"/> to any <see cref="Enum"/> type and the control will populate items automatically.
/// Supports:
///  - Caching enum metadata for performance.
///  - Ignoring specific enum values via <see cref="IgnoreValues"/>.
///  - Two-way synchronization of the selected enum value through <see cref="EnumValue"/>.
///  - Custom display names via <see cref="DisplayNameAttribute"/> on enum fields.
///  - Notification of enum selection changes through <see cref="SelectionEnumValueChanged"/>.
/// </summary>
/// <remarks>
/// Typical usage:
/// <code>
/// &lt;wpf:EnumComboBox EnumType="{x:Type local:MyEnum}"
///                     EnumValue="{Binding SelectedValue, Mode=TwoWay}" /&gt;
/// </code>
/// To ignore certain enum values assign <see cref="IgnoreValues"/> with a collection containing those values.
/// </remarks>
/// <seealso cref="ComboBox" />
public class EnumComboBox : ComboBox
{
    /// <summary>
    /// Global cache of enum metadata (array of <see cref="EnumDisplay"/>) keyed by enum <see cref="Type"/>.
    /// Prevents repeated reflection on enum members.
    /// </summary>
    private static readonly IDictionary<Type, EnumDisplay[]> EnumInfoMaps = new ConcurrentDictionary<Type, EnumDisplay[]>();

    /// <summary>
    /// Static constructor: overrides default style key so the control uses the base ComboBox style unless retemplated.
    /// </summary>
    static EnumComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumComboBox), new FrameworkPropertyMetadata(typeof(ComboBox)));
    }

    /// <summary>
    /// Internal wrapper that holds enum value, its hash (used for quick lookup) and optional display name.
    /// </summary>
    [DebuggerDisplay("{DisplayName,nq} : {Value}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private record EnumDisplay(object? Value, int HashCode, string? DisplayName)
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayName ?? string.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumComboBox"/> class.
    /// Sets up the item source and display member path.
    /// </summary>
    public EnumComboBox()
    {
        ItemsSource = new ObservableCollection<EnumDisplay>();
        DisplayMemberPath = nameof(EnumDisplay.DisplayName);
    }

    /// <summary>
    /// Gets or sets the enum <see cref="Type"/> whose members should populate the ComboBox.
    /// Must be assignable from <see cref="Enum"/>.
    /// </summary>
    public Type EnumType
    {
        get => (Type)GetValue(EnumTypeProperty);
        set => SetValue(EnumTypeProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="EnumType"/> dependency property.
    /// When changed:
    ///  - Clears previous items.
    ///  - Reflects enum members (with caching).
    ///  - Populates internal collection.
    ///  - Applies <see cref="IgnoreValues"/>.
    ///  - Tries to select current <see cref="EnumValue"/>.
    /// </summary>
    public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(nameof(EnumType), typeof(Type), typeof(EnumComboBox), new FrameworkPropertyMetadata(null, OnEnumTypeChanged));

    private static void OnEnumTypeChanged(DependencyObject s, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (s is not EnumComboBox @enum || @enum.ItemsSource is not IList<EnumDisplay> enumInfos)
        {
            return;
        }

        if (e.NewValue is not Type type || type.IsEnum == false)
        {
            return;
        }

        enumInfos.Clear();

        if (EnumInfoMaps.TryGetValue(type, out EnumDisplay[]? infos) == false)
        {
            EnumInfoMaps[type] = infos = type.GetFields()
                .Where(i => i.IsStatic)
                .Select(i => new { Value = i.GetValue(null)!, DisplayName = i.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? i.GetCustomAttribute<DescriptionAttribute>()?.Description ?? i.Name })
                .Select(i => new EnumDisplay(i.Value, i.Value.GetHashCode(), i.DisplayName))
                .ToArray();
        }

        for (int i = 0; i < infos.Length; i++)
        {
            enumInfos.Add(infos[i]);
        }

        @enum.TryRemoveIgnores(@enum.IgnoreValues);

        @enum.TrySetCurrent(@enum.EnumValue);
    }

    #region Internal List Operations

    /// <summary>
    /// Removes ignored enum values from the current observable collection (if they exist).
    /// Comparison is performed via hash code to avoid boxing overhead.
    /// </summary>
    /// <param name="ignores">Collection of enum values to ignore.</param>
    private void TryRemoveIgnores(IEnumerable? ignores)
    {
        if (ignores is null || ItemsSource is not IList<EnumDisplay> enumInfos)
        {
            return;
        }

        foreach (object? ignore in ignores)
        {
            if (ignore is null)
            {
                continue;
            }

            int hashCode = ignore.GetHashCode();

            if (enumInfos.FirstOrDefault(i => i.HashCode == hashCode) is EnumDisplay info)
            {
                enumInfos.Remove(info);
            }
        }
    }

    /// <summary>
    /// Attempts to set the selection to the item that matches the supplied enum value.
    /// If the value is null or not found, clears selection (<see cref="Selector.SelectedIndex"/> = -1).
    /// </summary>
    /// <param name="enumValue">Enum value to select.</param>
    private void TrySetCurrent(object enumValue)
    {
        if (enumValue is null || ItemsSource is not IList<EnumDisplay> enumInfos)
        {
            SelectedIndex = -1;
            return;
        }

        int hashCode = enumValue.GetHashCode();

        for (int i = 0; i < enumInfos.Count; i++)
        {
            if (enumInfos[i].HashCode == hashCode)
            {
                SelectedIndex = i;
            }
        }
    }

    #endregion

    /// <summary>
    /// Identifies the <see cref="EnumValue"/> dependency property.
    /// This property binds two-way by default and reflects the currently selected enum value (boxed).
    /// Updating this property programmatically attempts to update the selection.
    /// </summary>
    public static readonly DependencyProperty EnumValueProperty = DependencyProperty.Register(nameof(EnumValue), typeof(object), typeof(EnumComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEnumValueChanged));

    /// <summary>
    /// Gets or sets the currently selected enum value (boxed).
    /// Setting this property synchronizes the ComboBox selection.
    /// </summary>
    public object EnumValue
    {
        get => GetValue(EnumValueProperty);
        set => SetValue(EnumValueProperty, value);
    }

    private static void OnEnumValueChanged(DependencyObject s, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (s is EnumComboBox @enum)
        {
            @enum.TrySetCurrent(e.NewValue);
        }
    }

    /// <summary>
    /// Identifies the <see cref="EmptyValue"/> dependency property.
    /// This can be used in a custom template to display placeholder text when no selection exists.
    /// </summary>
    public static readonly DependencyProperty EmptyValueProperty = DependencyProperty.Register(nameof(EmptyValue), typeof(string), typeof(EnumComboBox), new FrameworkPropertyMetadata("None", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEmptyValueChanged));

    /// <summary>
    /// Gets or sets the placeholder text (not directly used by default template; provided for customization).
    /// </summary>
    public string EmptyValue
    {
        get => (string)GetValue(EmptyValueProperty);
        set => SetValue(EmptyValueProperty, value);
    }

    private static void OnEmptyValueChanged(DependencyObject s, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (s is EnumComboBox @enum)
        {
            @enum.TrySetCurrent(@enum.EnumValue);
        }
    }

    /// <summary>
    /// Identifies the <see cref="IgnoreValues"/> dependency property.
    /// When changed: removes the supplied values from the internal list (if present) and re-applies current selection.
    /// </summary>
    public static readonly DependencyProperty IgnoreValuesProperty = DependencyProperty.Register(nameof(IgnoreValues), typeof(IEnumerable), typeof(EnumComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIgnoreValuesChanged));

    /// <summary>
    /// Gets or sets a collection of enum values that should be excluded from the ComboBox items.
    /// </summary>
    public IEnumerable IgnoreValues
    {
        get => (IEnumerable)GetValue(IgnoreValuesProperty);
        set => SetValue(IgnoreValuesProperty, value);
    }

    private static void OnIgnoreValuesChanged(DependencyObject s, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (s is EnumComboBox @enum)
        {
            @enum.TryRemoveIgnores(e.NewValue as IEnumerable);
            @enum.TrySetCurrent(@enum.EnumValue);
        }
    }

    /// <summary>
    /// Overrides selection change logic to:
    ///  - Update <see cref="EnumValue"/> with the newly selected enum value (or null).
    ///  - Raise <see cref="SelectionEnumValueChanged"/> with the new enum value (if not null).
    /// </summary>
    /// <param name="e">Selection change event args.</param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        if (ItemsSource is not IList<EnumDisplay> enumInfos)
        {
            return;
        }

        object? updateValue = null;

        if (SelectedIndex >= 0 && SelectedIndex < enumInfos.Count)
        {
            updateValue = enumInfos[SelectedIndex].Value;
        }

        Enum? oldValue = GetValue(EnumValueProperty) as Enum;

        SetCurrentValue(EnumValueProperty, updateValue);

        // When updateValue is null we cannot cast to Enum (should only occur when clearing selection).
        if (updateValue is Enum ev)
        {
            RaiseEvent(new SelectionChangedRoutedEventArgs(ev, oldValue!) { RoutedEvent = SelectionEnumValueChangedEvent, Source = this });
        }

        base.OnSelectionChanged(e);
    }

    /// <summary>
    /// Identifies the SelectionEnumValueChanged routed event, which occurs when the selected enumeration value changes
    /// in the EnumComboBox.
    /// </summary>
    /// <remarks>Handlers for this event are invoked when the selection of the EnumComboBox changes to a
    /// different enumeration value. This event uses the bubbling routing strategy, allowing it to be handled by parent
    /// elements in the visual tree.</remarks>
    public static readonly RoutedEvent SelectionEnumValueChangedEvent = EventManager.RegisterRoutedEvent(name: "SelectionEnumValueChanged", routingStrategy: RoutingStrategy.Bubble, handlerType: typeof(SelectionChangedEventHandler), ownerType: typeof(EnumComboBox));

    /// <summary>
    /// Occurs when the selected enum value changes and provides the newly selected enum.
    /// Does not fire when selection becomes null (unless previous selection produced a valid enum).
    /// </summary>
    public event SelectionChangedEventHandler SelectionEnumValueChanged
    {
        add => AddHandler(SelectionEnumValueChangedEvent, value);
        remove => RemoveHandler(SelectionEnumValueChangedEvent, value);
    }
}

/// <summary>
/// Represents the method that handles a selection changed event in a user interface control.
/// </summary>
/// <param name="sender">The source of the event, typically the control where the selection changed.</param>
/// <param name="changedRoutedEventArgs">An object that contains the event data for the selection change.</param>
public delegate void SelectionChangedEventHandler(object? sender, SelectionChangedRoutedEventArgs changedRoutedEventArgs);

/// <summary>
/// Provides data for an event that occurs when a selection changes, containing the new and old selected values.
/// </summary>
/// <remarks>Use this class to access the previous and current selection values when handling selection change
/// events in controls that use enumerated types. Both the new and old values are provided as Enum instances, allowing
/// for flexible handling of different enumeration types.</remarks>
public class SelectionChangedRoutedEventArgs(Enum NewValue, Enum OldValue) : RoutedEventArgs { }
