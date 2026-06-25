using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Lfg;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Editable option within a group in the template editor.
/// </summary>
public sealed class LfgOptionEditorViewModel : ObservableObject
{
    private string descriptor;
    private string value;

    public LfgOptionEditorViewModel(string descriptor, string value, Action onChange)
    {
        this.descriptor = descriptor;
        this.value = value;
        OnChange = onChange;
    }

    private Action OnChange { get; }

    public string Descriptor
    {
        get => descriptor;
        set { if (SetProperty(ref descriptor, value)) OnChange(); }
    }

    public string Value
    {
        get => this.value;
        set { if (SetProperty(ref this.value, value)) OnChange(); }
    }

    public LfgOption ToModel() => new(Descriptor, Value);
}

/// <summary>
/// Editable option group in the template editor.
/// </summary>
public sealed class LfgGroupEditorViewModel : ObservableObject
{
    private string defaultValue;

    public LfgGroupEditorViewModel(LfgOptionGroup group, Action onChange)
    {
        Placeholder = group.Placeholder;
        defaultValue = group.DefaultValue;
        OnChange = onChange;

        void NotifyAndChange()
        {
            OnPropertyChanged(nameof(SelectedDefaultOption));
            onChange();
        }

        Options = new ObservableCollection<LfgOptionEditorViewModel>(
            group.Options.Select(o => new LfgOptionEditorViewModel(o.Descriptor, o.Value, NotifyAndChange)));

        AddOptionCommand = new RelayCommand(() =>
        {
            Options.Add(new LfgOptionEditorViewModel(string.Empty, string.Empty, NotifyAndChange));
            NotifyAndChange();
        });

        RemoveOptionCommand = new RelayCommand<LfgOptionEditorViewModel>(opt =>
        {
            if (opt is not null) Options.Remove(opt);
            NotifyAndChange();
        });
    }

    private Action OnChange { get; }

    public string Placeholder { get; }
    public ObservableCollection<LfgOptionEditorViewModel> Options { get; }
    public RelayCommand AddOptionCommand { get; }
    public RelayCommand<LfgOptionEditorViewModel> RemoveOptionCommand { get; }

    public string DefaultValue
    {
        get => defaultValue;
        set { if (SetProperty(ref defaultValue, value)) OnChange(); }
    }

    public LfgOptionEditorViewModel? SelectedDefaultOption
    {
        get => Options.FirstOrDefault(o => o.Value == defaultValue);
        set
        {
            // Avalonia may write null during ComboBox binding initialization or
            // when ItemsSource changes; treat as a no-op to avoid spurious dirty state.
            if (value is null) return;
            var newValue = value.Value;
            if (defaultValue != newValue)
            {
                defaultValue = newValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DefaultValue));
                OnChange();
            }
        }
    }

    public bool IsEmpty => Options.Count == 0;

    public LfgOptionGroup ToModel() => new(
        Placeholder,
        Options.Select(o => o.ToModel()).ToList(),
        DefaultValue);
}

/// <summary>
/// Settings section editor for the LFG message template.
/// Draft is persisted when the main Settings Save button is pressed.
/// Parse reconciles placeholder groups; Restore defaults resets the draft.
/// </summary>
public sealed class LfgTemplateEditorViewModel : ObservableObject
{
    private readonly IUserSettingsStore settingsStore;
    private LfgMessageTemplate savedTemplate;

    private string patternText;
    private string parseError = string.Empty;
    private string parseWarning = string.Empty;
    private bool isDirty;

    public LfgTemplateEditorViewModel(IUserSettingsStore settingsStore, LfgMessageTemplate initialTemplate)
    {
        this.settingsStore = settingsStore;
        savedTemplate = initialTemplate;
        patternText = initialTemplate.Pattern;
        Groups = new ObservableCollection<LfgGroupEditorViewModel>(
            initialTemplate.Groups.Select(g => new LfgGroupEditorViewModel(g, NotifyDirty)));

        ParseCommand = new RelayCommand(DoParse);
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        RestoreDefaultsCommand = new RelayCommand(RestoreDefaults);
    }

    public string PatternText
    {
        get => patternText;
        set
        {
            if (SetProperty(ref patternText, value))
                NotifyDirty();
        }
    }

    public string ParseError
    {
        get => parseError;
        private set => SetProperty(ref parseError, value);
    }

    public string ParseWarning
    {
        get => parseWarning;
        private set => SetProperty(ref parseWarning, value);
    }

    public ObservableCollection<LfgGroupEditorViewModel> Groups { get; }

    public RelayCommand ParseCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand RestoreDefaultsCommand { get; }

    public bool HasUnsavedChanges => isDirty;

    /// <summary>Returns the current draft as a template model.</summary>
    public LfgMessageTemplate CurrentDraftTemplate() =>
        new(patternText, Groups.Select(g => g.ToModel()).ToList());

    /// <summary>
    /// Called by the owner after the template has been persisted externally
    /// (e.g. via the main Settings Save). Updates the saved baseline.
    /// </summary>
    public void NotifySaved(LfgMessageTemplate template)
    {
        savedTemplate = template;
        isDirty = false;
        OnSaved?.Invoke(template);
        OnPropertyChanged(nameof(HasUnsavedChanges));
        SaveCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Invoked after a successful save with the persisted template.</summary>
    public Action<LfgMessageTemplate>? OnSaved { get; set; }

    /// <summary>Invoked whenever the dirty state changes. Wire to SettingsViewModel.NotifyLfgChanged.</summary>
    public Action? OnDirtyChanged { get; set; }

    private void DoParse()
    {
        var currentGroups = Groups.Select(g => g.ToModel()).ToList();
        var (ok, error, newGroups) = LfgPatternParser.Reconcile(patternText, currentGroups, savedTemplate.Groups);

        if (!ok)
        {
            ParseError = error ?? "Parse failed.";
            ParseWarning = string.Empty;
            return;
        }

        ParseError = string.Empty;

        Groups.Clear();
        foreach (var g in newGroups)
            Groups.Add(new LfgGroupEditorViewModel(g, NotifyDirty));

        var currentPlaceholders = currentGroups.Select(g => g.Placeholder).ToHashSet(StringComparer.Ordinal);
        var newEmpty = Groups.Where(g => g.IsEmpty && !currentPlaceholders.Contains(g.Placeholder))
                             .Select(g => $"'{g.Placeholder}'").ToList();
        ParseWarning = newEmpty.Count switch
        {
            0 => string.Empty,
            1 => $"A new group {newEmpty[0]} appeared. Please add options before saving.",
            _ => $"New groups appeared: {string.Join(", ", newEmpty)}. Please add options before saving.",
        };

        NotifyDirty();
    }

    private bool CanSave() => Groups.All(g => !g.IsEmpty);

    private async Task SaveAsync()
    {
        var template = CurrentDraftTemplate();
        var current = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(current with { LfgTemplate = template });
        NotifySaved(template);
    }

    private void RestoreDefaults()
    {
        patternText = LfgMessageTemplate.Default.Pattern;
        OnPropertyChanged(nameof(PatternText));
        Groups.Clear();
        foreach (var g in LfgMessageTemplate.Default.Groups)
            Groups.Add(new LfgGroupEditorViewModel(g, NotifyDirty));
        ParseError = string.Empty;
        ParseWarning = string.Empty;
        NotifyDirty();
    }

    private void NotifyDirty()
    {
        isDirty = true;
        OnPropertyChanged(nameof(HasUnsavedChanges));
        SaveCommand.NotifyCanExecuteChanged();
        OnDirtyChanged?.Invoke();
    }

}
