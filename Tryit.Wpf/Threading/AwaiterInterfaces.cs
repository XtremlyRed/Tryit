using System.Runtime.CompilerServices;

namespace Tryit.Wpf.Threading;

/// <summary>
/// Represents an awaitable object that provides an awaiter used to support the await pattern for asynchronous
/// operations.
/// </summary>
/// <typeparam name="TAwaiter">The type of the awaiter object returned by the awaitable. Must implement the IAwaiter interface.</typeparam>
public interface IAwaitable<out TAwaiter>
    where TAwaiter : IAwaiter
{
    /// <summary>
    /// Gets an awaiter used to await this instance.
    /// </summary>
    /// <remarks>This method enables the use of the await keyword on the containing type by returning an
    /// awaiter that implements the required pattern. Typically, this is used to support custom awaitable
    /// types.</remarks>
    /// <returns>An awaiter that can be used to await this instance.</returns>
    TAwaiter GetAwaiter();
}

/// <summary>
/// Represents an awaitable object that provides a custom awaiter and result type for use with the await keyword.
/// </summary>
/// <remarks>Implement this interface to enable custom awaitable patterns that work with the C# await keyword. The
/// GetAwaiter method is called by the compiler to retrieve the awaiter, which controls how the asynchronous operation
/// is awaited and how the result is produced.</remarks>
/// <typeparam name="TAwaiter">The type of the awaiter object returned by the GetAwaiter method. Must implement IAwaiter{TResult}.</typeparam>
/// <typeparam name="TResult">The type of the result produced when the await operation completes.</typeparam>
public interface IAwaitable<out TAwaiter, out TResult>
    where TAwaiter : IAwaiter<TResult>
{
    /// <summary>
    /// Gets an awaiter used to await this instance.
    /// </summary>
    /// <returns>An awaiter that can be used to await this instance.</returns>
    TAwaiter GetAwaiter();
}

/// <summary>
/// Represents an awaiter used to support the await pattern for asynchronous operations.
/// </summary>
/// <remarks>Implementations of this interface are used by the compiler to enable custom awaitable types. The
/// awaiter provides the logic for determining completion and retrieving the result or exception of an asynchronous
/// operation. Typically, users do not interact with this interface directly; it is intended for implementers of custom
/// awaitable types.</remarks>
public interface IAwaiter : INotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether the operation has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Performs the operation and retrieves the result. The specific result and its retrieval mechanism depend on the
    /// implementation.
    /// </summary>
    void GetResult();
}

/// <summary>
/// Defines an awaiter that supports critical notification of the completion of an asynchronous operation.
/// </summary>
/// <remarks>Implementations of this interface enable awaiting asynchronous operations with support for critical
/// notification, which is important for scenarios where execution must resume on a specific context or with guaranteed
/// reliability. This interface combines the capabilities of both IAwaiter and ICriticalNotifyCompletion.</remarks>
public interface ICriticalAwaiter : IAwaiter, ICriticalNotifyCompletion { }

/// <summary>
/// Represents an awaiter used to support the await pattern for asynchronous operations that produce a result.
/// </summary>
/// <remarks>Implementations of this interface are used by the C# compiler to enable the await keyword on custom
/// awaitable types. The awaiter provides the logic for determining completion and retrieving the result or exception of
/// the asynchronous operation. Typically, users do not interact with this interface directly; instead, it is
/// implemented by awaitable types to integrate with the language's asynchronous programming model.</remarks>
/// <typeparam name="TResult">The type of the result produced by the asynchronous operation.</typeparam>
public interface IAwaiter<out TResult> : INotifyCompletion
{
    /// <summary>
    /// Gets a value indicating whether the operation has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Retrieves the result of the operation.
    /// </summary>
    /// <returns>The result of the operation, of type <typeparamref name="TResult"/>.</returns>
    TResult GetResult();
}

/// <summary>
/// Defines an awaiter that supports critical notification of the completion of an asynchronous operation and provides a
/// result when awaited.
/// </summary>
/// <remarks>Implementations of this interface enable advanced scenarios where critical notification of completion
/// is required, such as in low-level synchronization or custom awaitable patterns. This interface extends both
/// IAwaiter{TResult} and ICriticalNotifyCompletion, allowing consumers to await operations with critical continuation
/// semantics.</remarks>
/// <typeparam name="TResult">The type of the result produced by the asynchronous operation.</typeparam>
public interface ICriticalAwaiter<out TResult> : IAwaiter<TResult>, ICriticalNotifyCompletion { }
