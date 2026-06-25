using Avalonia.Controls;
using Avalonia.Interactivity;
using WF.Reliquary.Application.Lfg;

namespace WF.Reliquary.Presentation;

public sealed partial class LfgMessageDialog : Window
{
    // Required by Avalonia's XAML loader.
    public LfgMessageDialog() : this(LfgMessageTemplate.Default, string.Empty) { }

    private LfgMessageDialog(LfgMessageTemplate template, string relicName)
    {
        InitializeComponent();
        var vm = new LfgMessageDialogViewModel(template, relicName);
        DataContext = vm;
        CloseButton.Click += (_, _) => Close();
    }

    /// <summary>
    /// Shows the dialog as a modal child of <paramref name="owner"/>.
    /// <paramref name="copyToClipboard"/> receives the preview text when the user clicks Copy.
    /// </summary>
    public static Task ShowAsync(
        Window owner,
        LfgMessageTemplate template,
        string relicName,
        Func<string, Task> copyToClipboard)
    {
        var dialog = new LfgMessageDialog(template, relicName);
        if (dialog.DataContext is LfgMessageDialogViewModel vm)
            vm.CopyToClipboard = copyToClipboard;

        return dialog.ShowDialog(owner);
    }
}
