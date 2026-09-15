using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FortnitePerformanceOptimizer.ViewModels;

/// <summary>
/// ViewModel for Settings tab and logging
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool autoStartMonitoring = true;

    [ObservableProperty]
    private bool autoDetectFortnite = true;

    [ObservableProperty]
    private int benchmarkDurationDefault = 30;

    [ObservableProperty]
    private bool showDetailedLogs = false;

    public ObservableCollection<LogEntry> ApplicationLogs { get; } = new();

    public SettingsViewModel()
    {
        var loggingService = LoggingService.Instance;
        if (loggingService.Logs is ObservableCollection<LogEntry> logs)
        {
            foreach (var log in logs)
            {
                ApplicationLogs.Add(log);
            }
        }
    }

    [RelayCommand]
    public void ExportLogs()
    {
        try
        {
            var logsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FortnitePerformanceOptimizer",
                "logs"
            );
            System.Diagnostics.Process.Start("explorer.exe", logsDir);
            SetSuccess($"Opened logs directory: {logsDir}");
        }
        catch (Exception ex)
        {
            SetError($"Failed to open logs: {ex.Message}");
        }
    }

    [RelayCommand]
    public void ClearLogs()
    {
        ApplicationLogs.Clear();
        SetSuccess("In-memory logs cleared");
    }
}
