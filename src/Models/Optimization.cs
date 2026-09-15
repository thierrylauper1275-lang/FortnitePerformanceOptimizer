namespace FortnitePerformanceOptimizer.Models;

/// <summary>
/// Represents a single optimization that can be applied to the system
/// </summary>
public class Optimization
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OptimizationCategory Category { get; set; }
    public PerformanceProfile ApplicableProfile { get; set; } = PerformanceProfile.Balanced;
    
    // Safety and compatibility
    public bool IsApplicable { get; set; } = true;
    public string? ApplicabilityReason { get; set; } // Why it's not applicable, if applicable is false
    public bool RequiresAdminRights { get; set; } = true;
    public bool RequiresRestart { get; set; } = false;
    
    // Expected impact
    public double ExpectedFpsImprovementPercent { get; set; }
    public string ImpactDescription { get; set; } = string.Empty;
    
    // Backup and restore
    public bool IsApplied { get; set; }
    public Dictionary<string, string> BackupData { get; set; } = new();
    public DateTime? AppliedAt { get; set; }
    
    // Execution
    public Func<CancellationToken, Task<bool>>? ApplyAction { get; set; }
    public Func<CancellationToken, Task<bool>>? RestoreAction { get; set; }
}

/// <summary>
/// Registry setting that needs to be backed up and restored
/// </summary>
public class RegistryBackup
{
    public string RegistryPath { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public object? OriginalValue { get; set; }
    public object? NewValue { get; set; }
    public string? ValueType { get; set; } // REG_SZ, REG_DWORD, etc.
    public DateTime BackupTime { get; set; }
}

/// <summary>
/// Session backup containing all system state snapshots
/// </summary>
public class SessionBackup
{
    public string BackupId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string Description { get; set; } = "Automatic backup before optimization";
    
    public List<RegistryBackup> RegistryBackups { get; set; } = new();
    public PowerPlanBackup? PowerPlanBackup { get; set; }
    public Dictionary<string, string> ServiceStates { get; set; } = new(); // Service name -> Enabled/Disabled
    public Dictionary<string, string> AppSettings { get; set; } = new();
    
    public string BackupPath { get; set; } = string.Empty; // Local storage path
}

/// <summary>
/// Power plan configuration snapshot
/// </summary>
public class PowerPlanBackup
{
    public string ActivePowerSchemeName { get; set; } = string.Empty;
    public string ActivePowerSchemeGuid { get; set; } = string.Empty;
    public Dictionary<string, string> PowerSettings { get; set; } = new();
}

/// <summary>
/// Optimization result after applying changes
/// </summary>
public class OptimizationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> AppliedOptimizations { get; set; } = new();
    public List<string> FailedOptimizations { get; set; } = new();
    public List<string> WarningMessages { get; set; } = new();
    public SessionBackup? Backup { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.Now;
    public bool RequiresRestart { get; set; }
    public string RestartMessage { get; set; } = string.Empty;
}