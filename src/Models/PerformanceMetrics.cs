namespace FortnitePerformanceOptimizer.Models;

/// <summary>
/// Represents a single frame's timing data
/// </summary>
public class FrameData
{
    public double FrametimeMs { get; set; }
    public double FpsValue { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Real-time performance snapshot
/// </summary>
public class PerformanceMetrics
{
    // FPS Metrics
    public double AverageFps { get; set; }
    public double FpsMin1Percent { get; set; }  // 1% Low
    public double FpsMin01Percent { get; set; } // 0.1% Low
    public double MaxFps { get; set; }
    
    // Frametime Metrics
    public double AverageFrametimeMs { get; set; }
    public double FrametimeSpikeCount { get; set; }
    public List<FrameData> FrameHistory { get; set; } = new();
    
    // CPU Metrics
    public double CpuUsagePercent { get; set; }
    public double FortniteProcessCpuPercent { get; set; }
    public double CpuClockMhz { get; set; }
    public double CpuTemperatureCelsius { get; set; }
    public bool CpuTemperatureAvailable { get; set; }
    
    // GPU Metrics
    public double GpuUsagePercent { get; set; }
    public double GpuClockMhz { get; set; }
    public double GpuMemoryUsageMb { get; set; }
    public double GpuMemoryTotalMb { get; set; }
    public double GpuTemperatureCelsius { get; set; }
    public bool GpuTemperatureAvailable { get; set; }
    public double GpuPowerUsageW { get; set; }
    public bool GpuPowerUsageAvailable { get; set; }
    
    // RAM Metrics
    public double RamUsageMb { get; set; }
    public double RamTotalMb { get; set; }
    public double RamUsagePercent { get; set; }
    public double FortniteRamUsageMb { get; set; }
    
    // Network Metrics
    public double PingMs { get; set; }
    public double JitterMs { get; set; }
    public double PacketLossPercent { get; set; }
    
    // Fortnite Status
    public bool FortniteRunning { get; set; }
    public int FortniteProcessId { get; set; }
    public DateTime CaptureTime { get; set; }
    public string? LastError { get; set; }
}

/// <summary>
/// Benchmark result containing before/after comparison
/// </summary>
public class BenchmarkResult
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationSeconds { get; set; }
    
    // FPS Statistics
    public double AverageFps { get; set; }
    public double Fps1Percent { get; set; }
    public double Fps01Percent { get; set; }
    public double FpsMax { get; set; }
    public double FpsMin { get; set; }
    
    // Frametime Statistics
    public double AverageFrametimeMs { get; set; }
    public double FrametimeSpikeCount { get; set; }
    public double MaxFrametimeMs { get; set; }
    
    // System Statistics
    public double AverageCpuUsage { get; set; }
    public double AverageGpuUsage { get; set; }
    public double AverageRamUsageMb { get; set; }
    public double AveragePingMs { get; set; }
    public double AveragePacketLoss { get; set; }
    
    // Raw frame data for detailed analysis
    public List<FrameData> FrameDataPoints { get; set; } = new();
}

/// <summary>
/// Comparison between two benchmark results
/// </summary>
public class BenchmarkComparison
{
    public BenchmarkResult BeforeBenchmark { get; set; } = null!;
    public BenchmarkResult AfterBenchmark { get; set; } = null!;
    
    // Percentage improvements (positive = better)
    public double FpsImprovement { get; set; }        // %
    public double Fps1PercentImprovement { get; set; } // %
    public double Fps01PercentImprovement { get; set; } // %
    public double FrametimeImprovement { get; set; }   // % (lower is better)
    public double CpuImprovement { get; set; }         // %
    public double GpuImprovement { get; set; }         // %
    public double RamImprovement { get; set; }         // %
    public double PingImprovement { get; set; }        // %
    
    public BenchmarkImpactLevel OverallImpact { get; set; }
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Impact level classification
/// </summary>
public enum BenchmarkImpactLevel
{
    Degraded,           // Performance got worse
    NoMeasurableChange, // Within margin of error (±2%)
    MinorImprovement,   // 2-5% improvement
    ModerateImprovement, // 5-15% improvement
    SignificantImprovement, // 15-30% improvement
    MajorImprovement    // >30% improvement
}

public enum PerformanceProfile
{
    Balanced,
    Competitive,
    MaxPerformance
}

public enum OptimizationCategory
{
    WindowsGaming,
    GameMode,
    GameBarCaptures,
    PowerPlan,
    BackgroundProcesses,
    StartupPrograms,
    Network,
    NvidiaDriver,
    Fortnite,
    CpuRam
}