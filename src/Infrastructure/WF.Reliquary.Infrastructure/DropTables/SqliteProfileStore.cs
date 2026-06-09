using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Profiles;
using WF.Reliquary.Infrastructure.Storage;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteProfileStore : IProfileStore
{
    private readonly DbContextOptions<ReliquaryDbContext> options;

    public SqliteProfileStore(string? databasePath = null)
    {
        var path = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        options = ReliquaryDbContext.BuildOptions(path);
    }

    public async Task SeedDefaultAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        await context.Database.MigrateAsync(cancellationToken);
        await DefaultProfileSeeder.EnsureAsync(context, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfileEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        var rows = await context.Profiles
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
        return rows.Select(ToEntry).ToList();
    }

    public async Task<ProfileEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);

        var prefs = await context.UserPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == 1, cancellationToken);

        if (prefs?.LastProfileId is long profileId)
        {
            var stored = await context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken);
            if (stored != null) return ToEntry(stored);
        }

        var fallback = await context.Profiles
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return fallback is null ? null : ToEntry(fallback);
    }

    public async Task SetActiveAsync(long profileId, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        var prefs = await context.UserPreferences.FindAsync([1L], cancellationToken);
        if (prefs is null)
        {
            context.UserPreferences.Add(new UserPreferencesEntity { Id = 1, LastProfileId = profileId });
        }
        else
        {
            prefs.LastProfileId = profileId;
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProfileEntry> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        var createdAt = DateTimeOffset.UtcNow;
        var entity = new ProfileEntity { Name = name, CreatedAt = createdAt.ToString("O") };
        context.Profiles.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return new ProfileEntry(entity.Id, name, createdAt);
    }

    public async Task RenameAsync(long profileId, string newName, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        await context.Profiles
            .Where(p => p.Id == profileId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Name, newName), cancellationToken);
    }

    public async Task DeleteAsync(long profileId, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        await context.Profiles
            .Where(p => p.Id == profileId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static ProfileEntry ToEntry(ProfileEntity e)
        => new(e.Id, e.Name, DateTimeOffset.Parse(e.CreatedAt));
}
