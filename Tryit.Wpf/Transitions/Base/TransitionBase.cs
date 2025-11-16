using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace Tryit.Wpf;

/// <summary>
/// Provides a base class for creating animated transitions on a FrameworkElement using a specified animation type and
/// value type. This class enables the configuration and control of property-based animations triggered by various UI
/// events.
/// </summary>
/// <remarks>TransitionBase<T, TAnimation> is designed for use in WPF applications to facilitate reusable,
/// configurable transitions on UI elements. It supports customization of animation parameters such as duration, delay,
/// easing, and value range, and can be triggered by different events (e.g., Loaded, MouseEnter, GotFocus). Derived
/// classes should implement the AnimationBuild method to define the specific animation behavior. This class is not
/// thread-safe and should be used only on the UI thread.</remarks>
/// <typeparam name="T">The type of the value being animated by the transition.</typeparam>
/// <typeparam name="TAnimation">The type of AnimationTimeline used to perform the animation. Must have a parameterless constructor.</typeparam>
public abstract class TransitionBase<T, TAnimation> : Behavior<FrameworkElement>
    where TAnimation : AnimationTimeline, new()
{
    /// <summary>
    /// Identifies the read-only IsPlaying dependency property key for use within the TransitionBase class and its
    /// derived types.
    /// </summary>
    /// <remarks>This key is used to set the value of the IsPlaying property internally. It should not be used
    /// directly outside of the class or its descendants. To retrieve the IsPlaying property, use the corresponding
    /// DependencyProperty instead.</remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly DependencyPropertyKey IsPlayingPropertyKey = DependencyProperty.RegisterReadOnly("IsPlaying", typeof(bool), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(false));

    /// <summary>
    /// Identifies the IsPlaying dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the IsPlaying property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue,
    /// GetValue, or for property metadata registration.</remarks>
    public static readonly DependencyProperty IsPlayingProperty = IsPlayingPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets a value indicating whether playback is currently in progress.
    /// </summary>
    public bool IsPlaying => (bool)GetValue(IsPlayingProperty);

    /// <summary>
    /// Gets or sets the transition event that triggers a state change.
    /// </summary>
    public TransitionEvent TransitionEvent
    {
        get => (TransitionEvent)GetValue(TransitionEventProperty);
        set => SetValue(TransitionEventProperty, value);
    }

    /// <summary>
    /// Identifies the TransitionEvent dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the TransitionEvent property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when adding property metadata or binding to
    /// the TransitionEvent property in XAML or code.</remarks>
    public static readonly DependencyProperty TransitionEventProperty = DependencyProperty.Register(nameof(TransitionEvent), typeof(TransitionEvent), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(TransitionEvent.Loaded));

    /// <summary>
    /// Gets or sets a value indicating whether playback is active.
    /// </summary>
    public bool Play
    {
        get => (bool)GetValue(PlayProperty);
        set => SetValue(PlayProperty, value);
    }

    /// <summary>
    /// Identifies the Play dependency property, which indicates whether the transition is currently playing.
    /// </summary>
    /// <remarks>This field is used to register and reference the Play property with the Windows Presentation
    /// Foundation (WPF) property system. It is typically used when interacting with property metadata or when calling
    /// methods that require a DependencyProperty identifier.</remarks>
    public static readonly DependencyProperty PlayProperty = DependencyProperty.Register(nameof(Play), typeof(bool), typeof(TransitionBase<T, TAnimation>), new FrameworkPropertyMetadata(false, OnPlayChanged));

    /// <summary>
    /// Gets or sets the starting value of the range or interval.
    /// </summary>
    public T? From
    {
        get => (T?)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    /// <summary>
    /// Identifies the From dependency property, which specifies the starting value of the transition animation.
    /// </summary>
    /// <remarks>This field is used when referencing the From property in property system operations, such as
    /// data binding or property metadata registration.</remarks>
    public static readonly DependencyProperty FromProperty = DependencyProperty.Register(nameof(From), typeof(T?), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the ending value of the animation.
    /// </summary>
    public T? To
    {
        get => (T?)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    /// <summary>
    /// Identifies the To dependency property, which specifies the target value for the transition animation.
    /// </summary>
    /// <remarks>This field is used when referencing the To property in property system operations, such as
    /// data binding or animation. It is typically used in conjunction with methods that interact with dependency
    /// properties.</remarks>
    public static readonly DependencyProperty ToProperty = DependencyProperty.Register(nameof(To), typeof(T?), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the duration of the animation.
    /// </summary>
    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Identifies the Duration dependency property, which specifies the length of time for the transition effect.
    /// </summary>
    /// <remarks>This field is used when referencing the Duration property in property system operations, such
    /// as data binding or animation. The default duration is one second.</remarks>
    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1))));

    /// <summary>
    /// Gets or sets the amount of time to wait before executing the associated action.
    /// </summary>
    public TimeSpan Delay
    {
        get => (TimeSpan)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    /// <summary>
    /// Identifies the Delay dependency property, which specifies the amount of time to wait before starting the
    /// transition animation.
    /// </summary>
    /// <remarks>This field is used when referencing the Delay property in property system operations, such as
    /// data binding or styling. The default value is TimeSpan.Zero, indicating no delay.</remarks>
    public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(nameof(Delay), typeof(TimeSpan), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(TimeSpan.Zero));

    /// <summary>
    /// Gets or sets the ratio at which the animation speed is increased or decreased relative to its normal rate.
    /// </summary>
    /// <remarks>A value greater than 1.0 increases the animation speed, while a value between 0.0 and 1.0
    /// slows it down. A value of 1.0 plays the animation at its normal speed. If the value is null, the default speed
    /// ratio is used.</remarks>
    public double? SpeedRatio
    {
        get => (double?)GetValue(SpeedRatioProperty);
        set => SetValue(SpeedRatioProperty, value);
    }

    /// <summary>
    /// Identifies the SpeedRatio dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the SpeedRatio property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on instances of TransitionBase<T, TAnimation>.</remarks>
    public static readonly DependencyProperty SpeedRatioProperty = DependencyProperty.Register(nameof(SpeedRatio), typeof(double?), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the ratio of time spent decelerating during the animation, relative to its total duration.
    /// </summary>
    /// <remarks>The value must be between 0.0 and 1.0, inclusive. A higher value results in a longer
    /// deceleration phase at the end of the animation. If null, a default deceleration ratio may be used depending on
    /// the animation implementation.</remarks>
    public double? DecelerationRatio
    {
        get => (double?)GetValue(DecelerationRatioProperty);
        set => SetValue(DecelerationRatioProperty, value);
    }

    /// <summary>
    /// Identifies the DecelerationRatio dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the DecelerationRatio property in property
    /// system operations, such as data binding and styling. It is typically used when calling methods that interact
    /// with dependency properties.</remarks>
    public static readonly DependencyProperty DecelerationRatioProperty = DependencyProperty.Register(nameof(DecelerationRatio), typeof(double?), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the point about which the element's render transform is applied.
    /// </summary>
    /// <remarks>The transform origin is specified as a relative point within the element, where (0,0)
    /// represents the top-left corner and (1,1) represents the bottom-right corner. Changing this property affects how
    /// the element is rotated, scaled, or skewed by its render transform.</remarks>
    public Point TransformOrigin
    {
        get => (Point)GetValue(TransformOriginProperty);
        set => SetValue(TransformOriginProperty, value);
    }

    /// <summary>
    /// Identifies the TransformOrigin dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the TransformOrigin property in property system
    /// operations, such as data binding and styling. The TransformOrigin property typically specifies the point about
    /// which a transformation is applied, expressed as a relative coordinate within the element.</remarks>
    public static readonly DependencyProperty TransformOriginProperty = DependencyProperty.Register(nameof(TransformOrigin), typeof(Point), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(new Point(0.5, 0.5)));

    /// <summary>
    /// Gets or sets the easing function that defines the rate of change for the animation.
    /// </summary>
    /// <remarks>The easing function determines how the animation accelerates or decelerates over time.
    /// Assigning a custom easing function allows for non-linear animation effects, such as ease-in, ease-out, or
    /// bounce. If not set, the animation uses a linear progression by default.</remarks>
    public EasingFunction EasingFunction
    {
        get => (EasingFunction)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    /// <summary>
    /// Identifies the EasingFunction dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the EasingFunction property in property system
    /// operations, such as data binding and animation behaviors. It is typically used when interacting with APIs that
    /// require a DependencyProperty identifier.</remarks>
    public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(nameof(EasingFunction), typeof(EasingFunction), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(EasingFunction.None));

    /// <summary>
    /// Gets or sets the easing mode that specifies how the easing function is applied to the animation.
    /// </summary>
    /// <remarks>The easing mode determines whether the easing function is applied at the start, end, or both
    /// ends of the animation. This property affects the acceleration and deceleration behavior of the
    /// animation.</remarks>
    public EasingMode EasingMode
    {
        get => (EasingMode)GetValue(EasingModeProperty);
        set => SetValue(EasingModeProperty, value);
    }

    /// <summary>
    /// Identifies the EasingMode dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the EasingMode property with the Windows
    /// property system. It is typically used when working with property metadata, data binding, or animation behaviors
    /// in XAML.</remarks>
    public static readonly DependencyProperty EasingModeProperty = DependencyProperty.Register(nameof(EasingMode), typeof(EasingMode), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(EasingMode.EaseInOut));

    /// <summary>
    /// Raises the event that occurs when an animation has completed.
    /// </summary>
    /// <remarks>Override this method in a derived class to perform additional actions when the animation
    /// finishes. This method is called after the animation sequence ends.</remarks>
    protected virtual void OnAnimationCompleted() { }

    /// <summary>
    /// When implemented in a derived class, creates and returns a sequence of animation objects to be used in the
    /// animation pipeline.
    /// </summary>
    /// <returns>An enumerable collection of animation objects of type <typeparamref name="TAnimation"/> to be processed. The
    /// collection may be empty if no animations are defined.</returns>
    protected abstract IEnumerable<TAnimation> AnimationGenerate();

    /// <summary>
    /// Configures the specified animation instance with the current settings, such as start and end values, duration,
    /// delay, and easing function.
    /// </summary>
    /// <remarks>Override this method in a derived class to customize how animation properties are applied.
    /// This method is typically called internally during animation setup and is not intended to be called directly by
    /// user code.</remarks>
    /// <param name="animation">The animation instance to configure. Must not be null.</param>
    /// <param name="animationIndex">The zero-based index of the animation being configured. Used to identify the animation in a sequence or
    /// collection.</param>
    protected virtual void ConfigureAnimation(TAnimation animation, int animationIndex)
    {
        if (From is not null)
        {
            TransitionHelpers<T, TAnimation>.FromSetter(animation, From);
        }

        if (To is not null)
        {
            TransitionHelpers<T, TAnimation>.ToSetter(animation, To);
        }

        if (EasingFunction.WithEasing(EasingMode) is IEasingFunction easingFunction)
        {
            TransitionHelpers<T, TAnimation>.EasingFunctionSetter(animation, easingFunction);
        }

        animation.Duration = Duration;

        animation.BeginTime = Delay;

        if (SpeedRatio.HasValue)
        {
            animation.SpeedRatio = SpeedRatio.Value;
        }

        if (DecelerationRatio.HasValue)
        {
            animation.DecelerationRatio = DecelerationRatio.Value;
        }

        AssociatedObject.RenderTransformOrigin = TransformOrigin;
    }

    /// <summary>
    /// Begins playback of the associated animation if it is not already playing.
    /// </summary>
    /// <remarks>If the animation is already in progress, this method has no effect. This method is typically
    /// used to start an animation sequence on the associated object.</remarks>
    private void AnimationPlay()
    {
        if (IsPlaying)
        {
            return;
        }

        if (GetStoryboard(AssociatedObject) is Storyboard storyboard)
        {
            SetValue(IsPlayingPropertyKey, true);

            storyboard?.Begin();
        }
    }

    #region event


    /// <summary>
    /// Attaches the behavior to the associated object and initializes the animation transforms and event handlers.
    /// </summary>
    /// <remarks>This method is called when the behavior is attached to its associated object. It sets up the
    /// necessary transform group for animations and subscribes to the event specified by the AnimationEvent property.
    /// The method also prepares the storyboard and animation sequence for later execution. This method is sealed and
    /// should not be overridden in derived classes.</remarks>
    protected sealed override void OnAttached()
    {
        base.OnAttached();

        TAnimation[] animations = AnimationGenerate().ToArray();

        if (animations is null || animations.Length == 0)
        {
            return;
        }

        if (AssociatedObject.RenderTransform is not TransformGroup transformGroup)
        {
            transformGroup = new TransformGroup();
            transformGroup.Children.Add(AssociatedObject.RenderTransform);
            AssociatedObject.RenderTransform = transformGroup;
        }

        transformGroup.TryAdd<TranslateTransform>();
        transformGroup.TryAdd<ScaleTransform>();
        transformGroup.TryAdd<RotateTransform>();
        transformGroup.TryAdd<SkewTransform>();

        if (TransitionEvent == TransitionEvent.Loaded)
        {
            AssociatedObject.Loaded += OnLoaded;
        }
        else if (TransitionEvent == TransitionEvent.DataContextChanged)
        {
            AssociatedObject.DataContextChanged += OnDataContextChanged;
        }
        else if (TransitionEvent == TransitionEvent.MouseEnter)
        {
            AssociatedObject.MouseEnter += OnMouseEnter;
        }
        else if (TransitionEvent == TransitionEvent.MouseLeave)
        {
            AssociatedObject.MouseLeave += OnMouseLeave;
        }
        else if (TransitionEvent == TransitionEvent.GotFocus)
        {
            AssociatedObject.GotFocus += OnGotFocus;
        }
        else if (TransitionEvent == TransitionEvent.LostFocus)
        {
            AssociatedObject.LostFocus += OnLostFocus;
        }

        Storyboard storyboard = new Storyboard();

        SetStoryboard(AssociatedObject, storyboard);

        for (int i = 0; i < animations.Length; i++)
        {
            ConfigureAnimation(animations[i], i);

            storyboard.Children.Add(animations[i]);
        }

        storyboard.Completed += OnCompleted;
    }

    /// <summary>
    /// Called when the behavior is being detached from its associated object.
    /// </summary>
    /// <remarks>This method unsubscribes from all event handlers and clears any associated storyboard
    /// resources to ensure proper cleanup. Override this method to perform additional cleanup when detaching the
    /// behavior.</remarks>
    protected override void OnDetaching()
    {
        Storyboard storyboard = GetStoryboard(AssociatedObject);

        storyboard.Completed -= OnCompleted;

        storyboard.Children.Clear();

        if (TransitionEvent == TransitionEvent.Loaded)
        {
            AssociatedObject.Loaded -= OnLoaded;
        }
        else if (TransitionEvent == TransitionEvent.DataContextChanged)
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
        }
        else if (TransitionEvent == TransitionEvent.MouseEnter)
        {
            AssociatedObject.MouseEnter -= OnMouseEnter;
        }
        else if (TransitionEvent == TransitionEvent.MouseLeave)
        {
            AssociatedObject.MouseLeave -= OnMouseLeave;
        }
        else if (TransitionEvent == TransitionEvent.GotFocus)
        {
            AssociatedObject.GotFocus -= OnGotFocus;
        }
        else if (TransitionEvent == TransitionEvent.LostFocus)
        {
            AssociatedObject.LostFocus -= OnLostFocus;
        }

        base.OnDetaching();
    }

    /// <summary>
    /// Handles the event that occurs when the data context of the associated object changes.
    /// </summary>
    /// <param name="sender">The source of the event, typically the object whose data context has changed.</param>
    /// <param name="e">The event data that contains information about the data context change.</param>
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        AnimationPlay();
    }

    /// <summary>
    /// Handles the event that occurs when the mouse pointer enters the control's bounds.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control that the mouse pointer entered.</param>
    /// <param name="e">A MouseEventArgs that contains the event data.</param>
    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        AnimationPlay();
    }

    /// <summary>
    /// Handles the event that occurs when the mouse pointer leaves the control.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control from which the mouse pointer has left.</param>
    /// <param name="e">A MouseEventArgs that contains the event data.</param>
    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        AnimationPlay();
    }

    /// <summary>
    /// Handles the event that occurs when the control receives keyboard focus.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control that received focus.</param>
    /// <param name="e">The event data associated with the focus event.</param>
    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        AnimationPlay();
    }

    /// <summary>
    /// Handles the event that occurs when the control loses keyboard focus.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data associated with the lost focus event.</param>
    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        AnimationPlay();
    }

    /// <summary>
    /// Handles the Loaded event to initiate the animation sequence when the control is loaded.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control that has been loaded.</param>
    /// <param name="e">The event data associated with the Loaded event.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AnimationPlay();
    }

    /// <summary>
    /// Handles the completion event for the associated animation sequence.
    /// </summary>
    /// <param name="sender">The source of the event. This parameter is typically the object that raised the event.</param>
    /// <param name="e">An object that contains the event data.</param>
    private void OnCompleted(object? sender, EventArgs e)
    {
        SetValue(IsPlayingPropertyKey, false);

        OnAnimationCompleted();

        if (CompleteCommand is not null && CompleteCommand.CanExecute(CompleteCommandParameter))
        {
            CompleteCommand.Execute(CompleteCommandParameter);
        }
    }

    /// <summary>
    /// Handles changes to the Play attached property and triggers the associated animation when appropriate.
    /// </summary>
    /// <remarks>This method is intended to be used as a property changed callback for the Play attached
    /// property. It initiates the animation only if the target object is a TransitionBase<T, TAnimation> with no easing
    /// function and the new value is true.</remarks>
    /// <param name="d">The object on which the property value has changed. Expected to be a TransitionBase<T, TAnimation> instance.</param>
    /// <param name="e">The event data that contains information about the property change.</param>
    private static void OnPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransitionBase<T, TAnimation> transition && transition.EasingFunction == EasingFunction.None && e.NewValue is bool booValue && booValue)
        {
            transition.AnimationPlay();

            transition.SetCurrentValue(PlayProperty, false);
        }
    }

    #endregion


    #region command

    /// <summary>
    /// Gets or sets the command that is executed when the completion action is triggered.
    /// </summary>
    /// <remarks>This property is typically bound to a command in the view model to handle completion logic,
    /// such as submitting a form or finalizing a process. The command is invoked when the associated completion event
    /// occurs in the UI.</remarks>
    public ICommand CompleteCommand
    {
        get => (ICommand)GetValue(CompleteCommandProperty);
        set => SetValue(CompleteCommandProperty, value);
    }

    /// <summary>
    /// Identifies the CompleteCommand dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the CompleteCommand property in property system
    /// operations, such as data binding and styling. Typically used in custom controls that derive from
    /// TransitionBase<T, TAnimation>.</remarks>
    public static readonly DependencyProperty CompleteCommandProperty = DependencyProperty.Register(nameof(CompleteCommand), typeof(ICommand), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the parameter to pass to the command when the completion action is invoked.
    /// </summary>
    public object CompleteCommandParameter
    {
        get => GetValue(CompleteCommandParameterProperty);
        set => SetValue(CompleteCommandParameterProperty, value);
    }

    /// <summary>
    /// Identifies the CompleteCommandParameter dependency property.
    /// </summary>
    /// <remarks>This field is used to register the CompleteCommandParameter property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when adding property metadata or binding to
    /// the CompleteCommandParameter property in XAML or code.</remarks>
    public static readonly DependencyProperty CompleteCommandParameterProperty = DependencyProperty.Register(nameof(CompleteCommandParameter), typeof(object), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(null));

    #endregion


    #region attach


    /// <summary>
    /// Retrieves the Storyboard associated with the specified dependency object.
    /// </summary>
    /// <param name="obj">The DependencyObject from which to retrieve the associated Storyboard.</param>
    /// <returns>The Storyboard associated with the specified object, or null if no Storyboard is set.</returns>
    private static Storyboard GetStoryboard(DependencyObject obj)
    {
        return (Storyboard)obj.GetValue(StoryboardProperty);
    }

    /// <summary>
    /// Sets the storyboard associated with the specified dependency object.
    /// </summary>
    /// <param name="obj">The dependency object on which to set the storyboard. Cannot be null.</param>
    /// <param name="value">The storyboard to associate with the dependency object. Can be null to clear the current storyboard.</param>
    private static void SetStoryboard(DependencyObject obj, Storyboard value)
    {
        obj.SetValue(StoryboardProperty, value);
    }

    /// <summary>
    /// Identifies the Storyboard attached dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the Storyboard attached property on elements
    /// that support transitions. Use this property to associate a Storyboard with a UI element for animation
    /// purposes.</remarks>
    private static readonly DependencyProperty StoryboardProperty = DependencyProperty.RegisterAttached("Storyboard", typeof(Storyboard), typeof(TransitionBase<T, TAnimation>), new PropertyMetadata(null));

    #endregion
}
