using Xunit;

namespace FortnitePerformanceOptimizer.Tests;

public class PerformanceMonitoringServiceTests
{
    [Fact]
    public void GetFpsStatistics_EmptyHistory_ShouldReturnZeros()
    {
        // Arrange
        var service = PerformanceMonitoringService.Instance;
        service.ClearFrameHistory();

        // Act
        var (avg, min1Pct, min01Pct, max) = service.GetFpsStatistics();

        // Assert
        Assert.Equal(0, avg);
        Assert.Equal(0, min1Pct);
        Assert.Equal(0, min01Pct);
        Assert.Equal(0, max);
    }

    [Fact]
    public void GetFpsStatistics_WithData_ShouldCalculateCorrectly()
    {
        // Arrange
        var service = PerformanceMonitoringService.Instance;
        service.ClearFrameHistory();

        var fps = new[] { 50.0, 55.0, 60.0, 60.0, 61.0, 62.0, 63.0, 64.0, 65.0, 70.0 };
        foreach (var f in fps)
        {
            service.RecordFrameData(f, 1000.0 / f);
        }

        // Act
        var (avg, min1Pct, min01Pct, max) = service.GetFpsStatistics();

        // Assert
        Assert.NotEqual(0, avg);
        Assert.Equal(70.0, max);
        Assert.True(min1Pct > 0);
        Assert.True(min01Pct > 0);
    }

    [Fact]
    public void GetFrametimeStatistics_ShouldCalculateAverage()
    {
        // Arrange
        var service = PerformanceMonitoringService.Instance;
        service.ClearFrameHistory();

        var frametimes = new[] { 16.0, 16.5, 17.0, 16.8, 16.2 };
        foreach (var ft in frametimes)
        {
            service.RecordFrameData(1000.0 / ft, ft);
        }

        // Act
        var (avg, spikes) = service.GetFrametimeStatistics();

        // Assert
        Assert.NotEqual(0, avg);
        Assert.True(avg > 15.0);
        Assert.True(avg < 18.0);
    }

    [Fact]
    public void RecordFrameData_ShouldNotExceedBufferSize()
    {
        // Arrange
        var service = PerformanceMonitoringService.Instance;
        service.ClearFrameHistory();
        const int recordCount = 1000;

        // Act
        for (int i = 0; i < recordCount; i++)
        {
            service.RecordFrameData(60.0, 16.667);
        }

        // Get statistics to see buffer size
        var (avg, _, _, _) = service.GetFpsStatistics();

        // Assert - buffer should be capped at 600
        Assert.NotEqual(0, avg);
    }
}
