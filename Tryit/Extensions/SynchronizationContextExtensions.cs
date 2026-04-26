using System.Runtime.InteropServices;

namespace System.Threading;

/// <summary>
/// Provides extension methods for <see cref="SynchronizationContext"/> to schedule delegates on a target context,
/// with support for fire-and-forget posting and awaitable completion semantics.
/// </summary>
/// <remarks>
/// <para>
/// This class offers a unified API surface for posting work to a specific synchronization context (for example, UI thread contexts),
/// including overloads for:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="Action"/> and parameterized <see cref="Action{T}"/> delegates.</description></item>
/// <item><description><see cref="Func{TResult}"/> and parameterized function delegates that return values.</description></item>
/// <item><description>Asynchronous delegates returning <see cref="Task"/> or <see cref="Task{TResult}"/>.</description></item>
/// </list>
/// <para>
/// The <c>Post</c> overloads schedule work without waiting for completion, while <c>PostAsync</c> overloads return
/// <see cref="ValueTask"/> / <see cref="ValueTask{TResult}"/> that complete when the posted delegate finishes.
/// </para>
/// <para>
/// Exceptions thrown by posted delegates are captured and propagated through the returned awaitable in <c>PostAsync</c> methods.
/// </para>
/// <para>
/// Internally, each awaitable operation is bridged through <see cref="TaskCompletionSource{TResult}"/>-based state objects.
/// </para>
/// </remarks>
public static class SynchronizationContextExtensions
{
    class PostFuncMap<T>(Func<T> action) : TaskCompletionSource<T>
    {
        public Func<T> Action = action;
    }

    class PostFuncMap<T1, T>(T1 parameter1, Func<T1, T> action) : TaskCompletionSource<T>
    {
        public Func<T1, T> Action = action;
        public T1 Parameter1 = parameter1;
    }

    class PostFuncMap<T1, T2, T>(T1 parameter1, T2 parameter2, Func<T1, T2, T> action) : TaskCompletionSource<T>
    {
        public Func<T1, T2, T> Action = action;
        public T1 Parameter1 = parameter1;
        public T2 Parameter2 = parameter2;
    }

    class PostFuncMapAsync<T>(Func<Task<T>> action) : TaskCompletionSource<T>
    {
        public Func<Task<T>> Action = action;
    }

    class PostFuncMapAsync<T1, T>(T1 parameter1, Func<T1, Task<T>> action) : TaskCompletionSource<T>
    {
        public Func<T1, Task<T>> Action = action;
        public T1 Parameter1 = parameter1;
    }

    class PostFuncMapAsync<T1, T2, T>(T1 parameter1, T2 parameter2, Func<T1, T2, Task<T>> action) : TaskCompletionSource<T>
    {
        public Func<T1, T2, Task<T>> Action = action;
        public T1 Parameter1 = parameter1;
        public T2 Parameter2 = parameter2;
    }

    class PostActionMap(Action action) : TaskCompletionSource<bool>
    {
        public Action Action = action;
    }

    class PostActionMap<T1>(T1 parameter1, Action<T1> action) : TaskCompletionSource<bool>
    {
        public Action<T1> Action = action;
        public T1 Parameter1 = parameter1;
    }

    class PostActionMap<T1, T2>(T1 parameter1, T2 parameter2, Action<T1, T2> action) : TaskCompletionSource<bool>
    {
        public Action<T1, T2> Action = action;
        public T1 Parameter1 = parameter1;
        public T2 Parameter2 = parameter2;
    }

    /// <summary>
    /// Posts the specified delegate to the provided SynchronizationContext for asynchronous execution.
    /// </summary>
    /// <remarks>This method allows code to be executed asynchronously on the specified synchronization
    /// context, which is commonly used to marshal work onto a particular thread or context, such as a UI
    /// thread.</remarks>
    /// <param name="context">The SynchronizationContext to which the delegate will be posted. Cannot be null.</param>
    /// <param name="action">The delegate to invoke asynchronously. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if either context or action is null.</exception>
    public static void Post(this SynchronizationContext context, Action action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostActionMap(action);

        context.Post(
            static o =>
            {
                if (o is PostActionMap postMap)
                {
                    postMap.Action();
                }
            },
            postMap
        );
    }

    /// <summary>
    /// Posts an action to the specified SynchronizationContext, passing a single parameter to the action when it is
    /// executed asynchronously.
    /// </summary>
    /// <remarks>This method is useful for posting parameterized work to a SynchronizationContext, such as
    /// updating UI elements from a background thread. The action will be executed asynchronously on the context's
    /// associated thread or scheduler.</remarks>
    /// <typeparam name="T1">The type of the parameter to pass to the action.</typeparam>
    /// <param name="context">The SynchronizationContext to which the action will be posted. Cannot be null.</param>
    /// <param name="parameter1">The parameter value to pass to the action when it is invoked.</param>
    /// <param name="action">The action to execute asynchronously, which receives the specified parameter. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if either context or action is null.</exception>
    public static void Post<T1>(this SynchronizationContext context, T1 parameter1, Action<T1> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostActionMap<T1>(parameter1, action);

        context.Post(
            static o =>
            {
                if (o is PostActionMap<T1> postMap)
                {
                    postMap.Action(postMap.Parameter1);
                }
            },
            postMap
        );
    }

    /// <summary>
    /// Posts an asynchronous message to the specified SynchronizationContext, invoking the provided action with two
    /// parameters.
    /// </summary>
    /// <remarks>This method allows posting an action with two strongly-typed parameters to a
    /// SynchronizationContext, enabling asynchronous execution on the context's associated thread or
    /// environment.</remarks>
    /// <typeparam name="T1">The type of the first parameter to pass to the action.</typeparam>
    /// <typeparam name="T2">The type of the second parameter to pass to the action.</typeparam>
    /// <param name="context">The SynchronizationContext to which the message is posted. Cannot be null.</param>
    /// <param name="parameter1">The first parameter to pass to the action when it is invoked.</param>
    /// <param name="parameter2">The second parameter to pass to the action when it is invoked.</param>
    /// <param name="action">The action to execute asynchronously on the specified context. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static void Post<T1, T2>(this SynchronizationContext context, T1 parameter1, T2 parameter2, Action<T1, T2> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostActionMap<T1, T2>(parameter1, parameter2, action);

        context.Post(
            static o =>
            {
                if (o is PostActionMap<T1, T2> postMap)
                {
                    postMap.Action(postMap.Parameter1, postMap.Parameter2);
                }
            },
            postMap
        );
    }

    /// <summary>
    /// Posts the specified action to the provided SynchronizationContext and asynchronously waits for its completion.
    /// </summary>
    /// <remarks>This method allows asynchronous code to post work to a SynchronizationContext and await its
    /// completion, enabling coordination with UI or other synchronization contexts. Any exception thrown by the action
    /// will be propagated to the returned task.</remarks>
    /// <param name="context">The SynchronizationContext to which the action will be posted. Cannot be null.</param>
    /// <param name="action">The action to execute on the specified SynchronizationContext. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the action has finished executing on
    /// the context.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either context or action is null.</exception>
    public static async ValueTask PostAsync(this SynchronizationContext context, Action action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostActionMap(action);

        context.Post(
            static o =>
            {
                if (o is PostActionMap postMap)
                {
                    try
                    {
                        postMap.Action();
                        postMap.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        await postMap;
    }

    /// <summary>
    /// Posts the specified action to the provided SynchronizationContext and asynchronously waits for its completion.
    /// </summary>
    /// <remarks>Use this method to marshal execution of an action to a specific SynchronizationContext and
    /// await its completion. This is useful for synchronizing work with UI threads or other synchronization
    /// contexts.</remarks>
    /// <typeparam name="T1">The type of the parameter to pass to the action.</typeparam>
    /// <param name="context">The SynchronizationContext to which the action will be posted. Cannot be null.</param>
    /// <param name="parameter1">The value to pass as an argument to the action when it is invoked.</param>
    /// <param name="action">The action to execute on the specified SynchronizationContext. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the action has finished executing on
    /// the context.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either context or action is null.</exception>
    public static async ValueTask PostAsync<T1>(this SynchronizationContext context, T1 parameter1, Action<T1> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostActionMap<T1>(parameter1, action);

        context.Post(
            static o =>
            {
                if (o is PostActionMap<T1> postMap)
                {
                    try
                    {
                        postMap.Action(postMap.Parameter1);
                        postMap.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        await postMap;
    }

    /// <summary>
    /// Posts an action with two parameters to the specified SynchronizationContext and asynchronously waits for its
    /// completion.
    /// </summary>
    /// <remarks>Use this method to marshal an action with two arguments onto a SynchronizationContext and
    /// await its completion. Any exception thrown by the action will be propagated to the returned task.</remarks>
    /// <typeparam name="T1">The type of the first parameter to pass to the action.</typeparam>
    /// <typeparam name="T2">The type of the second parameter to pass to the action.</typeparam>
    /// <param name="context">The SynchronizationContext to which the action is posted. Cannot be null.</param>
    /// <param name="parameter1">The first parameter to pass to the action when it is invoked.</param>
    /// <param name="parameter2">The second parameter to pass to the action when it is invoked.</param>
    /// <param name="action">The action to execute on the specified SynchronizationContext. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the action has finished executing on
    /// the context.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask PostAsync<T1, T2>(this SynchronizationContext context, T1 parameter1, T2 parameter2, Action<T1, T2> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostActionMap<T1, T2>(parameter1, parameter2, action);

        context.Post(
            static o =>
            {
                if (o is PostActionMap<T1, T2> postMap)
                {
                    try
                    {
                        postMap.Action(postMap.Parameter1, postMap.Parameter2);
                        postMap.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        await postMap;
    }

    /// <summary>
    /// Posts an asynchronous delegate to the specified SynchronizationContext and awaits its completion.
    /// </summary>
    /// <remarks>This method allows asynchronous code to be executed on a specific synchronization context,
    /// such as a UI thread, and returns a Task that completes when the delegate has finished. Exceptions thrown by the
    /// delegate are propagated through the returned Task.</remarks>
    /// <param name="context">The SynchronizationContext to which the asynchronous delegate is posted. Cannot be null.</param>
    /// <param name="action">The asynchronous delegate to execute. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the posted delegate has finished
    /// executing.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either context or action is null.</exception>
    public static async ValueTask PostAsync(this SynchronizationContext context, Func<Task> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMap<Task>(action);

        context.Post(
            static async o =>
            {
                if (o is PostFuncMap<Task> postMap)
                {
                    try
                    {
                        await postMap.Action();
                        postMap.SetResult(default!);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        await postMap;
    }

    /// <summary>
    /// Posts an asynchronous delegate to the specified SynchronizationContext and awaits its completion.
    /// </summary>
    /// <remarks>This method allows asynchronous code to be executed on a specific synchronization context,
    /// such as a UI thread. The returned task completes when the action has finished executing on the target
    /// context.</remarks>
    /// <typeparam name="T1">The type of the parameter to pass to the asynchronous action.</typeparam>
    /// <param name="context">The SynchronizationContext to which the asynchronous action is posted. Cannot be null.</param>
    /// <param name="parameter1">The value to pass as an argument to the asynchronous action.</param>
    /// <param name="action">The asynchronous delegate to invoke with the specified parameter. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the posted action has finished
    /// executing.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask PostAsync<T1>(this SynchronizationContext context, T1 parameter1, Func<T1, Task> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMap<T1, Task>(parameter1, action);

        context.Post(
            static async o =>
            {
                if (o is PostFuncMap<T1, Task> postMap)
                {
                    try
                    {
                        await postMap.Action(postMap.Parameter1);
                        postMap.SetResult(default!);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        await postMap;
    }

    /// <summary>
    /// Posts an asynchronous delegate to the specified SynchronizationContext, passing two parameters, and awaits its
    /// completion.
    /// </summary>
    /// <remarks>This method allows posting an asynchronous operation with two parameters to a
    /// SynchronizationContext and awaiting its completion. The delegate is executed on the context's thread or
    /// synchronization target.</remarks>
    /// <typeparam name="T1">The type of the first parameter to pass to the asynchronous delegate.</typeparam>
    /// <typeparam name="T2">The type of the second parameter to pass to the asynchronous delegate.</typeparam>
    /// <param name="context">The SynchronizationContext to which the delegate will be posted. Cannot be null.</param>
    /// <param name="parameter1">The first parameter to pass to the asynchronous delegate.</param>
    /// <param name="parameter2">The second parameter to pass to the asynchronous delegate.</param>
    /// <param name="action">The asynchronous delegate to invoke, which accepts two parameters and returns a Task. Cannot be null.</param>
    /// <returns>A Task that represents the asynchronous operation and completes when the delegate has finished executing.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask PostAsync<T1, T2>(this SynchronizationContext context, T1 parameter1, T2 parameter2, Func<T1, T2, Task> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMap<T1, T2, Task>(parameter1, parameter2, action);

        context.Post(
            static async o =>
            {
                if (o is PostFuncMap<T1, T2, Task> postMap)
                {
                    try
                    {
                        await postMap.Action(postMap.Parameter1, postMap.Parameter2);
                        postMap.SetResult(default!);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        await postMap;
    }

    /// <summary>
    /// Posts an asynchronous operation to the specified SynchronizationContext and returns a task that completes when
    /// the operation finishes.
    /// </summary>
    /// <remarks>This method allows asynchronous code to be executed on a specific SynchronizationContext,
    /// such as a UI thread. The returned ValueTask completes when the posted operation finishes, and any exceptions
    /// thrown by the action are propagated to the caller.</remarks>
    /// <typeparam name="T">The type of the result produced by the asynchronous operation.</typeparam>
    /// <param name="context">The SynchronizationContext to which the asynchronous operation is posted. Cannot be null.</param>
    /// <param name="action">A function that returns a Task producing the result to be executed on the specified context. Cannot be null.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task's result is the value produced by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask<T> PostAsync<T>(this SynchronizationContext context, Func<Task<T>> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMapAsync<T>(action);

        context.Post(
            static async o =>
            {
                if (o is PostFuncMapAsync<T> postMap)
                {
                    try
                    {
                        var result = await postMap.Action();
                        postMap.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        return await postMap;
    }

    /// <summary>
    /// Posts an asynchronous operation to the specified SynchronizationContext and returns a task representing the
    /// result.
    /// </summary>
    /// <remarks>The action is executed asynchronously on the provided SynchronizationContext. Any exception
    /// thrown by the action will be propagated through the returned task.</remarks>
    /// <typeparam name="T1">The type of the parameter to pass to the asynchronous action.</typeparam>
    /// <typeparam name="T">The type of the result produced by the asynchronous action.</typeparam>
    /// <param name="context">The SynchronizationContext to which the operation is posted. Cannot be null.</param>
    /// <param name="parameter1">The parameter to pass to the asynchronous action when it is invoked.</param>
    /// <param name="action">The asynchronous function to execute on the specified context. Cannot be null.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The result contains the value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask<T> PostAsync<T1, T>(this SynchronizationContext context, T1 parameter1, Func<T1, Task<T>> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMapAsync<T1, T>(parameter1, action);

        context.Post(
            static async o =>
            {
                if (o is PostFuncMapAsync<T1, T> postMap)
                {
                    try
                    {
                        var result = await postMap.Action(postMap.Parameter1);
                        postMap.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        return await postMap;
    }

    /// <summary>
    /// Posts an asynchronous operation to the specified SynchronizationContext and returns a ValueTask representing the
    /// result of the operation.
    /// </summary>
    /// <remarks>The operation is posted to the provided SynchronizationContext and will execute
    /// asynchronously. The returned ValueTask completes when the action has finished executing on the
    /// context.</remarks>
    /// <typeparam name="T1">The type of the first parameter to pass to the asynchronous action.</typeparam>
    /// <typeparam name="T2">The type of the second parameter to pass to the asynchronous action.</typeparam>
    /// <typeparam name="T">The type of the result produced by the asynchronous action.</typeparam>
    /// <param name="context">The SynchronizationContext to which the asynchronous operation is posted. Cannot be null.</param>
    /// <param name="parameter1">The first parameter to pass to the asynchronous action.</param>
    /// <param name="parameter2">The second parameter to pass to the asynchronous action.</param>
    /// <param name="action">The asynchronous function to execute, which takes parameter1 and parameter2 as arguments and returns a Task
    /// producing a result of type T. Cannot be null.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The result contains the value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask<T> PostAsync<T1, T2, T>(this SynchronizationContext context, T1 parameter1, T2 parameter2, Func<T1, T2, Task<T>> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMapAsync<T1, T2, T>(parameter1, parameter2, action);

        context.Post(
            static async o =>
            {
                if (o is PostFuncMapAsync<T1, T2, T> postMap)
                {
                    try
                    {
                        var result = await postMap.Action(postMap.Parameter1, postMap.Parameter2);
                        postMap.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        return await postMap;
    }

    /// <summary>
    /// Posts the specified delegate to the provided SynchronizationContext and asynchronously returns its result.
    /// </summary>
    /// <remarks>This method allows code to execute a delegate on a specific SynchronizationContext and await
    /// its result asynchronously. The delegate is invoked on the context's thread, and any exception thrown by the
    /// delegate is propagated to the returned task.</remarks>
    /// <typeparam name="T">The type of the result returned by the delegate.</typeparam>
    /// <param name="context">The SynchronizationContext to which the delegate will be posted. Cannot be null.</param>
    /// <param name="action">The delegate to execute on the specified SynchronizationContext. Cannot be null.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains the value returned by the
    /// delegate.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask<T> PostAsync<T>(this SynchronizationContext context, Func<T> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMap<T>(action);

        context.Post(
            static o =>
            {
                if (o is PostFuncMap<T> postMap)
                {
                    try
                    {
                        var result = postMap.Action();
                        postMap.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                    finally
                    {
                        postMap.Action = null!;
                    }
                }
            },
            postMap
        );

        return await postMap;
    }

    /// <summary>
    /// Posts an action to the specified synchronization context and asynchronously returns the result produced by the
    /// action.
    /// </summary>
    /// <remarks>The action is executed on the provided synchronization context. Any exception thrown by the
    /// action will be propagated to the returned task.</remarks>
    /// <typeparam name="T1">The type of the parameter passed to the action.</typeparam>
    /// <typeparam name="T">The type of the result returned by the action.</typeparam>
    /// <param name="context">The synchronization context to which the action is posted. Cannot be null.</param>
    /// <param name="parameter1">The parameter to pass to the action when it is invoked.</param>
    /// <param name="action">The function to execute on the synchronization context. Cannot be null.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The task result contains the value returned by the
    /// action.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask<T> PostAsync<T1, T>(this SynchronizationContext context, T1 parameter1, Func<T1, T> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMap<T1, T>(parameter1, action);

        context.Post(
            static o =>
            {
                if (o is PostFuncMap<T1, T> postMap)
                {
                    try
                    {
                        var result = postMap.Action(postMap.Parameter1);
                        postMap.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                    finally
                    {
                        postMap.Action = null!;
                        postMap.Parameter1 = default!;
                    }
                }
            },
            postMap
        );

        return await postMap;
    }

    /// <summary>
    /// Posts an action with two parameters to the specified SynchronizationContext and asynchronously returns the
    /// result.
    /// </summary>
    /// <remarks>The action is executed on the provided SynchronizationContext. Any exception thrown by the
    /// action will be propagated through the returned ValueTask.</remarks>
    /// <typeparam name="T1">The type of the first parameter to pass to the action.</typeparam>
    /// <typeparam name="T2">The type of the second parameter to pass to the action.</typeparam>
    /// <typeparam name="T">The type of the result returned by the action.</typeparam>
    /// <param name="context">The SynchronizationContext to which the action is posted. Cannot be null.</param>
    /// <param name="parameter1">The first parameter to pass to the action when it is invoked.</param>
    /// <param name="parameter2">The second parameter to pass to the action when it is invoked.</param>
    /// <param name="action">The function to execute, which takes two parameters and returns a result. Cannot be null.</param>
    /// <returns>A ValueTask that represents the asynchronous operation. The result contains the value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or action is null.</exception>
    public static async ValueTask<T> PostAsync<T1, T2, T>(this SynchronizationContext context, T1 parameter1, T2 parameter2, Func<T1, T2, T> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMap<T1, T2, T>(parameter1, parameter2, action);

        context.Post(
            static o =>
            {
                if (o is PostFuncMap<T1, T2, T> postMap)
                {
                    try
                    {
                        var result = postMap.Action(postMap.Parameter1, postMap.Parameter2);
                        postMap.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                    finally
                    {
                        postMap.Action = null!;
                        postMap.Parameter1 = default!;
                    }
                }
            },
            postMap
        );

        return await postMap;
    }
}
