using Microsoft.Data.Sqlite;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteDropTableImportStore : IDropTableImportStore
{
    private readonly string connectionString;

    public SqliteDropTableImportStore(string? databasePath = null)
    {
        DatabasePath = databasePath ?? DefaultDatabasePath();

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath
        };

        connectionString = builder.ConnectionString;
    }

    public string DatabasePath { get; }

    public static string DefaultDatabasePath()
    {
        return Path.Combine(LocalDropTableSourceStore.DefaultRootPath(), "drop-table-import.sqlite");
    }

    public async Task ReplaceAsync(ParsedDropTableImport import, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(import);

        var directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await CreateSchemaAsync(connection, transaction, cancellationToken);
        await ClearSnapshotAsync(connection, transaction, cancellationToken);
        await InsertSnapshotAsync(connection, transaction, import, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task CreateSchemaAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        if (await RewardItemsTableContainsObsoletePrimePartColumnAsync(connection, transaction, cancellationToken))
        {
            const string dropLegacySchemaSql = """
                DROP TABLE IF EXISTS mission_rewards;
                DROP TABLE IF EXISTS reward_rotations;
                DROP TABLE IF EXISTS mission_sources;
                DROP TABLE IF EXISTS relic_rewards;
                DROP TABLE IF EXISTS relics;
                DROP TABLE IF EXISTS reward_items;
                DROP TABLE IF EXISTS import_metadata;
                """;

            await ExecuteNonQueryAsync(connection, transaction, dropLegacySchemaSql, cancellationToken);
        }

        if (await MissionSourcesTableHasUniqueDisplayLabelAsync(connection, transaction, cancellationToken))
        {
            const string dropMissionSchemaSql = """
                DROP TABLE IF EXISTS mission_rewards;
                DROP TABLE IF EXISTS reward_rotations;
                DROP TABLE IF EXISTS mission_sources;
                """;

            await ExecuteNonQueryAsync(connection, transaction, dropMissionSchemaSql, cancellationToken);
        }

        const string sql = """
            CREATE TABLE IF NOT EXISTS import_metadata (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                source_url TEXT NULL,
                source_file_path TEXT NOT NULL,
                downloaded_at TEXT NULL,
                source_last_update TEXT NULL,
                imported_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS reward_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                normalized_name TEXT NOT NULL UNIQUE,
                item_kind TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS relics (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                era TEXT NOT NULL,
                code TEXT NOT NULL,
                display_name TEXT NOT NULL UNIQUE,
                reward_item_id INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS relic_rewards (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                relic_id INTEGER NOT NULL,
                reward_item_id INTEGER NOT NULL,
                reward_tier TEXT NOT NULL,
                source_rarity_label TEXT NOT NULL,
                source_chance_percent REAL NOT NULL,
                source_order INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS mission_sources (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                display_label TEXT NOT NULL,
                region TEXT NULL,
                node TEXT NULL,
                mission_type TEXT NULL,
                qualifier TEXT NULL,
                source_order INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS reward_rotations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                mission_source_id INTEGER NOT NULL,
                rotation_label TEXT NOT NULL,
                display_label TEXT NOT NULL,
                source_order INTEGER NOT NULL,
                UNIQUE (mission_source_id, rotation_label)
            );

            CREATE TABLE IF NOT EXISTS mission_rewards (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                mission_source_id INTEGER NOT NULL,
                rotation_id INTEGER NULL,
                reward_item_id INTEGER NOT NULL,
                relic_id INTEGER NULL,
                rarity_label TEXT NOT NULL,
                chance_percent REAL NOT NULL,
                source_order INTEGER NOT NULL
            );
            """;

        await ExecuteNonQueryAsync(connection, transaction, sql, cancellationToken);
    }

    private static async Task ClearSnapshotAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM mission_rewards;
            DELETE FROM reward_rotations;
            DELETE FROM mission_sources;
            DELETE FROM relic_rewards;
            DELETE FROM relics;
            DELETE FROM reward_items;
            DELETE FROM import_metadata;
            """;

        await ExecuteNonQueryAsync(connection, transaction, sql, cancellationToken);
    }

    private static async Task InsertSnapshotAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ParsedDropTableImport import,
        CancellationToken cancellationToken)
    {
        await ExecuteNonQueryAsync(
            connection,
            transaction,
            "INSERT INTO import_metadata (id, source_url, source_file_path, downloaded_at, source_last_update, imported_at) VALUES (1, $source_url, $source_file_path, $downloaded_at, $source_last_update, $imported_at);",
            cancellationToken,
            ("$source_url", (object?)import.SourceUrl?.ToString() ?? DBNull.Value),
            ("$source_file_path", import.SourcePath),
            ("$downloaded_at", (object?)import.DownloadedAt?.ToString("O") ?? DBNull.Value),
            ("$source_last_update", (object?)import.SourceLastUpdate ?? DBNull.Value),
            ("$imported_at", import.ImportedAt.ToString("O")));

        var rewardItemIds = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in import.RewardItems)
        {
            var id = await InsertAndReturnIdAsync(
                connection,
                transaction,
                "INSERT INTO reward_items (name, normalized_name, item_kind) VALUES ($name, $normalized_name, $item_kind);",
                cancellationToken,
                ("$name", item.Name),
                ("$normalized_name", item.NormalizedName),
                ("$item_kind", item.ItemKind));
            rewardItemIds[item.Name] = id;
        }

        var relicIds = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var relic in import.Relics)
        {
            var id = await InsertAndReturnIdAsync(
                connection,
                transaction,
                "INSERT INTO relics (era, code, display_name, reward_item_id) VALUES ($era, $code, $display_name, $reward_item_id);",
                cancellationToken,
                ("$era", relic.Era),
                ("$code", relic.Code),
                ("$display_name", relic.DisplayName),
                ("$reward_item_id", rewardItemIds[relic.RewardItemName]));
            relicIds[relic.DisplayName] = id;
        }

        foreach (var reward in import.RelicRewards)
        {
            await ExecuteNonQueryAsync(
                connection,
                transaction,
                "INSERT INTO relic_rewards (relic_id, reward_item_id, reward_tier, source_rarity_label, source_chance_percent, source_order) VALUES ($relic_id, $reward_item_id, $reward_tier, $source_rarity_label, $source_chance_percent, $source_order);",
                cancellationToken,
                ("$relic_id", relicIds[reward.RelicDisplayName]),
                ("$reward_item_id", rewardItemIds[reward.RewardItemName]),
                ("$reward_tier", reward.RewardTier),
                ("$source_rarity_label", reward.SourceRarityLabel),
                ("$source_chance_percent", reward.SourceChancePercent),
                ("$source_order", reward.SourceOrder));
        }

        var missionSourceIds = new Dictionary<MissionSourceKey, long>();
        foreach (var source in import.MissionSources)
        {
            var id = await InsertAndReturnIdAsync(
                connection,
                transaction,
                "INSERT INTO mission_sources (display_label, region, node, mission_type, qualifier, source_order) VALUES ($display_label, $region, $node, $mission_type, $qualifier, $source_order);",
                cancellationToken,
                ("$display_label", source.DisplayLabel),
                ("$region", (object?)source.Region ?? DBNull.Value),
                ("$node", (object?)source.Node ?? DBNull.Value),
                ("$mission_type", (object?)source.MissionType ?? DBNull.Value),
                ("$qualifier", (object?)source.Qualifier ?? DBNull.Value),
                ("$source_order", source.SourceOrder));
            missionSourceIds[MissionSourceKey.From(source.DisplayLabel, source.SourceOrder)] = id;
        }

        var rotationIds = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var rotation in import.RewardRotations)
        {
            var missionSourceId = missionSourceIds[MissionSourceKey.From(rotation.MissionSourceLabel, rotation.MissionSourceOrder)];
            var id = await InsertAndReturnIdAsync(
                connection,
                transaction,
                "INSERT INTO reward_rotations (mission_source_id, rotation_label, display_label, source_order) VALUES ($mission_source_id, $rotation_label, $display_label, $source_order);",
                cancellationToken,
                ("$mission_source_id", missionSourceId),
                ("$rotation_label", rotation.RotationLabel),
                ("$display_label", rotation.DisplayLabel),
                ("$source_order", rotation.SourceOrder));
            rotationIds[RotationKey(rotation.MissionSourceLabel, rotation.MissionSourceOrder, rotation.RotationLabel)] = id;
        }

        foreach (var reward in import.MissionRewards)
        {
            var rotationId = reward.RotationLabel is null
                ? DBNull.Value
                : (object)rotationIds[RotationKey(reward.MissionSourceLabel, reward.MissionSourceOrder, reward.RotationLabel)];

            var relicId = reward.RelicDisplayName is null
                ? DBNull.Value
                : (object)relicIds[reward.RelicDisplayName];

            await ExecuteNonQueryAsync(
                connection,
                transaction,
                "INSERT INTO mission_rewards (mission_source_id, rotation_id, reward_item_id, relic_id, rarity_label, chance_percent, source_order) VALUES ($mission_source_id, $rotation_id, $reward_item_id, $relic_id, $rarity_label, $chance_percent, $source_order);",
                cancellationToken,
                ("$mission_source_id", missionSourceIds[MissionSourceKey.From(reward.MissionSourceLabel, reward.MissionSourceOrder)]),
                ("$rotation_id", rotationId),
                ("$reward_item_id", rewardItemIds[reward.RewardItemName]),
                ("$relic_id", relicId),
                ("$rarity_label", reward.RarityLabel),
                ("$chance_percent", reward.ChancePercent),
                ("$source_order", reward.SourceOrder));
        }
    }

    private static async Task<long> InsertAndReturnIdAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        CancellationToken cancellationToken,
        params (string Name, object? Value)[] parameters)
    {
        await ExecuteNonQueryAsync(connection, transaction, sql, cancellationToken, parameters);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT last_insert_rowid();";
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(value);
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        CancellationToken cancellationToken,
        params (string Name, object? Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> RewardItemsTableContainsObsoletePrimePartColumnAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "PRAGMA table_info('reward_items');";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), "is_prime_part", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<bool> MissionSourcesTableHasUniqueDisplayLabelAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "PRAGMA index_list('mission_sources');";

        var indexNames = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var isUnique = reader.GetInt32(2) == 1;
                if (isUnique)
                {
                    indexNames.Add(reader.GetString(1));
                }
            }
        }

        foreach (var indexName in indexNames)
        {
            await using var indexCommand = connection.CreateCommand();
            indexCommand.Transaction = transaction;
            indexCommand.CommandText = $"PRAGMA index_info('{indexName.Replace("'", "''")}');";

            var indexedColumns = new List<string>();
            await using var indexReader = await indexCommand.ExecuteReaderAsync(cancellationToken);
            while (await indexReader.ReadAsync(cancellationToken))
            {
                indexedColumns.Add(indexReader.GetString(2));
            }

            if (indexedColumns.Count == 1
                && string.Equals(indexedColumns[0], "display_label", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string RotationKey(string missionSourceLabel, int missionSourceOrder, string rotationLabel)
    {
        return $"{missionSourceLabel}|{missionSourceOrder}|{rotationLabel}";
    }

    private readonly record struct MissionSourceKey(string DisplayLabel, int SourceOrder)
    {
        public static MissionSourceKey From(string displayLabel, int sourceOrder)
        {
            return new MissionSourceKey(displayLabel.ToUpperInvariant(), sourceOrder);
        }
    }
}
