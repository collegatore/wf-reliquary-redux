using Microsoft.Data.Sqlite;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class DropTableImportInfrastructureTests
{
    [Test]
    public void Parser_ParsesFocusedRelicAndMissionFixture()
    {
        var parser = new HtmlDropTableParser();

        var import = parser.Parse(StoredSource(FocusedHtml()), DateTimeOffset.UnixEpoch);

        Assert.Multiple(() =>
        {
            Assert.That(import.Relics, Has.Count.EqualTo(1));
            Assert.That(import.Relics[0].DisplayName, Is.EqualTo("Lith C14 Relic"));
            Assert.That(import.RelicRewards, Has.Count.EqualTo(6));
            Assert.That(import.RelicRewards.Count(reward => reward.RewardTier == "Common"), Is.EqualTo(3));
            Assert.That(import.RelicRewards.Count(reward => reward.RewardTier == "Uncommon"), Is.EqualTo(2));
            Assert.That(import.RelicRewards.Count(reward => reward.RewardTier == "Rare"), Is.EqualTo(1));
            Assert.That(import.MissionSources.Single().Region, Is.EqualTo("Mercury"));
            Assert.That(import.MissionSources.Single().Node, Is.EqualTo("Apollodorus"));
            Assert.That(import.MissionSources.Single().MissionType, Is.EqualTo("Survival"));
            Assert.That(import.RewardRotations.Single().RotationLabel, Is.EqualTo("B"));
            Assert.That(import.MissionRewards.Any(reward => reward.RelicDisplayName == "Lith C14 Relic"), Is.True);
            Assert.That(import.MissionRewards.Any(reward => reward.RewardItemName == "100 Endo"), Is.True);
        });
    }

    [Test]
    public void Parser_PreservesAmbiguousMissionLabelAndDuplicateRows()
    {
        var parser = new HtmlDropTableParser();

        var import = parser.Parse(StoredSource(AmbiguousMissionHtml()), DateTimeOffset.UnixEpoch);

        Assert.Multiple(() =>
        {
            Assert.That(import.MissionSources.Single().DisplayLabel, Is.EqualTo("Odd Reward Pool"));
            Assert.That(import.MissionSources.Single().Region, Is.Null);
            Assert.That(import.MissionRewards.Count(reward => reward.RewardItemName == "400 Endo"), Is.EqualTo(2));
            Assert.That(import.Warnings.Any(warning => warning.Contains("Odd Reward Pool", StringComparison.Ordinal)), Is.True);
        });
    }

    [Test]
    public void Parser_WhenRequiredSectionsAreMissing_Fails()
    {
        var parser = new HtmlDropTableParser();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            parser.Parse(StoredSource("<html><h3 id=\"missionRewards\">Missions:</h3></html>"), DateTimeOffset.UnixEpoch));

        Assert.That(exception?.Message, Does.Contain("Relics section"));
    }

    [Test]
    public async Task SqliteStore_PersistsSnapshotAndProvenance()
    {
        var databasePath = TempDatabasePath();
        var store = new SqliteDropTableImportStore(databasePath);
        var parser = new HtmlDropTableParser();
        var import = parser.Parse(StoredSource(FocusedHtml()), DateTimeOffset.UnixEpoch);

        await store.ReplaceAsync(import);

        Assert.Multiple(() =>
        {
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM import_metadata;"), Is.EqualTo(1));
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM relics;"), Is.EqualTo(1));
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM relic_rewards;"), Is.EqualTo(6));
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM mission_sources;"), Is.EqualTo(1));
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM reward_rotations;"), Is.EqualTo(1));
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM mission_rewards;"), Is.EqualTo(2));
            Assert.That(ScalarString(databasePath, "SELECT source_last_update FROM import_metadata;"), Is.EqualTo("08 April, 2026"));
        });
    }

    [Test]
    public async Task SqliteStore_ReplacesSnapshotAtomically()
    {
        var databasePath = TempDatabasePath();
        var store = new SqliteDropTableImportStore(databasePath);
        var parser = new HtmlDropTableParser();

        await store.ReplaceAsync(parser.Parse(StoredSource(FocusedHtml()), DateTimeOffset.UnixEpoch));
        await store.ReplaceAsync(parser.Parse(StoredSource(TwoRelicHtml()), DateTimeOffset.UnixEpoch));

        Assert.Multiple(() =>
        {
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM relics;"), Is.EqualTo(2));
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM mission_sources;"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task SqliteStore_AllowsDuplicateMissionSourceDisplayLabels()
    {
        var databasePath = TempDatabasePath();
        var store = new SqliteDropTableImportStore(databasePath);
        var import = new ParsedDropTableImport(
            null,
            "stored.html",
            null,
            null,
            DateTimeOffset.UnixEpoch,
            [
                new ParsedRewardItem("Reward A", "REWARD A", "Other"),
                new ParsedRewardItem("Reward B", "REWARD B", "Other")
            ],
            [],
            [],
            [
                new ParsedMissionSource("Duplicate Pool", null, null, null, null, 0),
                new ParsedMissionSource("Duplicate Pool", null, null, null, null, 1)
            ],
            [
                new ParsedRewardRotation("Duplicate Pool", 0, "A", "Rotation A", 0),
                new ParsedRewardRotation("Duplicate Pool", 1, "A", "Rotation A", 1)
            ],
            [
                new ParsedMissionReward("Duplicate Pool", 0, "A", "Reward A", null, "Common", 50.00m, 0),
                new ParsedMissionReward("Duplicate Pool", 1, "A", "Reward B", null, "Rare", 7.69m, 1)
            ],
            []);

        await store.ReplaceAsync(import);

        Assert.Multiple(() =>
        {
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM mission_sources WHERE display_label = 'Duplicate Pool';"), Is.EqualTo(2));
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM reward_rotations WHERE rotation_label = 'A';"), Is.EqualTo(2));
            Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM mission_rewards;"), Is.EqualTo(2));
            Assert.That(ScalarLong(databasePath, """
                SELECT COUNT(*)
                FROM mission_rewards AS reward
                INNER JOIN mission_sources AS source ON source.id = reward.mission_source_id
                INNER JOIN reward_items AS item ON item.id = reward.reward_item_id
                WHERE source.source_order = 0 AND item.name = 'Reward A';
                """), Is.EqualTo(1));
            Assert.That(ScalarLong(databasePath, """
                SELECT COUNT(*)
                FROM mission_rewards AS reward
                INNER JOIN mission_sources AS source ON source.id = reward.mission_source_id
                INNER JOIN reward_items AS item ON item.id = reward.reward_item_id
                WHERE source.source_order = 1 AND item.name = 'Reward B';
                """), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task SqliteStore_WhenReplacementFails_KeepsPreviousSnapshot()
    {
        var databasePath = TempDatabasePath();
        var store = new SqliteDropTableImportStore(databasePath);
        var parser = new HtmlDropTableParser();

        await store.ReplaceAsync(parser.Parse(StoredSource(FocusedHtml()), DateTimeOffset.UnixEpoch));

        var invalidImport = new ParsedDropTableImport(
            null,
            "stored.html",
            null,
            null,
            DateTimeOffset.UnixEpoch,
            [],
            [new ParsedRelic("Lith", "C14", "Lith C14 Relic", "Lith C14 Relic")],
            [],
            [],
            [],
            [],
            []);

        Assert.ThrowsAsync<KeyNotFoundException>(() => store.ReplaceAsync(invalidImport));

        Assert.That(ScalarLong(databasePath, "SELECT COUNT(*) FROM relics;"), Is.EqualTo(1));
    }

    [Test]
    public void Parser_ImportsPreservedSampleAtIntegrationScale()
    {
        var parser = new HtmlDropTableParser();
        var samplePath = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "warframe-drop-tables-sample.html");
        var html = File.ReadAllText(samplePath);

        var import = parser.Parse(StoredSource(html, samplePath), DateTimeOffset.UnixEpoch);

        Assert.Multiple(() =>
        {
            Assert.That(import.Relics.Count, Is.GreaterThan(0));
            Assert.That(import.RelicRewards.Count, Is.GreaterThan(0));
            Assert.That(import.RelicRewards.Any(reward => reward.RewardItemName.Contains(" Prime ", StringComparison.Ordinal)), Is.True);
            Assert.That(import.MissionSources.Count, Is.GreaterThan(0));
            Assert.That(import.RewardRotations.Count, Is.GreaterThan(0));
            Assert.That(import.MissionRewards.Any(reward => reward.RelicDisplayName is not null), Is.True);
        });
    }

    [Test]
    public void Parser_PrimeNamedMissionOnlyReward_IsClassifiedAsOther()
    {
        var parser = new HtmlDropTableParser();
        var html = MissionOnlyPrimeHtml();

        var import = parser.Parse(StoredSource(html), DateTimeOffset.UnixEpoch);

        var sagek = import.RewardItems.Single(i => i.Name == "Sagek Prime");
        Assert.That(sagek.ItemKind, Is.EqualTo("Other"));
    }

    [Test]
    public void Parser_PrimeNamedItemInBothMissionAndRelicRewards_IsClassifiedAsPrimePart()
    {
        var parser = new HtmlDropTableParser();

        // Mission section appears before relic section in the HTML
        var import = parser.Parse(StoredSource(MissionAndRelicPrimeHtml()), DateTimeOffset.UnixEpoch);

        var item = import.RewardItems.Single(i => i.Name == "Lex Prime Barrel");
        Assert.That(item.ItemKind, Is.EqualTo("PrimePart"));
    }

    [Test]
    public void Parser_PrimeNamedRelicOnlyReward_IsClassifiedAsPrimePart()
    {
        var parser = new HtmlDropTableParser();

        var import = parser.Parse(StoredSource(FocusedHtml()), DateTimeOffset.UnixEpoch);

        var item = import.RewardItems.Single(i => i.Name == "Cedo Prime Blueprint");
        Assert.That(item.ItemKind, Is.EqualTo("PrimePart"));
    }

    private static StoredDropTableSource StoredSource(string html, string filePath = "stored.html")
    {
        return new StoredDropTableSource(
            filePath,
            html,
            new DropTableSourceProvenance(
                new Uri(DropTableSourceDefaults.SourceUrl),
                DateTimeOffset.UnixEpoch,
                "08 April, 2026"));
    }

    private static string FocusedHtml()
    {
        return """
            <html>
            <h3 id="missionRewards">Missions:</h3>
            <table>
            <tr><th colspan="2">Mercury/Apollodorus (Survival)</th></tr>
            <tr><th colspan="2">Rotation B</th></tr>
            <tr><td>100 Endo</td><td>Common (50.00%)</td></tr>
            <tr><td>Lith C14 Relic</td><td>Rare (7.69%)</td></tr>
            </table>
            <h3 id="relicRewards">Relics:</h3>
            <table>
            <tr><th colspan="2">Lith C14 Relic (Intact)</th></tr>
            <tr><td>Cedo Prime Blueprint</td><td>Rare (2.00%)</td></tr>
            <tr><td>Perigale Prime Receiver</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Lavos Prime Blueprint</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Kestrel Prime Blade</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Sevagoth Prime Systems Blueprint</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (25.33%)</td></tr>
            <tr><th colspan="2">Lith C14 Relic (Radiant)</th></tr>
            <tr><td>Cedo Prime Blueprint</td><td>Rare (10.00%)</td></tr>
            </table>
            </html>
            """;
    }

    private static string AmbiguousMissionHtml()
    {
        return FocusedHtml().Replace(
            """
            <tr><th colspan="2">Mercury/Apollodorus (Survival)</th></tr>
            <tr><th colspan="2">Rotation B</th></tr>
            <tr><td>100 Endo</td><td>Common (50.00%)</td></tr>
            <tr><td>Lith C14 Relic</td><td>Rare (7.69%)</td></tr>
            """,
            """
            <tr><th colspan="2">Odd Reward Pool</th></tr>
            <tr><td>400 Endo</td><td>Uncommon (12.65%)</td></tr>
            <tr><td>400 Endo</td><td>Uncommon (12.65%)</td></tr>
            """);
    }

    private static string TwoRelicHtml()
    {
        return """
            <html>
            <h3 id="missionRewards">Missions:</h3>
            <table>
            <tr><th colspan="2">Venus/Unda (Spy)</th></tr>
            <tr><td>Meso D8 Relic</td><td>Rare (7.69%)</td></tr>
            </table>
            <h3 id="relicRewards">Relics:</h3>
            <table>
            <tr><th colspan="2">Lith C14 Relic (Intact)</th></tr>
            <tr><td>Cedo Prime Blueprint</td><td>Rare (2.00%)</td></tr>
            <tr><td>Perigale Prime Receiver</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Lavos Prime Blueprint</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Kestrel Prime Blade</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Sevagoth Prime Systems Blueprint</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (25.33%)</td></tr>
            <tr><th colspan="2">Meso D8 Relic (Intact)</th></tr>
            <tr><td>Paris Prime Blueprint</td><td>Rare (2.00%)</td></tr>
            <tr><td>Paris Prime String</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Paris Prime Grip</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Lex Prime Receiver</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Lex Prime Barrel</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (25.33%)</td></tr>
            </table>
            </html>
            """;
    }

    private static string MissionOnlyPrimeHtml()
    {
        return """
            <html>
            <h3 id="missionRewards">Missions:</h3>
            <table>
            <tr><th colspan="2">Mercury/Apollodorus (Survival)</th></tr>
            <tr><td>Sagek Prime</td><td>Rare (7.69%)</td></tr>
            </table>
            <h3 id="relicRewards">Relics:</h3>
            <table>
            <tr><th colspan="2">Lith C14 Relic (Intact)</th></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Rare (2.00%)</td></tr>
            </table>
            </html>
            """;
    }

    private static string MissionAndRelicPrimeHtml()
    {
        return """
            <html>
            <h3 id="missionRewards">Missions:</h3>
            <table>
            <tr><th colspan="2">Venus/Unda (Spy)</th></tr>
            <tr><td>Lex Prime Barrel</td><td>Rare (7.69%)</td></tr>
            </table>
            <h3 id="relicRewards">Relics:</h3>
            <table>
            <tr><th colspan="2">Meso D8 Relic (Intact)</th></tr>
            <tr><td>Paris Prime Blueprint</td><td>Rare (2.00%)</td></tr>
            <tr><td>Paris Prime String</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Paris Prime Grip</td><td>Uncommon (11.00%)</td></tr>
            <tr><td>Lex Prime Receiver</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Lex Prime Barrel</td><td>Uncommon (25.33%)</td></tr>
            <tr><td>Forma Blueprint</td><td>Uncommon (25.33%)</td></tr>
            </table>
            </html>
            """;
    }

    private static string TempDatabasePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "import.sqlite");
    }

    private static long ScalarLong(string databasePath, string sql)
    {
        return Convert.ToInt64(Scalar(databasePath, sql));
    }

    private static string? ScalarString(string databasePath, string sql)
    {
        return Convert.ToString(Scalar(databasePath, sql));
    }

    private static object? Scalar(string databasePath, string sql)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = databasePath }.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }
}
