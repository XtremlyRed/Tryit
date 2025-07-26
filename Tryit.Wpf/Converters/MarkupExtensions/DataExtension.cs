using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tryit.Wpf;

/// <summary>
/// CharExtension is a class that extends DataExtension for the char type. It provides constructors for creating
/// instances with or without an initial character value.
/// </summary>

[ContentProperty(nameof(Value))]
public class CharExtension : DataExtension<char>
{
    /// <summary>
    /// Constructor for the CharExtension class. Initializes a new instance of the CharExtension.
    /// </summary>
    public CharExtension() { }

    /// <summary>
    /// Initializes a new instance of the CharExtension class with a specified character value.
    /// </summary>
    /// <param name="value">The character that will be used to create the instance.</param>
    public CharExtension(char value)
        : base(value) { }
}

/// <summary>
/// Represents an extension for decimal data types. It includes a default constructor and one that initializes with a
/// specific decimal value.
/// </summary>
public class DecimalExtension : DataExtension<decimal>
{
    /// <summary>
    /// Constructor for the DecimalExtension class. Initializes a new instance of the class.
    /// </summary>
    public DecimalExtension() { }

    /// <summary>
    /// Initializes a new instance of the DecimalExtension class with a specified decimal value.
    /// </summary>
    /// <param name="value">The decimal number to be used for creating the instance.</param>
    public DecimalExtension(decimal value)
        : base(value) { }
}

/// <summary>
/// Represents an extension for boolean data types. It can be initialized with or without a specified boolean value.
/// </summary>
public class BooleanExtension : DataExtension<bool>
{
    /// <summary>
    /// Constructor for the BooleanExtension class. Initializes a new instance of the class.
    /// </summary>
    public BooleanExtension() { }

    /// <summary>
    /// Initializes a new instance of the BooleanExtension class with a specified boolean value.
    /// </summary>
    /// <param name="value">The boolean value determines the state of the instance.</param>
    public BooleanExtension(bool value)
        : base(value) { }
}

/// <summary>
/// StringExtension is a class that extends DataExtension for string types. It provides constructors to initialize
/// instances with or without a specified string value.
/// </summary>
public class StringExtension : DataExtension<string>
{
    /// <summary>
    /// Constructor for the StringExtension class. Initializes a new instance of the StringExtension.
    /// </summary>
    public StringExtension() { }

    /// <summary>
    /// Initializes a new instance of the StringExtension class with a specified string value.
    /// </summary>
    /// <param name="value">The string to be used for the new instance, which can be null.</param>
    public StringExtension(string? value)
        : base(value) { }
}

/// <summary>
/// Provides extensions for the sbyte data type. Includes constructors for initializing instances with or without a
/// specified value.
/// </summary>
public class SByteExtension : DataExtension<sbyte>
{
    /// <summary>
    /// Constructor for the SByteExtension class. Initializes a new instance of the SByteExtension.
    /// </summary>
    public SByteExtension() { }

    /// <summary>
    /// Initializes a new instance of the SByteExtension class with a specified signed byte value.
    /// </summary>
    /// <param name="value">The signed byte used to set the initial state of the instance.</param>
    public SByteExtension(sbyte value)
        : base(value) { }
}

/// <summary>
/// Represents an extension for byte data types. It provides constructors for initializing instances with or without a
/// specified byte value.
/// </summary>
public class ByteExtension : DataExtension<byte>
{
    /// <summary>
    /// Constructor for the ByteExtension class. Initializes a new instance of the ByteExtension.
    /// </summary>
    public ByteExtension() { }

    /// <summary>
    /// Initializes a new instance of the ByteExtension class with a specified byte value.
    /// </summary>
    /// <param name="value">The byte value used to set the initial state of the instance.</param>
    public ByteExtension(byte value)
        : base(value) { }
}

/// <summary>
/// DoubleExtension is a class that extends DataExtension for double values. It provides constructors for initializing
/// instances with or without a specified double value.
/// </summary>
public class DoubleExtension : DataExtension<double>
{
    /// <summary>
    /// Constructor for the DoubleExtension class. Initializes a new instance of the class.
    /// </summary>
    public DoubleExtension() { }

    /// <summary>
    /// Initializes a new instance of the DoubleExtension class with a specified double value.
    /// </summary>
    /// <param name="value">The double value serves as the base value for the instance.</param>
    public DoubleExtension(double value)
        : base(value) { }
}

/// <summary>
/// Represents a data extension for single-precision floating-point values. It can be initialized with or without a
/// specified float value.
/// </summary>
public class SingleExtension : DataExtension<float>
{
    /// <summary>
    /// Initializes a new instance of the SingleExtension class. This constructor does not take any parameters.
    /// </summary>
    public SingleExtension() { }

    /// <summary>
    /// Initializes a new instance of the SingleExtension class with a specified floating-point value.
    /// </summary>
    /// <param name="value">The floating-point number used to set the initial state of the instance.</param>
    public SingleExtension(float value)
        : base(value) { }
}

/// <summary>
/// Provides extensions for handling unsigned long (ulong) values. Includes constructors for creating instances with or
/// without an initial value.
/// </summary>
public class UInt64Extension : DataExtension<ulong>
{
    /// <summary>
    /// Constructor for the UInt64Extension class. Initializes a new instance of the class.
    /// </summary>
    public UInt64Extension() { }

    /// <summary>
    /// Initializes a new instance of the UInt64Extension class with a specified unsigned long value.
    /// </summary>
    /// <param name="value">The unsigned long value to be used for initializing the instance.</param>
    public UInt64Extension(ulong value)
        : base(value) { }
}

/// <summary>
/// Int64Extension is a class that extends DataExtension for long values. It provides constructors for initializing
/// instances with or without a specified long value.
/// </summary>
public class Int64Extension : DataExtension<long>
{
    /// <summary>
    /// Constructor for the Int64Extension class. Initializes a new instance of the class.
    /// </summary>
    public Int64Extension() { }

    /// <summary>
    /// Initializes a new instance of the Int64Extension class with a specified long value.
    /// </summary>
    /// <param name="value">The long value to be used for initializing the instance.</param>
    public Int64Extension(long value)
        : base(value) { }
}

/// <summary>
/// Provides extensions for the ushort data type. Includes constructors for initializing instances with or without a
/// specified value.
/// </summary>
public class UInt16Extension : DataExtension<ushort>
{
    /// <summary>
    /// Constructor for the UInt16Extension class. Initializes a new instance of the class.
    /// </summary>
    public UInt16Extension() { }

    /// <summary>
    /// Constructor for the UInt16Extension class that initializes an instance with a specified unsigned short value.
    /// </summary>
    /// <param name="value">The unsigned short value used to set the initial state of the instance.</param>
    public UInt16Extension(ushort value)
        : base(value) { }
}

/// <summary>
/// Int16Extension is a class that extends DataExtension for short values. It includes constructors for initializing
/// instances with or without a specified short value.
/// </summary>
public class Int16Extension : DataExtension<short>
{
    /// <summary>
    /// Constructor for the Int16Extension class. Initializes a new instance of the class.
    /// </summary>
    public Int16Extension() { }

    /// <summary>
    /// Initializes a new instance of the Int16Extension class with a specified short value.
    /// </summary>
    /// <param name="value">The short value to be used for initializing the instance.</param>
    public Int16Extension(short value)
        : base(value) { }
}

/// <summary>
/// Extends the DataExtension class for unsigned integers. Provides constructors for initializing instances with or
/// without a specified value.
/// </summary>
public class UInt32Extension : DataExtension<uint>
{
    /// <summary>
    /// Constructor for the UInt32Extension class. Initializes a new instance of the class.
    /// </summary>
    public UInt32Extension() { }

    /// <summary>
    /// Initializes a new instance of the UInt32Extension class with a specified unsigned integer value.
    /// </summary>
    /// <param name="value">The unsigned integer used to set the initial value of the instance.</param>
    public UInt32Extension(uint value)
        : base(value) { }
}

/// <summary>
/// Extends the functionality of the integer type. Provides constructors for creating instances with or without an
/// initial integer value.
/// </summary>
public class Int32Extension : DataExtension<int>
{
    /// <summary>
    /// Constructor for the Int32Extension class. Initializes a new instance of the class.
    /// </summary>
    public Int32Extension() { }

    /// <summary>
    /// Constructor for the Int32Extension class that initializes an instance with a specified integer value.
    /// </summary>
    /// <param name="value">The integer used to set the initial state of the instance.</param>
    public Int32Extension(int value)
        : base(value) { }
}

/// <summary>
/// Abstract class for data extensions that can hold a nullable value of a specified type.
/// </summary>
/// <typeparam name="T">Specifies the type of value that can be held, which may also be null.</typeparam>
[DefaultProperty(nameof(Value))]
[ContentProperty(nameof(Value))]
public abstract partial class DataExtension<T> : MarkupExtension
{
    /// <summary>
    /// Initializes a new instance of the DataExtension class. This constructor does not take any parameters.
    /// </summary>
    public DataExtension() { }

    /// <summary>
    /// Represents a nullable value of type T. It can hold a value of T or be null.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the DataExtension class with a specified value.
    /// </summary>
    /// <param name="value">The value to be assigned to the instance, which can be null.</param>
    protected DataExtension(T? value)
    {
        Value = value;
    }

    #region hide base function

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj"> The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    /// <summary>
    ///  Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
    {
        return base.ToString();
    }

    #endregion


    /// <summary>
    /// Returns a value based on the provided service provider context.
    /// </summary>
    /// <param name="serviceProvider">An interface that provides access to services required for value retrieval.</param>
    /// <returns>An object representing the value to be provided.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Value!;
}
