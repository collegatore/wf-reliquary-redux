namespace WF.Reliquary.Presentation;

/// <summary>
/// Serializes a ViewModel's reloads with latest-wins semantics. Each call to
/// <see cref="Begin"/> cancels the previous in-flight reload's token and returns a fresh
/// token for the new one. A reload superseded by a newer trigger sees its token cancelled
/// and must no-op (check the token before mutating bound collections) so only the latest
/// reload's result is applied.
/// </summary>
public sealed class ReloadGate : IDisposable
{
    private CancellationTokenSource? current;

    /// <summary>
    /// Cancels the previous reload (if any) and returns a token for the new one.
    /// </summary>
    public CancellationToken Begin()
    {
        var previous = current;
        var next = new CancellationTokenSource();
        current = next;

        previous?.Cancel();
        previous?.Dispose();

        return next.Token;
    }

    public void Dispose()
    {
        current?.Cancel();
        current?.Dispose();
        current = null;
    }
}
