using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Represents a grid layout panel that automatically determines the number of columns based on the available width and
/// a specified item width.
/// </summary>
/// <remarks>ColumnGrid extends UniformGrid to provide automatic column calculation, making it suitable for
/// scenarios where items should be arranged in a uniform grid with a dynamic number of columns. The number of columns
/// is recalculated whenever the control's size or the item width changes. This control is typically used in user
/// interfaces where items need to be displayed in a responsive, grid-like arrangement without manually specifying the
/// column count.</remarks>
public class ColumnGrid : UniformGrid
{

    /// <summary>
    /// Gets or sets the width, in device-independent units (1/96th inch per unit), of each item in the control.
    /// </summary>
    /// <remarks>Set this property to specify a fixed width for all items. If not set, the item width may be
    /// determined automatically based on the content or layout settings.</remarks>
    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>
    /// Identifies the ItemWidth dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the ItemWidth property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on instances of AutoColumnGrid.</remarks>
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(ColumnGrid), new PropertyMetadata(300d, OnItemWidthChanged));

    /// <summary>
    /// Handles changes to the ItemWidth dependency property by updating the layout of the associated AutoColumnGrid.
    /// </summary>
    /// <remarks>This method is typically used as a property changed callback for the ItemWidth dependency
    /// property. It ensures that the grid layout is updated whenever the ItemWidth value changes.</remarks>
    /// <param name="d">The object on which the property value has changed. Expected to be an instance of AutoColumnGrid.</param>
    /// <param name="e">The event data that contains information about the property change.</param>
    private static void OnItemWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColumnGrid auto)
        {
            auto.Rerender(auto.RenderSize);
        }
    }

    /// <summary>
    /// Handles changes to the rendered size of the control.
    /// </summary>
    /// <remarks>Override this method to respond to changes in the control's rendered size. This method is
    /// called whenever the layout system detects a change in the size of the control.</remarks>
    /// <param name="sizeInfo">Information about the size changes, including the previous and new size of the control.</param>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        Rerender(sizeInfo.NewSize);
    }

    /// <summary>
    /// Recalculates and updates the number of columns based on the specified available size and the current item width.
    /// </summary>
    /// <remarks>If the item width is not set to a valid value, the method does not update the column count.
    /// The number of columns is always at least one.</remarks>
    /// <param name="size">The available size used to determine the appropriate number of columns. The width component is used in the
    /// calculation.</param>
    private void Rerender(Size size)
    {
        if (ItemWidth is double.NaN)
        {
            return;
        }

        int columnCount = (int)Math.Round(size.Width / ItemWidth, 0);

        columnCount = columnCount <= 1 ? 1 : columnCount;

        if (base.Columns != columnCount)
        {
            base.Columns = columnCount;
        }
    }

    /// <summary>
    /// Gets the number of columns in the current context.
    /// </summary>
    public new int Columns => base.Columns;

    /// <summary>
    /// Gets the number of rows in the collection.
    /// </summary>
    public new int Rows => base.Rows;
}
