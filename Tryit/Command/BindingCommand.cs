using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Tryit;

/// <summary>
/// Defines a command interface with methods to check if it can execute and to execute the command. It also includes a
/// property to check if the command is currently executing.
/// </summary>
public interface IBindingCommand : ICommand
{
    /// <summary>
    /// Checks if the current command can be executed based on its state.
    /// </summary>
    /// <returns>Returns true if the command can be executed; otherwise, false.</returns>
    bool CanExecute();

    /// <summary>
    /// Executes a specific operation or task. It is typically called to perform the main functionality of a class.
    /// </summary>
    void Execute();

    /// <summary>
    /// Indicates whether a process is currently executing. Returns true if executing, otherwise false.
    /// </summary>
    bool IsExecuting { get; }
}

/// <summary>
/// Represents a command that can be executed and can determine if it can be executed. It allows for exception handling
/// during execution.
/// </summary>
public class BindingCommand : BindingCommandBase<object>, IBindingCommand, ICommand
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Action execute;

    /// <summary>
    /// Initializes a new instance of the BindingCommand class, allowing for an action to be executed conditionally.
    /// </summary>
    /// <param name="execute">Specifies the action to be performed when the command is executed.</param>
    /// <param name="canExecute">Defines a function that determines whether the command can be executed.</param>
    /// <exception cref="ArgumentNullException">Thrown when the action to be executed is null.</exception>
    public BindingCommand(Action execute, Func<bool>? canExecute = null)
        : base(canExecute is null ? null : i => canExecute.Invoke())
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    /// <summary>
    /// Checks if the current command can be executed based on its state.
    /// </summary>
    /// <returns>Returns true if the command can be executed, otherwise false.</returns>
    public bool CanExecute()
    {
        return base._CanExecute(default!);
    }

    /// <summary>
    /// Executes a command while managing its execution state and handling exceptions.
    /// </summary>
    /// <param name="parameter">An object that can be used to pass additional data or context during the command execution.</param>
    protected override void _Execute(object parameter)
    {
        try
        {
            base.IsExecuting = true;

            RaiseCanExecuteChanged();

            execute.Invoke();
        }
        catch (Exception ex)
        {
            if (globalCommandExceptionCallback is null)
            {
                throw;
            }
            globalCommandExceptionCallback.Invoke(ex);
        }
        finally
        {
            IsExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Executes a process by calling the internal _Execute method with a default parameter. It ensures the process runs
    /// without explicit arguments.
    /// </summary>
    public void Execute()
    {
        _Execute(default!);
    }

    /// <summary>
    /// Holds a callback action for handling exceptions in global commands. It is marked as internal and not visible in
    /// the debugger or editor.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static Action<Exception>? globalCommandExceptionCallback;

    /// <summary>
    /// Sets a callback function to handle exceptions that occur during command execution globally.
    /// </summary>
    /// <param name="globalCommandExceptionCallback">This parameter allows the registration of a function that processes exceptions.</param>
    public static void SetGlobalCommandExceptionCallback(Action<Exception> globalCommandExceptionCallback)
    {
        BindingCommand.globalCommandExceptionCallback = globalCommandExceptionCallback;
    }

    /// <summary>
    /// Converts an Action delegate into a BindingCommand instance.
    /// </summary>
    /// <param name="commandAction">Represents the method to be executed when the command is invoked.</param>
    public static implicit operator BindingCommand(Action commandAction)
    {
        return new BindingCommand(commandAction);
    }
}
