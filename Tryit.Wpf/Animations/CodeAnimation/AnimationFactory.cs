using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media.Animation;
using Tryit.Wpf.Internals;

namespace Tryit.Wpf;

//public static class AnimationFactory
//{
//    public static AnimationPropertySelector<T> Animation<T>(this T dependencyObject)
//        where T : DependencyObject
//    {
//        return new AnimationPropertySelector<T>(dependencyObject);
//    }

//    public static void BeginAnimation<T>(this T dependencyObject, Action<AnimationPropertySelector<T>> config)
//        where T : DependencyObject
//    {
//        var builder = new AnimationPropertySelector<T>(dependencyObject);

//        config?.Invoke(builder);
//    }
//}

//public interface IAnimationPropertySelector<T>
//    where T : DependencyObject
//{
//    T DependencyObject { get; }

//    IPropertyAnimationBuilder<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertySelector);
//    IPropertyAnimationBuilder<T, TProperty> Property<TProperty>(string propertySelector);
//    IPropertyAnimationBuilder<T, TProperty> Property<TProperty>(DependencyProperty propertySelector);
//}

//public interface IAnimationHandler
//{
//    void Play();
//}

//public interface IPropertyAnimationBuilder<T, TProperty>
//    where T : DependencyObject
//{
//    IPropertyAnimationBuilder<T, TProperty> From(TProperty fromValue);
//    IPropertyAnimationBuilder<T, TProperty> To(TProperty toValue);
//    IPropertyAnimationBuilder<T, TProperty> Duration(TimeSpan duration);
//    IPropertyAnimationBuilder<T, TProperty> Delay(TimeSpan delay);
//    IPropertyAnimationBuilder<T, TProperty> SpeedRatio(double speedRatio);
//    IPropertyAnimationBuilder<T, TProperty> RepeatBehavior(RepeatBehavior repeatBehavior);
//    IPropertyAnimationBuilder<T, TProperty> FillBehavior(FillBehavior fillBehavior);
//    IPropertyAnimationBuilder<T, TProperty> DecelerationRatio(double decelerationRatio);
//    IPropertyAnimationBuilder<T, TProperty> AccelerationRatio(double accelerationRatio);
//    IPropertyAnimationBuilder<T, TProperty> AutoReverse(bool autoReverse = true);
//    IPropertyAnimationBuilder<T, TProperty> Completed(Action callback);

//    IAnimationHandler Build();

//    //IAnimationBuilder MoveTo(double x, double y);
//    //IAnimationBuilder FadeTo(double opacity);
//    //IAnimationBuilder ScaleTo(double scaleX, double scaleY);
//    //IAnimationBuilder RotateTo(double angle);
//    //IAnimationBuilder WidthTo(double width);
//    //IAnimationBuilder HeightTo(double height);
//    //IAnimationBuilder MarginTo(double left, double top, double right, double bottom);
//    //IAnimationBuilder ColorTo(string propertyPath, Color to);
//    //IAnimationBuilder BackgroundColorTo(Color to);
//    //IAnimationBuilder OpacityTo(double opacity);
//    //IAnimationBuilder TranslateXTo(double x);
//    //IAnimationBuilder TranslateYTo(double y);
//    //IAnimationBuilder TranslateTo(double x, double y);
//    //IAnimationBuilder ScaleXTo(double scaleX);
//    //IAnimationBuilder ScaleYTo(double scaleY);
//    //IAnimationBuilder RotateXTo(double angle);
//    //IAnimationBuilder RotateYTo(double angle);
//    //IAnimationBuilder RotateZTo(double angle);
//    //IAnimationBuilder SkewXTo(double angle);
//    //IAnimationBuilder SkewYTo(double angle);
//    //IAnimationBuilder SkewTo(double angleX, double angleY);
//}
