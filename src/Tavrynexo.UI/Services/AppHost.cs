using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tavrynexo.Core.Interfaces;
using Tavrynexo.Data.Database;
using Tavrynexo.Data.Repositories;
using Tavrynexo.Engine.Services;
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

        services.AddSingleton<SqliteDatabaseInitializer>();
        services.AddSingleton<ISqliteRepository, SqliteRepository>();
        services.AddSingleton<IApplicationDiscoveryService, WindowsApplicationScanner>();
        services.AddSingleton<ILeftoverScanEngine, DeepLeftoverScanner>();
        services.AddSingleton<ISystemRestoreService, SystemRestoreManager>();
        services.AddSingleton<IUninstallExecutionService, UninstallExecutor>();
        services.AddSingleton<IInstallMonitorService, InstallationMonitor>();
        services.AddSingleton<IQuarantineService, QuarantineManager>();
        services.AddSingleton<INotificationService, NotificationService>();

        services.AddTransient<ShellViewModel>();
        services.AddTransient<InstalledAppsViewModel>();
        services.AddTransient<UninstallWizardViewModel>();

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
