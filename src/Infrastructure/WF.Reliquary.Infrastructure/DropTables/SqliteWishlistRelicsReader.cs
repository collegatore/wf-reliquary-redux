using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteWishlistRelicsReader : IWishlistRelicsReader
{
    private readonly string databasePath;
    private readonly ReliquaryDbExecutor executor;
    private readonly string wishlistRelicsSql;

    public SqliteWishlistRelicsReader(string? databasePath = null)
    {
        this.databasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(this.databasePath));
        wishlistRelicsSql = SqlQueryLoader.Load("GetWishlistRelics.sql");
    }

    public Task<IReadOnlyList<WishlistRelicEntry>> GetRelicsForWishlistAsync(
        long wishlistId, CancellationToken cancellationToken = default)
        => executor.RunAsync<IReadOnlyList<WishlistRelicEntry>>(async (context, ct) =>
        {
            try
            {
                var rows = await context.Database
                    .SqlQueryRaw<WishlistRelicRow>(
                        wishlistRelicsSql,
                        new SqliteParameter("@wishlist_id", wishlistId))
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                return BuildEntries(rows);
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1)
            {
                return [];
            }
        }, cancellationToken);

    private static IReadOnlyList<WishlistRelicEntry> BuildEntries(List<WishlistRelicRow> rows)
    {
        var entries = new List<WishlistRelicEntry>();
        string? currentDisplay = null;
        string? currentEra = null;
        var currentIsVaulted = false;
        var currentRewards = new List<WishlistRelicReward>();

        foreach (var row in rows)
        {
            if (currentDisplay != null && row.DisplayName != currentDisplay)
            {
                entries.Add(new WishlistRelicEntry(currentDisplay, currentEra!, currentIsVaulted, currentRewards));
                currentRewards = [];
            }

            currentDisplay = row.DisplayName;
            currentEra = row.Era;
            currentIsVaulted = row.IsVaulted != 0;
            currentRewards.Add(new WishlistRelicReward(row.RewardName, row.RewardTier, row.IsWishlisted != 0));
        }

        if (currentDisplay != null)
        {
            entries.Add(new WishlistRelicEntry(currentDisplay, currentEra!, currentIsVaulted, currentRewards));
        }

        return entries;
    }

    private sealed class WishlistRelicRow
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Era { get; set; } = string.Empty;
        public int IsVaulted { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public string RewardTier { get; set; } = string.Empty;
        public int IsWishlisted { get; set; }
    }
}
