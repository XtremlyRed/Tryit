using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class MediaConvertersTests
{
    [TestMethod]
    public void ColorStringConverter_ParsesColor()
    {
        var conv = new ColorStringConverter();
        var method = typeof(ColorStringConverter).GetMethod("ConvertFrom", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method);
        var color = method!.Invoke(conv, new object[] { "#00FF00" });
        Assert.IsInstanceOfType(color, typeof(Color));
        var c = (Color)color!;
        Assert.AreEqual(0, c.R);
        Assert.AreEqual(255, c.G);
    }
}
