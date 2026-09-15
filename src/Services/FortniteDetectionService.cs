using System.Diagnostics;

namespace FortnitePerformanceOptimizer.Services;

/// <summary>
/// Service for detecting and monitoring Fortnite process
/// </summary>
public class FortniteDetectionService
{
    private static readonly Lazy<FortniteDetectionService> _instance = new(() => new FortniteDetectionService());
    public static FortniteDetectionService Instance => _instance.Value;

    private const string FORTNITE_PROCESS_NAME = "FortniteClient-Win64-Shipping";
    private readonly ILogger _logger = LoggingService.Instance;

    public event EventHandler<FortniteStatusChangedEventArgs>? StatusChanged;

    private bool _isRunning;
    private int _processId;
    private Process? _fortniteProcess;
    private DateTime _lastProcessCheck;
    private const int ProcessCheckIntervalMs = 500;

    public bool IsFortniteRunning => _isRunning;
    public int FortniteProcessId => _processId;
    public Process? FortniteProcess => _fortniteProcess;

    public async Task<bool> TryStartMonitoringAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                RefreshFortniteStatus();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start Fortnite monitoring: {ex.Message}");
                return false;
            }
        }, cancellationToken);
    }

    public void RefreshFortniteStatus()
    {
        // Throttle checks
        if ((DateTime.Now - _lastProcessCheck).TotalMilliseconds < ProcessCheckIntervalMs)
        {
            return;
        }

        _lastProcessCheck = DateTime.Now;

        try
        {
            var processes = Process.GetProcessesByName(FORTNITE_PROCESS_NAME);
            var wasRunning = _isRunning;

            if (processes.Length > 0)
            {
                _isRunning = true;
                _processId = processes[0].Id;
                _fortniteProcess = processes[0];
            }
            else
            {
                _isRunning = false;
                _processId = 0;
                _fortniteProcess?.Dispose();
                _fortniteProcess = null;
            }

            if (wasRunning != _isRunning)
            {
                _logger.LogInfo(_isRunning ? "Fortnite process detected" : "Fortnite process stopped");
                StatusChanged?.Invoke(this, new FortniteStatusChangedEventArgs { IsRunning = _isRunning, ProcessId = _processId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error checking Fortnite status: {ex.Message}");
        }
    }

    public long GetFortniteMemoryUsageMb()
    {
        try
        {
            if (_fortniteProcess != null && !_fortniteProcess.HasExited)
            {
                _fortniteProcess.Refresh();
                return _fortniteProcess.WorkingSet64 / (1024 * 1024);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error getting Fortnite memory usage: {ex.Message}");
        }
        return 0;
    }
}

public class FortniteStatusChangedEventArgs : EventArgs
{
    public bool IsRunning { get; set; }
    public int ProcessId { get; set; }
}