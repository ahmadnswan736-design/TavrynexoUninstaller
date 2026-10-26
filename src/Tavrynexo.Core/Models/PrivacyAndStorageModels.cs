namespace Tavrynexo.Core.Models;

public enum PrivacyTargetType
{
    GoogleChrome,
    MicrosoftEdge,
    MozillaFirefox,
    BraveBrowser,
    WindowsRecentDocs
}

public class PrivacyCategoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public PrivacyTargetType TargetType { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WarningNotice { get; set; } = string.Empty; // تحذيرات تسجيل الخروج
    public string TargetDirectoryOrKey { get; set; } = string.Empty;
    public int TracesCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public bool IsSelected { get; set; } = false; // تحديد صريح مطلوب دائماً

    public const string PrivacyDisclaimer = "Transparency Note: Cleans targeted local application artifacts and cache histories. Does not promise complete anonymity, prevent ISP tracking, or eliminate OS-level telemetry.";
}

public class DuplicateFileItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName => Path.GetFileName(FilePath);
    public long SizeBytes { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public bool IsSelectedForRemoval { get; set; } = false; // غير محدد افتراضياً
}

public class DuplicateFileGroup
{
    public string FileHashSha256 { get; set; } = string.Empty;
    public long SingleFileSize { get; set; }
    public List<DuplicateFileItem> Duplicates { get; set; } = new();
    public int DuplicateCount => Duplicates.Count;
    public long WasteSizeBytes => SingleFileSize * Math.Max(0, Duplicates.Count - 1);
}

public class LargeFileItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName => Path.GetFileName(FilePath);
    public string Extension => Path.GetExtension(FilePath).ToUpperInvariant();
    public long SizeBytes { get; set; }
    public DateTime LastModifiedUtc { get; set; }

    public string FormattedSize
    {
        get
        {
            double bytes = SizeBytes;
            string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
            int counter = 0;
            while (bytes >= 1024 && counter < suffixes.Length - 1) { bytes /= 1024; counter++; }
            return $"{bytes:0.##} {suffixes[counter]}";
        }
    }
}
