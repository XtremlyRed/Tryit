using System.Windows;

namespace Tryit.Wpf;

public class BindingAgent : Freezable
{
    protected override Freezable CreateInstanceCore()
    {
        return new BindingAgent();
    }

    public object DataContext
    {
        get => GetValue(DataContextProperty);
        set => SetValue(DataContextProperty, value);
    }

    public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), typeof(BindingAgent), new PropertyMetadata(null));
}
