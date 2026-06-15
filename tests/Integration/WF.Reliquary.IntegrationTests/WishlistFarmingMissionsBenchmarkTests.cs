using System.Diagnostics;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

/// <summary>
/// Performance benchmark for the wishlist farming-missions rundown query against the
/// real full-size drop table with the maximal wishlist (every non-vaulted part selected).
/// Marked [Explicit] so it never runs in the normal suite — run on demand via:
///   dotnet test --filter "FullyQualifiedName~WishlistFarmingMissionsBenchmarkTests"
/// </summary>
[TestFixture]
[Explicit("Performance benchmark — run on demand, not in the normal suite.")]
public sealed class WishlistFarmingMissionsBenchmarkTests
{
    private const int Iterations = 25;

    [Test]
    public async Task Rundown_MaximalWishlist_Timing()
    {
        var dbPath = TempDatabasePath();

        // 1.1 — seed a full-size import from the bundled real drop table.
        await SeedFullImportAsync(dbPath);

        var wishlistStore = new SqliteWishlistStore(dbPath);
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();
        Assert.That(wishlist, Is.Not.Null);

        // 1.2 — build the maximal wishlist (all non-vaulted parts).
        var wishlistItemCount = SeedMaximalWishlist(dbPath, wishlist!.Id);

        var reader = new SqliteWishlistFarmingMissionsReader(dbPath);

        // Warm up once (JIT, EF model build, connection open, page cache).
        var warm = await reader.GetMissionsForWishlistAsync(wishlist.Id);

        // 1.3 — time several iterations, take the median.
        var samples = new List<double>(Iterations);
        for (var i = 0; i < Iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            await reader.GetMissionsForWishlistAsync(wishlist.Id);
            sw.Stop();
            samples.Add(sw.Elapsed.TotalMilliseconds);
        }

        var median = Median(samples);
        var min = samples.Min();
        var max = samples.Max();

        TestContext.Out.WriteLine("=== Wishlist farming-missions rundown benchmark ===");
        TestContext.Out.WriteLine($"mission_rewards rows : {ScalarLong(dbPath, "SELECT COUNT(*) FROM mission_rewards;")}");
        TestContext.Out.WriteLine($"relics (total)       : {ScalarLong(dbPath, "SELECT COUNT(*) FROM relics;")}");
        TestContext.Out.WriteLine($"relics (non-vaulted) : {ScalarLong(dbPath, "SELECT COUNT(*) FROM relics WHERE is_vaulted = 0;")}");
        TestContext.Out.WriteLine($"mission_sources      : {ScalarLong(dbPath, "SELECT COUNT(*) FROM mission_sources;")}");
        TestContext.Out.WriteLine($"wishlist items       : {wishlistItemCount}");
        TestContext.Out.WriteLine($"rundown groups       : {warm.Count}");
        TestContext.Out.WriteLine($"iterations           : {Iterations}");
        TestContext.Out.WriteLine($"median               : {median:F3} ms");
        TestContext.Out.WriteLine($"min / max            : {min:F3} / {max:F3} ms");
        TestContext.Out.WriteLine("");
        TestContext.Out.WriteLine("=== EXPLAIN QUERY PLAN ===");
        foreach (var line in ExplainQueryPlan(dbPath, wishlist.Id))
        {
            TestContext.Out.WriteLine(line);
        }

        Assert.That(warm, Is.Not.Empty, "Maximal wishlist should return missions.");
    }

    private static async Task SeedFullImportAsync(string dbPath)
    {
        var samplePath = Path.Combine(AppContext.BaseDirectory, "TestData", "warframe-drop-tables-sample.html");
        var html = await File.ReadAllTextAsync(samplePath);
        var source = new StoredDropTableSource(
            html,
            new DropTableSourceProvenance(
                new Uri(DropTableSourceDefaults.SourceUrl),
                DateTimeOffset.UnixEpoch,
                "08 April, 2026"));

        var import = new HtmlDropTableParser().Parse(source, DateTimeOffset.UnixEpoch);
        await new SqliteDropTableImportStore(dbPath).ReplaceAsync(import);
    }

    /// <summary>
    /// Inserts a wishlist item for every distinct part that is a relic reward of a
    /// non-vaulted relic, so the rundown query degenerates to its worst case.
    /// </summary>
    private static int SeedMaximalWishlist(string dbPath, long wishlistId)
    {
        using var connection = Open(dbPath);
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO wishlist_items (wishlist_id, part_normalized_name, gear_name, part_display_name, added_at)
            SELECT $wid, ri.normalized_name, '(benchmark)', ri.name, $now
            FROM reward_items ri
            JOIN relic_rewards rr ON rr.reward_item_id = ri.id
            JOIN relics r ON r.id = rr.relic_id
            WHERE r.is_vaulted = 0
            GROUP BY ri.normalized_name;
            """;
        command.Parameters.AddWithValue("$wid", wishlistId);
        command.Parameters.AddWithValue("$now", DateTimeOffset.UnixEpoch.ToString("O"));
        return command.ExecuteNonQuery();
    }

    private static List<string> ExplainQueryPlan(string dbPath, long wishlistId)
    {
        var sql = SqlQueryLoader.Load("GetWishlistFarmingMissions.sql");
        using var connection = Open(dbPath);
        using var command = connection.CreateCommand();
        command.CommandText = "EXPLAIN QUERY PLAN " + sql;
        command.Parameters.AddWithValue("@wishlist_id", wishlistId);

        var lines = new List<string>();
        using var read = command.ExecuteReader();
        while (read.Read())
        {
            // columns: id, parent, notused, detail
            lines.Add(read.GetString(read.GetOrdinal("detail")));
        }
        return lines;
    }

    private static double Median(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted[mid]
            : (sorted[mid - 1] + sorted[mid]) / 2.0;
    }

    private static long ScalarLong(string dbPath, string sql)
    {
        using var connection = Open(dbPath);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static SqliteConnection Open(string dbPath)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = dbPath }.ConnectionString);
        connection.Open();
        return connection;
    }

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "import.sqlite");
    }
}
