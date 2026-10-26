namespace Tavrynexo.UI.Services;

public interface INavigationService
{
    void Initialize(object navigationFrame);
    bool NavigateTo(Type pageType, object? parameter = null);
    bool GoBack();
    bool CanGoBack { get; }
}

public interface INotificationService
{
    event EventHandler<NotificationEventArgs>? NotificationReceived;
    void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Informational, int durationMs = 5000);
}

public enum NotificationSeverity { Informational, Success, Warning, Error }

public class NotificationEventArgs : EventArgs
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationSeverity Severity { get; init; }
    public int DurationMs { get; init; }
}
