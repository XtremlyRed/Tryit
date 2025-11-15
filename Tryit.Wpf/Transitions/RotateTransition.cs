using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

public class RotateTransition : TransitionBase<double?, DoubleAnimation>
{
    public RotateTransition()
    {
        To = 0;
    }

    protected override IEnumerable<DoubleAnimation> AnimationBuild()
    {
        const string Path = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(RotateTransform.Angle)";

        DoubleAnimation animation = new DoubleAnimation();

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup)
        {
            var index = transformGroup.IndexOf<System.Windows.Media.RotateTransform>();

            Storyboard.SetTarget(animation, AssociatedObject);

            Storyboard.SetTargetProperty(animation, new PropertyPath(string.Format(Path, index)));
        }

        yield return animation;
    }
}
