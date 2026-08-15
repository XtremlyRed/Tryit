using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class RangesConvertersTests
{
    [TestMethod]
    public void InRangeConverter_BehaviorDifferentIncludeEquals()
    {
        var r = new InRangeConverter { MinValue = 1, MaxValue = 10, IncludeEquals = true };
        Assert.AreEqual(true, ((IValueConverter)r).Convert(1, typeof(object), null, CultureInfo.InvariantCulture));
        r.IncludeEquals = false;
        Assert.AreEqual(false, ((IValueConverter)r).Convert(1, typeof(object), null, CultureInfo.InvariantCulture));
    }
}
