using System.Diagnostics;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class RealtimeInstallationMonitor
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private InstallationSessionRecord? _currentSession;
    private bool _isMonitoring;
    private readonly object _lock = new();

    public event EventHandler<MonitoredEvent>? EventCaptured;
    public bool IsMonitoring => _isMonitoring;
    public InstallationSessionRecord? CurrentSession => _currentSession;

    public void StartMonitoring(string applicationName, string installerPath)
    {
        lock (_lock)
        {
            if (_isMonitoring) return;
            _currentSession = new InstallationSessionRecord
            {
                ApplicationName = applicationName,
                InstallerPath = installerPath,
                StartTimeUtc = DateTime.UtcNow
            };

            var pathsToWatch = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };

            _watchers.Clear();
            foreach (var path in pathsToWatch)
            {
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) continue;
                try
                {
                    var watcher = new FileSystemWatcher(path)
                    {
                        IncludeSubdirectories = true,
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size,
                        EnableRaisingEvents = true
                    };
                    watcher.Created += (s, e) => RecordEvent(e.FullPath, MonitoredEventKind.FileCreated);
                    watcher.Changed += (s, e) => RecordEvent(e.FullPath, MonitoredEventKind.FileModified);
                    watcher.Deleted += (s, e) => RecordEvent(e.FullPath, MonitoredEventKind.FileDeleted);
                    _watchers.Add(watcher);
                }
                catch { }
            }
            _isMonitoring = true;
        }
    }

    public InstallationSessionRecord StopMonitoring()
    {
        lock (_lock)
        {
            if (!_isMonitoring || _currentSession == null) throw new InvalidOperationException("No active monitoring session.");
            foreach (var w in _watchers) { w.EnableRaisingEvents = false; w.Dispose(); }
            _watchers.Clear();

            _currentSession.EndTimeUtc = DateTime.UtcNow;
            _currentSession.FilesCreatedCount = _currentSession.Events.Count(e => e.Kind == MonitoredEventKind.FileCreated);
            _isMonitoring = false;
            return _currentSession;
        }
    }

    private void RecordEvent(string path, MonitoredEventKind kind)
    {
        lock (_lock)
        {
            if (!_isMonitoring || _currentSession == null) return;
            var ev = new MonitoredEvent
            {
                Kind = kind,
                Mechanism = CaptureMechanism.LiveCapturedEvent,
                TargetPath = path,
                Timestamp = DateTime.UtcNow
            };
            _currentSession.Events.Add(ev);
            EventCaptured?.Invoke(this, ev);
        }
    }
}
