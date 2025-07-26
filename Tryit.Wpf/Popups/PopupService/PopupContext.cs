using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using static Tryit.Wpf.PopupContext;
using static Tryit.Wpf.PopupService;

namespace Tryit.Wpf;

/// <summary>
/// PopupContext is an abstract class that manages a collection of button items and their context indices. It provides
/// methods for equality comparison and event publishing.
/// </summary>
public abstract class PopupContext : IEquatable<PopupContext>, IEquatable<object>
{
    /// <summary>
    /// A static integer variable initialized to the minimum value of an integer. It is marked to never be displayed in
    /// the debugger.
    /// </summary>
    [DebuggerBrowsable(Never)]
    static int configIndex = int.MinValue;

    /// <summary>
    /// Stores an integer value representing the context index. It is marked as private and read-only.
    /// </summary>
    [DebuggerBrowsable(Never)]
    private readonly int contextIndex;

    /// <summary>
    /// Initializes a new instance of PopupContext and increments the configIndex in a thread-safe manner.
    /// </summary>
    protected PopupContext()
    {
        contextIndex = Interlocked.Increment(ref configIndex);
    }

    /// <summary>
    /// Holds a list of ButtonItem objects. The list is internal and not visible in the debugger.
    /// </summary>
    [DebuggerBrowsable(Never)]
    internal List<ButtonItem> buttonItems = new List<ButtonItem>();

    /// <summary>
    /// Returns an array of button content strings by selecting from buttonItems. The result is a new array containing the
    /// content of each button.
    /// </summary>
    public string[] Buttons => buttonItems.Select(i => i.ButtonContent).ToArray();

    /// <summary>
    /// Represents the primary index with a default value of 0. It can be accessed and modified as needed.
    /// </summary>
    public int PrimaryIndex { get; set; } = 0;

    /// <summary>
    /// Represents the title of an entity, which can be null. The property can only be set internally within its
    /// containing class.
    /// </summary>
    public string? Title { get; internal set; }

    /// <summary>
    /// Represents the content as a nullable string. It can be accessed internally within the assembly.
    /// </summary>
    public string? Content { get; internal set; }

    /// <summary>
    /// Returns a command that publishes an event with the button content when executed. The event is published using an
    /// event service.
    /// </summary>
    public virtual ICommand ClickCommand =>
        new Command(
            (btnContent) =>
            {
                var eventArgs = new PubEventArgs(btnContent!, this);
                eventService.Publish(eventArgs);
            }
        );

    /// <summary>
    /// Retrieves a default PopupContext with a specified number of buttons. The buttons included depend on the provided
    /// count.
    /// </summary>
    /// <param name="buttonCount">Specifies how many buttons to include in the PopupContext, affecting which buttons are added.</param>
    /// <returns>Returns a dictionary mapping button results to their string representations.</returns>
    internal static PopupContext GetDefault(int buttonCount = 3)
    {
        var dict = new Dictionary<ButtonResult, string>();

        if (buttonCount > 0)
            Add(dict, ButtonResult.Yes);
        if (buttonCount > 1)
            Add(dict, ButtonResult.No);
        if (buttonCount > 2)
            Add(dict, ButtonResult.Cancel);

        return dict!;

        static void Add(Dictionary<ButtonResult, string> dict, ButtonResult buttonResult)
        {
            dict[buttonResult] = buttonResult.ToString();
        }
    }

    /// <summary>
    /// Compares the current instance with another object for equality based on a specific index.
    /// </summary>
    /// <param name="obj">An object to compare with the current instance to determine if they are equal.</param>
    /// <returns>True if the objects are considered equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is PopupContext context && contextIndex == context.contextIndex;
    }

    /// <summary>
    /// Generates a hash code based on the context index value.
    /// </summary>
    /// <returns>An integer representing the hash code.</returns>
    public override int GetHashCode()
    {
        return contextIndex;
    }

    /// <summary>
    /// Compares the current instance with another object to determine if they are equal based on context index.
    /// </summary>
    /// <param name="other">The object to compare with the current instance for equality.</param>
    /// <returns>True if the context indices are equal; otherwise, false.</returns>
    public bool Equals(PopupContext? other)
    {
        return other?.contextIndex == contextIndex;
    }

    /// <summary>
    /// Compares two PopupContext objects for equality based on their contextIndex property.
    /// </summary>
    /// <param name="left">The first PopupContext object to compare.</param>
    /// <param name="right">The second PopupContext object to compare.</param>
    /// <returns>True if both objects are not null and have the same contextIndex; otherwise, false.</returns>
    public static bool operator ==(PopupContext? left, PopupContext? right)
    {
        return left is not null && right is not null && left.contextIndex == right.contextIndex;
    }

    /// <summary>
    /// Compares two PopupContext objects for inequality. Returns true if either object is null or their context indices
    /// differ.
    /// </summary>
    /// <param name="left">Represents the first PopupContext object to compare.</param>
    /// <param name="right">Represents the second PopupContext object to compare.</param>
    /// <returns>Indicates whether the two PopupContext objects are not equal.</returns>
    public static bool operator !=(PopupContext? left, PopupContext? right)
    {
        return left is null || right is null || left.contextIndex != right.contextIndex;
    }

    /// <summary>
    /// Converts a dictionary of button results and their associated strings into a nullable PopupContext.
    /// </summary>
    /// <param name="buttonContexts">A collection of button results mapped to their corresponding string values for context creation.</param>
    public static implicit operator PopupContext?(Dictionary<ButtonResult, string>? buttonContexts)
    {
        if (buttonContexts is null || buttonContexts.Count == 0)
        {
            return default!;
        }

        var builder = new PopupContextBuilder();

        foreach (var item in buttonContexts)
        {
            builder.Register(item.Value, item.Key);
        }

        return builder.Build();
    }

    /// <summary>
    /// A command implementation that allows execution and notifies subscribers of its execution status.
    /// </summary>
    /// <param name="Callback">Used to process the input by converting it to a string and invoking the provided action.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    private record Command(Action<string> Callback) : ICommand
    {
        /// <summary>
        /// An event that occurs when the ability of a command to execute changes. It allows subscribers to be notified
        /// when the command's execution status updates.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Indicates whether the command can be executed based on the provided input.
        /// </summary>
        /// <param name="parameter">The input used to determine if the command can be executed.</param>
        /// <returns>Always returns true, indicating the command can be executed.</returns>
        bool ICommand.CanExecute(object? parameter)
        {
            return true;
        }

        /// <summary>
        /// Executes a command and raises an event to indicate if the command can execute. It also invokes a callback
        /// with the parameter converted to a string.
        /// </summary>
        /// <param name="parameter">An optional object that is passed to the command, which is converted to a string if it is not already.</param>
        void ICommand.Execute(object? parameter)
        {
            CanExecuteChanged?.Invoke(this!, EventArgs.Empty);

            Callback?.Invoke(parameter is string str ? str : parameter?.ToString()!);
        }
    }

    /// <summary>
    /// Represents a button with associated content, result, and an optional click action.
    /// </summary>
    /// <param name="ButtonContent">Specifies the text or label displayed on the button.</param>
    /// <param name="ButtonResult">Indicates the outcome or result that occurs when the button is clicked.</param>
    /// <param name="ClickAction">Defines an optional action to be executed when the button is clicked.</param>
    internal record ButtonItem(string ButtonContent, ButtonResult ButtonResult, Action<ButtonResult>? ClickAction = null);
}

/// <summary>
/// Builds and configures a PopupContext instance using properties like primary index, title, and button items. Supports
/// method chaining for configuration.
/// </summary>
public class PopupContextBuilder
{
    [DebuggerBrowsable(Never)]
    int primaryIndex;

    [DebuggerBrowsable(Never)]
    string title = default!;

    [DebuggerBrowsable(Never)]
    readonly List<ButtonItem> buttonItems = new List<ButtonItem>();

    /// <summary>
    /// InnerPopupConfig is a private class that inherits from PopupContext. It is likely used for configuring popup
    /// behavior within its containing class.
    /// </summary>
    private class InnerPopupConfig : PopupContext { }

    /// <summary>
    /// Creates and configures a new PopupContext instance using the current object's properties.
    /// </summary>
    /// <returns>Returns the configured PopupContext instance.</returns>
    public PopupContext Build()
    {
        var context = new InnerPopupConfig();

        context.PrimaryIndex = this.primaryIndex;

        context.Title = this.title;

        context.buttonItems.AddRange(buttonItems);

        return context;
    }

    /// <summary>
    /// Sets the primary index for the popup context builder and returns the updated instance.
    /// </summary>
    /// <param name="primaryIndex">Specifies the index to be used as the primary reference point.</param>
    /// <returns>Returns the current instance of the popup context builder for method chaining.</returns>
    public PopupContextBuilder PrimaryIndex(int primaryIndex)
    {
        this.primaryIndex = primaryIndex;
        return this;
    }

    /// <summary>
    /// <paramref name="title"/>
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public PopupContextBuilder Title(string title)
    {
        this.title = title;
        return this;
    }

    /// <summary>
    /// Registers a button item with specified context and result, optionally attaching a click action.
    /// </summary>
    /// <param name="buttonContext">Specifies the context in which the button will be used.</param>
    /// <param name="buttonResult">Defines the result that will be returned when the button is clicked.</param>
    /// <param name="click">An optional action that will be executed upon clicking the button.</param>
    /// <returns>Returns the current instance of the builder for method chaining.</returns>
    public PopupContextBuilder Register(string buttonContext, ButtonResult buttonResult, Action<ButtonResult>? click = null)
    {
        ButtonItem buttonItem = new ButtonItem(buttonContext, buttonResult, click);

        buttonItems.Add(buttonItem);

        return this;
    }
}
