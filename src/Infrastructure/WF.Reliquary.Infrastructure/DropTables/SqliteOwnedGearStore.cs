using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteOwnedGearStore : IOwnedGearStore
{
    private readonly DbContextOptions<ReliquaryDbContext> options;

    public SqliteOwnedGearStore(string? databasePath = null)
    {
        var path = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        options = ReliquaryDbContext.BuildOptions(path);
    }

    public async Task<IReadOnlyList<string>> GetOwnedGearNamesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        var profileId = await ResolveActiveProfileIdAsync(context, cancellationToken);
        if (profileId is null) return [];

        return await context.OwnedGear
            .AsNoTracking()
            .Where(g => g.ProfileId == profileId)
            .Select(g => g.GearName)
            .ToListAsync(cancellationToken);
    }

    public async Task SetOwnedAsync(string gearName, bool owned, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        var profileId = await ResolveActiveProfileIdAsync(context, cancellationToken);
        if (profileId is null) return;

        if (owned)
        {
            context.OwnedGear.Add(new OwnedGearEntity { ProfileId = profileId.Value, GearName = gearName });
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
            {
                // Idempotent — UNIQUE constraint violation, gear already owned in this profile.
            }
        }
        else
        {
            await context.OwnedGear
                .Where(g => g.ProfileId == profileId && g.GearName == gearName)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Resolves the active profile id from the persisted preference, falling back to
    /// the earliest-created profile when the preference is null or dangling. Returns
    /// null only when no profiles exist at all.
    /// </summary>
    private static async Task<long?> ResolveActiveProfileIdAsync(
        ReliquaryDbContext context, CancellationToken cancellationToken)
    {
        var prefs = await context.UserPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == 1, cancellationToken);

        if (prefs?.LastProfileId is long profileId
            && await context.Profiles.AnyAsync(p => p.Id == profileId, cancellationToken))
        {
            return profileId;
        }

        var fallback = await context.Profiles
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return fallback?.Id;
    }
}
