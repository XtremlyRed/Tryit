using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tryit;

/// <summary>
/// A lightweight array pooling implementation optimized for primitive scenarios where
/// callers frequently rent and return arrays of varying sizes. The pool rounds requested
/// lengths up to the next power-of-two and keeps several size buckets. Small arrays are
/// cached in a thread-local slot for extremely fast reuse; larger arrays are stored in
/// shared buckets with a bounded capacity.
///
/// Design notes:
/// - The pool uses simple spin-based synchronization (Interlocked + SpinWait) to avoid
///   heavier locking primitives in hot paths.
/// - The pool only accepts and returns arrays whose length is a power of two. Rent will
///   always return an array whose length is the next power-of-two >= requested length.
/// - Thread-local slots improve performance for small, frequently used arrays and avoid
///   cross-thread contention.
/// - The pool is intended for reuse inside a single process and is exposed via
///   <see cref="Shared"/> for convenience.
/// </summary>
/// <typeparam name="T">Element type of the arrays pooled.</typeparam>
public abstract class SimpleArrayPool<T>
{
    private class SimpleArrayPoolImpl : SimpleArrayPool<T> { }

    /// <summary>
    /// Shared singleton pool instance. Consumers can use this instance for general-purpose
    /// array pooling without creating their own pool object.
    /// </summary>
    public static readonly SimpleArrayPool<T> Shared = new SimpleArrayPoolImpl();

    /// <summary>
    /// Maximum array length that will be cached in the per-thread slot. Arrays with length
    /// less than or equal to this value are first attempted to be stored in a thread-local
    /// cache for fast reuse.
    /// </summary>
    private const int ThreadLocalMaxSize = 2 * 1024;

    /// <summary>
    /// Maximum capacity per shared bucket. Each bucket holds arrays of a single power-of-two
    /// length and can store up to this many arrays for reuse.
    /// </summary>
    private const int MaxBucketCount = 256;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Bucket[] buckets;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ThreadLocal<ThreadLocalCache> threadCache;

    /// <summary>
    /// Initializes the pool buckets and the thread-local caches. There are 32 buckets
    /// corresponding to power-of-two sizes from 2^0..2^31 (practically only a subset are used).
    /// </summary>
    protected SimpleArrayPool()
    {
        buckets = new Bucket[32];

        for (int i = 0; i < 32; i++)
        {
            buckets[i] = new Bucket(MaxBucketCount);
        }

        threadCache = new ThreadLocal<ThreadLocalCache>(() => new ThreadLocalCache(), trackAllValues: false);
    }

    /// <summary>
    /// Rent an array with at least the specified length. The returned array length will
    /// be the next power-of-two greater than or equal to <paramref name="length"/>.
    /// </summary>
    /// <param name="length">Minimum requested length. Must be non-negative.</param>
    /// <returns>An array of length equal to a power-of-two >= <paramref name="length"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is negative.</exception>
    public T[] Rent(int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        // Compute the bucket length (power-of-two) for the requested size.
        uint arrayLength = NextPowerOfTwo(length);

        int indexkey = BitIndex(arrayLength);

        // Try thread-local cache first for very small arrays to minimize cross-thread contention.
        if ((int)arrayLength <= ThreadLocalMaxSize)
        {
            if (threadCache.Value!.TryPop(indexkey, out T[]? array))
            {
                return array;
            }
        }

        // Fallback to shared bucket
        Bucket bucket = buckets[indexkey];

        return bucket.TryPop(out T[]? values) ? values : Create((int)arrayLength);
    }

    /// <summary>
    /// Return an array to the pool for potential reuse. The array must be non-null and its
    /// length must be a power of two; otherwise the return is rejected and <c>false</c> is
    /// returned.
    /// </summary>
    /// <param name="array">Array to return to the pool.</param>
    /// <returns><c>true</c> when the array was accepted into the pool; <c>false</c> otherwise.</returns>
    public bool Return(T[] array)
    {
        if (array == null || IsPowerOfTwo(array.Length) == false)
        {
            return false;
        }

        int indexkey = BitIndex((uint)array.Length);

        // Attempt to store in thread-local slot for small arrays first
        if (array.Length <= ThreadLocalMaxSize)
        {
            if (threadCache.Value!.TryPush(indexkey, array))
            {
                return true;
            }
        }

        Bucket bucket = buckets[indexkey];

        return bucket.TryPush(array);
    }

    /// <summary>
    /// Create a new array of the requested length. This method is virtual so derived
    /// pools can override allocation behavior if they need custom initialization.
    /// </summary>
    /// <param name="targetLength">Desired length (power-of-two) of the array to create.</param>
    /// <returns>A newly allocated array of length <paramref name="targetLength"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual T[] Create(int targetLength)
    {
        return new T[targetLength];
    }

    /// <summary>
    /// Shared per-size bucket that stores returned arrays. Each bucket has a fixed maximum
    /// capacity and uses a simple spin-lock (Interlocked + SpinWait) to protect the internal
    /// storage with minimal overhead.
    /// </summary>
    /// <remarks>
    /// Create a new bucket with a given maximum capacity.
    /// </remarks>
    /// <param name="maxCapacity">Maximum number of arrays to hold in this bucket.</param>
    private class Bucket(int maxCapacity)
    {
        private int count = 0;
        private int locker;
        private readonly T[][] items = new T[maxCapacity][];

        /// <summary>
        /// Try to push an array into the bucket. Returns false when the bucket has reached
        /// its maximum capacity.
        /// </summary>
        /// <param name="item">Array to store.</param>
        /// <returns>True when the array was stored; false when the bucket is full.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPush(T[] item)
        {
            bool pushResult = false;

            SpinWait spinWait = default;

            while (Interlocked.CompareExchange(ref locker, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            if (count < maxCapacity)
            {
                items[count++] = item;

                pushResult = true;
            }

            Interlocked.Exchange(ref locker, 0);

            return pushResult;
        }

        /// <summary>
        /// Try to pop an array from the bucket. If the bucket is empty the method returns false.
        /// </summary>
        /// <param name="value">When true is returned this out parameter contains the popped array.</param>
        /// <returns>True when an array was popped; false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T[] value)
        {
            value = null!;

            SpinWait spinWait = default;

            while (Interlocked.CompareExchange(ref locker, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            if (count > 0)
            {
                int idx = --count;

                value = items[idx];

                items[idx] = null!;
            }

            Interlocked.Exchange(ref locker, 0);

            return value is not null;
        }
    }

    /// <summary>
    /// Per-thread small cache used to store a single array for each supported bucket size.
    /// This cache avoids cross-thread contention for the most commonly used small arrays.
    /// </summary>
    private class ThreadLocalCache
    {
        /// <summary>
        /// Slots indexed by bucket index; each slot may contain at most one array for fast access.
        /// </summary>
        public readonly T[][] slots = new T[32][];

        /// <summary>
        /// Try to push an array into the thread-local slot for the specified bucket index.
        /// The push succeeds only if the slot is currently empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPush(int indexkey, T[] array)
        {
            if (slots[indexkey] == null)
            {
                slots[indexkey] = array;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to pop an array from the thread-local slot. Returns true and clears the slot when
        /// an array is present.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(int indexkey, out T[] array)
        {
            array = slots[indexkey];

            if (array != null)
            {
                slots[indexkey] = null!;

                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Compute the next power-of-two greater than or equal to the provided value.
    /// The method returns at least 2 for small inputs.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint NextPowerOfTwo(int value)
    {
        uint x = (uint)Math.Max(value, 2);
        x--;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        x++;
        return x;
    }

    /// <summary>
    /// Convert a power-of-two value to its bucket index by counting trailing shifts
    /// (i.e. log2(value)).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int BitIndex(uint value)
    {
        int index = 0;

        while ((value >>= 1) != 0)
        {
            index++;
        }

        return index;
    }

    /// <summary>
    /// Determines whether the provided integer is a power of two.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPowerOfTwo(int x)
    {
        return x > 0 && (x & (x - 1)) == 0;
    }
}
