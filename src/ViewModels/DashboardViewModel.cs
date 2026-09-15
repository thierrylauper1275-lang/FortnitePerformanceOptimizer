using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FortnitePerformanceOptimizer.ViewModels;

/// <summary>
/// ViewModel for Dashboard tab - real-time monitoring
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly PerformanceMonitoringService _performanceMonitoring = PerformanceMonitoringService.Instance;
    private readonly FortniteDetectionService _fortniteDetection = FortniteDetectionService.Instance;
    private CancellationTokenSource? _monitoringCts;

    [ObservableProperty]
    private double averageFps;

    [ObservableProperty]
    private double fps1Low;

    [ObservableProperty]
    private double fps01Low;

    [ObservableProperty]
    private double averageFrametimeMs;

    [ObservableProperty]
    private double cpuUsagePercent;

    [ObservableProperty]
    private double fortniteProcessCpuPercent;

    [ObservableProperty]
    private double gpuUsagePercent;

    [ObservableProperty]
    private double ramUsagePercent;

    [ObservableProperty]
    private double fortniteRamUsageMb;

    [ObservableProperty]
    private double pingMs;

    [ObservableProperty]
    private string fortniteStatusText = "Not Running";

    [ObservableProperty]
    private bool isFortniteRunning;

    [ObservableProperty]
    private bool isMonitoring;

    public ObservableCollection<LogEntry> RecentLogs { get; } = new();

    public DashboardViewModel()
    {
        _fortniteDetection.StatusChanged += OnFortniteStatusChanged;
        _performanceMonitoring.MetricsUpdated += OnMetricsUpdated;
    }

    [RelayCommand]
    public async Task StartMonitoring()
    {
        if (IsMonitoring)
        {
            return;
        }

        try
        {
            IsLoading = true;
            SetStatus("Starting performance monitoring...");

            _monitoringCts = new CancellationTokenSource();
            await _performanceMonitoring.StartMonitoringAsync(_monitoringCts.Token);

            IsMonitoring = true;
            SetSuccess("Performance monitoring started");
        }
        catch (Exception ex)
        {
            SetError($"Failed to start monitoring: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task StopMonitoring()
    {
        if (!IsMonitoring)
        {
            return;
        }

        try
        {
            SetStatus("Stopping performance monitoring...");
            _monitoringCts?.Cancel();
            await _performanceMonitoring.StopMonitoringAsync();

            IsMonitoring = false;
            SetSuccess("Performance monitoring stopped");
        }
        catch (Exception ex)
        {
            SetError($"Failed to stop monitoring: {ex.Message}");
        }
    }

    private void OnMetricsUpdated(object? sender, PerformanceMetrics metrics)
    {
        AverageFps = metrics.AverageFps;
        Fps1Low = metrics.FpsMin1Percent;
        Fps01Low = metrics.FpsMin01Percent;
        AverageFrametimeMs = metrics.AverageFrametimeMs;
        CpuUsagePercent = metrics.CpuUsagePercent;
        FortniteProcessCpuPercent = metrics.FortniteProcessCpuPercent;
        GpuUsagePercent = metrics.GpuUsagePercent;
        RamUsagePercent = metrics.RamUsagePercent;
        FortniteRamUsageMb = metrics.FortniteRamUsageMb;
        PingMs = metrics.PingMs;
    }

    private void OnFortniteStatusChanged(object? sender, FortniteStatusChangedEventArgs e)
    {
        IsFortniteRunning = e.IsRunning;
        FortniteStatusText = e.IsRunning ? "Running" : "Not Running";
    }
}

public interface IReadOnlyObservableCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
}