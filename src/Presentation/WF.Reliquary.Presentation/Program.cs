using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Fonts.Inter;
using Avalonia.Logging;

namespace WF.Reliquary.Presentation;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Last-resort observation of asynchronous failures. SafeFireAndForget routes any
        // unhandled fire-and-forget exception here; TaskScheduler catches tasks whose
        // exceptions were never observed. Both forward to the active error reporter
        // (interim no-op until error-reporting-surface supplies the toast + log reporter).
        SafeFireAndForgetExtensions.SetDefaultExceptionHandling(
            ex => AppErrors.Reporter.Report("An unexpected error occurred.", ex));

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            AppErrors.Reporter.Report("An unexpected background error occurred.", e.Exception);
            e.SetObserved();
        };

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
