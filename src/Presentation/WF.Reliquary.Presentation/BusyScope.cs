namespace WF.Reliquary.Presentation;

/// <summary>
/// Reusable busy + reentrancy guard. A ViewModel holds one <see cref="BusyScope"/> and
/// calls <see cref="TryEnter"/> at the top of a guarded operation: the first entry sets
/// <see cref="IsBusy"/> true and returns a disposable token; a re-entrant call while busy
/// returns <see langword="null"/> so the caller early-returns. Disposing the token resets
/// <see cref="IsBusy"/> to false (use with <c>using</c> so it resets on success or throw).
/// </summary>
public sealed class BusyScope
{
    private readonly Action<bool> onBusyChanged;
    private bool isBusy;

    /// <param name="onBusyChanged">Invoked whenever the busy state flips, so the owner can
    /// raise its own <c>IsBusy</c> change notification and re-evaluate command CanExecute.</param>
    public BusyScope(Action<bool>? onBusyChanged = null)
    {
        this.onBusyChanged = onBusyChanged ?? (_ => { });
    }

    public bool IsBusy => isBusy;

    /// <summary>
    /// Enters the scope. Returns a disposable token on success, or <see langword="null"/>
    /// if already busy (the caller must early-return).
    /// </summary>
    public IDisposable? TryEnter()
    {
        if (isBusy)
        {
            return null;
        }

        isBusy = true;
        onBusyChanged(true);
        return new Token(this);
    }

    private void Exit()
    {
        isBusy = false;
        onBusyChanged(false);
    }

    private sealed class Token : IDisposable
    {
        private BusyScope? owner;

        public Token(BusyScope owner) => this.owner = owner;

        public void Dispose()
        {
            owner?.Exit();
            owner = null;
        }
    }
}
