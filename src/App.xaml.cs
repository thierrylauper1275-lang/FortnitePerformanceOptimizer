using System.Windows;

namespace FortnitePerformanceOptimizer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var logger = LoggingService.Instance;
        logger.LogSuccess("Application started");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LoggingService.Instance.LogInfo("Application shutting down");
        LoggingService.Instance.Dispose();
        base.OnExit(e);
    }
}
