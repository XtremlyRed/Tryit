using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

[ContentProperty("Content")]
[DefaultProperty("Content")]
public class TransitioningControl : ContentControl
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private FrameworkElement? contentPresenter;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly Random Random = new(Guid.NewGuid().GetHashCode());

    static TransitioningControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitioningControl), new FrameworkPropertyMetadata(typeof(TransitioningControl)));
    }

    /// <summary>
    /// TransitioningControl
    /// </summary>
    public TransitioningControl() { }

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(TransitioningControl), new PropertyMetadata(new CornerRadius()));

    /// <summary>
    /// TransitionModeProperty
    /// </summary>
    public static DependencyProperty TransitionModeProperty = DependencyProperty.Register(
        nameof(TransitionMode),
        typeof(TransitionMode),
        typeof(TransitioningControl),
        new FrameworkPropertyMetadata(TransitionMode.Random, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Inherits, (s, e) => (s as TransitioningControl)?.RunTransition())
    );

    /// <summary>
    /// TransitionMode
    /// </summary>
    public TransitionMode TransitionMode
    {
        get => (TransitionMode)GetValue(TransitionModeProperty);
        set => SetValue(TransitionModeProperty, value);
    }

    private void RunTransition()
    {
        if (!IsArrangeValid || contentPresenter == null)
        {
            return;
        }

        TransitionMode mode = TransitionMode;

        if (mode == TransitionMode.None)
        {
            return;
        }

        if (mode == TransitionMode.Random)
        {
            byte value = (byte)Random.Next(1, 10);
            mode = (TransitionMode)value;
        }

        if (mode == TransitionMode.None)
        {
            return;
        }

        if (storyboardMapper.TryGetValue(mode, out Storyboard? storyboard) == false)
        {
            storyboardMapper[mode] = storyboard = GetResource<Storyboard>($"{mode}Transition");
        }

        storyboard?.Begin(contentPresenter);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never), EditorBrowsable(EditorBrowsableState.Never)]
    private readonly Dictionary<TransitionMode, Storyboard?> storyboardMapper = new();

    /// <summary>
    /// <seealso cref="OnApplyTemplate"/>
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        contentPresenter = GetTemplateChild("PATH_Container") as FrameworkElement;
    }

    /// <summary>
    ///  Resource Key
    /// </summary>
    /// <typeparam name="Target"></typeparam>
    /// <param name="key"> Resource Key</param>
    /// <returns></returns>
    public static Target? GetResource<Target>(string key)
    {
        return string.IsNullOrEmpty(key) || Application.Current is null ? default
            : Application.Current.TryFindResource(key) is Target resource ? resource
            : default;
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (newContent is not null)
        {
            RunTransition();
        }
        base.OnContentChanged(oldContent, newContent);
    }
}

/// <summary>
/// TransitionMode
/// </summary>
public enum TransitionMode : byte
{
    /// <summary>
    /// None
    /// </summary>
    None = 0,

    /// <summary>
    /// RightToLeft
    /// </summary>
    RightToLeft = 1,

    /// <summary>
    /// LeftToRight
    /// </summary>
    LeftToRight = 2,

    /// <summary>
    /// BottomToTop
    /// </summary>
    BottomToTop = 3,

    /// <summary>
    /// TopToBottom
    /// </summary>
    TopToBottom = 4,

    /// <summary>
    /// RightToLeftWithFade
    /// </summary>
    RightToLeftWithFade = 5,

    /// <summary>
    /// LeftToRightWithFade
    /// </summary>
    LeftToRightWithFade = 6,

    /// <summary>
    /// BottomToTopWithFade
    /// </summary>
    BottomToTopWithFade = 7,

    /// <summary>
    /// TopToBottomWithFade
    /// </summary>
    TopToBottomWithFade = 8,

    /// <summary>
    /// Fade
    /// </summary>
    Fade = 9,

    /// <summary>
    /// Random
    /// </summary>
    Random = 254,
}
