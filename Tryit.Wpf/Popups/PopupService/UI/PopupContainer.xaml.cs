using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tryit.Wpf;

namespace Tryit.Wpf.Services.PopupService;

/// <summary>
/// PopupContainer is a UserControl that initializes a color animation on load. It adjusts the alignment of a panel
/// based on its child count.
/// </summary>
internal partial class PopupContainer : UserControl
{
    /// <summary>
    /// Constructor for the PopupContainer class. Initializes components and subscribes to the Loaded event.
    /// </summary>
    internal PopupContainer()
    {
        InitializeComponent();

        Loaded += PopupContainer_Loaded;
    }

    /// <summary>
    /// Handles the Loaded event for the popup container, initiating a color animation for the background.
    /// </summary>
    /// <param name="sender">Represents the source of the event that triggered the loading of the popup container.</param>
    /// <param name="e">Contains event data related to the loading of the popup container.</param>
    private void PopupContainer_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PopupContainer_Loaded;

        var timeSpan = TimeSpan.FromMilliseconds(500);

        ColorAnimation colorAnimation = new ColorAnimation() { To = Color.FromArgb(80, 0, 0, 0), Duration = timeSpan };

        ContainerBackground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
    }

    /// <summary>
    /// Handles the Loaded event for a button container, adjusting its alignment based on the number of child elements.
    /// </summary>
    /// <param name="sender">Represents the source of the event, which is expected to be a UI panel.</param>
    /// <param name="e">Contains event data related to the Loaded event, providing additional context for the event handling.</param>
    private void Btn_Container_Loaded(object sender, RoutedEventArgs e)
    {
        var panel = sender as Panel;
        if (panel!.Children.Count <= 2)
        {
            panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        }
    }

    /// <summary>
    /// Handles click events for a UI element.
    /// </summary>
    /// <param name="sender">Represents the source of the click event.</param>
    /// <param name="e">Contains data related to the routed event.</param>
    private void Click(object sender, RoutedEventArgs e) { }

    /// <summary>
    /// Handles the Loaded event for a button, allowing for initialization or setup when the button is ready for
    /// interaction.
    /// </summary>
    /// <param name="sender">Represents the source of the event, typically the button that has been loaded.</param>
    /// <param name="e">Contains event data related to the Loaded event, providing additional information about the event.</param>
    private void Button_Loaded(object sender, RoutedEventArgs e) { }
}
