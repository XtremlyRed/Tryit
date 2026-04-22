# Tryit.CSharp

A comprehensive C# utility library providing common helpers, extensions, and WPF components for .NET applications.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Features

Tryit.CSharp is organized into two main packages:

- **Tryit.Core** - Core utilities, extensions, and helpers for any .NET application
- **Tryit.Wpf** - WPF-specific components including animations, dialogs, popups, and converters

## Supported Frameworks

| Framework      | Version          |
|----------------|------------------|
| .NET Framework  | 4.5.1, 4.7.2      |
| .NET Core      | 3.1              |
| .NET           | 8.0, 10.0      | 

## Installation

```bash
# Install core library
dotnet add package Tryit.Core

# Install WPF library (for WPF projects)
dotnet add package Tryit.Wpf
```

---

## Tryit (Core)

### Commands

Powerful command implementations for MVVM pattern with synchronous and asynchronous support.

#### BindingCommand

```csharp
// Simple command
public ICommand SaveCommand => new BindingCommand(() => Save());

// Command with CanExecute
public ICommand DeleteCommand => new BindingCommand(
    () => Delete(),
    () => SelectedItem != null
);

// Implicit conversion from Action
BindingCommand command = () => DoSomething();

// Global exception handling
BindingCommand.SetGlobalCommandExceptionCallback(ex => 
{
    Logger.LogError(ex, "Command execution failed");
});
```

#### BindingCommandAsync

```csharp
// Async command
public IBindingCommandAsync LoadCommand => new BindingCommandAsync(
    async () => await LoadDataAsync(),
    () => !IsLoading
);

// Check if command is executing
if (LoadCommand.IsExecuting)
{
    // Show loading indicator
}
```

#### Generic Commands with Parameters

```csharp
// Command with parameter
public ICommand SelectCommand => new BindingCommand<Item>(item => Select(item));

// Async command with parameter
public IBindingCommandAsync<int> FetchCommand => new BindingCommandAsync<int>(
    async id => await FetchAsync(id)
);
```

### Event Manager

A flexible pub/sub event aggregator supporting synchronous and asynchronous events with channel-based messaging.

```csharp
// Create event manager
IEventManager eventManager = new EventManager();

// Subscribe to events
IUnsubscrible subscription = eventManager.GetEvent<UserLoggedInEvent>()
    .Subscribe(e => HandleUserLogin(e), EventThreadPolicy.Current);

// Publish events
eventManager.GetEvent<UserLoggedInEvent>().Publish(new UserLoggedInEvent(user));

// Channel-based messaging
eventManager.GetEvent<NotificationEvent>()
    .Subscribe("alerts", notification => ShowAlert(notification));

eventManager.GetEvent<NotificationEvent>()
    .Publish("alerts", new NotificationEvent("New message"));

// Async events
var asyncEvent = eventManager.GetAsyncEvent<DataChangedEvent>();
await asyncEvent.PublishAsync(new DataChangedEvent());

// Unsubscribe when done
subscription.Unsubscribe();
```

**Thread Policies:**
- `EventThreadPolicy.Current` - Execute on the subscriber's original thread
- `EventThreadPolicy.PublishThread` - Execute on the publisher's thread
- `EventThreadPolicy.NewThread` - Execute on a new thread pool thread

### Context

Type-safe context containers for storing and retrieving values.

```csharp
// Application-wide context
IApplicationContext appContext = new ApplicationContext();

// Store values by type
appContext.SetValue(new UserSettings());
appContext.SetValue("theme", "dark");

// Retrieve values
var settings = appContext.GetValue<UserSettings>();
var theme = appContext.GetValue<string>("theme");

// Safe retrieval
if (appContext.TryGetValue<UserSettings>(out var userSettings))
{
    // Use settings
}
```

### Deferred Execution

Execute actions after a specified delay with debouncing support.

```csharp
// Simple deferred action
IDeferredToken token = Defer.Deferred(500) // 500ms delay
    .Invoke(() => SearchAsync(query));

// Restart the timer (debouncing)
token.Restart();

// With TimeSpan
IDeferredToken token2 = Defer.Deferred(TimeSpan.FromSeconds(1))
    .Invoke(async () => await SaveChangesAsync());

// With context information
Defer.Deferred(1000).Invoke(context =>
{
    Console.WriteLine($"Executed on thread {context.ThreadId}");
    Console.WriteLine($"Was abandoned: {context.IsAbandoned}");
});

// Execute on UI thread
Defer.Deferred(500).Invoke(
    () => UpdateUI(),
    invokeInCurrentThread: true
);

// Dispose to cancel
token.Dispose();
```

### Awaiter (Semaphore Wrapper)

A simple wrapper around SemaphoreSlim for resource management.

```csharp
// Create awaiter with initial and max count
var awaiter = new Awaiter(initialCount: 3, maxCount: 5);

// Wait for resource
await awaiter.WaitAsync();
try
{
    // Use resource
}
finally
{
    awaiter.Release();
}

// Extension methods for locked execution
await awaiter.LockInvokeAsync(async () =>
{
    await DoWorkAsync();
});

// With timeout
bool acquired = await awaiter.WaitAsync(
    millisecondsTimeout: 5000,
    cancellationToken: cts.Token
);
```

### Extension Methods

#### Enumerable Extensions

```csharp
// Null/empty checks
if (collection.IsNullOrEmpty()) { }
if (collection.IsNotNullOrEmpty()) { }

// Conditional filtering
var filtered = items.WhereIf(includeDeleted, x => x.IsDeleted);

// Find index
int index = items.IndexOf(x => x.Id == targetId);

// Pagination
var page = items.Paginate(pageIndex: 2, pageSize: 10);

// ForEach with action
items.ForEach(item => Process(item));
items.ForEach((item, index) => Process(item, index));

// Async ForEach
await items.ForEachAsync(async item => await ProcessAsync(item));

// Join to string
string csv = items.Join(x => x.Name, ", ");

// To read-only collections
IReadOnlyList<T> readOnlyList = items.ToReadOnlyList();
IReadOnlyDictionary<K, V> readOnlyDict = dict.ToReadOnlayDictionary();

// Chunking (for older frameworks)
var chunks = items.Chunk(100);
```

#### Task Extensions

```csharp
// Await TaskCompletionSource directly
var tcs = new TaskCompletionSource<int>();
int result = await tcs;

// Await multiple TaskCompletionSources
var sources = new[] { tcs1, tcs2, tcs3 };
int[] results = await sources;

// Await TimeSpan as delay
await TimeSpan.FromSeconds(1);

// Await collection of tasks
var tasks = items.Select(x => ProcessAsync(x));
await tasks;
```

#### String Extensions

```csharp
// Various string utility methods
string formatted = text.ToTitleCase();
bool isEmpty = text.IsNullOrEmpty();
```

#### DateTime Extensions

```csharp
// DateTime utility methods
var startOfDay = date.StartOfDay();
var endOfMonth = date.EndOfMonth();
```

### Type Conversion

Flexible type conversion with custom converter registration.

```csharp
// Convert using registered converters
int number = "123".ConvertTo<int>();
DateTime date = "2024-01-01".ConvertTo<DateTime>();

// Register custom converter
TypeConverterExtensions.ConvertRegister<string, CustomType>(
    str => CustomType.Parse(str)
);

// Use custom converter
var custom = "value".ConvertTo<CustomType>();
```

### Thrower (Argument Validation)

Utility methods for argument validation with detailed exception messages.

```csharp
public void Process(string input, object data)
{
    Thrower.IsNullOrEmpty(input, nameof(input));
    Thrower.IsNullOrWhiteSpace(input, nameof(input));
    Thrower.IsNull(data, nameof(data));
}
```

---

## Tryit.Wpf

### Animations

Fluent API for creating WPF property animations.

```csharp
// Create animation
var handler = element.BeginAnimation(UIElement.OpacityProperty)
    .From(0.0)
    .To(1.0)
    .Duration(TimeSpan.FromMilliseconds(300))
    .EasingFunction(new CubicEase())
    .Delay(TimeSpan.FromMilliseconds(100))
    .AutoReverse(true)
    .RepeatBehavior(RepeatBehavior.Forever)
    .Completed(() => Console.WriteLine("Animation completed"))
    .Build();

// Start animation
handler.Begin();

// Supported property types:
// - Numeric: byte, short, int, long, float, double, decimal
// - Geometry: Point, Point3D, Vector, Vector3D, Rect, Size
// - Other: Color, Quaternion, Rotation3D
```

### Dialog Service

Show modal and non-modal dialogs.

```csharp
IDialogService dialogService = new DialogService();

// Show non-modal dialog
dialogService.Show(new MyDialogView(), new DialogParameter
{
    Title = "Settings",
    Width = 400,
    Height = 300
});

// Show modal dialog
bool? result = dialogService.ShowDialog(new ConfirmView(), new DialogParameter
{
    Title = "Confirm Action"
});
```

### Popup Service

Display popups for messages, confirmations, and custom content.

```csharp
IPopupService popupService = new PopupService();

// Show message
await popupService.ShowAsync("Operation completed successfully", "Success");

// Show confirmation
ButtonResult result = await popupService.ConfirmAsync(
    "Are you sure you want to delete this item?",
    "Confirm Delete"
);

if (result == ButtonResult.OK)
{
    // Delete item
}

// Show custom popup
var customResult = await popupService.PopupAsync<CustomResult>(
    new CustomPopupView(),
    new PopupParameter { /* options */ }
);

// Show in specific host
await popupService.ShowAsyncIn("DialogHost", "Message", "Title");
```

### Notification Service

Display toast-style notifications.

```csharp
INotificationService notificationService = new NotificationService();

// Show notification
await notificationService.NotifyAsync(
    "File saved successfully",
    TimeSpan.FromSeconds(3)
);

// Show in specific host
await notificationService.NotifyAsyncIn(
    "NotificationHost",
    "New message received",
    TimeSpan.FromSeconds(5)
);
```

### Converters

Pre-built value converters for common scenarios.

```csharp
// Boolean converters
Converters.BooleanReverse                    // true -> false, false -> true
Converters.BooleanToVisibility               // true -> Visible, false -> Collapsed
Converters.BooleanToVisibilityReverse        // true -> Collapsed, false -> Visible

// Null check converters
Converters.IsNull                            // null -> true
Converters.IsNotNull                         // not null -> true
Converters.IsNullOrEmpty                     // null/empty -> true
Converters.IsNotNullOrEmpty                  // not null/empty -> true
Converters.IsNullOrWhiteSpace                // null/whitespace -> true
Converters.IsNotNullOrWhiteSpace             // not null/whitespace -> true

// Visibility converters
Converters.IsNullToVisibility
Converters.IsNotNullToVisibility
Converters.IsNullOrEmptyToVisibility
Converters.IsNotNullOrEmptyToVisibility

// Comparison converters (to Visibility)
Converters.EqualToVisibility
Converters.NotEqualToVisibility
Converters.GreaterThanToVisibility
Converters.GreaterThanOrEqualToVisibility
Converters.LessThanToVisibility
Converters.LessThanOrEqualToVisibility

// Enum converters
Converters.GetEnumDescription               // Get [Description] attribute
Converters.GetEnumDisplayName               // Get [Display] attribute

// Color converters
Converters.StringToColor                    // "#FF0000" -> Color
Converters.StringToBrush                    // "#FF0000" -> Brush
```

**Usage in XAML:**

```xml
<Window xmlns:hi="clr-namespace:Tryit;assembly=Tryit.Wpf">
    <Button Visibility="{Binding HasItems, 
        Converter={x:Static hi:Converters.BooleanToVisibility}}" />
    
    <TextBlock Visibility="{Binding Name, 
        Converter={x:Static hi:Converters.IsNotNullOrEmptyToVisibility}}" />
</Window>
```

### UI Dispatcher

Create background UI threads with their own Dispatcher.

```csharp
// Create async
Dispatcher backgroundDispatcher = await UIDispatcher.RunNewAsync("RenderThread");

// Create sync
Dispatcher dispatcher = UIDispatcher.RunNew("BackgroundUI");

// Execute on background dispatcher
backgroundDispatcher.InvokeAsync(() =>
{
    // This runs on the background UI thread
    var visual = CreateVisual();
});
```

### Controls

#### TransitioningControl

A content control that animates content changes.

```csharp
<hi:TransitioningControl Content="{Binding CurrentView}">
    <!-- Content transitions automatically when changed -->
</hi:TransitioningControl>
```

#### EnumComboBox

A ComboBox that automatically populates with enum values.

```csharp
<hi:EnumComboBox EnumType="{x:Type local:MyEnum}" 
                  SelectedValue="{Binding SelectedOption}" />
```

### Performance Utilities

## Overview

- `Performance` ！ attached helper; use `Performance.GetStages(element)` to get or create a `PerformanceStages` collection for an element.
- `PerformanceStages` ！ a `FreezableCollection<PerformanceStage>` that wires into element lifecycle (ensures `TransformGroup` on `Loaded`).
- `PerformanceStage` ！ a stage containing one or more `Performer` objects, a `StageEvent`, and a `Play` toggle; constructs a `Storyboard` on initialize.
- `Performer` ！ abstract base exposing timing dependency properties (`Duration`, `Delay`, `SpeedRatio`, `DecelerationRatio`) and animation creation helpers.
- `TransitionPerformer` ！ concrete performer implementing common transitions (`TransitionOn` values such as `FadeTo`, `TranslateYFrom`, `ScaleXTo`, `RotateTo`, etc.).
- Enums: `TransitionEvent`, `TransitionOn`, `EasingFunction` ！ configure when and how transitions run.

## When to use

- Create appearance animations on `Loaded`.
- Provide hover/interaction feedback on `MouseEnter` / `MouseLeave`.
- Respond to view-model changes on `DataContextChanged`.
- Combine multiple performers into stages and reuse across elements.

## Quick programmatic example

## XAML example

`PerformanceStages` and `PerformanceStage` are `Freezable`-based and can be declared in XAML. To attach resources to an element at runtime, retrieve and merge them into `Performance.GetStages(element)`.

``` XAMl
<UserControl xmlns:perf="clr-namespace:Tryit.Wpf;assembly=Tryit.Wpf"> 
	<UserControl.Resources> 
		<perf:PerformanceStages x:Key="stages"> 
			<perf:PerformanceStage StageEvent="Loaded" Play="True"> 
				<perf:TransitionPerformer TransitionOn="FadeTo" Target="1" Duration="0:0:0.3" EasingFunction="Cubic" EasingMode="EaseOut" /> 
			</perf:PerformanceStage>
			<perf:PerformanceStage StageEvent="MouseEnter" Play="True"> 
				<perf:TransitionPerformer TransitionOn="TranslateYFrom" Target="-20" Duration="0:0:0.2" /> 
			</perf:PerformanceStage> 
		</perf:PerformanceStages> 
	</UserControl.Resources> 
</UserControl>
```

## Common scenarios and usage tips

- `Loaded` ！ appearance/fade/scale animations.
- `MouseEnter` / `MouseLeave` ！ interactive translate/scale feedback.
- `DataContextChanged` ！ animate when view-model swaps.
- `Play` flag ！ enable or disable a stage without removing it.

Implementation notes:
- `PerformanceStages` ensures a `TransformGroup` and adds standard transforms (Translate, Scale, Rotate, Skew) on `Loaded`. This simplifies `TransitionPerformer` usage ！ it finds transform indices automatically.
- If an animation doesn't run, verify:
  - The stage was initialized (element `Loaded` fired).
  - `StageEvent` matches the runtime event.
  - Generated `PropertyPath` corresponds to the element's transform structure.
- All performer configuration properties are dependency properties ！ bindable and stylable.

## Troubleshooting

- No storyboard created: ensure `Performance.GetStages(element)` was called or XAML stages were attached.
- Incorrect property path: inspect element's `RenderTransform` and confirm `TransformGroup` order; `TransitionPerformer` tries to locate required transforms but custom templates may change expectations.
- Opacity fade uses `OpacityMask` Color alpha; ensure element supports an `OpacityMask` or the performer will set one when needed.

## Where to look in source

- `Tryit.Wpf/Performance/Performance.cs` ！ attached helpers and `PerformanceStages`.
- `Tryit.Wpf/Performance/PerformanceStage.cs` ！ stage lifecycle and storyboard assembly.
- `Tryit.Wpf/Performance/Performer.cs` ！ base performer implementation and helpers.
- `Tryit.Wpf/Performance/TransitionPerformer.cs` ！ concrete transitions and `TransitionOn` enum.


## Examples

### MVVM ViewModel with Commands

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    public IBindingCommand SaveCommand { get; }
    public IBindingCommandAsync LoadCommand { get; }
    
    public MainViewModel()
    {
        SaveCommand = new BindingCommand(Save, () => CanSave);
        LoadCommand = new BindingCommandAsync(LoadAsync, () => !IsLoading);
    }
    
    private void Save() { /* ... */ }
    private async Task LoadAsync() { /* ... */ }
}
```

### Event-Driven Architecture

```csharp
public class OrderService
{
    private readonly IEventManager _eventManager;
    
    public OrderService(IEventManager eventManager)
    {
        _eventManager = eventManager;
    }
    
    public async Task PlaceOrderAsync(Order order)
    {
        await ProcessOrderAsync(order);
        
        // Notify other parts of the application
        _eventManager.GetEvent<OrderPlacedEvent>()
            .Publish(new OrderPlacedEvent(order));
    }
}

public class NotificationHandler
{
    public NotificationHandler(IEventManager eventManager)
    {
        eventManager.GetEvent<OrderPlacedEvent>()
            .Subscribe(OnOrderPlaced, EventThreadPolicy.Current);
    }
    
    private void OnOrderPlaced(OrderPlacedEvent e)
    {
        ShowNotification($"Order {e.Order.Id} placed successfully");
    }
}
```

### Debounced Search

```csharp
public class SearchViewModel
{
    private IDeferredToken? _searchToken;
    
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            DebouncedSearch();
        }
    }
    
    private void DebouncedSearch()
    {
        _searchToken?.Restart();
        _searchToken ??= Defer.Deferred(300)
            .Invoke(async () => await SearchAsync(_searchText), 
                    invokeInCurrentThread: true);
    }
}
```

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

[XtremlyRed](https://github.com/XtremlyRed)