using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Presentation;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IDropTableSourceRefreshWorkflow refreshWorkflow;
    private readonly IDropTableImportWorkflow importWorkflow;
    private readonly IDropTableLocalFileSourceWorkflow localFileWorkflow;
    private readonly IDropTableSourceExporter sourceExporter;
    private readonly IImportMetadataReader importMetadataReader;
    private readonly IDropTableImportStore importStore;
    private readonly IErrorReporter errorReporter;
    private readonly BusyScope busy;

    [ObservableProperty]
    private string sourceStatusText = "Checking stored source...";

    [ObservableProperty]
    private string operationStatusText = string.Empty;

    [ObservableProperty]
    private string importStatusText = "No source imported yet.";

    [ObservableProperty]
    private string footerImportStatusText = string.Empty;

    [ObservableProperty]
    private bool hasStoredSources;

    [ObservableProperty]
    private bool hasImportedData;

    public MainWindowViewModel(
        IDropTableSourceRefreshWorkflow refreshWorkflow,
        IDropTableImportWorkflow importWorkflow,
        IImportMetadataReader importMetadataReader,
        IDropTableLocalFileSourceWorkflow localFileWorkflow,
        IDropTableSourceExporter sourceExporter,
        IDropTableImportStore importStore,
        IErrorReporter? errorReporter = null)
    {
        this.refreshWorkflow = refreshWorkflow;
        this.importWorkflow = importWorkflow;
        this.localFileWorkflow = localFileWorkflow;
        this.sourceExporter = sourceExporter;
        this.importMetadataReader = importMetadataReader;
        this.importStore = importStore;
        this.errorReporter = errorReporter ?? NoOpErrorReporter.Instance;

        DownloadCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        ImportCommand = new AsyncRelayCommand(ImportAsync, () => !IsBusy && HasStoredSources);
        ReadFromFileCommand = new AsyncRelayCommand(ReadFromFileAsync, () => !IsBusy);
        ExportSourcesCommand = new AsyncRelayCommand(ExportSourcesAsync, () => !IsBusy && HasStoredSources);
        ClearImportCommand = new AsyncRelayCommand(ClearImportAsync, () => HasImportedData && !IsBusy);

        busy = new BusyScope(_ =>
        {
            OnPropertyChanged(nameof(IsBusy));
            DownloadCommand.NotifyCanExecuteChanged();
            ImportCommand.NotifyCanExecuteChanged();
            ReadFromFileCommand.NotifyCanExecuteChanged();
            ExportSourcesCommand.NotifyCanExecuteChanged();
            ClearImportCommand.NotifyCanExecuteChanged();
        });
    }

    public Func<Task>? OnImportSucceeded { get; set; }

    public Func<Task>? OnImportCleared { get; set; }

    public Func<Task<string?>>? PickSourceFileAsync { get; set; }

    public Func<Task<string?>>? PickExportFolderAsync { get; set; }

    public Func<Task<bool>>? ConfirmClear { get; set; }

    public string Title => ApplicationInfo.GetStartupSummary();

    /// <summary>True while a source operation runs; gates commands and the footer spinner.</summary>
    public bool IsBusy => busy.IsBusy;

    public IAsyncRelayCommand DownloadCommand { get; }

    public IAsyncRelayCommand ImportCommand { get; }

    public IAsyncRelayCommand ReadFromFileCommand { get; }

    public IAsyncRelayCommand ExportSourcesCommand { get; }

    public IAsyncRelayCommand ClearImportCommand { get; }

    public async Task LoadStatusAsync(CancellationToken cancellationToken = default)
    {
        ApplyStatus(await refreshWorkflow.GetStatusAsync(cancellationToken));
        await RefreshImportStatusAsync(cancellationToken);
        HasStoredSources = await refreshWorkflow.HasStoredSourcesAsync(cancellationToken);
    }

    public async Task RefreshAsync()
    {
        using var scope = busy.TryEnter();
        if (scope is null) return;

        OperationStatusText = "Downloading source from URL...";

        try
        {
            var result = await refreshWorkflow.RefreshAsync();
            ApplyStatus(result.Status);

            if (result.Succeeded)
            {
                OperationStatusText = $"Source downloaded. {LastUpdateText(result.Status.LastUpdate)}.";
                HasStoredSources = true;
            }
            else
            {
                OperationStatusText = string.Empty;
                errorReporter.Report($"Download failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            OperationStatusText = string.Empty;
            errorReporter.Report("Download failed.", ex);
        }
    }

    public async Task ImportAsync()
    {
        using var scope = busy.TryEnter();
        if (scope is null) return;

        OperationStatusText = "Importing stored source...";

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
                OperationStatusText = string.Empty;
                errorReporter.Report($"Import failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            OperationStatusText = string.Empty;
            errorReporter.Report("Import failed.", ex);
        }
    }

    public async Task ClearImportAsync()
    {
        if (!HasImportedData) return;

        if (ConfirmClear != null && !await ConfirmClear())
        {
            return;
        }

        using var scope = busy.TryEnter();
        if (scope is null) return;

        OperationStatusText = "Clearing imported data...";

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
        catch (Exception ex)
        {
            OperationStatusText = string.Empty;
            errorReporter.Report("Clearing imported data failed.", ex);
        }
    }

    public async Task ReadFromFileAsync()
    {
        if (PickSourceFileAsync is null) return;

        var path = await PickSourceFileAsync();
        if (path is null)
        {
            return;
        }

        using var scope = busy.TryEnter();
        if (scope is null) return;

        OperationStatusText = "Reading source from file...";

        try
        {
            var result = await localFileWorkflow.RegisterFromFileAsync(path);
            if (result.Succeeded)
            {
                ApplyStatus(await refreshWorkflow.GetStatusAsync());
                HasStoredSources = true;
                OperationStatusText = $"Source read from file. {LastUpdateText(result.LastUpdate)}.";
            }
            else
            {
                OperationStatusText = string.Empty;
                errorReporter.Report($"Reading file failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            OperationStatusText = string.Empty;
            errorReporter.Report("Reading file failed.", ex);
        }
    }

    public async Task ExportSourcesAsync()
    {
        if (PickExportFolderAsync is null) return;

        var folder = await PickExportFolderAsync();
        if (folder is null)
        {
            return;
        }

        using var scope = busy.TryEnter();
        if (scope is null) return;

        OperationStatusText = "Exporting stored sources...";

        try
        {
            var result = await sourceExporter.ExportAllAsync(folder);
            if (result.Succeeded)
            {
                OperationStatusText = $"Exported {result.FileCount} source file(s) to the selected folder.";
            }
            else
            {
                OperationStatusText = string.Empty;
                errorReporter.Report($"Export failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            OperationStatusText = string.Empty;
            errorReporter.Report("Export failed.", ex);
        }
    }

    partial void OnHasStoredSourcesChanged(bool value)
    {
        ImportCommand.NotifyCanExecuteChanged();
        ExportSourcesCommand.NotifyCanExecuteChanged();
    }

    partial void OnHasImportedDataChanged(bool value)
    {
        ClearImportCommand.NotifyCanExecuteChanged();
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
}
