using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

public static class TransformGroupExtensions
{
    public static int IndexOf<T>(this TransformGroup transformCollection)
        where T : Transform
    {
        for (int i = 0; i < transformCollection.Children.Count; i++)
        {
            if (transformCollection.Children[i] is T)
            {
                return i;
            }
        }
        return -1;
    }
}
