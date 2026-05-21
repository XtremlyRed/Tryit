using System.Diagnostics;
using System.Text;

namespace Tryit;

/// <summary>
/// Provides convenience factory methods for creating <see cref="SimpleObjectPool{T}"/> instances.
/// </summary>
/// <remarks>
/// This non-generic facade is useful when type inference is preferred at call sites,
/// and it also exposes a shared pool for <see cref="StringBuilder"/>.
/// </remarks>
public static class SimpleObjectPool
{
    /// <summary>
    /// Shared <see cref="StringBuilder"/> pool intended for high-frequency temporary string composition.
    /// </summary>
    /// <remarks>
    /// The reset callback clears the builder before it is re-enqueued.
    /// The default max pool size is configured to avoid unbounded growth.
    /// </remarks>
    private static readonly SimpleObjectPool<StringBuilder> stringBuilderPool = SimpleObjectPool<StringBuilder>.Create(() => new StringBuilder(), sb => sb.Clear(), ushort.MaxValue);

    /// <summary>
    /// Creates a new <see cref="SimpleObjectPool{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of objects to pool.</typeparam>
    /// <param name="factory">Factory function used to create new instances when the pool is empty.</param>
    /// <param name="resetCallback">Optional callback executed when an object is returned to the pool, allowing for state reset.</param>
    /// <param name="maxPoolSize">
    /// Maximum number of cached objects allowed in the pool queue.
    /// Values less than zero are not supported.
    /// </param>
    /// <returns>A new instance of <see cref="SimpleObjectPool{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is <see langword="null"/>.</exception>
    public static SimpleObjectPool<T> Create<T>(Func<T> factory, Action<T>? resetCallback = null, int maxPoolSize = ushort.MaxValue)
    {
        return SimpleObjectPool<T>.Create(factory, resetCallback, maxPoolSize);
    }

    /// <summary>
    /// Creates a new <see cref="SimpleObjectPool{T}"/> using <c>new T()</c> as the factory.
    /// </summary>
    /// <typeparam name="T">The pooled type that must expose a public parameterless constructor.</typeparam>
    /// <param name="resetCallback">Optional callback used to reset state when an instance is returned.</param>
    /// <param name="maxPoolSize">Maximum number of objects allowed to remain cached in the internal queue.</param>
    /// <returns>A new instance of <see cref="SimpleObjectPool{T}"/>.</returns>
    public static SimpleObjectPool<T> Create<T>(Action<T>? resetCallback = null, int maxPoolSize = ushort.MaxValue)
        where T : new()
    {
        return SimpleObjectPool<T>.Create(() => new T(), resetCallback, maxPoolSize);
    }

    /// <summary>
    /// Gets the shared <see cref="StringBuilder"/> object pool.
    /// </summary>
    /// <returns>The singleton <see cref="StringBuilder"/> pool instance.</returns>
    public static SimpleObjectPool<StringBuilder> StringBuilderPool()
    {
        return stringBuilderPool;
    }
}

/// <summary>
/// Provides a minimal object pool abstraction for reusing instances of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">
/// The pooled object type.
/// </typeparam>
/// <remarks>
/// This pool exposes two core operations:
/// <list type="bullet">
/// <item><description><see cref="Rent"/>: obtains an instance from the pool, or creates one when the pool is empty.</description></item>
/// <item><description><see cref="SimpleObjectPool{T}.Return(T)"/>: returns an instance back to the pool for future reuse.</description></item>
/// </list>
/// Use <see cref="Create"/> to build a default queue-based implementation.
/// </remarks>
public abstract class SimpleObjectPool<T>
{
    /// <summary>
    /// Creates a default <see cref="SimpleObjectPool{T}"/> implementation.
    /// </summary>
    /// <param name="factory">
    /// Factory function used to create a new object when the pool does not currently contain one.
    /// </param>
    /// <param name="resetCallback">
    /// Optional callback executed when an object is returned to the pool.
    /// This can be used to reset object state before reuse.
    /// </param>
    /// <param name="maxPoolSize">
    /// Maximum number of items that may be retained in the internal queue.
    /// When the limit is exceeded, subsequent returns may trigger an exception depending on implementation policy.
    /// </param>
    /// <returns>
    /// A queue-based object pool instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public static SimpleObjectPool<T> Create(Func<T> factory, Action<T>? resetCallback = null, int maxPoolSize = ushort.MaxValue)
    {
        _ = factory ?? throw new ArgumentNullException(nameof(factory));
        return new DefaultSimpleObjectPool(factory, resetCallback, maxPoolSize);
    }

    /// <summary>
    /// Default concrete implementation backed by a <see cref="Queue{T}"/>.
    /// </summary>
    /// <remarks>
    /// Synchronization is implemented with a lightweight spin-based critical section.
    /// The lock is held only while accessing the queue.
    /// Potentially expensive user code (<c>factory</c> and <c>resetCallback</c>) is executed outside the lock.
    /// </remarks>
    private class DefaultSimpleObjectPool : SimpleObjectPool<T>
    {
        /// <summary>
        /// Creates new objects when the pool is empty.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Func<T> factory;

        /// <summary>
        /// Optional state reset callback executed before an object is re-enqueued.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Action<T>? resetCallback;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int maxPoolSize;

        /// <summary>
        /// LIFO storage for returned objects.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Stack<T> stack = new Stack<T>();

        /// <summary>
        /// Spin-lock flag: 0 means unlocked, 1 means locked.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int flag = 0;

        /// <summary>
        /// Initializes the default pool implementation.
        /// </summary>
        /// <param name="factory">Factory function used to create objects on cache miss.</param>
        /// <param name="resetCallback">Optional callback used to reset returned instances.</param>
        /// <param name="maxPoolSize">Maximum number of cached instances allowed in <see cref="stack"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is <see langword="null"/>.</exception>
        internal DefaultSimpleObjectPool(Func<T> factory, Action<T>? resetCallback = null, int maxPoolSize = ushort.MaxValue)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.resetCallback = resetCallback;
            this.maxPoolSize = maxPoolSize;
        }

        /// <summary>
        /// Gets an object from the pool, or creates a new instance when none are available.
        /// </summary>
        /// <returns>A pooled object or a newly created object.</returns>
        /// <remarks>
        /// Queue access is synchronized.
        /// Object creation via <c>factory</c> occurs after lock release to reduce contention.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the internal queue has exceeded <see cref="maxPoolSize"/>.
        /// </exception>
        public override T Rent()
        {
            T? item = default;

            bool found = false;

            if (stack.Count > 0)
            {
                try
                {
                    SpinWait spinWait = default;

                    // Acquire exclusive access to the queue.
                    while (Interlocked.CompareExchange(ref flag, 1, 0) != 0)
                    {
                        // Perform adaptive spinning while waiting for the lock.
                        spinWait.SpinOnce();
                    }

                    // Check if the pool has exceeded its maximum size before allowing more items to be added.
                    if (stack.Count > maxPoolSize)
                    {
                        throw new InvalidOperationException($"The pool has exceeded its maximum size of {maxPoolSize} items.");
                    }

                    if (stack.Count > 0)
                    {
                        // Fast path: reuse an existing instance from the pool.
                        item = stack.Pop();
                        found = true;
                    }
                }
                finally
                {
                    // Always release the lock, even if dequeue throws unexpectedly.

                    Interlocked.CompareExchange(ref flag, 0, 1);
                }

                if (found)
                {
                    return item!;
                }
            }

            // Slow path: create a new instance outside the lock.
            return factory();
        }

        /// <summary>
        /// Returns an object to the pool for later reuse.
        /// </summary>
        /// <param name="item">The object instance to return.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The optional reset callback runs before queue insertion so the object is normalized for the next renter.
        /// </remarks>
        public override void Return(T item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            // Execute caller-provided reset logic before storing the object.
            resetCallback?.Invoke(item);

            try
            {
                SpinWait spinWait = default;

                // Acquire exclusive access to the queue.
                while (Interlocked.CompareExchange(ref flag, 1, 0) != 0)
                {
                    // Perform adaptive spinning while waiting for the lock.
                    spinWait.SpinOnce();
                }

                // Return the instance to the reuse stack.
                stack.Push(item);
            }
            finally
            {
                // Always release the lock.
                Interlocked.CompareExchange(ref flag, 0, 1);
            }
        }

        /// <summary>
        /// Returns a collection of objects to the pool for later reuse.
        /// </summary>
        /// <param name="items">The collection of object instances to return.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/>.</exception>
        public override void Return(params IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            using IEnumerator<T> enumerator = items.GetEnumerator(); // Get an enumerator for the input collection.

            if (resetCallback is not null) // If a reset callback is provided, we need to execute it for each item before returning them to the pool.
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is not null) // Only process non-null items to avoid exceptions in the reset callback.
                    {
                        resetCallback.Invoke(enumerator.Current); // Reset the state of the item before returning it to the pool.
                    }
                }

                enumerator.Reset();
            }

            try
            {
                SpinWait spinWait = default;

                // Acquire exclusive access to the queue.
                while (Interlocked.CompareExchange(ref flag, 1, 0) != 0)
                {
                    // Perform adaptive spinning while waiting for the lock.
                    spinWait.SpinOnce();
                }

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is not null)
                    {
                        stack.Push(enumerator.Current);
                    }
                }
            }
            finally
            {
                // Always release the lock.
                Interlocked.CompareExchange(ref flag, 0, 1);
            }
        }
    }

    /// <summary>
    /// Rents an object from the pool.
    /// </summary>
    /// <returns>A reusable object instance.</returns>
    public abstract T Rent();

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <param name="item">The object to return.</param>
    public abstract void Return(T item);

    /// <summary>
    /// Returns the specified collections of items.
    /// </summary>
    /// <param name="items">Collections of items to return.</param>
    public abstract void Return(params IEnumerable<T> items);
}
