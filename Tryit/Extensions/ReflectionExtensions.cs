using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Reflection;

/// <summary>
/// Provides extension methods for reflection, allowing retrieval and setting of property or field values, including
/// nested paths. Supports collections and case-insensitive member access.
/// </summary>
public static class ReflectionExtensions
{
    #region get set path

    /// <summary>
    /// Retrieves the value of a specified property or field from an object, supporting nested paths.
    /// </summary>
    /// <param name="obj">The object from which to retrieve the value.</param>
    /// <param name="propertyOrFieldPath">A dot-separated string representing the path to the desired property or field.</param>
    /// <returns>The value found at the specified path, or null if not found.</returns>
    public static object? GetValue(object? obj, string propertyOrFieldPath)
    {
        if (obj is null || string.IsNullOrWhiteSpace(propertyOrFieldPath))
        {
            return obj;
        }

        string[] parts = propertyOrFieldPath.Split('.');
        object? currentObj = obj;

        foreach (string part in parts)
        {
            if (currentObj == null)
            {
                return null;
            }

            Type currentType = currentObj.GetType();

            if (currentType.IsCollectionType())
            {
                int index = GetCollectionIndex(part);
                currentObj = GetCollectionItem(currentObj, index);
                continue;
            }

            currentObj = FindMemberValueIgnoreCase(currentObj, part);
        }

        return currentObj;
    }

    /// <summary>
    /// Sets a value for a specified property or field on an object, supporting nested properties and collections.
    /// </summary>
    /// <param name="obj">The target object on which the property or field will be set.</param>
    /// <param name="propertyOrFieldPath">The path to the property or field, which can include nested properties.</param>
    /// <param name="value">The value to assign to the specified property or field.</param>
    /// <returns>Returns true if the value was successfully set; otherwise, false.</returns>
    public static bool SetValue(object? obj, string propertyOrFieldPath, object? value)
    {
        if (obj is null || string.IsNullOrWhiteSpace(propertyOrFieldPath))
        {
            return false;
        }

        string[] parts = propertyOrFieldPath.Split('.');
        object? currentObj = obj;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (currentObj == null)
            {
                return false;
            }

            Type currentType = currentObj.GetType();

            if (currentType.IsCollectionType())
            {
                int index = GetCollectionIndex(parts[i]);
                currentObj = GetCollectionItem(currentObj, index);
                continue;
            }

            currentObj = FindMemberValueIgnoreCase(currentObj, parts[i]);
        }

        if (currentObj != null)
        {
            string lastPropertyName = parts[parts.Length - 1];

            Type currentType = currentObj.GetType();

            if (currentType.IsCollectionType())
            {
                int index = GetCollectionIndex(lastPropertyName);
                SetCollectionItem(currentObj, index, value);

                return true;
            }

            FindMemberValueIgnoreCase(currentObj, lastPropertyName, true, value);

            return true;
        }

        return false;
    }

    private static int GetCollectionIndex(string part)
    {
        if (part.EndsWith("]"))
        {
            int startIndex = part.IndexOf("[");
            int endIndex = part.IndexOf("]");
            if (startIndex >= 0 && endIndex >= 0 && startIndex < endIndex)
            {
                string indexStr = part.Substring(startIndex + 1, endIndex - startIndex - 1);
                if (int.TryParse(indexStr, out int index))
                {
                    return index;
                }
            }
        }
        return -1;
    }

    private static object? GetCollectionItem(object? collection, int index)
    {
        if (index < 0)
        {
            throw new IndexOutOfRangeException("invalid index");
        }

        if (collection is IList list)
        {
            return list[index];
        }

        if (collection is IEnumerable enumerable)
        {
            return enumerable.Cast<object>().ElementAtOrDefault(index);
        }

        return null;
    }

    private static void SetCollectionItem(object? collection, int index, object? value)
    {
        if (collection is IList list)
        {
            if (index >= 0 && index < list.Count)
            {
                list[index] = value;
                return;
            }
            throw new IndexOutOfRangeException();
        }

        throw new InvalidOperationException($"The data type must be {typeof(IList)}");
    }

    /// <summary>
    /// Find Member value IgnoreCase
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="memberName"></param>
    /// <param name="isSetValue"></param>
    /// <param name="setValue"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static object? FindMemberValueIgnoreCase(object instance, string memberName, bool isSetValue = false, object? setValue = null)
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;

        Type type = instance.GetType();

        PropertyInfo? property = type.GetProperty(memberName, bindingFlags);
        if (property != null)
        {
            if (isSetValue)
            {
                property.SetValue(instance, setValue);
                return setValue;
            }

            return property.GetValue(instance);
        }

        FieldInfo? field = type.GetField(memberName, bindingFlags);
        if (field != null)
        {
            if (isSetValue)
            {
                field.SetValue(instance, setValue);
                return setValue;
            }

            return field.GetValue(instance);
        }

        throw new ArgumentException($"Property or Field '{memberName}' does not exist in object type '{type}'.");
    }

    private static bool IsCollectionType(this Type type, bool containsStringType = false)
    {
        if (containsStringType && type == typeof(string))
        {
            return false;
        }

        return typeof(IEnumerable).IsAssignableFrom(type);
    }

    #endregion

    #region get/set member value

    /// <summary>
    /// Retrieves the value of a specified member from an object, supporting nested members and collections.
    /// </summary>
    /// <param name="source">The object from which to retrieve the member value.</param>
    /// <param name="memberPath">A string representing the path to the member, which can include nested members and collection indices.</param>
    /// <param name="ignoreCase">Indicates whether to ignore case when matching member names.</param>
    /// <returns>The value of the specified member or collection element.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source object is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the member path is null or empty.</exception>
    public static object GetMemberValue(object source, string memberPath, bool ignoreCase = false)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (string.IsNullOrWhiteSpace(memberPath))
        {
            throw new ArgumentException("Member path cannot be null or empty.", nameof(memberPath));
        }

        return InnerGetValue(source, memberPath.ToCharArray(), 0, ignoreCase);

        static object InnerGetValue(object source, char[] chars, int index = 0, bool ignoreCase = false)
        {
            int last = chars.Length - 1;

            for (int i = index, length = chars.Length; i < length; i++)
            {
                // member
                if (chars[i] is '.' or '[')
                {
                    string memberName = new string(chars, index, i - index);

                    object currentValue = GetValue(source, memberName, ignoreCase);

                    return InnerGetValue(currentValue, chars, i + 1, ignoreCase);
                }

                //collection
                if (chars[i] == ']')
                {
                    string memberName = new string(chars, index, i - index);
                    int offset = int.Parse(memberName);

                    object currentValue = GetCollectionValue(source as IEnumerable, offset);

                    offset = i + 1;

                    if (offset < length && (chars[offset] is '[' or '.'))
                    {
                        offset += 1;
                    }

                    return InnerGetValue(currentValue, chars, offset, ignoreCase);
                }

                // member
                if (i == last)
                {
                    string memberName = new string(chars, index, i - index + 1);

                    object currentValue = GetValue(source, memberName, ignoreCase);

                    return currentValue;
                }
            }

            return source;
        }
    }

    /// <summary>
    /// Sets the value of a specified member in an object using a member path string.
    /// </summary>
    /// <param name="source">The object from which the member value will be set.</param>
    /// <param name="memberPath">A string representing the path to the member to be set.</param>
    /// <param name="value">The new value to assign to the specified member.</param>
    /// <param name="ignoreCase">Indicates whether member names should be matched case-insensitively.</param>
    /// <exception cref="ArgumentNullException">Thrown when the source object is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the member path is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the source is not of the expected type for collection operations.</exception>
    public static void SetMemberValue(object source, string memberPath, object value, bool ignoreCase = false)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (string.IsNullOrWhiteSpace(memberPath))
        {
            throw new ArgumentException("Member path cannot be null or empty.", nameof(memberPath));
        }

        InnerSetValue(source, memberPath.ToCharArray(), 0, value, ignoreCase);

        static void InnerSetValue(object source, char[] chars, int index, object value, bool ignoreCase)
        {
            int last = chars.Length - 1;

            for (int i = index, length = chars.Length; i < length; i++)
            {
                // member
                if (chars[i] is '.' or '[')
                {
                    string memberName = new string(chars, index, i - index);

                    object currentValue = GetValue(source, memberName, ignoreCase);

                    InnerSetValue(currentValue, chars, i + 1, value, ignoreCase);

                    return;
                }

                // collection
                if (chars[i] == ']')
                {
                    string memberName = new string(chars, index, i - index);
                    int offset = int.Parse(memberName);

                    if (i == last)
                    {
                        if (source is IList list)
                        {
                            list[offset] = value;
                            return;
                        }

                        throw new InvalidOperationException($"object type must be {typeof(IList).FullName}");
                    }

                    object currentValue = GetCollectionValue(source as IEnumerable, offset);

                    offset = i + 1;

                    if (offset < length && (chars[offset] is '[' or '.'))
                    {
                        offset += 1;
                    }

                    InnerSetValue(currentValue, chars, offset, value, ignoreCase);

                    return;
                }

                // member
                if (i == last)
                {
                    string memberName = new string(chars, index, i - index + 1);

                    SetValue(source, memberName, value, ignoreCase);

                    return;
                }
            }
        }
    }

    /// <summary>
    /// Sets the value of a specified member (property or field) of an object using reflection.
    /// </summary>
    /// <param name="obj">The object whose member value is to be set.</param>
    /// <param name="memberName">The name of the member (property or field) to be modified.</param>
    /// <param name="value">The new value to assign to the specified member.</param>
    /// <param name="ignoreCase">Indicates whether the member name search should be case-insensitive.</param>
    /// <exception cref="InvalidOperationException">Thrown when the specified member cannot be found in the object.</exception>
    private static void SetValue(object obj, string memberName, object value, bool ignoreCase = false)
    {
        BindingFlags bindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        if (ignoreCase)
        {
            bindingFlag |= BindingFlags.IgnoreCase;
        }

        Type dataType = obj.GetType();

        PropertyInfo? property = dataType.GetProperty(memberName, bindingFlag);

        if (property is not null)
        {
            property.SetValue(obj, value);
            return;
        }

        FieldInfo? field = dataType.GetField(memberName, bindingFlag);

        if (field is not null)
        {
            field.SetValue(obj, value);
            return;
        }

        throw new InvalidOperationException($"Member variable '{memberName}' not found.");
    }

    /// <summary>
    /// Retrieves an item from a collection at a specified position.
    /// </summary>
    /// <param name="objects">The collection from which an item is to be retrieved.</param>
    /// <param name="offset">Specifies the position of the item to retrieve from the collection.</param>
    /// <returns>The item located at the specified position in the collection.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when the specified position exceeds the bounds of the collection.</exception>
    private static object GetCollectionValue(IEnumerable? objects, int offset)
    {
        if (objects is null)
        {
            return null!;
        }

        int index = 0;
        foreach (object? item in objects)
        {
            if (index == offset)
            {
                return item!;
            }
            index++;
        }

        throw new IndexOutOfRangeException($"{offset}: out of range");
    }

    /// <summary>
    /// Retrieves the value of a specified property or field from an object.
    /// </summary>
    /// <param name="obj">The object from which to retrieve the property or field value.</param>
    /// <param name="memberName">The name of the property or field whose value is to be retrieved.</param>
    /// <param name="ignoreCase">Indicates whether the search for the member name should be case-insensitive.</param>
    /// <returns>The value of the specified property or field if found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified member cannot be found in the object.</exception>
    private static object GetValue(object obj, string memberName, bool ignoreCase = false)
    {
        BindingFlags bindingFlag = Instance | Public | NonPublic;

        if (ignoreCase)
        {
            bindingFlag |= IgnoreCase;
        }

        Type dataType = obj.GetType();

        PropertyInfo? property = dataType.GetProperty(memberName, bindingFlag);

        if (property is not null)
        {
            return property.GetValue(obj)!;
        }

        FieldInfo? field = dataType.GetField(memberName, bindingFlag);

        if (field is not null)
        {
            return field.GetValue(obj)!;
        }

        throw new InvalidOperationException($"member variable:{memberName} not found");
    }

    #endregion

    /// <summary>
    /// Retrieves the explicit name of a given type, including its generic type arguments if present. It formats the name
    /// appropriately for display.
    /// </summary>
    /// <param name="type">A representation of a data structure that defines the characteristics of a class or interface.</param>
    /// <returns>A string that represents the explicit name of the type, formatted with any generic type arguments.</returns>
    public static string GetExplicitName(this Type type)
    {
        string typeName = type.Name;

        Type[] typeArguments = type.GenericTypeArguments;

        if (typeArguments != null && typeArguments.Length > 0)
        {
            typeName = type.Name.Replace($"`{typeArguments.Length}", "");

            string typeArgumentString = string.Join(",", typeArguments.Select(genericType => GetExplicitName(genericType)));

            return $"{typeName}<{typeArgumentString}>";
        }

        return typeName;
    }

    /// <summary>
    /// Retrieves a resource stream from a specified assembly based on the resource name.
    /// </summary>
    /// <param name="assembly">Specifies the assembly from which to retrieve the resource.</param>
    /// <param name="resourceName">Indicates the name of the resource to search for within the assembly.</param>
    /// <param name="stringComparison">Determines how the resource name is compared to the names in the assembly.</param>
    /// <returns>Returns a stream of the resource if found, otherwise returns null.</returns>
    public static Stream? GetAssemblyResource(Assembly assembly, string resourceName, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        string? resourceFullName = assembly.GetManifestResourceNames().Where(i => i.IndexOf(resourceName, stringComparison) > 0).SingleOrDefault();

        if (string.IsNullOrWhiteSpace(resourceFullName))
        {
            return null;
        }

        Stream? stream = assembly.GetManifestResourceStream(resourceFullName);

        if (stream?.CanSeek ?? false)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return stream;
    }

    /// <summary>
    /// Invokes a method on a specified object using reflection, allowing for both public and private method access.
    /// </summary>
    /// <typeparam name="Target">Specifies the type of the object on which the method will be invoked.</typeparam>
    /// <param name="target">The object instance on which the method is to be called.</param>
    /// <param name="methodName">The name of the method to invoke on the specified object.</param>
    /// <param name="params">An array of parameters to pass to the method being invoked.</param>
    /// <returns>Returns the result of the method invocation or null if the target is null or the method is not found.</returns>
    public static object? InvokeMethod<Target>(Target target, string methodName, params object[] @params)
        where Target : class
    {
        if (target is null)
        {
            return default;
        }

        Type[]? types = @params?.Select(i => i.GetType()).ToArray();

        BindingFlags binf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        Type type = target.GetType()!;

        Type baseType = typeof(object);

        do
        {
            if (type!.GetMethod(methodName, binf, null, types!, null) is not MethodInfo methodInfo)
            {
                type = type.BaseType!;

                if (type == baseType)
                {
                    break;
                }

                continue;
            }

            object? value = methodInfo.Invoke(target, @params);

            return value;
        } while (true);

        return default;
    }

    /// <summary>
    /// Invokes a specified method on a given object asynchronously, handling both task and non-task return types.
    /// </summary>
    /// <typeparam name="Target">Represents the type of the object on which the method will be invoked.</typeparam>
    /// <param name="target">The object instance on which the specified method will be called.</param>
    /// <param name="methodName">The name of the method to invoke on the target object.</param>
    /// <param name="params">An array of parameters to pass to the method being invoked.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    public static async Task InvokeMethodAsync<Target>(Target target, string methodName, params object[] @params)
        where Target : class
    {
        if (target is null)
        {
            return;
        }

        Type[]? types = @params?.Select(i => i.GetType()).ToArray();

        BindingFlags binf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        System.Reflection.MethodInfo? methodInfo = target.GetType().GetMethod(methodName, binf, null, types!, null);
        if (methodInfo is null)
        {
            return;
        }

        if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
        {
            await (methodInfo.Invoke(target, @params) as Task)!;
            return;
        }

#if NET451
        await Task.FromResult(0);
#else
        await Task.CompletedTask;
#endif

        _ = methodInfo.Invoke(target, @params);
    }

    /// <summary>
    /// Retrieves the value of a specified property from a given object, returning it as a nullable type.
    /// </summary>
    /// <typeparam name="Target">Specifies the type of the property value to be retrieved from the object.</typeparam>
    /// <param name="target">The object from which the property value will be extracted.</param>
    /// <param name="propertyName">The name of the property whose value is to be retrieved from the object.</param>
    /// <returns>Returns the property value as the specified type or null if the property does not exist or the object is null.</returns>

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Target? GetPropertyValue<Target>(object target, string propertyName)
    {
        if (target is null)
        {
            return default;
        }

        BindingFlags binf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        PropertyInfo? propertyInfo = target.GetType().GetProperty(propertyName, binf);

        if (propertyInfo is null)
        {
            return default;
        }

        object? propertyValue = propertyInfo.GetValue(target);

        return propertyValue is Target @switch ? @switch : default;
    }

    /// <summary>
    /// Sets the value of a specified property on a given object if the object is not null.
    /// </summary>
    /// <typeparam name="Target">Represents the type of the value being assigned to the property.</typeparam>
    /// <param name="target">The object whose property value is to be set.</param>
    /// <param name="propertyName">The name of the property to be modified on the target object.</param>
    /// <param name="value">The new value to assign to the specified property.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetPropertyValue<Target>(object target, string propertyName, Target value)
    {
        if (target is null)
        {
            return;
        }

        BindingFlags binf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        PropertyInfo? propertyInfo = target.GetType().GetProperty(propertyName, binf);

        propertyInfo?.SetValue(target, value);
    }

    /// <summary>
    /// Sets the value of a specified property on a given object using a lambda expression to identify the property.
    /// </summary>
    /// <typeparam name="TObject">Represents the type of the object that contains the property to be set.</typeparam>
    /// <typeparam name="TProperty">Represents the type of the property value that will be assigned to the target object.</typeparam>
    /// <param name="target">The object whose property value will be modified.</param>
    /// <param name="filter">A lambda expression used to specify which property of the target object to set.</param>
    /// <param name="value">The new value to assign to the specified property of the target object.</param>
    public static void SetPropertyValue<TObject, TProperty>(TObject target, Expression<Func<TObject, TProperty>> filter, TProperty value)
    {
        if (target is null || filter is null)
        {
            return;
        }

        string propertyName = ReflectionExtensions.GetPropertyName(filter);

        BindingFlags binf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        PropertyInfo? propertyInfo = target.GetType().GetProperty(propertyName, binf);

        propertyInfo?.SetValue(target, value);
    }

    /// <summary>
    /// Retrieves the name of a property from a given expression.
    /// </summary>
    /// <typeparam name="TSource">Represents the type of the object that contains the property.</typeparam>
    /// <typeparam name="TPropertyType">Represents the type of the property being accessed.</typeparam>
    /// <param name="propertySelector">An expression that selects the property whose name is to be retrieved.</param>
    /// <returns>Returns the name of the property as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided expression for selecting the property is null.</exception>
    public static string GetPropertyName<TSource, TPropertyType>(Expression<Func<TSource, TPropertyType>> propertySelector)
    {
        if (propertySelector is null)
        {
            throw new ArgumentNullException(nameof(propertySelector));
        }

        if (propertySelector.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        UnaryExpression? unaryExpression = propertySelector.Body as UnaryExpression;

        return unaryExpression?.Operand is MemberExpression memberExpression2 ? memberExpression2.Member.Name : string.Empty;
    }

    /// <summary>
    /// Stores a mapping of types to their associated attributes in a thread-safe manner. Utilizes a
    /// ConcurrentDictionary for concurrent access.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Dictionary<string, Attribute[]>> attributeMapper = new();

    /// <summary>
    /// Retrieves a specific attribute associated with an enumeration value.
    /// </summary>
    /// <typeparam name="TAttribute">Specifies the type of attribute to retrieve from the enumeration value.</typeparam>
    /// <param name="enumValue">The enumeration value from which to retrieve the associated attribute.</param>
    /// <returns>Returns the requested attribute of the specified type, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided enumeration value is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the provided value is not of an enumeration type.</exception>
    public static TAttribute GetEnumAttribute<TAttribute>(Enum enumValue)
        where TAttribute : Attribute
    {
        _ = enumValue ?? throw new ArgumentNullException(nameof(enumValue));

        Type enumType = enumValue.GetType();

        if (enumType.IsEnum == false)
        {
            throw new InvalidOperationException("invalid data type");
        }

        if (attributeMapper.TryGetValue(enumType, out Dictionary<string, Attribute[]>? mapper) == false)
        {
            Type type = enumValue.GetType();
            string[] allNames = Enum.GetNames(type);

            Dictionary<string, Attribute[]> valueAttributes = new();

            foreach (string item in allNames)
            {
                FieldInfo field = type.GetField(item)!;
                string currentLongValue = field.GetValue(null)!.ToString()!;

                valueAttributes[currentLongValue] = field.GetCustomAttributes().OfType<Attribute>().ToArray();
            }

            attributeMapper[enumType] = mapper = valueAttributes;
        }
        string longValue = enumValue.ToString()!;
        return mapper[longValue].OfType<TAttribute>().FirstOrDefault()!;
    }

    /// <summary>
    /// Retrieves a dictionary of property information and associated attributes for a specified class type.
    /// </summary>
    /// <typeparam name="TObjectType">Represents the class type from which properties and their attributes are extracted.</typeparam>
    /// <typeparam name="TAttribute">Specifies the type of attribute to be retrieved from the properties of the class.</typeparam>
    /// <param name="removeNull">Indicates whether to exclude properties that do not have the specified attribute.</param>
    /// <returns>A dictionary mapping property information to their corresponding attributes, filtered based on the removeNull
    /// parameter.</returns>
    public static IDictionary<PropertyInfo, TAttribute> GetPropertyAttributes<TObjectType, TAttribute>(bool removeNull = false)
        where TAttribute : Attribute
        where TObjectType : class
    {
        PropertyInfo[] fields = typeof(TObjectType).GetProperties(BindingFlags.Instance | BindingFlags.Public).ToArray();

        Dictionary<PropertyInfo, TAttribute> dict = fields.Select(i => new { Mode = i, Attribute = i.GetCustomAttribute<TAttribute>() }).ToDictionary(i => i.Mode, i => i.Attribute!);

        if (removeNull == false)
        {
            return dict;
        }

        return dict.Where(i => i.Value != null).ToDictionary(i => i.Key, i => i.Value);
    }

    /// <summary>
    /// Retrieves a dictionary of field information and associated attributes for a specified class type.
    /// </summary>
    /// <typeparam name="TObjectType">Specifies the class type from which to retrieve field attributes.</typeparam>
    /// <typeparam name="TAttribute">Defines the type of attribute to be retrieved from the fields.</typeparam>
    /// <returns>A dictionary mapping field information to their corresponding attributes.</returns>
    public static IDictionary<FieldInfo, TAttribute> GetFieldAttributes<TObjectType, TAttribute>()
        where TAttribute : Attribute
        where TObjectType : class
    {
        FieldInfo[] fields = typeof(TObjectType).GetFields(BindingFlags.Instance | BindingFlags.Public).ToArray();

        return fields.Select(i => new { Mode = i, Attribute = i.GetCustomAttribute<TAttribute>() }).ToDictionary(i => i.Mode, i => i.Attribute!);
    }

    /// <summary>
    /// Retrieves a dictionary mapping enum values to their associated attributes.
    /// </summary>
    /// <typeparam name="TEnum">Represents an enumeration type that is used to define a set of named constants.</typeparam>
    /// <typeparam name="TAttribute">Represents a custom attribute type that can be associated with the enum values.</typeparam>
    /// <returns>A dictionary where each key is an enum value and each value is the corresponding attribute.</returns>
    public static IDictionary<TEnum, TAttribute> GetEnumAttributes<TEnum, TAttribute>()
        where TAttribute : Attribute
        where TEnum : struct, Enum
    {
        FieldInfo[] fields = typeof(TEnum).GetFields().Where(i => i.IsStatic).ToArray();

        return fields.Select(i => new { Mode = (TEnum)i.GetValue(null)!, Attribute = i.GetCustomAttribute<TAttribute>() }).ToDictionary(i => i.Mode, i => i.Attribute!);
    }

    /// <summary>
    /// Retrieves a dictionary mapping enum values to their associated attributes using a specified selector function.
    /// </summary>
    /// <typeparam name="TEnum">Represents an enumeration type that is used to define the keys in the resulting dictionary.</typeparam>
    /// <typeparam name="TAttribute">Represents the type of attribute associated with the enum values, which is used to extract additional
    /// information.</typeparam>
    /// <typeparam name="TResult">Represents the type of the result produced by applying the selector function to the attributes.</typeparam>
    /// <param name="selector">A function that transforms the attribute into a desired result type for each enum value.</param>
    /// <returns>A dictionary where each key is an enum value and each value is the result of applying the selector to the
    /// corresponding attribute.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the selector function provided is null.</exception>
    public static IDictionary<TEnum, TResult> GetEnumAttributes<TEnum, TAttribute, TResult>(Func<TAttribute, TResult> selector)
        where TAttribute : Attribute
        where TEnum : struct, Enum
    {
        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        FieldInfo[] fields = typeof(TEnum).GetFields().Where(i => i.IsStatic).ToArray();

        return fields.Select(i => new { Mode = (TEnum)i.GetValue(null)!, Attribute = i.GetCustomAttribute<TAttribute>() }).ToDictionary(i => i.Mode, i => selector(i.Attribute!));
    }

    /// <summary>
    /// Checks if the type of the given object matches or is assignable from a specified type.
    /// </summary>
    /// <typeparam name="T">Represents the type of the object being checked for compatibility with the specified type.</typeparam>
    /// <param name="target">The object whose type is being evaluated for compatibility with the specified type.</param>
    /// <param name="type">The type against which the object's type is being checked for compatibility.</param>
    /// <returns>Returns true if the object's type matches or is assignable from the specified type, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the object or the specified type is null.</exception>
    public static bool IsAssignableFrom<T>(this T target, Type type)
    {
        _ = target ?? throw new ArgumentNullException(nameof(target));
        _ = type ?? throw new ArgumentNullException(nameof(type));

        var targetType = target.GetType();

        return targetType == type || type.IsAssignableFrom(targetType);
    }
}
