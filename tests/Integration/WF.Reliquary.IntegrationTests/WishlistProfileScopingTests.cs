using NUnit.Framework;
using WF.Reliquary.Infrastructure.DropTables;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistProfileScopingTests
{
    [Test]
    public async Task GetAllAsync_ReturnsOnlyActiveProfileWishlists()
    {
        var dbPath = TempDatabasePath();
        var profileStore = new SqliteProfileStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        await profileStore.SeedDefaultAsync();
        await wishlistStore.SeedDefaultAsync();
        var defaultProfile = (await profileStore.GetAllAsync()).Single();

        await wishlistStore.CreateAsync("Default-Only");

        // Second profile with its own wishlist.
        var alt = await profileStore.CreateAsync("Alt");
        await profileStore.SetActiveAsync(alt.Id);
        await wishlistStore.SeedDefaultAsync();
        await wishlistStore.CreateAsync("Alt-Only");

        var altLists = await wishlistStore.GetAllAsync();
        Assert.That(altLists.Any(w => w.Name == "Alt-Only"), Is.True);
        Assert.That(altLists.Any(w => w.Name == "Default-Only"), Is.False);

        await profileStore.SetActiveAsync(defaultProfile.Id);
        var defaultLists = await wishlistStore.GetAllAsync();
        Assert.That(defaultLists.Any(w => w.Name == "Default-Only"), Is.True);
        Assert.That(defaultLists.Any(w => w.Name == "Alt-Only"), Is.False);
    }

    [Test]
    public async Task CreateAsync_AddsWishlistToActiveProfile()
    {
        var dbPath = TempDatabasePath();
        var profileStore = new SqliteProfileStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        await profileStore.SeedDefaultAsync();
        await wishlistStore.SeedDefaultAsync();

        var alt = await profileStore.CreateAsync("Alt");
        await profileStore.SetActiveAsync(alt.Id);
        await wishlistStore.SeedDefaultAsync();

        var created = await wishlistStore.CreateAsync("Alt Farm");

        var all = await wishlistStore.GetAllAsync();
        Assert.That(all.Any(w => w.Id == created.Id), Is.True);
    }

    [Test]
    public async Task SwitchingProfile_ChangesActiveWishlist()
    {
        var dbPath = TempDatabasePath();
        var profileStore = new SqliteProfileStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        await profileStore.SeedDefaultAsync();
        await wishlistStore.SeedDefaultAsync();
        var defaultProfile = (await profileStore.GetAllAsync()).Single();
        var defaultActive = await wishlistStore.GetActiveAsync();

        var alt = await profileStore.CreateAsync("Alt");
        await profileStore.SetActiveAsync(alt.Id);
        await wishlistStore.SeedDefaultAsync();
        var altActive = await wishlistStore.GetActiveAsync();

        // The two profiles have distinct active wishlists.
        Assert.That(altActive!.Id, Is.Not.EqualTo(defaultActive!.Id));

        await profileStore.SetActiveAsync(defaultProfile.Id);
        var restored = await wishlistStore.GetActiveAsync();
        Assert.That(restored!.Id, Is.EqualTo(defaultActive.Id));
    }

    [Test]
    public async Task DeletingProfile_CascadesItsWishlistItems()
    {
        var dbPath = TempDatabasePath();
        var profileStore = new SqliteProfileStore(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);

        await profileStore.SeedDefaultAsync();
        await wishlistStore.SeedDefaultAsync();
        var defaultProfile = (await profileStore.GetAllAsync()).Single();

        var alt = await profileStore.CreateAsync("Alt");
        await profileStore.SetActiveAsync(alt.Id);
        await wishlistStore.SeedDefaultAsync();
        var altList = await wishlistStore.GetActiveAsync();
        await wishlistStore.AddItemAsync(altList!.Id, "PART", "Gear", "Part");

        await profileStore.SetActiveAsync(defaultProfile.Id);
        await profileStore.DeleteAsync(alt.Id);

        // The deleted profile's wishlist (and its items) are gone — querying items
        // on the now-deleted wishlist id returns nothing.
        var items = await wishlistStore.GetItemsAsync(altList.Id);
        Assert.That(items, Is.Empty);
    }

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "import.sqlite");
    }
}
