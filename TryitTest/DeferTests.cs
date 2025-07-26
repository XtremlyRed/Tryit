using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest
{
    [TestClass]
    public class DeferTests
    {
        [TestMethod]
        public void Deferred_WithTimeSpan_InvokesActionAfterDelay()
        {
            var invoked = false;
            var token = Defer.Deferred(TimeSpan.FromMilliseconds(100)).Invoke(() => invoked = true);

            Thread.Sleep(200);
            Assert.IsTrue(invoked);
            token.Dispose();
        }

        [TestMethod]
        public void Deferred_WithMilliseconds_InvokesActionAfterDelay()
        {
            var invoked = false;
            var token = Defer.Deferred(100).Invoke(() => invoked = true);

            Thread.Sleep(200);
            Assert.IsTrue(invoked);
            token.Dispose();
        }

        [TestMethod]
        public async Task Deferred_AsyncTask_InvokesTaskAfterDelay()
        {
            var invoked = false;
            var token = Defer
                .Deferred(100)
                .Invoke(async () =>
                {
                    await Task.Delay(10);
                    invoked = true;
                });

            await Task.Delay(200);
            Assert.IsTrue(invoked);
            token.Dispose();
        }

        [TestMethod]
        public void Deferred_ActionWithParameter_InvokesWithCorrectValue()
        {
            int result = 0;
            var token = Defer.Deferred(100).Invoke(42, x => result = x);

            Thread.Sleep(200);
            Assert.AreEqual(42, result);
            token.Dispose();
        }

        [TestMethod]
        public async Task Deferred_TaskWithParameter_InvokesWithCorrectValue()
        {
            int result = 0;
            var token = Defer
                .Deferred(100)
                .Invoke(
                    99,
                    async x =>
                    {
                        await Task.Delay(10);
                        result = x;
                    }
                );

            await Task.Delay(200);
            Assert.AreEqual(99, result);
            token.Dispose();
        }

        [TestMethod]
        public void Deferred_ActionWithContext_ProvidesContext()
        {
            IDeferredContext? context = null;
            var token = Defer.Deferred(100).Invoke(ctx => context = ctx);

            Thread.Sleep(200);
            Assert.IsNotNull(context);
            Assert.AreEqual(100, context!.DeferTime.TotalMilliseconds, 1);
            token.Dispose();
        }

        [TestMethod]
        public async Task Deferred_TaskWithContext_ProvidesContext()
        {
            IDeferredContext? context = null;
            var token = Defer
                .Deferred(100)
                .Invoke(async ctx =>
                {
                    await Task.Delay(10);
                    context = ctx;
                });

            await Task.Delay(200);
            Assert.IsNotNull(context);
            Assert.AreEqual(100, context!.DeferTime.TotalMilliseconds, 1);
            token.Dispose();
        }

        [TestMethod]
        public void Deferred_ActionWithContextAndParameter_InvokesCorrectly()
        {
            int result = 0;
            IDeferredContext? context = null;
            var token = Defer
                .Deferred(100)
                .Invoke(
                    77,
                    (ctx, x) =>
                    {
                        context = ctx;
                        result = x;
                    }
                );

            Thread.Sleep(200);
            Assert.IsNotNull(context);
            Assert.AreEqual(77, result);
            token.Dispose();
        }

        [TestMethod]
        public async Task Deferred_TaskWithContextAndParameter_InvokesCorrectly()
        {
            int result = 0;
            IDeferredContext? context = null;
            var token = Defer
                .Deferred(100)
                .Invoke(
                    88,
                    async (ctx, x) =>
                    {
                        await Task.Delay(10);
                        context = ctx;
                        result = x;
                    }
                );

            await Task.Delay(200);
            Assert.IsNotNull(context);
            Assert.AreEqual(88, result);
            token.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Deferred_ThrowsOnZeroTimeSpan()
        {
            Defer.Deferred(TimeSpan.Zero);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Deferred_ThrowsOnNegativeMilliseconds()
        {
            Defer.Deferred(-1);
        }

        [TestMethod]
        public void DeferredToken_Restart_InvokesAgain()
        {
            int count = 0;
            var token = Defer.Deferred(50).Invoke(() => Interlocked.Increment(ref count));

            Thread.Sleep(100);
            Assert.AreEqual(1, count);

            token.Restart();
            Thread.Sleep(100);
            Assert.AreEqual(2, count);

            token.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void DeferredToken_RestartAfterDispose_Throws()
        {
            var token = Defer.Deferred(50).Invoke(() => { });

            token.Dispose();
            token.Restart();
        }
    }
}
