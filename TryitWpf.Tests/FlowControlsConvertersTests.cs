using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class FlowControlsConvertersTests
{
    [TestMethod]
    public void If_And_Switch_Converters()
    {
        var iff = new If { True = "T", False = "F" };
        Assert.AreEqual("T", ((IValueConverter)iff).Convert(true, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("F", ((IValueConverter)iff).Convert(false, typeof(object), null, CultureInfo.InvariantCulture));

        var sw = new Switch();
        sw.Cases.Add(new Case { Input = 1, Value = "one" });
        sw.DefaultValue = "def";
        Assert.AreEqual("one", ((IValueConverter)sw).Convert(1, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("def", ((IValueConverter)sw).Convert(2, typeof(object), null, CultureInfo.InvariantCulture));
    }
}
