using System.Diagnostics;

namespace Tryit;

public sealed class LockFreeObjectPool<T>
    where T : class, new()
{
    private readonly T[] _buffer;

    private readonly int _mask;

    private int _head;

    private int _tail;

    private int flag;

    public LockFreeObjectPool(int capacity = 32)
    {
        if (capacity < 2 || (capacity & (capacity - 1)) != 0)
        {
            throw new ArgumentException("Capacity must be power of 2");
        }

        _buffer = new T[capacity];

        _mask = capacity - 1;
    }

    public T Rent()
    {
        try
        {
            SpinWait spinWait = default!;

            while (Interlocked.CompareExchange(ref flag, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            int tail = Volatile.Read(ref _tail);

            if (tail == Volatile.Read(ref _head))
            {
                return new T();
            }

            int nextTail = tail + 1;

            int idx = (int)(tail & _mask);

            T item = _buffer[idx];

            _buffer[idx] = null!;

            Volatile.Write(ref _tail, nextTail);

            return item ?? new T();
        }
        finally
        {
            Interlocked.Exchange(ref flag, 0);
        }
    }

    public void Return(T item)
    {
        if (item == null)
        {
            return;
        }

        try
        {
            SpinWait spinWait = default!;

            while (Interlocked.CompareExchange(ref flag, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            int head = Volatile.Read(ref _head);
            int nextHead = head + 1;

            if ((nextHead - Volatile.Read(ref _tail)) > _buffer.Length)
            {
                return;
            }

            int idx = (int)(head & _mask);

            _buffer[idx] = item;

            Volatile.Write(ref _head, nextHead);
        }
        finally
        {
            Interlocked.Exchange(ref flag, 0);
        }
    }

    public Lease RentLease()
    {
        return new Lease(this, Rent());
    }

    public sealed class Lease : IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private LockFreeObjectPool<T>? _pool;

        internal Lease(LockFreeObjectPool<T> pool, T item)
        {
            _pool = pool;
            Item = item;
        }

        public T Item { get; }

        void IDisposable.Dispose()
        {
            var pool = Interlocked.Exchange(ref _pool, null);
            if (pool != null)
            {
                pool.Return(Item);
            }
        }
    }
}
