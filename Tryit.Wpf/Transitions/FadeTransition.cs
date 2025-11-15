using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

public class FadeTransition : TransitionBase<Color?, ColorAnimation>
{
    public FadeTransition()
    {
        To = Colors.Black;
    }

    protected override IEnumerable<ColorAnimation> AnimationBuild()
    {
        const string Path = "(UIElement.OpacityMask).(SolidColorBrush.Color)";

        ColorAnimation animation = new ColorAnimation();

        Storyboard.SetTargetProperty(animation, new PropertyPath(Path));

        Storyboard.SetTarget(animation, AssociatedObject);

        base.AssociatedObject.OpacityMask ??= new SolidColorBrush(Colors.Black);

        yield return animation;
    }
}
