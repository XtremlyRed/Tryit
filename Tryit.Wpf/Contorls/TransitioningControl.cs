using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// A ContentControl that plays a transition animation whenever its <see cref="ContentControl.Content"/> changes.
/// The specific animation is selected via <see cref="TransitionMode"/>. Each concrete (non-random) mode expects
/// a <see cref="Storyboard"/> resource whose key follows the naming convention: "{ModeName}Transition"
/// (e.g. "RightToLeftTransition", "FadeTransition").
/// </summary>
/// <remarks>
/// Requirements:
/// 1. Control template must expose a FrameworkElement named "PATH_Container" which becomes the target root
///    for the storyboard (passed to <see cref="Storyboard.Begin(FrameworkElement)"/>).
/// 2. If a storyboard resource is missing, no exception is thrown; the transition is skipped silently.
/// 3. The <see cref="TransitionMode"/> property is inheritable (FrameworkPropertyMetadataOptions.Inherits)
///    and two-way bindable by default.
/// 4. <see cref="TransitionMode.Random"/> randomly picks a concrete mode (1..9) excluding <see cref="TransitionMode.None"/>.
/// 5. Caching: Located storyboards are cached per <see cref="TransitionMode"/> to avoid repeated resource lookups.
/// </remarks>
[ContentProperty("Content")]
[DefaultProperty("Content")]
public class TransitioningControl : ContentControl
{
    /// <summary>
    /// Template element that hosts the visual content and receives the storyboard animations.
    /// Name in control template: "PATH_Container".
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private FrameworkElement? contentPresenter;

    /// <summary>
    /// Random generator used when <see cref="TransitionMode.Random"/> is selected.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly Random Random = new(Guid.NewGuid().GetHashCode());

    static TransitioningControl()
    {
        // Override the default style key to map to this control's default style.
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitioningControl), new FrameworkPropertyMetadata(typeof(TransitioningControl)));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="TransitioningControl"/>.
    /// </summary>
    public TransitioningControl() { }

    /// <summary>
    /// Gets or sets the corner radius that can be consumed by the control template to round visual edges.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(TransitioningControl),
            new PropertyMetadata(new CornerRadius()));

    /// <summary>
    /// Gets or sets the transition mode used when content changes or when the property itself changes.
    /// Setting this property (after template/application of layout) triggers <see cref="RunTransition"/> if not <see cref="TransitionMode.None"/>.
    /// </summary>
    public TransitionMode TransitionMode
    {
        get => (TransitionMode)GetValue(TransitionModeProperty);
        set => SetValue(TransitionModeProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="TransitionMode"/> dependency property.
    /// Metadata:
    /// - Two-way by default
    /// - Inheritable through the visual/logical tree
    /// - Property changed callback triggers <see cref="RunTransition"/>
    /// </summary>
    public static readonly DependencyProperty TransitionModeProperty = DependencyProperty.Register(
        nameof(TransitionMode),
        typeof(TransitionMode),
        typeof(TransitioningControl),
        new FrameworkPropertyMetadata(
            TransitionMode.Random,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Inherits,
            (d, e) => (d as TransitioningControl)?.RunTransition()
        )
    );

    /// <summary>
    /// Executes one transition attempt:
    /// 1. Verifies layout validity and presence of template target.
    /// 2. Resolves actual mode (handles <see cref="TransitionMode.Random"/>).
    /// 3. Loads storyboard from cache or application resources.
    /// 4. Begins storyboard if available.
    /// If no storyboard is found, nothing is animated and no exception is thrown.
    /// </summary>
    private void RunTransition()
    {
        if (!IsArrangeValid || contentPresenter == null)
            return;

        TransitionMode mode = TransitionMode;

        if (mode == TransitionMode.None)
            return;

        if (mode == TransitionMode.Random)
        {
            // Randomly pick one of the concrete modes (1..9)
            byte value = (byte)Random.Next(1, 10);
            mode = (TransitionMode)value;
        }

        if (mode == TransitionMode.None)
            return;

        if (!storyboardMapper.TryGetValue(mode, out Storyboard? storyboard))
        {
            storyboard = GetResource<Storyboard>($"{mode}Transition");
            storyboardMapper[mode] = storyboard;
        }

        storyboard?.Begin(contentPresenter);
    }

    /// <summary>
    /// Cache mapping from <see cref="TransitionMode"/> to resolved <see cref="Storyboard"/> (may contain null if not found).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never), EditorBrowsable(EditorBrowsableState.Never)]
    private readonly Dictionary<TransitionMode, Storyboard?> storyboardMapper = new();

    /// <summary>
    /// Called when the control template is applied.
    /// Acquires the animation target element named "PATH_Container".
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        contentPresenter = GetTemplateChild("PATH_Container") as FrameworkElement;
    }

    /// <summary>
    /// Attempts to resolve a resource from <see cref="Application.Current"/> by key.
    /// Returns default(T) if the application context is null or the resource is missing.
    /// </summary>
    /// <typeparam name="Target">Expected resource type.</typeparam>
    /// <param name="key">Resource key.</param>
    /// <returns>Resolved resource or default.</returns>
    public static Target? GetResource<Target>(string key)
    {
        return string.IsNullOrEmpty(key) || Application.Current is null
            ? default
            : Application.Current.TryFindResource(key) is Target resource
                ? resource
                : default;
    }

    /// <summary>
    /// Called when <see cref="ContentControl.Content"/> changes.
    /// Triggers a transition if the new content is non-null.
    /// </summary>
    /// <param name="oldContent">Previous content.</param>
    /// <param name="newContent">New content.</param>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (newContent is not null)
            RunTransition();

        base.OnContentChanged(oldContent, newContent);
    }
}

/// <summary>
/// Enumerates transition animation modes used by <see cref="TransitioningControl"/>.
/// </summary>
public enum TransitionMode : byte
{
    /// <summary>
    /// No animation is performed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Slide from right to left.
    /// </summary>
    RightToLeft = 1,

    /// <summary>
    /// Slide from left to right.
    /// </summary>
    LeftToRight = 2,

    /// <summary>
    /// Slide from bottom to top.
    /// </summary>
    BottomToTop = 3,

    /// <summary>
    /// Slide from top to bottom.
    /// </summary>
    TopToBottom = 4,

    /// <summary>
    /// Slide from right to left with fade effect.
    /// </summary>
    RightToLeftWithFade = 5,

    /// <summary>
    /// Slide from left to right with fade effect.
    /// </summary>
    LeftToRightWithFade = 6,

    /// <summary>
    /// Slide from bottom to top with fade effect.
    /// </summary>
    BottomToTopWithFade = 7,

    /// <summary>
    /// Slide from top to bottom with fade effect.
    /// </summary>
    TopToBottomWithFade = 8,

    /// <summary>
    /// Pure fade (opacity) transition.
    /// </summary>
    Fade = 9,

    /// <summary>
    /// Randomly select a concrete transition (one of 1..9) excluding <see cref="None"/>.
    /// </summary>
    Random = 255,
}