namespace FortnitePerformanceOptimizer.Utils;

/// <summary>
/// Static utilities for benchmark calculations and comparisons
/// </summary>
public static class BenchmarkCalculations
{
    /// <summary>
    /// Calculate percentage improvement between two values
    /// Positive = improvement, Negative = degradation
    /// </summary>
    public static double CalculatePercentageImprovement(double before, double after, bool lowerIsBetter = false)
    {
        if (before == 0)
        {
            return 0;
        }

        if (lowerIsBetter)
        {
            // For metrics like frametime, ping, packet loss (lower is better)
            // Improvement = (before - after) / before * 100
            return ((before - after) / before) * 100.0;
        }
        else
        {
            // For metrics like FPS (higher is better)
            // Improvement = (after - before) / before * 100
            return ((after - before) / before) * 100.0;
        }
    }

    /// <summary>
    /// Determine impact level based on improvement percentage
    /// </summary>
    public static BenchmarkImpactLevel DetermineImpactLevel(double improvementPercent, double marginOfError = 2.0)
    {
        if (Math.Abs(improvementPercent) <= marginOfError)
        {
            return BenchmarkImpactLevel.NoMeasurableChange;
        }

        if (improvementPercent < 0)
        {
            return BenchmarkImpactLevel.Degraded;
        }

        if (improvementPercent < 5.0)
        {
            return BenchmarkImpactLevel.MinorImprovement;
        }

        if (improvementPercent < 15.0)
        {
            return BenchmarkImpactLevel.ModerateImprovement;
        }

        if (improvementPercent < 30.0)
        {
            return BenchmarkImpactLevel.SignificantImprovement;
        }

        return BenchmarkImpactLevel.MajorImprovement;
    }

    /// <summary>
    /// Calculate 1% Low (99th percentile, bottom 1%)
    /// </summary>
    public static double Calculate1PercentLow(List<double> values)
    {
        if (values.Count == 0)
        {
            return 0;
        }

        var sorted = values.OrderBy(x => x).ToList();
        var index = Math.Max(0, (int)(sorted.Count * 0.01) - 1);
        return sorted[index];
    }

    /// <summary>
    /// Calculate 0.1% Low (99.9th percentile, bottom 0.1%)
    /// </summary>
    public static double Calculate01PercentLow(List<double> values)
    {
        if (values.Count == 0)
        {
            return 0;
        }

        var sorted = values.OrderBy(x => x).ToList();
        var index = Math.Max(0, (int)(sorted.Count * 0.001) - 1);
        return sorted[index];
    }

    /// <summary>
    /// Convert FPS to average frametime in milliseconds
    /// </summary>
    public static double FpsToFrameTimeMs(double fps)
    {
        return fps > 0 ? 1000.0 / fps : 0;
    }

    /// <summary>
    /// Convert frametime in milliseconds to FPS
    /// </summary>
    public static double FrameTimeToFps(double frametimeMs)
    {
        return frametimeMs > 0 ? 1000.0 / frametimeMs : 0;
    }

    /// <summary>
    /// Generate benchmark summary text
    /// </summary>
    public static string GenerateSummary(BenchmarkComparison comparison)
    {
        var impactText = comparison.OverallImpact switch
        {
            BenchmarkImpactLevel.Degraded => "❌ Performance Degraded",
            BenchmarkImpactLevel.NoMeasurableChange => "➖ No Measurable Change",
            BenchmarkImpactLevel.MinorImprovement => "✅ Minor Improvement",
            BenchmarkImpactLevel.ModerateImprovement => "✅ Moderate Improvement",
            BenchmarkImpactLevel.SignificantImprovement => "✅ Significant Improvement",
            BenchmarkImpactLevel.MajorImprovement => "✅✅ Major Improvement",
            _ => "❓ Unknown"
        };

        return $"""{impactText}

FPS Improvement: {comparison.FpsImprovement:+0.00;-0.00;0.00}%
1% Low Improvement: {comparison.Fps1PercentImprovement:+0.00;-0.00;0.00}%
Frametime Improvement: {comparison.FrametimeImprovement:+0.00;-0.00;0.00}%
CPU Improvement: {comparison.CpuImprovement:+0.00;-0.00;0.00}%
GPU Improvement: {comparison.GpuImprovement:+0.00;-0.00;0.00}%";
    }
}