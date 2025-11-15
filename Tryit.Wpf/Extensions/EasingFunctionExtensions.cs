using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides extension methods for working with easing functions and their associated modes.
/// </summary>
/// <remarks>This class contains static methods that simplify the creation and manipulation of easing function
/// instances with specific easing modes. It is intended to enhance usability when working with animation easing in
/// applications.</remarks>
public static class EasingFunctionExtensions
{
    /// <summary>
    /// Creates an easing function instance that corresponds to the specified easing function and mode.
    /// </summary>
    /// <remarks>Use this method to obtain a concrete <see cref="IEasingFunction"/> implementation for
    /// animation scenarios based on the provided <paramref name="easingFunction"/> and <paramref name="easingMode"/>
    /// values.</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="easingFunction">The type of easing function to create.</param>
    /// <param name="easingMode">The easing mode to apply to the created easing function.</param>
    /// <returns>An <see cref="IEasingFunction"/> instance configured with the specified easing function and mode, or <see
    /// langword="null"/> if the easing function is not recognized.</returns>
    public static IEasingFunction? WithEasing(this EasingFunction easingFunction, EasingMode easingMode)
    {
        return easingFunction switch
        {
            EasingFunction.Back => new BackEase() { EasingMode = easingMode },
            EasingFunction.Bounce => new BounceEase() { EasingMode = easingMode },
            EasingFunction.Circle => new CircleEase() { EasingMode = easingMode },
            EasingFunction.Cubic => new CubicEase() { EasingMode = easingMode },
            EasingFunction.Elastic => new ElasticEase() { EasingMode = easingMode },
            EasingFunction.Exponential => new ExponentialEase() { EasingMode = easingMode },
            EasingFunction.Power => new PowerEase() { EasingMode = easingMode },
            EasingFunction.Quadratic => new QuadraticEase() { EasingMode = easingMode },
            EasingFunction.Quartic => new QuarticEase() { EasingMode = easingMode },
            EasingFunction.Quintic => new QuinticEase() { EasingMode = easingMode },
            EasingFunction.Sine => new SineEase() { EasingMode = easingMode },
            _ => null!,
        };
    }
}
