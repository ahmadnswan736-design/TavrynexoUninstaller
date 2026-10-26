using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tavrynexo.Engine.CoreEngines;
using Tavrynexo.UI.ViewModels;

namespace Tavrynexo.UI.Services;

public static class AppHost
{
    private static IServiceProvider? _serviceProvider;
    public static IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("AppHost not initialized.");

    public static void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => { b.SetMinimumLevel(LogLevel.Information); b.AddDebug(); });

        services.AddSingleton<IApplicationDiscoveryEngine, ApplicationDiscoveryEngine>();
        services.AddSingleton<INotificationService, NotificationService>();

        services.AddTransient<InstalledAppsViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
}

public class NotificationService : INotificationService
{
    public event EventHandler<NotificationEventArgs>? NotificationReceived;
    public void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Informational, int durationMs = 5000)
    {
        NotificationReceived?.Invoke(this, new NotificationEventArgs { Title = title, Message = message, Severity = severity, DurationMs = durationMs });
    }
}
