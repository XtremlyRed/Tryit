using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class EnumConvertersTests
{
    private enum LocalEnum { A = 1 }

    [TestMethod]
    public void EnumConverter_ThrowsOnNonEnum()
    {
        var conv = new EnumDescriptionConverter();

        Assert.ThrowsException<InvalidOperationException>(() => ((IValueConverter)conv).Convert("not-enum", typeof(string), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void EnumConverter_CachesValuesAcrossCalls()
    {
        var conv = new EnumDescriptionConverter();

        var first = ((IValueConverter)conv).Convert(LocalEnum.A, typeof(string), null, CultureInfo.InvariantCulture);
        var second = ((IValueConverter)conv).Convert(LocalEnum.A, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.AreEqual(first, second);
        Assert.IsInstanceOfType(first, typeof(string));
    }
}
