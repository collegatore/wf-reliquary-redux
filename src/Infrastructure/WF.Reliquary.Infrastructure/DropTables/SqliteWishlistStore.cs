using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Infrastructure.Storage;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteWishlistStore : IWishlistStore
{
    private readonly ReliquaryDbExecutor executor;

    public SqliteWishlistStore(string? databasePath = null)
    {
        DatabasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(DatabasePath));
    }

    private string DatabasePath { get; }

    public Task SeedDefaultAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            await context.Database.MigrateAsync(ct).ConfigureAwait(false);

            // Ensure a profile exists so seeding is not order-dependent on the profile store,
            // then seed the default wishlist into the *active* profile specifically.
            await DefaultProfileSeeder.EnsureAsync(context, ct).ConfigureAwait(false);
            var profileId = await ResolveActiveProfileIdAsync(context, ct).ConfigureAwait(false);
            if (profileId is null) return;

            if (!await context.Wishlists.AnyAsync(w => w.ProfileId == profileId, ct).ConfigureAwait(false))
            {
                var wishlist = new WishlistEntity
                {
                    ProfileId = profileId.Value,
                    Name = "Default",
                    CreatedAt = DateTimeOffset.UtcNow.ToString("O")
                };
                context.Wishlists.Add(wishlist);
                await context.SaveChangesAsync(ct).ConfigureAwait(false);

                await SetProfileActiveWishlistAsync(context, profileId.Value, wishlist.Id, ct).ConfigureAwait(false);
            }
        }, cancellationToken);

    public Task<IReadOnlyList<WishlistEntry>> GetAllAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync<IReadOnlyList<WishlistEntry>>(async (context, ct) =>
        {
            var profileId = await ResolveActiveProfileIdAsync(context, ct).ConfigureAwait(false);
            if (profileId is null) return [];

            var rows = await context.Wishlists
                .AsNoTracking()
                .Where(w => w.ProfileId == profileId)
                .OrderBy(w => w.CreatedAt)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return rows.Select(ToEntry).ToList();
        }, cancellationToken);

    public Task<WishlistEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync<WishlistEntry?>(async (context, ct) =>
        {
            var profileId = await ResolveActiveProfileIdAsync(context, ct).ConfigureAwait(false);
            if (profileId is null) return null;

            var profile = await context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == profileId, ct)
                .ConfigureAwait(false);

            if (profile?.ActiveWishlistId is long wishlistId)
            {
                var stored = await context.Wishlists
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == wishlistId && w.ProfileId == profileId, ct)
                    .ConfigureAwait(false);
                if (stored != null) return ToEntry(stored);
            }

            var fallback = await context.Wishlists
                .AsNoTracking()
                .Where(w => w.ProfileId == profileId)
                .OrderBy(w => w.CreatedAt)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
            return fallback is null ? null : ToEntry(fallback);
        }, cancellationToken);

    public Task SetActiveAsync(long wishlistId, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var profileId = await ResolveActiveProfileIdAsync(context, ct).ConfigureAwait(false);
            if (profileId is null) return;

            await SetProfileActiveWishlistAsync(context, profileId.Value, wishlistId, ct).ConfigureAwait(false);
        }, cancellationToken);

    public Task<WishlistEntry> CreateAsync(string name, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var profileId = await ResolveActiveProfileIdAsync(context, ct).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Cannot create a wishlist when no profile exists.");

            var createdAt = DateTimeOffset.UtcNow;
            var entity = new WishlistEntity
            {
                ProfileId = profileId,
                Name = name,
                CreatedAt = createdAt.ToString("O")
            };
            context.Wishlists.Add(entity);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return new WishlistEntry(entity.Id, name, createdAt);
        }, cancellationToken);

    public Task RenameAsync(long wishlistId, string newName, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            await context.Wishlists
                .Where(w => w.Id == wishlistId)
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.Name, newName), ct)
                .ConfigureAwait(false);
        }, cancellationToken);

    public Task DeleteAsync(long wishlistId, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            await context.Wishlists
                .Where(w => w.Id == wishlistId)
                .ExecuteDeleteAsync(ct)
                .ConfigureAwait(false);
        }, cancellationToken);

    public Task<IReadOnlyList<WishlistItem>> GetItemsAsync(long wishlistId, CancellationToken cancellationToken = default)
        => executor.RunAsync<IReadOnlyList<WishlistItem>>(async (context, ct) =>
        {
            var rows = await context.WishlistItems
                .AsNoTracking()
                .Where(wi => wi.WishlistId == wishlistId)
                .GroupJoin(
                    context.RewardItems.AsNoTracking(),
                    wi => wi.PartNormalizedName,
                    ri => ri.NormalizedName,
                    (wi, ris) => new { wi, ris })
                .SelectMany(
                    x => x.ris.DefaultIfEmpty(),
                    (x, ri) => new { x.wi, IsGhost = ri == null })
                .OrderBy(x => x.wi.GearName)
                .ThenBy(x => x.wi.PartDisplayName)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return rows.Select(r => new WishlistItem(
                r.wi.WishlistId,
                r.wi.PartNormalizedName,
                r.wi.GearName,
                r.wi.PartDisplayName,
                r.IsGhost)).ToList();
        }, cancellationToken);

    public Task AddItemAsync(
        long wishlistId,
        string partNormalizedName,
        string gearName,
        string partDisplayName,
        CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var entity = new WishlistItemEntity
            {
                WishlistId = wishlistId,
                PartNormalizedName = partNormalizedName,
                GearName = gearName,
                PartDisplayName = partDisplayName,
                AddedAt = DateTimeOffset.UtcNow.ToString("O")
            };
            context.WishlistItems.Add(entity);

            try
            {
                await context.SaveChangesAsync(ct).ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
            {
                // Idempotent — UNIQUE constraint violation, part already in wishlist.
            }
        }, cancellationToken);

    public Task RemoveItemAsync(long wishlistId, string partNormalizedName, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            await context.WishlistItems
                .Where(wi => wi.WishlistId == wishlistId && wi.PartNormalizedName == partNormalizedName)
                .ExecuteDeleteAsync(ct)
                .ConfigureAwait(false);
        }, cancellationToken);

    private static WishlistEntry ToEntry(WishlistEntity e)
        => new(e.Id, e.Name, DateTimeOffset.Parse(e.CreatedAt));

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

    private static async Task SetProfileActiveWishlistAsync(
        ReliquaryDbContext context, long profileId, long wishlistId, CancellationToken cancellationToken)
    {
        await context.Profiles
            .Where(p => p.Id == profileId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ActiveWishlistId, wishlistId), cancellationToken);
    }
}
