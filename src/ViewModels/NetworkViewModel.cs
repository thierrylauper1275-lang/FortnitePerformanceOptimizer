using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FortnitePerformanceOptimizer.ViewModels;

/// <summary>
/// ViewModel for Network tab - ping and latency testing
/// </summary>
public partial class NetworkViewModel : BaseViewModel
{
    private readonly NetworkMonitoringService _networkMonitoring = NetworkMonitoringService.Instance;
    private CancellationTokenSource? _measurementCts;

    [ObservableProperty]
    private double avgPingMs;

    [ObservableProperty]
    private double minPingMs;

    [ObservableProperty]
    private double maxPingMs;

    [ObservableProperty]
    private double jitterMs;

    [ObservableProperty]
    private double packetLossPercent;

    [ObservableProperty]
    private bool isMeasuring;

    [ObservableProperty]
    private string networkStatusText = "Idle";

    [RelayCommand]
    public async Task MeasureNetwork()
    {
        if (IsMeasuring)
        {
            return;
        }

        try
        {
            IsMeasuring = true;
            IsLoading = true;
            SetStatus("Measuring network performance...");
            NetworkStatusText = "Testing...";

            _measurementCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var metrics = await _networkMonitoring.MeasureNetworkAsync(_measurementCts.Token);

            if (metrics.IsValid)
            {
                AvgPingMs = metrics.PingMs;
                MinPingMs = metrics.PingMinMs;
                MaxPingMs = metrics.PingMaxMs;
                JitterMs = metrics.JitterMs;
                PacketLossPercent = metrics.PacketLossPercent;

                var statusQuality = AvgPingMs < 50 ? "Excellent" : 
                                   AvgPingMs < 100 ? "Good" : 
                                   AvgPingMs < 150 ? "Fair" : "Poor";
                
                NetworkStatusText = $"{statusQuality} ({AvgPingMs:F0}ms)";
                SetSuccess("Network measurement completed");
            }
            else
            {
                NetworkStatusText = "No connection";
                SetError("Network measurement failed - check your internet connection");
            }
        }
        catch (OperationCanceledException)
        {
            SetStatus("Network measurement cancelled");
            NetworkStatusText = "Cancelled";
        }
        catch (Exception ex)
        {
            SetError($"Network measurement failed: {ex.Message}");
            NetworkStatusText = "Error";
        }
        finally
        {
            IsMeasuring = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void StopMeasurement()
    {
        _measurementCts?.Cancel();
        IsMeasuring = false;
        NetworkStatusText = "Stopped";
    }
}
