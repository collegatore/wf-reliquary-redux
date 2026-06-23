using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Profiles;
using WF.Reliquary.Infrastructure.Storage;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteProfileStore : IProfileStore
{
    private readonly ReliquaryDbExecutor executor;

    public SqliteProfileStore(string? databasePath = null)
    {
        var path = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(path));
    }

    public Task SeedDefaultAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            await context.Database.MigrateAsync(ct).ConfigureAwait(false);
            await DefaultProfileSeeder.EnsureAsync(context, ct).ConfigureAwait(false);
        }, cancellationToken);

    public Task<IReadOnlyList<ProfileEntry>> GetAllAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync<IReadOnlyList<ProfileEntry>>(async (context, ct) =>
        {
            var rows = await context.Profiles
                .AsNoTracking()
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return rows.Select(ToEntry).ToList();
        }, cancellationToken);

    public Task<ProfileEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync<ProfileEntry?>(async (context, ct) =>
        {
            var prefs = await context.UserPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == 1, ct)
                .ConfigureAwait(false);

            if (prefs?.LastProfileId is long profileId)
            {
                var stored = await context.Profiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == profileId, ct)
                    .ConfigureAwait(false);
                if (stored != null) return ToEntry(stored);
            }

            var fallback = await context.Profiles
                .AsNoTracking()
                .OrderBy(p => p.CreatedAt)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
            return fallback is null ? null : ToEntry(fallback);
        }, cancellationToken);

    public Task SetActiveAsync(long profileId, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var prefs = await context.UserPreferences.FindAsync([1L], ct).ConfigureAwait(false);
            if (prefs is null)
            {
                context.UserPreferences.Add(new UserPreferencesEntity { Id = 1, LastProfileId = profileId });
            }
            else
            {
                prefs.LastProfileId = profileId;
            }
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken);

    public Task<ProfileEntry> CreateAsync(string name, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var createdAt = DateTimeOffset.UtcNow;
            var entity = new ProfileEntity { Name = name, CreatedAt = createdAt.ToString("O") };
            context.Profiles.Add(entity);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return new ProfileEntry(entity.Id, name, createdAt);
        }, cancellationToken);

    public Task RenameAsync(long profileId, string newName, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            await context.Profiles
                .Where(p => p.Id == profileId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Name, newName), ct)
                .ConfigureAwait(false);
        }, cancellationToken);

    public Task DeleteAsync(long profileId, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            await context.Profiles
                .Where(p => p.Id == profileId)
                .ExecuteDeleteAsync(ct)
                .ConfigureAwait(false);
        }, cancellationToken);

    private static ProfileEntry ToEntry(ProfileEntity e)
        => new(e.Id, e.Name, DateTimeOffset.Parse(e.CreatedAt));
}
