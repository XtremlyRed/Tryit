using System.ComponentModel;

namespace Tryit.Wpf;

/// <summary>
/// Abstract class for converting between true/false values and a specified type. It provides properties for defining
/// true and false representations.
/// </summary>
/// <typeparam name="T">Specifies the type that will be converted to and from true/false values.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TrueFalseConverter<T> : ValueConverterBase<T>, IValueConverter
{
    /// <summary>
    /// Represents a property that can hold a value indicating true. The value can be null.
    /// </summary>
    public object? True { get; set; }

    /// <summary>
    /// Represents a property that can hold a nullable object value. It is typically used to indicate a false state or
    /// condition.
    /// </summary>
    public object? False { get; set; }
}

/// <summary>
/// Provides a base markup extension for creating value converters that map between true and false states using
/// customizable values.
/// </summary>
/// <remarks>This abstract class is intended for use in XAML scenarios where a value converter is needed to
/// translate between Boolean values and custom representations. It allows specifying the values to use for true and
/// false states via the True and False properties. Derived classes can override the ProvideValue method to customize
/// converter instantiation or configuration.</remarks>
/// <typeparam name="T">The type of the true/false value converter to instantiate. Must inherit from TrueFalseConverter{TP} and have a
/// parameterless constructor.</typeparam>
/// <typeparam name="TP">The type of the values used to represent true and false states in the converter.</typeparam>
public abstract class TrueFalseConverterExtension<T, TP> : MarkupExtension
    where T : TrueFalseConverter<TP>, new()
{
    /// <summary>
    /// Represents a property that can hold a value indicating true. The value can be null.
    /// </summary>
    public object? True { get; set; }

    /// <summary>
    /// Represents a property that can hold a nullable object value. It is typically used to indicate a false state or
    /// condition.
    /// </summary>
    public object? False { get; set; }

    /// <summary>
    /// Provides a value based on the specified service provider context.
    /// </summary>
    /// <param name="serviceProvider">The context used to retrieve services needed for value provision.</param>
    /// <returns>Returns an instance of TrueFalseConverter.</returns>
    public sealed override object ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    protected virtual T ProvideValue()
    {
        return new T() { True = this.True, False = this.False };
    }
}
