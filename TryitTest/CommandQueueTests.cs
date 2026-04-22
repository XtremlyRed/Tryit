using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Diagnostics;
using Tryit;

namespace TryitTest;

[TestClass]
public class CommandQueueTests
{
    private sealed class SyncItem : ICommandItem
    {
        private readonly Action<string>? _onExecute;

        public SyncItem(string id, CommandType type, Action<string>? onExecute = null)
        {
            Id = id;
            CommandType = type;
            _onExecute = onExecute;
        }

        public string Id { get; }

        public CommandType CommandType { get; }

        public int ExecuteCount;

        public void Execute()
        {
            Interlocked.Increment(ref ExecuteCount);
            _onExecute?.Invoke(Id);
        }
    }

    private sealed class AsyncItem : IAsyncCommandItem
    {
        private readonly Func<string, Task>? _onExecuteAsync;

        public AsyncItem(string id, CommandType type, Func<string, Task>? onExecuteAsync = null)
        {
            Id = id;
            CommandType = type;
            _onExecuteAsync = onExecuteAsync;
        }

        public string Id { get; }

        public CommandType CommandType { get; }

        public int ExecuteCount;

        public async Task ExecuteAsync()
        {
            Interlocked.Increment(ref ExecuteCount);
            if (_onExecuteAsync is not null)
            {
                await _onExecuteAsync(Id);
            }
        }
    }

    private sealed class InvalidTypeItem : ICommandItem
    {
        public CommandType CommandType => (CommandType)999;
        public int ExecuteCount;
        public void Execute() => Interlocked.Increment(ref ExecuteCount);
    }

    [TestMethod]
    public void NewQueue_Count_ShouldBeZero()
    {
        Assert.AreEqual(0, new CommandQueue<SyncItem>().Count);
        Assert.AreEqual(0, new AsyncCommandQueue<AsyncItem>().Count);
    }

    [TestMethod]
    public void Append_Null_ShouldBeIgnored()
    {
        var q = new CommandQueue<SyncItem>();
        q.Append(null!);
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    public void AppendRange_Null_ShouldBeIgnored()
    {
        var q = new CommandQueue<SyncItem>();
        q.AppendRange(null!);
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    public void AppendRange_ShouldSkipNulls_AndSplitByCommandType()
    {
        var q = new CommandQueue<SyncItem>();
        q.AppendRange(new SyncItem?[]
        {
            new("C1", CommandType.Continuous),
            null,
            new("O1", CommandType.Once),
            null,
            new("C2", CommandType.Continuous),
        }!);

        Assert.AreEqual(3, q.Count);
    }

    [TestMethod]
    public void Append_InvalidCommandType_ShouldBeIgnored()
    {
        var q = new CommandQueue<ICommandItem>();
        q.Append(new InvalidTypeItem());
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    public void Execute_EmptyQueue_ShouldNotThrow()
    {
        var q = new CommandQueue<SyncItem>();
        q.Execute();
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    public async Task ExecuteAsync_EmptyQueue_ShouldNotThrow()
    {
        var q = new AsyncCommandQueue<AsyncItem>();
        await q.ExecuteAsync();
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    public void Execute_Once_ShouldRemoveImmediately()
    {
        var q = new CommandQueue<SyncItem>();
        var log = new List<string>();
        var item = new SyncItem("O1", CommandType.Once, log.Add);

        q.Append(item);

        q.Execute();
        q.Execute(); // empty

        CollectionAssert.AreEqual(new[] { "O1" }, log);
        Assert.AreEqual(1, item.ExecuteCount);
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    public void Execute_Continuous_ShouldRemainAndRoundRobin()
    {
        var q = new CommandQueue<SyncItem>();
        var log = new List<string>();

        q.AppendRange(new[]
        {
            new SyncItem("C1", CommandType.Continuous, log.Add),
            new SyncItem("C2", CommandType.Continuous, log.Add),
            new SyncItem("C3", CommandType.Continuous, log.Add),
        });

        for (int i = 0; i < 7; i++) q.Execute();

        CollectionAssert.AreEqual(new[] { "C1", "C2", "C3", "C1", "C2", "C3", "C1" }, log);
        Assert.AreEqual(3, q.Count);
    }

    [TestMethod]
    public void Execute_OnceHasPriorityOverContinuous_ShouldRunAllOnceFirst()
    {
        var q = new CommandQueue<SyncItem>();
        var log = new List<string>();

        q.Append(new SyncItem("C1", CommandType.Continuous, log.Add));
        q.Append(new SyncItem("O1", CommandType.Once, log.Add));
        q.Append(new SyncItem("C2", CommandType.Continuous, log.Add));
        q.Append(new SyncItem("O2", CommandType.Once, log.Add));

        q.Execute();
        q.Execute();
        q.Execute();
        q.Execute();
        q.Execute();

        CollectionAssert.AreEqual(new[] { "O1", "O2", "C1", "C2", "C1" }, log);
        Assert.AreEqual(2, q.Count);
    }

    [TestMethod]
    public void Execute_CommandThrows_ShouldPropagate_AndQueueShouldRecover()
    {
        var q = new CommandQueue<SyncItem>();
        var log = new List<string>();

        q.Append(new SyncItem("O1", CommandType.Once, _ => throw new InvalidOperationException("sync boom")));
        q.Append(new SyncItem("O2", CommandType.Once, log.Add));

        var ex = Assert.ThrowsException<InvalidOperationException>(() => q.Execute());
        Assert.AreEqual("sync boom", ex.Message);

        q.Execute(); // should still work

        CollectionAssert.AreEqual(new[] { "O2" }, log);
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    public async Task ExecuteAsync_CommandThrows_ShouldPropagate_AndQueueShouldRecover()
    {
        var q = new AsyncCommandQueue<AsyncItem>();
        var log = new List<string>();

        q.Append(new AsyncItem("O1", CommandType.Once, _ => throw new InvalidOperationException("async boom")));
        q.Append(new AsyncItem("O2", CommandType.Once, id =>
        {
            log.Add(id);
            return Task.CompletedTask;
        }));

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => q.ExecuteAsync());
        Assert.AreEqual("async boom", ex.Message);

        await q.ExecuteAsync();

        CollectionAssert.AreEqual(new[] { "O2" }, log);
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task Concurrent_Append_ShouldKeepExactCount()
    {
        var q = new CommandQueue<SyncItem>();
        const int producers = 12;
        const int each = 300;
        var gate = new ManualResetEventSlim(false);

        var tasks = Enumerable.Range(0, producers).Select(p => Task.Run(() =>
        {
            gate.Wait();
            for (int i = 0; i < each; i++)
            {
                var type = (i % 2 == 0) ? CommandType.Once : CommandType.Continuous;
                q.Append(new SyncItem($"{p}-{i}", type));
            }
        })).ToArray();

        gate.Set();
        await Task.WhenAll(tasks);

        Assert.AreEqual(producers * each, q.Count);
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task Concurrent_AppendRange_ShouldKeepExactCount()
    {
        var q = new CommandQueue<SyncItem>();
        const int producers = 10;
        const int eachBatch = 250;
        var gate = new ManualResetEventSlim(false);

        var tasks = Enumerable.Range(0, producers).Select(p => Task.Run(() =>
        {
            var batch = Enumerable.Range(0, eachBatch)
                .Select(i => new SyncItem($"{p}-{i}", i % 3 == 0 ? CommandType.Once : CommandType.Continuous))
                .ToArray();

            gate.Wait();
            q.AppendRange(batch);
        })).ToArray();

        gate.Set();
        await Task.WhenAll(tasks);

        Assert.AreEqual(producers * eachBatch, q.Count);
    }

    [TestMethod]
    [Timeout(20000)]
    public void Concurrent_Execute_WithOnlyOnce_ShouldExecuteEachExactlyOnce()
    {
        var q = new CommandQueue<SyncItem>();
        const int total = 2000;
        var map = new ConcurrentDictionary<string, int>();

        q.AppendRange(Enumerable.Range(0, total).Select(i =>
            new SyncItem($"O{i}", CommandType.Once, id => map.AddOrUpdate(id, 1, (_, old) => old + 1))));

        Parallel.For(0, total, _ => q.Execute());

        Assert.AreEqual(0, q.Count);
        Assert.AreEqual(total, map.Count);
        Assert.IsTrue(map.Values.All(v => v == 1), "Once item executed more than once.");
    }

    [TestMethod]
    [Timeout(20000)]
    public async Task Concurrent_ExecuteAsync_WithOnlyOnce_ShouldExecuteEachExactlyOnce()
    {
        var q = new AsyncCommandQueue<AsyncItem>();
        const int total = 1500;
        var map = new ConcurrentDictionary<string, int>();

        q.AppendRange(Enumerable.Range(0, total).Select(i =>
            new AsyncItem($"O{i}", CommandType.Once, id =>
            {
                map.AddOrUpdate(id, 1, (_, old) => old + 1);
                return Task.CompletedTask;
            })));

        var workers = Enumerable.Range(0, total)
            .Select(_ => Task.Run(() => q.ExecuteAsync()))
            .ToArray();

        await Task.WhenAll(workers);

        Assert.AreEqual(0, q.Count);
        Assert.AreEqual(total, map.Count);
        Assert.IsTrue(map.Values.All(v => v == 1), "Async once item executed more than once.");
    }

    [TestMethod]
    [Timeout(20000)]
    public async Task ProducerConsumer_ConcurrentAppendAndExecute_AllOnceShouldBeConsumed()
    {
        var q = new CommandQueue<SyncItem>();
        const int producers = 8;
        const int each = 300;
        int consumed = 0;
        int expected = producers * each;

        var gate = new ManualResetEventSlim(false);

        var producerTasks = Enumerable.Range(0, producers).Select(p => Task.Run(() =>
        {
            gate.Wait();
            for (int i = 0; i < each; i++)
            {
                q.Append(new SyncItem($"{p}-{i}", CommandType.Once, _ => Interlocked.Increment(ref consumed)));
            }
        })).ToArray();

        var consumerTasks = Enumerable.Range(0, 6).Select(_ => Task.Run(() =>
        {
            gate.Wait();
            while (Volatile.Read(ref consumed) < expected)
            {
                q.Execute();
            }
        })).ToArray();

        gate.Set();

        await Task.WhenAll(producerTasks);
        await Task.WhenAll(consumerTasks);

        Assert.AreEqual(expected, consumed);
        Assert.AreEqual(0, q.Count);
    }

    [TestMethod]
    public void Execute_CommandCanAppendDuringExecution_ShouldRemainConsistent()
    {
        var q = new CommandQueue<SyncItem>();
        var log = new List<string>();
        int once = 0;

        q.Append(new SyncItem("C1", CommandType.Continuous, _ =>
        {
            log.Add("C1");
            if (Interlocked.Exchange(ref once, 1) == 0)
            {
                q.Append(new SyncItem("O1", CommandType.Once, __ => log.Add("O1")));
                q.Append(new SyncItem("O2", CommandType.Once, __ => log.Add("O2")));
            }
        }));

        q.Execute(); // C1 and append O1/O2
        q.Execute(); // O1
        q.Execute(); // O2
        q.Execute(); // C1

        CollectionAssert.AreEqual(new[] { "C1", "O1", "O2", "C1" }, log);
        Assert.AreEqual(1, q.Count); // continuous only
    }

    [TestMethod]
    [Timeout(20000)]
    public void Stress_MixedConcurrentOperations_ShouldStayStable()
    {
        var q = new CommandQueue<SyncItem>();
        var onceMap = new ConcurrentDictionary<int, int>();
        int continuousHits = 0;

        // keep queue always runnable
        q.AppendRange(Enumerable.Range(0, 20)
            .Select(i => new SyncItem($"C{i}", CommandType.Continuous, _ => Interlocked.Increment(ref continuousHits))));

        const int onceTotal = 3000;
        var producer = Task.Run(() =>
        {
            q.AppendRange(Enumerable.Range(0, onceTotal).Select(i =>
                new SyncItem($"O{i}", CommandType.Once, _ => onceMap.AddOrUpdate(i, 1, (_, old) => old + 1))));
        });

        var consumers = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 2500)
            {
                q.Execute();
            }
        })).ToArray();

        Task.WaitAll(consumers.Append(producer).ToArray());

        // once 至多一次
        Assert.IsTrue(onceMap.Values.All(v => v == 1));
        Assert.IsTrue(continuousHits > 0);
        Assert.IsTrue(q.Count >= 20); // continuous still exists
    }
}