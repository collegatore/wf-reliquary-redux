namespace WF.Reliquary.Presentation;

/// <summary>
/// Sink for surfacing asynchronous failures (caught command failures,
/// <c>SafeFireAndForget</c> exceptions, failed background loads).
/// The real implementation is <see cref="ErrorReporter"/>, which logs to file,
/// updates the footer, and shows a dismissible toast with log-access controls.
/// </summary>
public interface IErrorReporter
{
    /// <summary>
    /// Reports a failure with a short, operation-named user-facing message and,
    /// where available, the originating exception.
    /// </summary>
    void Report(string userMessage, Exception? exception = null);
}

/// <summary>
/// No-op reporter used in tests and design-time contexts where no real reporting
/// surface is available. Swallows all reports silently.
/// </summary>
public sealed class NoOpErrorReporter : IErrorReporter
{
    public static readonly NoOpErrorReporter Instance = new();

    public void Report(string userMessage, Exception? exception = null) { }
}

/// <summary>
/// Process-wide handle to the active <see cref="IErrorReporter"/>. The global
/// <c>SafeFireAndForget</c> / unobserved-exception handlers and the ViewModels route
/// here, so the <c>error-reporting-surface</c> change can swap in the real toast + log
/// reporter at one place. Defaults to <see cref="NoOpErrorReporter"/>.
/// </summary>
public static class AppErrors
{
    public static IErrorReporter Reporter { get; set; } = NoOpErrorReporter.Instance;
}
