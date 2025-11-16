using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using Expression = System.Linq.Expressions.Expression;

namespace Tryit.Wpf;

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TAnimation"></typeparam>
internal static class TransitionHelpers<T, TAnimation>
    where TAnimation : AnimationTimeline, new()
{
    /// <summary>
    /// Provides a delegate that sets the value of the 'From' property on a specified animation object.
    /// </summary>
    /// <remarks>This delegate is intended for internal use when manipulating animation properties
    /// generically. It enables setting the 'From' value without requiring compile-time knowledge of the animation
    /// type.</remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly Action<TAnimation, T> FromSetter = CreateSetDelegate<T>("From");

    /// <summary>
    /// Represents a delegate that sets the 'To' value on a specified animation instance.
    /// </summary>
    /// <remarks>This delegate is typically used to assign the target value of an animation. It is intended
    /// for internal use and is not visible in debugger variable windows due to the DebuggerBrowsable
    /// attribute.</remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly Action<TAnimation, T> ToSetter = CreateSetDelegate<T>("To");

    /// <summary>
    /// Provides a delegate that sets the easing function on an animation instance.
    /// </summary>
    /// <remarks>This delegate is intended for internal use to assign an <see cref="IEasingFunction"/> to the
    /// 'EasingFunction' property of a <typeparamref name="TAnimation"/> object. It is not intended to be used directly
    /// in application code.</remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly Action<TAnimation, IEasingFunction> EasingFunctionSetter = CreateSetDelegate<IEasingFunction>("EasingFunction");

    /// <summary>
    /// Creates a delegate that sets the value of a specified property on a TAnimation object.
    /// </summary>
    /// <remarks>If the specified property does not exist or is not writable, the returned delegate will
    /// perform no action. This method uses expression trees to generate the setter delegate at runtime.</remarks>
    /// <typeparam name="TParameter">The type of the value to assign to the property.</typeparam>
    /// <param name="propertyName">The name of the property to set on the TAnimation object. Must correspond to a writable property of TAnimation.</param>
    /// <returns>A delegate that takes a TAnimation object and a value of type TParameter, and sets the specified property to the
    /// given value. If the property cannot be set, returns a no-op delegate.</returns>
    private static Action<TAnimation, TParameter> CreateSetDelegate<TParameter>(string propertyName)
    {
        try
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TAnimation), "obj");

            ParameterExpression valueParameter = Expression.Parameter(typeof(TParameter), "value");

            UnaryExpression instance = Expression.Convert(parameter, typeof(TAnimation));

            MemberExpression property = Expression.Property(instance, propertyName);

            UnaryExpression convertValue = Expression.Convert(valueParameter, property.Type);

            BinaryExpression assign = Expression.Assign(property, convertValue);

            Expression<Action<TAnimation, TParameter>> func = Expression.Lambda<Action<TAnimation, TParameter>>(assign, parameter, valueParameter);

            return func.Compile();
        }
        catch
        {
            return (obj, value) => { };
        }
    }
}
