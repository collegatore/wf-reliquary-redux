using Avalonia.Controls;
using Avalonia.Interactivity;
using WF.Reliquary.Application.RecruitingMessage;

namespace WF.Reliquary.Presentation;

public sealed partial class RecruitingMessageDialog : Window
{
    // Required by Avalonia's XAML loader.
    public RecruitingMessageDialog() : this(RecruitingMessageTemplate.Default, string.Empty) { }

    private RecruitingMessageDialog(RecruitingMessageTemplate template, string relicName)
    {
        InitializeComponent();
        var vm = new RecruitingMessageDialogViewModel(template, relicName);
        DataContext = vm;
        CloseButton.Click += (_, _) => Close();
    }

    /// <summary>
    /// Shows the dialog as a modal child of <paramref name="owner"/>.
    /// <paramref name="copyToClipboard"/> receives the preview text when the user clicks Copy.
    /// </summary>
    public static Task ShowAsync(
        Window owner,
        RecruitingMessageTemplate template,
        string relicName,
        Func<string, Task> copyToClipboard)
    {
        var dialog = new RecruitingMessageDialog(template, relicName);
        if (dialog.DataContext is RecruitingMessageDialogViewModel vm)
            vm.CopyToClipboard = copyToClipboard;

        return dialog.ShowDialog(owner);
    }
}
