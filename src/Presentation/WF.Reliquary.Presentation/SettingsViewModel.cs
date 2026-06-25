using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApp = Avalonia.Application;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Presentation;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly IUserSettingsStore settingsStore;

    // Last-saved values — used for unsaved-change detection
    private string savedSourceUrl;
    private FontSizeOption savedFontSize;
    private double? savedRelicsPanelHeight;

    // Current editable values
    private string sourceUrlText;
    private FontSizeOption selectedFontSize;
    private double? relicsPanelHeight;

    private string urlValidationError = string.Empty;

    public SettingsViewModel(IUserSettingsStore settingsStore, UserSettings initialSettings)
    {
        this.settingsStore = settingsStore;

        savedSourceUrl = initialSettings.SourceUrl.ToString();
        savedFontSize = ToPresentation(initialSettings.FontSize);
        savedRelicsPanelHeight = initialSettings.RelicsPanelHeight;

        sourceUrlText = savedSourceUrl;
        selectedFontSize = savedFontSize;
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

    public FontSizeOption SelectedFontSize
    {
        get => selectedFontSize;
        set
        {
            if (SetProperty(ref selectedFontSize, value))
            {
                ApplyFontSize(value);   // live preview — reverted by Back if not saved
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
            {
                NotifyHasUnsavedChanges();
            }
        }
    }

    public bool HasUnsavedChanges =>
        !string.Equals(sourceUrlText, savedSourceUrl, StringComparison.Ordinal) ||
        selectedFontSize != savedFontSize ||
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

    /// <summary>Records the current live splitter position so an explicit Save
    /// persists it. Called by the shell before navigating to Settings.</summary>
    public void SyncRelicsPanelHeight(double? currentHeight)
    {
        // Sync the saved baseline to the live position first so the splitter
        // never appears as an "unsaved change" in the leave-guard. The user
        // cannot adjust the splitter from within Settings, so any delta between
        // the live position and the last-persisted value is not a user edit.
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

        var newSettings = new UserSettings(
            SourceUrl: sourceUrl,
            WindowWidth: 0,   // will be refreshed from window on close; safe to leave 0 here
            WindowHeight: 0,
            FontSize: ToApplication(SelectedFontSize),
            RelicsPanelHeight: RelicsPanelHeight,
            LfgTemplate: GetCurrentLfgTemplate?.Invoke() ?? Application.Lfg.LfgMessageTemplate.Default);

        await settingsStore.SaveAsync(newSettings);

        // Update saved baselines
        savedSourceUrl = SourceUrlText;
        savedFontSize = SelectedFontSize;
        savedRelicsPanelHeight = RelicsPanelHeight;

        // Apply font size immediately
        ApplyFontSize(SelectedFontSize);

        OnSaved?.Invoke(newSettings);
        NotifyHasUnsavedChanges();
    }

    public async Task BackAsync()
    {
        if (!await TryDiscardAsync()) return;

        if (OnNavigateBack is not null)
        {
            await OnNavigateBack();
        }
    }

    /// <summary>
    /// If there are unsaved changes, shows the confirmation dialog. Returns
    /// <see langword="true"/> if the caller may proceed with navigation (no
    /// changes, or the user confirmed discard). Returns <see langword="false"/>
    /// if the user cancelled. On discard, all in-memory edits — including the
    /// live font-size preview — are reverted.
    /// </summary>
    public async Task<bool> TryDiscardAsync()
    {
        if (!HasUnsavedChanges) return true;

        var confirmed = ConfirmDiscard is not null && await ConfirmDiscard();
        if (!confirmed) return false;

        // Roll back in-memory edits and undo the live font-size preview.
        sourceUrlText = savedSourceUrl;
        selectedFontSize = savedFontSize;
        relicsPanelHeight = savedRelicsPanelHeight;
        urlValidationError = string.Empty;
        ApplyFontSize(savedFontSize);
        OnPropertyChanged(nameof(SourceUrlText));
        OnPropertyChanged(nameof(SelectedFontSize));
        OnPropertyChanged(nameof(RelicsPanelHeight));
        NotifyHasUnsavedChanges();
        OnPropertyChanged(nameof(UrlValidationError));

        return true;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static void ApplyFontSize(FontSizeOption option)
    {
        if (AvaloniaApp.Current?.Resources is not { } resources)
        {
            return;
        }

        var size = option switch
        {
            FontSizeOption.Small => resources["FontSizeSmall"],
            FontSizeOption.Large => resources["FontSizeLarge"],
            _ => resources["FontSizeMedium"],
        };

        if (size is double d)
        {
            resources["ContentFontSize"] = d;

            // Card widths scale with the font so the per-line character budget stays
            // roughly constant across sizes. Each card's current width is its Medium
            // anchor; scale by the ratio to the Medium font size.
            if (resources["FontSizeMedium"] is double medium && medium > 0)
            {
                var ratio = d / medium;
                resources["RelicCardWidth"] = 300 * ratio;
                resources["MissionCardWidth"] = 300 * ratio;
            }
        }
    }

    internal static void ApplyFontSize(UserFontSize fontSize)
    {
        ApplyFontSize(ToPresentation(fontSize));
    }

    private static FontSizeOption ToPresentation(UserFontSize fs) => fs switch
    {
        UserFontSize.Small => FontSizeOption.Small,
        UserFontSize.Large => FontSizeOption.Large,
        _ => FontSizeOption.Medium,
    };

    private static UserFontSize ToApplication(FontSizeOption fs) => fs switch
    {
        FontSizeOption.Small => UserFontSize.Small,
        FontSizeOption.Large => UserFontSize.Large,
        _ => UserFontSize.Medium,
    };

    private void NotifyHasUnsavedChanges()
    {
        OnPropertyChanged(nameof(HasUnsavedChanges));
        SaveCommand.NotifyCanExecuteChanged();
    }
}
