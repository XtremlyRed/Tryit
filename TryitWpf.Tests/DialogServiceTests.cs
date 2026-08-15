using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class DialogServiceTests
{
    private class StubDialogAware : IDialogAware
    {
        public bool OpenedCalled;
        public bool ClosedCalled;
        public object? LastParameter;

        public event Action<object>? RequestCloseEvent;

        public void Opened(DialogParameter? parameter)
        {
            OpenedCalled = true;
            LastParameter = parameter;
        }

        public void Closed()
        {
            ClosedCalled = true;
        }

        public void RaiseRequestClose(object? obj) => RequestCloseEvent?.Invoke(obj!);
    }

    [TestMethod]
    public void SetDialogWindiw_Null_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => DialogService.SetDialogWindiw(null!));
    }

    //[TestMethod]
    //public void InnerInit_SetsContentAndCallsOpenedWhenDataContextImplementsIDialogAware()
    //{
    //    // Arrange
    //    var dialogService = new DialogService();
    //    var win = new Window();
    //    DialogService.SetDialogWindiw(win);

    //    var aware = new StubDialogAware();
    //    var visual = new UserControl { DataContext = aware };
    //    var param = new DialogParameter();

    //    // Use reflection to call private InnerInit
    //    var method = typeof(DialogService).GetMethod("InnerInit", BindingFlags.NonPublic | BindingFlags.Instance);
    //    Assert.IsNotNull(method, "InnerInit method should exist");

    //    // Act
    //    var returnedWindow = (Window)method!.Invoke(dialogService, new object[] { visual, param })!;

    //    // Assert
    //    Assert.AreSame(win, returnedWindow, "InnerInit should return the static dialog window instance");
    //    Assert.AreSame(visual, win.Content, "Dialog window content should be set to the provided visual");
    //    Assert.IsTrue(aware.OpenedCalled, "IDialogAware.Opened should be invoked during InnerInit");
    //    Assert.AreSame(param, aware.LastParameter);
    //}
}
