using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class NotificationServiceTests
{
    [TestMethod]
    public void NotificationTemplate_SetAndGet_Works()
    {
        var decorator = new AdornerDecorator();
        var template = new DataTemplate();

        NotificationService.SetNotificationTemplate(decorator, template);
        var got = NotificationService.GetNotificationTemplate(decorator);

        Assert.AreSame(template, got);
    }

    [TestMethod]
    public void HostedName_RegistersHostedStorageEntry_ForNotification()
    {
        var decorator = new AdornerDecorator();
        var hostedName = "notify-host";

        NotificationService.SetHostedName(decorator, hostedName);

        var field = typeof(NotificationService).GetField("hostedStorages", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(field);

        var dict = (ConcurrentDictionary<string, object>)field!.GetValue(null)!;

        Assert.IsTrue(dict.ContainsKey(hostedName));
    }

    [TestMethod]
    public void GetMainHost_ThrowsWhenNotConfigured()
    {
        var svc = new NotificationService();

        Assert.ThrowsExceptionAsync<InvalidOperationException>(() => svc.NotifyAsync("hi", TimeSpan.FromMilliseconds(10)));
    }
}
