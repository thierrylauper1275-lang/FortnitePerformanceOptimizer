using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace FortnitePerformanceOptimizer.Services;

/// <summary>
/// Service for gathering system hardware and software information
/// </summary>
public class SystemInfoService
{
    private static readonly Lazy<SystemInfoService> _instance = new(() => new SystemInfoService());
    public static SystemInfoService Instance => _instance.Value;

    private SystemInfo? _cachedSystemInfo;
    private DateTime _cacheTime;
    private const int CacheValiditySeconds = 300; // Cache for 5 minutes

    private readonly ILogger _logger = LoggingService.Instance;

    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        // Return cached info if still valid
        if (_cachedSystemInfo != null && (DateTime.Now - _cacheTime).TotalSeconds < CacheValiditySeconds)
        {
            return _cachedSystemInfo;
        }

        _logger.LogInfo("Scanning system information...");

        var info = new SystemInfo
        {
            CpuName = GetCpuName(),
            CpuCoreCount = GetCpuCoreCount(),
            CpuLogicalProcessors = Environment.ProcessorCount,
            GpuName = GetGpuName(),
            GpuVendor = GetGpuVendor(),
            TotalRamMb = GetTotalRamMb(),
            WindowsVersion = GetWindowsVersion(),
            WindowsBuild = GetWindowsBuild(),
            IsWindows11 = IsWindows11(),
            IsWindows10 = IsWindows10(),
            FortniteInstalled = IsFortniteInstalled(),
            FortniteInstallPath = GetFortniteInstallPath(),
            PresentMonAvailable = IsPresentMonAvailable(),
            PerformanceCountersAvailable = ArePerformanceCountersAvailable(),
            CanAccessRegistry = CanAccessRegistry(),
            InternetConnected = await IsInternetConnectedAsync(),
            CurrentPowerPlan = GetCurrentPowerPlan(),
        };

        info.IsNvidiaGpu = info.GpuVendor.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase);

        if (info.IsNvidiaGpu)
        {
            var nvidiaInfo = await GetNvidiaGpuInfoAsync();
            if (nvidiaInfo != null)
            {
                info.NvidiaDriverVersion = nvidiaInfo.DriverVersion;
                info.GpuMemoryMb = nvidiaInfo.MemoryMb;
            }
        }

        _cachedSystemInfo = info;
        _cacheTime = DateTime.Now;

        _logger.LogSuccess($"System scan complete: {info.CpuName} + {info.GpuName}");
        return info;
    }

    private string GetCpuName()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                return obj["Name"]?.ToString() ?? "Unknown CPU";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to get CPU name: {ex.Message}");
        }
        return "Unknown CPU";
    }

    private int GetCpuCoreCount()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                if (int.TryParse(obj["NumberOfCores"]?.ToString(), out var cores))
                {
                    return cores;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to get CPU core count: {ex.Message}");
        }
        return Environment.ProcessorCount;
    }

    private string GetGpuName()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString();
                if (!string.IsNullOrEmpty(name) && !name.Contains("Basic"))
                {
                    return name;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to get GPU name: {ex.Message}");
        }
        return "Unknown GPU";
    }

    private string GetGpuVendor()
    {
        var gpuName = GetGpuName();
        if (gpuName.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)) return "NVIDIA";
        if (gpuName.Contains("AMD", StringComparison.OrdinalIgnoreCase)) return "AMD";
        if (gpuName.Contains("Intel", StringComparison.OrdinalIgnoreCase)) return "Intel";
        return "Unknown";
    }

    private long GetTotalRamMb()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
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
            _logger.LogWarning($"Failed to get RAM info: {ex.Message}");
        }
        return 8192; // Default fallback
    }

    private string GetWindowsVersion()
    {
        try
        {
            var osVersion = Environment.OSVersion.VersionString;
            if (osVersion.Contains("Windows 11")) return "Windows 11";
            if (osVersion.Contains("Windows 10")) return "Windows 10";
            return osVersion;
        }
        catch
        {
            return "Windows";
        }
    }

    private string GetWindowsBuild()
    {
        try
        {
            var buildKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return buildKey?.GetValue("CurrentBuildNumber")?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private bool IsWindows11()
    {
        return GetWindowsVersion().Contains("11");
    }

    private bool IsWindows10()
    {
        return GetWindowsVersion().Contains("10");
    }

    private bool IsFortniteInstalled()
    {
        try
        {
            // Check common Epic Games launcher paths
            var epicPaths = new[]
            {
                Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles") ?? "", "Epic Games", "Fortnite"),
                Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? "", "Epic Games", "Fortnite"),
            };

            foreach (var path in epicPaths)
            {
                if (Directory.Exists(path))
                {
                    var binaryPath = Path.Combine(path, "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");
                    if (File.Exists(binaryPath))
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to check Fortnite installation: {ex.Message}");
        }
        return false;
    }

    private string GetFortniteInstallPath()
    {
        try
        {
            var epicPaths = new[]
            {
                Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles") ?? "", "Epic Games", "Fortnite"),
                Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? "", "Epic Games", "Fortnite"),
            };

            foreach (var path in epicPaths)
            {
                if (Directory.Exists(path))
                {
                    var binaryPath = Path.Combine(path, "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");
                    if (File.Exists(binaryPath))
                    {
                        return path;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to get Fortnite path: {ex.Message}");
        }
        return string.Empty;
    }

    private bool IsPresentMonAvailable()
    {
        try
        {
            var programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles") ?? "";
            var presentMonPath = Path.Combine(programFilesPath, "PresentMon", "PresentMon.exe");
            return File.Exists(presentMonPath);
        }
        catch
        {
            return false;
        }
    }

    private bool ArePerformanceCountersAvailable()
    {
        try
        {
            var perfCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total", readOnly: true);
            _ = perfCounter.NextValue();
            perfCounter.Dispose();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool CanAccessRegistry()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> IsInternetConnectedAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var response = await client.GetAsync("https://1.1.1.1/");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string GetCurrentPowerPlan()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes");
            if (key != null)
            {
                var activePlan = key.GetValue("ActivePowerScheme")?.ToString();
                if (!string.IsNullOrEmpty(activePlan))
                {
                    using var planKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{activePlan}");
                    return planKey?.GetValue("FriendlyName")?.ToString() ?? "Unknown";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to get power plan: {ex.Message}");
        }
        return "Unknown";
    }

    private async Task<NvidiaGpuInfo?> GetNvidiaGpuInfoAsync()
    {
        // This would require NVIDIA NVML SDK or direct driver interaction
        // For now, return null - will be implemented with proper NVIDIA integration
        await Task.Delay(0);
        return null;
    }
}

public interface ILogger
{
    void LogDebug(string message);
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogSuccess(string message);
}

public class LoggerAdapter : ILogger
{
    private readonly LoggingService _loggingService;

    public LoggerAdapter(LoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public void LogDebug(string message) => _loggingService.LogDebug(message);
    public void LogInfo(string message) => _loggingService.LogInfo(message);
    public void LogWarning(string message) => _loggingService.LogWarning(message);
    public void LogError(string message) => _loggingService.LogError(message);
    public void LogSuccess(string message) => _loggingService.LogSuccess(message);
}