using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class AddOwnedGearMigrationTests
{
    [Test]
    public async Task Migration_CreatesOwnedGearTable_WithUniqueIndexAndCascade()
    {
        var dbPath = TempDatabasePath();
        var options = ReliquaryDbContext.BuildOptions(dbPath);

        // Apply all migrations (including AddOwnedGear) and seed a profile.
        var profileStore = new SqliteProfileStore(dbPath);
        await profileStore.SeedDefaultAsync();

        await using var context = new ReliquaryDbContext(options);

        // Table exists and is queryable.
        var profileId = await context.Profiles.Select(p => p.Id).FirstAsync();

        // Unique index: inserting the same (profile_id, gear_name) twice fails.
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO owned_gear (profile_id, gear_name) VALUES ({0}, 'Ash Prime');", profileId);

        var ex = Assert.CatchAsync<SqliteException>(async () =>
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO owned_gear (profile_id, gear_name) VALUES ({0}, 'Ash Prime');", profileId));
        Assert.That(ex!.SqliteErrorCode, Is.EqualTo(19),
            "duplicate (profile_id, gear_name) must violate the unique index");

        // FK cascade: deleting the profile removes its owned_gear rows.
        await context.Database.ExecuteSqlRawAsync("DELETE FROM profiles WHERE id = {0};", profileId);
        var remaining = await context.OwnedGear.CountAsync(g => g.ProfileId == profileId);
        Assert.That(remaining, Is.EqualTo(0));
    }

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var dbPath = Path.Combine(dir, "import.sqlite");
        new SqliteDatabaseInitializer(dbPath).InitializeAsync().GetAwaiter().GetResult();
        return dbPath;
    }
}
