namespace Tavrynexo.Core.Models;

public enum ServiceStartupMode { Automatic, Manual, Disabled, Unknown }

public class ServiceInfoItem
{
    public string ServiceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = "Stopped";
    public ServiceStartupMode StartupType { get; set; } = ServiceStartupMode.Manual;
    public string BinaryPath { get; set; } = string.Empty;
    public bool IsProtectedSystemService { get; set; }
    public string ProtectionWarning { get; set; } = string.Empty;
    public bool CanBeModified => !IsProtectedSystemService;
}

public class ScheduledTaskItem
{
    public string TaskName { get; set; } = string.Empty;
    public string TaskPath { get; set; } = string.Empty;
    public string State { get; set; } = "Ready";
    public DateTime? LastRunTime { get; set; }
    public string ActionCommand { get; set; } = string.Empty;
    public bool IsSystemTask { get; set; }
}
