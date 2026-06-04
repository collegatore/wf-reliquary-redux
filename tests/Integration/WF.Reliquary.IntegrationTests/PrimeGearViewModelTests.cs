using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.PrimeGear;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class PrimeGearViewModelTests
{
    private const long ActiveWishlistId = 1;

    private static readonly WishlistEntry ActiveWishlist =
        new(ActiveWishlistId, "Default", DateTimeOffset.UnixEpoch);

    // ── Merged display set ────────────────────────────────────────────────────

    [Test]
    public async Task LoadAsync_ShowsImportParts_WithCorrectCheckedState()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics", "Systems"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        var neuroptics = card.Parts.Single(p => p.PartName == "Neuroptics");
        var systems = card.Parts.Single(p => p.PartName == "Systems");

        Assert.Multiple(() =>
        {
            Assert.That(neuroptics.IsChecked, Is.True);
            Assert.That(neuroptics.IsGhost, Is.False);
            Assert.That(systems.IsChecked, Is.False);
            Assert.That(systems.IsGhost, Is.False);
        });
    }

    [Test]
    public async Task LoadAsync_IncludesGhostParts_FromWishlistNotInImport()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Systems"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: true)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        var ghost = card.Parts.Single(p => p.PartName == "Neuroptics");

        Assert.Multiple(() =>
        {
            Assert.That(ghost.IsGhost, Is.True);
            Assert.That(ghost.IsChecked, Is.True);
        });
    }

    [Test]
    public async Task LoadAsync_CreatesGhostOnlyCard_WhenGearAbsentFromImport()
    {
        var reader = ReaderWith([]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: true)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object);
        await SettleAsync();

        var card = vm.ActiveCards.SingleOrDefault(c => c.GearName == "Ash Prime");
        Assert.That(card, Is.Not.Null);
        Assert.That(card!.Parts.Single().IsGhost, Is.True);
    }

    // ── Check / uncheck calls store ───────────────────────────────────────────

    [Test]
    public async Task CheckingPart_CallsAddItemAsync_OnStore()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object);
        await SettleAsync();

        var part = vm.ActiveCards.Single().Parts.Single();
        part.IsChecked = true;
        await SettleAsync();

        store.Verify(s => s.AddItemAsync(
            ActiveWishlistId,
            It.Is<string>(n => n.Contains("NEUROPTICS", StringComparison.OrdinalIgnoreCase)),
            "Ash Prime",
            "Neuroptics",
            default), Times.Once);
    }

    [Test]
    public async Task UncheckingPart_CallsRemoveItemAsync_OnStore()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object);
        await SettleAsync();

        var part = vm.ActiveCards.Single().Parts.Single();
        part.IsChecked = false;
        await SettleAsync();

        store.Verify(s => s.RemoveItemAsync(
            ActiveWishlistId,
            It.Is<string>(n => n.Contains("NEUROPTICS", StringComparison.OrdinalIgnoreCase)),
            default), Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Mock<IPrimeGearReader> ReaderWith(IReadOnlyList<PrimeGearEntry> entries)
    {
        var mock = new Mock<IPrimeGearReader>();
        mock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(entries);
        return mock;
    }

    private static Mock<IWishlistStore> StoreWithItems(IReadOnlyList<WishlistItem> items)
    {
        var mock = new Mock<IWishlistStore>();
        mock.Setup(s => s.GetActiveAsync(default)).ReturnsAsync(ActiveWishlist);
        mock.Setup(s => s.GetItemsAsync(ActiveWishlistId, default)).ReturnsAsync(items);
        mock.Setup(s => s.AddItemAsync(
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        mock.Setup(s => s.RemoveItemAsync(It.IsAny<long>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static async Task SettleAsync()
    {
        await Task.Yield();
        await Task.Yield();
    }
}
