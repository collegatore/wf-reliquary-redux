using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Infrastructure.Storage;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteWishlistStore : IWishlistStore
{
    private readonly DbContextOptions<ReliquaryDbContext> options;

    public SqliteWishlistStore(string? databasePath = null)
    {
        var path = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        options = ReliquaryDbContext.BuildOptions(path);
    }

    public async Task SeedDefaultAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        await context.Database.MigrateAsync(cancellationToken);

        // Ensure a profile exists so seeding is not order-dependent on the profile store,
        // then seed the default wishlist into the *active* profile specifically.
        await DefaultProfileSeeder.EnsureAsync(context, cancellationToken);
        var profileId = await ResolveActiveProfileIdAsync(context, cancellationToken);
        if (profileId is null) return;

        if (!await context.Wishlists.AnyAsync(w => w.ProfileId == profileId, cancellationToken))
        {
            var wishlist = new WishlistEntity
            {
                ProfileId = profileId.Value,
                Name = "Default",
                CreatedAt = DateTimeOffset.UtcNow.ToString("O")
            };
            context.Wishlists.Add(wishlist);
            await context.SaveChangesAsync(cancellationToken);

            await SetProfileActiveWishlistAsync(context, profileId.Value, wishlist.Id, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<WishlistEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        var profileId = await ResolveActiveProfileIdAsync(context, cancellationToken);
        if (profileId is null) return [];

        var rows = await context.Wishlists
            .AsNoTracking()
            .Where(w => w.ProfileId == profileId)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
        return rows.Select(ToEntry).ToList();
    }

    public async Task<WishlistEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);

        var profileId = await ResolveActiveProfileIdAsync(context, cancellationToken);
        if (profileId is null) return null;

        var profile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken);

        if (profile?.ActiveWishlistId is long wishlistId)
        {
            var stored = await context.Wishlists
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == wishlistId && w.ProfileId == profileId, cancellationToken);
            if (stored != null) return ToEntry(stored);
        }

        var fallback = await context.Wishlists
            .AsNoTracking()
            .Where(w => w.ProfileId == profileId)
            .OrderBy(w => w.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return fallback is null ? null : ToEntry(fallback);
    }

    public async Task SetActiveAsync(long wishlistId, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        var profileId = await ResolveActiveProfileIdAsync(context, cancellationToken);
        if (profileId is null) return;

        await SetProfileActiveWishlistAsync(context, profileId.Value, wishlistId, cancellationToken);
    }

    public async Task<WishlistEntry> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        var profileId = await ResolveActiveProfileIdAsync(context, cancellationToken)
            ?? throw new InvalidOperationException("Cannot create a wishlist when no profile exists.");

        var createdAt = DateTimeOffset.UtcNow;
        var entity = new WishlistEntity
        {
            ProfileId = profileId,
            Name = name,
            CreatedAt = createdAt.ToString("O")
        };
        context.Wishlists.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return new WishlistEntry(entity.Id, name, createdAt);
    }

    public async Task RenameAsync(long wishlistId, string newName, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        await context.Wishlists
            .Where(w => w.Id == wishlistId)
            .ExecuteUpdateAsync(s => s.SetProperty(w => w.Name, newName), cancellationToken);
    }

    public async Task DeleteAsync(long wishlistId, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        await context.Wishlists
            .Where(w => w.Id == wishlistId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WishlistItem>> GetItemsAsync(long wishlistId, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);

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
            .ToListAsync(cancellationToken);

        return rows.Select(r => new WishlistItem(
            r.wi.WishlistId,
            r.wi.PartNormalizedName,
            r.wi.GearName,
            r.wi.PartDisplayName,
            r.IsGhost)).ToList();
    }

    public async Task AddItemAsync(
        long wishlistId,
        string partNormalizedName,
        string gearName,
        string partDisplayName,
        CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
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
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            // Idempotent — UNIQUE constraint violation, part already in wishlist.
        }
    }

    public async Task RemoveItemAsync(long wishlistId, string partNormalizedName, CancellationToken cancellationToken = default)
    {
        await using var context = new ReliquaryDbContext(options);
        await context.WishlistItems
            .Where(wi => wi.WishlistId == wishlistId && wi.PartNormalizedName == partNormalizedName)
            .ExecuteDeleteAsync(cancellationToken);
    }

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
