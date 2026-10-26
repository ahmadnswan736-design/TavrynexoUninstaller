namespace Tavrynexo.Core.Models;

public enum RestorationStatus
{
    NotRestored,
    IntegrityVerified,
    RestorationInProgress,
    RestoredSuccessfully,
    PartiallyRestored,
    RestorationFailed
}

public class QuarantinedFileRecord
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString("N");
    public string OriginalFilePath { get; set; } = string.Empty;
    public string InternalArchiveName { get; set; } = string.Empty;
    public string FileSha256 { get; set; } = string.Empty;
    public bool WasRestored { get; set; }
}

public class QuarantineArchiveManifest
{
    public string ArchiveId { get; set; } = Guid.NewGuid().ToString("N");
    public string ApplicationName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string ArchiveZipPath { get; set; } = string.Empty;
    public List<QuarantinedFileRecord> Files { get; set; } = new();
    public RestorationStatus Status { get; set; } = RestorationStatus.NotRestored;
    public string? DiagnosticError { get; set; }

    public bool IsRestorable => File.Exists(ArchiveZipPath);
}

public class ActivityTaskItem
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public bool HasErrors { get; set; }
    public string? ErrorMessage { get; set; }
}
