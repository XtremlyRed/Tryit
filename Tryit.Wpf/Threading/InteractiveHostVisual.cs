using System.Windows.Media;

namespace Tryit.Wpf.Threading;

/// <summary>
/// Represents a visual host that enables interactive content to be displayed within a visual tree.
/// </summary>
/// <remarks>Use this class to host interactive visuals, such as those requiring user input or dynamic updates,
/// within a WPF visual tree. This is typically used in advanced scenarios where separation between the visual tree and
/// the interactive content is required.</remarks>
public class InteractiveHostVisual : HostVisual { }
