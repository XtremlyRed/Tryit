namespace Tryit.Wpf.Threading;

/// <summary>
/// Represents an awaitable asynchronous operation that executes on a Dispatcher and provides a result of type
/// <typeparamref name="T"/>. Enables awaiting the completion of a dispatcher-based task and retrieving its result in a
/// thread-safe manner.
/// </summary>
/// <remarks>This class is designed for use with dispatcher-based asynchronous programming scenarios, such as in
/// UI frameworks that require code to run on a specific thread. It allows asynchronous methods to return a value that
/// can be awaited, ensuring that continuations after 'await' are scheduled on the original Dispatcher. Unlike Task<T>,
/// if the operation is not yet complete, accessing the result does not block the calling thread but instead returns the
/// default value of <typeparamref name="T"/>. Exceptions that occur during the asynchronous operation are rethrown when
/// retrieving the result. Use the static Create method to instantiate and control the completion of the
/// operation.</remarks>
/// <typeparam name="T">The type of the result produced by the asynchronous operation.</typeparam>
public class DispatcherAsyncOperation<T> : DispatcherObject, IAwaitable<DispatcherAsyncOperation<T>, T>, IAwaiter<T>
{
    /// <summary>
    /// Initializes a new instance of the DispatcherAsyncOperation class.
    /// </summary>
    private DispatcherAsyncOperation() { }

    /// <summary>
    /// Gets an awaiter used to await the completion of this asynchronous operation.
    /// </summary>
    /// <remarks>This method enables instances of DispatcherAsyncOperation<T> to be awaited using the await
    /// keyword in asynchronous methods.</remarks>
    /// <returns>An awaiter that can be used to await this operation.</returns>
    public DispatcherAsyncOperation<T> GetAwaiter()
    {
        return this;
    }

    /// <summary>
    /// Gets a value indicating whether the operation has completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Gets the result value produced by the operation.
    /// </summary>
    public T Result { get; private set; } = default!;

    /// <summary>
    /// Retrieves the result value or throws the captured exception if the operation failed.
    /// </summary>
    /// <remarks>If the operation resulted in an exception, this method will rethrow the original exception. Callers
    /// should be prepared to handle any exceptions that may have occurred during the operation.</remarks>
    /// <returns>The result value of type <typeparamref name="T"/> if the operation completed successfully.</returns>
    public T GetResult()
    {
        if (_exception != null)
        {
            ExceptionDispatchInfo.Capture(_exception).Throw();
        }
        return Result;
    }

    /// <summary>
    /// Configures the priority at which the asynchronous operation will be dispatched.
    /// </summary>
    /// <param name="priority">The dispatcher priority to assign to the operation. Determines the relative importance of the operation when
    /// scheduled.</param>
    /// <returns>The current instance with the updated priority, enabling method chaining.</returns>
    public DispatcherAsyncOperation<T> ConfigurePriority(DispatcherPriority priority)
    {
        _priority = priority;
        return this;
    }

    /// <summary>
    /// Schedules the specified action to be invoked when the awaitable operation has completed.
    /// </summary>
    /// <remarks>If the operation has already completed, the specified action is invoked immediately. If
    /// multiple awaiters register continuations before completion, all registered actions will be invoked when the
    /// operation completes. This method may be called multiple times for the same awaitable instance.</remarks>
    /// <param name="continuation">The action to execute when the operation completes. This action typically resumes execution after an await
    /// statement.</param>
    public void OnCompleted(Action continuation)
    {
        if (IsCompleted)
        {
            continuation?.Invoke();
        }
        else
        {
            _continuation += continuation;
        }
    }

    /// <summary>
    /// Marks the operation as completed and reports the specified result and exception to any awaiting continuations.
    /// </summary>
    /// <remarks>If a continuation is registered, it is invoked asynchronously on the Dispatcher associated
    /// with the operation, ensuring consistent execution context regardless of the calling thread.</remarks>
    /// <param name="result">The result value to be reported for the completed operation.</param>
    /// <param name="exception">The exception to associate with the operation if it failed; otherwise, null to indicate successful completion.</param>
    private void ReportResult(T result, Exception exception)
    {
        Result = result;
        _exception = exception;
        IsCompleted = true;

        if (_continuation != null)
        {
            Dispatcher.InvokeAsync(_continuation, _priority);
        }
    }

    /// <summary>
    /// Represents the action to execute as a continuation after a preceding operation completes.
    /// </summary>
    private Action _continuation = default!;

    /// <summary>
    ///
    /// </summary>
    private Exception _exception = default!;

    /// <summary>
    ///
    /// </summary>
    private DispatcherPriority _priority = DispatcherPriority.Normal;

    /// <summary>
    /// Creates a new DispatcherAsyncOperation<T> instance and provides a delegate for reporting the operation's result
    /// or exception.
    /// </summary>
    /// <remarks>The returned delegate must be called exactly once to complete the operation. Calling the
    /// delegate with a non-null exception indicates the operation failed; otherwise, the result value is used. This
    /// method is typically used to integrate custom asynchronous logic with the DispatcherAsyncOperation<T>
    /// pattern.</remarks>
    /// <param name="reportResult">When this method returns, contains a delegate that should be called to report the result or exception of the
    /// asynchronous operation. The first parameter is the result value; the second parameter is the exception, or null
    /// if the operation completed successfully.</param>
    /// <returns>A new DispatcherAsyncOperation<T> instance that represents the asynchronous operation.</returns>
    public static DispatcherAsyncOperation<T> Create(out Action<T, Exception> reportResult)
    {
        DispatcherAsyncOperation<T> asyncOperation = new();
        reportResult = asyncOperation.ReportResult;
        return asyncOperation;
    }
}
