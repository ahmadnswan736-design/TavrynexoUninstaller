namespace Tavrynexo.Core.Models;

public class AppSettings
{
    public string ThemeMode { get; set; } = "Dark";
    public string Language { get; set; } = "English";
    public bool EnableMicaBackdrop { get; set; } = true;
    public bool StartWithWindows { get; set; } = false;
    public bool CreateRestorePointBeforeUninstall { get; set; } = true;
    public bool AutoQuarantineBeforePurge { get; set; } = true;
    public int BackupRetentionDays { get; set; } = 30;
    public List<string> UserCustomProtectedPaths { get; set; } = new();
}

public class DiagnosticsPackageInfo
{
    public string AppVersion { get; set; } = "1.0.0.0 (Production Architecture Build 2026.10)";
    public string DotNetRuntimeVersion { get; set; } = Environment.Version.ToString();
    public string WindowsOsVersion { get; set; } = Environment.OSVersion.ToString();
    public string SystemArchitecture { get; set; } = Environment.Is64BitOperatingSystem ? "x64" : "x86";
    public string SupportedWindowsVersions => "Windows 10 Build 19041 (Version 2004) or later, and Windows 11 (all editions).";
}
