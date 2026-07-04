using NUnit.Framework;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class OwnedGearInfrastructureTests
{
    [Test]
    public async Task SetOwnedAsync_MarksGear_VisibleInGetOwned()
    {
        var dbPath = TempDatabasePath();
        await SeedAsync(dbPath);
        var store = new SqliteOwnedGearStore(dbPath);

        await store.SetOwnedAsync("Ash Prime", true);

        var owned = await store.GetOwnedGearNamesAsync();
        Assert.That(owned, Does.Contain("Ash Prime"));
    }

    [Test]
    public async Task SetOwnedAsync_ClearsGear_RemovedFromGetOwned()
    {
        var dbPath = TempDatabasePath();
        await SeedAsync(dbPath);
        var store = new SqliteOwnedGearStore(dbPath);

        await store.SetOwnedAsync("Ash Prime", true);
        await store.SetOwnedAsync("Ash Prime", false);

        var owned = await store.GetOwnedGearNamesAsync();
        Assert.That(owned, Does.Not.Contain("Ash Prime"));
    }

    [Test]
    public async Task SetOwnedAsync_IsIdempotent_WhenMarkedTwice()
    {
        var dbPath = TempDatabasePath();
        await SeedAsync(dbPath);
        var store = new SqliteOwnedGearStore(dbPath);

        await store.SetOwnedAsync("Ash Prime", true);
        await store.SetOwnedAsync("Ash Prime", true);

        var owned = await store.GetOwnedGearNamesAsync();
        Assert.That(owned.Count(n => n == "Ash Prime"), Is.EqualTo(1));
    }

    [Test]
    public async Task OwnedGear_IsIsolated_PerProfile()
    {
        var dbPath = TempDatabasePath();
        var profileStore = new SqliteProfileStore(dbPath);
        await profileStore.SeedDefaultAsync();
        var ownedStore = new SqliteOwnedGearStore(dbPath);

        // Mark owned in the Default profile.
        await ownedStore.SetOwnedAsync("Ash Prime", true);

        // Switch to a second profile — its owned set must be empty.
        var second = await profileStore.CreateAsync("Second");
        await profileStore.SetActiveAsync(second.Id);

        var secondOwned = await ownedStore.GetOwnedGearNamesAsync();
        Assert.That(secondOwned, Does.Not.Contain("Ash Prime"));

        // Switch back — the original profile still owns it.
        var all = await profileStore.GetAllAsync();
        await profileStore.SetActiveAsync(all.Single(p => p.Name == "Default").Id);
        var defaultOwned = await ownedStore.GetOwnedGearNamesAsync();
        Assert.That(defaultOwned, Does.Contain("Ash Prime"));
    }

    [Test]
    public async Task DeletingProfile_CascadesOwnedGear()
    {
        var dbPath = TempDatabasePath();
        var profileStore = new SqliteProfileStore(dbPath);
        await profileStore.SeedDefaultAsync();
        var ownedStore = new SqliteOwnedGearStore(dbPath);

        var extra = await profileStore.CreateAsync("Extra");
        await profileStore.SetActiveAsync(extra.Id);
        await ownedStore.SetOwnedAsync("Mag Prime", true);

        var all = await profileStore.GetAllAsync();
        var defaultProfile = all.Single(p => p.Name == "Default");
        await profileStore.SetActiveAsync(defaultProfile.Id);
        await profileStore.DeleteAsync(extra.Id);

        // Reactivating the (now-deleted) extra falls back to Default, which never
        // owned Mag Prime — so the cascade removed the extra profile's owned rows.
        var remaining = await ownedStore.GetOwnedGearNamesAsync();
        Assert.That(remaining, Does.Not.Contain("Mag Prime"));
    }

    private static async Task SeedAsync(string dbPath)
    {
        var profileStore = new SqliteProfileStore(dbPath);
        await profileStore.SeedDefaultAsync();
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
