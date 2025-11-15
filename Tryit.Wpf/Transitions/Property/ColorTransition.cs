using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Tryit.Wpf;

public class ColorTransition : PropertyTransitionBase<Color?, ColorAnimation>
{
    public ColorTransition()
    {
        To = Colors.Black;
    }
}

public class QuaternionTransition : PropertyTransitionBase<Quaternion?, QuaternionAnimation>
{
    public QuaternionTransition()
    {
        To = new Quaternion();
    }
}

public class ThicknessTransition : PropertyTransitionBase<Thickness?, ThicknessAnimation>
{
    public ThicknessTransition()
    {
        To = new Thickness();
    }
}

public class DecimalTransition : PropertyTransitionBase<Decimal?, DecimalAnimation>
{
    public DecimalTransition()
    {
        To = 0;
    }
}

public class DoubleTransition : PropertyTransitionBase<Double?, DoubleAnimation>
{
    public DoubleTransition()
    {
        To = 0;
    }
}

public class SingleTransition : PropertyTransitionBase<Single?, SingleAnimation>
{
    public SingleTransition()
    {
        To = 0;
    }
}

public class ByteTransition : PropertyTransitionBase<Byte?, ByteAnimation>
{
    public ByteTransition()
    {
        To = 0;
    }
}

public class Int16Transition : PropertyTransitionBase<Int16?, Int16Animation>
{
    public Int16Transition()
    {
        To = 0;
    }
}

public class Int32Transition : PropertyTransitionBase<Int32?, Int32Animation>
{
    public Int32Transition()
    {
        To = 0;
    }
}

public class Int64Transition : PropertyTransitionBase<Int64?, Int64Animation>
{
    public Int64Transition()
    {
        To = 0;
    }
}

public class Vector3DTransition : PropertyTransitionBase<Vector3D?, Vector3DAnimation>
{
    public Vector3DTransition()
    {
        To = new Vector3D();
    }
}

public class VectorTransition : PropertyTransitionBase<Vector?, VectorAnimation>
{
    public VectorTransition()
    {
        To = new Vector();
    }
}

public class Point3DTransition : PropertyTransitionBase<Point3D?, Point3DAnimation>
{
    public Point3DTransition()
    {
        To = new Point3D();
    }
}

public class PointTransition : PropertyTransitionBase<Point?, PointAnimation>
{
    public PointTransition()
    {
        To = new Point();
    }
}

public class SizeTransition : PropertyTransitionBase<Size?, SizeAnimation>
{
    public SizeTransition()
    {
        To = new Size();
    }
}

public class RectTransition : PropertyTransitionBase<Rect?, RectAnimation>
{
    public RectTransition()
    {
        To = new Rect();
    }
}
