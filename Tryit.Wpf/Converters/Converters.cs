using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Static class providing various converters for boolean, enum, and visibility transformations. Includes converters for
/// null checks and color parsing.
/// </summary>
public static partial class Converters
{
    /// <summary>
    /// Represents a static object that holds the value of true as an object type. It can be used for comparisons or
    /// logical operations.
    /// </summary>
    static readonly object TrueObject = (object)true;

    /// <summary>
    /// Represents a static object that holds the value of false. It can be used as a reference for boolean false in
    /// various contexts.
    /// </summary>
    static readonly object FalseObject = (object)false;

    /// <summary>
    /// BooleanReverse is a static instance of BooleanConverter that reverses the true and false values. True is mapped
    /// to FalseObject and false to TrueObject.
    /// </summary>
    public static readonly BooleanConverter BooleanReverse = new() { True = FalseObject, False = TrueObject };

    /// <summary>
    /// Provides an instance of EnumDescriptionConverter for converting enum values to their descriptions.
    /// </summary>
    public static readonly EnumDescriptionConverter GetEnumDescription = new();

    /// <summary>
    /// Creates an instance of EnumDisplayNameConverter, which is used to convert enum values to their display names.
    /// </summary>
    public static readonly EnumDisplayNameConverter GetEnumDisplayName = new();

#if __WPF__

    /// <summary>
    /// Converts string representations of colors into Color objects. Useful for parsing color values in WPF applications.
    /// </summary>
    public static readonly ColorStringConverter StringToColor = new();

    /// <summary>
    /// Converts string representations to Brush objects. It facilitates the transformation of string data into a visual
    /// format.
    /// </summary>
    public static readonly BrushStringConverter StringToBrush = new();
#endif

    /// <summary>
    /// Creates a new instance of the LengthConverter class. This static member provides a way to convert lengths between
    /// different units.
    /// </summary>
    public static readonly LengthConverter GetLength = new();

    /// <summary>
    /// IsNullOrEmpty is a static instance of NullOrEmptyConverter. It provides True and False objects for null or empty
    /// checks.
    /// </summary>
    public static readonly NullOrEmptyConverter IsNullOrEmpty = new() { True = TrueObject, False = FalseObject };

    /// <summary>
    /// Creates a NotNullOrEmptyConverter instance that checks if a value is not null or empty. It has True and False
    /// objects for validation results.
    /// </summary>
    public static readonly NotNullOrEmptyConverter IsNotNullOrEmpty = new() { True = TrueObject, False = FalseObject };

    /// <summary>
    /// IsNullOrWhiteSpace is a static instance of NullOrWhiteSpaceConverter that defines True and False objects for null
    /// or whitespace checks.
    /// </summary>
    public static readonly NullOrWhiteSpaceConverter IsNullOrWhiteSpace = new() { True = TrueObject, False = FalseObject };

    /// <summary>
    /// IsNotNullOrWhiteSpace is a static instance of NotNullOrWhiteSpaceConverter. It provides True and False objects for
    /// validation.
    /// </summary>
    public static readonly NotNullOrWhiteSpaceConverter IsNotNullOrWhiteSpace = new() { True = TrueObject, False = FalseObject };

    /// <summary>
    /// IsNull is a static instance of NullConverter that holds references to TrueObject and FalseObject. It is used to
    /// represent null checks.
    /// </summary>
    public static readonly NullConverter IsNull = new() { True = TrueObject, False = FalseObject };

    /// <summary>
    /// IsNotNull is a static instance of NotNullConverter that defines True and False objects for null checks. It
    /// simplifies null validation.
    /// </summary>
    public static readonly NotNullConverter IsNotNull = new() { True = TrueObject, False = FalseObject };

#if __WPF__ || __MAUI__
    /// <summary>
    /// A static object representing the visibility state, initialized to 'Visible'. It is used in WPF or MAUI
    /// applications.
    /// </summary>
    static readonly object VisibleObject = (object)Visibility.Visible;

    /// <summary>
    /// Represents a collapsed visibility state as an object. It is used to indicate that an element is not visible.
    /// </summary>
    static readonly object CollapsedObject = (object)Visibility.Collapsed;

    /// <summary>
    /// Converts boolean values to visibility states. True maps to a visible object, while false maps to a collapsed object.
    /// </summary>
    public readonly static BooleanConverter BooleanToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// A BooleanConverter that reverses the visibility states, mapping true to CollapsedObject and false to
    /// VisibleObject.
    /// </summary>
    public readonly static BooleanConverter BooleanToVisibilityReverse = new() { True = CollapsedObject, False = VisibleObject };

    /// <summary>
    /// A converter that transforms null or empty values into visibility states. It maps true to a visible state and
    /// false to a collapsed state.
    /// </summary>
    public readonly static NullOrEmptyConverter IsNullOrEmptyToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// A converter that transforms a not-null or non-empty value into a visibility state. It maps true to a visible state
    /// and false to a collapsed state.
    /// </summary>
    public readonly static NotNullOrEmptyConverter IsNotNullOrEmptyToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// A converter that transforms null or whitespace values into visibility states. It maps true to a visible state and
    /// false to a collapsed state.
    /// </summary>
    public readonly static NullOrWhiteSpaceConverter IsNullOrWhiteSpaceToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// A converter that transforms a not-null or non-whitespace value into a visibility state. It sets visible for true
    /// and collapsed for false.
    /// </summary>
    public readonly static NotNullOrWhiteSpaceConverter IsNotNullOrWhiteSpaceToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// IsNullToVisibility is a static NullConverter instance that maps null values to visibility states. True values
    /// are set to VisibleObject, while false values are set to CollapsedObject.
    /// </summary>
    public readonly static NullConverter IsNullToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// IsNotNullToVisibility is a static NotNullConverter that converts a non-null value to a visible state and a null
    /// value to a collapsed state.
    /// </summary>
    public readonly static NotNullConverter IsNotNullToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// EqualConverter is a static instance that converts boolean values to visibility states. True maps to
    /// VisibleObject and False maps to CollapsedObject.
    /// </summary>
    public readonly static EqualConverter EqualToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    ///  GreaterThanToVisibility is a static instance of GreaterThanConverter that maps a true condition to VisibleObject
    /// and a false condition to CollapsedObject.
    /// </summary>
    public readonly static GreaterThanConverter GreaterThanToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// A static converter that maps boolean values to visibility states. True corresponds to a visible object, while
    /// false corresponds to a collapsed object.
    /// </summary>
    public readonly static GreaterThanOrEqualConverter GreaterThanOrEqualToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// LessThanToVisibility is a static instance of LessThanConverter that maps a true condition to VisibleObject and a
    /// false condition to CollapsedObject.
    /// </summary>
    public readonly static LessThanConverter LessThanToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// A converter that translates boolean values into visibility states. True results in a visible object, while false
    /// results in a collapsed object.
    /// </summary>
    public readonly static LessThanOrEqualConverter LessThanOrEqualToVisibility = new() { True = VisibleObject, False = CollapsedObject };

    /// <summary>
    /// NotEqualToVisibility is a static NotEqualConverter instance that maps a true value to VisibleObject and a false
    /// value to CollapsedObject.
    /// </summary>
    public readonly static NotEqualConverter NotEqualToVisibility = new() { True = VisibleObject, False = CollapsedObject };

#endif
}
