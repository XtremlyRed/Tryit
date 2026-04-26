using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest;

[TestClass]
public class SynchronizationContextExtensionsTests
{
    private sealed class TestSynchronizationContext : SynchronizationContext, IDisposable
    {
        private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = new();
        private readonly Thread _thread;
        private readonly ManualResetEventSlim _started = new(false);

        public int ContextThreadId { get; private set; }

        public TestSynchronizationContext()
        {
            _thread = new Thread(Run) { IsBackground = true, Name = "TestSynchronizationContextThread" };
            _thread.Start();
            _started.Wait(TimeSpan.FromSeconds(3));
        }

        private void Run()
        {
            ContextThreadId = Thread.CurrentThread.ManagedThreadId;
            _started.Set();

            foreach (var work in _queue.GetConsumingEnumerable())
            {
                work.Callback(work.State);
            }
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            _queue.Add((d, state));
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _thread.Join(TimeSpan.FromSeconds(3));
            _started.Dispose();
            _queue.Dispose();
        }
    }

    private static async Task AwaitWithTimeout(Task task, int milliseconds = 5000)
    {
        var completed = await Task.WhenAny(task, Task.Delay(milliseconds));
        Assert.AreSame(task, completed, "Operation timed out.");
        await task;
    }

    [TestMethod]
    public void Post_Action_ShouldExecuteOnContextThread()
    {
        using var context = new TestSynchronizationContext();
        var mre = new ManualResetEventSlim(false);
        var invoked = false;
        var threadId = -1;

        context.Post(() =>
        {
            invoked = true;
            threadId = Thread.CurrentThread.ManagedThreadId;
            mre.Set();
        });

        Assert.IsTrue(mre.Wait(TimeSpan.FromSeconds(3)));
        Assert.IsTrue(invoked);
        Assert.AreEqual(context.ContextThreadId, threadId);
    }

    [TestMethod]
    public void Post_T1_ShouldPassParameter_AndExecuteOnContextThread()
    {
        using var context = new TestSynchronizationContext();
        var mre = new ManualResetEventSlim(false);
        var captured = "";
        var threadId = -1;

        context.Post(
            "hello",
            value =>
            {
                captured = value;
                threadId = Thread.CurrentThread.ManagedThreadId;
                mre.Set();
            }
        );

        Assert.IsTrue(mre.Wait(TimeSpan.FromSeconds(3)));
        Assert.AreEqual("hello", captured);
        Assert.AreEqual(context.ContextThreadId, threadId);
    }

    [TestMethod]
    public void Post_T1T2_ShouldPassParameters_AndExecuteOnContextThread()
    {
        using var context = new TestSynchronizationContext();
        var mre = new ManualResetEventSlim(false);
        var sum = 0;
        var threadId = -1;

        context.Post(
            2,
            3,
            (a, b) =>
            {
                sum = a + b;
                threadId = Thread.CurrentThread.ManagedThreadId;
                mre.Set();
            }
        );

        Assert.IsTrue(mre.Wait(TimeSpan.FromSeconds(3)));
        Assert.AreEqual(5, sum);
        Assert.AreEqual(context.ContextThreadId, threadId);
    }

    [TestMethod]
    public async Task PostAsync_Action_ShouldAwaitCompletion_OnContextThread()
    {
        using var context = new TestSynchronizationContext();
        var threadId = -1;

        await context.PostAsync(() =>
        {
            threadId = Thread.CurrentThread.ManagedThreadId;
        });

        Assert.AreEqual(context.ContextThreadId, threadId);
    }

    [TestMethod]
    public async Task PostAsync_Action_T1_ShouldAwaitCompletion_AndPassParameter()
    {
        using var context = new TestSynchronizationContext();
        var captured = "";
        var threadId = -1;

        await context.PostAsync(
            "abc",
            v =>
            {
                captured = v;
                threadId = Thread.CurrentThread.ManagedThreadId;
            }
        );

        Assert.AreEqual("abc", captured);
        Assert.AreEqual(context.ContextThreadId, threadId);
    }

    [TestMethod]
    public async Task PostAsync_Action_T1T2_ShouldAwaitCompletion_AndPassParameters()
    {
        using var context = new TestSynchronizationContext();
        var result = 0;
        var threadId = -1;

        await context.PostAsync(
            7,
            8,
            (a, b) =>
            {
                result = a * b;
                threadId = Thread.CurrentThread.ManagedThreadId;
            }
        );

        Assert.AreEqual(56, result);
        Assert.AreEqual(context.ContextThreadId, threadId);
    }

    [TestMethod]
    public async Task PostAsync_FuncTask_ShouldAwaitInnerTask()
    {
        using var context = new TestSynchronizationContext();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = false;

        var running = context
            .PostAsync(async () =>
            {
                entered = true;
                await gate.Task;
            })
            .AsTask();

        await Task.Delay(100);
        Assert.IsTrue(entered, "Delegate should have started on the context.");
        Assert.IsFalse(running.IsCompleted, "Returned task should wait for inner task completion.");

        gate.SetResult(true);
        await AwaitWithTimeout(running);
    }

    [TestMethod]
    public async Task PostAsync_FuncTask_T1_ShouldPassParameter_AndAwait()
    {
        using var context = new TestSynchronizationContext();
        var captured = "";
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var running = context
            .PostAsync(
                "param",
                async p =>
                {
                    captured = p;
                    await gate.Task;
                }
            )
            .AsTask();

        await Task.Delay(100);
        Assert.AreEqual("param", captured);

        gate.SetResult(true);
        await AwaitWithTimeout(running);
    }

    [TestMethod]
    public async Task PostAsync_FuncTask_T1T2_ShouldPassParameters_AndAwait()
    {
        using var context = new TestSynchronizationContext();
        var calc = 0;
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var running = context
            .PostAsync(
                4,
                5,
                async (a, b) =>
                {
                    calc = a + b;
                    await gate.Task;
                }
            )
            .AsTask();

        await Task.Delay(100);
        Assert.AreEqual(9, calc);

        gate.SetResult(true);
        await AwaitWithTimeout(running);
    }

    [TestMethod]
    public async Task PostAsync_FuncTaskOfT_ShouldReturnResult()
    {
        using var context = new TestSynchronizationContext();

        var result = await context.PostAsync(async () =>
        {
            await Task.Delay(20);
            return 42;
        });

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task PostAsync_FuncTaskOfT_T1_ShouldReturnResult_AndUseParameter()
    {
        using var context = new TestSynchronizationContext();

        var result = await context.PostAsync(
            10,
            async p =>
            {
                await Task.Delay(20);
                return p * 2;
            }
        );

        Assert.AreEqual(20, result);
    }

    [TestMethod]
    public async Task PostAsync_FuncTaskOfT_T1T2_ShouldReturnResult_AndUseParameters()
    {
        using var context = new TestSynchronizationContext();

        var result = await context.PostAsync(
            6,
            7,
            async (a, b) =>
            {
                await Task.Delay(20);
                return a * b;
            }
        );

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task PostAsync_FuncT_ShouldReturnResult_OnContextThread()
    {
        using var context = new TestSynchronizationContext();
        var threadId = -1;

        var result = await context.PostAsync(() =>
        {
            threadId = Thread.CurrentThread.ManagedThreadId;
            return "ok";
        });

        Assert.AreEqual("ok", result);
        Assert.AreEqual(context.ContextThreadId, threadId);
    }

    [TestMethod]
    public async Task PostAsync_FuncT_T1_ShouldReturnResult_AndUseParameter()
    {
        using var context = new TestSynchronizationContext();

        var result = await context.PostAsync(5, x => x + 3);

        Assert.AreEqual(8, result);
    }

    [TestMethod]
    public async Task PostAsync_FuncT_T1T2_ShouldReturnResult_AndUseParameters()
    {
        using var context = new TestSynchronizationContext();

        var result = await context.PostAsync(3, 9, (a, b) => b - a);

        Assert.AreEqual(6, result);
    }

    [TestMethod]
    public void NullContext_ForPostOverloads_ShouldThrowArgumentNullException()
    {
        SynchronizationContext? context = null;

        Assert.ThrowsException<ArgumentNullException>(() => context!.Post(() => { }));
        Assert.ThrowsException<ArgumentNullException>(() => context!.Post(1, _ => { }));
        Assert.ThrowsException<ArgumentNullException>(() => context!.Post(1, 2, (_, _) => { }));
    }

    [TestMethod]
    public async Task NullContext_ForPostAsyncOverloads_ShouldThrowArgumentNullException()
    {
        SynchronizationContext? context = null;

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(() => { }));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(1, _ => { }));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(1, 2, (_, _) => { }));

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(async () => await Task.CompletedTask));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(1, async _ => await Task.CompletedTask));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(1, 2, async (_, _) => await Task.CompletedTask));

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(async () => 1));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(1, async _ => 1));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(1, 2, async (_, _) => 1));

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(() => 1));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(1, _ => 1));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context!.PostAsync(1, 2, (_, _) => 1));
    }

    [TestMethod]
    public void NullAction_ForPostOverloads_ShouldThrowArgumentNullException()
    {
        using var context = new TestSynchronizationContext();

        Assert.ThrowsException<ArgumentNullException>(() => context.Post((Action)null!));
        Assert.ThrowsException<ArgumentNullException>(() => context.Post(1, (Action<int>)null!));
        Assert.ThrowsException<ArgumentNullException>(() => context.Post(1, 2, (Action<int, int>)null!));
    }

    [TestMethod]
    public async Task NullAction_ForPostAsyncOverloads_ShouldThrowArgumentNullException()
    {
        using var context = new TestSynchronizationContext();

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync((Action)null!));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync(1, (Action<int>)null!));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync(1, 2, (Action<int, int>)null!));

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync((Func<Task>)null!));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync(1, (Func<int, Task>)null!));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync(1, 2, (Func<int, int, Task>)null!));

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync((Func<Task<int>>)null!));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync(1, (Func<int, Task<int>>)null!));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync(1, 2, (Func<int, int, Task<int>>)null!));

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync((Func<int>)null!));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync(1, (Func<int, int>)null!));
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await context.PostAsync(1, 2, (Func<int, int, int>)null!));
    }

    [TestMethod]
    public async Task PostAsync_Action_WhenDelegateThrows_ShouldPropagate()
    {
        using var context = new TestSynchronizationContext();

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await context.PostAsync(() => throw new InvalidOperationException("A")));

        Assert.AreEqual("A", ex.Message);
    }

    [TestMethod]
    public async Task PostAsync_FuncTask_WhenDelegateThrows_ShouldPropagate()
    {
        using var context = new TestSynchronizationContext();

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            await context.PostAsync(async () =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("B");
            })
        );

        Assert.AreEqual("B", ex.Message);
    }

    [TestMethod]
    public async Task PostAsync_FuncTaskOfT_WhenDelegateThrows_ShouldPropagate()
    {
        using var context = new TestSynchronizationContext();

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            await context.PostAsync(async () =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("C");
            })
        );

        Assert.AreEqual("C", ex.Message);
    }

    [TestMethod]
    public async Task PostAsync_FuncT_WhenDelegateThrows_ShouldPropagate()
    {
        using var context = new TestSynchronizationContext();

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            await context.PostAsync<int>(async () =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("D");
            })
        );

        Assert.AreEqual("D", ex.Message);
    }
}
