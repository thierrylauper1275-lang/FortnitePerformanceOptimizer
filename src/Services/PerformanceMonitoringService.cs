using System.Diagnostics;

namespace FortnitePerformanceOptimizer.Services;

/// <summary>
/// Service for real-time performance monitoring using Windows Performance Counters
/// </summary>
public class PerformanceMonitoringService : IDisposable
{
    private static readonly Lazy<PerformanceMonitoringService> _instance = new(() => new PerformanceMonitoringService());
    public static PerformanceMonitoringService Instance => _instance.Value;

    private readonly ILogger _logger = LoggingService.Instance;
    private readonly FortniteDetectionService _fortniteDetection = FortniteDetectionService.Instance;

    private System.Diagnostics.PerformanceCounter? _cpuCounter;
    private System.Diagnostics.PerformanceCounter? _ramCounter;
    private CancellationTokenSource? _monitoringCts;
    private Task? _monitoringTask;
    private bool _isMonitoring;

    public event EventHandler<PerformanceMetrics>? MetricsUpdated;

    private PerformanceMetrics _currentMetrics = new();
    public PerformanceMetrics CurrentMetrics => _currentMetrics;

    private readonly Queue<double> _fpsHistory = new();
    private readonly Queue<double> _frametimeHistory = new();
    private const int HistoryBufferSize = 600; // ~10 seconds at 60 FPS
    private const double FRAMETIME_SPIKE_THRESHOLD_MS = 20.0; // Flag framerates below 50 FPS

    private PerformanceMonitoringService()
    {
        try
        {
            _cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total", readOnly: true);
            _ramCounter = new System.Diagnostics.PerformanceCounter("Memory", "% Committed Bytes In Use", readOnly: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Performance counters initialization failed: {ex.Message}");
        }
    }

    public async Task StartMonitoringAsync(CancellationToken externalToken = default)
    {
        if (_isMonitoring)
        {
            return;
        }

        _monitoringCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _isMonitoring = true;

        _logger.LogInfo("Performance monitoring started");

        _monitoringTask = MonitoringLoopAsync(_monitoringCts.Token);
        await _monitoringTask;
    }

    public async Task StopMonitoringAsync()
    {
        if (!_isMonitoring)
        {
            return;
        }

        _isMonitoring = false;
        _monitoringCts?.Cancel();

        if (_monitoringTask != null)
        {
            try
            {
                await _monitoringTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        _logger.LogInfo("Performance monitoring stopped");
    }

    private async Task MonitoringLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _currentMetrics = new PerformanceMetrics
                    {
                        CaptureTime = DateTime.Now,
                        CpuUsagePercent = GetCpuUsage(),
                        RamUsagePercent = GetRamUsage(),
                        RamUsageMb = GetRamUsageMb(),
                        RamTotalMb = GetTotalRamMb(),
                        FortniteRunning = _fortniteDetection.IsFortniteRunning,
                        FortniteProcessId = _fortniteDetection.FortniteProcessId,
                        FortniteRamUsageMb = _fortniteDetection.GetFortniteMemoryUsageMb(),
                    };

                    // GPU and Network metrics would come from other services
                    _currentMetrics.GpuUsagePercent = 0; // Placeholder
                    _currentMetrics.PingMs = 0; // Placeholder

                    _fortniteDetection.RefreshFortniteStatus();

                    MetricsUpdated?.Invoke(this, _currentMetrics);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error in monitoring loop: {ex.Message}");
                }

                await Task.Delay(250, cancellationToken); // Update every 250ms
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
    }

    public void RecordFrameData(double fps, double frametimeMs)
    {
        lock (_fpsHistory)
        {
            _fpsHistory.Enqueue(fps);
            if (_fpsHistory.Count > HistoryBufferSize)
            {
                _fpsHistory.Dequeue();
            }
        }

        lock (_frametimeHistory)
        {
            _frametimeHistory.Enqueue(frametimeMs);
            if (_frametimeHistory.Count > HistoryBufferSize)
            {
                _frametimeHistory.Dequeue();
            }
        }
    }

    public void ClearFrameHistory()
    {
        lock (_fpsHistory)
        {
            _fpsHistory.Clear();
        }

        lock (_frametimeHistory)
        {
            _frametimeHistory.Clear();
        }
    }

    public (double average, double min1Percent, double min01Percent, double max) GetFpsStatistics()
    {
        lock (_fpsHistory)
        {
            if (_fpsHistory.Count == 0)
            {
                return (0, 0, 0, 0);
            }

            var sorted = _fpsHistory.OrderBy(x => x).ToList();
            var average = sorted.Average();
            var max = sorted.Max();

            // 1% Low = 99th percentile (bottom 1%)
            var index1Percent = Math.Max(0, (int)(sorted.Count * 0.01) - 1);
            var min1Percent = sorted[index1Percent];

            // 0.1% Low = 99.9th percentile (bottom 0.1%)
            var index01Percent = Math.Max(0, (int)(sorted.Count * 0.001) - 1);
            var min01Percent = sorted[index01Percent];

            return (average, min1Percent, min01Percent, max);
        }
    }

    public (double average, double spikeCount) GetFrametimeStatistics()
    {
        lock (_frametimeHistory)
        {
            if (_frametimeHistory.Count == 0)
            {
                return (0, 0);
            }

            var average = _frametimeHistory.Average();
            var spikeCount = _frametimeHistory.Count(x => x > FRAMETIME_SPIKE_THRESHOLD_MS);

            return (average, spikeCount);
        }
    }

    private double GetCpuUsage()
    {
        try
        {
            return _cpuCounter?.NextValue() ?? 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    private double GetRamUsage()
    {
        try
        {
            return _ramCounter?.NextValue() ?? 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    private long GetRamUsageMb()
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                if (long.TryParse(obj["FreePhysicalMemory"]?.ToString(), out var freeKb))
                {
                    var totalMb = GetTotalRamMb();
                    var usedMb = totalMb - (freeKb / 1024);
                    return usedMb;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error getting RAM usage: {ex.Message}");
        }
        return 0;
    }

    private long GetTotalRamMb()
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                if (long.TryParse(obj["TotalVisibleMemorySize"]?.ToString(), out var bytes))
                {
                    return bytes / 1024; // KB to MB
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error getting total RAM: {ex.Message}");
        }
        return 8192; // Fallback
    }

    public void Dispose()
    {
        StopMonitoringAsync().Wait();
        _monitoringCts?.Dispose();
        _cpuCounter?.Dispose();
        _ramCounter?.Dispose();
    }
}