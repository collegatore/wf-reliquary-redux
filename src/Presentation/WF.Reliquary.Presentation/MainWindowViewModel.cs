using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WF.Reliquary.Application;
using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Presentation;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IDropTableSourceRefreshWorkflow refreshWorkflow;
    private readonly IDropTableImportWorkflow importWorkflow;
    private string statusText = "Checking drop-table file status...";
    private string operationStatusText = string.Empty;
    private string sourceUrlText = DropTableSourceDefaults.SourceUrl;
    private bool isBusy;

    public MainWindowViewModel(
        IDropTableSourceRefreshWorkflow refreshWorkflow,
        IDropTableImportWorkflow importWorkflow)
    {
        this.refreshWorkflow = refreshWorkflow;
        this.importWorkflow = importWorkflow;
        DownloadCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        ImportCommand = new AsyncRelayCommand(ImportAsync, () => !IsBusy);
        SaveSourceUrlCommand = new AsyncRelayCommand(SaveSourceUrlAsync, () => !IsBusy);
    }

    public Func<Task>? OnImportSucceeded { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => ApplicationInfo.Name;

    public string StatusText
    {
        get => statusText;
        private set => SetProperty(ref statusText, value);
    }

    public string OperationStatusText
    {
        get => operationStatusText;
        private set => SetProperty(ref operationStatusText, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetProperty(ref isBusy, value))
            {
                DownloadCommand.RaiseCanExecuteChanged();
                ImportCommand.RaiseCanExecuteChanged();
                SaveSourceUrlCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SourceUrlText
    {
        get => sourceUrlText;
        set => SetProperty(ref sourceUrlText, value);
    }

    public AsyncRelayCommand DownloadCommand { get; }

    public AsyncRelayCommand ImportCommand { get; }

    public AsyncRelayCommand SaveSourceUrlCommand { get; }

    public async Task LoadStatusAsync(CancellationToken cancellationToken = default)
    {
        var settings = await refreshWorkflow.GetSettingsAsync(cancellationToken);
        SourceUrlText = settings.SourceUrl.ToString();
        ApplyStatus(await refreshWorkflow.GetStatusAsync(cancellationToken));
    }

    public async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        OperationStatusText = "Downloading drop-table file...";

        try
        {
            var result = await refreshWorkflow.RefreshAsync();
            ApplyStatus(result.Status);
            OperationStatusText = result.Succeeded
                ? "Drop-table file updated."
                : $"Download failed: {result.ErrorMessage}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ImportAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        OperationStatusText = "Importing stored drop-table file...";

        try
        {
            var result = await importWorkflow.ImportAsync();
            OperationStatusText = result.Succeeded
                ? ImportSuccessText(result)
                : $"Import failed: {result.ErrorMessage}";

            if (result.Succeeded && OnImportSucceeded != null)
            {
                await OnImportSucceeded();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveSourceUrlAsync()
    {
        if (!Uri.TryCreate(SourceUrlText, UriKind.Absolute, out var sourceUrl))
        {
            OperationStatusText = "Source URL must be an absolute URL.";
            return;
        }

        await refreshWorkflow.SaveSettingsAsync(new DropTableSourceSettings(sourceUrl));
        OperationStatusText = "Drop-table source URL saved.";
        ApplyStatus(await refreshWorkflow.GetStatusAsync());
    }

    private void ApplyStatus(DropTableSourceStatus status)
    {
        if (!status.FileExists)
        {
            StatusText = "No drop-table file downloaded yet.";
            return;
        }

        StatusText = string.IsNullOrWhiteSpace(status.LastUpdate)
            ? "Drop-table file exists. Last Update unavailable."
            : $"Last Update: {status.LastUpdate}";
    }

    private static string ImportSuccessText(DropTableImportResult result)
    {
        var lastUpdate = string.IsNullOrWhiteSpace(result.SourceLastUpdate)
            ? "Last Update unavailable"
            : $"Last Update: {result.SourceLastUpdate}";

        return $"Import completed. {lastUpdate}. Relics: {result.Counts.Relics}; relic rewards: {result.Counts.RelicRewards}; mission sources: {result.Counts.MissionSources}; mission rewards: {result.Counts.MissionRewards}.";
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> execute;
    private readonly Func<bool> canExecute;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return canExecute();
    }

    public async void Execute(object? parameter)
    {
        await execute();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
