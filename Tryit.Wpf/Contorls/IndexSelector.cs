using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a custom panel that manages the selection and animated transition of its child elements based on an
/// index.
/// </summary>
/// <remarks>The IndexSelector provides animated transitions between its child elements when the selected index
/// changes. It exposes properties to control the selected index, animation offset, and animation duration. This control
/// is typically used to display one child at a time with smooth transitions, such as in tabbed interfaces or carousels.
/// The panel arranges its children and handles their visibility and animation states automatically. Only one child is
/// visible at a time, and changing the SelectedIndex property triggers an animated transition to the corresponding
/// child element.</remarks>
public class IndexSelector : Panel
{
    /// <summary>
    /// Initializes a new instance of the IndexSelector class.
    /// </summary>
    public IndexSelector()
    {
        Loaded += IndexSelector_Loaded;
    }

    /// <summary>
    /// Handles the Loaded event for the index selector control, initializing the selected index and raising the
    /// SelectedIndexChanged event as appropriate.
    /// </summary>
    /// <remarks>This method ensures that the selected index is valid when the control is loaded. If the
    /// selected index is not within the valid range, it resets the selection and raises the SelectedIndexChanged event
    /// after a short delay. This helps synchronize the control's state with its visual representation.</remarks>
    /// <param name="sender">The source of the event, typically the index selector control.</param>
    /// <param name="e">The event data associated with the Loaded event.</param>
    private void IndexSelector_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= IndexSelector_Loaded;

        if (VerifyIn(SelectedIndex, 0, InternalChildren.Count - 1))
        {
            _ = Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, TimeSpan.FromMilliseconds(300), DefaultSelected);
            return;
        }

        SelectedIndex = -1;

        void DefaultSelected()
        {
            DependencyPropertyChangedEventArgs dpca = new(SelectedIndexProperty, -1, SelectedIndex);
            SelectedIndexChanged(this, dpca);
        }
    }

    /// <summary>
    /// Gets or sets the index of the currently selected item.
    /// </summary>
    /// <remarks>A value of -1 indicates that no item is selected. Setting this property to a value less than
    /// -1 or greater than the number of items will result in no selection.</remarks>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// Identifies the SelectedIndex dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the SelectedIndex property with the WPF property
    /// system. It is typically used when calling methods such as SetValue or GetValue on an IndexSelector
    /// instance.</remarks>
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(IndexSelector), new PropertyMetadata(-1, SelectedIndexChanged));

    /// <summary>
    /// Gets or sets the offset value applied to the animation sequence.
    /// </summary>
    /// <remarks>Use this property to adjust the starting point or phase of the animation. The interpretation
    /// of the offset value depends on the specific animation implementation; typically, it represents a time or
    /// position shift.</remarks>
    public double AnimationOffset
    {
        get => (double)GetValue(AnimationOffsetProperty);
        set => SetValue(AnimationOffsetProperty, value);
    }

    /// <summary>
    /// Identifies the AnimationOffset dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the AnimationOffset property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when adding property metadata or binding to
    /// the AnimationOffset property in XAML or code.</remarks>
    public static readonly DependencyProperty AnimationOffsetProperty = DependencyProperty.Register("AnimationOffset", typeof(double), typeof(IndexSelector), new PropertyMetadata(100d));

    /// <summary>
    /// Gets or sets the duration of the animation applied to the control.
    /// </summary>
    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Identifies the AnimationDuration dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the AnimationDuration property in the
    /// IndexSelector control. Dependency properties enable styling, data binding, animation, and default value support
    /// in WPF.</remarks>
    public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(Duration), typeof(IndexSelector), new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(350))));

    /// <summary>
    /// Handles changes to the selected index of an IndexSelector control and triggers the appropriate animation between
    /// the old and new selected elements.
    /// </summary>
    /// <remarks>This method is intended to be used as a property changed callback for the selected index
    /// dependency property of the IndexSelector control. It animates the transition between the previously selected and
    /// newly selected elements if both indices are valid and the control contains children.</remarks>
    /// <param name="d">The dependency object on which the property value has changed. Expected to be an instance of IndexSelector.</param>
    /// <param name="e">The event data that contains information about the property change, including the old and new values of the
    /// selected index.</param>
    private static void SelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not IndexSelector selector || selector.InternalChildren.Count == 0 || e.NewValue is not int newIndex || e.OldValue is not int oldIndex)
        {
            return;
        }

        FrameworkElement? old = default!;
        FrameworkElement? @new = default!;

        if (VerifyIn(oldIndex, 0, selector.InternalChildren.Count - 1))
        {
            old = selector.InternalChildren[oldIndex] as FrameworkElement;
        }

        if (VerifyIn(newIndex, 0, selector.InternalChildren.Count - 1))
        {
            @new = selector.InternalChildren[newIndex] as FrameworkElement;
        }

        Animation(@new, old, selector.AnimationOffset, selector.AnimationDuration.TimeSpan.TotalMilliseconds);
    }

    /// <summary>
    /// Measures the size required for the panel and its child elements, given an upper limit constraint.
    /// </summary>
    /// <remarks>The returned size is determined by the largest DesiredSize of the panel's child elements.
    /// This method is typically called by the layout system during the measure pass.</remarks>
    /// <param name="constraint">The maximum size that the panel and its children can occupy.</param>
    /// <returns>A Size structure representing the width and height required to arrange all child elements within the specified
    /// constraint.</returns>
    protected override Size MeasureOverride(Size constraint)
    {
        Size maxSize = new();

        foreach (UIElement? child in InternalChildren)
        {
            if (child != null)
            {
                child.Measure(constraint);
                maxSize.Width = Math.Max(maxSize.Width, child.DesiredSize.Width);
                maxSize.Height = Math.Max(maxSize.Height, child.DesiredSize.Height);
            }
        }

        return maxSize;
    }

    /// <summary>
    /// Arranges the content of the panel and determines the final size of the element.
    /// </summary>
    /// <remarks>This method is typically called by the layout system and should not be called directly. It
    /// arranges each child element within the specified arrange size.</remarks>
    /// <param name="arrangeSize">The final area within the parent that this element should use to arrange itself and its children.</param>
    /// <returns>The actual size used after arranging the element and its children.</returns>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        foreach (UIElement? child in InternalChildren)
        {
            child?.Arrange(new Rect(arrangeSize));
        }

        return arrangeSize;
    }

    /// <summary>
    /// Responds to changes in the collection of visual child elements by updating the state of added or removed
    /// visuals.
    /// </summary>
    /// <remarks>When a new visual child of type FrameworkElement is added, its transform origin, opacity
    /// mask, and visibility are initialized. Override this method to customize how visual children are handled when
    /// added or removed.</remarks>
    /// <param name="visualAdded">The visual child element that has been added to the collection, or null if no element was added.</param>
    /// <param name="visualRemoved">The visual child element that has been removed from the collection, or null if no element was removed.</param>
    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        if (visualAdded is FrameworkElement element)
        {
            element.RenderTransformOrigin = new Point(0.5, 0.5);

            element.OpacityMask = new SolidColorBrush(Color.FromArgb(0, 0xff, 0xff, 0xff));

            element.Visibility = Visibility.Hidden;

            _ = element.TryAppend(new TranslateTransform(), out _);
        }

        base.OnVisualChildrenChanged(visualAdded, visualRemoved);
    }

    /// <summary>
    /// Animates the transition between two UI elements by sliding one out and the other in, applying translation and
    /// opacity effects over a specified duration.
    /// </summary>
    /// <remarks>If both parameters are null, no animation is performed. The method animates the X and Y
    /// translation and opacity mask of the elements, and updates their visibility accordingly. Both elements must have
    /// a RenderTransform containing a TranslateTransform for the animation to take effect.</remarks>
    /// <param name="displayElement">The element to display with an entrance animation. If null, no element is shown.</param>
    /// <param name="hideElement">The element to hide with an exit animation. If null, no element is hidden.</param>
    /// <param name="translateOffset">The distance, in device-independent pixels, to translate the elements during the animation. Must be a finite
    /// value.</param>
    /// <param name="time_ms">The duration of the animation, in milliseconds. Must be non-negative.</param>
    private static void Animation(FrameworkElement? displayElement, FrameworkElement? hideElement, double translateOffset, double time_ms)
    {
        if (displayElement is null && hideElement is null)
        {
            return;
        }

        Storyboard stoa = new();

        if (hideElement is not null && hideElement.TryIndex<TranslateTransform>(out int index))
        {
            DoubleAnimation doubleAnimation = new() { Duration = TimeSpan.FromMilliseconds(time_ms) };

            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath($"(FrameworkElement.RenderTransform).Children[{index}].(TranslateTransform.X)"));
            Storyboard.SetTarget(doubleAnimation, hideElement);

            doubleAnimation.From = 0;
            doubleAnimation.To = -translateOffset;

            stoa.Children.Add(doubleAnimation);

            ColorAnimation colorAnimation = new() { Duration = TimeSpan.FromMilliseconds(time_ms * 1.5) };

            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(FrameworkElement.OpacityMask).(SolidColorBrush.Color)"));
            Storyboard.SetTarget(colorAnimation, hideElement);

            colorAnimation.From = Color.FromArgb(0xff, 0, 0, 0);
            colorAnimation.To = Color.FromArgb(0, 0, 0, 0);

            stoa.Children.Add(colorAnimation);

            ObjectAnimationUsingKeyFrames animationUsingKeyFrames = new();

            Storyboard.SetTargetProperty(animationUsingKeyFrames, new PropertyPath("Visibility"));
            Storyboard.SetTarget(animationUsingKeyFrames, hideElement);

            _ = animationUsingKeyFrames.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Hidden, TimeSpan.FromMilliseconds(time_ms)));

            stoa.Children.Add(animationUsingKeyFrames);
        }

        if (displayElement is not null && displayElement.TryIndex<TranslateTransform>(out index))
        {
            ObjectAnimationUsingKeyFrames animationUsingKeyFrames = new();

            Storyboard.SetTargetProperty(animationUsingKeyFrames, new PropertyPath("Visibility"));
            Storyboard.SetTarget(animationUsingKeyFrames, displayElement);

            _ = animationUsingKeyFrames.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.Zero));

            stoa.Children.Add(animationUsingKeyFrames);

            DoubleAnimation doubleAnimation1 = new() { Duration = TimeSpan.FromMilliseconds(time_ms) };

            Storyboard.SetTargetProperty(doubleAnimation1, new PropertyPath($"(FrameworkElement.RenderTransform).Children[{index}].(TranslateTransform.X)"));
            Storyboard.SetTarget(doubleAnimation1, displayElement);

            doubleAnimation1.From = translateOffset;
            doubleAnimation1.To = 0;

            stoa.Children.Add(doubleAnimation1);

            DoubleAnimation doubleAnimation2 = new() { Duration = TimeSpan.FromMilliseconds(time_ms) };

            Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath($"(FrameworkElement.RenderTransform).Children[{index}].(TranslateTransform.Y)"));
            Storyboard.SetTarget(doubleAnimation2, displayElement);

            doubleAnimation2.From = translateOffset;
            doubleAnimation2.To = 0;

            stoa.Children.Add(doubleAnimation2);

            ColorAnimation colorAnimation = new() { Duration = TimeSpan.FromMilliseconds(time_ms) };

            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(FrameworkElement.OpacityMask).(SolidColorBrush.Color)"));
            Storyboard.SetTarget(colorAnimation, displayElement);

            colorAnimation.From = Color.FromArgb(0, 0, 0, 0);
            colorAnimation.To = Color.FromArgb(0xff, 0, 0, 0);

            stoa.Children.Add(colorAnimation);
        }

        stoa.Begin();
    }

    /// <summary>
    /// Determines whether a specified value falls within a given range.
    /// </summary>
    /// <remarks>If includeEquals is true, the method returns true when input is equal to minValue or
    /// maxValue. If includeEquals is false, the method returns true only when input is strictly greater than minValue
    /// and less than maxValue.</remarks>
    /// <param name="input">The value to test for inclusion within the range.</param>
    /// <param name="minValue">The lower bound of the range.</param>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <param name="includeEquals">true to include the boundary values in the range comparison; otherwise, false. The default is true.</param>
    /// <returns>true if the input value is within the specified range; otherwise, false.</returns>
    private static bool VerifyIn(int input, int minValue, int maxValue, bool includeEquals = true)
    {
        return includeEquals
            ? (minValue <= input && input <= maxValue) // inculde equals
            : (minValue < input && input < maxValue);
    }
}
