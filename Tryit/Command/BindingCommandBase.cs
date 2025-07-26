using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using PropertyChanged;

namespace Tryit;

/// <summary>
/// An abstract class that implements ICommand and INotifyPropertyChanged for command binding with a specified input
/// type.
/// </summary>
/// <typeparam name="T">Represents the type of input that influences the command's executability and execution.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class BindingCommandBase<T> : ICommand, INotifyPropertyChanged
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly PropertyChangedEventArgs IsExecutingProperty = new(nameof(IsExecuting));

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Func<T, bool> canExecute;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool isExecuting;

    /// <summary>
    /// Holds the current synchronization context for the thread. It is initialized to the current synchronization context
    /// at the time of declaration.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    protected SynchronizationContext SynchronizationContext = SynchronizationContext.Current!;

    /// <summary>
    ///
    /// </summary>
    /// <param name="canExecute"></param>
    protected BindingCommandBase(Func<T, bool>? canExecute)
    {
        this.canExecute = canExecute ?? (i => true);
    }

    /// <summary>
    /// Indicates whether a process is currently executing. It triggers a property change notification when the value
    /// changes.
    /// </summary>
    public bool IsExecuting
    {
        get => isExecuting;
        protected set
        {
            if (isExecuting != value)
            {
                isExecuting = value;
                PropertyChanged?.Invoke(this, IsExecutingProperty);
            }
        }
    }

    /// <summary>
    /// Raises the CanExecuteChanged event, notifying that the ability to execute a command may have changed. This allows
    /// UI elements to update their enabled state.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// An event that occurs when the ability of a command to execute changes. It allows subscribers to be notified when
    /// the command's execution status updates.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// An event that is triggered when a property value changes. It allows subscribers to be notified of property
    /// updates.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///
    /// </summary>
    /// <param name="propertyName"></param>
    protected virtual void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Determines if a command can be executed based on the provided input.
    /// </summary>
    /// <param name="parameter">An optional input that may influence the command's executability.</param>
    /// <returns>A boolean indicating whether the command can be executed.</returns>
    bool ICommand.CanExecute(object? parameter)
    {
        return _CanExecute(parameter is T target ? target : default!);
    }

    /// <summary>
    /// Executes a command with the provided parameter, converting it to a specific type if possible.
    /// </summary>
    /// <param name="parameter">The input value that will be processed by the command execution logic.</param>
    void ICommand.Execute(object? parameter)
    {
        _Execute(parameter is T target ? target : default!);
    }

    /// <summary>
    /// Determines if a command can be executed based on the current execution state and a provided parameter.
    /// </summary>
    /// <param name="parameter">The input used to evaluate whether the command can be executed.</param>
    /// <returns>Returns true if the command can be executed; otherwise, false.</returns>
    public virtual bool _CanExecute(T parameter)
    {
        if (IsExecuting)
        {
            return false;
        }
        return this.canExecute(parameter);
    }

    /// <summary>
    /// Executes an operation using the provided input.
    /// </summary>
    /// <param name="parameter">The input value required to perform the operation.</param>
    protected abstract void _Execute(T parameter);
}
