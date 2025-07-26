using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tryit;

/// <summary>
/// Defines an asynchronous command interface with methods to check execution capability and execute commands. It also
/// indicates if a command is currently executing.
/// </summary>
public interface IBindingCommandAsync : ICommand
{
    /// <summary>
    /// Checks if the current command can be executed based on its state.
    /// </summary>
    /// <returns>Returns true if the command can be executed; otherwise, false.</returns>
    bool CanExecute();

    /// <summary>
    /// Executes an asynchronous operation.
    /// </summary>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    Task ExecuteAsync();

    /// <summary>
    /// Indicates whether a process is currently executing. Returns true if executing, otherwise false.
    /// </summary>
    bool IsExecuting { get; }
}

/// <summary>
/// Represents an asynchronous command that can be executed and checked for execution status. It handles exceptions
/// during execution.
/// </summary>
public class BindingCommandAsync : BindingCommandBase<object>, IBindingCommandAsync
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Func<Task> execute;

    /// <summary>
    /// Initializes a command that can be executed asynchronously. It optionally checks if the command can be executed
    /// before running it.
    /// </summary>
    /// <param name="execute">Defines the asynchronous operation to be performed when the command is executed.</param>
    /// <param name="canExecute">Specifies a condition that determines whether the command can be executed.</param>
    /// <exception cref="Exception">Thrown when the asynchronous operation to be executed is null.</exception>
    public BindingCommandAsync(Func<Task> execute, Func<bool>? canExecute = null)
        : base(canExecute is null ? null! : indexer => canExecute())
    {
        this.execute = execute ?? throw new Exception(nameof(execute));
    }

    /// <summary>
    /// Executes an asynchronous operation when called. It overrides a base method to provide specific functionality.
    /// </summary>
    /// <param name="parameter">An object that can be used to pass additional information to the execution context.</param>
    protected override async void _Execute(object parameter)
    {
        await ExecuteAsync();
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
    /// Executes an asynchronous operation while managing execution state and handling exceptions. It updates the
    /// execution status before and after the operation.
    /// </summary>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        try
        {
            IsExecuting = true;
            RaiseCanExecuteChanged();

            await execute();
        }
        catch (Exception ex)
        {
            if (BindingCommand.globalCommandExceptionCallback is null)
            {
                throw;
            }
            BindingCommand.globalCommandExceptionCallback.Invoke(ex);
        }
        finally
        {
            IsExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="commandAction"></param>
    public static implicit operator BindingCommandAsync(Func<Task> commandAction)
    {
        return new BindingCommandAsync(commandAction);
    }
}
