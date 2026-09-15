using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FortnitePerformanceOptimizer.ViewModels;

/// <summary>
/// ViewModel for Benchmark tab
/// </summary>
public partial class BenchmarkViewModel : BaseViewModel
{
    private readonly PerformanceMonitoringService _performanceMonitoring = PerformanceMonitoringService.Instance;
    private readonly FortniteDetectionService _fortniteDetection = FortniteDetectionService.Instance;
    private CancellationTokenSource? _benchmarkCts;
    private BenchmarkResult? _beforeBenchmark;
    private BenchmarkResult? _afterBenchmark;

    [ObservableProperty]
    private bool isBenchmarking;

    [ObservableProperty]
    private int benchmarkDurationSeconds = 30;

    [ObservableProperty]
    private int benchmarkProgress;

    [ObservableProperty]
    private string benchmarkPhaseText = "Ready";

    [ObservableProperty]
    private BenchmarkComparison? benchmarkComparison;

    [ObservableProperty]
    private string summaryText = string.Empty;

    public ObservableCollection<BenchmarkResultDisplay> BeforeResults { get; } = new();
    public ObservableCollection<BenchmarkResultDisplay> AfterResults { get; } = new();

    [RelayCommand]
    public async Task StartBenchmark()
    {
        if (!_fortniteDetection.IsFortniteRunning)
        {
            SetError("Fortnite is not running. Please start Fortnite before benchmarking.");
            return;
        }

        if (IsBenchmarking)
        {
            return;
        }

        try
        {
            IsBenchmarking = true;
            IsLoading = true;
            BenchmarkProgress = 0;
            BeforeResults.Clear();
            SetStatus("Starting BEFORE benchmark...");
            BenchmarkPhaseText = "Measuring BEFORE optimization";

            _benchmarkCts = new CancellationTokenSource();
            _beforeBenchmark = await RunBenchmarkAsync(BenchmarkDurationSeconds, _benchmarkCts.Token);

            if (_beforeBenchmark != null)
            {
                DisplayBenchmarkResults(_beforeBenchmark, BeforeResults);
                SetSuccess($"BEFORE benchmark completed. FPS: {_beforeBenchmark.AverageFps:F1}");
                BenchmarkPhaseText = "BEFORE benchmark complete. Apply optimizations and restart Fortnite, then click 'Benchmark AFTER'".ToUpper();
            }
        }
        catch (OperationCanceledException)
        {
            SetStatus("Benchmark cancelled");
        }
        catch (Exception ex)
        {
            SetError($"Benchmark failed: {ex.Message}");
        }
        finally
        {
            IsBenchmarking = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task StartAfterBenchmark()
    {
        if (_beforeBenchmark == null)
        {
            SetError("Please run BEFORE benchmark first");
            return;
        }

        if (!_fortniteDetection.IsFortniteRunning)
        {
            SetError("Fortnite is not running. Please restart Fortnite in the same scene before benchmarking.");
            return;
        }

        if (IsBenchmarking)
        {
            return;
        }

        try
        {
            IsBenchmarking = true;
            IsLoading = true;
            BenchmarkProgress = 0;
            AfterResults.Clear();
            SetStatus("Starting AFTER benchmark...");
            BenchmarkPhaseText = "Measuring AFTER optimization";

            _benchmarkCts = new CancellationTokenSource();
            _afterBenchmark = await RunBenchmarkAsync(BenchmarkDurationSeconds, _benchmarkCts.Token);

            if (_afterBenchmark != null)
            {
                DisplayBenchmarkResults(_afterBenchmark, AfterResults);
                CompareBenchmarks();
                SetSuccess("AFTER benchmark completed. Comparison ready.");
            }
        }
        catch (OperationCanceledException)
        {
            SetStatus("Benchmark cancelled");
        }
        catch (Exception ex)
        {
            SetError($"Benchmark failed: {ex.Message}");
        }
        finally
        {
            IsBenchmarking = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void StopBenchmark()
    {
        _benchmarkCts?.Cancel();
        IsBenchmarking = false;
        SetStatus("Benchmark stopped");
    }

    [RelayCommand]
    public void ClearResults()
    {
        BeforeResults.Clear();
        AfterResults.Clear();
        BenchmarkComparison = null;
        SummaryText = string.Empty;
        _beforeBenchmark = null;
        _afterBenchmark = null;
        BenchmarkPhaseText = "Ready";
    }

    private async Task<BenchmarkResult> RunBenchmarkAsync(int durationSeconds, CancellationToken cancellationToken)
    {
        var result = new BenchmarkResult
        {
            StartTime = DateTime.Now,
            DurationSeconds = durationSeconds
        };

        _performanceMonitoring.ClearFrameHistory();

        for (int i = 0; i < durationSeconds; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            BenchmarkProgress = (i * 100) / durationSeconds;
            BenchmarkPhaseText = $"Benchmarking... {i}/{durationSeconds}s";
            await Task.Delay(1000, cancellationToken);
        }

        result.EndTime = DateTime.Now;

        // Collect frame statistics
        var (avgFps, fps1Low, fps01Low, maxFps) = _performanceMonitoring.GetFpsStatistics();
        var (avgFrametime, spikeCount) = _performanceMonitoring.GetFrametimeStatistics();

        result.AverageFps = avgFps;
        result.Fps1Percent = fps1Low;
        result.Fps01Percent = fps01Low;
        result.FpsMax = maxFps;
        result.AverageFrametimeMs = avgFrametime;
        result.FrametimeSpikeCount = spikeCount;

        return result;
    }

    private void DisplayBenchmarkResults(BenchmarkResult result, ObservableCollection<BenchmarkResultDisplay> collection)
    {
        collection.Add(new BenchmarkResultDisplay { Label = "Average FPS", Value = $"{result.AverageFps:F1}" });
        collection.Add(new BenchmarkResultDisplay { Label = "1% Low", Value = $"{result.Fps1Percent:F1}" });
        collection.Add(new BenchmarkResultDisplay { Label = "0.1% Low", Value = $"{result.Fps01Percent:F1}" });
        collection.Add(new BenchmarkResultDisplay { Label = "Max FPS", Value = $"{result.FpsMax:F1}" });
        collection.Add(new BenchmarkResultDisplay { Label = "Avg Frametime", Value = $"{result.AverageFrametimeMs:F2} ms" });
        collection.Add(new BenchmarkResultDisplay { Label = "Frametime Spikes", Value = $"{result.FrametimeSpikeCount}" });
    }

    private void CompareBenchmarks()
    {
        if (_beforeBenchmark == null || _afterBenchmark == null)
        {
            return;
        }

        var comparison = new BenchmarkComparison
        {
            BeforeBenchmark = _beforeBenchmark,
            AfterBenchmark = _afterBenchmark,
            FpsImprovement = BenchmarkCalculations.CalculatePercentageImprovement(_beforeBenchmark.AverageFps, _afterBenchmark.AverageFps),
            Fps1PercentImprovement = BenchmarkCalculations.CalculatePercentageImprovement(_beforeBenchmark.Fps1Percent, _afterBenchmark.Fps1Percent),
            Fps01PercentImprovement = BenchmarkCalculations.CalculatePercentageImprovement(_beforeBenchmark.Fps01Percent, _afterBenchmark.Fps01Percent),
            FrametimeImprovement = BenchmarkCalculations.CalculatePercentageImprovement(_beforeBenchmark.AverageFrametimeMs, _afterBenchmark.AverageFrametimeMs, lowerIsBetter: true),
        };

        comparison.OverallImpact = BenchmarkCalculations.DetermineImpactLevel(comparison.FpsImprovement);
        comparison.Summary = BenchmarkCalculations.GenerateSummary(comparison);

        BenchmarkComparison = comparison;
        SummaryText = comparison.Summary;
    }
}

public class BenchmarkResultDisplay
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}