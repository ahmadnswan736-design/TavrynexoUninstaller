using System.Globalization;
using Microsoft.Win32;
using Tavrynexo.Core.Enums;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.CoreEngines;

public interface IApplicationDiscoveryEngine
{
    Task<IReadOnlyList<InstalledApplication>> DiscoverApplicationsAsync(
        bool includeWindowsStoreApps = true,
        bool includeSystemComponents = false,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}

public class ApplicationDiscoveryEngine : IApplicationDiscoveryEngine
{
    private static readonly string[] UninstallRegistrySubKeys =
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    public Task<IReadOnlyList<InstalledApplication>> DiscoverApplicationsAsync(
        bool includeWindowsStoreApps = true,
        bool includeSystemComponents = false,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var appsMap = new Dictionary<string, InstalledApplication>(StringComparer.OrdinalIgnoreCase);
            progress?.Report(10);

            // 1. فحص سجل 64-بت على مستوى الجهاز (HKLM)
            ScanHive(RegistryHive.LocalMachine, RegistryView.Registry64, appsMap, false, includeSystemComponents, cancellationToken);
            progress?.Report(40);

            // 2. فحص سجل 32-بت على مستوى الجهاز (WoW64)
            if (Environment.Is64BitOperatingSystem)
            {
                ScanHive(RegistryHive.LocalMachine, RegistryView.Registry32, appsMap, false, includeSystemComponents, cancellationToken);
            }
            progress?.Report(65);

            // 3. فحص سجل المستخدم الحالي (HKCU)
            ScanHive(RegistryHive.CurrentUser, RegistryView.Default, appsMap, true, includeSystemComponents, cancellationToken);
            progress?.Report(85);

            var verifiedApps = appsMap.Values
                .Where(a => !string.IsNullOrWhiteSpace(a.DisplayName) && (!a.IsSystemComponent || includeSystemComponents))
                .OrderBy(a => a.DisplayName)
                .ToList();

            progress?.Report(100);
            return (IReadOnlyList<InstalledApplication>)verifiedApps;
        }, cancellationToken);
    }

    private void ScanHive(RegistryHive hive, RegistryView view, Dictionary<string, InstalledApplication> destination, bool isPerUser, bool includeSystemComponents, CancellationToken ct)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            foreach (var subKeyPath in UninstallRegistrySubKeys)
            {
                ct.ThrowIfCancellationRequested();
                using var uninstallKey = baseKey.OpenSubKey(subKeyPath);
                if (uninstallKey == null) continue;

                foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                {
                    try
                    {
                        using var appKey = uninstallKey.OpenSubKey(subKeyName);
                        if (appKey == null) continue;

                        string displayName = (appKey.GetValue("DisplayName") as string ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(displayName)) continue;

                        int isSystem = Convert.ToInt32(appKey.GetValue("SystemComponent") ?? 0);
                        if (isSystem == 1 && !includeSystemComponents) continue;

                        string uninstallString = (appKey.GetValue("UninstallString") as string ?? string.Empty).Trim();
                        string quietUninstallString = (appKey.GetValue("QuietUninstallString") as string ?? string.Empty).Trim();
                        int windowsInstaller = Convert.ToInt32(appKey.GetValue("WindowsInstaller") ?? 0);

                        if (string.IsNullOrEmpty(uninstallString) && string.IsNullOrEmpty(quietUninstallString) && windowsInstaller != 1)
                            continue;

                        string publisher = (appKey.GetValue("Publisher") as string ?? string.Empty).Trim();
                        string version = (appKey.GetValue("DisplayVersion") as string ?? string.Empty).Trim();
                        string installLocation = (appKey.GetValue("InstallLocation") as string ?? string.Empty).Trim();

                        var app = new InstalledApplication
                        {
                            DisplayName = displayName,
                            Publisher = publisher,
                            DisplayVersion = version,
                            InstallLocation = installLocation,
                            UninstallString = uninstallString,
                            QuietUninstallString = quietUninstallString,
                            Architecture = view == RegistryView.Registry64 ? "x64" : "x86",
                            AppType = windowsInstaller == 1 ? ApplicationType.WindowsInstallerMsi : ApplicationType.Win32Registry,
                            RegistryKeyPath = $@"{hive}\{subKeyPath}\{subKeyName}",
                            ProductCodeGuid = subKeyName.StartsWith("{") && subKeyName.EndsWith("}") ? subKeyName : string.Empty,
                            IsSystemComponent = isSystem == 1
                        };

                        string dedupeKey = $"{app.DisplayName}_{app.Publisher}_{app.DisplayVersion}";
                        if (!destination.ContainsKey(dedupeKey) || (view == RegistryView.Registry64 && !isPerUser))
                        {
                            destination[dedupeKey] = app;
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }
    }
}
