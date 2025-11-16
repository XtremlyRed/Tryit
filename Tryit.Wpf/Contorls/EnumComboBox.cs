using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
    /// Backing collection bound to <see cref="ItemsControl.ItemsSource"/> (populated when <see cref="EnumType"/> changes).
    /// </summary>
    private readonly ObservableCollection<EnumDisplay> enumInfos = [];

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
        ItemsSource = enumInfos;
        DisplayMemberPath = nameof(EnumDisplay.DisplayName);
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
    public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
        nameof(EnumType),
        typeof(Type),
        typeof(EnumComboBox),
        new FrameworkPropertyMetadata(
            null,
            (s, e) =>
            {
                if (s is not EnumComboBox @enum)
                    return;

                @enum.enumInfos.Clear();

                if (e.NewValue is not Type type || type.IsEnum == false)
                    return;

                if (EnumInfoMaps.TryGetValue(type, out EnumDisplay[]? infos) == false)
                {
                    EnumInfoMaps[type] = infos = type.GetFields()
                        .Where(i => i.IsStatic)
                        .Select(i => new
                        {
                            Field = i,
                            Value = i.GetValue(null)!,
                            i.Name,
                            DisplayName = i.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? i.Name,
                        })
                        .Select(i => new EnumDisplay(i.Value, i.Value.GetHashCode(), i.DisplayName))
                        .ToArray();
                }

                for (int i = 0; i < infos.Length; i++)
                    @enum.enumInfos.Add(infos[i]);

                @enum.TryRemoveIgnores(@enum.IgnoreValues);
                @enum.TrySetCurrent(@enum.EnumValue);
            }
        )
    );

    /// <summary>
    /// Gets or sets the enum <see cref="Type"/> whose members should populate the ComboBox.
    /// Must be assignable from <see cref="Enum"/>.
    /// </summary>
    public Type EnumType
    {
        get => (Type)GetValue(EnumTypeProperty);
        set => SetValue(EnumTypeProperty, value);
    }

    #region Internal List Operations

    /// <summary>
    /// Removes ignored enum values from the current observable collection (if they exist).
    /// Comparison is performed via hash code to avoid boxing overhead.
    /// </summary>
    /// <param name="ignores">Collection of enum values to ignore.</param>
    private void TryRemoveIgnores(IEnumerable? ignores)
    {
        if (ignores is null)
            return;

        foreach (object? ignore in ignores)
        {
            if (ignore is null)
                continue;

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
        if (enumValue is null)
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
    public static readonly DependencyProperty EnumValueProperty = DependencyProperty.Register(
        nameof(EnumValue),
        typeof(object),
        typeof(EnumComboBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (s, e) =>
            {
                if (s is EnumComboBox @enum)
                    @enum.TrySetCurrent(e.NewValue);
            }
        )
    );

    /// <summary>
    /// Gets or sets the currently selected enum value (boxed).
    /// Setting this property synchronizes the ComboBox selection.
    /// </summary>
    public object EnumValue
    {
        get => GetValue(EnumValueProperty);
        set => SetValue(EnumValueProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="EmptyValue"/> dependency property.
    /// This can be used in a custom template to display placeholder text when no selection exists.
    /// </summary>
    public static readonly DependencyProperty EmptyValueProperty = DependencyProperty.Register(
        nameof(EmptyValue),
        typeof(string),
        typeof(EnumComboBox),
        new FrameworkPropertyMetadata(
            "None",
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (s, e) =>
            {
                if (s is EnumComboBox @enum)
                    @enum.TrySetCurrent(@enum.EnumValue);
            }
        )
    );

    /// <summary>
    /// Gets or sets the placeholder text (not directly used by default template; provided for customization).
    /// </summary>
    public string EmptyValue
    {
        get => (string)GetValue(EmptyValueProperty);
        set => SetValue(EmptyValueProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="IgnoreValues"/> dependency property.
    /// When changed: removes the supplied values from the internal list (if present) and re-applies current selection.
    /// </summary>
    public static readonly DependencyProperty IgnoreValuesProperty = DependencyProperty.Register(
        nameof(IgnoreValues),
        typeof(IEnumerable),
        typeof(EnumComboBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (s, e) =>
            {
                if (s is EnumComboBox @enum)
                {
                    @enum.TryRemoveIgnores(e.NewValue as IEnumerable);
                    @enum.TrySetCurrent(@enum.EnumValue);
                }
            }
        )
    );

    /// <summary>
    /// Gets or sets a collection of enum values that should be excluded from the ComboBox items.
    /// </summary>
    public IEnumerable IgnoreValues
    {
        get => (IEnumerable)GetValue(IgnoreValuesProperty);
        set => SetValue(IgnoreValuesProperty, value);
    }

    /// <summary>
    /// Overrides selection change logic to:
    ///  - Update <see cref="EnumValue"/> with the newly selected enum value (or null).
    ///  - Raise <see cref="SelectionEnumValueChanged"/> with the new enum value (if not null).
    /// </summary>
    /// <param name="e">Selection change event args.</param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
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
public class SelectionChangedRoutedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the SelectionChangedRoutedEventArgs class with the specified new and old selection
    /// values.
    /// </summary>
    /// <param name="newValue">The new value selected. Represents the current selection after the change.</param>
    /// <param name="oldValue">The previous value that was selected before the change occurred.</param>
    public SelectionChangedRoutedEventArgs(Enum newValue, Enum oldValue)
    {
        NewValue = newValue;
        OldValue = oldValue;
    }

    /// <summary>
    /// Gets the new value assigned to the property or field represented by this change event.
    /// </summary>
    public Enum NewValue { get; }

    /// <summary>
    /// Gets the previous value of the enumeration before the most recent change.
    /// </summary>
    public Enum OldValue { get; }
}
