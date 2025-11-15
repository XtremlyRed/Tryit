using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace Tryit;

/// <summary>
/// Defines a contract for storing and retrieving values, optionally associated with keys, in a type-safe manner.
/// </summary>
/// <remarks>Implementations of this interface provide generic methods to set and get values by type or by key.
/// The interface supports both direct and key-based access, as well as methods to attempt retrieval without throwing
/// exceptions if a value is not present. All type parameters must be non-nullable. Callers should ensure that keys and
/// values are not null when required by the method signatures.</remarks>
public interface IContext
{
    /// <summary>
    /// Retrieves the stored value as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the value is cast. Must be a non-nullable type.</typeparam>
    /// <returns>The value cast to type <typeparamref name="T"/>.</returns>
    T GetValue<T>()
        where T : notnull;

    /// <summary>
    /// Sets the value to be stored or processed by the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to set. Must be a non-nullable type.</typeparam>
    /// <param name="value">The value to assign. Cannot be null.</param>
    void SetValue<T>(T value)
        where T : notnull;

    /// <summary>
    /// Attempts to retrieve a value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="value">When this method returns, contains the retrieved value if the operation succeeds; otherwise, the default value
    /// for the type.</param>
    /// <returns>true if the value was successfully retrieved; otherwise, false.</returns>
    bool TryGetValue<T>(out T value);

    /// <summary>
    /// Retrieves the value associated with the specified key and converts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the value is converted. Must be a non-nullable type.</typeparam>
    /// <param name="key">The key whose associated value is to be retrieved. Cannot be null.</param>
    /// <returns>The value associated with the specified key, converted to type T.</returns>
    T GetValue<T>(string key)
        where T : notnull;

    /// <summary>
    /// Sets the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to store. Must be a non-nullable type.</typeparam>
    /// <param name="key">The key with which the value will be associated. Cannot be null.</param>
    /// <param name="value">The value to set for the specified key. Cannot be null.</param>
    void SetValue<T>(string key, T value)
        where T : notnull;

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key whose value to retrieve. Cannot be null.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise,
    /// the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if the key was found and the value was retrieved successfully; otherwise, false.</returns>
    bool TryGetValue<T>(string key, out T value);
}

/// <summary>
/// Represents an execution context that provides access to contextual information and services for operations.
/// </summary>
/// <remarks>Use the Context class to access shared data, configuration, or services relevant to the current
/// operation or request. This class typically extends ContextBase and implements IContext to provide a consistent
/// interface for context management across different components.</remarks>
public class Context : ContextBase, IContext { }

/// <summary>
/// Provides a base implementation for a context that supports storing and retrieving values by type or by key. Intended
/// for use as a foundation for custom context implementations that require type-safe and key-based value access.
/// </summary>
/// <remarks>This abstract class is not intended to be used directly. It defines core mechanisms for associating
/// values with types or string keys, supporting both retrieval and update operations. Implementations should ensure
/// proper disposal semantics if resource management is required. Thread safety is provided for value storage and
/// retrieval operations. Members of this class are not intended to be accessed directly from user code; use derived
/// context types instead.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class ContextBase : IContext
{
    /// <summary>
    ///
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ConcurrentDictionary<Type, object> typeCaches = new();

    /// <summary>
    ///
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ConcurrentDictionary<string, object> keyCaches = new();

    /// <summary>
    /// Retrieves a value of the specified type from the context.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve. Must be a non-nullable reference or value type.</typeparam>
    /// <returns>The value of type <typeparamref name="T"/> retrieved from the context.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no value of type <typeparamref name="T"/> is available in the context.</exception>
    public virtual T GetValue<T>()
        where T : notnull
    {
        _ = typeCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        return typeCaches.TryGetValue(typeof(T), out object? cacheValue) && cacheValue is T target ? target : throw new InvalidOperationException($"no valid data matched from context by:{typeof(T).FullName}");
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and casts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve. Must be a non-nullable type.</typeparam>
    /// <param name="key">The key whose associated value is to be retrieved. Cannot be null, empty, or consist only of white-space
    /// characters.</param>
    /// <returns>The value associated with the specified key, cast to type T.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    /// <exception cref="ArgumentException">Thrown if key is null, empty, or consists only of white-space characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no value of type T is associated with the specified key.</exception>
    public virtual T GetValue<T>(string key)
        where T : notnull
    {
        _ = keyCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        return string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("toke is null or empty")
            : keyCaches.TryGetValue(key, out object? cacheValue) && cacheValue is T target ? target
            : throw new InvalidOperationException($"no valid data matched from context by:{key}");
    }

    /// <summary>
    /// Sets the value associated with the specified type parameter.
    /// </summary>
    /// <typeparam name="T">The type of the value to set. Must be a non-nullable type.</typeparam>
    /// <param name="value">The value to associate with the type parameter. Cannot be null.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the underlying storage has been disposed.</exception>
    public void SetValue<T>(T value)
        where T : notnull
    {
        _ = typeCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        Type dataType = typeof(T);

        typeCaches[dataType] = value;
    }

    /// <summary>
    /// Sets the value associated with the specified key. If the key already exists, its value is updated.
    /// </summary>
    /// <typeparam name="T">The type of the value to associate with the specified key. Must be a non-nullable type.</typeparam>
    /// <param name="key">The key with which the value will be associated. Cannot be null, empty, or consist only of white-space
    /// characters.</param>
    /// <param name="value">The value to associate with the specified key.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the underlying cache has been disposed.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null, empty, or consists only of white-space characters.</exception>
    public void SetValue<T>(string key, T value)
        where T : notnull
    {
        _ = keyCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        _ = string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("key is null or empty") : 1;

        keyCaches[key] = value;
    }

    /// <summary>
    /// Attempts to retrieve a cached value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve from the cache.</typeparam>
    /// <param name="value">When this method returns, contains the value associated with the specified type if found; otherwise, the default
    /// value for the type.</param>
    /// <returns>true if a value of the specified type was found in the cache; otherwise, false.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the cache has been disposed.</exception>
    public virtual bool TryGetValue<T>(out T value)
    {
        _ = typeCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        if (typeCaches.TryGetValue(typeof(T), out object? cacheValue) && cacheValue is T target)
        {
            value = target;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key and type parameter from the cache.
    /// </summary>
    /// <remarks>If the key exists in the cache but the associated value is not of the specified type, the
    /// method returns false and value is set to the default value for type T.</remarks>
    /// <typeparam name="T">The type of the value to retrieve from the cache.</typeparam>
    /// <param name="key">The key whose associated value is to be retrieved. Cannot be null, empty, or consist only of white-space
    /// characters.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key if the key is found and the value
    /// is of type <typeparamref name="T"/>; otherwise, the default value for the type of the value parameter.</param>
    /// <returns>true if the cache contains an entry with the specified key and the value is of type <typeparamref name="T"/>;
    /// otherwise, false.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the cache has been disposed.</exception>
    /// <exception cref="ArgumentException">Thrown if key is null, empty, or consists only of white-space characters.</exception>
    public virtual bool TryGetValue<T>(string key, out T value)
    {
        _ = keyCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        _ = string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("key is null or empty") : 1;

        if (keyCaches.TryGetValue(key, out object? cacheValue) && cacheValue is T target)
        {
            value = target;
            return true;
        }

        value = default!;
        return false;
    }

    #region override object

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    /// <summary>
    /// Serves as the default hash function.
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
}
