using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Presentation;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly IUserSettingsStore settingsStore;

    // Last-saved values — used for unsaved-change detection
    private string savedSourceUrl;
    private double? savedRelicsPanelHeight;

    // Current editable values
    private string sourceUrlText;
    private double? relicsPanelHeight;

    private string urlValidationError = string.Empty;

    public SettingsViewModel(IUserSettingsStore settingsStore, UserSettings initialSettings)
    {
        this.settingsStore = settingsStore;

        savedSourceUrl = initialSettings.SourceUrl.ToString();
        savedRelicsPanelHeight = initialSettings.RelicsPanelHeight;

        sourceUrlText = savedSourceUrl;
        relicsPanelHeight = savedRelicsPanelHeight;

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => HasUnsavedChanges);
        BackCommand = new AsyncRelayCommand(BackAsync);
        OpenLogFileCommand = new RelayCommand(() => OpenLogFile?.Invoke());
        CopyLogPathCommand = new AsyncRelayCommand(async () =>
        {
            if (CopyLogPath is not null) await CopyLogPath();
        });
    }

    // ── Delegates wired by MainWindow ──────────────────────────────────────

    /// <summary>Called after a successful save. Provides the new settings so the
    /// shell can update live state (relics panel height, source URL cache).</summary>
    public Action<UserSettings>? OnSaved { get; set; }

    /// <summary>Called when Back navigation should occur.</summary>
    public Func<Task>? OnNavigateBack { get; set; }

    /// <summary>Returns true if the user confirms discarding unsaved changes.</summary>
    public Func<Task<bool>>? ConfirmDiscard { get; set; }

    /// <summary>
    /// Opens the application log file. Injected from <c>MainWindow</c> as a
    /// delegate so <c>SettingsViewModel</c> has no dependency on Infrastructure.
    /// </summary>
    public Action? OpenLogFile { get; set; }

    /// <summary>
    /// Copies the log file path to the clipboard. Injected from <c>MainWindow</c>
    /// as an async delegate (clipboard access requires a <c>TopLevel</c> reference).
    /// </summary>
    public Func<Task>? CopyLogPath { get; set; }

    /// <summary>
    /// Returns the current LFG template so it is preserved when settings are saved here.
    /// Injected from <c>MainWindow</c> because the LFG editor manages that template separately.
    /// </summary>
    public Func<Application.Lfg.LfgMessageTemplate>? GetCurrentLfgTemplate { get; set; }

    /// <summary>Returns true when the LFG draft has unsaved changes. Injected from MainWindow.</summary>
    public Func<bool>? IsLfgTemplateDirty { get; set; }

    /// <summary>Called by the LFG editor when its draft changes, so SaveCommand re-evaluates CanExecute.</summary>
    public void NotifyLfgChanged() => NotifyHasUnsavedChanges();

    // ── Properties ─────────────────────────────────────────────────────────

    public string SourceUrlText
    {
        get => sourceUrlText;
        set
        {
            if (SetProperty(ref sourceUrlText, value))
            {
                UrlValidationError = string.Empty;
                NotifyHasUnsavedChanges();
            }
        }
    }

    public double? RelicsPanelHeight
    {
        get => relicsPanelHeight;
        private set
        {
            if (SetProperty(ref relicsPanelHeight, value))
                NotifyHasUnsavedChanges();
        }
    }

    public bool HasUnsavedChanges =>
        !string.Equals(sourceUrlText, savedSourceUrl, StringComparison.Ordinal) ||
        relicsPanelHeight != savedRelicsPanelHeight ||
        (IsLfgTemplateDirty?.Invoke() ?? false);

    public string UrlValidationError
    {
        get => urlValidationError;
        private set => SetProperty(ref urlValidationError, value);
    }

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand BackCommand { get; }
    public RelayCommand OpenLogFileCommand { get; }
    public AsyncRelayCommand CopyLogPathCommand { get; }

    // ── Called by shell to sync live splitter position ──────────────────────

    public void SyncRelicsPanelHeight(double? currentHeight)
    {
        savedRelicsPanelHeight = currentHeight;
        RelicsPanelHeight = currentHeight;
    }

    // ── Command implementations ─────────────────────────────────────────────

    public async Task SaveAsync()
    {
        if (!Uri.TryCreate(SourceUrlText, UriKind.Absolute, out var sourceUrl))
        {
            UrlValidationError = "Source URL must be an absolute URL.";
            return;
        }

        UrlValidationError = string.Empty;

        var current = await settingsStore.LoadAsync();
        var newSettings = current with
        {
            SourceUrl = sourceUrl,
            RelicsPanelHeight = RelicsPanelHeight,
            LfgTemplate = GetCurrentLfgTemplate?.Invoke() ?? Application.Lfg.LfgMessageTemplate.Default,
        };

        await settingsStore.SaveAsync(newSettings);

        savedSourceUrl = SourceUrlText;
        savedRelicsPanelHeight = RelicsPanelHeight;

        OnSaved?.Invoke(newSettings);
        NotifyHasUnsavedChanges();
    }

    public async Task BackAsync()
    {
        if (!await TryDiscardAsync()) return;
        if (OnNavigateBack is not null) await OnNavigateBack();
    }

    public async Task<bool> TryDiscardAsync()
    {
        if (!HasUnsavedChanges) return true;

        var confirmed = ConfirmDiscard is not null && await ConfirmDiscard();
        if (!confirmed) return false;

        sourceUrlText = savedSourceUrl;
        relicsPanelHeight = savedRelicsPanelHeight;
        urlValidationError = string.Empty;
        OnPropertyChanged(nameof(SourceUrlText));
        OnPropertyChanged(nameof(RelicsPanelHeight));
        NotifyHasUnsavedChanges();
        OnPropertyChanged(nameof(UrlValidationError));

        return true;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void NotifyHasUnsavedChanges()
    {
        OnPropertyChanged(nameof(HasUnsavedChanges));
        SaveCommand.NotifyCanExecuteChanged();
    }
}
