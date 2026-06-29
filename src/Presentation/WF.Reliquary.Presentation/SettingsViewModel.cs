using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApp = Avalonia.Application;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Presentation;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly IUserSettingsStore settingsStore;

    private const double SmallPt = 11.0;
    private const double MediumPt = 14.0;
    private const double LargePt = 18.0;

    // Last-saved values — used for unsaved-change detection (font size excluded: auto-saved)
    private string savedSourceUrl;
    private double? savedRelicsPanelHeight;
    private RelicsMissionsOrientation savedOrientation;
    private double? savedSideBySideRelicsWidth;

    // Current editable values
    private string sourceUrlText;
    private double fontSizePt;          // sole font-size state; SelectedFontSize is derived
    private double? relicsPanelHeight;
    private RelicsMissionsOrientation orientation;
    private double? sideBySideRelicsWidth;

    private string urlValidationError = string.Empty;

    public SettingsViewModel(IUserSettingsStore settingsStore, UserSettings initialSettings)
    {
        this.settingsStore = settingsStore;

        savedSourceUrl = initialSettings.SourceUrl.ToString();
        savedRelicsPanelHeight = initialSettings.RelicsPanelHeight;
        savedOrientation = initialSettings.Orientation;
        savedSideBySideRelicsWidth = initialSettings.SideBySideRelicsWidth;

        sourceUrlText = savedSourceUrl;
        fontSizePt = initialSettings.FontSizePt;
        relicsPanelHeight = savedRelicsPanelHeight;
        orientation = savedOrientation;
        sideBySideRelicsWidth = savedSideBySideRelicsWidth;

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

    /// <summary>
    /// Applies the chosen Relics/Missions layout orientation to the live shell so the
    /// switch takes effect immediately (like font size). Wired by the shell.
    /// </summary>
    public Action<RelicsMissionsOrientation>? ApplyOrientation { get; set; }

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

    /// <summary>
    /// The effective font size in points. Always visible; drives the radio selection.
    /// Setting it auto-selects the matching preset radio, or Custom if no preset matches.
    /// </summary>
    public double EffectiveFontSizePt
    {
        get => fontSizePt;
        set
        {
            var pt = Math.Clamp(value, 8.0, 32.0);
            if (fontSizePt == pt) return;

            var prevOption = SelectedFontSize;
            fontSizePt = pt;

            OnPropertyChanged();
            if (SelectedFontSize != prevOption)
                OnPropertyChanged(nameof(SelectedFontSize));

            ApplyFontSize(pt);
            _ = SaveFontSizeNow();
        }
    }

    /// <summary>
    /// Derived from <see cref="EffectiveFontSizePt"/>: Small/Medium/Large when the value
    /// matches a preset, Custom otherwise. Setting a preset updates <see cref="EffectiveFontSizePt"/>.
    /// </summary>
    public FontSizeOption SelectedFontSize
    {
        get => fontSizePt == SmallPt ? FontSizeOption.Small
             : fontSizePt == LargePt ? FontSizeOption.Large
             : fontSizePt == MediumPt ? FontSizeOption.Medium
             : FontSizeOption.Custom;
        set
        {
            var pt = value switch
            {
                FontSizeOption.Small => SmallPt,
                FontSizeOption.Large => LargePt,
                FontSizeOption.Custom => fontSizePt,   // clicking Custom is a no-op
                _ => MediumPt,
            };
            EffectiveFontSizePt = pt;
        }
    }

    /// <summary>
    /// The Relics/Missions layout orientation. An appearance setting that — like font
    /// size — applies live to the shell and auto-saves on selection, so it stays out of
    /// the unsaved-changes guard.
    /// </summary>
    public RelicsMissionsOrientation SelectedOrientation
    {
        get => orientation;
        set
        {
            if (orientation == value) return;

            orientation = value;
            savedOrientation = value;   // auto-saved; keep discard baseline in sync
            OnPropertyChanged();

            ApplyOrientation?.Invoke(value);
            _ = SaveOrientationNow();
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

    public void SyncOrientation(RelicsMissionsOrientation currentOrientation, double? currentRelicsWidth)
    {
        savedOrientation = currentOrientation;
        orientation = currentOrientation;
        savedSideBySideRelicsWidth = currentRelicsWidth;
        sideBySideRelicsWidth = currentRelicsWidth;
        OnPropertyChanged(nameof(SelectedOrientation));
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
            WindowWidth: 0,
            WindowHeight: 0,
            FontSizePt: fontSizePt,
            RelicsPanelHeight: RelicsPanelHeight,
            LfgTemplate: GetCurrentLfgTemplate?.Invoke() ?? Application.Lfg.LfgMessageTemplate.Default,
            Orientation: orientation,
            SideBySideRelicsWidth: sideBySideRelicsWidth);

        await settingsStore.SaveAsync(newSettings);

        savedSourceUrl = SourceUrlText;
        savedRelicsPanelHeight = RelicsPanelHeight;
        savedOrientation = orientation;
        savedSideBySideRelicsWidth = sideBySideRelicsWidth;

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
        orientation = savedOrientation;
        sideBySideRelicsWidth = savedSideBySideRelicsWidth;
        urlValidationError = string.Empty;
        OnPropertyChanged(nameof(SourceUrlText));
        OnPropertyChanged(nameof(RelicsPanelHeight));
        NotifyHasUnsavedChanges();
        OnPropertyChanged(nameof(UrlValidationError));

        return true;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task SaveFontSizeNow()
    {
        var current = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(current with { FontSizePt = fontSizePt });
    }

    private async Task SaveOrientationNow()
    {
        var current = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(current with { Orientation = orientation });
    }

    internal static void ApplyFontSize(double pt)
    {
        if (AvaloniaApp.Current?.Resources is not { } resources) return;

        resources["ContentFontSize"] = pt;

        if (resources["FontSizeMedium"] is double medium && medium > 0)
        {
            var ratio = pt / medium;
            resources["RelicCardWidth"] = 300 * ratio;
            resources["MissionCardWidth"] = 300 * ratio;
        }
    }

    private void NotifyHasUnsavedChanges()
    {
        OnPropertyChanged(nameof(HasUnsavedChanges));
        SaveCommand.NotifyCanExecuteChanged();
    }
}
