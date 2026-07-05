using Avalonia.Controls;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Presentation;

public sealed partial class FirstStartSetupWindow : Window
{
    private bool allowClose;

    // Required by Avalonia's XAML loader.
    public FirstStartSetupWindow() => InitializeComponent();

    public FirstStartSetupWindow(FirstStartSetupViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // The native close (X) control is inert for the modal's entire lifetime
        // (Consent, Running, and Done) — Close is only reachable via the in-window
        // Close command, which routes through RequestClose below.
        viewModel.RequestClose = () =>
        {
            allowClose = true;
            Close();
        };

        Closing += (_, e) =>
        {
            if (!allowClose)
            {
                e.Cancel = true;
            }
        };
    }

    /// <summary>
    /// Loads the effective source URL, then shows the modal and awaits its full
    /// lifecycle (consent, run, done, close).
    /// </summary>
    public static async Task ShowAsync(
        Window owner,
        IDropTableSourceRefreshWorkflow refreshWorkflow,
        IDropTableImportWorkflow importWorkflow,
        IDropTableSourceSettingsStore sourceSettingsStore,
        IErrorReporter? errorReporter = null,
        Func<Task>? onDownloadSucceeded = null,
        Func<Task>? onImportSucceeded = null)
    {
        var viewModel = new FirstStartSetupViewModel(refreshWorkflow, importWorkflow, sourceSettingsStore, errorReporter)
        {
            OnDownloadSucceeded = onDownloadSucceeded,
            OnImportSucceeded = onImportSucceeded,
        };
        await viewModel.LoadEffectiveUrlAsync();

        var window = new FirstStartSetupWindow(viewModel);
        await window.ShowDialog(owner);
    }
}
