using Microsoft.Data.Sqlite;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteWishlistRelicsReader : IWishlistRelicsReader
{
    private readonly string databasePath;
    private readonly string connectionString;

    public SqliteWishlistRelicsReader(string? databasePath = null)
    {
        this.databasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        connectionString = new SqliteConnectionStringBuilder { DataSource = this.databasePath }.ConnectionString;
    }

    public async Task<IReadOnlyList<WishlistRelicEntry>> GetRelicsForWishlistAsync(
        long wishlistId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH wishlisted_relic_ids AS (
                SELECT DISTINCT rr.relic_id
                FROM wishlist_items wi
                JOIN reward_items ri ON ri.normalized_name = wi.part_normalized_name
                JOIN relic_rewards rr ON rr.reward_item_id = ri.id
                WHERE wi.wishlist_id = $wishlist_id
            )
            SELECT
                r.display_name,
                r.era,
                r.is_vaulted,
                ri.name AS reward_name,
                rr.reward_tier,
                CASE WHEN wi.part_normalized_name IS NOT NULL THEN 1 ELSE 0 END AS is_wishlisted
            FROM relics r
            JOIN wishlisted_relic_ids wri ON wri.relic_id = r.id
            JOIN relic_rewards rr ON rr.relic_id = r.id
            JOIN reward_items ri ON ri.id = rr.reward_item_id
            LEFT JOIN wishlist_items wi
                ON wi.part_normalized_name = ri.normalized_name
                AND wi.wishlist_id = $wishlist_id
            ORDER BY
                CASE r.era WHEN 'Lith' THEN 1 WHEN 'Meso' THEN 2 WHEN 'Neo' THEN 3 WHEN 'Axi' THEN 4 END,
                r.display_name,
                CASE rr.reward_tier WHEN 'Common' THEN 1 WHEN 'Uncommon' THEN 2 WHEN 'Rare' THEN 3 ELSE 4 END,
                rr.source_order
            """;

        if (!File.Exists(databasePath))
        {
            return [];
        }

        var entries = new List<WishlistRelicEntry>();

        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Ensure is_vaulted column exists. It is normally added by the import pipeline,
            // but on first launch after this schema change the column may be absent from
            // an existing database. Default 0 (not vaulted) is corrected on the next import.
            await EnsureIsVaultedColumnAsync(connection, cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$wishlist_id", wishlistId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            string? currentDisplay = null;
            string? currentEra = null;
            var currentIsVaulted = false;
            var currentRewards = new List<WishlistRelicReward>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var displayName = reader.GetString(0);
                var era = reader.GetString(1);
                var isVaulted = reader.GetInt32(2) == 1;
                var rewardName = reader.GetString(3);
                var rewardTier = reader.GetString(4);
                var isWishlisted = reader.GetInt32(5) == 1;

                if (currentDisplay != null && displayName != currentDisplay)
                {
                    entries.Add(new WishlistRelicEntry(currentDisplay, currentEra!, currentIsVaulted, currentRewards));
                    currentRewards = [];
                }

                currentDisplay = displayName;
                currentEra = era;
                currentIsVaulted = isVaulted;
                currentRewards.Add(new WishlistRelicReward(rewardName, rewardTier, isWishlisted));
            }

            if (currentDisplay != null)
            {
                entries.Add(new WishlistRelicEntry(currentDisplay, currentEra!, currentIsVaulted, currentRewards));
            }
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
        {
            return [];
        }

        return entries;
    }

    private static async Task EnsureIsVaultedColumnAsync(
        SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA table_info('relics');";
        await using var pragmaReader = await pragmaCmd.ExecuteReaderAsync(cancellationToken);
        while (await pragmaReader.ReadAsync(cancellationToken))
        {
            if (string.Equals(pragmaReader.GetString(1), "is_vaulted", StringComparison.OrdinalIgnoreCase))
                return;
        }

        // Column absent — add it. Default 0 is corrected on the next import.
        await using var alterCmd = connection.CreateCommand();
        alterCmd.CommandText = "ALTER TABLE relics ADD COLUMN is_vaulted INTEGER NOT NULL DEFAULT 0;";
        await alterCmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
