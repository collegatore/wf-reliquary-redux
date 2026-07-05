using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Presentation;

public enum FirstStartSetupStage
{
    Consent,
    Running,
    Done,
}

public enum FirstStartSetupStepState
{
    Pending,
    Running,
    Succeeded,
    Failed,
    Skipped,
}

/// <summary>
/// Drives the first-start automated setup consent modal: explains the download,
/// shows the effective source URL, and — only after explicit Approval — chains the
/// existing drop-table download and import workflows, reporting each step's live
/// status. See <c>openspec/changes/automated-first-start-setup</c> for the full design.
/// </summary>
public sealed partial class FirstStartSetupViewModel : ObservableObject
{
    private readonly IDropTableSourceRefreshWorkflow refreshWorkflow;
    private readonly IDropTableImportWorkflow importWorkflow;
    private readonly IDropTableSourceSettingsStore sourceSettingsStore;
    private readonly IErrorReporter errorReporter;

    [ObservableProperty]
    private FirstStartSetupStage stage = FirstStartSetupStage.Consent;

    [ObservableProperty]
    private string sourceUrlText = string.Empty;

    [ObservableProperty]
    private FirstStartSetupStepState downloadState = FirstStartSetupStepState.Pending;

    [ObservableProperty]
    private string? downloadMessage;

    [ObservableProperty]
    private FirstStartSetupStepState importState = FirstStartSetupStepState.Pending;

    [ObservableProperty]
    private string? importMessage;

    public FirstStartSetupViewModel(
        IDropTableSourceRefreshWorkflow refreshWorkflow,
        IDropTableImportWorkflow importWorkflow,
        IDropTableSourceSettingsStore sourceSettingsStore,
        IErrorReporter? errorReporter = null)
    {
        this.refreshWorkflow = refreshWorkflow;
        this.importWorkflow = importWorkflow;
        this.sourceSettingsStore = sourceSettingsStore;
        this.errorReporter = errorReporter ?? NoOpErrorReporter.Instance;

        ApproveCommand = new AsyncRelayCommand(ApproveAsync, () => Stage == FirstStartSetupStage.Consent);
        DenyCommand = new RelayCommand(Deny, () => Stage == FirstStartSetupStage.Consent);
        CloseCommand = new RelayCommand(RequestCloseNow, () => Stage == FirstStartSetupStage.Done);
    }

    public IAsyncRelayCommand ApproveCommand { get; }

    public IRelayCommand DenyCommand { get; }

    public IRelayCommand CloseCommand { get; }

    /// <summary>Invoked when the modal should actually close (Deny, or Close once done).</summary>
    public Action? RequestClose { get; set; }

    /// <summary>
    /// Invoked after a successful download, before the import step starts. Wired by
    /// <c>MainWindow</c> to reload the Drop Tables tab's own acquisition/import
    /// display (<c>MainWindowViewModel.LoadStatusAsync</c>) so the newly saved source
    /// shows up there, exactly as a manual "Download from URL" click would.
    /// </summary>
    public Func<Task>? OnDownloadSucceeded { get; set; }

    /// <summary>
    /// Invoked after a successful import. Wired by <c>MainWindow</c> to the same
    /// reload cascade the manual Import button already triggers (Prime Gear, Wishlist
    /// Relics, Wishlist Farming Missions), so this automated path doesn't leave those
    /// views showing stale empty state.
    /// </summary>
    public Func<Task>? OnImportSucceeded { get; set; }

    public string ExecutionPhaseHeader => FormatExecutionHeader(Stage, DownloadState, ImportState);

    public string DownloadStatusText => FormatStatus("Download", DownloadState, DownloadMessage);

    public string ImportStatusText => FormatStatus("Import", ImportState, ImportMessage);

    private static string FormatExecutionHeader(
        FirstStartSetupStage stage,
        FirstStartSetupStepState downloadState,
        FirstStartSetupStepState importState)
    {
        if (stage != FirstStartSetupStage.Done)
        {
            return "Performing initial setup";
        }

        if (downloadState == FirstStartSetupStepState.Failed || importState == FirstStartSetupStepState.Failed)
        {
            return "Initial setup failed. Please verify that the download URL leads to a valid Warframe PC Drops page.";
        }

        return "Initial setup successfully completed";
    }

    private static string FormatStatus(string stepName, FirstStartSetupStepState state, string? message)
    {
        return state switch
        {
            FirstStartSetupStepState.Pending => $"{stepName}: pending",
            FirstStartSetupStepState.Running => $"{stepName}: in progress...",
            FirstStartSetupStepState.Succeeded when !string.IsNullOrWhiteSpace(message) => $"{stepName}: succeeded ({message})",
            FirstStartSetupStepState.Succeeded => $"{stepName}: succeeded",
            FirstStartSetupStepState.Failed => $"{stepName}: failed - {message}",
            FirstStartSetupStepState.Skipped => $"{stepName}: skipped",
            _ => stepName,
        };
    }

    /// <summary>
    /// Loads the currently configured source URL for display. Must complete before the
    /// Consent panel is shown, so the user sees the URL that will actually be requested.
    /// </summary>
    public async Task LoadEffectiveUrlAsync(CancellationToken cancellationToken = default)
    {
        var settings = await sourceSettingsStore.LoadAsync(cancellationToken);
        SourceUrlText = settings.SourceUrl.ToString();
    }

    partial void OnStageChanged(FirstStartSetupStage value)
    {
        ApproveCommand.NotifyCanExecuteChanged();
        DenyCommand.NotifyCanExecuteChanged();
        CloseCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(ExecutionPhaseHeader));
    }

    partial void OnDownloadStateChanged(FirstStartSetupStepState value)
    {
        OnPropertyChanged(nameof(DownloadStatusText));
        OnPropertyChanged(nameof(ExecutionPhaseHeader));
    }

    partial void OnDownloadMessageChanged(string? value) => OnPropertyChanged(nameof(DownloadStatusText));

    partial void OnImportStateChanged(FirstStartSetupStepState value)
    {
        OnPropertyChanged(nameof(ImportStatusText));
        OnPropertyChanged(nameof(ExecutionPhaseHeader));
    }

    partial void OnImportMessageChanged(string? value) => OnPropertyChanged(nameof(ImportStatusText));

    private async Task ApproveAsync()
    {
        Stage = FirstStartSetupStage.Running;
        DownloadState = FirstStartSetupStepState.Running;

        DropTableRefreshResult refreshResult;
        try
        {
            refreshResult = await refreshWorkflow.RefreshAsync();
        }
        catch (Exception ex)
        {
            DownloadState = FirstStartSetupStepState.Failed;
            DownloadMessage = ex.Message;
            ImportState = FirstStartSetupStepState.Skipped;
            errorReporter.Report("Download failed.", ex);
            Stage = FirstStartSetupStage.Done;
            return;
        }

        if (!refreshResult.Succeeded)
        {
            DownloadState = FirstStartSetupStepState.Failed;
            DownloadMessage = refreshResult.ErrorMessage;
            ImportState = FirstStartSetupStepState.Skipped;
            errorReporter.Report($"Download failed: {refreshResult.ErrorMessage}");
            Stage = FirstStartSetupStage.Done;
            return;
        }

        DownloadState = FirstStartSetupStepState.Succeeded;
        DownloadMessage = refreshResult.Status.LastUpdate;

        if (OnDownloadSucceeded is not null)
        {
            await OnDownloadSucceeded();
        }

        ImportState = FirstStartSetupStepState.Running;

        try
        {
            var importResult = await importWorkflow.ImportAsync(refreshResult.Id!.Value);
            if (importResult.Succeeded)
            {
                ImportState = FirstStartSetupStepState.Succeeded;

                if (OnImportSucceeded is not null)
                {
                    await OnImportSucceeded();
                }
            }
            else
            {
                ImportState = FirstStartSetupStepState.Failed;
                ImportMessage = importResult.ErrorMessage;
                errorReporter.Report($"Import failed: {importResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            ImportState = FirstStartSetupStepState.Failed;
            ImportMessage = ex.Message;
            errorReporter.Report("Import failed.", ex);
        }

        Stage = FirstStartSetupStage.Done;
    }

    private void Deny() => RequestClose?.Invoke();

    private void RequestCloseNow() => RequestClose?.Invoke();
}
