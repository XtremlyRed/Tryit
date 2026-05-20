# Tryit.Core

Tryit.Core is a general-purpose .NET utility library that provides reusable building blocks for application development. It focuses on reducing boilerplate code in common scenarios such as commands, event aggregation, deferred execution, object pooling, conversion, validation, timing, and collection/task helpers.

## Supported Target Frameworks

- .NET Core 3.1
- .NET 8
- .NET 10
- .NET Standard 2.0

## Installation

```bash
dotnet add package Tryit.Core
```

## Namespace

```csharp
using Tryit;
```

---

## Main Feature Areas

Tryit.Core includes the following major areas:

- Command helpers for MVVM-style command handling
- Event aggregation for loosely coupled communication
- Deferred execution and debounce-friendly scheduling
- Lightweight synchronization helpers
- Object pooling utilities
- Timing helpers
- Type conversion helpers
- Guard/validation helpers
- Common extension methods

---

# 1. Commands

## BindingCommand

`BindingCommand` is a synchronous command implementation for command-based UI or application workflows.

### Basic usage

```csharp
public IBindingCommand SaveCommand => new BindingCommand(
	execute: () => Save(),
	canExecute: () => CanSave
);
```

### Without a can-execute predicate

```csharp
public IBindingCommand RefreshCommand => new BindingCommand(() => Refresh());
```

### Implicit conversion from `Action`

```csharp
BindingCommand command = () => Console.WriteLine("Executed");
command.Execute();
```

### Global exception handling

```csharp
BindingCommand.SetGlobalCommandExceptionCallback(ex =>
{
	Console.WriteLine($"Command failed: {ex.Message}");
});
```

### Typical MVVM usage

```csharp
public sealed class CustomerViewModel
{
	public bool CanSave { get; set; }

	public IBindingCommand SaveCommand => new BindingCommand(
		() => SaveCustomer(),
		() => CanSave
	);

	private void SaveCustomer()
	{
		// Save logic
	}
}
```

## BindingCommandAsync

`BindingCommandAsync` is the asynchronous counterpart.

### Basic usage

```csharp
public IBindingCommandAsync LoadCommand => new BindingCommandAsync(
	execute: async () => await LoadAsync(),
	canExecute: () => !IsBusy
);
```

### Example

```csharp
private bool IsBusy;

private async Task LoadAsync()
{
	IsBusy = true;
	try
	{
		await Task.Delay(500);
	}
	finally
	{
		IsBusy = false;
	}
}
```

### Notes

- `IsExecuting` can be used by consumers to check whether the command is currently running.
- A global exception callback configured through `BindingCommand.SetGlobalCommandExceptionCallback(...)` also applies to async commands.

---

# 2. Event Aggregation

`EventManager` provides publish/subscribe messaging for synchronous and asynchronous scenarios.

## Basic synchronous event

```csharp
IEventManager eventManager = new EventManager();

IUnsubscrible subscription = eventManager
	.GetEvent<string>()
	.Subscribe(message => Console.WriteLine($"Received: {message}"));

eventManager.GetEvent<string>().Publish("Hello from Tryit.Core");

subscription.Unsubscribe();
```

## Channel-based publishing

```csharp
IEventManager eventManager = new EventManager();

eventManager.GetEvent<string>()
	.Subscribe("audit", message => Console.WriteLine($"Audit: {message}"));

eventManager.GetEvent<string>()
	.Publish("audit", "User signed in");
```

## Asynchronous event

```csharp
IEventManager eventManager = new EventManager();

var asyncEvent = eventManager.GetAsyncEvent<int>();

asyncEvent.Subscribe(async number =>
{
	await Task.Delay(50);
	Console.WriteLine($"Processed: {number}");
});

await asyncEvent.PublishAsync(42);
```

## Thread policy

`Subscribe` overloads accept an `EventThreadPolicy`:

- `Current`
- `PublishThread`
- `NewThread`

Use the appropriate option depending on whether execution affinity or throughput is more important in the current scenario.

---

# 3. Deferred Execution

`Defer` provides delayed execution and debounce-friendly behavior.

## Simple delayed action

```csharp
var token = Defer.Deferred(500)
	.Invoke(() => Console.WriteLine("Executed after 500ms"));
```

## Using `TimeSpan`

```csharp
var token = Defer.Deferred(TimeSpan.FromSeconds(1))
	.Invoke(() => DoWork());
```

## Debounce pattern

```csharp
IDeferredToken? searchToken = null;

void OnSearchTextChanged(string keyword)
{
	searchToken?.Dispose();
	searchToken = Defer.Deferred(300)
		.Invoke(() => Search(keyword));
}
```

## Restarting an existing token

```csharp
var token = Defer.Deferred(300).Invoke(() => SaveDraft());

// Restart the countdown
 token.Restart();
```

## Async delegate

```csharp
var token = Defer.Deferred(200)
	.Invoke(async () =>
	{
		await Task.Delay(10);
		Console.WriteLine("Async callback completed.");
	});
```

## Context-aware callback

```csharp
Defer.Deferred(500).Invoke(ctx =>
{
	Console.WriteLine($"Started at: {ctx.BeginTime}");
	Console.WriteLine($"Thread ID: {ctx.ThreadId}");
	Console.WriteLine($"Deferred time: {ctx.DeferTime}");
	Console.WriteLine($"Abandoned: {ctx.IsAbandoned}");
});
```

### Important note

Always dispose the returned token when it is no longer needed to avoid unnecessary delayed work remaining alive longer than intended.

---

# 4. Awaiter

`Awaiter` is a lightweight synchronization helper around controlled wait/release patterns.

## Manual usage

```csharp
var awaiter = new Awaiter(initialCount: 1, maxCount: 1);

await awaiter.WaitAsync();
try
{
	await DoWorkAsync();
}
finally
{
	awaiter.Release();
}
```

## Helper extension methods

```csharp
await awaiter.LockInvokeAsync(async () =>
{
	await ProcessAsync();
});
```

## Synchronous helper

```csharp
awaiter.LockInvoke(() =>
{
	Process();
});
```

---

# 5. SimpleObjectPool

`SimpleObjectPool` provides a queue-based object reuse mechanism.

## Create a custom pool

```csharp
var pool = SimpleObjectPool.Create(
	factory: () => new StringBuilder(),
	resetCallback: sb => sb.Clear(),
	maxPoolSize: 1024
);
```

## Rent and return

```csharp
var sb = pool.Rent();
try
{
	sb.Append("Hello");
	sb.Append(' ');
	sb.Append("Pool");

	Console.WriteLine(sb.ToString());
}
finally
{
	pool.Return(sb);
}
```

## Create using `new T()`

```csharp
var customerPool = SimpleObjectPool.Create<CustomerBuffer>(
	resetCallback: x => x.Clear(),
	maxPoolSize: 256
);
```

## Built-in `StringBuilder` pool

```csharp
var sbPool = SimpleObjectPool.StringBuilderPool();
var sb = sbPool.Rent();
try
{
	sb.Append("Tryit.Core");
}
finally
{
	sbPool.Return(sb);
}
```

### Usage guidance

- Only pool objects that can be safely reset.
- Keep the reset callback deterministic and inexpensive.
- Avoid returning `null`.
- Choose `maxPoolSize` based on realistic throughput and memory constraints.

---

# 6. XTimer

`XTimer` provides convenient anchor-based timing.

## Basic anchor

```csharp
var anchor = XTimer.SetAnchor();

// Do work...

Console.WriteLine(anchor.Elapsed);
Console.WriteLine(anchor.ElapsedMilliseconds);
```

## Named anchor

```csharp
XTimer.SetAnchor("job-1");

// Do work...

var anchor = XTimer.GetAnchor("job-1");
Console.WriteLine(anchor.ElapsedMilliseconds);
```

## Decay anchor

```csharp
var decay = XTimer.SetDecayAnchor(5000);

Console.WriteLine(decay.ElapsedMilliseconds);
Console.WriteLine(decay.RemainingMilliseconds);
```

---

# 7. Type Conversion

`TypeConverterExtensions` simplifies conversion between common types and supports custom registration.

## Basic conversion

```csharp
int number = "123".ConvertTo<int>();
DateTime date = "2025-01-01".ConvertTo<DateTime>();
```

## Register a custom converter

```csharp
TypeConverterExtensions.ConvertRegister<string, Version>(
	s => Version.Parse(s)
);

Version version = "1.2.3.4".ConvertTo<Version>();
```

---

# 8. Guard Helpers

`Thrower` contains validation helpers for argument checking.

## Examples

```csharp
Thrower.IsNullOrEmpty(name, nameof(name));
Thrower.IsNullOrWhiteSpace(connectionString, nameof(connectionString));
Thrower.IsNull(model, nameof(model));
```

These methods are useful when you want consistent exception behavior across an application.

---

# 9. Extension Methods

The library includes common extension sets such as:

- `EnumerableExtensions`
- `TaskExtensions`
- `StringExtensions`
- `DateTimeExtensions`
- `MathExtensions`
- `ReflectionExtensions`
- `SynchronizationContextExtensions`

## Sample enumerable helpers

```csharp
var page = items.Paginate(pageIndex: 2, pageSize: 20);
var filtered = items.WhereIf(includeInactive, x => x.IsInactive);
bool empty = items.IsNullOrEmpty();
```

## Sample task helpers

```csharp
await TimeSpan.FromSeconds(1);
await tasks;
```

---

# 10. Practical Integration Example

The following example combines multiple features in a single application service:

```csharp
public sealed class SearchService
{
	private readonly IEventManager _eventManager = new EventManager();
	private readonly SimpleObjectPool<StringBuilder> _stringBuilderPool =
		SimpleObjectPool.StringBuilderPool();

	private IDeferredToken? _searchToken;

	public void ScheduleSearch(string keyword)
	{
		_searchToken?.Dispose();
		_searchToken = Defer.Deferred(300).Invoke(async () =>
		{
			var result = await SearchAsync(keyword);
			_eventManager.GetEvent<string>().Publish(result);
		});
	}

	private Task<string> SearchAsync(string keyword)
	{
		var sb = _stringBuilderPool.Rent();
		try
		{
			sb.Append("Searching for: ");
			sb.Append(keyword);
			return Task.FromResult(sb.ToString());
		}
		finally
		{
			_stringBuilderPool.Return(sb);
		}
	}
}
```

---

# 11. Best Practices

1. Use `BindingCommand`/`BindingCommandAsync` to centralize command behavior.
2. Dispose deferred tokens when they are no longer needed.
3. Unsubscribe event subscriptions at the end of the subscriber lifecycle.
4. Only pool objects that can be safely reused.
5. Keep pooled object reset logic simple and predictable.

---

# 12. License

MIT License
