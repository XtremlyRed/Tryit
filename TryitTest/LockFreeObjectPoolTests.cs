using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest.Utils;

[TestClass]
public class LockFreeObjectPoolTests
{
    private sealed class PooledItem
    {
        public int Id { get; set; }
    }

    private static PooledItem NewItem(ref int id)
    {
        id++;
        return new PooledItem { Id = id };
    }

    private static void AssertPoolMatchesQueue(LockFreeObjectPool<PooledItem> pool, Queue<PooledItem> expected)
    {
        while (expected.Count > 0)
        {
            PooledItem actual = pool.Rent();
            PooledItem expect = expected.Dequeue();
            Assert.AreSame(expect, actual);
        }

        Assert.IsNotNull(pool.Rent());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Ctor_CapacityZero_ShouldThrow()
    {
        _ = new LockFreeObjectPool<PooledItem>(0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Ctor_CapacityNegative_ShouldThrow()
    {
        _ = new LockFreeObjectPool<PooledItem>(-8);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Ctor_CapacityLessThan2_ShouldThrow()
    {
        _ = new LockFreeObjectPool<PooledItem>(1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Ctor_CapacityNotPowerOfTwo_ShouldThrow()
    {
        _ = new LockFreeObjectPool<PooledItem>(3);
    }

    [TestMethod]
    public void Ctor_DefaultCapacity_ShouldWork()
    {
        var pool = new LockFreeObjectPool<PooledItem>();

        var item = pool.Rent();

        Assert.IsNotNull(item);
    }

    [TestMethod]
    public void Ctor_DefaultCapacity_ShouldSupport32Items()
    {
        var pool = new LockFreeObjectPool<PooledItem>();
        var source = Enumerable.Range(1, 32).Select(i => new PooledItem { Id = i }).ToArray();

        foreach (var item in source)
        {
            pool.Return(item);
        }

        foreach (var item in source)
        {
            Assert.AreSame(item, pool.Rent());
        }

        var extra = pool.Rent();
        Assert.IsFalse(source.Any(x => ReferenceEquals(x, extra)));
    }

    [TestMethod]
    public void Ctor_MinValidCapacity_ShouldWork()
    {
        var pool = new LockFreeObjectPool<PooledItem>(2);

        var first = new PooledItem { Id = 11 };
        var second = new PooledItem { Id = 22 };

        pool.Return(first);
        pool.Return(second);

        Assert.AreSame(first, pool.Rent());
        Assert.AreSame(second, pool.Rent());
    }

    [TestMethod]
    public void Rent_WhenPoolEmpty_ShouldCreateNewInstance()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);

        PooledItem first = pool.Rent();
        PooledItem second = pool.Rent();

        Assert.IsNotNull(first);
        Assert.IsNotNull(second);
        Assert.AreNotSame(first, second);
    }

    [TestMethod]
    public void Return_ThenRent_ShouldReturnSameInstance()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);
        var source = new PooledItem { Id = 100 };

        pool.Return(source);

        var rented = pool.Rent();

        Assert.AreSame(source, rented);
    }

    [TestMethod]
    public void Return_ManyItemsBeyondCapacity_ShouldKeepOldestWithinCapacity()
    {
        const int capacity = 4;
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        var source = Enumerable.Range(1, 10).Select(i => new PooledItem { Id = i }).ToArray();

        foreach (var item in source)
        {
            pool.Return(item);
        }

        Assert.AreSame(source[0], pool.Rent());
        Assert.AreSame(source[1], pool.Rent());
        Assert.AreSame(source[2], pool.Rent());
        Assert.AreSame(source[3], pool.Rent());

        var extra = pool.Rent();
        for (int i = 0; i < source.Length; i++)
        {
            Assert.IsFalse(ReferenceEquals(source[i], extra));
        }
    }

    [TestMethod]
    public void Return_AndRent_ShouldSupportFullCapacity()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);
        var source = new[]
        {
            new PooledItem { Id = 1 },
            new PooledItem { Id = 2 },
            new PooledItem { Id = 3 },
            new PooledItem { Id = 4 },
        };

        foreach (var item in source)
            pool.Return(item);

        var rented = new List<PooledItem> { pool.Rent(), pool.Rent(), pool.Rent(), pool.Rent() };

        CollectionAssert.AreEqual(source, rented);

        var extra = pool.Rent();
        Assert.IsFalse(ReferenceEquals(source[0], extra));
        Assert.IsFalse(ReferenceEquals(source[1], extra));
        Assert.IsFalse(ReferenceEquals(source[2], extra));
        Assert.IsFalse(ReferenceEquals(source[3], extra));
    }

    [TestMethod]
    public void Return_AndRent_MoreThanOneCycle_ShouldKeepFifoPerCycle()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);

        var cycle1 = Enumerable.Range(1, 4).Select(i => new PooledItem { Id = i }).ToArray();

        foreach (var item in cycle1)
        {
            pool.Return(item);
        }

        foreach (var item in cycle1)
        {
            Assert.AreSame(item, pool.Rent());
        }

        var cycle2 = Enumerable.Range(101, 4).Select(i => new PooledItem { Id = i }).ToArray();

        foreach (var item in cycle2)
        {
            pool.Return(item);
        }

        foreach (var item in cycle2)
        {
            Assert.AreSame(item, pool.Rent());
        }
    }

    [TestMethod]
    public void Return_AndRent_ShouldHandleRingWrapAround()
    {
        var pool = new LockFreeObjectPool<PooledItem>(2);

        var a = new PooledItem { Id = 1 };
        var b = new PooledItem { Id = 2 };
        var c = new PooledItem { Id = 3 };
        var d = new PooledItem { Id = 4 };

        pool.Return(a);
        pool.Return(b);

        Assert.AreSame(a, pool.Rent());

        pool.Return(c);

        Assert.AreSame(b, pool.Rent());
        Assert.AreSame(c, pool.Rent());

        pool.Return(d);

        Assert.AreSame(d, pool.Rent());
    }

    [TestMethod]
    public void InterleavedOperations_ShouldPreserveQueueSemantics()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);

        var a = new PooledItem { Id = 1 };
        var b = new PooledItem { Id = 2 };
        var c = new PooledItem { Id = 3 };
        var d = new PooledItem { Id = 4 };
        var e = new PooledItem { Id = 5 };
        var f = new PooledItem { Id = 6 };

        pool.Return(a);
        pool.Return(b);
        Assert.AreSame(a, pool.Rent());

        pool.Return(c);
        pool.Return(d);
        Assert.AreSame(b, pool.Rent());

        pool.Return(e);
        pool.Return(f);

        Assert.AreSame(c, pool.Rent());
        Assert.AreSame(d, pool.Rent());
        Assert.AreSame(e, pool.Rent());
        Assert.AreSame(f, pool.Rent());
    }

    [TestMethod]
    public void Rent_AfterDrainingPool_ShouldNotReturnPreviouslyDrainedInstances()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);
        var a = new PooledItem { Id = 1 };
        var b = new PooledItem { Id = 2 };
        var c = new PooledItem { Id = 3 };
        var d = new PooledItem { Id = 4 };

        pool.Return(a);
        pool.Return(b);
        pool.Return(c);
        pool.Return(d);

        var drained = new[] { pool.Rent(), pool.Rent(), pool.Rent(), pool.Rent() };

        var next = pool.Rent();

        Assert.IsFalse(drained.Any(x => ReferenceEquals(x, next)));
    }

    [TestMethod]
    public void Return_WhenPoolIsFull_ShouldDropExtraItem()
    {
        var pool = new LockFreeObjectPool<PooledItem>(2);
        var a = new PooledItem { Id = 1 };
        var b = new PooledItem { Id = 2 };
        var c = new PooledItem { Id = 3 };

        pool.Return(a);
        pool.Return(b);
        pool.Return(c);

        var r1 = pool.Rent();
        var r2 = pool.Rent();
        var r3 = pool.Rent();

        Assert.AreSame(a, r1);
        Assert.AreSame(b, r2);
        Assert.IsFalse(ReferenceEquals(c, r3));
    }

    [TestMethod]
    public void Return_Null_ShouldBeIgnored()
    {
        var pool = new LockFreeObjectPool<PooledItem>(2);

        pool.Return(null!);

        var item = pool.Rent();
        Assert.IsNotNull(item);
    }

    [TestMethod]
    public void Return_SameInstanceMultipleTimes_ShouldAllowDuplicateReferences()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);
        var shared = new PooledItem { Id = 7 };

        pool.Return(shared);
        pool.Return(shared);

        var first = pool.Rent();
        var second = pool.Rent();

        Assert.AreSame(shared, first);
        Assert.AreSame(shared, second);
    }

    [TestMethod]
    public void ManyWrapAroundCycles_ShouldStayCorrect()
    {
        const int capacity = 8;
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        int id = 0;

        for (int cycle = 0; cycle < 200; cycle++)
        {
            var expected = new Queue<PooledItem>();

            for (int i = 0; i < capacity; i++)
            {
                var item = NewItem(ref id);
                expected.Enqueue(item);
                pool.Return(item);
            }

            AssertPoolMatchesQueue(pool, expected);
        }
    }

    [TestMethod]
    public void ModelBasedSequentialScenario_ShouldMatchReferenceQueue()
    {
        const int capacity = 8;
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        var expected = new Queue<PooledItem>();
        var inExpected = new HashSet<PooledItem>();
        var random = new Random(12345);
        int id = 0;

        for (int i = 0; i < 5000; i++)
        {
            bool doReturn = expected.Count == 0 || random.Next(100) < 65;

            if (doReturn)
            {
                var item = NewItem(ref id);
                pool.Return(item);

                if (expected.Count < capacity)
                {
                    expected.Enqueue(item);
                    inExpected.Add(item);
                }
            }
            else
            {
                var actual = pool.Rent();

                if (expected.Count > 0)
                {
                    var expect = expected.Dequeue();
                    inExpected.Remove(expect);
                    Assert.AreSame(expect, actual);
                }
                else
                {
                    Assert.IsNotNull(actual);
                    Assert.IsFalse(inExpected.Contains(actual));
                }
            }
        }

        while (expected.Count > 0)
        {
            var expect = expected.Dequeue();
            var actual = pool.Rent();
            Assert.AreSame(expect, actual);
        }

        Assert.IsNotNull(pool.Rent());
    }

    public static IEnumerable<object[]> FifoRoundTripCases()
    {
        int[] capacities = [2, 4, 8, 16, 32];
        int[] rounds = [1, 2, 3, 4, 5];

        foreach (int capacity in capacities)
        {
            foreach (int round in rounds)
            {
                yield return [capacity, round];
            }
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(FifoRoundTripCases), DynamicDataSourceType.Method)]
    public void FifoRoundTrip_Matrix_ShouldPass(int capacity, int rounds)
    {
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        int id = 0;

        for (int r = 0; r < rounds; r++)
        {
            var expected = new Queue<PooledItem>();
            for (int i = 0; i < capacity; i++)
            {
                var item = NewItem(ref id);
                expected.Enqueue(item);
                pool.Return(item);
            }

            while (expected.Count > 0)
            {
                Assert.AreSame(expected.Dequeue(), pool.Rent());
            }

            Assert.IsNotNull(pool.Rent());
        }
    }

    [TestMethod]
    public void RentLease_Dispose_ShouldReturnObjectToPool()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);
        var source = new PooledItem { Id = 999 };
        pool.Return(source);

        using (var lease = pool.RentLease())
        {
            Assert.AreSame(source, lease.Item);
        }

        Assert.AreSame(source, pool.Rent());
    }

    [TestMethod]
    public void RentLease_DisposeTwice_ShouldBeSafe()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);
        var source = new PooledItem { Id = 123 };

        using var lease = pool.RentLease();
        lease.Item.Id = source.Id;

        var first = pool.Rent();
        var second = pool.Rent();

        Assert.AreSame(lease.Item, first);
        Assert.AreNotSame(lease.Item, second);
    }

    [TestMethod]
    public void RentLease_WhenPoolEmpty_ShouldDisposeAndReturnRentedInstance()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);
        PooledItem fromLease;

        using (var lease = pool.RentLease())
        {
            fromLease = lease.Item;
            Assert.IsNotNull(fromLease);
        }

        Assert.AreSame(fromLease, pool.Rent());
    }

    public static IEnumerable<object[]> InvalidCapacityCases()
    {
        int[] values = [-16, -8, -1, 0, 1, 3, 5, 6, 7, 9, 10, 12, 18, 33, 127];
        foreach (int value in values)
        {
            yield return [value];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(InvalidCapacityCases), DynamicDataSourceType.Method)]
    public void Ctor_InvalidCapacities_ShouldThrowWithExpectedMessage(int capacity)
    {
        var ex = Assert.ThrowsException<ArgumentException>(() => _ = new LockFreeObjectPool<PooledItem>(capacity));
        StringAssert.Contains(ex.Message, "power of 2");
    }

    public static IEnumerable<object[]> ValidCapacityCases()
    {
        int[] values = [2, 4, 8, 16, 32, 64, 128, 256, 512, 1024];
        foreach (int value in values)
        {
            yield return [value];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(ValidCapacityCases), DynamicDataSourceType.Method)]
    public void Ctor_ValidCapacities_ShouldRentAndReturnNormally(int capacity)
    {
        var pool = new LockFreeObjectPool<PooledItem>(capacity);

        var expected = Enumerable.Range(1, capacity).Select(i => new PooledItem { Id = i }).ToArray();

        foreach (var item in expected)
        {
            pool.Return(item);
        }

        foreach (var item in expected)
        {
            Assert.AreSame(item, pool.Rent());
        }

        Assert.IsNotNull(pool.Rent());
    }

    [TestMethod]
    public void Return_NullBetweenValidItems_ShouldNotAffectOrder()
    {
        var pool = new LockFreeObjectPool<PooledItem>(4);
        var a = new PooledItem { Id = 1 };
        var b = new PooledItem { Id = 2 };

        pool.Return(a);
        pool.Return(null!);
        pool.Return(b);
        pool.Return(null!);

        Assert.AreSame(a, pool.Rent());
        Assert.AreSame(b, pool.Rent());
        Assert.IsNotNull(pool.Rent());
    }

    public static IEnumerable<object[]> DuplicateReturnCases()
    {
        int[] capacities = [2, 4, 8, 16];
        int[] repeats = [2, 3, 4, 5, 6, 8, 10, 12];

        foreach (int capacity in capacities)
        {
            foreach (int repeat in repeats)
            {
                yield return [capacity, repeat];
            }
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(DuplicateReturnCases), DynamicDataSourceType.Method)]
    public void Return_SameInstanceManyTimes_ShouldFollowCapacityRule(int capacity, int repeat)
    {
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        var shared = new PooledItem { Id = 42 };

        for (int i = 0; i < repeat; i++)
        {
            pool.Return(shared);
        }

        int expectedCount = Math.Min(capacity, repeat);
        for (int i = 0; i < expectedCount; i++)
        {
            Assert.AreSame(shared, pool.Rent());
        }

        Assert.IsNotNull(pool.Rent());
    }

    public static IEnumerable<object[]> ExhaustiveScriptCases()
    {
        const int length = 6;
        const int count = 729; // 3^6

        for (int scriptId = 0; scriptId < count; scriptId++)
        {
            yield return [scriptId, length, 2];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(ExhaustiveScriptCases), DynamicDataSourceType.Method)]
    public void Exhaustive_Capacity2_AllShortScripts_ShouldMatchModel(int scriptId, int length, int capacity)
    {
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        var expected = new Queue<PooledItem>();
        var stillExpected = new HashSet<PooledItem>();
        int id = 0;

        int value = scriptId;
        for (int step = 0; step < length; step++)
        {
            int op = value % 3;
            value /= 3;

            if (op == 0)
            {
                var item = NewItem(ref id);
                pool.Return(item);
                if (expected.Count < capacity)
                {
                    expected.Enqueue(item);
                    stillExpected.Add(item);
                }
            }
            else if (op == 1)
            {
                pool.Return(null!);
            }
            else
            {
                var actual = pool.Rent();

                if (expected.Count > 0)
                {
                    var expect = expected.Dequeue();
                    stillExpected.Remove(expect);
                    Assert.AreSame(expect, actual);
                }
                else
                {
                    Assert.IsNotNull(actual);
                    Assert.IsFalse(stillExpected.Contains(actual));
                }
            }
        }

        while (expected.Count > 0)
        {
            Assert.AreSame(expected.Dequeue(), pool.Rent());
        }

        Assert.IsNotNull(pool.Rent());
    }

    public static IEnumerable<object[]> NoThrowStressCases()
    {
        int[] capacities = [2, 4, 8, 16, 32, 64];
        int[] seeds = [7, 17, 27, 37, 47, 57, 67, 77, 87, 97];

        foreach (int capacity in capacities)
        {
            foreach (int seed in seeds)
            {
                yield return [capacity, seed, 2000];
            }
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(NoThrowStressCases), DynamicDataSourceType.Method)]
    public void Stress_NoNullAndNoThrow_ShouldHold(int capacity, int seed, int steps)
    {
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        var expected = new Queue<PooledItem>();
        var random = new Random(seed);
        int id = 0;

        for (int i = 0; i < steps; i++)
        {
            int op = random.Next(100);

            if (op < 50)
            {
                var item = NewItem(ref id);
                pool.Return(item);
                if (expected.Count < capacity)
                {
                    expected.Enqueue(item);
                }
            }
            else if (op < 65)
            {
                pool.Return(null!);
            }
            else
            {
                var item = pool.Rent();
                Assert.IsNotNull(item);

                if (expected.Count > 0)
                {
                    Assert.AreSame(expected.Dequeue(), item);
                }
            }
        }

        while (expected.Count > 0)
        {
            Assert.AreSame(expected.Dequeue(), pool.Rent());
        }

        Assert.IsNotNull(pool.Rent());
    }

    public static IEnumerable<object[]> OverflowDropCases()
    {
        int[] capacities = [2, 4, 8, 16, 32, 64];
        int[] extras = [1, 2, 3, 4, 5];

        foreach (int capacity in capacities)
        {
            foreach (int extra in extras)
            {
                yield return [capacity, extra];
            }
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(OverflowDropCases), DynamicDataSourceType.Method)]
    public void OverflowDrop_Matrix_ShouldKeepFirstCapacityItems(int capacity, int extraReturns)
    {
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        var source = Enumerable.Range(1, capacity + extraReturns).Select(i => new PooledItem { Id = i }).ToArray();

        foreach (var item in source)
        {
            pool.Return(item);
        }

        for (int i = 0; i < capacity; i++)
        {
            Assert.AreSame(source[i], pool.Rent());
        }

        var next = pool.Rent();
        for (int i = 0; i < source.Length; i++)
        {
            Assert.IsFalse(ReferenceEquals(source[i], next));
        }
    }

    public static IEnumerable<object[]> ModelMatrixCases()
    {
        int[] capacities = [2, 4, 8, 16, 32];

        for (int seed = 1; seed <= 70; seed++)
        {
            int capacity = capacities[seed % capacities.Length];
            int steps = 200 + (seed % 5) * 50;
            yield return [seed, capacity, steps];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(ModelMatrixCases), DynamicDataSourceType.Method)]
    public void ModelBased_Matrix_ShouldMatchReferenceQueue(int seed, int capacity, int steps)
    {
        var pool = new LockFreeObjectPool<PooledItem>(capacity);
        var expected = new Queue<PooledItem>();
        var inExpected = new HashSet<PooledItem>();
        var random = new Random(seed);
        int id = 0;

        for (int i = 0; i < steps; i++)
        {
            bool doReturn = expected.Count == 0 || random.Next(100) < 60;

            if (doReturn)
            {
                var item = NewItem(ref id);
                pool.Return(item);

                if (expected.Count < capacity)
                {
                    expected.Enqueue(item);
                    inExpected.Add(item);
                }
            }
            else
            {
                var actual = pool.Rent();

                if (expected.Count > 0)
                {
                    var expect = expected.Dequeue();
                    inExpected.Remove(expect);
                    Assert.AreSame(expect, actual);
                }
                else
                {
                    Assert.IsNotNull(actual);
                    Assert.IsFalse(inExpected.Contains(actual));
                }
            }
        }

        while (expected.Count > 0)
        {
            Assert.AreSame(expected.Dequeue(), pool.Rent());
        }

        Assert.IsNotNull(pool.Rent());
    }
}
