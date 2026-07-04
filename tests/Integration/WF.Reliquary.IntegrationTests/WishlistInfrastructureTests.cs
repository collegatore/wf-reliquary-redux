using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistInfrastructureTests
{
    // ── SeedDefaultAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task SeedDefaultAsync_CreatesDefaultWishlist_WhenNoneExist()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();

        var all = await store.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(1));
        Assert.That(all[0].Name, Is.EqualTo("Default"));
    }

    [Test]
    public async Task SeedDefaultAsync_SetsDefaultAsActive()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();

        var active = await store.GetActiveAsync();

        Assert.That(active, Is.Not.Null);
        Assert.That(active!.Name, Is.EqualTo("Default"));
    }

    [Test]
    public async Task SeedDefaultAsync_IsIdempotent_WhenCalledTwice()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        await store.SeedDefaultAsync();

        var all = await store.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(1));
    }

    // ── Schema coexistence with import ────────────────────────────────────────

    [Test]
    public async Task WishlistData_SurvivesDropTableReImport()
    {
        var dbPath = TempDatabasePath();
        var importStore = new SqliteDropTableImportStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        // First import creates the schema.
        await importStore.ReplaceAsync(EmptyImport());
        await wishlistStore.SeedDefaultAsync();

        var wishlist = await wishlistStore.GetActiveAsync();
        await wishlistStore.AddItemAsync(
            wishlist!.Id,
            "ASH PRIME NEUROPTICS",
            "Ash Prime",
            "Neuroptics");

        // Re-import wipes import data.
        await importStore.ReplaceAsync(EmptyImport());

        // Wishlist data must survive.
        var items = await wishlistStore.GetItemsAsync(wishlist.Id);
        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].PartNormalizedName, Is.EqualTo("ASH PRIME NEUROPTICS"));
    }

    // ── CRUD ──────────────────────────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_AddsWishlistToGetAll()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();

        var created = await store.CreateAsync("Farm Set A");
        var all = await store.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(2));
        Assert.That(all.Any(w => w.Id == created.Id && w.Name == "Farm Set A"), Is.True);
    }

    [Test]
    public async Task RenameAsync_ChangesWishlistName()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var all = await store.GetAllAsync();
        var id = all[0].Id;

        await store.RenameAsync(id, "My Farm List");

        var updated = await store.GetAllAsync();
        Assert.That(updated[0].Name, Is.EqualTo("My Farm List"));
    }

    [Test]
    public async Task DeleteAsync_RemovesWishlistFromGetAll()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var extra = await store.CreateAsync("Extra");

        await store.DeleteAsync(extra.Id);

        var all = await store.GetAllAsync();
        Assert.That(all.Any(w => w.Id == extra.Id), Is.False);
    }

    [Test]
    public async Task DeleteAsync_CascadesItems()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var wishlist = await store.GetActiveAsync();
        await store.AddItemAsync(wishlist!.Id, "PART", "Gear", "Part");

        var extra = await store.CreateAsync("Extra");
        await store.DeleteAsync(wishlist.Id);

        // Items for deleted list must be gone; extra list unaffected.
        var remaining = await store.GetAllAsync();
        Assert.That(remaining.Single().Id, Is.EqualTo(extra.Id));
    }

    // ── GetActiveAsync / SetActiveAsync ───────────────────────────────────────

    [Test]
    public async Task SetActiveAsync_ChangesActiveWishlist()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
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
        var store = new SqliteWishlistStore(dbPath);
        await store.SeedDefaultAsync();
        var second = await store.CreateAsync("Second");

        // Point preference at the second list, then delete it.
        await store.SetActiveAsync(second.Id);
        await store.DeleteAsync(second.Id);

        // Stored preference now points to a non-existent ID.
        var active = await store.GetActiveAsync();
        Assert.That(active, Is.Not.Null);
        Assert.That(active!.Name, Is.EqualTo("Default"));
    }

    // ── AddItemAsync / RemoveItemAsync ────────────────────────────────────────

    [Test]
    public async Task AddItemAsync_AppearsInGetItems()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var wishlist = await store.GetActiveAsync();

        await store.AddItemAsync(wishlist!.Id, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics");

        var items = await store.GetItemsAsync(wishlist.Id);
        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].PartNormalizedName, Is.EqualTo("ASH PRIME NEUROPTICS"));
        Assert.That(items[0].GearName, Is.EqualTo("Ash Prime"));
        Assert.That(items[0].PartDisplayName, Is.EqualTo("Neuroptics"));
    }

    [Test]
    public async Task AddItemAsync_IsIdempotent_WhenSamePartAddedTwice()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var wishlist = await store.GetActiveAsync();

        await store.AddItemAsync(wishlist!.Id, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics");
        await store.AddItemAsync(wishlist!.Id, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics");

        var items = await store.GetItemsAsync(wishlist.Id);
        Assert.That(items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task RemoveItemAsync_RemovesPartFromWishlist()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var wishlist = await store.GetActiveAsync();

        await store.AddItemAsync(wishlist!.Id, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics");
        await store.RemoveItemAsync(wishlist.Id, "ASH PRIME NEUROPTICS");

        var items = await store.GetItemsAsync(wishlist.Id);
        Assert.That(items, Is.Empty);
    }

    [Test]
    public async Task RemoveItemAsync_IsIdempotent_WhenPartNotPresent()
    {
        var store = new SqliteWishlistStore(TempDatabasePath());
        await store.SeedDefaultAsync();
        var wishlist = await store.GetActiveAsync();

        Assert.DoesNotThrowAsync(
            () => store.RemoveItemAsync(wishlist!.Id, "NONEXISTENT PART"));
    }

    // ── Ghost detection ───────────────────────────────────────────────────────

    [Test]
    public async Task GetItemsAsync_MarksGhost_WhenPartAbsentFromImport()
    {
        var dbPath = TempDatabasePath();
        var importStore = new SqliteDropTableImportStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        await importStore.ReplaceAsync(EmptyImport());
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();

        await wishlistStore.AddItemAsync(
            wishlist!.Id,
            "ASH PRIME NEUROPTICS",
            "Ash Prime",
            "Neuroptics");

        var items = await wishlistStore.GetItemsAsync(wishlist.Id);

        Assert.That(items[0].IsGhost, Is.True);
    }

    [Test]
    public async Task GetItemsAsync_DoesNotMarkGhost_WhenPartPresentInImport()
    {
        var dbPath = TempDatabasePath();
        var importStore = new SqliteDropTableImportStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        await importStore.ReplaceAsync(ImportWithPrimeParts([
            new ParsedRewardItem("Ash Prime Neuroptics", "ASH PRIME NEUROPTICS", "PrimePart"),
        ]));
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();

        await wishlistStore.AddItemAsync(
            wishlist!.Id,
            "ASH PRIME NEUROPTICS",
            "Ash Prime",
            "Neuroptics");

        var items = await wishlistStore.GetItemsAsync(wishlist.Id);

        Assert.That(items[0].IsGhost, Is.False);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var dbPath = Path.Combine(dir, "import.sqlite");
        new SqliteDatabaseInitializer(dbPath).InitializeAsync().GetAwaiter().GetResult();
        return dbPath;
    }

    private static ParsedDropTableImport EmptyImport() =>
        ImportWithPrimeParts([]);

    private static ParsedDropTableImport ImportWithPrimeParts(IReadOnlyList<ParsedRewardItem> items) =>
        new(new Uri(DropTableSourceDefaults.SourceUrl), DateTimeOffset.UnixEpoch, "08 April, 2026", DateTimeOffset.UnixEpoch, items, [], [], [], [], [], []);
}
