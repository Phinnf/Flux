namespace Flux.Infrastructure.Services;

public interface IToastService
{
    void ShowInfo(string message);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    event Action<ToastMessage>? OnToastAdded;
}

public record ToastMessage(string Message, ToastLevel Level, DateTime Timestamp);

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}
