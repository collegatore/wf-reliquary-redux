using Microsoft.Data.Sqlite;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteWishlistStore : IWishlistStore
{
    private readonly string connectionString;

    public SqliteWishlistStore(string? databasePath = null)
    {
        var path = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        connectionString = new SqliteConnectionStringBuilder { DataSource = path }.ConnectionString;
    }

    public async Task SeedDefaultAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await EnsureSchemaAsync(connection, transaction, cancellationToken);

        var count = await ScalarAsync<long>(
            connection, transaction,
            "SELECT COUNT(*) FROM wishlists;",
            cancellationToken);

        if (count == 0)
        {
            var id = await InsertAndReturnIdAsync(
                connection, transaction,
                "INSERT INTO wishlists (name, created_at) VALUES ('Default', $created_at);",
                cancellationToken,
                ("$created_at", DateTimeOffset.UtcNow.ToString("O")));

            await NonQueryAsync(
                connection, transaction,
                "INSERT OR REPLACE INTO user_preferences (id, last_wishlist_id) VALUES (1, $id);",
                cancellationToken,
                ("$id", id));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WishlistEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);

        var result = new List<WishlistEntry>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, created_at FROM wishlists ORDER BY created_at;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(ReadEntry(reader));
        }

        return result;
    }

    public async Task<WishlistEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);

        // Try the stored preference first.
        var storedId = await ScalarAsync<object?>(
            connection, null,
            "SELECT last_wishlist_id FROM user_preferences WHERE id = 1;",
            cancellationToken);

        if (storedId is long wishlistId)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id, name, created_at FROM wishlists WHERE id = $id;";
            cmd.Parameters.AddWithValue("$id", wishlistId);
            await using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await rdr.ReadAsync(cancellationToken))
            {
                return ReadEntry(rdr);
            }
        }

        // Fall back to first available wishlist.
        await using var fallbackCmd = connection.CreateCommand();
        fallbackCmd.CommandText = "SELECT id, name, created_at FROM wishlists ORDER BY created_at LIMIT 1;";
        await using var fallbackRdr = await fallbackCmd.ExecuteReaderAsync(cancellationToken);
        if (await fallbackRdr.ReadAsync(cancellationToken))
        {
            return ReadEntry(fallbackRdr);
        }

        return null;
    }

    public async Task SetActiveAsync(long wishlistId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await NonQueryAsync(
            connection, transaction,
            "INSERT OR REPLACE INTO user_preferences (id, last_wishlist_id) VALUES (1, $id);",
            cancellationToken,
            ("$id", wishlistId));

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<WishlistEntry> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        var createdAt = DateTimeOffset.UtcNow;
        var id = await InsertAndReturnIdAsync(
            connection, transaction,
            "INSERT INTO wishlists (name, created_at) VALUES ($name, $created_at);",
            cancellationToken,
            ("$name", name),
            ("$created_at", createdAt.ToString("O")));

        await transaction.CommitAsync(cancellationToken);
        return new WishlistEntry(id, name, createdAt);
    }

    public async Task RenameAsync(long wishlistId, string newName, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await NonQueryAsync(
            connection, transaction,
            "UPDATE wishlists SET name = $name WHERE id = $id;",
            cancellationToken,
            ("$name", newName),
            ("$id", wishlistId));

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(long wishlistId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await NonQueryAsync(
            connection, transaction,
            "DELETE FROM wishlists WHERE id = $id;",
            cancellationToken,
            ("$id", wishlistId));

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WishlistItem>> GetItemsAsync(long wishlistId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);

        // Ghost detection: LEFT JOIN reward_items. If reward_items doesn't exist
        // (no import has run yet), fall back to treating all items as ghosts.
        const string joinSql = """
            SELECT
                wi.wishlist_id,
                wi.part_normalized_name,
                wi.gear_name,
                wi.part_display_name,
                CASE WHEN ri.id IS NULL THEN 1 ELSE 0 END AS is_ghost
            FROM wishlist_items wi
            LEFT JOIN reward_items ri ON ri.normalized_name = wi.part_normalized_name
            WHERE wi.wishlist_id = $wishlist_id
            ORDER BY wi.gear_name, wi.part_display_name;
            """;

        const string fallbackSql = """
            SELECT wishlist_id, part_normalized_name, gear_name, part_display_name, 1 AS is_ghost
            FROM wishlist_items
            WHERE wishlist_id = $wishlist_id
            ORDER BY gear_name, part_display_name;
            """;

        try
        {
            return await QueryItemsAsync(connection, joinSql, wishlistId, cancellationToken);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1) // SQLITE_ERROR: reward_items may not exist yet
        {
            return await QueryItemsAsync(connection, fallbackSql, wishlistId, cancellationToken);
        }
    }

    private static async Task<IReadOnlyList<WishlistItem>> QueryItemsAsync(
        SqliteConnection connection,
        string sql,
        long wishlistId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$wishlist_id", wishlistId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<WishlistItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new WishlistItem(
                WishlistId: reader.GetInt64(0),
                PartNormalizedName: reader.GetString(1),
                GearName: reader.GetString(2),
                PartDisplayName: reader.GetString(3),
                IsGhost: reader.GetInt32(4) == 1));
        }

        return result;
    }

    public async Task AddItemAsync(
        long wishlistId,
        string partNormalizedName,
        string gearName,
        string partDisplayName,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await NonQueryAsync(
            connection, transaction,
            """
            INSERT OR IGNORE INTO wishlist_items
                (wishlist_id, part_normalized_name, gear_name, part_display_name, added_at)
            VALUES ($wishlist_id, $part_normalized_name, $gear_name, $part_display_name, $added_at);
            """,
            cancellationToken,
            ("$wishlist_id", wishlistId),
            ("$part_normalized_name", partNormalizedName),
            ("$gear_name", gearName),
            ("$part_display_name", partDisplayName),
            ("$added_at", DateTimeOffset.UtcNow.ToString("O")));

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(long wishlistId, string partNormalizedName, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await NonQueryAsync(
            connection, transaction,
            "DELETE FROM wishlist_items WHERE wishlist_id = $wishlist_id AND part_normalized_name = $part_normalized_name;",
            cancellationToken,
            ("$wishlist_id", wishlistId),
            ("$part_normalized_name", partNormalizedName));

        await transaction.CommitAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async Task EnsureSchemaAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS wishlists (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                created_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS wishlist_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                wishlist_id INTEGER NOT NULL REFERENCES wishlists(id) ON DELETE CASCADE,
                part_normalized_name TEXT NOT NULL,
                gear_name TEXT NOT NULL,
                part_display_name TEXT NOT NULL,
                added_at TEXT NOT NULL,
                UNIQUE(wishlist_id, part_normalized_name)
            );

            CREATE TABLE IF NOT EXISTS user_preferences (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                last_wishlist_id INTEGER NULL REFERENCES wishlists(id) ON DELETE SET NULL
            );
            """;

        await NonQueryAsync(connection, transaction, sql, cancellationToken);
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Enable foreign key support (required for ON DELETE CASCADE).
        await using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        await pragma.ExecuteNonQueryAsync(cancellationToken);

        return connection;
    }

    private static WishlistEntry ReadEntry(SqliteDataReader reader)
    {
        return new WishlistEntry(
            Id: reader.GetInt64(0),
            Name: reader.GetString(1),
            CreatedAt: DateTimeOffset.Parse(reader.GetString(2)));
    }

    private static async Task<long> InsertAndReturnIdAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        CancellationToken cancellationToken,
        params (string Name, object Value)[] parameters)
    {
        await NonQueryAsync(connection, transaction, sql, cancellationToken, parameters);
        await using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT last_insert_rowid();";
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(value);
    }

    private static async Task NonQueryAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string sql,
        CancellationToken cancellationToken,
        params (string Name, object Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<T?> ScalarAsync<T>(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        var value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is DBNull || value is null)
        {
            return default;
        }

        return (T)value;
    }
}
