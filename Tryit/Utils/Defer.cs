using System.Diagnostics;
using System.Runtime.CompilerServices;

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
    private class DeferredBehavior : IDeferredBehavior
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TimeSpan timeSpan;

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
            InvokeActionDeferredBehavior behavior = new InvokeActionDeferredBehavior(action, timeSpan, invokeInCurrentThread, deferName);

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
            InvokeFuncTaskDeferredBehavior behavior = new InvokeFuncTaskDeferredBehavior(taskfunc, timeSpan, invokeInCurrentThread, deferName);

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
            InvokeActionWithParameterDeferredBehavior<T> behavior = new InvokeActionWithParameterDeferredBehavior<T>(action, parameter, timeSpan, invokeInCurrentThread, deferName);

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
            InvokeFuncTaskWithParameterDeferredBehavior<T> behavior = new InvokeFuncTaskWithParameterDeferredBehavior<T>(taskfunc, parameter, timeSpan, invokeInCurrentThread, deferName);

            return new DeferredToken(behavior);
        }
    }

    /// <summary>
    /// Defers the execution of a specified action for a set duration. It can execute the action on the current thread
    /// or later.
    /// </summary>
    private class InvokeActionDeferredBehavior : DeferredBehaviorBase
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
        protected override
#if NETSTANDARD2_0 || NETCOREAPP3_1
        async
#endif
        ValueTask InvokeAsync()
        {
            action?.Invoke();

#if NETSTANDARD2_0 || NETCOREAPP3_1
            await Task.CompletedTask;
#else
            return ValueTask.CompletedTask;
#endif
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
    private class InvokeFuncTaskDeferredBehavior : DeferredBehaviorBase
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
        protected override async ValueTask InvokeAsync()
        {
            if (taskfunc is not null)
            {
                await taskfunc().ConfigureAwait(false);
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
    private class InvokeActionWithParameterDeferredBehavior<T> : DeferredBehaviorBase
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
        protected override
#if NETSTANDARD2_0 || NETCOREAPP3_1
        async
#endif
        ValueTask InvokeAsync()
        {
            if (action is not null)
            {
                action(parameter);
            }

#if NETSTANDARD2_0 || NETCOREAPP3_1
            await Task.CompletedTask;
#else
            return ValueTask.CompletedTask;
#endif
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
    private class InvokeFuncTaskWithParameterDeferredBehavior<T> : DeferredBehaviorBase
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
        protected override async ValueTask InvokeAsync()
        {
            if (taskfunc is not null)
            {
                await taskfunc(parameter).ConfigureAwait(false);
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
    /// Abstract class for managing deferred behavior with cancellation and synchronization. It includes methods for
    /// execution and resource management.
    /// </summary>
    [DebuggerDisplay("{DeferName,nq} deferred:{TimeSpan}")]
    private abstract class DeferredBehaviorBase
    {
        /// <summary>
        /// Stores the version of behavior as a long integer. It is not visible in the debugger.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private long behaviorVersion = 0;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private InvokeProxy? invokeProxy;

        /// <summary>
        /// Initializes a deferred behavior with a specified time span and execution context. It sets up the
        /// synchronization context based on the execution preference.
        /// </summary>
        /// <param name="timeSpan">Specifies the duration for which the behavior will be deferred before execution.</param>
        /// <param name="invokeInCurrentThread">Indicates whether the behavior should execute in the current thread or not.</param>
        /// <param name="deferName">Provides an optional name for the deferred behavior for identification purposes.</param>
        protected DeferredBehaviorBase(TimeSpan timeSpan, bool invokeInCurrentThread, string? deferName = null)
        {
            Restart(timeSpan, invokeInCurrentThread ? SynchronizationContext.Current : null);
        }

        /// <summary>
        /// Releases resources used by the object. Cancels and disposes of the cancellation token source if it exists.
        /// </summary>
        public virtual void Dispose()
        {
            behaviorVersion |= (1 << 31);

            (invokeProxy as IDisposable)?.Dispose();
            invokeProxy = null!;
        }

        /// <summary>
        /// Restarts the behavior by creating a new cancellation token and a new map, while handling deferred context
        /// and invoking a delay.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the method is called after the object has been disposed.</exception>
        public Task Restart(TimeSpan? timeSpan = null, SynchronizationContext? synchronizationContext = null)
        {
            if ((behaviorVersion & (1 << 31)) > 0)
            {
                throw new InvalidOperationException("disabled");
            }

            invokeProxy = new InvokeProxy(Interlocked.Increment(ref behaviorVersion))
            {
                Behavior = this, //
                timeSpan = timeSpan ?? invokeProxy?.timeSpan ?? TimeSpan.Zero,
                SynchronizationContext = synchronizationContext ?? invokeProxy?.SynchronizationContext ?? null,
            };

            ThreadPool.QueueUserWorkItem(static async o => await ((InvokeProxy)o!).InnerInvokeAsync().ConfigureAwait(false), invokeProxy);

            return invokeProxy.taskCompletionSource.Task;
        }

        protected abstract ValueTask InvokeAsync();

        /// <summary>
        /// Delays execution for a specified time unless the object is disposed. It handles exceptions and returns a
        /// boolean indicating success.
        /// </summary>
        /// <returns>Returns true if the delay completes successfully, otherwise returns false.</returns>
        private sealed class InvokeProxy(long Version) : CancellationTokenSource
        {
            internal TimeSpan timeSpan;

            internal DeferredBehaviorBase Behavior = default!;

            internal SynchronizationContext? SynchronizationContext;

            internal TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                taskCompletionSource = null!;

                SynchronizationContext = null!;

                Behavior = null!;
            }

            public async ValueTask InnerInvokeAsync()
            {
                if (await TaskEx.SafeDelayAsync(timeSpan, this.Token).ConfigureAwait(false) == false)
                {
                    return;
                }

                if (Version != Behavior.behaviorVersion)
                {
                    return;
                }

                if (SynchronizationContext is null)
                {
                    await (this!).InvokeAsync().ConfigureAwait(false);
                }
                else
                {
                    SynchronizationContext.Post(async o => await ((InvokeProxy)o!).InvokeAsync().ConfigureAwait(false), this);
                }

                await taskCompletionSource;
            }

            private async ValueTask InvokeAsync()
            {
                try
                {
                    await Behavior.InvokeAsync().ConfigureAwait(false);

                    taskCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            }
        }
    }

    /// <summary>
    /// Represents a token for deferred operations with a specified behavior. It manages resource cleanup and can
    /// restart operations unless disposed.
    /// </summary>
    private class DeferredToken : IDeferredToken
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Task? runTask;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DeferredBehaviorBase deferredBehavior;

        /// <summary>
        /// Initializes a new instance of the DeferredToken class with a specified behavior for deferred operations.
        /// </summary>
        /// <param name="deferredBehavior">Specifies the behavior to be used for handling deferred operations.</param>
        internal DeferredToken(DeferredBehaviorBase deferredBehavior)
        {
            this.deferredBehavior = deferredBehavior;
        }

        public TaskAwaiter GetAwaiter()
        {
            return runTask?.GetAwaiter() ?? Task.CompletedTask.GetAwaiter();
        }

        /// <summary>
        /// Restarts the deferred behavior if the object has not been disposed. If disposed, it throws an
        /// ObjectDisposedException.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when attempting to restart after the object has been disposed.</exception>
        public void Restart()
        {
            runTask = deferredBehavior?.Restart();
        }

        /// <summary>
        /// Cleans up resources used by the object, allowing for proper memory management.
        /// </summary>
        /// <param name="disposing">Indicates whether to release both managed and unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                deferredBehavior?.Dispose();
                deferredBehavior = null!;
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

    /// <summary>
    /// Gets an awaiter used to await this task.
    /// </summary>
    /// <remarks>This method enables instances of this type to be awaited using the await keyword in
    /// asynchronous programming. It is intended for compiler use and should not be called directly in most application
    /// code.</remarks>
    /// <returns>A TaskAwaiter instance that can be used to await this task.</returns>
    TaskAwaiter GetAwaiter();
}
