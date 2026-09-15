using Xunit;

namespace FortnitePerformanceOptimizer.Tests;

public class BackupServiceTests
{
    [Fact]
    public async Task CreateBackupAsync_ShouldCreateValidBackup()
    {
        // Arrange
        var backupService = new BackupService();

        // Act
        var backup = await backupService.CreateBackupAsync();

        // Assert
        Assert.NotNull(backup);
        Assert.NotEmpty(backup.BackupId);
        Assert.NotEmpty(backup.BackupPath);
        Assert.True(File.Exists(backup.BackupPath));
    }

    [Fact]
    public async Task CreateBackupAsync_ShouldHaveTimestamp()
    {
        // Arrange
        var backupService = new BackupService();

        // Act
        var backup = await backupService.CreateBackupAsync();

        // Assert
        Assert.NotEqual(default, backup.CreatedAt);
        Assert.True(backup.CreatedAt <= DateTime.Now);
    }

    [Fact]
    public async Task RestoreBackupAsync_ShouldNotThrowWhenValid()
    {
        // Arrange
        var backupService = new BackupService();
        var backup = await backupService.CreateBackupAsync();

        // Act & Assert
        await backupService.RestoreBackupAsync(backup);
    }
}
