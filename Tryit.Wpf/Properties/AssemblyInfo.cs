global using System;
global using System.ComponentModel;
global using static System.Diagnostics.DebuggerBrowsableState;
global using System.Linq;
global using System.Runtime.InteropServices;
global using System.Windows.Controls.Primitives;
global using System.Windows.Data;
global using System.Windows.Markup;
global using Application = System.Windows.Application;
global using BF = System.Reflection.BindingFlags;
global using Binding = System.Windows.Data.Binding;
global using Brush = System.Windows.Media.Brush;
global using Brushes = System.Windows.Media.Brushes;
global using Button = System.Windows.Controls.Button;
global using Color = System.Windows.Media.Color;
global using ComboBox = System.Windows.Controls.ComboBox;
global using DataGrid = System.Windows.Controls.DataGrid;
global using DataGridColumn = System.Windows.Controls.DataGridColumn;
global using DBA = System.Diagnostics.DebuggerBrowsableAttribute;
global using HorizontalAlignment = System.Windows.HorizontalAlignment;
global using MouseEventArgs = System.Windows.Input.MouseEventArgs;
global using Panel = System.Windows.Controls.Panel;
global using Point = System.Windows.Point;
global using Size = System.Windows.Size;
global using TextBox = System.Windows.Controls.TextBox;
global using TriggerCollection = Microsoft.Xaml.Behaviors.TriggerCollection;
global using UserContorl = System.Windows.Controls.UserControl;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/xaml/behaviors", "Tryit.Wpf")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Tryit.Wpf")]
[assembly: XmlnsDefinition("https://github.com/xtremlyred/Tryit.Wpf", "Tryit.Wpf")]
[assembly: XmlnsPrefix("Tryit.Wpf", "tryit")]

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}
