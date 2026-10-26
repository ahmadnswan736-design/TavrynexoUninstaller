using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Tavrynexo.Core.Enums;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.CoreEngines;

public class MultiFactorLeftoverScanner
{
    public async Task<List<DetailedLeftoverItem>> PerformMultiFactorScanAsync(
        InstalledApplication app,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var leftovers = new List<DetailedLeftoverItem>();
        if (app == null) return leftovers;

        await Task.Run(() =>
        {
            progress?.Report(15);
            cancellationToken.ThrowIfCancellationRequested();

            ScanDirectories(app, leftovers, cancellationToken);
            progress?.Report(60);
            cancellationToken.ThrowIfCancellationRequested();

            ScanRegistry(app, leftovers, cancellationToken);
            progress?.Report(100);
        }, cancellationToken);

        return leftovers;
    }

    private void ScanDirectories(InstalledApplication app, List<DetailedLeftoverItem> results, CancellationToken cancellationToken)
    {
        string name = app.DisplayName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name)) return;

        string[] roots =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
        };

        foreach (var root in roots)
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) continue;

            try
            {
                var candidate = Path.Combine(root, name);
                if (Directory.Exists(candidate))
                {
                    var item = new DetailedLeftoverItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        PathOrKey = candidate,
                        Type = LeftoverType.Folder,
                        Safety = SafetyLevel.SafeToPurge,
                        Confidence = ConfidenceLevel.High,
                        Reason = "Application data residue directory match"
                    };
                    item.Evidences.Add(new LeftoverEvidence
                    {
                        Type = EvidenceType.PublisherAndAppFolderMatch,
                        Description = $"Folder detected: {candidate}",
                        Weight = 85
                    });
                    results.Add(item);
                }
            }
            catch
            {
                // Ignore directory enumeration exceptions
            }
        }
    }

    private void ScanRegistry(InstalledApplication app, List<DetailedLeftoverItem> results, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows()) return;

        string name = app.DisplayName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name)) return;

        string[] subKeys = { @"Software\" + name };
        if (!string.IsNullOrWhiteSpace(app.Publisher))
        {
            subKeys = new[] { @"Software\" + app.Publisher, @"Software\" + name };
        }

        foreach (var subKey in subKeys)
        {
            if (cancellationToken.IsCancellationRequested) break;
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(subKey);
                if (key != null)
                {
                    var item = new DetailedLeftoverItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        PathOrKey = $@"HKEY_CURRENT_USER\{subKey}",
                        Type = LeftoverType.RegistryKey,
                        Safety = SafetyLevel.SafeToPurge,
                        Confidence = ConfidenceLevel.High,
                        Reason = "Registry software key residue"
                    };
                    item.Evidences.Add(new LeftoverEvidence
                    {
                        Type = EvidenceType.RegistryValueReferenceMatch,
                        Description = $@"HKCU\{subKey}",
                        Weight = 80
                    });
                    results.Add(item);
                }
            }
            catch
            {
                // Ignore registry access exceptions
            }
        }
    }
}
