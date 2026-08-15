using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TryitTest;

[TestClass]
public class SimpleArrayPoolTests
{
    [TestMethod]
    public void Rent_Returns_Array_With_AtLeastRequestedLength()
    {
        var pool = Tryit.SimpleArrayPool<int>.Shared;

        var arr = pool.Rent(7);

        Assert.IsTrue(arr.Length >= 7);
        Assert.IsTrue((arr.Length & (arr.Length - 1)) == 0); // power of two

        pool.Return(arr);
    }

    [TestMethod]
    public void Return_Rejects_NonPowerOfTwo()
    {
        var pool = Tryit.SimpleArrayPool<int>.Shared;

        var arr = new int[3];

        Assert.IsFalse(pool.Return(arr));
    }

    [TestMethod]
    public void ThreadLocalCache_Prioritizes_ThreadLocal()
    {
        var pool = Tryit.SimpleArrayPool<int>.Shared;

        var arr = pool.Rent(16);
        pool.Return(arr);

        // Immediately renting again on same thread should get the thread-local cached array
        var arr2 = pool.Rent(16);

        Assert.AreSame(arr, arr2);

        pool.Return(arr2);
    }

    [TestMethod]
    public void Concurrent_Rent_Return_Stress()
    {
        var pool = Tryit.SimpleArrayPool<int>.Shared;

        int threads = 8;
        int iterations = 1000;

        Thread[] ths = new Thread[threads];
        Exception? exception = null;

        for (int t = 0; t < threads; t++)
        {
            ths[t] = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        var a = pool.Rent(128);
                        // touch memory
                        a[0] = i;
                        pool.Return(a);
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            ths[t].Start();
        }

        for (int t = 0; t < threads; t++)
        {
            ths[t].Join();
        }

        Assert.IsNull(exception);
    }
}
