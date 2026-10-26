using System.Security.Cryptography;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class DuplicateFinderService
{
    public Task<IReadOnlyList<DuplicateFileGroup>> ScanDuplicatesAsync(string targetDirectory, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var groups = new List<DuplicateFileGroup>();
            if (!Directory.Exists(targetDirectory)) return (IReadOnlyList<DuplicateFileGroup>)groups;

            progress?.Report(10);
            var sizeBuckets = new Dictionary<long, List<string>>();

            try
            {
                foreach (var file in Directory.EnumerateFiles(targetDirectory, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var fi = new FileInfo(file);
                        if (fi.Length > 0)
                        {
                            if (!sizeBuckets.TryGetValue(fi.Length, out var list))
                            {
                                list = new List<string>();
                                sizeBuckets[fi.Length] = list;
                            }
                            list.Add(file);
                        }
                    }
                    catch { }
                }
            }
            catch { }

            progress?.Report(40);
            var candidateBuckets = sizeBuckets.Where(b => b.Value.Count > 1).ToList();
            int total = candidateBuckets.Count;
            int processed = 0;

            using var sha256 = SHA256.Create();
            foreach (var bucket in candidateBuckets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var hashGroups = new Dictionary<string, List<DuplicateFileItem>>();

                foreach (var filePath in bucket.Value)
                {
                    try
                    {
                        using var stream = File.OpenRead(filePath);
                        byte[] hash = sha256.ComputeHash(stream);
                        string hashStr = Convert.ToHexString(hash);

                        var item = new DuplicateFileItem
                        {
                            FilePath = filePath,
                            SizeBytes = bucket.Key,
                            LastModifiedUtc = File.GetLastWriteTimeUtc(filePath),
                            IsSelectedForRemoval = false // لا حذف تلقائي نهائياً
                        };

                        if (!hashGroups.TryGetValue(hashStr, out var list))
                        {
                            list = new List<DuplicateFileItem>();
                            hashGroups[hashStr] = list;
                        }
                        list.Add(item);
                    }
                    catch { }
                }

                foreach (var kv in hashGroups.Where(h => h.Value.Count > 1))
                {
                    groups.Add(new DuplicateFileGroup
                    {
                        FileHashSha256 = kv.Key,
                        SingleFileSize = bucket.Key,
                        Duplicates = kv.Value
                    });
                }
                processed++;
                progress?.Report(40 + (int)(((double)processed / Math.Max(1, total)) * 55));
            }

            progress?.Report(100);
            return (IReadOnlyList<DuplicateFileGroup>)groups;
        }, cancellationToken);
    }
}
