namespace Tavrynexo.Core.Models;

public enum SizeAccuracy { NotReported, EstimatedFromRegistry, CalculatedFromDisk }

public class SizeInfo
{
    public long? RawBytes { get; set; }
    public SizeAccuracy Accuracy { get; set; } = SizeAccuracy.NotReported;

    public string DisplayText
    {
        get
        {
            if (!RawBytes.HasValue || RawBytes.Value <= 0) return "Not Reported";
            double bytes = RawBytes.Value;
            string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
            int counter = 0;
            while (bytes >= 1024 && counter < suffixes.Length - 1) { bytes /= 1024; counter++; }
            string formatted = $"{bytes:0.##} {suffixes[counter]}";
            return Accuracy switch
            {
                SizeAccuracy.EstimatedFromRegistry => $"{formatted} (Estimated)",
                SizeAccuracy.CalculatedFromDisk => $"{formatted} (Verified)",
                _ => formatted
            };
        }
    }
}

public class DashboardMetrics
{
    public int TotalInstalledApps { get; set; }
    public int Win32AppsCount { get; set; }
    public int StoreAppsCount { get; set; }
    public string TotalDiskUsageFormatted { get; set; } = "Calculating...";
    public List<InstalledApplication> RecentlyInstalledApps { get; set; } = new();
    public List<ActivityLogEntry> RecentActivities { get; set; } = new();
}

public class ActivityLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
}
