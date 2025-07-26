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
