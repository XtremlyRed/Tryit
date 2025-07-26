using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows.Input;

namespace Tryit;

/// <summary>
/// Defines a command interface that can execute and check if it can execute based on a parameter.
/// </summary>
/// <typeparam name="TParameter">Specifies the type of parameter used to determine command execution eligibility and to execute the command.</typeparam>
public interface IBindingCommand<in TParameter> : ICommand
{
    /// <summary>
    /// Determines if a command can be executed based on the provided input.
    /// </summary>
    /// <param name="parameter">The input used to evaluate the command's executability.</param>
    /// <returns>Returns true if the command can be executed; otherwise, false.</returns>
    bool CanExecute(TParameter parameter);

    /// <summary>
    /// Executes a process using the provided input.
    /// </summary>
    /// <param name="parameter">The input used to carry out the execution of the process.</param>
    void Execute(TParameter parameter);

    /// <summary>
    /// Indicates whether a process is currently executing. Returns true if executing, otherwise false.
    /// </summary>
    bool IsExecuting { get; }
}

/// <summary>
/// Represents a command that can be executed with a parameter of a specified type. It supports execution and validation
/// of the command's ability to execute.
/// </summary>
/// <typeparam name="TParameter">Specifies the type of the parameter that the command will accept when executed.</typeparam>
public class BindingCommand<TParameter> : BindingCommandBase<TParameter>, ICommand, IBindingCommand<TParameter>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Action<TParameter> execute;

    /// <summary>
    /// Initializes a new instance of the BindingCommand class, allowing execution of a specified action with an optional
    /// condition.
    /// </summary>
    /// <param name="execute">Specifies the action to be performed when the command is executed.</param>
    /// <param name="canExecute">Defines a condition that determines whether the command can be executed.</param>
    /// <exception cref="ArgumentNullException">Thrown when the action to be executed is null.</exception>
    public BindingCommand(Action<TParameter> execute, Func<TParameter, bool>? canExecute = null)
        : base(canExecute)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    /// <summary>
    /// Executes a command with the provided parameter while handling exceptions. It manages the execution state and
    /// updates command availability.
    /// </summary>
    /// <param name="parameter">The input used to perform the command's action.</param>
    protected override void _Execute(TParameter parameter)
    {
        try
        {
            IsExecuting = true;
            RaiseCanExecuteChanged();
            execute.Invoke(parameter);
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
    /// Executes a process using the provided parameter.
    /// </summary>
    /// <param name="parameter">The input used to carry out the execution of the process.</param>
    public void Execute(TParameter parameter)
    {
        _Execute(parameter);
    }

    /// <summary>
    /// Determines if a command can be executed based on the provided input.
    /// </summary>
    /// <param name="parameter">The input used to evaluate the command's executability.</param>
    /// <returns>A boolean indicating whether the command can be executed.</returns>
    public bool CanExecute(TParameter parameter)
    {
        return base._CanExecute(parameter);
    }

    /// <summary>
    /// Converts an action that takes a parameter into a BindingCommand that can be used for binding in a UI context.
    /// </summary>
    /// <param name="commandAction">An action that defines the logic to execute when the command is invoked.</param>
    public static implicit operator BindingCommand<TParameter>(Action<TParameter> commandAction)
    {
        return new BindingCommand<TParameter>(commandAction);
    }
}
