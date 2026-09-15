using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FortnitePerformanceOptimizer.ViewModels;

/// <summary>
/// ViewModel for System tab - PC scan and performance score
/// </summary>
public partial class SystemViewModel : BaseViewModel
{
    private readonly SystemInfoService _systemInfoService = SystemInfoService.Instance;
    private readonly FortniteDetectionService _fortniteDetection = FortniteDetectionService.Instance;

    [ObservableProperty]
    private SystemInfo? systemInfo;

    [ObservableProperty]
    private PerformanceScore? performanceScore;

    [ObservableProperty]
    private int overallScore;

    [ObservableProperty]
    private bool isScanning;

    public ObservableCollection<SystemScanResult> ScanResults { get; } = new();

    [RelayCommand]
    public async Task ScanPC()
    {
        if (IsScanning)
        {
            return;
        }

        try
        {
            IsScanning = true;
            IsLoading = true;
            ScanResults.Clear();
            SetStatus("Scanning PC...");

            AddScanResult("Scanning CPU...");
            SystemInfo = await _systemInfoService.GetSystemInfoAsync();
            if (SystemInfo != null)
            {
                AddScanResult($"✓ CPU: {SystemInfo.CpuName} ({SystemInfo.CpuCoreCount} cores)");
            }

            await Task.Delay(100);
            AddScanResult("Scanning GPU...");
            if (SystemInfo != null)
            {
                AddScanResult($"✓ GPU: {SystemInfo.GpuName}");
                if (!string.IsNullOrEmpty(SystemInfo.NvidiaDriverVersion))
                {
                    AddScanResult($"✓ NVIDIA Driver: {SystemInfo.NvidiaDriverVersion}");
                }
            }

            await Task.Delay(100);
            AddScanResult("Scanning RAM...");
            if (SystemInfo != null)
            {
                AddScanResult($"✓ RAM: {SystemInfo.TotalRamMb / 1024} GB");
            }

            await Task.Delay(100);
            AddScanResult("Scanning OS...");
            if (SystemInfo != null)
            {
                AddScanResult($"✓ OS: {SystemInfo.WindowsVersion} (Build {SystemInfo.WindowsBuild})");
            }

            await Task.Delay(100);
            AddScanResult("Checking Fortnite...");
            if (SystemInfo?.FortniteInstalled == true)
            {
                AddScanResult($"✓ Fortnite: Installed at {SystemInfo.FortniteInstallPath}");
            }
            else
            {
                AddScanResult("⚠ Fortnite: Not installed");
            }

            await Task.Delay(100);
            AddScanResult("Checking tools...");
            if (SystemInfo?.PresentMonAvailable == true)
            {
                AddScanResult("✓ PresentMon: Available");
            }
            else
            {
                AddScanResult("⚠ PresentMon: Not available");
            }

            await Task.Delay(100);
            AddScanResult("Checking network...");
            if (SystemInfo?.InternetConnected == true)
            {
                AddScanResult("✓ Internet: Connected");
            }
            else
            {
                AddScanResult("⚠ Internet: Not connected");
            }

            // Calculate performance score
            CalculatePerformanceScore();

            SetSuccess("System scan completed");
        }
        catch (Exception ex)
        {
            SetError($"Scan failed: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
            IsLoading = false;
        }
    }

    private void AddScanResult(string result)
    {
        if (Application.Current?.Dispatcher != null)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ScanResults.Add(new SystemScanResult { Message = result, Timestamp = DateTime.Now });
            });
        }
    }

    private void CalculatePerformanceScore()
    {
        if (SystemInfo == null)
        {
            return;
        }

        var score = new PerformanceScore { CalculatedAt = DateTime.Now };

        // CPU Score (simplified)
        score.CpuScore = SystemInfo.CpuCoreCount >= 8 ? 90 : SystemInfo.CpuCoreCount >= 6 ? 75 : 60;

        // GPU Score
        score.GpuScore = SystemInfo.IsNvidiaGpu ? 85 : 70;

        // RAM Score
        var ramGb = SystemInfo.TotalRamMb / 1024;
        score.RamScore = ramGb >= 32 ? 95 : ramGb >= 16 ? 85 : ramGb >= 8 ? 70 : 50;

        // Network Score
        score.NetworkScore = SystemInfo.InternetConnected ? 80 : 0;

        // Overall score (weighted average)
        OverallScore = (score.CpuScore + score.GpuScore * 2 + score.RamScore) / 4;

        PerformanceScore = score;
    }
}

public class SystemScanResult
{
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}