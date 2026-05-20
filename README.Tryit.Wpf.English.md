# Tryit.Wpf

Tryit.Wpf is a WPF-oriented utility library that provides reusable infrastructure and UI helpers for desktop applications. It includes popup services, dialog services, notification services, converter collections, animation helpers, dispatcher/threading helpers, and reusable controls.

## Supported Target Frameworks

- .NET Framework 4.5.1
- .NET Framework 4.7.2
- .NET Core 3.1
- .NET 8 (Windows)
- .NET 10 (Windows)

## Installation

```bash
dotnet add package Tryit.Wpf
```

## Namespace

```csharp
using Tryit.Wpf;
```

---

## Main Feature Areas

- Popup service
- Dialog service
- Notification service
- Static XAML converters
- Animation helpers
- Dispatcher and UI-thread helpers
- Reusable WPF controls

---

# 1. Popup Service

`IPopupService` supports lightweight popup-based interaction inside a visual host.

## Show a simple message

```csharp
public sealed class MainViewModel
{
	private readonly IPopupService _popupService;

	public MainViewModel(IPopupService popupService)
	{
		_popupService = popupService;
	}

	public async Task ShowSavedMessageAsync()
	{
		await _popupService.ShowAsync(
			content: "The document has been saved.",
			title: "Information"
		);
	}
}
```

## Show a confirmation popup

```csharp
var result = await _popupService.ConfirmAsync(
	content: "Do you want to delete this item?",
	title: "Confirm"
);

if (result == PopupService.ButtonResult.Ok)
{
	DeleteItem();
}
```

## Show a popup in a specific host

```csharp
await _popupService.ShowAsyncIn(
	hostedName: "RightPaneHost",
	content: "This message appears in the right-side host.",
	title: "Pane Message"
);
```

## Show custom visual content and await a result

```csharp
var editorView = new CustomerEditorView();

string result = await _popupService.PopupAsync<string>(
	editorView,
	new PopupParameter
	{
		Title = "Edit Customer"
	}
);
```

## Show custom visual content in a named host

```csharp
var settingsView = new SettingsView();

bool saved = await _popupService.PopupAsyncIn<bool>(
	hostedName: "MainHost",
	visual: settingsView,
	parameter: new PopupParameter
	{
		Title = "Settings"
	}
);
```

### When to use `IPopupService`

Use `IPopupService` when the UI interaction should stay inside an existing visual container rather than opening a separate WPF window.

---

# 2. Dialog Service

`IDialogService` is intended for traditional dialog-style interactions.

## Show a non-modal dialog

```csharp
_dialogService.Show(
	visual: new SettingsView(),
	parameter: new DialogParameter
	{
		Title = "Settings",
		Width = 700,
		Height = 450
	}
);
```

## Show a modal dialog

```csharp
bool? accepted = _dialogService.ShowDialog(
	visual: new ConfirmView(),
	parameter: new DialogParameter
	{
		Title = "Please Confirm"
	}
);
```

### When to use `IDialogService`

Use `IDialogService` when you want a separate dialog window or a modal user interaction flow.

---

# 3. Notification Service

`INotificationService` provides asynchronous, non-blocking notifications.

## Show a basic notification

```csharp
await _notificationService.NotifyAsync(
	message: "Saved successfully.",
	timeSpan: TimeSpan.FromSeconds(2)
);
```

## Show a notification in a named host

```csharp
await _notificationService.NotifyAsyncIn(
	hostedName: "MainHost",
	message: "Background synchronization completed.",
	timeSpan: TimeSpan.FromSeconds(3)
);
```

### Typical use cases

- Save succeeded
- Clipboard copied
- Background work finished
- Short, non-blocking user feedback

---

# 4. Converters

Tryit.Wpf exposes many reusable converters through the `Converters` static class.

Examples include:

- `BooleanToVisibility`
- `BooleanToVisibilityReverse`
- `IsNullOrEmptyToVisibility`
- `IsNotNullOrEmptyToVisibility`
- `GetEnumDescription`
- `GetEnumDisplayName`
- `StringToColor`
- `StringToBrush`
- `BooleanReverse`

## Example: boolean to visibility

```xml
<Window
	xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
	xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
	xmlns:t="clr-namespace:Tryit.Wpf;assembly=Tryit.Wpf">

	<Grid>
		<TextBlock Text="Ready"
				   Visibility="{Binding IsReady, Converter={x:Static t:Converters.BooleanToVisibility}}" />
	</Grid>
</Window>
```

## Example: null-or-empty to visibility

```xml
<TextBlock Text="Name is missing"
		   Visibility="{Binding Name, Converter={x:Static t:Converters.IsNullOrEmptyToVisibility}}" />
```

## Example: enum description

```xml
<TextBlock Text="{Binding CurrentStatus, Converter={x:Static t:Converters.GetEnumDescription}}" />
```

## Example: reverse a boolean value

```xml
<Button Content="Retry"
		IsEnabled="{Binding IsBusy, Converter={x:Static t:Converters.BooleanReverse}}" />
```

## Example: convert a string into a brush

```xml
<Border Background="{Binding AccentColorText, Converter={x:Static t:Converters.StringToBrush}}" />
```

### Why use the static converter instances?

Using the predefined static converter instances avoids repetitive XAML resource declarations and keeps markup simpler.

---

# 5. Animations

The animation module contains helpers such as:

- `AnimationBuilder`
- `AnimationExtensions`
- `AnimationHandler`
- `AnimationPropertyBuilder`

These APIs are designed to simplify animated transitions for WPF elements.

## Typical usage idea

```csharp
// Example pattern only. Adjust to the exact API available in your version.
// var builder = new AnimationBuilder(target)
//     .Opacity(0, 1, TimeSpan.FromMilliseconds(300))
//     .TranslateX(-20, 0, TimeSpan.FromMilliseconds(300));
// await builder.StartAsync();
```

### Recommended animation practices

- Keep animation orchestration close to the view layer.
- Extract shared animation patterns into helper methods.
- Standardize durations and easing functions for consistent UX.

---

# 6. Threading and Dispatcher Helpers

Tryit.Wpf contains dispatcher-oriented helpers for marshaling work back to the UI thread.

Relevant types include:

- `UIDispatcher`
- `DispatcherContainer`
- `DispatcherAsyncOperation`

## Usage pattern example

```csharp
// Example pattern only. Adjust to the actual API available in your code.
// await UIDispatcher.InvokeAsync(() =>
// {
//     StatusText = "Updated on UI thread";
// });
```

### Recommended use cases

- Updating bindable UI state from background work
- Coordinating async workflows with WPF visuals
- Centralizing UI-thread dispatch logic

---

# 7. Reusable Controls

The library includes reusable controls such as:

- `TransitioningControl`
- `EnumComboBox`
- `ColumnGrid`
- `IndexSelector`

## Example idea: enum-based combo box

```xml
<!-- Adjust properties according to the current control API -->
<!-- <t:EnumComboBox SelectedItem="{Binding SelectedStatus}" EnumType="{x:Type local:OrderStatus}" /> -->
```

## Example idea: transitioning content host

```xml
<!-- Adjust properties according to the current control API -->
<!-- <t:TransitioningControl Content="{Binding CurrentView}" /> -->
```

These controls are intended to reduce repetitive WPF infrastructure code in common UI scenarios.

---

# 8. MVVM Integration Example

The following example shows how popup and notification services can be used from a view model.

```csharp
public sealed class CustomerViewModel
{
	private readonly IPopupService _popupService;
	private readonly INotificationService _notificationService;

	public CustomerViewModel(
		IPopupService popupService,
		INotificationService notificationService)
	{
		_popupService = popupService;
		_notificationService = notificationService;
	}

	public async Task DeleteAsync()
	{
		var result = await _popupService.ConfirmAsync(
			"Delete the selected customer?",
			"Confirmation"
		);

		if (result != PopupService.ButtonResult.Ok)
		{
			return;
		}

		// Delete logic here.

		await _notificationService.NotifyAsync(
			"Customer deleted.",
			TimeSpan.FromSeconds(2)
		);
	}
}
```

---

# 9. Recommended Integration Strategy

1. Register `IPopupService`, `IDialogService`, and `INotificationService` in your bootstrapper or DI container.
2. Define consistent visual host names such as `MainHost`, `SidebarHost`, or `RightPaneHost`.
3. Keep popup/dialog orchestration in view models through interfaces rather than direct window references.
4. Prefer the predefined static converters in `Converters` instead of repeated XAML resource declarations.
5. Keep animation definitions in views or dedicated UI services.

---

# 10. Choosing the Right UI Mechanism

## Use `IPopupService` when

- The interaction should stay inside an existing visual host
- The UI should remain lightweight
- You want async popup result handling

## Use `IDialogService` when

- A separate dialog window is more appropriate
- A modal interaction is required
- Standard dialog semantics fit the use case better

## Use `INotificationService` when

- The feedback should not block the user
- The message is short-lived
- No confirmation is required

---

# 11. License

MIT License
