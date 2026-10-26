using Tavrynexo.Core.Enums;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class JunkCleanerService
{
    public Task<IReadOnlyList<JunkCategoryItem>> ScanJunkCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var categories = new List<JunkCategoryItem>
            {
                ScanDirectoryJunk("User Temporary Files", "Temporary files older than 24 hours.", Path.GetTempPath(), SafetyLevel.SafeToPurge),
                ScanDirectoryJunk("System Temporary Cache", "Windows OS system temp folder residue.", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"), SafetyLevel.SafeToPurge),
                ScanDirectoryJunk("Windows Update Download Cache", "Cached setup files from completed updates.", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download"), SafetyLevel.SafeToPurge),
                ScanDirectoryJunk("Application Crash Dumps", "Memory dumps from past software crashes.", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrashDumps"), SafetyLevel.SafeToPurge)
            };
            return (IReadOnlyList<JunkCategoryItem>)categories;
        }, cancellationToken);
    }

    public async Task<long> PurgeJunkAsync(IEnumerable<JunkCategoryItem> categories, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            long freedBytes = 0;
            var cutoff = DateTime.UtcNow.AddHours(-24); // حماية الملفات النشطة والمعدلة في آخر 24 ساعة
            foreach (var cat in categories.Where(c => c.IsSelectedForCleaning))
            {
                if (!Directory.Exists(cat.DirectoryPath)) continue;
                try
                {
                    foreach (var file in Directory.EnumerateFiles(cat.DirectoryPath, "*", SearchOption.AllDirectories))
                    {
                        ct.ThrowIfCancellationRequested();
                        try
                        {
                            var fi = new FileInfo(file);
                            if (fi.LastWriteTimeUtc < cutoff)
                            {
                                long size = fi.Length;
                                File.SetAttributes(file, FileAttributes.Normal);
                                File.Delete(file);
                                freedBytes += size;
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return freedBytes;
        });
    }

    private JunkCategoryItem ScanDirectoryJunk(string name, string desc, string dirPath, SafetyLevel safety)
    {
        var item = new JunkCategoryItem { CategoryName = name, Description = desc, DirectoryPath = dirPath, SafetyLevel = safety };
        if (Directory.Exists(dirPath))
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);
            try
            {
                foreach (var file in Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        if (fi.LastWriteTimeUtc < cutoff) item.TotalSizeBytes += fi.Length;
                    }
                    catch { }
                }
            }
            catch { }
        }
        return item;
    }
}
