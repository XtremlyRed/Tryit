using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class ComparesConvertersTests
{
    [TestMethod]
    public void CompareConverter_MatchStaticHelper()
    {
        Assert.IsTrue(CompareConverter.Match(5, 5, CompareMode.Equal));
        Assert.IsFalse(CompareConverter.Match(5, 6, CompareMode.Equal));
        Assert.IsTrue(CompareConverter.Match(6, 5, CompareMode.GreaterThan));
        Assert.IsTrue(CompareConverter.Match(5, 5, CompareMode.GreaterThanOrEqual));
        Assert.IsTrue(CompareConverter.Match(4, 5, CompareMode.LessThan));
    }
}
