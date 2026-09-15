namespace FortnitePerformanceOptimizer.Models;

/// <summary>
/// System hardware and software information
/// </summary>
public class SystemInfo
{
    // CPU Information
    public string CpuName { get; set; } = string.Empty;
    public int CpuCoreCount { get; set; }
    public int CpuLogicalProcessors { get; set; }
    public double CpuBaseClockGhz { get; set; }
    public double CpuMaxClockGhz { get; set; }
    
    // GPU Information
    public string GpuName { get; set; } = string.Empty;
    public string GpuVendor { get; set; } = string.Empty; // NVIDIA, AMD, Intel
    public bool IsNvidiaGpu { get; set; }
    public string? NvidiaDriverVersion { get; set; }
    public long GpuMemoryMb { get; set; }
    public string? NvmlAvailable { get; set; }  // NVIDIA Management Library status
    
    // RAM Information
    public long TotalRamMb { get; set; }
    public long AvailableRamMb { get; set; }
    
    // OS Information
    public string WindowsVersion { get; set; } = string.Empty; // "Windows 10" or "Windows 11"
    public string WindowsBuild { get; set; } = string.Empty;
    public bool IsWindows11 { get; set; }
    public bool IsWindows10 { get; set; }
    
    // Game Installation
    public bool FortniteInstalled { get; set; }
    public string FortniteInstallPath { get; set; } = string.Empty;
    public string FortniteVersion { get; set; } = string.Empty;
    
    // Tool Availability
    public bool PresentMonAvailable { get; set; }
    public string PresentMonPath { get; set; } = string.Empty;
    public bool PerformanceCountersAvailable { get; set; }
    public bool CanAccessRegistry { get; set; }
    
    // Network
    public string NetworkInterfaceName { get; set; } = string.Empty;
    public bool InternetConnected { get; set; }
    
    // Power Plan
    public string CurrentPowerPlan { get; set; } = string.Empty; // "High Performance", "Balanced", etc.
    public string CurrentPowerSchemeGuid { get; set; } = string.Empty;
}

/// <summary>
/// Performance score calculation breakdown
/// </summary>
public class PerformanceScore
{
    public int OverallScore { get; set; }  // 0-100
    public int CpuScore { get; set; }      // 0-100
    public int GpuScore { get; set; }      // 0-100
    public int RamScore { get; set; }      // 0-100
    public int NetworkScore { get; set; }  // 0-100
    public int StorageScore { get; set; }  // 0-100
    
    public string Recommendation { get; set; } = string.Empty;
    public List<string> OptimizationSuggestions { get; set; } = new();
    
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// GPU-specific information from NVIDIA
/// </summary>
public class NvidiaGpuInfo
{
    public string GpuName { get; set; } = string.Empty;
    public string DriverVersion { get; set; } = string.Empty;
    public long MemoryMb { get; set; }
    public double? Temperature { get; set; }
    public double? ClockMhz { get; set; }
    public double? MemoryClockMhz { get; set; }
    public int? PowerLimitW { get; set; }
    public double? PowerUsageW { get; set; }
    public bool NvmlSupported { get; set; }
}