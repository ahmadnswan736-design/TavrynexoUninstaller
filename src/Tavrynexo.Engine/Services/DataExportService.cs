using System.Text;
using System.Text.Json;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public static class DataExportService
{
    public static async Task ExportToCsvAsync(IEnumerable<InstalledApplication> apps, string targetFilePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Application Name,Publisher,Version,Install Date,Size (Bytes),Formatted Size,Architecture,Type,Install Location");
        foreach (var app in apps)
        {
            string date = !string.IsNullOrWhiteSpace(app.InstallDate) ? app.InstallDate : "Unknown";
            string sizeBytes = app.EstimatedSizeBytes?.ToString() ?? "Not Reported";
            sb.AppendLine($"\"{app.DisplayName.Replace("\"", "\"\"")}\",\"{app.Publisher.Replace("\"", "\"\"")}\",\"{app.DisplayVersion}\",\"{date}\",\"{sizeBytes}\",\"{app.FormattedSize}\",\"{app.Architecture}\",\"{app.AppType}\",\"{app.InstallLocation.Replace("\"", "\"\"")}\"");
        }
        await File.WriteAllTextAsync(targetFilePath, sb.ToString(), Encoding.UTF8);
    }

    public static async Task ExportToJsonAsync(IEnumerable<InstalledApplication> apps, string targetFilePath)
    {
        string json = JsonSerializer.Serialize(apps, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(targetFilePath, json, Encoding.UTF8);
    }
}
