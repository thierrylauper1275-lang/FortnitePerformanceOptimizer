using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FortnitePerformanceOptimizer.ViewModels;

/// <summary>
/// Base ViewModel with common MVVM functionality
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    protected readonly ILogger Logger = LoggingService.Instance;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [RelayCommand]
    public void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    protected void SetError(string message)
    {
        HasError = true;
        ErrorMessage = message;
        Logger.LogError(message);
    }

    protected void SetStatus(string message)
    {
        StatusMessage = message;
        Logger.LogInfo(message);
    }

    protected void SetSuccess(string message)
    {
        StatusMessage = message;
        HasError = false;
        Logger.LogSuccess(message);
    }
}