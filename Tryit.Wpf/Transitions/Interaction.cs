using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Tryit.Wpf;

/// <summary>
/// Provides static methods for attaching and retrieving collections of transition behaviors to dependency objects at
/// runtime.
/// </summary>
/// <remarks>The Interaction class enables the dynamic association of BehaviorCollection instances with dependency
/// objects, allowing behaviors to be added, removed, or managed without modifying the object's code. This is commonly
/// used in UI frameworks that support behaviors and transitions, such as WPF or similar XAML-based technologies. All
/// members are static and thread safety depends on the usage of the attached objects and collections.</remarks>
public static class Interaction
{
    /// <summary>
    /// Identifies the ShadowTransitions attached dependency property, which is used to associate a collection of
    /// behaviors with a dependency object.
    /// </summary>
    /// <remarks>This field is typically used when registering or accessing the ShadowTransitions attached
    /// property on elements that support dependency properties. It enables the storage and retrieval of a
    /// BehaviorCollection for a given object, allowing behaviors to be dynamically attached at runtime.</remarks>
    private static readonly DependencyProperty TransitionsProperty = DependencyProperty.RegisterAttached("ShadowTransitions", typeof(BehaviorCollection), typeof(Interaction), new FrameworkPropertyMetadata(new PropertyChangedCallback(Interaction.OnTransitionsChanged)));

    /// <summary>
    /// Gets the collection of transition behaviors associated with the specified dependency object. If no collection
    /// exists, a new one is created and attached.
    /// </summary>
    /// <remarks>This method ensures that each dependency object has a unique BehaviorCollection instance for
    /// managing its transitions. Subsequent calls with the same object will return the same collection
    /// instance.</remarks>
    /// <param name="dependencyObject">The object from which to retrieve or to which to attach the transition behaviors. Cannot be null.</param>
    /// <returns>A BehaviorCollection containing the transition behaviors associated with the specified dependency object. If no
    /// behaviors are associated, a new, empty collection is returned and attached.</returns>
    public static BehaviorCollection GetTransitions(DependencyObject dependencyObject)
    {
        BehaviorCollection behaviorCollection = (BehaviorCollection)dependencyObject.GetValue(Interaction.TransitionsProperty);
        if (behaviorCollection == null)
        {
            behaviorCollection = (BehaviorCollection)Activator.CreateInstance(typeof(BehaviorCollection), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture)!;
            dependencyObject.SetValue(Interaction.TransitionsProperty, behaviorCollection);
        }
        return behaviorCollection;
    }

    /// <summary>
    /// Handles changes to the collection of transitions attached to a dependency object.
    /// </summary>
    /// <remarks>This method is typically used as a property changed callback for an attached property
    /// representing a collection of transitions or behaviors. It ensures that the old collection is detached from the
    /// dependency object and the new collection is attached, maintaining the correct association between the object and
    /// its behaviors.</remarks>
    /// <param name="dependencyObject">The object to which the transitions are attached or detached.</param>
    /// <param name="args">The event data that contains information about the change to the transitions collection.</param>
    private static void OnTransitionsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        BehaviorCollection oldCollection = (BehaviorCollection)args.OldValue;
        BehaviorCollection newCollection = (BehaviorCollection)args.NewValue;

        if (oldCollection == newCollection)
        {
            return;
        }

        if (oldCollection != null && ((IAttachedObject)oldCollection).AssociatedObject != null)
        {
            oldCollection.Detach();
        }

        if (newCollection != null && dependencyObject != null)
        {
            if (((IAttachedObject)newCollection).AssociatedObject is null)
            {
                newCollection.Attach(dependencyObject);
            }
        }
    }
}

/// <summary>
/// Specifies the types of transitions or events that can occur for a UI element, such as changes in data context, mouse
/// pointer movement, or focus state.
/// </summary>
/// <remarks>Use this enumeration to identify or handle specific UI transitions or events, such as responding to
/// focus changes or mouse interactions. The values correspond to common UI lifecycle events and can be used in event
/// handling or state management scenarios.</remarks>
public enum TransitionEvent
{
    /// <summary>
    ///
    /// </summary>
    None,

    /// <summary>
    /// Gets a value indicating whether the resource has been successfully loaded.
    /// </summary>
    Loaded,

    /// <summary>
    /// Occurs when the data context for this element changes.
    /// </summary>
    /// <remarks>This event is typically used to respond to changes in the data context, such as updating
    /// bindings or performing additional initialization when the data context is set or replaced.</remarks>
    DataContextChanged,

    /// <summary>
    /// Occurs when the mouse pointer enters the bounds of the control.
    /// </summary>
    /// <remarks>This event is typically used to provide visual feedback or initiate actions when the user
    /// moves the mouse pointer over a control. The event is raised only when the pointer enters the control's area; it
    /// is not raised again until the pointer leaves and re-enters the control.</remarks>
    MouseEnter,

    /// <summary>
    /// Occurs when the mouse pointer leaves the boundaries of the element.
    /// </summary>
    MouseLeave,

    /// <summary>
    /// Occurs when the control receives input focus.
    /// </summary>
    /// <remarks>This event is typically used to perform actions when a control becomes active, such as
    /// updating the user interface or preparing resources. The event is raised when the control receives focus either
    /// through user interaction or programmatically.</remarks>
    GotFocus,

    /// <summary>
    /// Represents the event that occurs when a control loses input focus.
    /// </summary>
    LostFocus,
}

/// <summary>
/// Specifies the type of easing function to use for interpolating values in animations.
/// </summary>
/// <remarks>Easing functions control the rate of change of an animation, allowing for effects such as
/// acceleration, deceleration, or bouncing. The selected easing function determines how the animated value progresses
/// over time, enabling more natural or stylized motion compared to linear interpolation. Choose an easing function
/// based on the desired animation effect.</remarks>
public enum EasingFunction
{
    /// <summary>
    ///
    /// </summary>
    None,

    /// <summary>
    /// Gets or sets the back navigation command or state for the control.
    /// </summary>
    Back,

    /// <summary>
    /// Represents the bounce animation type.
    /// </summary>
    Bounce,

    /// <summary>
    /// Represents a geometric circle defined by its center point and radius.
    /// </summary>
    /// <remarks>Use this class to perform calculations or operations related to circles, such as computing
    /// area, circumference, or determining point containment. The circle is typically defined in a two-dimensional
    /// coordinate system.</remarks>
    Circle,

    /// <summary>
    /// Specifies a cubic interpolation method or easing function.
    /// </summary>
    /// <remarks>Use this value to indicate that cubic interpolation should be applied, typically resulting in
    /// smoother transitions compared to linear methods. The specific behavior may depend on the context in which this
    /// value is used.</remarks>
    Cubic,

    /// <summary>
    /// Represents an elastic easing function used for animations that simulate a spring-like motion.
    /// </summary>
    /// <remarks>This type is typically used to create animations where the value overshoots and oscillates
    /// before settling, mimicking the behavior of a spring. It is commonly applied in UI transitions to provide a more
    /// natural and dynamic effect.</remarks>
    Elastic,

    /// <summary>
    /// Represents an exponential mathematical function or distribution.
    /// </summary>
    /// <remarks>Use this type to perform calculations or represent values related to exponential growth,
    /// decay, or probability distributions. The specific behavior depends on the context in which the type is
    /// used.</remarks>
    Exponential,

    /// <summary>
    /// Gets or sets the power level or value associated with this instance.
    /// </summary>
    Power,

    /// <summary>
    /// Represents a quadratic equation or function, typically of the form ax² + bx + c.
    /// </summary>
    /// <remarks>Use this class to model, evaluate, or solve quadratic equations. Quadratic equations are
    /// commonly used in mathematics, physics, and engineering to describe parabolic relationships.</remarks>
    Quadratic,

    /// <summary>
    /// Represents a quartic (fourth-degree) polynomial or operation involving quartic equations.
    /// </summary>
    Quartic,

    /// <summary>
    /// Represents a quintic (fifth-degree) polynomial or easing function, typically used for smooth interpolation or
    /// animation curves.
    /// </summary>
    /// <remarks>A quintic function is commonly used in animation and graphics to create smooth transitions
    /// with gradual acceleration and deceleration. This type may provide methods for evaluating the polynomial or
    /// generating easing curves for use in motion or value interpolation.</remarks>
    Quintic,

    /// <summary>
    /// Represents the sine trigonometric function.
    /// </summary>
    Sine,
}
