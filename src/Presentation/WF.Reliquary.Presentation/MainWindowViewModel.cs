using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WF.Reliquary.Application;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Presentation;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IDropTableSourceRefreshWorkflow refreshWorkflow;
    private readonly IDropTableImportWorkflow importWorkflow;
    private readonly IDropTableLocalFileSourceWorkflow localFileWorkflow;
    private readonly IDropTableSourceExporter sourceExporter;
    private readonly IImportMetadataReader importMetadataReader;
    private readonly IDropTableImportStore importStore;
    private string sourceStatusText = "Checking stored source...";
    private string operationStatusText = string.Empty;
    private string importStatusText = "No source imported yet.";
    private string footerImportStatusText = string.Empty;
    private bool isBusy;
    private bool hasStoredSources;
    private bool hasImportedData;

    public MainWindowViewModel(
        IDropTableSourceRefreshWorkflow refreshWorkflow,
        IDropTableImportWorkflow importWorkflow,
        IImportMetadataReader importMetadataReader,
        IDropTableLocalFileSourceWorkflow localFileWorkflow,
        IDropTableSourceExporter sourceExporter,
        IDropTableImportStore importStore)
    {
        this.refreshWorkflow = refreshWorkflow;
        this.importWorkflow = importWorkflow;
        this.localFileWorkflow = localFileWorkflow;
        this.sourceExporter = sourceExporter;
        this.importMetadataReader = importMetadataReader;
        this.importStore = importStore;
        DownloadCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        ImportCommand = new AsyncRelayCommand(ImportAsync, () => !IsBusy && HasStoredSources);
        ReadFromFileCommand = new AsyncRelayCommand(ReadFromFileAsync, () => !IsBusy);
        ExportSourcesCommand = new AsyncRelayCommand(ExportSourcesAsync, () => !IsBusy && HasStoredSources);
        ClearImportCommand = new AsyncRelayCommand(ClearImportAsync, () => HasImportedData && !IsBusy);
    }

    public Func<Task>? OnImportSucceeded { get; set; }

    public Func<Task>? OnImportCleared { get; set; }

    public Func<Task<string?>>? PickSourceFileAsync { get; set; }

    public Func<Task<string?>>? PickExportFolderAsync { get; set; }

    public Func<Task<bool>>? ConfirmClear { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => ApplicationInfo.Name;

    public string SourceStatusText
    {
        get => sourceStatusText;
        private set => SetProperty(ref sourceStatusText, value);
    }

    public string FooterImportStatusText
    {
        get => footerImportStatusText;
        private set => SetProperty(ref footerImportStatusText, value);
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
                ReadFromFileCommand.RaiseCanExecuteChanged();
                ExportSourcesCommand.RaiseCanExecuteChanged();
                ClearImportCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasStoredSources
    {
        get => hasStoredSources;
        private set
        {
            if (SetProperty(ref hasStoredSources, value))
            {
                ImportCommand.RaiseCanExecuteChanged();
                ExportSourcesCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasImportedData
    {
        get => hasImportedData;
        private set
        {
            if (SetProperty(ref hasImportedData, value))
            {
                ClearImportCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ImportStatusText
    {
        get => importStatusText;
        private set => SetProperty(ref importStatusText, value);
    }

    public AsyncRelayCommand DownloadCommand { get; }

    public AsyncRelayCommand ImportCommand { get; }

    public AsyncRelayCommand ReadFromFileCommand { get; }

    public AsyncRelayCommand ExportSourcesCommand { get; }

    public AsyncRelayCommand ClearImportCommand { get; }

    public async Task LoadStatusAsync(CancellationToken cancellationToken = default)
    {
        ApplyStatus(await refreshWorkflow.GetStatusAsync(cancellationToken));
        await RefreshImportStatusAsync(cancellationToken);
        HasStoredSources = await refreshWorkflow.HasStoredSourcesAsync(cancellationToken);
    }

    public async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        OperationStatusText = "Downloading source from URL...";
        await Task.Yield();

        try
        {
            var result = await refreshWorkflow.RefreshAsync();
            ApplyStatus(result.Status);
            OperationStatusText = result.Succeeded
                ? $"Source downloaded. {LastUpdateText(result.Status.LastUpdate)}."
                : $"Download failed: {result.ErrorMessage}";

            if (result.Succeeded)
            {
                HasStoredSources = true;
            }
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
        OperationStatusText = "Importing stored source...";
        await Task.Yield();

        try
        {
            var result = await importWorkflow.ImportAsync();
            if (result.Succeeded)
            {
                OperationStatusText = ImportCompletedText(result);
                await RefreshImportStatusAsync();
                ImportStatusText = ImportStatusDetail(result);
                if (OnImportSucceeded != null)
                {
                    await OnImportSucceeded();
                }
            }
            else
            {
                OperationStatusText = $"Import failed: {result.ErrorMessage}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ClearImportAsync()
    {
        if (IsBusy || !HasImportedData)
        {
            return;
        }

        if (ConfirmClear != null && !await ConfirmClear())
        {
            return;
        }

        IsBusy = true;
        OperationStatusText = "Clearing imported data...";
        await Task.Yield();

        try
        {
            await importStore.ClearAsync();
            await RefreshImportStatusAsync();
            OperationStatusText = "Imported data cleared.";

            if (OnImportCleared != null)
            {
                await OnImportCleared();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ReadFromFileAsync()
    {
        if (IsBusy || PickSourceFileAsync is null)
        {
            return;
        }

        var path = await PickSourceFileAsync();
        if (path is null)
        {
            return;
        }

        IsBusy = true;
        OperationStatusText = "Reading source from file...";
        await Task.Yield();

        try
        {
            var result = await Task.Run(() => localFileWorkflow.RegisterFromFileAsync(path));
            if (result.Succeeded)
            {
                ApplyStatus(await refreshWorkflow.GetStatusAsync());
                HasStoredSources = true;
                OperationStatusText = $"Source read from file. {LastUpdateText(result.LastUpdate)}.";
            }
            else
            {
                OperationStatusText = $"Reading file failed: {result.ErrorMessage}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ExportSourcesAsync()
    {
        if (IsBusy || PickExportFolderAsync is null)
        {
            return;
        }

        var folder = await PickExportFolderAsync();
        if (folder is null)
        {
            return;
        }

        IsBusy = true;
        OperationStatusText = "Exporting stored sources...";
        await Task.Yield();

        try
        {
            var result = await sourceExporter.ExportAllAsync(folder);
            OperationStatusText = result.Succeeded
                ? $"Exported {result.FileCount} source file(s) to the selected folder."
                : $"Export failed: {result.ErrorMessage}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshImportStatusAsync(CancellationToken cancellationToken = default)
    {
        var version = await importMetadataReader.GetImportedVersionAsync(cancellationToken);
        HasImportedData = version is not null;
        ImportStatusText = version is null
            ? "Source data not imported."
            : $"Source data imported. {LastUpdateText(version)}.";
        FooterImportStatusText = version is null
            ? "Source data not imported"
            : $"Drop Tables version: {version}";
    }

    private void ApplyStatus(DropTableSourceStatus status)
    {
        if (!status.FileExists)
        {
            SourceStatusText = "Source data not aquired.";
            return;
        }

        SourceStatusText = $"Source data ready. {LastUpdateText(status.LastUpdate)}.";
    }

    private static string LastUpdateText(string? lastUpdate)
    {
        return string.IsNullOrWhiteSpace(lastUpdate)
            ? "Drop Tables version unavailable"
            : $"Drop Tables version: {lastUpdate}";
    }

    private static string ImportCompletedText(DropTableImportResult result)
    {
        return $"Import completed. {LastUpdateText(result.SourceLastUpdate)}.";
    }

    private static string ImportStatusDetail(DropTableImportResult result)
    {
        return $"Source data imported. {LastUpdateText(result.SourceLastUpdate)}. Relics: {result.Counts.Relics}; relic rewards: {result.Counts.RelicRewards}; mission sources: {result.Counts.MissionSources}; mission rewards: {result.Counts.MissionRewards}.";
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

    /// <summary>Awaitable version for use in tests.</summary>
    public Task ExecuteTaskAsync() => execute();

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
