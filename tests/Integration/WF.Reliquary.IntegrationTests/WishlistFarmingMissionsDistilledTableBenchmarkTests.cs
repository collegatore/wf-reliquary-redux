using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

/// <summary>
/// Distilled-table counterpart of <see cref="WishlistFarmingMissionsBenchmarkTests"/>. Replicates
/// the same worst-case rundown (full bundled drop table, maximal wishlist, warm-up, 25-iteration
/// median, EXPLAIN QUERY PLAN), but serves reward dedup from a physically materialized, indexed
/// <c>mission_rewards_distilled</c> table instead of the shipped <c>mission_rewards_merged</c> view —
/// so the two outputs can be compared apples-to-apples. The distilled table lives only in the test
/// database; no production code, schema, migration, or shipped query is touched.
///
/// Marked [Explicit] so it never runs in the normal suite — run on demand via:
///   dotnet test --filter "FullyQualifiedName~WishlistFarmingMissionsDistilledTableBenchmarkTests"
/// </summary>
[TestFixture]
[Explicit("Performance benchmark — run on demand, not in the normal suite.")]
public sealed class WishlistFarmingMissionsDistilledTableBenchmarkTests
{
    private const int Iterations = 25;

    /// <summary>
    /// Raw, un-deduplicated <c>mission_rewards</c> table (the pre-view baseline shape): the shipped
    /// query is swapped to read the raw table directly. No distilled table is created.
    /// </summary>
    [Test]
    public Task Rundown_MaximalWishlist_RawTable_Timing()
        => RunBenchmarkAsync("RAW mission_rewards TABLE", materializeDistilled: null, swapTarget: "mission_rewards");

    /// <summary>
    /// Shipped <c>mission_rewards_merged</c> view: the query is read from file and run unchanged,
    /// exactly as <see cref="SqliteWishlistFarmingMissionsReader"/> does. No distilled table is created.
    /// </summary>
    [Test]
    public Task Rundown_MaximalWishlist_MergedView_Timing()
        => RunBenchmarkAsync("mission_rewards_merged VIEW", materializeDistilled: null, swapTarget: null);

    /// <summary>
    /// Physically materialized <c>mission_rewards_distilled</c> table with the composite
    /// <c>(mission_source_id, rotation_id, relic_id)</c> index; the query is swapped to read it.
    /// </summary>
    [Test]
    public Task Rundown_MaximalWishlist_DistilledTableCompositeIndex_Timing()
        => RunBenchmarkAsync("DISTILLED TABLE (composite index)", MaterializeDistilledTableComposite, swapTarget: "mission_rewards_distilled");

    /// <summary>
    /// Physically materialized <c>mission_rewards_distilled</c> table with a single-column index on
    /// <c>mission_source_id</c> only — mirroring the raw table's <c>IX_mission_rewards_mission_source_id</c>.
    /// Isolates the index as the sole variable versus the composite-index variant; the prediction is
    /// it falls back to roughly raw-table speed because the planner can no longer push <c>relic_id</c>
    /// into the seek and must filter relic membership as a residual (one probe per mission×rotation).
    /// </summary>
    [Test]
    public Task Rundown_MaximalWishlist_DistilledTableSimpleIndex_Timing()
        => RunBenchmarkAsync("DISTILLED TABLE (simple index)", MaterializeDistilledTableSimple, swapTarget: "mission_rewards_distilled");

    /// <summary>
    /// One timing routine shared by all variants so they run under identical conditions: same import,
    /// same maximal wishlist, same warm-up, same 25-iteration median, same diagnostics. The only
    /// differences are whether (and how) the distilled table is materialized and which table/view the
    /// rundown query reads.
    /// </summary>
    /// <param name="label">Header label for the diagnostics block.</param>
    /// <param name="materializeDistilled">Builds + indexes the distilled table; null for variants that don't materialize it.</param>
    /// <param name="swapTarget">Table/view to swap <c>mission_rewards_merged</c> for; null reads the shipped query unchanged.</param>
    private async Task RunBenchmarkAsync(string label, Action<string>? materializeDistilled, string? swapTarget)
    {
        var dbPath = TempDatabasePath();

        // Seed a full-size import from the bundled real drop table (identical across variants).
        await SeedFullImportAsync(dbPath);

        // Distilled table is materialized for its variants only — every other variant runs against the
        // import exactly as produced, so the comparison is on equal conditions.
        long? distilledRows = null;
        if (materializeDistilled is not null)
        {
            materializeDistilled(dbPath);
            distilledRows = ScalarLong(dbPath, "SELECT COUNT(*) FROM mission_rewards_distilled;");
            var mergedRows = ScalarLong(dbPath, "SELECT COUNT(*) FROM mission_rewards_merged;");
            Assert.That(distilledRows, Is.EqualTo(mergedRows),
                "Distilled table must produce exactly the same dedup rows as the mission_rewards_merged view.");
        }

        var wishlistStore = new SqliteWishlistStore(dbPath);
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();
        Assert.That(wishlist, Is.Not.Null);

        // Build the maximal wishlist (all non-vaulted parts) so the query hits its worst case.
        var wishlistItemCount = SeedMaximalWishlist(dbPath, wishlist!.Id);

        // Read the shipped query; swap the dedup source unless this variant reads the view as-is.
        var sql = SqlQueryLoader.Load("GetWishlistFarmingMissions.sql");
        if (swapTarget is not null)
        {
            sql = SwapMergedViewFor(sql, swapTarget);
        }

        var options = ReliquaryDbContext.BuildOptions(dbPath);

        // Warm up once (JIT, EF model build, connection open, page cache).
        var warm = await RunRundownAsync(options, sql, wishlist.Id);

        // Time several iterations of query-execute + BuildGroups, take the median.
        var samples = new List<double>(Iterations);
        for (var i = 0; i < Iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            await RunRundownAsync(options, sql, wishlist.Id);
            sw.Stop();
            samples.Add(sw.Elapsed.TotalMilliseconds);
        }

        var median = Median(samples);
        var min = samples.Min();
        var max = samples.Max();

        // Same diagnostic block for every variant so the outputs line up side by side.
        TestContext.Out.WriteLine($"=== Wishlist farming-missions rundown benchmark ({label}) ===");
        TestContext.Out.WriteLine($"mission_rewards rows      : {ScalarLong(dbPath, "SELECT COUNT(*) FROM mission_rewards;")}");
        TestContext.Out.WriteLine($"mission_rewards_distilled : {(distilledRows.HasValue ? distilledRows.Value.ToString() : "(not materialized)")}");
        TestContext.Out.WriteLine($"relics (total)            : {ScalarLong(dbPath, "SELECT COUNT(*) FROM relics;")}");
        TestContext.Out.WriteLine($"relics (non-vaulted)      : {ScalarLong(dbPath, "SELECT COUNT(*) FROM relics WHERE is_vaulted = 0;")}");
        TestContext.Out.WriteLine($"mission_sources           : {ScalarLong(dbPath, "SELECT COUNT(*) FROM mission_sources;")}");
        TestContext.Out.WriteLine($"wishlist items            : {wishlistItemCount}");
        TestContext.Out.WriteLine($"rundown groups            : {warm.Count}");
        TestContext.Out.WriteLine($"iterations                : {Iterations}");
        TestContext.Out.WriteLine($"median                    : {median:F3} ms");
        TestContext.Out.WriteLine($"min / max                 : {min:F3} / {max:F3} ms");
        TestContext.Out.WriteLine("");
        TestContext.Out.WriteLine($"=== EXPLAIN QUERY PLAN ({label}) ===");
        foreach (var line in ExplainQueryPlan(dbPath, sql, wishlist.Id))
        {
            TestContext.Out.WriteLine(line);
        }

        // The maximal wishlist must return missions.
        Assert.That(warm, Is.Not.Empty, "Maximal wishlist should return missions.");
    }

    // ── Distilled-table materialization ───────────────────────────────────────

    /// <summary>
    /// Distilled table with indexes matched to the rundown's access pattern: relic_id membership
    /// subqueries and the (mission_source_id, rotation_id, relic_id) join predicates.
    /// </summary>
    private static void MaterializeDistilledTableComposite(string dbPath)
    {
        using var connection = Open(dbPath);
        CreateDistilledTable(connection);
        Execute(connection, "CREATE INDEX ix_distilled_relic ON mission_rewards_distilled(relic_id);");
        Execute(connection, "CREATE INDEX ix_distilled_join ON mission_rewards_distilled(mission_source_id, rotation_id, relic_id);");
        Execute(connection, "ANALYZE mission_rewards_distilled;");
    }

    /// <summary>
    /// Distilled table with a single-column index on mission_source_id only — the same shape the raw
    /// table carries (IX_mission_rewards_mission_source_id). Without relic_id in an index the planner
    /// cannot push the relic_id IN (...) membership into the seek, so it does one probe per
    /// mission×rotation and filters relics as a residual, like the raw table.
    /// </summary>
    private static void MaterializeDistilledTableSimple(string dbPath)
    {
        using var connection = Open(dbPath);
        CreateDistilledTable(connection);
        Execute(connection, "CREATE INDEX ix_distilled_simple ON mission_rewards_distilled(mission_source_id);");
        Execute(connection, "ANALYZE mission_rewards_distilled;");
    }

    /// <summary>
    /// Builds <c>mission_rewards_distilled</c> from <c>mission_rewards</c> using the exact defining
    /// SELECT of the <c>mission_rewards_merged</c> view (so every variant dedups identically).
    /// Indexes are added by the caller after this bulk insert.
    /// </summary>
    private static void CreateDistilledTable(SqliteConnection connection)
    {
        Execute(connection, """
            CREATE TABLE mission_rewards_distilled AS
            SELECT
                mission_source_id,
                rotation_id,
                reward_item_id,
                relic_id,
                SUM(chance_percent) AS chance_percent,
                MIN(source_order)   AS source_order
            FROM mission_rewards
            GROUP BY mission_source_id, rotation_id, reward_item_id;
            """);
    }

    /// <summary>
    /// Rewrites the shipped query to read <paramref name="target"/> in place of the merged view, and
    /// fails loudly if the expected token is absent (e.g. a future rename) so we never silently
    /// benchmark the view by accident.
    /// </summary>
    private static string SwapMergedViewFor(string sql, string target)
    {
        const string from = "mission_rewards_merged";
        var occurrences = (sql.Length - sql.Replace(from, string.Empty).Length) / from.Length;
        Assert.That(occurrences, Is.GreaterThan(0),
            $"Expected the shipped query to reference '{from}'; nothing to swap — the benchmark would otherwise measure the view.");
        return sql.Replace(from, target);
    }

    /// <summary>
    /// Mirrors the timed work of <see cref="SqliteWishlistFarmingMissionsReader.GetMissionsForWishlistAsync"/>:
    /// execute the rundown query and shape the rows through the same grouping path the live reader uses.
    /// </summary>
    private static async Task<IReadOnlyList<WishlistFarmingMissionGroupEntry>> RunRundownAsync(
        DbContextOptions<ReliquaryDbContext> options, string sql, long wishlistId)
    {
        await using var context = new ReliquaryDbContext(options);
        var rows = await context.Database
            .SqlQueryRaw<SqliteWishlistFarmingMissionsReader.FarmingRow>(
                sql, new SqliteParameter("@wishlist_id", wishlistId))
            .ToListAsync();
        return SqliteWishlistFarmingMissionsReader.BuildGroups(rows);
    }

    // ── Shared setup (identical to WishlistFarmingMissionsBenchmarkTests) ──────

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

    private static List<string> ExplainQueryPlan(string dbPath, string sql, long wishlistId)
    {
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

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
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
