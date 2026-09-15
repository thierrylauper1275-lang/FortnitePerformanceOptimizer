using System.Windows;

namespace FortnitePerformanceOptimizer;

public partial class MainWindow : Window
{
    private readonly DashboardViewModel _dashboardViewModel = new();
    private readonly SystemViewModel _systemViewModel = new();
    private readonly BenchmarkViewModel _benchmarkViewModel = new();
    private readonly OptimizerViewModel _optimizerViewModel = new();
    private readonly NetworkViewModel _networkViewModel = new();
    private readonly SettingsViewModel _settingsViewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        ShowDashboard();
    }

    private void OnDashboardClick(object sender, RoutedEventArgs e)
    {
        ShowDashboard();
    }

    private void OnSystemClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new SystemView { DataContext = _systemViewModel };
    }

    private void OnBenchmarkClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new BenchmarkView { DataContext = _benchmarkViewModel };
    }

    private void OnOptimizerClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new OptimizerView { DataContext = _optimizerViewModel };
    }

    private void OnNetworkClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new NetworkView { DataContext = _networkViewModel };
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new SettingsView { DataContext = _settingsViewModel };
    }

    private void OnExit(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowDashboard()
    {
        MainContent.Content = new DashboardView { DataContext = _dashboardViewModel };
        _ = _dashboardViewModel.StartMonitoringCommand.ExecuteAsync(null);
    }
}
