using System.Diagnostics;
using Microsoft.Win32;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class WindowsAppManagerService
{
    private static readonly HashSet<string> ProtectedPackageRoots = new(StringComparer.OrdinalIgnoreCase)
    {
        "Microsoft.Windows.ShellExperienceHost",
        "Microsoft.Windows.SecHealthUI",
        "Microsoft.Windows.StartMenuExperienceHost",
        "Microsoft.Windows.Search",
        "Microsoft.Windows.Cortana",
        "Microsoft.AccountsControl"
    };

    public Task<IReadOnlyList<WindowsAppPackageItem>> EnumerateWindowsAppsAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var packages = new List<WindowsAppPackageItem>();
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                using var storeKey = baseKey.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages");
                if (storeKey == null) return (IReadOnlyList<WindowsAppPackageItem>)packages;

                foreach (var pkgName in storeKey.GetSubKeyNames())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var pkg = storeKey.OpenSubKey(pkgName);
                    if (pkg == null) continue;

                    string displayName = pkg.GetValue("DisplayName") as string ?? pkgName;
                    if (displayName.StartsWith("@{")) continue;

                    string publisher = pkg.GetValue("Publisher") as string ?? "Microsoft Corporation";
                    bool isProtected = ProtectedPackageRoots.Any(p => pkgName.StartsWith(p, StringComparison.OrdinalIgnoreCase));

                    packages.Add(new WindowsAppPackageItem
                    {
                        DisplayName = displayName,
                        PackageFullName = pkgName,
                        Publisher = publisher,
                        IsProtectedSystemComponent = isProtected,
                        IsRemovable = !isProtected,
                        RestrictionReason = isProtected ? "Critical Windows Shell / Security component. Removal restricted by OS." : string.Empty
                    });
                }
            }
            catch { }
            return (IReadOnlyList<WindowsAppPackageItem>)packages.OrderBy(p => p.DisplayName).ToList();
        }, cancellationToken);
    }

    public async Task<bool> RemoveWindowsAppPackageAsync(WindowsAppPackageItem package)
    {
        if (package.IsProtectedSystemComponent || !package.IsRemovable)
            throw new InvalidOperationException($"Cannot remove protected Windows component '{package.DisplayName}'.");

        return await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Remove-AppxPackage -Package '{package.PackageFullName}'\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(30000);
                return proc?.ExitCode == 0;
            }
            catch { return false; }
        });
    }
}
