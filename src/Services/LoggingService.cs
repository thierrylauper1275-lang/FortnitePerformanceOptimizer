using System.Collections.ObjectModel;

namespace FortnitePerformanceOptimizer.Services;

/// <summary>
/// Centralized logging service for the application
/// Writes to file and in-memory collection for UI display
/// </summary>
public class LoggingService
{
    private static readonly Lazy<LoggingService> _instance = new(() => new LoggingService());
    public static LoggingService Instance => _instance.Value;

    private readonly ObservableCollection<LogEntry> _logs = new();
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly SemaphoreSlim _logSemaphore = new(1, 1);
    private StreamWriter? _fileWriter;

    public IReadOnlyObservableCollection<LogEntry> Logs => new ReadOnlyObservableCollection<LogEntry>(_logs);

    public LoggingService()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FortnitePerformanceOptimizer",
            "logs"
        );

        Directory.CreateDirectory(_logDirectory);

        _logFilePath = Path.Combine(
            _logDirectory,
            $"fpo_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"
        );

        try
        {
            _fileWriter = new StreamWriter(
                new FileStream(_logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read),
                append: true
            )
            {
                AutoFlush = true
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize log file: {ex.Message}");
        }
    }

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message
        };

        _ = Task.Run(async () => await LogAsync(entry));
    }

    private async Task LogAsync(LogEntry entry)
    {
        try
        {
            await _logSemaphore.WaitAsync();

            // Add to in-memory collection (UI thread)
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (_logs.Count > 1000)
                    {
                        _logs.RemoveAt(0);
                    }
                    _logs.Add(entry);
                });
            }

            // Write to file
            var logLine = $"[{entry.Timestamp:HH:mm:ss}] [{entry.Level}] {entry.Message}";
            _fileWriter?.WriteLine(logLine);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logging error: {ex.Message}");
        }
        finally
        {
            _logSemaphore.Release();
        }
    }

    public void LogDebug(string message) => Log(message, LogLevel.Debug);
    public void LogInfo(string message) => Log(message, LogLevel.Info);
    public void LogWarning(string message) => Log(message, LogLevel.Warning);
    public void LogError(string message) => Log(message, LogLevel.Error);
    public void LogSuccess(string message) => Log(message, LogLevel.Success);

    public void Dispose()
    {
        _fileWriter?.Dispose();
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Success
}