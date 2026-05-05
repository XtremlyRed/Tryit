using System.Diagnostics;

namespace Tryit;

/// <summary>
/// Represents a synchronous command item that can be queued and executed by <see cref="CommandQueue{T}"/>.
/// </summary>
public interface ICommandItem
{
    /// <summary>
    /// Gets the command scheduling behavior.
    /// </summary>
    CommandType CommandType { get; }

    /// <summary>
    /// Executes the command logic.
    /// </summary>
    void Execute();
}

/// <summary>
/// Represents an asynchronous command item that can be queued and executed by <see cref="AsyncCommandQueue{T}"/>.
/// </summary>
public interface IAsyncCommandItem
{
    /// <summary>
    /// Gets the command scheduling behavior.
    /// </summary>
    CommandType CommandType { get; }

    /// <summary>
    /// Executes the command logic asynchronously.
    /// </summary>
    Task ExecuteAsync();
}

/// <summary>
/// Specifies command lifetime behavior in the queue.
/// </summary>
public enum CommandType
{
    /// <summary>
    /// Execute once and remove from queue.
    /// </summary>
    Once,

    /// <summary>
    /// Keep in queue and execute repeatedly in round-robin order.
    /// </summary>
    Continuous,
}

/// <summary>
/// A thread-safe queue wrapper for synchronous command items.
/// </summary>
/// <typeparam name="T">The command item type.</typeparam>
/// <remarks>
/// Internally delegates all queueing behavior to <see cref="CommandPacket{T}"/>.
/// </remarks>
public class CommandQueue<T>
    where T : ICommandItem
{
    /// <summary>
    /// Internal command storage and scheduling packet.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly CommandPacket<T> commandPacket = new();

    /// <summary>
    /// Gets the current command counter from the internal packet.
    /// </summary>
    /// <remarks>
    /// This value tracks accepted commands and is decremented when <see cref="CommandType.Once"/> commands are dequeued.
    /// Continuous commands remain counted while they stay in the queue.
    /// </remarks>
    public int Count => commandPacket.commandCounter;

    /// <summary>
    /// Appends a command to the queue.
    /// </summary>
    /// <param name="command">The command to append. Null is ignored.</param>
    public virtual void Append(T command)
    {
        commandPacket.Append(command);
    }

    /// <summary>
    /// Appends multiple commands to the queue.
    /// </summary>
    /// <param name="commands">The command sequence. Null is ignored.</param>
    public virtual void AppendRange(IEnumerable<T> commands)
    {
        commandPacket.AppendRange(commands);
    }

    /// <summary>
    /// Tries to execute a single command according to queue policy.
    /// </summary>
    /// <remarks>
    /// Priority order:
    /// 1) One-time queue (<see cref="CommandType.Once"/>) in FIFO order.
    /// 2) Continuous list in round-robin order.
    /// </remarks>
    public virtual void Execute()
    {
        if (commandPacket.TryGetCommandItem(out T commandItem))
        {
            commandItem.Execute();
        }
    }
}

/// <summary>
/// A thread-safe queue wrapper for asynchronous command items.
/// </summary>
/// <typeparam name="T">The async command item type.</typeparam>
/// <remarks>
/// Internally delegates all queueing behavior to <see cref="CommandPacket{T}"/>.
/// </remarks>
public class AsyncCommandQueue<T>
    where T : IAsyncCommandItem
{
    /// <summary>
    /// Internal command storage and scheduling packet.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly CommandPacket<T> commandPacket = new();

    /// <summary>
    /// Gets the current command counter from the internal packet.
    /// </summary>
    /// <remarks>
    /// This value tracks accepted commands and is decremented when <see cref="CommandType.Once"/> commands are dequeued.
    /// Continuous commands remain counted while they stay in the queue.
    /// </remarks>
    public int Count => commandPacket.commandCounter;

    /// <summary>
    /// Appends an async command to the queue.
    /// </summary>
    /// <param name="command">The command to append. Null is ignored.</param>
    public virtual void Append(T command)
    {
        commandPacket.Append(command);
    }

    /// <summary>
    /// Appends multiple async commands to the queue.
    /// </summary>
    /// <param name="commands">The command sequence. Null is ignored.</param>
    public virtual void AppendRange(IEnumerable<T> commands)
    {
        commandPacket.AppendRange(commands);
    }

    /// <summary>
    /// Tries to execute a single command according to queue policy.
    /// </summary>
    /// <remarks>
    /// Priority order:
    /// 1) One-time queue (<see cref="CommandType.Once"/>) in FIFO order.
    /// 2) Continuous list in round-robin order.
    /// </remarks>
    public virtual async Task ExecuteAsync()
    {
        if (commandPacket.TryGetCommandItem(out T commandItem))
        {
            await commandItem.ExecuteAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Core thread-safe packet that stores commands and applies scheduling logic.
/// </summary>
/// <typeparam name="T">Command payload type.</typeparam>
/// <remarks>
/// Synchronization model:
/// - Uses an integer spin lock (`optFlag`) with <see cref="Interlocked.CompareExchange(ref int, int, int)"/>.
/// - All mutation and selection operations are performed under the lock.
/// </remarks>
internal class CommandPacket<T> : List<T>
{
    /// <summary>
    /// Spin-lock flag (0 = free, 1 = locked).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int optFlag;

    /// <summary>
    /// Current round-robin index for continuous commands.
    /// Starts at -1 so first successful access becomes index 0.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int executeIndex = -1;

    /// <summary>
    /// Storage for one-time commands (FIFO).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Queue<T> onceItems = new Queue<T>();

    /// <summary>
    /// Publicly exposed command counter used by outer queue wrappers.
    /// </summary>
    /// <remarks>
    /// Incremented when a supported command is appended.
    /// Decremented only when a one-time command is dequeued.
    /// </remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int commandCounter;

    /// <summary>
    /// Tries to fetch the next command based on queue policy.
    /// </summary>
    /// <param name="commandItem">When true, receives selected command; otherwise default value.</param>
    /// <returns>True if a command is available; otherwise false.</returns>
    /// <remarks>
    /// Selection policy:
    /// - One-time commands first (FIFO).
    /// - Then continuous commands in round-robin order.
    /// </remarks>
    internal bool TryGetCommandItem(out T commandItem)
    {
        try
        {
            SpinWait spinWait = default;

            // Acquire spin lock.
            while (Interlocked.CompareExchange(ref optFlag, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            // One-time commands have priority.
            if (onceItems.Count > 0)
            {
                commandItem = onceItems.Dequeue();

                Interlocked.Decrement(ref commandCounter);

                return true;
            }

            // Continuous commands fallback (round-robin).
            if (this.Count > 0)
            {
                executeIndex++;

                if (executeIndex >= this.Count)
                {
                    executeIndex = 0;
                }

                commandItem = this[executeIndex];

                return true;
            }

            commandItem = default!;
            return false;
        }
        finally
        {
            // Release lock even if selection/logic throws.
            Interlocked.Exchange(ref optFlag, 0);
        }
    }

    /// <summary>
    /// Appends a single command into internal storage.
    /// </summary>
    /// <param name="command">Command to append. Null is ignored.</param>
    public void Append(T command)
    {
        if (command is null)
        {
            return;
        }

        try
        {
            SpinWait spinWait = default;

            // Acquire spin lock.
            while (Interlocked.CompareExchange(ref optFlag, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            AppendCommand(command);
        }
        finally
        {
            // Release lock.
            Interlocked.Exchange(ref optFlag, 0);
        }
    }

    /// <summary>
    /// Appends a sequence of commands into internal storage.
    /// </summary>
    /// <param name="commands">Sequence of commands. Null sequence is ignored.</param>
    /// <remarks>
    /// Individual null/unsupported entries are ignored by <see cref="AppendCommand(T)"/>.
    /// </remarks>
    public void AppendRange(IEnumerable<T> commands)
    {
        if (commands is null)
        {
            return;
        }

        try
        {
            SpinWait spinWait = default;

            // Acquire spin lock.
            while (Interlocked.CompareExchange(ref optFlag, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            foreach (T? command in commands)
            {
                AppendCommand(command!);
            }
        }
        finally
        {
            // Release lock.
            Interlocked.Exchange(ref optFlag, 0);
        }
    }

    /// <summary>
    /// Routes command to proper internal collection based on runtime command contract and <see cref="CommandType"/>.
    /// </summary>
    /// <param name="command">Command instance. Unsupported/null entries are ignored.</param>
    /// <remarks>
    /// This method supports both runtime shapes:
    /// - <see cref="ICommandItem"/>
    /// - <see cref="IAsyncCommandItem"/>
    ///
    /// If command type is not <see cref="CommandType.Once"/> or <see cref="CommandType.Continuous"/>,
    /// the command is effectively ignored after counter increment is avoided.
    /// </remarks>
    private void AppendCommand(T command)
    {
        if (command is ICommandItem commandItem)
        {
            if (commandItem.CommandType == CommandType.Once)
            {
                onceItems.Enqueue(command);
                Interlocked.Increment(ref commandCounter);
            }
            else if (commandItem.CommandType == CommandType.Continuous)
            {
                this.Add(command);
                Interlocked.Increment(ref commandCounter);
            }
        }
        else if (command is IAsyncCommandItem asyncCommandItem)
        {
            if (asyncCommandItem.CommandType == CommandType.Once)
            {
                onceItems.Enqueue(command);
                Interlocked.Increment(ref commandCounter);
            }
            else if (asyncCommandItem.CommandType == CommandType.Continuous)
            {
                this.Add(command);
                Interlocked.Increment(ref commandCounter);
            }
        }
    }
}
