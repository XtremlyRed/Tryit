using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class PopupServiceTests
{
    [TestMethod]
    public void AttachedProperties_SetAndGetPopupTemplate_Work()
    {
        var decorator = new AdornerDecorator();
        var template = new DataTemplate();

        // Set and get PopupTemplate
        PopupService.SetPopupTemplate(decorator, template);
        var got = PopupService.GetPopupTemplate(decorator);

        Assert.AreSame(template, got);
    }

    [TestMethod]
    public void HostedName_RegistersHostedStorageEntry()
    {
        var decorator = new AdornerDecorator();
        var hostedName = "test-host";

        PopupService.SetHostedName(decorator, hostedName);

        // Access private hostedStorages
        var field = typeof(PopupService).GetField("hostedStorages", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(field);

        var dict = (ConcurrentDictionary<string, object>)field!.GetValue(null)!;

        Assert.IsTrue(dict.ContainsKey(hostedName));

        // Set as main hosted and verify attached property
        PopupService.SetIsMainHosted(decorator, true);
        Assert.IsTrue(PopupService.GetIsMainHosted(decorator));
    }

    [TestMethod]
    public void GetMainHost_ThrowsWhenNoHostConfigured()
    {
        var svc = new PopupService();

        Assert.ThrowsExceptionAsync<InvalidOperationException>(() => svc.ConfirmAsync("hello"));
    }
}
