using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using Tryit;
using Tryit.Wpf;

namespace TryitWpf.Test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly PopupService popupService = new PopupService();

    public MainWindow()
    {
        InitializeComponent();

        BindingOperations.EnableCollectionSynchronization(Array, this);

        DataContext = this;

        ThreadPool.QueueUserWorkItem(async o =>
        {
            await Task.Delay(1000);

            var array = Enumerable.Range(0, 1000000).ToArray();

            for (int i = 0; i < array.Length; i++)
            {
                Array.Add(i);
            }
        });
    }

    public ICommand CompleteCommand =>
        new BindingCommand<int>(i =>
        {
            MessageBox.Show(i.ToString());
        });

    public ObservableCollection<int> Array { get; } = new ObservableCollection<int>();
}
