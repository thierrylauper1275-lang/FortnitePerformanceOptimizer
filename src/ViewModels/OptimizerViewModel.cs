using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FortnitePerformanceOptimizer.ViewModels;

/// <summary>
ViewModel for Optimizer tab - applying system optimizations
/// </summary>
public partial class OptimizerViewModel : BaseViewModel
{
    private readonly BackupService _backupService;
    private readonly OptimizationService _optimizationService;
    private SessionBackup? _currentBackup;

    [ObservableProperty]
    private PerformanceProfile selectedProfile = PerformanceProfile.Balanced;

    [ObservableProperty]
    private bool isApplyingOptimizations;

    [ObservableProperty]
    private int optimizationProgress;

    [ObservableProperty]
    private bool canRestoreBackup;

    public ObservableCollection<Optimization> AvailableOptimizations { get; } = new();
    public ObservableCollection<string> AppliedChanges { get; } = new();

    public OptimizerViewModel()
    {
        _backupService = new BackupService();
        _optimizationService = new OptimizationService(_backupService);
    }

    [RelayCommand]
    public async Task ScanOptimizations()
    {
        try
        {
            IsLoading = true;
            SetStatus("Scanning for optimizations...");
            AvailableOptimizations.Clear();

            var optimizations = await _optimizationService.GetApplicableOptimizationsAsync(SelectedProfile);
            foreach (var opt in optimizations)
            {
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(() => AvailableOptimizations.Add(opt));
                }
            }

            SetSuccess($"Found {optimizations.Count} optimizations");
        }
        catch (Exception ex)
        {
            SetError($"Scan failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApplyOptimizations()
    {
        var selectedOptimizations = AvailableOptimizations.Where(x => x.IsApplied).ToList();
        if (selectedOptimizations.Count == 0)
        {
            SetError("Please select at least one optimization");
            return;
        }

        try
        {
            IsApplyingOptimizations = true;
            IsLoading = true;
            OptimizationProgress = 0;
            AppliedChanges.Clear();
            SetStatus("Creating backup...");

            _currentBackup = await _backupService.CreateBackupAsync();
            SetSuccess("Backup created");

            int count = 0;
            foreach (var optimization in selectedOptimizations)
            {
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        AppliedChanges.Add($"Applying: {optimization.Name}...");
                    });
                }

                try
                {
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    if (optimization.ApplyAction != null)
                    {
                        await optimization.ApplyAction(cts.Token);
                        optimization.IsApplied = true;
                        optimization.AppliedAt = DateTime.Now;

                        if (Application.Current?.Dispatcher != null)
                        {
                            Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                AppliedChanges[AppliedChanges.Count - 1] = $"✓ {optimization.Name}";
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Optimization {optimization.Name} failed: {ex.Message}");
                    if (Application.Current?.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            AppliedChanges[AppliedChanges.Count - 1] = $"✗ {optimization.Name}";
                        });
                    }
                }

                count++;
                OptimizationProgress = (count * 100) / selectedOptimizations.Count;
                await Task.Delay(100);
            }

            CanRestoreBackup = true;
            SetSuccess($"Optimizations applied! {count} changes made.");
        }
        catch (Exception ex)
        {
            SetError($"Failed to apply optimizations: {ex.Message}");
        }
        finally
        {
            IsApplyingOptimizations = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RestoreBackup()
    {
        if (_currentBackup == null)
        {
            SetError("No backup available to restore");
            return;
        }

        try
        {
            IsLoading = true;
            SetStatus("Restoring backup...");
            await _backupService.RestoreBackupAsync(_currentBackup);
            
            // Reset optimization states
            foreach (var opt in AvailableOptimizations)
            {
                opt.IsApplied = false;
            }
            AppliedChanges.Clear();
            CanRestoreBackup = false;
            _currentBackup = null;

            SetSuccess("Backup restored successfully");
        }
        catch (Exception ex)
        {
            SetError($"Failed to restore backup: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
