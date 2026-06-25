using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Lfg;

namespace WF.Reliquary.Presentation;

public sealed class LfgSelectorViewModel : ObservableObject
{
    private LfgOption? selectedOption;

    public LfgSelectorViewModel(LfgOptionGroup group, Action onSelectionChanged)
    {
        Label = group.Placeholder;
        Options = group.Options.ToList();
        selectedOption = group.Options.FirstOrDefault(o => o.Value == group.DefaultValue)
            ?? group.Options.FirstOrDefault();
        OnSelectionChanged = onSelectionChanged;
    }

    public string Label { get; }
    public IReadOnlyList<LfgOption> Options { get; }
    private Action OnSelectionChanged { get; }

    public LfgOption? SelectedOption
    {
        get => selectedOption;
        set
        {
            if (SetProperty(ref selectedOption, value))
                OnSelectionChanged();
        }
    }

    public string SelectedValue => selectedOption?.Value ?? string.Empty;
}

public sealed class LfgMessageDialogViewModel : ObservableObject
{
    private readonly LfgMessageTemplate template;
    private readonly string relicName;
    private IReadOnlyList<LfgSelectorViewModel> allSelectors = [];
    private string preview = string.Empty;
    private Func<string, Task>? copyToClipboard;

    public LfgMessageDialogViewModel(LfgMessageTemplate template, string relicName)
    {
        this.template = template;
        this.relicName = relicName;

        var all = template.Groups.Select(g => new LfgSelectorViewModel(g, UpdatePreview)).ToList();
        allSelectors = all;
        Selectors = all.Where(s => s.Options.Count > 1).ToList();

        CopyCommand = new AsyncRelayCommand(CopyAsync, () => copyToClipboard is not null);

        UpdatePreview();
    }

    public string Pattern => template.Pattern;

    public IReadOnlyList<LfgSelectorViewModel> Selectors { get; }

    public string Preview
    {
        get => preview;
        private set => SetProperty(ref preview, value);
    }

    public AsyncRelayCommand CopyCommand { get; }

    /// <summary>Injected by MainWindow — places text on the clipboard.</summary>
    public Func<string, Task>? CopyToClipboard
    {
        set
        {
            copyToClipboard = value;
            CopyCommand.NotifyCanExecuteChanged();
        }
    }

    private void UpdatePreview()
    {
        var selectedValues = allSelectors.ToDictionary(s => s.Label, s => s.SelectedValue);
        Preview = LfgMessageBuilder.Build(template.Pattern, relicName, selectedValues);
    }

    private async Task CopyAsync()
    {
        if (copyToClipboard is not null)
            await copyToClipboard(Preview);
    }
}
