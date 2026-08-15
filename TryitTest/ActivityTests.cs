using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest;

[TestClass]
public class ActivityTests
{
    [TestMethod]
    public void FuncTaskActivity_Constructor_NullDelegate_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new FuncTaskActivity("load", null!));
    }

    [TestMethod]
    public async Task FuncTaskActivity_RunAsync_InvokesDelegateAndPreservesName()
    {
        var context = new Context();
        var invoked = false;
        var activity = new FuncTaskActivity(
            "load",
            async ctx =>
            {
                invoked = true;
                ctx.SetValue("status", "done");
                await Task.Yield();
            }
        );

        await activity.RunAsync(context);

        Assert.AreEqual("load", activity.Name);
        Assert.IsTrue(invoked);
        Assert.AreEqual("done", context.GetValue<string>("status"));
    }

    [TestMethod]
    public void ActionActivity_Constructor_NullDelegate_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new ActionActivity("save", null!));
    }

    [TestMethod]
    public async Task ActionActivity_RunAsync_InvokesDelegateAndReturnsCompletedTask()
    {
        var context = new Context();
        var invoked = false;
        var activity = new ActionActivity(
            "save",
            ctx =>
            {
                invoked = true;
                ctx.SetValue("count", 1);
            }
        );

        var task = activity.RunAsync(context);

        Assert.IsTrue(task.IsCompletedSuccessfully);
        await task;
        Assert.AreEqual("save", activity.Name);
        Assert.IsTrue(invoked);
        Assert.AreEqual(1, context.GetValue<int>("count"));
    }

    [TestMethod]
    public void CompositeActivity_Constructor_NullName_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new CompositeActivity(null!));
    }

    [TestMethod]
    public async Task CompositeActivity_RunAsync_ExecutesChildrenSequentiallyWithSharedContext()
    {
        var composite = new CompositeActivity("pipeline");
        var context = new Context();
        var executionCount = 0;
        var runningCount = 0;
        var maxRunningCount = 0;

        composite.Add(
            new Activity[]
            {
                new FuncTaskActivity(
                    "first",
                    async ctx =>
                    {
                        ctx.SetValue("step-1", "completed");
                        TrackConcurrency(ref runningCount, ref maxRunningCount);
                        Interlocked.Increment(ref executionCount);
                        await Task.Delay(30);
                        Interlocked.Decrement(ref runningCount);
                    }
                ),
                new FuncTaskActivity(
                    "second",
                    async ctx =>
                    {
                        Assert.AreEqual("completed", ctx.GetValue<string>("step-1"));
                        TrackConcurrency(ref runningCount, ref maxRunningCount);
                        Interlocked.Increment(ref executionCount);
                        await Task.Delay(30);
                        Interlocked.Decrement(ref runningCount);
                    }
                ),
                new FuncTaskActivity(
                    "third",
                    async _ =>
                    {
                        TrackConcurrency(ref runningCount, ref maxRunningCount);
                        Interlocked.Increment(ref executionCount);
                        await Task.Delay(30);
                        Interlocked.Decrement(ref runningCount);
                    }
                ),
            }
        );

        await composite.RunAsync(context);

        Assert.AreEqual("pipeline", composite.Name);
        Assert.AreEqual(3, executionCount);
        Assert.AreEqual(1, maxRunningCount);
    }

    [TestMethod]
    public async Task CompositeActivity_Add_DoesNotExecuteDuplicateActivityInstancesMoreThanOnce()
    {
        var composite = new CompositeActivity("pipeline");
        var executionCount = 0;
        var sharedActivity = new ActionActivity("shared", _ => Interlocked.Increment(ref executionCount));

        composite.Add(new Activity[] { sharedActivity, sharedActivity });

        await composite.RunAsync(new Context());

        Assert.AreEqual(1, executionCount);
    }

    [TestMethod]
    public async Task ActivityExtensions_AddAction_WrapsActionActivityAndExecutesIt()
    {
        var composite = new CompositeActivity("pipeline");
        var context = new Context();
        var invoked = false;

        composite.Add(
            "sync-step",
            ctx =>
            {
                invoked = true;
                ctx.SetValue("mode", "sync");
            }
        );

        var child = GetChildActivities(composite).Single();

        Assert.IsInstanceOfType<ActionActivity>(child);
        Assert.AreEqual("sync-step", child.Name);

        await composite.RunAsync(context);

        Assert.IsTrue(invoked);
        Assert.AreEqual("sync", context.GetValue<string>("mode"));
    }

    [TestMethod]
    public async Task ActivityExtensions_AddFuncTask_WrapsFuncTaskActivityAndExecutesIt()
    {
        var composite = new CompositeActivity("pipeline");
        var context = new Context();
        var invoked = false;

        composite.Add(
            "async-step",
            async ctx =>
            {
                invoked = true;
                ctx.SetValue("mode", "async");
                await Task.Yield();
            }
        );

        var child = GetChildActivities(composite).Single();

        Assert.IsInstanceOfType<FuncTaskActivity>(child);
        Assert.AreEqual("async-step", child.Name);

        await composite.RunAsync(context);

        Assert.IsTrue(invoked);
        Assert.AreEqual("async", context.GetValue<string>("mode"));
    }

    [TestMethod]
    public void ActivityExtensions_Add_ValidateArguments()
    {
        var composite = new CompositeActivity("pipeline");

        Assert.ThrowsException<ArgumentNullException>(() => ActivityExtensions.Add(null!, "step", (Action<IContext>)(_ => { })));
        Assert.ThrowsException<ArgumentNullException>(() => ActivityExtensions.Add(composite, null!, (Action<IContext>)(_ => { })));
        Assert.ThrowsException<ArgumentNullException>(() => ActivityExtensions.Add(composite, "step", (Action<IContext>)null!));
        Assert.ThrowsException<ArgumentNullException>(() => ActivityExtensions.Add(null!, "step", (Func<IContext, Task>)(_ => Task.CompletedTask)));
        Assert.ThrowsException<ArgumentNullException>(() => ActivityExtensions.Add(composite, null!, (Func<IContext, Task>)(_ => Task.CompletedTask)));
        Assert.ThrowsException<ArgumentNullException>(() => ActivityExtensions.Add(composite, "step", (Func<IContext, Task>)null!));
    }

    [TestMethod]
    public async Task ParallelActivity_RunAsync_ExecutesChildrenConcurrently()
    {
        const int participantCount = 3;
        var parallel = new ParallelActivity("parallel");
        var allStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var executionCount = 0;
        var startedCount = 0;
        var runningCount = 0;
        var maxRunningCount = 0;

        parallel.Add(new Activity[] { new FuncTaskActivity("first", _ => ExecuteParallelWorkAsync()), new FuncTaskActivity("second", _ => ExecuteParallelWorkAsync()), new FuncTaskActivity("third", _ => ExecuteParallelWorkAsync()) });

        await parallel.RunAsync(new Context());

        Assert.AreEqual("parallel", parallel.Name);
        Assert.AreEqual(participantCount, executionCount);
        Assert.IsTrue(maxRunningCount > 1, "Expected at least two child activities to overlap in execution.");

        async Task ExecuteParallelWorkAsync()
        {
            Interlocked.Increment(ref executionCount);
            TrackConcurrency(ref runningCount, ref maxRunningCount);

            if (Interlocked.Increment(ref startedCount) == participantCount)
            {
                allStarted.TrySetResult(true);
            }

            await allStarted.Task;
            await Task.Delay(30);
            Interlocked.Decrement(ref runningCount);
        }
    }

    [TestMethod]
    public async Task ParallelActivity_RunAsync_PropagatesChildExceptions()
    {
        var parallel = new ParallelActivity("parallel");
        var expected = new InvalidOperationException("boom");

        parallel.Add(new Activity[] { new FuncTaskActivity("fails", _ => Task.FromException(expected)) });

        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await parallel.RunAsync(new Context()));

        Assert.AreSame(expected, exception);
    }

    private static IReadOnlyCollection<Activity> GetChildActivities(CompositeActivity composite)
    {
        var field = typeof(CompositeActivity).GetField("activities", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, "The composite activity backing field should be available for verification.");

        return ((IEnumerable<Activity>)field.GetValue(composite)!).ToArray();
    }

    private static void TrackConcurrency(ref int runningCount, ref int maxRunningCount)
    {
        var current = Interlocked.Increment(ref runningCount);
        int snapshot;

        while (current > (snapshot = maxRunningCount))
        {
            if (Interlocked.CompareExchange(ref maxRunningCount, current, snapshot) == snapshot)
            {
                break;
            }
        }
    }
}
