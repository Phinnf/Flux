using System;

namespace Flux.Infrastructure.Services;

public class ToastService : IToastService
{
    public event Action<ToastMessage>? OnToastAdded;

    public void ShowInfo(string message) => Notify(message, ToastLevel.Info);
    public void ShowSuccess(string message) => Notify(message, ToastLevel.Success);
    public void ShowError(string message) => Notify(message, ToastLevel.Error);
    public void ShowWarning(string message) => Notify(message, ToastLevel.Warning);

    private void Notify(string message, ToastLevel level)
    {
        OnToastAdded?.Invoke(new ToastMessage(message, level, DateTime.Now));
    }
}
