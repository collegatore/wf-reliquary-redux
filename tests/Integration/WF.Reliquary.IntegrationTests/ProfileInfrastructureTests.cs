using NUnit.Framework;
using WF.Reliquary.Infrastructure.DropTables;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class ProfileInfrastructureTests
{
    // ── SeedDefaultAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task SeedDefaultAsync_CreatesDefaultProfile_WhenNoneExist()
    {
        var store = new SqliteProfileStore(TempDatabasePath());
        await store.SeedDefaultAsync();

        var all = await store.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(1));
        Assert.That(all[0].Name, Is.EqualTo("Default"));
    }

    [Test]
    public async Task SeedDefaultAsync_SetsDefaultAsActive()
    {
        var store = new SqliteProfileStore(TempDatabasePath());
        await store.SeedDefaultAsync();

        var active = await store.GetActiveAsync();

        Assert.That(active, Is.Not.Null);
        Assert.That(active!.Name, Is.EqualTo("Default"));
    }

    [Test]
    public async Task SeedDefaultAsync_IsIdempotent_WhenCalledTwice()
    {
        var store = new SqliteProfileStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        await store.SeedDefaultAsync();

        var all = await store.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(1));
    }

    // ── CRUD ──────────────────────────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_AddsProfileToGetAll()
    {
        var store = new SqliteProfileStore(TempDatabasePath());
        await store.SeedDefaultAsync();

        var created = await store.CreateAsync("Alt Account");
        var all = await store.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(2));
        Assert.That(all.Any(p => p.Id == created.Id && p.Name == "Alt Account"), Is.True);
    }

    [Test]
    public async Task RenameAsync_ChangesProfileName()
    {
        var store = new SqliteProfileStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var all = await store.GetAllAsync();

        await store.RenameAsync(all[0].Id, "Main");

        var updated = await store.GetAllAsync();
        Assert.That(updated[0].Name, Is.EqualTo("Main"));
    }

    [Test]
    public async Task DeleteAsync_RemovesProfileFromGetAll()
    {
        var store = new SqliteProfileStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var extra = await store.CreateAsync("Extra");

        await store.DeleteAsync(extra.Id);

        var all = await store.GetAllAsync();
        Assert.That(all.Any(p => p.Id == extra.Id), Is.False);
    }

    [Test]
    public async Task DeleteAsync_CascadesWishlistsAndItems()
    {
        var dbPath = TempDatabasePath();
        var profileStore = new SqliteProfileStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        await profileStore.SeedDefaultAsync();
        await wishlistStore.SeedDefaultAsync();

        // Create a second profile and add a wishlist + item to it.
        var extra = await profileStore.CreateAsync("Extra");
        await profileStore.SetActiveAsync(extra.Id);
        var wishlist = await wishlistStore.CreateAsync("Extra List");
        await wishlistStore.AddItemAsync(wishlist.Id, "PART", "Gear", "Part");

        // Switch back, then delete the extra profile.
        var all = await profileStore.GetAllAsync();
        var defaultProfile = all.Single(p => p.Name == "Default");
        await profileStore.SetActiveAsync(defaultProfile.Id);
        await profileStore.DeleteAsync(extra.Id);

        // The extra profile's wishlists should be gone; only the Default profile's
        // wishlists remain visible.
        await profileStore.SetActiveAsync(defaultProfile.Id);
        var remaining = await wishlistStore.GetAllAsync();
        Assert.That(remaining.Any(w => w.Id == wishlist.Id), Is.False);
    }

    // ── GetActiveAsync / SetActiveAsync ───────────────────────────────────────

    [Test]
    public async Task SetActiveAsync_ChangesActiveProfile()
    {
        var store = new SqliteProfileStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var second = await store.CreateAsync("Second");

        await store.SetActiveAsync(second.Id);

        var active = await store.GetActiveAsync();
        Assert.That(active!.Id, Is.EqualTo(second.Id));
    }

    [Test]
    public async Task GetActiveAsync_FallsBackToFirst_WhenStoredIdIsMissing()
    {
        var dbPath = TempDatabasePath();
        var store = new SqliteProfileStore(dbPath);
        await store.SeedDefaultAsync();
        var second = await store.CreateAsync("Second");

        await store.SetActiveAsync(second.Id);
        await store.DeleteAsync(second.Id);

        var active = await store.GetActiveAsync();
        Assert.That(active, Is.Not.Null);
        Assert.That(active!.Name, Is.EqualTo("Default"));
    }

    // ── Per-profile active wishlist memory ────────────────────────────────────

    [Test]
    public async Task ActiveWishlist_IsRemembered_PerProfile()
    {
        var dbPath = TempDatabasePath();
        var profileStore = new SqliteProfileStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        await profileStore.SeedDefaultAsync();
        await wishlistStore.SeedDefaultAsync();

        var all = await profileStore.GetAllAsync();
        var defaultProfile = all.Single();

        // In Default profile, create a second list and make it active.
        var defaultSecond = await wishlistStore.CreateAsync("Default Second");
        await wishlistStore.SetActiveAsync(defaultSecond.Id);

        // Create and switch to a new profile; it gets its own Default wishlist.
        var alt = await profileStore.CreateAsync("Alt");
        await profileStore.SetActiveAsync(alt.Id);
        await wishlistStore.SeedDefaultAsync();
        var altActive = await wishlistStore.GetActiveAsync();

        // Switch back to the original profile: its active wishlist is restored.
        await profileStore.SetActiveAsync(defaultProfile.Id);
        var defaultActive = await wishlistStore.GetActiveAsync();

        Assert.That(altActive!.Name, Is.EqualTo("Default"));
        Assert.That(defaultActive!.Id, Is.EqualTo(defaultSecond.Id));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "import.sqlite");
    }
}
