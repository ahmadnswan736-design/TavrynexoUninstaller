using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class QuarantineRecoveryEngine
{
    private readonly string _vaultPath;

    public QuarantineRecoveryEngine(string? customVaultPath = null)
    {
        _vaultPath = customVaultPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TavrynexoUninstaller",
            "QuarantineVault");
        Directory.CreateDirectory(_vaultPath);
    }

    public Task<IReadOnlyList<QuarantineArchiveManifest>> EnumerateQuarantinePackagesAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var manifests = new List<QuarantineArchiveManifest>();
            if (Directory.Exists(_vaultPath))
            {
                foreach (var zipFile in Directory.EnumerateFiles(_vaultPath, "*.tnbackup"))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        using var archive = ZipFile.OpenRead(zipFile);
                        var manifestEntry = archive.GetEntry("manifest.json");
                        if (manifestEntry != null)
                        {
                            using var stream = manifestEntry.Open();
                            using var reader = new StreamReader(stream);
                            var manifest = JsonSerializer.Deserialize<QuarantineArchiveManifest>(reader.ReadToEnd());
                            if (manifest != null)
                            {
                                manifest.ArchiveZipPath = zipFile;
                                manifests.Add(manifest);
                                continue;
                            }
                        }
                        manifests.Add(new QuarantineArchiveManifest
                        {
                            ApplicationName = Path.GetFileNameWithoutExtension(zipFile),
                            ArchiveZipPath = zipFile,
                            CreatedAtUtc = File.GetCreationTimeUtc(zipFile)
                        });
                    }
                    catch { }
                }
            }
            return (IReadOnlyList<QuarantineArchiveManifest>)manifests.OrderByDescending(m => m.CreatedAtUtc).ToList();
        }, cancellationToken);
    }

    public async Task<bool> VerifyArchiveIntegrityAsync(QuarantineArchiveManifest manifest)
    {
        if (!File.Exists(manifest.ArchiveZipPath)) return false;
        return await Task.Run(() =>
        {
            try
            {
                using var archive = ZipFile.OpenRead(manifest.ArchiveZipPath);
                foreach (var entry in archive.Entries)
                {
                    using var s = entry.Open();
                    using var sha = SHA256.Create();
                    sha.ComputeHash(s);
                }
                return true;
            }
            catch { return false; }
        });
    }

    public async Task<RestorationStatus> RestoreQuarantinePackageAsync(QuarantineArchiveManifest manifest)
    {
        if (!File.Exists(manifest.ArchiveZipPath)) return RestorationStatus.RestorationFailed;
        bool isIntact = await VerifyArchiveIntegrityAsync(manifest);
        if (!isIntact)
        {
            manifest.DiagnosticError = "Archive integrity failed. File may be corrupted.";
            return RestorationStatus.RestorationFailed;
        }

        return await Task.Run(() =>
        {
            int restoredCount = 0;
            int failedCount = 0;
            try
            {
                using var archive = ZipFile.OpenRead(manifest.ArchiveZipPath);
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.Equals("manifest.json", StringComparison.OrdinalIgnoreCase)) continue;

                    if (entry.Name.EndsWith(".reg", StringComparison.OrdinalIgnoreCase))
                    {
                        string tempReg = Path.Combine(Path.GetTempPath(), entry.Name);
                        entry.ExtractToFile(tempReg, true);
                        var psi = new ProcessStartInfo
                        {
                            FileName = "reg.exe",
                            Arguments = $"import \"{tempReg}\"",
                            Verb = "runas",
                            CreateNoWindow = true,
                            UseShellExecute = true
                        };
                        using var proc = Process.Start(psi);
                        proc?.WaitForExit(10000);
                        try { File.Delete(tempReg); } catch { }
                        restoredCount++;
                    }
                    else
                    {
                        var fileRecord = manifest.Files.FirstOrDefault(f => f.InternalArchiveName.Equals(entry.Name, StringComparison.OrdinalIgnoreCase));
                        string targetPath = fileRecord?.OriginalFilePath ?? string.Empty;
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            try
                            {
                                string? parent = Path.GetDirectoryName(targetPath);
                                if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
                                entry.ExtractToFile(targetPath, true);
                                if (File.Exists(targetPath)) { restoredCount++; if (fileRecord != null) fileRecord.WasRestored = true; }
                                else failedCount++;
                            }
                            catch { failedCount++; }
                        }
                    }
                }
                if (restoredCount > 0 && failedCount == 0)
                {
                    manifest.Status = RestorationStatus.RestoredSuccessfully;
                    return RestorationStatus.RestoredSuccessfully;
                }
                manifest.Status = RestorationStatus.RestorationFailed;
                return RestorationStatus.RestorationFailed;
            }
            catch (Exception ex)
            {
                manifest.DiagnosticError = ex.Message;
                manifest.Status = RestorationStatus.RestorationFailed;
                return RestorationStatus.RestorationFailed;
            }
        });
    }
}
