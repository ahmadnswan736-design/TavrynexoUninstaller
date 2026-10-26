namespace Tavrynexo.Core.Models;

public enum CaptureMechanism
{
    LiveCapturedEvent,    // حدث تم رصده لحظياً عبر خطافات ومراقبي النظام
    InferredCorrelation   // ارتباط تم استنتاجه بناءً على بيانات وحزم التثبيت
}

public enum MonitoredEventKind
{
    FileCreated,
    FileModified,
    FileDeleted,
    RegistryKeyCreated,
    RegistryValueSet,
    WindowsServiceCreated,
    ScheduledTaskCreated,
    ProcessSpawned
}

public class MonitoredEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MonitoredEventKind Kind { get; set; }
    public CaptureMechanism Mechanism { get; set; } = CaptureMechanism.LiveCapturedEvent;
    public string TargetPath { get; set; } = string.Empty;
    public string? ValueName { get; set; }
    public int? ProcessId { get; set; }
}

public class InstallationSessionRecord
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public string ApplicationName { get; set; } = string.Empty;
    public string InstallerPath { get; set; } = string.Empty;
    public DateTime StartTimeUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndTimeUtc { get; set; }
    public List<MonitoredEvent> Events { get; set; } = new();
    public int FilesCreatedCount { get; set; }
    public double CompletenessScore { get; set; } = 95.0;

    public List<string> DocumentedLimitations { get; set; } = new()
    {
        "Kernel-mode driver installations (Ring 0) cannot be captured in user-mode.",
        "Boot-time service initializations occurring after a system reboot are excluded.",
        "Direct disk sector writes bypassing the Windows File System cache are omitted."
    };
}
