using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Tavrynexo.Core.Enums;
using Tavrynexo.Core.Models;
using Tavrynexo.Engine.CoreEngines;
using Tavrynexo.UI.Services;

namespace Tavrynexo.UI.ViewModels;

public partial class InstalledAppsViewModel : ObservableObject
{
    private readonly IApplicationDiscoveryEngine _discoveryService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<InstalledAppsViewModel> _logger;

    private List<InstalledApplication> _allAppsMaster = new();
    private CancellationTokenSource? _scanCts;

    [ObservableProperty] private ObservableCollection<InstalledApplication> _displayedApplications = new();
    [ObservableProperty] private InstalledApplication? _focusedApplication;
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private string _activeFilter = "All";
    [ObservableProperty] private string _sortBy = "Name";
    [ObservableProperty] private bool _sortAscending = true;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private int _totalInstalledCount;
    [ObservableProperty] private string _totalFootprintFormatted = "0 GB";
    [ObservableProperty] private string _statusText = "Ready";

    public InstalledAppsViewModel(IApplicationDiscoveryEngine discoveryService, INotificationService notificationService, ILogger<InstalledAppsViewModel> logger)
    {
        _discoveryService = discoveryService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [RelayCommand]
    public async Task RefreshApplicationsAsync()
    {
        _scanCts?.Cancel();
        _scanCts = new CancellationTokenSource();
        var token = _scanCts.Token;

        IsLoading = true;
        StatusText = "Scanning Windows registries & Store apps...";

        try
        {
            var apps = await _discoveryService.DiscoverApplicationsAsync(true, false, cancellationToken: token);
            token.ThrowIfCancellationRequested();

            _allAppsMaster = apps.ToList();
            ApplyFiltersAndSort();

            TotalInstalledCount = _allAppsMaster.Count;
            long totalBytes = _allAppsMaster.Where(a => a.EstimatedSizeBytes.HasValue).Sum(a => a.EstimatedSizeBytes!.Value);
            TotalFootprintFormatted = $"{totalBytes / (1024.0 * 1024 * 1024):0.##} GB";

            StatusText = $"Loaded {TotalInstalledCount} applications successfully.";
            _notificationService.Show("Scan Complete", $"Found {TotalInstalledCount} applications ({TotalFootprintFormatted})", NotificationSeverity.Success);
        }
        catch (OperationCanceledException) { StatusText = "Scan cancelled."; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enumerate apps");
            StatusText = "Error scanning applications.";
            _notificationService.Show("Scan Error", ex.Message, NotificationSeverity.Error);
        }
        finally { IsLoading = false; }
    }

    partial void OnSearchQueryChanged(string value) => ApplyFiltersAndSort();

    [RelayCommand]
    public void SetFilter(string filter) { ActiveFilter = filter; ApplyFiltersAndSort(); }

    private void ApplyFiltersAndSort()
    {
        IEnumerable<InstalledApplication> result = _allAppsMaster;
        result = ActiveFilter switch
        {
            "Win32" => result.Where(a => a.AppType == ApplicationType.Win32Registry || a.AppType == ApplicationType.WindowsInstallerMsi),
            "Store" => result.Where(a => a.AppType == ApplicationType.UniversalWindowsPlatformAppx),
            "Large" => result.Where(a => a.EstimatedSizeBytes.HasValue && a.EstimatedSizeBytes.Value > 500 * 1024 * 1024),
            "Recent" => result.Where(a =>
                DateTime.TryParse(a.InstallDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var installDate)
                && installDate >= DateTime.Now.AddDays(-30)),
            _ => result
        };

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            string q = SearchQuery.Trim();
            result = result.Where(a => a.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) || a.Publisher.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        result = (SortBy, SortAscending) switch
        {
            ("Name", true) => result.OrderBy(a => a.DisplayName),
            ("Name", false) => result.OrderByDescending(a => a.DisplayName),
            ("Size", true) => result.OrderBy(a => a.EstimatedSizeBytes ?? 0),
            ("Size", false) => result.OrderByDescending(a => a.EstimatedSizeBytes ?? 0),
            _ => result.OrderBy(a => a.DisplayName)
        };

        DisplayedApplications = new ObservableCollection<InstalledApplication>(result);
        IsEmpty = DisplayedApplications.Count == 0;
    }
}
