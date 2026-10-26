using Tavrynexo.Core.Enums;

namespace Tavrynexo.Core.Models;

public class SystemStateSnapshot
{
    public string SnapshotId { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public int TotalInstalledAppsCount { get; set; }
    public int ActiveServicesCount { get; set; }
    public List<string> InstalledAppNames { get; set; } = new();

    public const string LegalDisclaimer = "Notice: This snapshot records tracked registry keys, services, and applications state. It is NOT a full sector disk image and does not replace full Windows System Restore points.";
}

public class CrashReportItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FaultingApplicationName { get; set; } = string.Empty;
    public string FaultingModule { get; set; } = string.Empty;
    public string ExceptionCode { get; set; } = string.Empty;
    public DateTime CrashTimeUtc { get; set; }
    public string ErrorSummary { get; set; } = string.Empty;
    public string ProbableCause { get; set; } = string.Empty;
    public string ConfidenceLevel { get; set; } = "Suspected (Probabilistic)";
    public string LocalDumpPath { get; set; } = string.Empty;
}

public class JunkCategoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public long TotalSizeBytes { get; set; }
    public SafetyLevel SafetyLevel { get; set; } = SafetyLevel.SafeToPurge;
    public bool IsSelectedForCleaning { get; set; } = true;

    public string FormattedSize
    {
        get
        {
            double bytes = TotalSizeBytes;
            string[] suffixes = ["B", "KB", "MB", "GB"];
            int counter = 0;
            while (bytes >= 1024 && counter < suffixes.Length - 1) { bytes /= 1024; counter++; }
            return $"{bytes:0.##} {suffixes[counter]}";
        }
    }
}
