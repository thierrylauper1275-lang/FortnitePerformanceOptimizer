using Xunit;

namespace FortnitePerformanceOptimizer.Tests;

public class BenchmarkCalculationsTests
{
    [Fact]
    public void CalculatePercentageImprovement_HigherIsBetter_ShouldReturnPositiveForImprovement()
    {
        // Arrange
        double before = 100.0;
        double after = 120.0;

        // Act
        double result = BenchmarkCalculations.CalculatePercentageImprovement(before, after, lowerIsBetter: false);

        // Assert
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void CalculatePercentageImprovement_HigherIsBetter_ShouldReturnNegativeForDegradation()
    {
        // Arrange
        double before = 100.0;
        double after = 80.0;

        // Act
        double result = BenchmarkCalculations.CalculatePercentageImprovement(before, after, lowerIsBetter: false);

        // Assert
        Assert.Equal(-20.0, result);
    }

    [Fact]
    public void CalculatePercentageImprovement_LowerIsBetter_ShouldReturnPositiveForImprovement()
    {
        // Arrange
        double before = 20.0;  // Higher frametime (bad)
        double after = 16.0;   // Lower frametime (good)

        // Act
        double result = BenchmarkCalculations.CalculatePercentageImprovement(before, after, lowerIsBetter: true);

        // Assert
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void CalculatePercentageImprovement_ZeroBefore_ShouldReturnZero()
    {
        // Arrange
        double before = 0.0;
        double after = 100.0;

        // Act
        double result = BenchmarkCalculations.CalculatePercentageImprovement(before, after);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Calculate1PercentLow_ShouldReturnCorrectPercentile()
    {
        // Arrange
        var values = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Act
        double result = BenchmarkCalculations.Calculate1PercentLow(values);

        // Assert
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void Calculate01PercentLow_ShouldReturnCorrectPercentile()
    {
        // Arrange
        var values = Enumerable.Range(1, 1000).Select(x => (double)x).ToList();

        // Act
        double result = BenchmarkCalculations.Calculate01PercentLow(values);

        // Assert - 0.1% of 1000 = 1, so index should be close to 0
        Assert.True(result <= 10.0);
    }

    [Fact]
    public void FpsToFrameTimeMs_ShouldConvertCorrectly()
    {
        // Arrange & Act
        double frametime = BenchmarkCalculations.FpsToFrameTimeMs(60.0);

        // Assert
        Assert.Equal(1000.0 / 60.0, frametime, precision: 2);
    }

    [Fact]
    public void FrameTimeToFps_ShouldConvertCorrectly()
    {
        // Arrange & Act
        double fps = BenchmarkCalculations.FrameTimeToFps(16.667);

        // Assert
        Assert.Equal(60.0, fps, precision: 1);
    }

    [Fact]
    public void DetermineImpactLevel_WithinMarginOfError_ShouldReturnNoMeasurableChange()
    {
        // Arrange
        double improvement = 1.5; // Within ±2% margin

        // Act
        var result = BenchmarkCalculations.DetermineImpactLevel(improvement);

        // Assert
        Assert.Equal(BenchmarkImpactLevel.NoMeasurableChange, result);
    }

    [Fact]
    public void DetermineImpactLevel_NegativeImprovement_ShouldReturnDegraded()
    {
        // Arrange
        double improvement = -10.0;

        // Act
        var result = BenchmarkCalculations.DetermineImpactLevel(improvement);

        // Assert
        Assert.Equal(BenchmarkImpactLevel.Degraded, result);
    }

    [Fact]
    public void DetermineImpactLevel_MinorImprovement_ShouldReturnMinorImprovement()
    {
        // Arrange
        double improvement = 3.5; // 2-5%

        // Act
        var result = BenchmarkCalculations.DetermineImpactLevel(improvement);

        // Assert
        Assert.Equal(BenchmarkImpactLevel.MinorImprovement, result);
    }

    [Fact]
    public void DetermineImpactLevel_ModerateImprovement_ShouldReturnModerateImprovement()
    {
        // Arrange
        double improvement = 10.0; // 5-15%

        // Act
        var result = BenchmarkCalculations.DetermineImpactLevel(improvement);

        // Assert
        Assert.Equal(BenchmarkImpactLevel.ModerateImprovement, result);
    }

    [Fact]
    public void DetermineImpactLevel_SignificantImprovement_ShouldReturnSignificantImprovement()
    {
        // Arrange
        double improvement = 20.0; // 15-30%

        // Act
        var result = BenchmarkCalculations.DetermineImpactLevel(improvement);

        // Assert
        Assert.Equal(BenchmarkImpactLevel.SignificantImprovement, result);
    }

    [Fact]
    public void DetermineImpactLevel_MajorImprovement_ShouldReturnMajorImprovement()
    {
        // Arrange
        double improvement = 50.0; // >30%

        // Act
        var result = BenchmarkCalculations.DetermineImpactLevel(improvement);

        // Assert
        Assert.Equal(BenchmarkImpactLevel.MajorImprovement, result);
    }

    [Fact]
    public void GenerateSummary_ShouldCreateValidSummaryText()
    {
        // Arrange
        var comparison = new BenchmarkComparison
        {
            FpsImprovement = 15.0,
            Fps1PercentImprovement = 12.0,
            Fps01PercentImprovement = 10.0,
            FrametimeImprovement = 13.0,
            CpuImprovement = 5.0,
            GpuImprovement = 8.0,
            OverallImpact = BenchmarkImpactLevel.SignificantImprovement
        };

        // Act
        string summary = BenchmarkCalculations.GenerateSummary(comparison);

        // Assert
        Assert.NotEmpty(summary);
        Assert.Contains("15.00%", summary);
    }
}
