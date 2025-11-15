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

public static class Interaction
{
    private static readonly DependencyProperty TransitionsProperty = DependencyProperty.RegisterAttached("ShadowTransitions", typeof(BehaviorCollection), typeof(Interaction), new FrameworkPropertyMetadata(new PropertyChangedCallback(Interaction.OnTransitionsChanged)));

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

public enum AnimationEvent
{
    None,
    Loaded,
    DataContextChanged,
    MouseEnter,
    MouseLeave,
    GotFocus,
    LostFocus,
}

public enum EasingFunction
{
    None,
    Back,
    Bounce,
    Circle,
    Cubic,
    Elastic,
    Exponential,
    Power,
    Quadratic,
    Quartic,
    Quintic,
    Sine,

    //BackEase
    //BounceEase
    //CircleEase
    //CubicEase
    //EasingFunctionBase
    //ElasticEase
    //ExponentialEase
    //PowerEase
    //QuadraticEase
    //QuarticEase
    //QuinticEase
    //SineEase
    //DummyEasingFunction
}
