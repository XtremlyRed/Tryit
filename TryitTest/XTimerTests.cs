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
    public void TimeAnchor_ElapsedMilliseconds_ImmediatelyAfterCreation_ShouldBeNonNegative()
    {
        var anchor = XTimer.SetAnchor();

        Assert.IsTrue(anchor.ElapsedMilliseconds >= 0);
    }

    [TestMethod]
    public void TimeAnchor_ElapsedMilliseconds_AfterDelay_ShouldBeGreaterThanZero()
    {
        var anchor = XTimer.SetAnchor();

        Thread.Sleep(30);

        Assert.IsTrue(anchor.ElapsedMilliseconds > 0, $"Expected elapsed milliseconds to be greater than zero after delay, but got {anchor.ElapsedMilliseconds}.");
    }

    [TestMethod]
    public void TimeAnchor_ElapsedMilliseconds_ShouldBeMonotonic()
    {
        var anchor = XTimer.SetAnchor();

        long first = anchor.ElapsedMilliseconds;
        Thread.Sleep(15);
        long second = anchor.ElapsedMilliseconds;
        Thread.Sleep(15);
        long third = anchor.ElapsedMilliseconds;

        Assert.IsTrue(second >= first, $"Expected second value to be >= first. first={first}, second={second}.");
        Assert.IsTrue(third >= second, $"Expected third value to be >= second. second={second}, third={third}.");
    }

    [TestMethod]
    public void TimeAnchor_ElapsedMilliseconds_ShouldRoughlyMatchElapsedTotalMilliseconds()
    {
        var anchor = XTimer.SetAnchor();

        Thread.Sleep(40);

        long elapsedMilliseconds = anchor.ElapsedMilliseconds;
        double elapsedFromTimeSpan = anchor.Elapsed.TotalMilliseconds;

        Assert.IsTrue(
            Math.Abs(elapsedMilliseconds - elapsedFromTimeSpan) <= 20,
            $"Expected ElapsedMilliseconds ({elapsedMilliseconds}) to roughly match Elapsed.TotalMilliseconds ({elapsedFromTimeSpan})."
        );
    }

    [TestMethod]
    public void TimeAnchor_StartNew_ElapsedMilliseconds_AfterDelay_ShouldBeGreaterThanZero()
    {
        var anchor = TimeAnchor.StartNew();

        Thread.Sleep(30);

        Assert.IsTrue(anchor.ElapsedMilliseconds > 0, $"Expected elapsed milliseconds to be greater than zero after delay, but got {anchor.ElapsedMilliseconds}.");
    }

    [TestMethod]
    public void TimeAnchor_ElapsedMilliseconds_OlderAnchor_ShouldBeGreaterThanNewerAnchor()
    {
        var older = XTimer.SetAnchor();
        Thread.Sleep(25);
        var newer = XTimer.SetAnchor();
        Thread.Sleep(15);

        Assert.IsTrue(
            older.ElapsedMilliseconds > newer.ElapsedMilliseconds,
            $"Expected older anchor elapsed milliseconds ({older.ElapsedMilliseconds}) to be greater than newer anchor ({newer.ElapsedMilliseconds})."
        );
    }

    [TestMethod]
    public void TimeAnchor_ElapsedMilliseconds_FromRetrievedAnchor_ShouldContinueFromOriginalAnchorTime()
    {
        var token = NewToken();
        var original = XTimer.SetAnchor();

        try
        {
            Thread.Sleep(20);
            _ = XTimer.SetAnchor(token, original);
            Thread.Sleep(20);

            var retrieved = XTimer.GetAnchor(token);

            Assert.IsTrue(retrieved.ElapsedMilliseconds >= original.ElapsedMilliseconds - 10,
                $"Expected retrieved elapsed milliseconds ({retrieved.ElapsedMilliseconds}) to reflect original anchor lifetime ({original.ElapsedMilliseconds}).");
        }
        finally
        {
            CleanupToken(token);
        }
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
        _ = XTimer.SetCountdownAnchor(0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetDecayAnchor_WithNegativeMilliseconds_ShouldThrow()
    {
        _ = XTimer.SetCountdownAnchor(-1);
    }

    [TestMethod]
    public void SetDecayAnchor_WithPositiveMilliseconds_ShouldReturnValidDecayAnchor()
    {
        var decay = XTimer.SetCountdownAnchor(100);

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
