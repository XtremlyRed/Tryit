using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

public class TranslateTransition : TransitionBase<Point?, DoubleAnimation>
{
    public TranslateTransition()
    {
        To = new Point(0, 0);
    }

    protected override IEnumerable<DoubleAnimation> AnimationBuild()
    {
        const string XPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.X)";
        const string YPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.Y)";

        DoubleAnimation xAnimation = new DoubleAnimation();
        DoubleAnimation yAnimation = new DoubleAnimation();

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup)
        {
            var index = transformGroup.IndexOf<System.Windows.Media.TranslateTransform>();

            Storyboard.SetTarget(xAnimation, AssociatedObject);
            Storyboard.SetTarget(yAnimation, AssociatedObject);

            Storyboard.SetTargetProperty(xAnimation, new PropertyPath(string.Format(XPath, index)));
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath(string.Format(YPath, index)));
        }

        yield return xAnimation;
        yield return yAnimation;
    }

    protected override void ConfigureAnimation(DoubleAnimation animation, int animationIndex)
    {
        base.ConfigureAnimation(animation, animationIndex);

        if (animationIndex == 0)
        {
            animation.From = From.HasValue ? From.Value.X : animation.From;
            animation.To = To.HasValue ? To.Value.X : animation.To;
        }
        else if (animationIndex == 1)
        {
            animation.From = From.HasValue ? From.Value.Y : animation.From;
            animation.To = To.HasValue ? To.Value.Y : animation.To;
        }
    }
}
