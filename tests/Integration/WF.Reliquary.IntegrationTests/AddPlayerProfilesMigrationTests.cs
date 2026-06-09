using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class AddPlayerProfilesMigrationTests
{
    [Test]
    public async Task Upgrade_AssignsExistingWishlistsToDefaultProfile_AndMovesActiveWishlist()
    {
        var dbPath = TempDatabasePath();
        var options = ReliquaryDbContext.BuildOptions(dbPath);

        // 1. Bring the database up to just the InitialSchema migration (pre-profiles).
        await using (var context = new ReliquaryDbContext(options))
        {
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync("InitialSchema");

            // 2. Seed legacy data: a wishlist and a user_preferences row pointing at it
            //    via the old last_wishlist_id column.
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO wishlists (id, name, created_at) VALUES (1, 'Legacy List', '2026-01-01T00:00:00Z');");
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO user_preferences (id, last_wishlist_id) VALUES (1, 1);");
        }

        // 3. Apply the AddPlayerProfiles migration.
        await using (var context = new ReliquaryDbContext(options))
        {
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync();
        }

        // 4. Verify via the stores that data landed correctly.
        var profileStore = new SqliteProfileStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        var activeProfile = await profileStore.GetActiveAsync();
        Assert.That(activeProfile, Is.Not.Null);
        Assert.That(activeProfile!.Name, Is.EqualTo("Default"));

        // The legacy wishlist must be visible in (and therefore scoped to) the Default profile.
        var lists = await wishlistStore.GetAllAsync();
        Assert.That(lists.Any(w => w.Id == 1 && w.Name == "Legacy List"), Is.True);

        // The previously-active wishlist (via last_wishlist_id) is now the profile's
        // active wishlist.
        var active = await wishlistStore.GetActiveAsync();
        Assert.That(active, Is.Not.Null);
        Assert.That(active!.Id, Is.EqualTo(1));
    }

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "import.sqlite");
    }
}
