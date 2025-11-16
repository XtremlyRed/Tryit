using System.Windows.Media.Animation;
using Expression = System.Linq.Expressions.Expression;

namespace Tryit.Wpf.Internals;

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TProperty"></typeparam>
internal abstract record AnimationSetter<T, TProperty>
    where T : AnimationTimeline
{
    /// <summary>
    /// Initializes a new instance of the AnimationSetter class using the specified function to provide the animation
    /// value.
    /// </summary>
    /// <param name="func">A function that returns the initial value for the animation. Cannot be null.</param>
    public AnimationSetter(Func<T> func)
    {
        this.Animation = func();

        FromSetter = CreateSetDelegate<TProperty>("From");
        ToSetter = CreateSetDelegate<TProperty>("To");
        EasingFunctionSetter = CreateSetDelegate<IEasingFunction>("EasingFunction");
    }

    /// <summary>
    /// Gets the animation associated with this instance.
    /// </summary>
    public T Animation { get; }

    /// <summary>
    /// Gets the type of animation associated with the derived class.
    /// </summary>
    protected abstract Type AnimationType { get; }

    /// <summary>
    /// Represents a delegate that sets the value of a property on a specified object.
    /// </summary>
    /// <remarks>The delegate takes two parameters: the target object and the value to assign to the property.
    /// This is typically used to provide a fast, strongly-typed way to set property values dynamically.</remarks>
    public readonly Action<T, TProperty> FromSetter;

    /// <summary>
    /// Represents a delegate that sets the value of a property on an object of type T.
    /// </summary>
    /// <remarks>The delegate takes two parameters: the target object and the value to assign to the property.
    /// This can be used to programmatically set property values without using reflection at runtime.</remarks>
    public readonly Action<T, TProperty> ToSetter;

    /// <summary>
    /// Represents a delegate that sets the easing function for an animation or transition on the specified target
    /// object.
    /// </summary>
    /// <remarks>The delegate accepts a target object of type <typeparamref name="T"/> and an <see
    /// cref="IEasingFunction"/> instance, allowing customization of the animation's interpolation behavior. This field
    /// is typically used to assign or update the easing function applied to an animation target.</remarks>
    public readonly Action<T, IEasingFunction> EasingFunctionSetter;

    /// <summary>
    /// Creates a delegate that sets the value of a specified property on an object of type T.
    /// </summary>
    /// <remarks>If the specified property does not exist or is not writable, the returned delegate will
    /// perform no action when invoked. This method uses expression trees to generate the setter delegate at
    /// runtime.</remarks>
    /// <typeparam name="TTProperty">The type of the property to set on the object.</typeparam>
    /// <param name="propertyName">The name of the property to set. This value is case-sensitive and must correspond to a writable property on the
    /// object.</param>
    /// <returns>An Action delegate that sets the specified property on an object of type T to a given value of type TTProperty.
    /// If the property cannot be set, returns a no-op delegate.</returns>
    private Action<T, TTProperty> CreateSetDelegate<TTProperty>(string propertyName)
    {
        try
        {
            var parameter = Expression.Parameter(typeof(T), "obj");

            var valueParameter = Expression.Parameter(typeof(TTProperty), "value");

            var instance = Expression.Convert(parameter, AnimationType);

            var property = Expression.Property(instance, propertyName);

            var convertValue = Expression.Convert(valueParameter, property.Type);

            var assign = Expression.Assign(property, convertValue);

            var func = Expression.Lambda<Action<T, TTProperty>>(assign, parameter, valueParameter);

            return func.Compile();
        }
        catch
        {
            return (obj, value) => { };
        }
    }
}

/// <summary>
/// Provides a base implementation for setting animation properties using a specific animation type. Intended for use
/// with animation timelines that require a concrete animation type at runtime.
/// </summary>
/// <remarks>This type is typically used to facilitate generic animation scenarios where the animation type must
/// be specified at compile time. It is intended for internal use within animation frameworks or libraries.</remarks>
/// <typeparam name="T">The base type of the animation timeline. Must inherit from AnimationTimeline.</typeparam>
/// <typeparam name="TAnimationType">The concrete animation type to instantiate. Must inherit from T and have a parameterless constructor.</typeparam>
/// <typeparam name="TProperty">The type of the property being animated.</typeparam>
internal record AnimationTypeSetter<T, TAnimationType, TProperty> : AnimationSetter<T, TProperty>
    where T : AnimationTimeline
    where TAnimationType : T, new()
{
    /// <summary>
    /// Gets the type of animation associated with this instance.
    /// </summary>
    protected override Type AnimationType => typeof(TAnimationType);

    /// <summary>
    /// Initializes a new instance of the AnimationTypeSetter class.
    /// </summary>
    /// <remarks>This constructor creates a new AnimationTypeSetter using the default constructor of the
    /// TAnimationType type parameter. Use this when you want to set up an animation type with its default
    /// configuration.</remarks>
    public AnimationTypeSetter()
        : base(() => new TAnimationType()) { }
}
