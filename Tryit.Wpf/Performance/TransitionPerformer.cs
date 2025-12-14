using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

public class TransitionPerformer : Performer
{
    public TransitionOn TransitionOn
    {
        get => (TransitionOn)GetValue(TransitionOnProperty);
        set => SetValue(TransitionOnProperty, value);
    }

    public static readonly DependencyProperty TransitionOnProperty = DependencyProperty.Register(nameof(TransitionOn), typeof(TransitionOn), typeof(TransitionPerformer), new PropertyMetadata(TransitionOn.FadeTo));

    public double? Target
    {
        get => (double?)GetValue(TargetProperty); set => SetValue(TargetProperty, value);
    }

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(double?), typeof(TransitionPerformer), new PropertyMetadata(null));

    internal override AnimationTimeline CreateAnimation(DependencyObject dependencyObject)
    {

        const string FadePath = "(UIElement.OpacityMask).(SolidColorBrush.Color)";
        const string RotatePath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(RotateTransform.Angle)";
        const string ScaleXPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(ScaleTransform.ScaleX)";
        const string ScaleYPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(ScaleTransform.ScaleY)";
        const string TranslateXPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.X)";
        const string TranslateYPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.Y)";
        const string SkewXPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(SkewTransform.AngleX)";
        const string SkewYPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(SkewTransform.AngleY)";

        if (TransitionOn is TransitionOn.FadeFrom)
        {
            return Initialize<ColorAnimation, double?, MatrixTransform>(dependencyObject, FadePath, Target, (animation, data) => animation.From = data.HasValue ? Color.FromArgb((byte)((data!.Value < 0 ? 0 : data.Value > 1 ? 1 : data.Value) * 255), 0, 0, 0) : null, null, s => ((FrameworkElement)s).OpacityMask ??= new SolidColorBrush(Colors.Black));
        }

        if (TransitionOn is TransitionOn.FadeTo)
        {
            return Initialize<ColorAnimation, double?, MatrixTransform>(dependencyObject, FadePath, Target, (animation, data) => animation.To = data.HasValue ? Color.FromArgb((byte)((data!.Value < 0 ? 0 : data.Value > 1 ? 1 : data.Value) * 255), 0, 0, 0) : null, null, s => ((FrameworkElement)s).OpacityMask ??= new SolidColorBrush(Colors.Black));
        }

        if (dependencyObject is not UIElement uIElement || uIElement.RenderTransform is not TransformGroup transformGroup)
        {
            return null!;
        }

        if (TransitionOn is TransitionOn.ScaleXTo)
        {
            return Initialize<DoubleAnimation, double?, ScaleTransform>(dependencyObject, ScaleXPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<ScaleTransform>);
        }

        if (TransitionOn is TransitionOn.ScaleYTo)
        {
            return Initialize<DoubleAnimation, double?, ScaleTransform>(dependencyObject, ScaleYPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<ScaleTransform>);//
        }
        if (TransitionOn is TransitionOn.ScaleXFrom)
        {
            return Initialize<DoubleAnimation, double?, ScaleTransform>(dependencyObject, ScaleXPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<ScaleTransform>);
        }

        if (TransitionOn is TransitionOn.ScaleYFrom)
        {
            return Initialize<DoubleAnimation, double?, ScaleTransform>(dependencyObject, ScaleYPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<ScaleTransform>);//
        }

        if (TransitionOn is TransitionOn.TranslateXTo)
        {
            return Initialize<DoubleAnimation, double?, TranslateTransform>(dependencyObject, TranslateXPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<TranslateTransform>);
        }

        if (TransitionOn is TransitionOn.TranslateYTo)
        {
            return Initialize<DoubleAnimation, double?, TranslateTransform>(dependencyObject, TranslateYPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<TranslateTransform>);//
        }
        if (TransitionOn is TransitionOn.TranslateXFrom)
        {
            return Initialize<DoubleAnimation, double?, TranslateTransform>(dependencyObject, TranslateXPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<TranslateTransform>);
        }

        if (TransitionOn is TransitionOn.TranslateYFrom)
        {
            return Initialize<DoubleAnimation, double?, TranslateTransform>(dependencyObject, TranslateYPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<TranslateTransform>);//
        }

        if (TransitionOn is TransitionOn.SkewXTo)
        {
            return Initialize<DoubleAnimation, double?, SkewTransform>(dependencyObject, SkewXPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<SkewTransform>);
        }

        if (TransitionOn is TransitionOn.SkewYTo)
        {
            return Initialize<DoubleAnimation, double?, SkewTransform>(dependencyObject, SkewYPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<SkewTransform>);//
        }
        if (TransitionOn is TransitionOn.SkewXFrom)
        {
            return Initialize<DoubleAnimation, double?, SkewTransform>(dependencyObject, SkewXPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<SkewTransform>);
        }

        if (TransitionOn is TransitionOn.SkewYFrom)
        {
            return Initialize<DoubleAnimation, double?, SkewTransform>(dependencyObject, SkewYPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<SkewTransform>);//
        }

        if (TransitionOn is TransitionOn.RotateFrom)
        {
            return Initialize<DoubleAnimation, double?, RotateTransform>(dependencyObject, RotatePath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<RotateTransform>);
        }

        if (TransitionOn is TransitionOn.RotateTo)
        {
            return Initialize<DoubleAnimation, double?, RotateTransform>(dependencyObject, RotatePath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<RotateTransform>);//
        }

        //返回
        return default!;

        static string Initialize<TTransform>(DependencyObject dependencyObject, string animationPath)
            where TTransform : Transform, new()
        {
            if (dependencyObject is UIElement uIElement && uIElement.RenderTransform is TransformGroup transformGroup)
            {
                _ = transformGroup.TryIndexOf<TTransform>(out var index);
                animationPath = string.Format(animationPath, index);
            }
            return animationPath;
        }
    }
}

public enum TransitionOn
{
    FadeTo,
    FadeFrom,
    RotateTo,
    RotateFrom,
    ScaleXTo,
    ScaleYTo,
    ScaleXFrom,
    ScaleYFrom,
    TranslateXTo,
    TranslateYTo,
    TranslateXFrom,
    TranslateYFrom,
    SkewXTo,
    SkewYTo,
    SkewXFrom,
    SkewYFrom,
}
