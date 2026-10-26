using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class PrivacyCleanerService
{
    public Task<IReadOnlyList<PrivacyCategoryItem>> ScanPrivacyTracesAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var categories = new List<PrivacyCategoryItem>();
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Chrome
            categories.Add(CreateCategory("Google Chrome Web Cache", "Temporary website images and script caches.", "None. Safe to clean.", Path.Combine(localApp, @"Google\Chrome\User Data\Default\Cache"), PrivacyTargetType.GoogleChrome));
            categories.Add(CreateCategory("Google Chrome Sessions & Cookies", "Stored website logins and active sessions.", "Warning: Will log you out of active websites.", Path.Combine(localApp, @"Google\Chrome\User Data\Default\Network\Cookies"), PrivacyTargetType.GoogleChrome));

            // Edge
            categories.Add(CreateCategory("Microsoft Edge Web Cache", "Temporary cached web pages.", "None. Safe to clean.", Path.Combine(localApp, @"Microsoft\Edge\User Data\Default\Cache"), PrivacyTargetType.MicrosoftEdge));
            categories.Add(CreateCategory("Microsoft Edge Sessions & Cookies", "Stored website cookies and session states.", "Warning: Active web logins will be terminated.", Path.Combine(localApp, @"Microsoft\Edge\User Data\Default\Network\Cookies"), PrivacyTargetType.MicrosoftEdge));

            // Firefox
            categories.Add(CreateCategory("Mozilla Firefox Cache", "Temporary offline website content.", "None. Safe to clean.", Path.Combine(localApp, @"Mozilla\Firefox\Profiles"), PrivacyTargetType.MozillaFirefox));

            // Windows Recent Docs
            categories.Add(CreateCategory("Windows Recent Documents History", "Recently opened files in Windows Explorer.", "Clears recent files list.", Path.Combine(appData, @"Microsoft\Windows\Recent"), PrivacyTargetType.WindowsRecentDocs));

            return (IReadOnlyList<PrivacyCategoryItem>)categories;
        }, cancellationToken);
    }

    public async Task<int> PurgePrivacyTracesAsync(IEnumerable<PrivacyCategoryItem> categories, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            int purged = 0;
            foreach (var cat in categories.Where(c => c.IsSelected))
            {
                ct.ThrowIfCancellationRequested();
                if (File.Exists(cat.TargetDirectoryOrKey))
                {
                    try { File.Delete(cat.TargetDirectoryOrKey); purged++; } catch { }
                }
                else if (Directory.Exists(cat.TargetDirectoryOrKey))
                {
                    try
                    {
                        foreach (var f in Directory.EnumerateFiles(cat.TargetDirectoryOrKey, "*", SearchOption.AllDirectories))
                        {
                            try { File.Delete(f); purged++; } catch { }
                        }
                    }
                    catch { }
                }
            }
            return purged;
        });
    }

    private PrivacyCategoryItem CreateCategory(string name, string desc, string warning, string path, PrivacyTargetType type)
    {
        var item = new PrivacyCategoryItem { CategoryName = name, Description = desc, WarningNotice = warning, TargetDirectoryOrKey = path, TargetType = type, IsSelected = false };
        if (Directory.Exists(path))
        {
            try
            {
                foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { item.TotalSizeBytes += new FileInfo(f).Length; item.TracesCount++; } catch { }
                }
            }
            catch { }
        }
        else if (File.Exists(path))
        {
            try { item.TotalSizeBytes = new FileInfo(path).Length; item.TracesCount = 1; } catch { }
        }
        return item;
    }
}
