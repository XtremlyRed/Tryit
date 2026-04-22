using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;

using Tryit.Wpf;

namespace TryitWpf.Test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly PopupService popupService = new();

    public MainWindow()
    {
        InitializeComponent();

        ConcurrentBag<int> bag = new(Enumerable.Range(0, 10));


        var count = 0;

       


        while (count++<100)
        {
            if(bag.TryTake(out var value))
            {
                Debug.WriteLine(value);

                bag.Add(value);

            }
        }



    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        DataContext = Enumerable.Range(0, Random.Shared.Next(1, 5)).ToArray();

    }
}
