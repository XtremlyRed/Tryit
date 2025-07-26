using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace Tryit;

/// <summary>
/// 表示一个上下文对象，用于存储和检索类型化的值。
/// </summary>
public interface IContext
{
    /// <summary>
    /// 获取指定类型的值。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <returns>类型为 <typeparamref name="T"/> 的值。</returns>
    T GetValue<T>()
        where T : notnull;

    /// <summary>
    /// 设置指定类型的值。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="value">要设置的值。</param>
    void SetValue<T>(T value)
        where T : notnull;

    /// <summary>
    /// 尝试获取指定类型的值。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="value">如果找到则返回该值，否则为默认值。</param>
    /// <returns>如果找到值则为 true，否则为 false。</returns>
    bool TryGetValue<T>(out T value);

    /// <summary>
    /// 获取指定键和类型的值。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="key">值的键。</param>
    /// <returns>类型为 <typeparamref name="T"/> 的值。</returns>
    T GetValue<T>(string key)
        where T : notnull;

    /// <summary>
    /// 设置指定键和类型的值。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="key">值的键。</param>
    /// <param name="value">要设置的值。</param>
    void SetValue<T>(string key, T value)
        where T : notnull;

    /// <summary>
    /// 尝试获取指定键和类型的值。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="key">值的键。</param>
    /// <param name="value">如果找到则返回该值，否则为默认值。</param>
    /// <returns>如果找到值则为 true，否则为 false。</returns>
    bool TryGetValue<T>(string key, out T value);
}

/// <summary>
/// 表示上下文对象，继承自 <see cref="ContextBase"/>，实现 <see cref="IContext"/> 接口，
/// 用于存储和检索类型化的值。
/// </summary>
public class Context : ContextBase, IContext { }

/// <summary>
/// 表示上下文基类，实现 <see cref="IContext"/> 接口，提供类型化和键值对的数据存储与检索功能。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class ContextBase : IContext
{
    /// <summary>
    /// 类型缓存字典，用于存储按类型索引的数据。
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ConcurrentDictionary<Type, object> typeCaches = new();

    /// <summary>
    /// 键缓存字典，用于存储按字符串键索引的数据。
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ConcurrentDictionary<string, object> keyCaches = new();

    /// <inheritdoc/>
    public virtual T GetValue<T>()
        where T : notnull
    {
        _ = typeCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        return typeCaches.TryGetValue(typeof(T), out object? cacheValue) && cacheValue is T target ? target : throw new InvalidOperationException($"no valid data matched from context by:{typeof(T).FullName}");
    }

    /// <inheritdoc/>
    public virtual T GetValue<T>(string key)
        where T : notnull
    {
        _ = keyCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        return string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("toke is null or empty")
            : keyCaches.TryGetValue(key, out object? cacheValue) && cacheValue is T target ? target
            : throw new InvalidOperationException($"no valid data matched from context by:{key}");
    }

    /// <inheritdoc/>
    public void SetValue<T>(T value)
        where T : notnull
    {
        _ = typeCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        Type dataType = typeof(T);

        typeCaches[dataType] = value;
    }

    /// <inheritdoc/>
    public void SetValue<T>(string key, T value)
        where T : notnull
    {
        _ = keyCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        _ = string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("toke is null or empty") : 1;

        keyCaches[key] = value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public virtual bool TryGetValue<T>(string key, out T value)
    {
        _ = keyCaches ?? throw new ObjectDisposedException(nameof(typeCaches));

        _ = string.IsNullOrWhiteSpace(key) ? throw new ArgumentException("toke is null or empty") : 1;

        if (keyCaches.TryGetValue(key, out object? cacheValue) && cacheValue is T target)
        {
            value = target;
            return true;
        }
        value = default!;
        return false;
    }

    #region override object

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
    {
        return base.ToString();
    }

    #endregion
}
