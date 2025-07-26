namespace Tryit.Wpf;

/// <summary>
/// ButtonResult is an enumeration representing three possible responses: Yes, No, and Cancel. It is used for handling
/// user confirmations and choices.
/// </summary>
public enum ButtonResult
{
    /// <summary>
    /// Indicates a positive affirmation or agreement. Often used in contexts requiring confirmation.
    /// </summary>
    Yes,

    /// <summary>
    /// Represents a negative response or absence of a value. Often used in contexts where a binary choice is required.
    /// </summary>
    No,

    /// <summary>
    /// Cancels an ongoing operation or process. It stops any further execution related to the task.
    /// </summary>
    Cancel,
}
