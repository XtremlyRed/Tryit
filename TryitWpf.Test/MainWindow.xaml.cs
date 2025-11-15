using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using Tryit.Wpf;

namespace TryitWpf.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PopupService popupService = new PopupService();

        public MainWindow()
        {
            var a = new int();

            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // var behaviorCollection = (BehaviorCollection)Activator.CreateInstance(typeof(BehaviorCollection), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture)!;

            //await Task.Delay(2000);

            AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => typeof(AnimationTimeline).IsAssignableFrom(x)).Select(x => x.Name).ToList().ForEach(x => Debug.WriteLine(x));

            //Dispatcher.InvokeAsync(async () =>
            //{
            //    await Task.Delay(2000);

            TestGrid
                .Animation() //
                .BrushProperty(x => x.Background)
                .From(Colors.Green)
                .To(Colors.DeepSkyBlue)
                .Duration(5000)
                .Build()
                .Play();

            //RegisterName("back", TestGrid.Background);

            //ColorAnimation colorAnimation = new ColorAnimation
            //{
            //    From = Colors.Black,
            //    To = Colors.Red,
            //    Duration = TimeSpan.FromSeconds(2)
            //};

            //Storyboard.SetTargetName(colorAnimation, "back");
            //Storyboard.SetTargetProperty(colorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));

            //Storyboard storyboard = new Storyboard();
            //storyboard.Children.Add(colorAnimation);
            //storyboard.Begin( this);

            //TestGrid.BeginAnimation(i => i.BrushProperty(x => x.Background).From(Colors.Green).To(Colors.Red).Duration(2000));

            //await popupService.ShowAsync("test");
        }
    }
}
