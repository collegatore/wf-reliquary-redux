using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class ReliquaryDbExecutorTests
{
    // Microsoft.Data.Sqlite runs its async methods synchronously, so the executor's
    // job is to push that work onto a thread-pool thread. The reliable invariant is
    // that the unit-of-work body runs on a thread-pool thread (Task.Run guarantees it).
    [Test]
    public async Task RunAsync_ExecutesWorkOnAThreadPoolThread()
    {
        var executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(TempDatabasePath()));

        var ranOnThreadPool = await executor.RunAsync(async (context, ct) =>
        {
            await context.Database.MigrateAsync(ct);
            return Thread.CurrentThread.IsThreadPoolThread;
        });

        Assert.That(ranOnThreadPool, Is.True);
    }

    // The context created for one unit of work must stay confined to the single thread
    // the delegate runs on (DbContext is not thread-safe). Capture the thread id at the
    // start of the delegate and after an awaited EF call; they must match.
    [Test]
    public async Task RunAsync_KeepsTheContextOnASingleThread()
    {
        var executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(TempDatabasePath()));

        var (before, after) = await executor.RunAsync(async (context, ct) =>
        {
            var startThread = Environment.CurrentManagedThreadId;
            await context.Database.MigrateAsync(ct);
            return (startThread, Environment.CurrentManagedThreadId);
        });

        Assert.That(after, Is.EqualTo(before));
    }

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "executor.sqlite");
    }
}
