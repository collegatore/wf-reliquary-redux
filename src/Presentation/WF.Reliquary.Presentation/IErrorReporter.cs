namespace WF.Reliquary.Presentation;

/// <summary>
/// Sink for surfacing asynchronous failures (caught command failures,
/// <c>SafeFireAndForget</c> exceptions, failed background loads).
///
/// This is the seam the UI concurrency contract routes failures to. The full
/// implementation — a toast backed by an application log file with copy-path /
/// open-log controls — is delivered by the separate <c>error-reporting-surface</c>
/// change. Until that lands, a <see cref="NoOpErrorReporter"/> is wired so the
/// contract has a real, swappable target.
/// </summary>
public interface IErrorReporter
{
    /// <summary>
    /// Reports a failure with a short user-facing message and, where available, the
    /// originating exception (caught exceptions pass it; domain failures that carry only
    /// a message may omit it).
    /// </summary>
    void Report(string userMessage, Exception? exception = null);
}

/// <summary>
/// Interim no-op reporter used until the <c>error-reporting-surface</c> change
/// supplies the toast + log implementation. Swallows reports silently.
/// </summary>
public sealed class NoOpErrorReporter : IErrorReporter
{
    public static readonly NoOpErrorReporter Instance = new();

    public void Report(string userMessage, Exception? exception = null)
    {
        // Intentionally no-op; replaced by the toast+log reporter from error-reporting-surface.
    }
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
