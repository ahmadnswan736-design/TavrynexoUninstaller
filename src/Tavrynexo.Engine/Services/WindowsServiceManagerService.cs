using System.Diagnostics;
using Microsoft.Win32;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class WindowsServiceManagerService
{
    private static readonly HashSet<string> ProtectedServices = new(StringComparer.OrdinalIgnoreCase)
    {
        "RpcSs", "DcomLaunch", "WinDefend", "CryptSvc", "Dhcp", "Dnscache",
        "EventLog", "PlugPlay", "SamSs", "LSM", "TermService", "SecurityHealthService", "MpsSvc", "BFE"
    };

    public Task<IReadOnlyList<ServiceInfoItem>> EnumerateServicesAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var result = new List<ServiceInfoItem>();
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                using var servicesKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services");
                if (servicesKey == null) return (IReadOnlyList<ServiceInfoItem>)result;

                foreach (var sName in servicesKey.GetSubKeyNames())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var sKey = servicesKey.OpenSubKey(sName);
                    if (sKey == null) continue;

                    object? typeObj = sKey.GetValue("Type");
                    if (typeObj == null) continue;
                    int serviceType = Convert.ToInt32(typeObj);
                    if ((serviceType & 0x10) == 0 && (serviceType & 0x20) == 0) continue;

                    string displayName = sKey.GetValue("DisplayName") as string ?? sName;
                    string imagePath = sKey.GetValue("ImagePath") as string ?? string.Empty;
                    int startType = Convert.ToInt32(sKey.GetValue("Start") ?? 3);

                    var startup = startType switch
                    {
                        2 => ServiceStartupMode.Automatic,
                        3 => ServiceStartupMode.Manual,
                        4 => ServiceStartupMode.Disabled,
                        _ => ServiceStartupMode.Unknown
                    };

                    bool isProtected = ProtectedServices.Contains(sName);
                    result.Add(new ServiceInfoItem
                    {
                        ServiceName = sName,
                        DisplayName = displayName,
                        BinaryPath = imagePath,
                        StartupType = startup,
                        IsProtectedSystemService = isProtected,
                        ProtectionWarning = isProtected ? "Protected core Windows service. Modification strictly blocked." : string.Empty
                    });
                }
            }
            catch { }
            return (IReadOnlyList<ServiceInfoItem>)result.OrderBy(s => s.DisplayName).ToList();
        }, cancellationToken);
    }

    public async Task<bool> ChangeServiceStartupTypeAsync(string serviceName, ServiceStartupMode newMode)
    {
        if (ProtectedServices.Contains(serviceName))
            throw new InvalidOperationException($"Modification of protected service '{serviceName}' is blocked.");

        return await Task.Run(() =>
        {
            try
            {
                string modeStr = newMode switch
                {
                    ServiceStartupMode.Automatic => "auto",
                    ServiceStartupMode.Manual => "demand",
                    ServiceStartupMode.Disabled => "disabled",
                    _ => "demand"
                };
                var psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"config \"{serviceName}\" start= {modeStr}",
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(10000);
                return proc?.ExitCode == 0;
            }
            catch { return false; }
        });
    }
}
