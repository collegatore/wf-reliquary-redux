using AsyncAwaitBestPractices;
using Avalonia.Controls;
using Avalonia.Threading;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Real implementation of <see cref="IErrorReporter"/>. On each report:
/// 1. Appends the full detail to the application log file (fire-and-forget, swallowed).
/// 2. Posts to the UI thread: sets the footer status text and adds an
///    <see cref="ErrorToastCard"/> to the toast panel.
///
/// Each step is defensive — a failure in one does not prevent the others.
/// May be called from any thread.
/// </summary>
public sealed class ErrorReporter : IErrorReporter
{
    private readonly IErrorLog errorLog;
    private readonly IFileLauncher fileLauncher;
    private readonly Action<string> setFooterStatus;
    private Panel? toastPanel;

    public ErrorReporter(IErrorLog errorLog, IFileLauncher fileLauncher, Action<string> setFooterStatus)
    {
        this.errorLog = errorLog;
        this.fileLauncher = fileLauncher;
        this.setFooterStatus = setFooterStatus;
    }

    /// <summary>
    /// Provides the <see cref="Panel"/> into which toast cards are inserted.
    /// Called from <c>MainWindow</c> after <c>InitializeComponent()</c> so the
    /// named control is available. If not set, toasts are skipped (log + footer still run).
    /// </summary>
    public void SetToastPanel(Panel panel)
    {
        toastPanel = panel;
    }

    public void Report(string userMessage, Exception? exception = null)
    {
        // Fire-and-forget the log write; a failing log must not prevent the toast.
        errorLog.AppendAsync(userMessage, exception).SafeFireAndForget(onException: _ => { });

        Dispatcher.UIThread.Post(() =>
        {
            try { setFooterStatus($"{userMessage}. Refer to logs for the error details."); }
            catch { }

            try
            {
                if (toastPanel is not null)
                {
                    var card = new ErrorToastCard(userMessage, errorLog.LogFilePath, fileLauncher);
                    toastPanel.Children.Add(card);
                }
            }
            catch { }
        });
    }
}
