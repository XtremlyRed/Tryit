using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tryit;

/// <summary>
/// Creates instances of deferred behavior with specified durations for executing actions or tasks. Supports both
/// TimeSpan and milliseconds. Handles execution in current thread or deferred context.
/// </summary>
public static class Defer
{
    /// <summary>
    /// Creates a new instance of DeferredBehavior with a specified duration.
    /// </summary>
    /// <param name="timeSpan">Specifies the duration for which the behavior will be deferred.</param>
    /// <returns>Returns an instance of IDeferredBehavior configured with the given duration.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified duration is less than or equal to zero.</exception>
    public static IDeferredBehavior Deferred(TimeSpan timeSpan)
    {
        _ = (timeSpan <= TimeSpan.Zero) ? throw new ArgumentOutOfRangeException(nameof(timeSpan)) : 0;

        return new DeferredBehavior(timeSpan);
    }

    /// <summary>
    /// Creates a deferred behavior that waits for a specified duration before executing. The duration is defined in
    /// milliseconds.
    /// </summary>
    /// <param name="milliseconds">Specifies the time to wait before executing the deferred behavior.</param>
    /// <returns>Returns an instance of IDeferredBehavior configured to wait for the given duration.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified duration is less than or equal to zero.</exception>
    public static IDeferredBehavior Deferred(double milliseconds)
    {
        _ = (milliseconds <= 0) ? throw new ArgumentOutOfRangeException(nameof(milliseconds)) : 0;

        return new DeferredBehavior(TimeSpan.FromMilliseconds(milliseconds));
    }

    /// <summary>
    /// Handles deferred execution of actions and functions with a specified time delay. Supports various overloads for
    /// different parameter types.
    /// </summary>
    [DebuggerDisplay("deferred {timeSpan}")]
    class DeferredBehavior : IDeferredBehavior
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TimeSpan timeSpan;

        /// <summary>
        /// Initializes a new instance with a specified duration for deferred behavior.
        /// </summary>
        /// <param name="timeSpan">Specifies the duration for which the behavior will be deferred.</param>
        internal DeferredBehavior(TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
        }

        /// <summary>
        /// Invokes a specified action, optionally in the current thread, and returns a deferred token for the
        /// operation.
        /// </summary>
        /// <param name="action">Specifies the action to be executed.</param>
        /// <param name="invokeInCurrentThread">Determines whether the action should be executed immediately in the current thread.</param>
        /// <param name="deferName">Allows naming the deferred operation for identification purposes.</param>
        /// <returns>Returns a token representing the deferred execution of the action.</returns>
        public IDeferredToken Invoke(Action action, bool invokeInCurrentThread = false, string? deferName = null)
        {
            var behavior = new InvokeActionDeferredBehavior(action, this.timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }

        /// <summary>
        /// Invokes a specified asynchronous task and returns a token for deferred execution.
        /// </summary>
        /// <param name="taskfunc">Specifies the asynchronous function to be executed.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the task should be executed in the current thread.</param>
        /// <param name="deferName">Allows naming the deferred execution for identification purposes.</param>
        /// <returns>Returns a token that represents the deferred execution behavior.</returns>
        public IDeferredToken Invoke(Func<Task> taskfunc, bool invokeInCurrentThread = false, string? deferName = null)
        {
            var behavior = new InvokeFuncTaskDeferredBehavior(taskfunc, this.timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }

        /// <summary>
        /// Invokes an action with a specified parameter, allowing for deferred execution.
        /// </summary>
        /// <typeparam name="T">Represents the type of the parameter used in the action invocation.</typeparam>
        /// <param name="parameter">The value that will be passed to the action when it is invoked.</param>
        /// <param name="action">The operation to be executed with the provided parameter.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the action should be executed immediately in the current thread.</param>
        /// <param name="deferName">An optional name for the deferred action, useful for identification.</param>
        /// <returns>A token representing the deferred action that can be used for further control.</returns>
        public IDeferredToken Invoke<T>(T parameter, Action<T> action, bool invokeInCurrentThread = false, string? deferName = null)
        {
            var behavior = new InvokeActionWithParameterDeferredBehavior<T>(action, parameter, this.timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }

        /// <summary>
        /// Invokes a task asynchronously with a specified parameter and behavior settings.
        /// </summary>
        /// <typeparam name="T">Represents the type of the parameter used in the task function.</typeparam>
        /// <param name="parameter">The input value that will be passed to the task function during execution.</param>
        /// <param name="taskfunc">A function that takes the specified parameter and returns a task to be executed.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the task should be executed in the current thread or on a separate thread.</param>
        /// <param name="deferName">An optional name for the deferred operation, useful for identification.</param>
        /// <returns>Returns a token that represents the deferred operation for further management.</returns>
        public IDeferredToken Invoke<T>(T parameter, Func<T, Task> taskfunc, bool invokeInCurrentThread = false, string? deferName = null)
        {
            var behavior = new InvokeFuncTaskWithParameterDeferredBehavior<T>(taskfunc, parameter, this.timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }

        /// <summary>
        /// Invokes a specified action with a deferred context and returns a token for the deferred operation.
        /// </summary>
        /// <param name="action">Specifies the action to be executed within the deferred context.</param>
        /// <param name="invokeInCurrentThread">Determines whether the action should be executed immediately in the current thread.</param>
        /// <param name="deferName">Allows naming the deferred operation for identification purposes.</param>
        /// <returns>Returns a token representing the deferred action for further management.</returns>
        public IDeferredToken Invoke(Action<IDeferredContext> action, bool invokeInCurrentThread = false, string? deferName = null)
        {
            var behavior = new InvokeActionDeferredBehavior2(action, this.timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }

        /// <summary>
        /// Invokes a specified asynchronous function and returns a token for managing the deferred operation.
        /// </summary>
        /// <param name="taskfunc">Specifies the asynchronous function to be executed within the deferred context.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the function should be executed in the current thread or on a separate thread.</param>
        /// <param name="deferName">Allows naming the deferred operation for easier identification and management.</param>
        /// <returns>Returns a token that represents the deferred operation for further control.</returns>
        public IDeferredToken Invoke(Func<IDeferredContext, Task> taskfunc, bool invokeInCurrentThread = false, string? deferName = null)
        {
            var behavior = new InvokeFuncTaskDeferredBehavior2(taskfunc, this.timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }

        /// <summary>
        /// Invokes an action with a specified parameter and returns a deferred token for the operation.
        /// </summary>
        /// <typeparam name="T">Represents the type of the parameter used in the action invocation.</typeparam>
        /// <param name="parameter">The value that will be passed to the action during invocation.</param>
        /// <param name="action">The action to be executed with the provided parameter and context.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the action should be executed in the current thread.</param>
        /// <param name="deferName">An optional name for the deferred operation, useful for identification.</param>
        /// <returns>A token representing the deferred action that can be used for further operations.</returns>
        public IDeferredToken Invoke<T>(T parameter, Action<IDeferredContext, T> action, bool invokeInCurrentThread = false, string? deferName = null)
        {
            var behavior = new InvokeActionWithParameterDeferredBehavior2<T>(action, parameter, this.timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }

        /// <summary>
        /// Invokes a task with a specified parameter and returns a deferred token for the operation.
        /// </summary>
        /// <typeparam name="T">Represents the type of the parameter used in the task invocation.</typeparam>
        /// <param name="parameter">The input value that will be passed to the task function during execution.</param>
        /// <param name="taskfunc">A function that defines the task to be executed with the provided parameter.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the task should be executed in the current thread or not.</param>
        /// <param name="deferName">An optional name for the deferred operation, useful for identification.</param>
        /// <returns>A token representing the deferred operation, allowing for further interaction.</returns>
        public IDeferredToken Invoke<T>(T parameter, Func<IDeferredContext, T, Task> taskfunc, bool invokeInCurrentThread = false, string? deferName = null)
        {
            var behavior = new InvokeFuncTaskWithParameterDeferredBehavior2<T>(taskfunc, parameter, this.timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }
    }

    /// <summary>
    /// Defers the execution of a specified action for a set duration. It can execute the action on the current thread
    /// or later.
    /// </summary>
    class InvokeActionDeferredBehavior : DeferredBehaviorBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Action action;

        /// <summary>
        /// Initializes a behavior that defers the execution of a specified action for a given duration.
        /// </summary>
        /// <param name="action">Specifies the action to be executed after the delay.</param>
        /// <param name="timeSpan">Defines the duration to wait before executing the action.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the action should be executed on the current thread.</param>
        /// <param name="deferName">Provides an optional name for the deferred action for identification.</param>
        public InvokeActionDeferredBehavior(Action action, TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
            : base(timeSpan, invokeInCurrentThread, deferName)
        {
            this.action = action;
        }

        /// <summary>
        /// Executes an action if it is defined, using a context that allows for deferred execution.
        /// </summary>
        /// <param name="deferredContext">Provides the context necessary for executing the action at a later time.</param>
        protected override void Invoke(IDeferredContext deferredContext)
        {
            action?.Invoke();
        }

        /// <summary>
        /// Releases resources used by the object. Sets the action to null and calls the base class's Dispose method.
        /// </summary>
        public override void Dispose()
        {
            action = null!;
            base.Dispose();
        }
    }

    /// <summary>
    /// Handles deferred execution of an asynchronous task with configurable timing and threading options. Allows for
    /// resource cleanup and nullifies the task function.
    /// </summary>
    class InvokeFuncTaskDeferredBehavior : DeferredBehaviorBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<Task> taskfunc;

        /// <summary>
        /// Constructs an instance for deferred task execution with specified timing and invocation options.
        /// </summary>
        /// <param name="taskfunc">Defines the asynchronous operation to be executed after the specified delay.</param>
        /// <param name="timeSpan">Specifies the duration to wait before executing the task.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the task should run on the current thread or a separate one.</param>
        /// <param name="deferName">Provides an optional name for the deferred task for identification purposes.</param>
        public InvokeFuncTaskDeferredBehavior(Func<Task> taskfunc, TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
            : base(timeSpan, invokeInCurrentThread, deferName)
        {
            this.taskfunc = taskfunc;
        }

        /// <summary>
        /// Executes a task asynchronously if a function is defined.
        /// </summary>
        /// <param name="deferredContext">Provides context for deferred execution.</param>
        protected override async void Invoke(IDeferredContext deferredContext)
        {
            if (taskfunc is not null)
            {
                await taskfunc();
            }
        }

        /// <summary>
        /// Releases resources used by the object and sets the task function to null. Calls the base class's Dispose
        /// method.
        /// </summary>
        public override void Dispose()
        {
            taskfunc = null!;
            base.Dispose();
        }
    }

    /// <summary>
    /// Defers the execution of an action with a specified parameter until a defined time span elapses.
    /// </summary>
    /// <typeparam name="T">Represents the type of the input value that will be passed to the action when it is executed.</typeparam>
    class InvokeActionWithParameterDeferredBehavior<T> : DeferredBehaviorBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Action<T> action;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private T parameter;

        /// <summary>
        /// Constructs an instance that defers the execution of an action with a specified parameter until a given time
        /// span elapses.
        /// </summary>
        /// <param name="action">Defines the action to be executed with the provided parameter.</param>
        /// <param name="parameter">Specifies the input value that will be passed to the action when executed.</param>
        /// <param name="timeSpan">Indicates the duration to wait before executing the action.</param>
        /// <param name="invokeInCurrentThread">Determines whether the action should be executed on the current thread.</param>
        /// <param name="deferName">Provides an optional name for the deferred action for identification purposes.</param>
        public InvokeActionWithParameterDeferredBehavior(Action<T> action, T parameter, TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
            : base(timeSpan, invokeInCurrentThread, deferName)
        {
            this.action = action;
            this.parameter = parameter;
        }

        /// <summary>
        /// Executes an action if it is defined, using a provided context for deferred execution.
        /// </summary>
        /// <param name="deferredContext">Provides the context necessary for executing the action at a later time.</param>
        protected override void Invoke(IDeferredContext deferredContext)
        {
            if (action is not null)
                action(parameter);
        }

        /// <summary>
        /// Releases resources used by the object. Sets action to null and parameter to its default value before calling
        /// the base Dispose method.
        /// </summary>
        public override void Dispose()
        {
            action = null!;
            parameter = default!;
            base.Dispose();
        }
    }

    /// <summary>
    /// Executes an asynchronous task with a specified parameter after a delay, allowing control over timing and
    /// context.
    /// </summary>
    /// <typeparam name="T">Represents the type of the input value that will be passed to the asynchronous operation.</typeparam>
    class InvokeFuncTaskWithParameterDeferredBehavior<T> : DeferredBehaviorBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<T, Task> taskfunc;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private T parameter;

        /// <summary>
        /// Constructs an instance that executes a task with a specified parameter after a delay. It allows for control
        /// over execution timing and context.
        /// </summary>
        /// <param name="taskfunc">Defines the asynchronous operation to be executed with the provided parameter.</param>
        /// <param name="parameter">Specifies the input value that will be passed to the asynchronous operation.</param>
        /// <param name="timeSpan">Indicates the duration to wait before executing the task.</param>
        /// <param name="invokeInCurrentThread">Determines whether the task should run on the current thread or a separate one.</param>
        /// <param name="deferName">Provides an optional name for the deferred execution, useful for identification.</param>
        public InvokeFuncTaskWithParameterDeferredBehavior(Func<T, Task> taskfunc, T parameter, TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
            : base(timeSpan, invokeInCurrentThread, deferName)
        {
            this.taskfunc = taskfunc;
            this.parameter = parameter;
        }

        /// <summary>
        /// Executes an asynchronous operation if a task function is defined.
        /// </summary>
        /// <param name="deferredContext">Provides context for deferred execution of the operation.</param>
        protected override async void Invoke(IDeferredContext deferredContext)
        {
            if (taskfunc is not null)
            {
                await taskfunc(parameter);
            }
        }

        /// <summary>
        /// Releases resources used by the object. Sets taskfunc to null and parameter to default before calling the
        /// base Dispose method.
        /// </summary>
        public override void Dispose()
        {
            taskfunc = null!;
            parameter = default!;
            base.Dispose();
        }
    }

    /// <summary>
    /// Delays the execution of a specified action until certain conditions are met. It can execute the action in a
    /// specified context.
    /// </summary>
    class InvokeActionDeferredBehavior2 : DeferredBehaviorBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Action<IDeferredContext> action;

        /// <summary>
        /// Constructs an instance that defers the execution of a specified action based on a time span and execution
        /// context.
        /// </summary>
        /// <param name="action">Specifies the action to be executed when the conditions are met.</param>
        /// <param name="timeSpan">Defines the duration after which the action will be invoked.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the action should be executed on the current thread.</param>
        /// <param name="deferName">Provides an optional name for the deferred action for identification purposes.</param>
        public InvokeActionDeferredBehavior2(Action<IDeferredContext> action, TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
            : base(timeSpan, invokeInCurrentThread, deferName)
        {
            this.action = action;
        }

        /// <summary>
        /// Executes an action if it is defined, passing a context for deferred operations.
        /// </summary>
        /// <param name="deferredContext">Provides the context needed for executing the action.</param>
        protected override void Invoke(IDeferredContext deferredContext)
        {
            action?.Invoke(deferredContext);
        }

        /// <summary>
        /// Releases resources used by the object and sets the action to null. Calls the base class's Dispose method.
        /// </summary>
        public override void Dispose()
        {
            action = null!;
            base.Dispose();
        }
    }

    /// <summary>
    /// Handles deferred execution of an asynchronous function with specified timing and options. It can execute a task
    /// in a given context.
    /// </summary>
    class InvokeFuncTaskDeferredBehavior2 : DeferredBehaviorBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<IDeferredContext, Task> taskfunc;

        /// <summary>
        /// Constructs an instance for deferred task execution with specified timing and invocation options.
        /// </summary>
        /// <param name="taskfunc">Defines the asynchronous operation to be executed within the deferred context.</param>
        /// <param name="timeSpan">Specifies the duration before the task is executed.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the task should run on the current thread.</param>
        /// <param name="deferName">Provides an optional name for the deferred task for identification.</param>
        public InvokeFuncTaskDeferredBehavior2(Func<IDeferredContext, Task> taskfunc, TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
            : base(timeSpan, invokeInCurrentThread, deferName)
        {
            this.taskfunc = taskfunc;
        }

        /// <summary>
        /// Executes a function asynchronously if it is not null, using a provided context for deferred operations.
        /// </summary>
        /// <param name="deferredContext">Provides the necessary context for executing the function.</param>
        protected override async void Invoke(IDeferredContext deferredContext)
        {
            if (taskfunc is not null)
            {
                await taskfunc(deferredContext);
            }
        }

        /// <summary>
        /// Releases resources used by the object and sets the task function to null. Calls the base class's Dispose
        /// method.
        /// </summary>
        public override void Dispose()
        {
            taskfunc = null!;
            base.Dispose();
        }
    }

    /// <summary>
    /// Defers the execution of an action with a specified parameter and timing behavior.
    /// </summary>
    /// <typeparam name="T">Represents the type of the parameter that will be passed to the action when invoked.</typeparam>
    class InvokeActionWithParameterDeferredBehavior2<T> : DeferredBehaviorBase
    {
        /// <summary>
        /// Holds a private action delegate that takes an IDeferredContext and a generic type T as parameters. It is not
        /// visible in the debugger.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Action<IDeferredContext, T> action;

        /// <summary>
        /// Holds a private field of type T, which is not visible in the debugger. This is indicated by the
        /// DebuggerBrowsable attribute.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private T parameter;

        /// <summary>
        /// Constructs an instance that defers the execution of an action with a specified parameter and timing
        /// behavior.
        /// </summary>
        /// <param name="action">Defines the action to be executed with a deferred context and a specific parameter.</param>
        /// <param name="parameter">Specifies the parameter that will be passed to the action when it is invoked.</param>
        /// <param name="timeSpan">Indicates the duration to wait before executing the action.</param>
        /// <param name="invokeInCurrentThread">Determines whether the action should be executed on the current thread.</param>
        /// <param name="deferName">Provides an optional name for the deferred action for identification purposes.</param>
        public InvokeActionWithParameterDeferredBehavior2(Action<IDeferredContext, T> action, T parameter, TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
            : base(timeSpan, invokeInCurrentThread, deferName)
        {
            this.action = action;
            this.parameter = parameter;
        }

        /// <summary>
        /// Executes an action if it is defined, passing a context and a parameter to it.
        /// </summary>
        /// <param name="deferredContext">Provides the context needed for the action to execute properly.</param>
        protected override void Invoke(IDeferredContext deferredContext)
        {
            if (action is not null)
                action(deferredContext, parameter);
        }

        /// <summary>
        /// Releases resources used by the object. Sets action to null and parameter to its default value before calling
        /// the base Dispose method.
        /// </summary>
        public override void Dispose()
        {
            action = null!;
            parameter = default!;
            base.Dispose();
        }
    }

    /// <summary>
    /// Executes an asynchronous task with a specified parameter after a defined time interval, allowing control over
    /// execution context.
    /// </summary>
    /// <typeparam name="T">Represents the type of the input value that will be passed to the asynchronous operation.</typeparam>
    class InvokeFuncTaskWithParameterDeferredBehavior2<T> : DeferredBehaviorBase
    {
        /// <summary>
        /// Holds a function that takes a deferred context and a parameter of type T, returning a Task. Used for
        /// asynchronous operations.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<IDeferredContext, T, Task> taskfunc;

        /// <summary>
        /// A private field of generic type T named 'parameter'. It is not visible in the debugger due to the
        /// DebuggerBrowsable attribute.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private T parameter;

        /// <summary>
        /// Constructs an instance that executes a task with a parameter at a specified time interval. It allows for
        /// control over execution context and naming.
        /// </summary>
        /// <param name="taskfunc">Defines the asynchronous operation to be executed with the provided context and parameter.</param>
        /// <param name="parameter">Specifies the input value that will be passed to the asynchronous operation.</param>
        /// <param name="timeSpan">Indicates the duration after which the task should be executed.</param>
        /// <param name="invokeInCurrentThread">Determines whether the task should run on the current thread or a separate one.</param>
        /// <param name="deferName">Provides an optional name for the deferred execution, useful for identification.</param>
        public InvokeFuncTaskWithParameterDeferredBehavior2(Func<IDeferredContext, T, Task> taskfunc, T parameter, TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
            : base(timeSpan, invokeInCurrentThread, deferName)
        {
            this.taskfunc = taskfunc;
            this.parameter = parameter;
        }

        /// <summary>
        /// Invokes an asynchronous function if it is defined, passing a context and a parameter to it.
        /// </summary>
        /// <param name="deferredContext">Provides the context needed for the asynchronous function to execute properly.</param>
        protected override async void Invoke(IDeferredContext deferredContext)
        {
            if (taskfunc is not null)
            {
                await taskfunc(deferredContext, parameter);
            }
        }

        /// <summary>
        /// Releases resources used by the object. Sets taskfunc to null and parameter to its default value before
        /// calling the base Dispose method.
        /// </summary>
        public override void Dispose()
        {
            taskfunc = null!;
            parameter = default!;
            base.Dispose();
        }
    }

    /// <summary>
    /// Abstract class for managing deferred behavior with cancellation and synchronization. It includes methods for
    /// execution and resource management.
    /// </summary>
    [DebuggerDisplay("{DeferName,nq} deferred:{TimeSpan}")]
    abstract class DeferredBehaviorBase
    {
        /// <summary>
        /// Holds a CancellationTokenSource instance for managing cancellation tokens. It is set to never be displayed
        /// in the debugger.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Defines a TimeSpan field that is not visible in the debugger. It is used to represent a time interval.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        TimeSpan TimeSpan;

        /// <summary>
        /// Represents an optional name for deferring actions. It is initialized to null and is not visible in the
        /// debugger.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string? DeferName = null;

        /// <summary>
        /// Holds an optional SynchronizationContext instance for managing synchronization in a multi-threaded
        /// environment.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SynchronizationContext? synchronizationContext;

        /// <summary>
        /// Stores the version of behavior as a long integer. It is not visible in the debugger.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        long behaviorVersion = 0;

        /// <summary>
        /// Indicates whether the object has been disposed. Used to manage resource cleanup.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool disposedValue;

        /// <summary>
        /// Holds an optional reference to a DeferredContext object. It is not displayed in the debugger.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        DeferredContext? deferredContext;

        /// <summary>
        /// Initializes a deferred behavior with a specified time span and execution context. It sets up the
        /// synchronization context based on the execution preference.
        /// </summary>
        /// <param name="timeSpan">Specifies the duration for which the behavior will be deferred before execution.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the behavior should execute in the current thread or not.</param>
        /// <param name="deferName">Provides an optional name for the deferred behavior for identification purposes.</param>
        protected DeferredBehaviorBase(TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName)
        {
            TimeSpan = timeSpan;
            DeferName = deferName;
            synchronizationContext = invokeInCurrentThread ? SynchronizationContext.Current : null;

            Restart();
        }

        /// <summary>
        /// Releases resources used by the object. Cancels and disposes of the cancellation token source if it exists.
        /// </summary>
        public virtual void Dispose()
        {
            try
            {
                disposedValue = true;
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null!;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Restarts the behavior by creating a new cancellation token and a new map, while handling deferred context
        /// and invoking a delay.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the method is called after the object has been disposed.</exception>
        public void Restart()
        {
            _ = (disposedValue) ? throw new ObjectDisposedException("deferredBehavior") : 0;

            if (deferredContext is not null)
            {
                deferredContext.IsAbandoned = true;
            }

            try
            {
                var @new = new CancellationTokenSource();
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = @new;
            }
            catch (Exception) { }

            var newMap = new Map(this, Interlocked.Increment(ref behaviorVersion));

            ThreadPool.QueueUserWorkItem(
                static async o =>
                {
                    var newMap = (Map)o!;

                    if (newMap.Version != newMap.Behavior.behaviorVersion)
                    {
                        return;
                    }

                    if ((await newMap.Behavior.SafeDelay()))
                    {
                        DeferredContext deferredContext = newMap.Behavior.deferredContext = new DeferredContext()
                        {
                            BeginTime = DateTime.Now,
                            DeferName = newMap.Behavior.DeferName,
                            DeferTime = newMap.Behavior.TimeSpan,
                            IsAbandoned = false,
                        };

                        newMap.Behavior.InnerInvoke(deferredContext);
                    }
                },
                newMap
            );
        }

        /// <summary>
        /// Executes an action based on the provided context, either immediately or on a synchronization context.
        /// </summary>
        /// <param name="deferredContext">Holds information about the timing and thread of the current operation.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the method is called after the object has been disposed.</exception>
        private void InnerInvoke(DeferredContext deferredContext)
        {
            _ = (disposedValue) ? throw new ObjectDisposedException("deferredBehavior") : 0;

            if (synchronizationContext is null)
            {
                deferredContext.BeginTime = DateTime.Now;
                deferredContext.ThreadId = Thread.CurrentThread.ManagedThreadId;

                Invoke(deferredContext);
                return;
            }

            synchronizationContext.Post(
                o =>
                {
                    var map2 = (Map2)o!;
                    map2.DeferredContext.BeginTime = DateTime.Now;
                    map2.DeferredContext.ThreadId = Thread.CurrentThread.ManagedThreadId;
                    map2.Behavior.Invoke(map2.DeferredContext);
                },
                new Map2(this, deferredContext)
            );
        }

        /// <summary>
        /// An abstract method that must be implemented to perform an action using a deferred context.
        /// </summary>
        /// <param name="deferredContext">Provides the context needed for deferred execution.</param>
        protected abstract void Invoke(IDeferredContext deferredContext);

        /// <summary>
        /// Delays execution for a specified time unless the object is disposed. It handles exceptions and returns a
        /// boolean indicating success.
        /// </summary>
        /// <returns>Returns true if the delay completes successfully, otherwise returns false.</returns>
        private async Task<bool> SafeDelay()
        {
            try
            {
                _ = (disposedValue) ? throw new ObjectDisposedException("deferredBehavior") : 0;

                await Task.Delay(TimeSpan, cancellationTokenSource.Token).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Represents a mapping with specific behavior and a version number.
        /// </summary>
        /// <param name="Behavior">Defines the behavior of the mapping in a deferred manner.</param>
        /// <param name="Version">Indicates the version of the mapping.</param>
        record Map(DeferredBehaviorBase Behavior, long Version);

        /// <summary>
        /// Represents a mapping between a behavior and a context in a deferred manner.
        /// </summary>
        /// <param name="Behavior">Defines the behavior that will be applied in the mapping.</param>
        /// <param name="DeferredContext">Specifies the context in which the behavior will be executed.</param>
        record Map2(DeferredBehaviorBase Behavior, DeferredContext DeferredContext);
    }

    /// <summary>
    /// Represents a token for deferred operations with a specified behavior. It manages resource cleanup and can
    /// restart operations unless disposed.
    /// </summary>
    class DeferredToken : IDeferredToken
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        DeferredBehaviorBase deferredBehavior;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the DeferredToken class with a specified behavior for deferred operations.
        /// </summary>
        /// <param name="deferredBehavior">Specifies the behavior to be used for handling deferred operations.</param>
        internal DeferredToken(DeferredBehaviorBase deferredBehavior)
        {
            this.deferredBehavior = deferredBehavior;
        }

        /// <summary>
        /// Restarts the deferred behavior if the object has not been disposed. If disposed, it throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when attempting to restart after the object has been disposed.</exception>
        public void Restart()
        {
            _ = (disposedValue) ? throw new ObjectDisposedException(nameof(deferredBehavior)) : 0;

            deferredBehavior?.Restart();
        }

        /// <summary>
        /// Cleans up resources used by the object, allowing for proper memory management.
        /// </summary>
        /// <param name="disposing">Indicates whether to release both managed and unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    deferredBehavior?.Dispose();
                    deferredBehavior = null!;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases resources used by the object. It also prevents the garbage collector from calling the object's
        /// finalizer.
        /// </summary>
        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a context for deferred actions with properties for abandonment status, defer name, thread ID, start
    /// time, and defer duration.
    /// </summary>
    class DeferredContext : IDeferredContext
    {
        /// <summary>
        /// Indicates whether an item or entity is considered abandoned. A value of true means it is abandoned, while
        /// false means it is not.
        /// </summary>
        public bool IsAbandoned { get; set; }

        /// <summary>
        /// Represents the name to defer an action or process. It can be null, indicating no defer name is set.
        /// </summary>
        public string? DeferName { get; set; }

        /// <summary>
        /// Represents the unique identifier for a thread. It is an integer property that can be both retrieved and set.
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// Represents the starting time of an event or process. It is a DateTime property that can be both retrieved
        /// and set.
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// Represents the amount of time to defer an action. It is expressed as a TimeSpan value.
        /// </summary>
        public TimeSpan DeferTime { get; set; }
    }
}

/// <summary>
/// an interface of <see cref="IDeferredBehavior"/>
/// </summary>
public interface IDeferredBehavior
{
    /// <summary>
    /// Invokes a specified action, optionally in the current thread, with an optional name for deferral.
    /// </summary>
    /// <param name="action">Specifies the operation to be executed.</param>
    /// <param name="invokeInCurrentThread">Determines whether the action should be executed immediately in the current thread.</param>
    /// <param name="deferName">Provides an optional name for the deferred action for identification purposes.</param>
    /// <returns>Returns a token representing the deferred action.</returns>
    IDeferredToken Invoke(Action action, bool invokeInCurrentThread = false, string? deferName = null);

    /// <summary>
    /// Invokes a specified asynchronous task and returns a token for managing its execution.
    /// </summary>
    /// <param name="taskfunc">Specifies the asynchronous operation to be executed.</param>
    /// <param name="invokeInCurrentThread">Determines whether the task should be executed on the current thread.</param>
    /// <param name="deferName">Allows naming the deferred operation for identification purposes.</param>
    /// <returns>Returns a token that represents the deferred execution of the task.</returns>
    IDeferredToken Invoke(Func<Task> taskfunc, bool invokeInCurrentThread = false, string? deferName = null);

    /// <summary>
    /// invoke <paramref name="action"/> with <typeparamref name="T"/> <paramref name="parameter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameter"></param>
    /// <param name="invokeInCurrentThread"></param>
    /// <param name="action"></param>
    /// <param name="deferName"></param>
    /// <returns></returns>
    IDeferredToken Invoke<T>(T parameter, Action<T> action, bool invokeInCurrentThread = false, string? deferName = null);

    /// <summary>
    /// invoke <paramref name="taskfunc"/> with <typeparamref name="T"/> <paramref name="parameter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameter"></param>
    /// <param name="invokeInCurrentThread"></param>
    /// <param name="taskfunc"></param>
    /// <param name="deferName"></param>
    /// <returns></returns>
    IDeferredToken Invoke<T>(T parameter, Func<T, Task> taskfunc, bool invokeInCurrentThread = false, string? deferName = null);

    /// <summary>
    /// invoke taskfunc
    /// </summary>
    /// <param name="action"></param>
    /// <param name="invokeInCurrentThread"></param>
    /// <param name="deferName"></param>
    /// <returns></returns>
    IDeferredToken Invoke(Action<IDeferredContext> action, bool invokeInCurrentThread = false, string? deferName = null);

    /// <summary>
    /// invoke  <paramref name="taskfunc"/>
    /// </summary>
    /// <param name="taskfunc"></param>
    /// <param name="invokeInCurrentThread"></param>
    /// <param name="deferName"></param>
    /// <returns></returns>
    IDeferredToken Invoke(Func<IDeferredContext, Task> taskfunc, bool invokeInCurrentThread = false, string? deferName = null);

    /// <summary>
    /// invoke <paramref name="action"/> with <typeparamref name="T"/> <paramref name="parameter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameter"></param>
    /// <param name="invokeInCurrentThread"></param>
    /// <param name="action"></param>
    /// <param name="deferName"></param>
    /// <returns></returns>
    IDeferredToken Invoke<T>(T parameter, Action<IDeferredContext, T> action, bool invokeInCurrentThread = false, string? deferName = null);

    /// <summary>
    /// invoke <paramref name="taskfunc"/> with <typeparamref name="T"/> <paramref name="parameter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameter"></param>
    /// <param name="invokeInCurrentThread"></param>
    /// <param name="taskfunc"></param>
    /// <param name="deferName"></param>
    /// <returns></returns>
    IDeferredToken Invoke<T>(T parameter, Func<IDeferredContext, T, Task> taskfunc, bool invokeInCurrentThread = false, string? deferName = null);
}

/// <summary>
/// Represents a deferred token that can be restarted. It also implements IDisposable for resource management.
/// </summary>
public interface IDeferredToken : IDisposable
{
    /// <summary>
    /// Restarts the current process or operation. This may reset the state and reinitialize resources.
    /// </summary>
    void Restart();
}

/// <summary>
/// Interface for managing deferred contexts, indicating if an object is abandoned, its defer name, thread ID, start
/// time, and defer duration.
/// </summary>
public interface IDeferredContext
{
    /// <summary>
    /// Indicates whether an object has been abandoned. Returns true if abandoned, otherwise false.
    /// </summary>
    bool IsAbandoned { get; }

    /// <summary>
    /// Represents the name to defer. It can be null, indicating that no defer name is set.
    /// </summary>
    public string? DeferName { get; }

    /// <summary>
    /// Represents the unique identifier for a thread. It is a read-only property.
    /// </summary>
    public int ThreadId { get; }

    /// <summary>
    /// Represents the start time of an event or process. It is a read-only property of type DateTime.
    /// </summary>
    public DateTime BeginTime { get; }

    /// <summary>
    /// Represents the amount of time to defer an action. It is a read-only property of type TimeSpan.
    /// </summary>
    public TimeSpan DeferTime { get; }
}
