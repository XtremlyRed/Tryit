//using System;
//using System.Threading;

//namespace Tryit;

///// <summary>
/////
///// </summary>
//public class AsyncWaiter
//{
//    /// <summary>
//    /// wait
//    /// </summary>
//    /// <param name="func"></param>
//    public void Wait(Func<Task> func)
//    {
//        using WaitHandler<Func<Task>, int, int, int> handler = new WaitHandler<Func<Task>, int, int, int>(func, 1!, 1!);

//        ThreadPool.QueueUserWorkItem(
//            static async handler =>
//            {
//                await ((WaitHandler<Func<Task>, int, int, int>)handler!)!.Callback();
//            },
//            handler
//        );

//        handler.Semaphore.Wait();
//    }

//    /// <summary>
//    /// wait
//    /// </summary>
//    /// <param name="func"></param>
//    public void Wait<P1>(P1 p1, Func<P1, Task> func)
//    {
//        using WaitHandler<Func<P1, Task>, P1, int, int> handler = new WaitHandler<Func<P1, Task>, P1, int, int>(func, p1!, 1!);

//        ThreadPool.QueueUserWorkItem(
//            static async handler =>
//            {
//                var handler2 = ((WaitHandler<Func<P1, Task>, P1, int, int>)handler!)!;
//                await handler2.Callback(handler2.Parameter1);
//            },
//            handler
//        );

//        handler.Semaphore.Wait();
//    }

//    /// <summary>
//    ///
//    /// </summary>
//    /// <typeparam name="P1"></typeparam>
//    /// <typeparam name="P2"></typeparam>
//    /// <param name="p1"></param>
//    /// <param name="p2"></param>
//    /// <param name="func"></param>
//    public void Wait<P1, P2>(P1 p1, P2 p2, Func<P1, P2, Task> func)
//    {
//        using WaitHandler<Func<P1, P2, Task>, P1, P2, int> handler = new WaitHandler<Func<P1, P2, Task>, P1, P2, int>(func, p1!, p2!);

//        ThreadPool.QueueUserWorkItem(
//            static async handler =>
//            {
//                var handler2 = ((WaitHandler<Func<P1, P2, Task>, P1, P2, int>)handler!)!;
//                await handler2.Callback(handler2.Parameter1, handler2.Parameter2);
//            },
//            handler
//        );

//        handler.Semaphore.Wait();
//    }

//    public T Wait<T>(Func<Task<T>> func)
//    {
//        using WaitHandler<Func<Task<T>>, int, int, T> handler = new WaitHandler<Func<Task<T>>, int, int, T>(func, 1!, 1!);

//        ThreadPool.QueueUserWorkItem(
//            static async handler =>
//            {
//                var handler2 = (WaitHandler<Func<Task<T>>, int, int, T>)handler!;

//                handler2.Result = await handler2!.Callback();
//            },
//            handler
//        );

//        handler.Semaphore.Wait();
//        return handler.Result;
//    }

//    public T Wait<P1, T>(P1 p1, Func<P1, Task<T>> func)
//    {
//        using WaitHandler<Func<P1, Task<T>>, P1, int, T> handler = new WaitHandler<Func<P1, Task<T>>, P1, int, T>(func, p1!, 1!);

//        ThreadPool.QueueUserWorkItem(
//            static async handler =>
//            {
//                var handler2 = (WaitHandler<Func<P1, Task<T>>, P1, int, T>)handler!;

//                handler2.Result = await handler2!.Callback(handler2.Parameter1);
//            },
//            handler
//        );

//        handler.Semaphore.Wait();
//        return handler.Result;
//    }

//    public T Wait<P1, P2, T>(P1 p1, P2 p2, Func<P1, P2, Task<T>> func)
//    {
//        using WaitHandler<Func<P1, P2, Task<T>>, P1, P2, T> handler = new WaitHandler<Func<P1, P2, Task<T>>, P1, P2, T>(func, p1!, p2!);

//        ThreadPool.QueueUserWorkItem(
//            static async handler =>
//            {
//                var handler2 = (WaitHandler<Func<P1, P2, Task<T>>, P1, P2, T>)handler!;

//                handler2.Result = await handler2!.Callback(handler2.Parameter1, handler2.Parameter2);
//            },
//            handler
//        );

//        handler.Semaphore.Wait();
//        return handler.Result;
//    }

//    private record WaitHandler<TCallback, TParameter1, TParameter2, TResult>(TCallback Callback, TParameter1 Parameter1, TParameter2 Parameter2) : IDisposable
//    {
//        public SemaphoreSlim Semaphore = new SemaphoreSlim(0, 1);

//        public void Dispose()
//        {
//            Semaphore?.Dispose();
//            Semaphore = null!;
//        }

//        public TResult Result = default!;

//        public void Invoke()
//        {
//            ThreadPool.QueueUserWorkItem(o => { }, this);
//        }

//        public void Invoke(Func<WaitHandler<TCallback, TParameter1, TParameter2, TResult>, Task> func)
//        {
//            var iHandler = new InvokeHandle<WaitHandler<TCallback, TParameter1, TParameter2, TResult>, Func<WaitHandler<TCallback, TParameter1, TParameter2, TResult>, Task>>(this, func);

//            ThreadPool.QueueUserWorkItem(static o =>
//            {
//                var handle = (InvokeHandle<WaitHandler<TCallback, TParameter1, TParameter2, TResult>, Func<WaitHandler<TCallback, TParameter1, TParameter2, TResult>, Task>>)o!;


//            }, iHandler);
//        }
//    }

//    private record InvokeHandle<T, THandle>(T Target, THandle Handle);
//}
