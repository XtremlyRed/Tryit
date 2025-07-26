using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tryit;

/// <summary>
/// Manages resource allocation using a semaphore, allowing for synchronous and asynchronous waits and releases. It also
/// implements IDisposable for resource cleanup.
/// </summary>
[DebuggerDisplay("[ count : {currentCounter} ]  [ max : {maxCount} ]")]
public class Awaiter : IDisposable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SemaphoreSlim semaphoreSlim;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int currentCounter;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool isDisposabled;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int maxCount;

    /// <summary>
    /// Returns the value of the current counter. It provides a read-only property for accessing the counter's value.
    /// </summary>
    public int Counter => currentCounter;

    /// <summary>
    /// Initializes an Awaiter with specified initial and maximum counts for resource management.
    /// </summary>
    /// <param name="initialCount">Specifies the starting number of resources available for use.</param>
    /// <param name="maxCount">Defines the upper limit on the number of resources that can be allocated.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the starting number of resources is negative or exceeds the maximum limit.</exception>
    public Awaiter(int initialCount, int maxCount = int.MaxValue)
    {
        _ = (initialCount < 0 || initialCount > maxCount) ? throw new ArgumentOutOfRangeException(nameof(initialCount)) : 0;

        semaphoreSlim = new(initialCount, maxCount);
        currentCounter = initialCount;
        this.maxCount = maxCount;
    }

    /// <summary>
    /// Releases a semaphore, incrementing the current counter if it is below the maximum count. Throws an exception if
    /// the object is disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to release the semaphore after the object has been disposed.</exception>
    public void Release()
    {
        _ = isDisposabled ? throw new ObjectDisposedException(nameof(Awaiter)) : 0;

        if (currentCounter < maxCount)
        {
            Interlocked.Increment(ref currentCounter);
            semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// Releases all permits of a semaphore up to a maximum count. It increments the current counter and releases the
    /// semaphore accordingly.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to release permits after the semaphore has been disposed.</exception>
    public void ReleaseAll()
    {
        _ = isDisposabled ? throw new ObjectDisposedException(nameof(Awaiter)) : 0;

        while (currentCounter < maxCount)
        {
            Interlocked.Increment(ref currentCounter);
            semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// Waits for the semaphore to be released, decrementing the current counter beforehand. If the object is disposed,
    /// an exception is thrown.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to wait on a disposed object.</exception>
    public void Wait()
    {
        _ = isDisposabled ? throw new ObjectDisposedException(nameof(Awaiter)) : 0;

        Interlocked.Decrement(ref currentCounter);
        semaphoreSlim.Wait();
    }

    /// <summary>
    /// Waits asynchronously until the semaphore is available, decrementing the current counter beforehand.
    /// </summary>
    /// <returns>Returns a Task that represents the asynchronous wait operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the method is called on a disposed Awaiter instance.</exception>
    public async Task WaitAsync()
    {
        _ = isDisposabled ? throw new ObjectDisposedException(nameof(Awaiter)) : 0;

        Interlocked.Decrement(ref currentCounter);
        await semaphoreSlim.WaitAsync();
    }

    /// <summary>
    /// Waits for a semaphore to become available or until a specified timeout occurs. It can also be canceled using a
    /// token.
    /// </summary>
    /// <param name="millisecondsTimeout">Specifies the maximum time to wait for the semaphore to become available.</param>
    /// <param name="cancellationToken">Allows the wait operation to be canceled before the timeout expires.</param>
    /// <returns>Returns true if the semaphore was entered; otherwise, false.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the semaphore has been disposed before the wait operation.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified timeout is less than or equal to zero.</exception>
    public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken = default)
    {
        _ = isDisposabled ? throw new ObjectDisposedException(nameof(Awaiter)) : 0;
        _ = millisecondsTimeout <= 0 ? throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout)) : 0;
        try
        {
            Interlocked.Decrement(ref currentCounter);
            var result = semaphoreSlim.Wait(millisecondsTimeout, cancellationToken);
            if (result == false)
            {
                Interlocked.Increment(ref currentCounter);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref currentCounter);
            return false;
        }
    }

    /// <summary>
    /// Waits asynchronously for a semaphore to be entered, with a specified timeout and optional cancellation support.
    /// </summary>
    /// <param name="millisecondsTimeout">Specifies the maximum time to wait for the semaphore to become available.</param>
    /// <param name="cancellationToken">Allows the wait operation to be canceled before the timeout expires.</param>
    /// <returns>Returns true if the semaphore was entered; otherwise, false.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the operation is attempted on a disposed object.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified timeout is less than or equal to zero.</exception>
    public async Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken = default)
    {
        _ = isDisposabled ? throw new ObjectDisposedException(nameof(Awaiter)) : 0;
        _ = millisecondsTimeout <= 0 ? throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout)) : 0;
        try
        {
            Interlocked.Decrement(ref currentCounter);
            var result = await semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken);
            if (result == false)
            {
                Interlocked.Increment(ref currentCounter);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref currentCounter);
            return false;
        }
    }

    /// <summary>
    /// Waits for a semaphore to be released, allowing for cancellation through a token. It decrements a counter and
    /// handles cancellation gracefully.
    /// </summary>
    /// <param name="cancellationToken">Allows the wait operation to be canceled if needed.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the method is called on a disposed instance of the Awaiter.</exception>
    public void Wait(CancellationToken cancellationToken)
    {
        _ = isDisposabled ? throw new ObjectDisposedException(nameof(Awaiter)) : 0;

        try
        {
            Interlocked.Decrement(ref currentCounter);

            semaphoreSlim.Wait(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref currentCounter);
        }
    }

    /// <summary>
    /// Waits asynchronously for a semaphore to become available, decrementing a counter in the process. If canceled, it
    /// increments the counter back.
    /// </summary>
    /// <param name="cancellationToken">Allows the operation to be canceled before completion.</param>
    /// <returns>This method returns a Task representing the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the method is called on a disposed instance of the Awaiter class.</exception>
    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        _ = isDisposabled ? throw new ObjectDisposedException(nameof(Awaiter)) : 0;

        try
        {
            Interlocked.Decrement(ref currentCounter);

            await semaphoreSlim.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref currentCounter);
        }
    }

    /// <summary>
    /// Releases resources used by the object and prevents further use. It resets internal state variables and disposes
    /// of the semaphore.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Dispose()
    {
        if (isDisposabled == false)
        {
            isDisposabled = true;

            if (currentCounter != 0)
            {
                semaphoreSlim?.Release(currentCounter);
            }

            semaphoreSlim?.Dispose();
            semaphoreSlim = null!;
            currentCounter = 0;
            maxCount = 0;
        }
    }
}

/// <summary>
/// Provides extension methods for executing actions and functions with exclusive access using an asynchronous locking
/// mechanism. Supports both synchronous and asynchronous operations.
/// </summary>
public static class AsyncLockerExtensions
{
    /// <summary>
    /// Executes a specified action while ensuring exclusive access through a locking mechanism. It waits for the lock
    /// to be available before execution.
    /// </summary>
    /// <param name="asyncLocker">Used to manage the locking mechanism, ensuring that the action is executed only when the lock is acquired.</param>
    /// <param name="action">Represents the code to be executed once the lock is successfully acquired.</param>
    public static void LockInvoke(this Awaiter asyncLocker, Action action)
    {
        asyncLocker.Wait();

        try
        {
            action();
        }
        finally
        {
            asyncLocker.Release();
        }
    }

    /// <summary>
    /// Executes a function while ensuring exclusive access through an asynchronous locker.
    /// </summary>
    /// <typeparam name="T">Represents the return type of the function being executed.</typeparam>
    /// <param name="asyncLocker">Used to manage access to a resource in an asynchronous context.</param>
    /// <param name="func">The function to be executed while the resource is locked.</param>
    /// <returns>The result of the executed function.</returns>
    public static T LockInvoke<T>(this Awaiter asyncLocker, Func<T> func)
    {
        asyncLocker.Wait();

        try
        {
            return func();
        }
        finally
        {
            asyncLocker.Release();
        }
    }

    /// <summary>
    /// Executes a given asynchronous function while ensuring exclusive access through a locking mechanism.
    /// </summary>
    /// <param name="asyncLocker">Used to manage the asynchronous locking and unlocking process.</param>
    /// <param name="func">Represents the asynchronous function to be executed under the lock.</param>
    /// <returns>Completes when the function execution is finished, ensuring proper resource management.</returns>
    public static async Task LockInvokeAsync(this Awaiter asyncLocker, Func<Task> func)
    {
        await asyncLocker.WaitAsync();

        try
        {
            await func();
        }
        finally
        {
            asyncLocker.Release();
        }
    }

    /// <summary>
    /// Executes a function asynchronously while ensuring exclusive access through a locking mechanism.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned by the asynchronous function being executed.</typeparam>
    /// <param name="asyncLocker">Used to manage the locking mechanism to ensure that the function is executed without concurrent access.</param>
    /// <param name="func">The asynchronous function to be executed under the lock, which returns a result of the specified type.</param>
    /// <returns>The result of the asynchronous function after it has been executed.</returns>
    public static async Task<T> LockInvokeAsync<T>(this Awaiter asyncLocker, Func<Task<T>> func)
    {
        await asyncLocker.WaitAsync();

        try
        {
            return await func();
        }
        finally
        {
            asyncLocker.Release();
        }
    }
}
