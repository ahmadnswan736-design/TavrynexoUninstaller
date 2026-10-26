using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public static class DiagnosticsPackageGenerator
{
    public static async Task<string> GenerateSanitizedDiagnosticsBundleAsync(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        string time = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string tempFolder = Path.Combine(outputDirectory, $"Tavrynexo_Diagnostics_{time}");
        Directory.CreateDirectory(tempFolder);

        var diagInfo = new DiagnosticsPackageInfo();
        string envJson = JsonSerializer.Serialize(diagInfo, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(tempFolder, "environment_info.json"), envJson, Encoding.UTF8);

        string sampleLog = "Diagnostic Log: System engines operational. Passwords and credentials redacted.";
        string sanitized = RedactSensitiveInformation(sampleLog);
        await File.WriteAllTextAsync(Path.Combine(tempFolder, "diagnostic_events.log"), sanitized, Encoding.UTF8);

        string zipPath = tempFolder + ".zip";
        ZipFile.CreateFromDirectory(tempFolder, zipPath);
        Directory.Delete(tempFolder, true);
        return zipPath;
    }

    public static string RedactSensitiveInformation(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return string.Empty;
        string clean = Regex.Replace(rawText, @"(?i)(password|secret|token|apikey)\s*[:=]\s*['""]?[^'"";\s]+", "$1=REDACTED");
        return Regex.Replace(clean, @"C:\\Users\\[^\\]+", @"C:\Users\<UserRedacted>");
    }
}
