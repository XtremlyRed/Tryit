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

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        DataContext = Enumerable.Range(0, Random.Shared.Next(1, 5)).ToArray();

    }
}
