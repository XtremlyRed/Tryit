using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest.Utils;

[TestClass]
public class SimpleObjectPoolTests
{
    private sealed class PooledItem
    {
        public int Value { get; set; }
    }

    [TestMethod]
    public void GenericCreate_NullFactory_ShouldThrow()
    {
        Assert.ThrowsException<ArgumentNullException>(() => SimpleObjectPool<PooledItem>.Create(null!));
    }

    [TestMethod]
    public void FacadeCreate_NullFactory_ShouldThrow()
    {
        Assert.ThrowsException<ArgumentNullException>(() => global::Tryit.SimpleObjectPool.Create<PooledItem>(null!));
    }

    [TestMethod]
    public void FacadeCreate_NewConstraint_ShouldCreateInstances()
    {
        var pool = global::Tryit.SimpleObjectPool.Create<PooledItem>();

        var item = pool.Rent();

        Assert.IsNotNull(item);
    }

    [TestMethod]
    public void Rent_WhenPoolEmpty_ShouldUseFactory()
    {
        int factoryCount = 0;
        var pool = SimpleObjectPool<PooledItem>.Create(() =>
        {
            factoryCount++;
            return new PooledItem();
        });

        _ = pool.Rent();
        _ = pool.Rent();

        Assert.AreEqual(2, factoryCount);
    }

    [TestMethod]
    public void Return_ThenRent_ShouldReuseSameInstance()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem());
        var source = new PooledItem { Value = 10 };

        pool.Return(source);
        var rented = pool.Rent();

        Assert.AreSame(source, rented);
    }

    [TestMethod]
    public void Return_Null_ShouldThrow()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem());

        Assert.ThrowsException<ArgumentNullException>(() => pool.Return((PooledItem)null!));
    }

    [TestMethod]
    public void Return_ShouldInvokeResetCallback()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem(), x => x.Value = 0);

        var item = new PooledItem { Value = 99 };
        pool.Return(item);

        var rented = pool.Rent();
        Assert.AreEqual(0, rented.Value);
    }

    [TestMethod]
    public void Return_ResetCallbackThrows_ShouldPropagate_AndNotEnqueue()
    {
        int factoryCount = 0;
        var pool = SimpleObjectPool<PooledItem>.Create(
            () =>
            {
                factoryCount++;
                return new PooledItem();
            },
            _ => throw new InvalidOperationException("reset failed")
        );

        var ex = Assert.ThrowsException<InvalidOperationException>(() => pool.Return(new PooledItem()));
        Assert.AreEqual("reset failed", ex.Message);

        var rented = pool.Rent();
        Assert.IsNotNull(rented);
        Assert.AreEqual(1, factoryCount);
    }

    [TestMethod]
    public void Return_MultipleItems_ShouldBeFifo()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem());
        var a = new PooledItem { Value = 1 };
        var b = new PooledItem { Value = 2 };
        var c = new PooledItem { Value = 3 };

        pool.Return(a);
        pool.Return(b);
        pool.Return(c);

        Assert.AreSame(a, pool.Rent());
        Assert.AreSame(b, pool.Rent());
        Assert.AreSame(c, pool.Rent());
    }

    [TestMethod]
    public void Rent_AfterPoolDrained_ShouldUseFactoryAgain()
    {
        int factoryCount = 0;
        var pool = SimpleObjectPool<PooledItem>.Create(() =>
        {
            factoryCount++;
            return new PooledItem();
        });

        var item = new PooledItem();
        pool.Return(item);
        _ = pool.Rent();
        _ = pool.Rent();

        Assert.AreEqual(1, factoryCount);
    }

    [TestMethod]
    public void MaxPoolSizeExceeded_ShouldThrowOnRent()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem(), maxPoolSize: 1);

        pool.Return(new PooledItem());
        pool.Return(new PooledItem());

        Assert.ThrowsException<InvalidOperationException>(() => pool.Rent());
    }

    [TestMethod]
    public void MaxPoolSizeBoundary_ShouldAllowWhenEqual()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem(), maxPoolSize: 2);

        pool.Return(new PooledItem { Value = 1 });
        pool.Return(new PooledItem { Value = 2 });

        Assert.IsNotNull(pool.Rent());
    }

    [TestMethod]
    public void NegativeMaxPoolSize_ShouldThrowOnRent()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem(), maxPoolSize: -1);

        Assert.ThrowsException<InvalidOperationException>(() => pool.Rent());
    }

    [TestMethod]
    public void StringBuilderPool_ShouldReturnSingleton()
    {
        var a = global::Tryit.SimpleObjectPool.StringBuilderPool();
        var b = global::Tryit.SimpleObjectPool.StringBuilderPool();

        Assert.AreSame(a, b);
    }

    [TestMethod]
    public void StringBuilderPool_ShouldClearBuilderOnReturn()
    {
        var pool = global::Tryit.SimpleObjectPool.StringBuilderPool();
        var sb = pool.Rent();
        sb.Append("hello");

        pool.Return(sb);
        var rented = pool.Rent();

        Assert.AreSame(sb, rented);
        Assert.AreEqual(0, rented.Length);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task Concurrent_ReturnThenRent_ShouldKeepAllObjects()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem());
        const int total = 400;
        var source = Enumerable.Range(0, total).Select(i => new PooledItem { Value = i }).ToArray();

        await Task.WhenAll(source.Select(item => Task.Run(() => pool.Return(item))));

        var rented = new ConcurrentBag<PooledItem>();
        await Task.WhenAll(Enumerable.Range(0, total).Select(_ => Task.Run(() => rented.Add(pool.Rent()))));

        Assert.AreEqual(total, rented.Count);
        Assert.AreEqual(total, rented.Distinct().Count());
    }

    [TestMethod]
    public void Create_NonGenericFacadeAndGeneric_ShouldBothWork()
    {
        var facade = global::Tryit.SimpleObjectPool.Create(() => new PooledItem { Value = 1 });
        var generic = SimpleObjectPool<PooledItem>.Create(() => new PooledItem { Value = 2 });

        Assert.AreEqual(1, facade.Rent().Value);
        Assert.AreEqual(2, generic.Rent().Value);
    }

    [TestMethod]
    public void Return_ManyThenRentAll_ShouldPreserveOrder()
    {
        var pool = SimpleObjectPool<PooledItem>.Create(() => new PooledItem());
        var list = new List<PooledItem>();

        for (int i = 0; i < 50; i++)
        {
            var item = new PooledItem { Value = i };
            list.Add(item);
            pool.Return(item);
        }

        for (int i = 0; i < list.Count; i++)
        {
            Assert.AreSame(list[i], pool.Rent());
        }
    }

    [TestMethod]
    public void FactoryThrow_ShouldPropagate_AndPoolRemainsUsable()
    {
        int call = 0;
        var pool = SimpleObjectPool<PooledItem>.Create(() =>
        {
            call++;
            if (call == 1)
            {
                throw new InvalidOperationException("factory failed");
            }

            return new PooledItem();
        });

        var ex = Assert.ThrowsException<InvalidOperationException>(() => pool.Rent());
        Assert.AreEqual("factory failed", ex.Message);

        var item = pool.Rent();
        Assert.IsNotNull(item);
    }

    [TestMethod]
    public void ResetCallbackThrow_ShouldNotBreakSubsequentOperations()
    {
        bool throwNow = true;
        var pool = SimpleObjectPool<PooledItem>.Create(
            () => new PooledItem(),
            _ =>
            {
                if (throwNow)
                {
                    throwNow = false;
                    throw new InvalidOperationException("once");
                }
            }
        );

        Assert.ThrowsException<InvalidOperationException>(() => pool.Return(new PooledItem()));

        var good = new PooledItem { Value = 5 };
        pool.Return(good);

        Assert.AreSame(good, pool.Rent());
    }
}
