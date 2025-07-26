using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Tryit;

/// <summary>
/// An interface for asynchronous command binding that allows execution based on input parameters. It provides methods
/// to check executability and execute operations asynchronously.
/// </summary>
/// <typeparam name="TParameter">The input type used to determine if the command can be executed and to perform the asynchronous operation.</typeparam>
public interface IBindingCommandAsync<in TParameter> : ICommand
{
    /// <summary>
    /// Determines if a command can be executed based on the provided input.
    /// </summary>
    /// <param name="parameter">The input used to evaluate the command's executability.</param>
    /// <returns>Returns true if the command can be executed; otherwise, false.</returns>
    bool CanExecute(TParameter parameter);

    /// <summary>
    /// Executes an asynchronous operation using the provided input.
    /// </summary>
    /// <param name="parameter">The input required to perform the operation.</param>
    /// <returns>A task representing the asynchronous operation's completion.</returns>

    Task ExecuteAsync(TParameter parameter);

    /// <summary>
    /// Indicates whether a process is currently executing. Returns true if executing, otherwise false.
    /// </summary>
    bool IsExecuting { get; }
}

/// <summary>
/// Represents an asynchronous command that can be executed with a parameter, allowing for conditional execution.
/// </summary>
/// <typeparam name="TParameter">The type of the parameter used when executing the asynchronous command.</typeparam>
public class BindingCommandAsync<TParameter> : BindingCommandBase<TParameter>, IBindingCommandAsync<TParameter>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Func<TParameter, Task> execute;

    /// <summary>
    /// Initializes a new instance of the BindingCommandAsync class with specified execution and conditional execution
    /// logic.
    /// </summary>
    /// <param name="execute">Defines the action to be performed asynchronously when the command is invoked.</param>
    /// <param name="canExecute">Specifies a function that determines whether the command can be executed based on certain conditions.</param>
    /// <exception cref="ArgumentNullException">Thrown when the action to be performed is not provided.</exception>
    public BindingCommandAsync(Func<TParameter, Task> execute, Func<TParameter, bool>? canExecute = null)
        : base(canExecute)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    /// <summary>
    /// Executes an asynchronous operation using the provided parameter. It overrides a base class method to implement
    /// specific functionality.
    /// </summary>
    /// <param name="parameter">The input used to perform the asynchronous operation.</param>
    protected override async void _Execute(TParameter parameter)
    {
        await ExecuteAsync(parameter);
    }

    /// <summary>
    /// Determines if a command can be executed based on the provided input.
    /// </summary>
    /// <param name="parameter">The input used to evaluate the command's executability.</param>
    /// <returns>A boolean indicating whether the command can be executed.</returns>
    public bool CanExecute(TParameter parameter)
    {
        return _CanExecute(parameter);
    }

    /// <summary>
    /// Executes an asynchronous operation with the provided parameter while managing execution state and error handling.
    /// </summary>
    /// <param name="parameter">The input used to perform the asynchronous operation.</param>
    /// <returns>This method does not return a value.</returns>
    public async Task ExecuteAsync(TParameter parameter)
    {
        try
        {
            IsExecuting = true;

            RaiseCanExecuteChanged();

            await execute(parameter);
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
    /// Converts a function that takes a parameter and returns a task into a BindingCommandAsync instance.
    /// </summary>
    /// <param name="commandAction">The function that will be executed asynchronously with a specified parameter.</param>
    public static implicit operator BindingCommandAsync<TParameter>(Func<TParameter, Task> commandAction)
    {
        return new BindingCommandAsync<TParameter>(commandAction);
    }
}
