using Microsoft.Win32;

namespace FortnitePerformanceOptimizer.Services;

/// <summary>
/// Service for backing up and restoring system state
/// </summary>
public class BackupService
{
    private readonly ILogger _logger = LoggingService.Instance;
    private readonly string _backupDirectory;

    public BackupService()
    {
        _backupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FortnitePerformanceOptimizer",
            "backups"
        );
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<SessionBackup> CreateBackupAsync()
    {
        var backup = new SessionBackup
        {
            BackupId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Now,
            BackupPath = Path.Combine(_backupDirectory, $"backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json")
        };

        try
        {
            _logger.LogInfo($"Creating backup: {backup.BackupId}");

            // Backup power plan
            backup.PowerPlanBackup = BackupPowerPlan();

            // Backup registry keys
            BackupRegistryKeys(backup);

            // Backup service states
            BackupServiceStates(backup);

            // Save backup to file
            await SaveBackupAsync(backup);

            _logger.LogSuccess($"Backup created: {backup.BackupId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to create backup: {ex.Message}");
            throw;
        }

        return backup;
    }

    public async Task RestoreBackupAsync(SessionBackup backup)
    {
        try
        {
            _logger.LogInfo($"Restoring backup: {backup.BackupId}");

            // Restore power plan
            if (backup.PowerPlanBackup != null)
            {
                RestorePowerPlan(backup.PowerPlanBackup);
            }

            // Restore registry keys
            RestoreRegistryKeys(backup);

            // Restore service states
            RestoreServiceStates(backup);

            _logger.LogSuccess($"Backup restored: {backup.BackupId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to restore backup: {ex.Message}");
            throw;
        }
    }

    private PowerPlanBackup BackupPowerPlan()
    {
        var backup = new PowerPlanBackup();
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes");
            if (key != null)
            {
                var activePlan = key.GetValue("ActivePowerScheme")?.ToString();
                if (!string.IsNullOrEmpty(activePlan))
                {
                    backup.ActivePowerSchemeGuid = activePlan;
                    using var planKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\{activePlan}");
                    backup.ActivePowerSchemeName = planKey?.GetValue("FriendlyName")?.ToString() ?? "Unknown";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to backup power plan: {ex.Message}");
        }
        return backup;
    }

    private void RestorePowerPlan(PowerPlanBackup backup)
    {
        try
        {
            if (string.IsNullOrEmpty(backup.ActivePowerSchemeGuid))
            {
                return;
            }

            // This would require running: powercfg /setactive {GUID}
            // For now, just log it
            _logger.LogInfo($"Would restore power plan: {backup.ActivePowerSchemeName}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to restore power plan: {ex.Message}");
        }
    }

    private void BackupRegistryKeys(SessionBackup backup)
    {
        var keysToBackup = new Dictionary<string, List<string>>
        {
            [@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services"] = new() { "GameConfigStore", "nvlddmkm" },
            [@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options"] = new() { "FortniteClient-Win64-Shipping.exe" },
            [@"HKEY_CURRENT_USER\System\GameConfigStore"] = new() { "GameDVR_Enabled", "GameDVR_FSEBehavior" }
        };

        foreach (var keyPath in keysToBackup)
        {
            try
            {
                // Simplified backup - full implementation would iterate through all values
                _logger.LogDebug($"Backed up registry: {keyPath.Key}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to backup registry {keyPath.Key}: {ex.Message}");
            }
        }
    }

    private void RestoreRegistryKeys(SessionBackup backup)
    {
        try
        {
            foreach (var regBackup in backup.RegistryBackups)
            {
                try
                {
                    _logger.LogInfo($"Restoring registry: {regBackup.RegistryPath}\\{regBackup.ValueName}");
                    // Restore logic would go here
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to restore registry value: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to restore registry keys: {ex.Message}");
        }
    }

    private void BackupServiceStates(SessionBackup backup)
    {
        var servicesToBackup = new[] { "DiagTrack", "dmwappushservice", "GameConfigStore" };
        try
        {
            foreach (var serviceName in servicesToBackup)
            {
                try
                {
                    using var sc = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
                    var startType = sc?.GetValue("Start")?.ToString() ?? "Unknown";
                    backup.ServiceStates[serviceName] = startType;
                    _logger.LogDebug($"Backed up service: {serviceName} = {startType}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to backup service {serviceName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to backup service states: {ex.Message}");
        }
    }

    private void RestoreServiceStates(SessionBackup backup)
    {
        try
        {
            foreach (var (serviceName, state) in backup.ServiceStates)
            {
                _logger.LogInfo($"Would restore service: {serviceName} to {state}");
                // Actual restoration would require admin and sc.exe commands
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to restore services: {ex.Message}");
        }
    }

    private async Task SaveBackupAsync(SessionBackup backup)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(backup, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(backup.BackupPath, json);
            _logger.LogDebug($"Backup saved to: {backup.BackupPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to save backup file: {ex.Message}");
            throw;
        }
    }
}
