using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest.Utils;

[TestClass]
public class XTimerTests
{
    private static string NewToken() => $"XTimerTests-{Guid.NewGuid():N}";

    private static void CleanupToken(string token)
    {
        try
        {
            _ = XTimer.GetAnchor(token);
        }
        catch
        {
            // ignore cleanup failures
        }
    }

    [TestMethod]
    public void SetAnchor_WithoutToken_ShouldReturnValidAnchor()
    {
        var anchor = XTimer.SetAnchor();

        Assert.IsTrue(anchor.Elapsed >= TimeSpan.Zero);

        var changed = SpinWait.SpinUntil(() => anchor.Elapsed > TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        Assert.IsTrue(changed, "Elapsed should move forward over time.");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetAnchor_WithNullToken_ShouldThrow()
    {
        _ = XTimer.SetAnchor(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetAnchor_WithNullToken_ShouldThrow()
    {
        _ = XTimer.GetAnchor(null!);
    }

    [TestMethod]
    public void SetAnchor_WithSameTokenTwice_ShouldThrowInvalidOperationException()
    {
        var token = NewToken();

        try
        {
            _ = XTimer.SetAnchor(token);

            var ex = Assert.ThrowsException<InvalidOperationException>(() => XTimer.SetAnchor(token));
            StringAssert.Contains(ex.Message, token);
        }
        finally
        {
            CleanupToken(token);
        }
    }

    [TestMethod]
    public void GetAnchor_WithUnknownToken_ShouldThrowInvalidOperationException()
    {
        var token = NewToken();

        var ex = Assert.ThrowsException<InvalidOperationException>(() => XTimer.GetAnchor(token));
        StringAssert.Contains(ex.Message, token);
    }

    [TestMethod]
    public void GetAnchor_ShouldRemoveAnchorAfterRetrieval()
    {
        var token = NewToken();

        _ = XTimer.SetAnchor(token);
        _ = XTimer.GetAnchor(token);

        Assert.ThrowsException<InvalidOperationException>(() => XTimer.GetAnchor(token));
    }

    [TestMethod]
    public void SetAnchor_WithProvidedAnchor_ShouldStoreProvidedAnchor()
    {
        var token = NewToken();
        var provided = XTimer.SetAnchor();

        try
        {
            Thread.Sleep(20);
            _ = XTimer.SetAnchor(token, provided);

            var retrieved = XTimer.GetAnchor(token);
            var delta = (retrieved.Elapsed - provided.Elapsed).Duration();

            Assert.IsTrue(delta < TimeSpan.FromMilliseconds(20), $"Elapsed delta was too large: {delta}.");
        }
        finally
        {
            CleanupToken(token);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetDecayAnchor_WithZeroMilliseconds_ShouldThrow()
    {
        _ = XTimer.SetDecayAnchor(0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetDecayAnchor_WithNegativeMilliseconds_ShouldThrow()
    {
        _ = XTimer.SetDecayAnchor(-1);
    }

    [TestMethod]
    public void SetDecayAnchor_WithPositiveMilliseconds_ShouldReturnValidDecayAnchor()
    {
        var decay = XTimer.SetDecayAnchor(100);

        Assert.IsTrue(decay.Elapsed >= TimeSpan.Zero);
        Assert.IsTrue(decay.ElapsedMilliseconds >= 0);
        Assert.IsTrue(decay.RemainingMilliseconds >= 0);
        Assert.IsTrue(decay.RemainingMilliseconds <= 100);
        Assert.IsTrue(decay.Remaining >= TimeSpan.Zero);
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task SetAnchor_ConcurrentUniqueTokens_ShouldAllBeRetrievable()
    {
        const int total = 300;
        var tokens = Enumerable.Range(0, total).Select(_ => NewToken()).ToArray();

        await Task.WhenAll(tokens.Select(token => Task.Run(() => XTimer.SetAnchor(token))));

        var retrieved = new ConcurrentBag<TimeAnchor>();
        await Task.WhenAll(tokens.Select(token => Task.Run(() => retrieved.Add(XTimer.GetAnchor(token)))));

        Assert.AreEqual(total, retrieved.Count);
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task SetAnchor_ConcurrentSameToken_ShouldOnlyOneSucceed()
    {
        var token = NewToken();
        const int workers = 40;
        int success = 0;
        int failure = 0;

        try
        {
            await Task.WhenAll(
                Enumerable
                    .Range(0, workers)
                    .Select(_ =>
                        Task.Run(() =>
                        {
                            try
                            {
                                var as11 = XTimer.SetAnchor(token);
                                Interlocked.Increment(ref success);
                            }
                            catch (InvalidOperationException)
                            {
                                Interlocked.Increment(ref failure);
                            }
                        })
                    )
            );

            Assert.AreEqual(1, success);
            Assert.AreEqual(workers - 1, failure);
        }
        finally
        {
            CleanupToken(token);
        }
    }
}
