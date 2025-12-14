using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

public class ColorPerformer : Performer
{
    public ColorOn ColorOn
    {
        get => (ColorOn)GetValue(ColorOnProperty);
        set => SetValue(ColorOnProperty, value);
    }

    public static readonly DependencyProperty ColorOnProperty =
        DependencyProperty.Register(nameof(ColorOn), typeof(ColorOn), typeof(ColorPerformer), new PropertyMetadata(ColorOn.BackgroundTo));

    public Color? Target
    {
        get => (Color?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(Color?), typeof(ColorPerformer), new PropertyMetadata(null));

    internal override AnimationTimeline CreateAnimation(DependencyObject dependencyObject)
    {
        const string ColorPath = "({0}.{1}).(SolidColorBrush.Color)";

        var colorOn = ColorOn.ToString().Replace("From", "").Replace("To", "");

        var animationPath = string.Format(ColorPath, dependencyObject.GetType().Name, colorOn);

        if (ColorOn is ColorOn.BackgroundFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.BackgroundTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.BorderBrushFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.BorderBrushTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.ForegroundFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.ForegroundTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.FillFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.FillTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.StrokeFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);  //
        }
        if (ColorOn is ColorOn.StrokeTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);  //
        }

        //返回
        return default!;
    }
}

public enum ColorOn
{
    BackgroundFrom,
    BackgroundTo,
    BorderBrushFrom,
    BorderBrushTo,
    ForegroundFrom,
    ForegroundTo,
    FillFrom,
    FillTo,
    StrokeFrom,
    StrokeTo,
}