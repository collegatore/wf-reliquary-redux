using Microsoft.EntityFrameworkCore;

namespace WF.Reliquary.Infrastructure.Storage;

/// <summary>
/// Runs a unit of work against a fresh <see cref="ReliquaryDbContext"/> on a
/// thread-pool thread.
///
/// The SQLite EF Core provider (<c>Microsoft.Data.Sqlite</c>) executes its
/// <c>…Async</c> methods <em>synchronously</em> — there is no async I/O for SQLite —
/// so awaiting <c>ToListAsync</c>/<c>SaveChangesAsync</c>/etc. on the caller's thread
/// does not yield it. Offloading here, at the single seam that owns the context
/// lifetime, guarantees DB work never runs on the UI thread without each store method
/// or consumer remembering to wrap. One <see cref="Task.Run(Func{Task})"/> is used per
/// logical operation (per context lifetime); the context is created and used entirely
/// inside that delegate, so it stays confined to a single thread.
/// </summary>
internal sealed class ReliquaryDbExecutor
{
    private readonly DbContextOptions<ReliquaryDbContext> options;

    public ReliquaryDbExecutor(DbContextOptions<ReliquaryDbContext> options)
    {
        this.options = options;
    }

    public Task<T> RunAsync<T>(
        Func<ReliquaryDbContext, CancellationToken, Task<T>> work,
        CancellationToken cancellationToken = default)
        => Task.Run(async () =>
        {
            await using var context = new ReliquaryDbContext(options);
            return await work(context, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);

    public Task RunAsync(
        Func<ReliquaryDbContext, CancellationToken, Task> work,
        CancellationToken cancellationToken = default)
        => Task.Run(async () =>
        {
            await using var context = new ReliquaryDbContext(options);
            await work(context, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
}
