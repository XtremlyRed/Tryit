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

internal static class AnimationHelpers<T, TAnimation>
    where TAnimation : AnimationTimeline, new()
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly Action<TAnimation, T> FromSetter = CreateSetDelegate<T>("From");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly Action<TAnimation, T> ToSetter = CreateSetDelegate<T>("To");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly Action<TAnimation, IEasingFunction> EasingFunctionSetter = CreateSetDelegate<IEasingFunction>("EasingFunction");

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
