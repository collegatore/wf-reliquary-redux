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

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
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

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
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

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
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

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
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

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        var part = vm.ActiveCards.Single().Parts.Single();
        part.IsChecked = false;
        await SettleAsync();

        store.Verify(s => s.RemoveItemAsync(
            ActiveWishlistId,
            It.Is<string>(n => n.Contains("NEUROPTICS", StringComparison.OrdinalIgnoreCase)),
            default), Times.Once);
    }

    // ── Owned state & Show owned gear filtering ───────────────────────────────

    [Test]
    public async Task LoadAsync_ReflectsPersistedOwnedState_AndHidesOwnedByDefault()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Mag Prime", ["Systems"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]);
        var owned = OwnedStore("Ash Prime");

        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object);
        await SettleAsync();

        var ashCard = vm.ActiveCards.SingleOrDefault(c => c.GearName == "Ash Prime");
        Assert.That(ashCard, Is.Null, "owned gear is hidden by default");
        Assert.That(vm.ActiveCards.Any(c => c.GearName == "Mag Prime"), Is.True);
    }

    [Test]
    public async Task ShowOwned_RevealsAndHidesOwnedCards_InBothSections()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Boar Prime", ["Stock"], IsVaulted: true)
        ]);
        var store = StoreWithItems([]);
        var owned = OwnedStore("Ash Prime", "Boar Prime");

        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object);
        await SettleAsync();

        Assert.That(vm.ActiveCards.Any(c => c.GearName == "Ash Prime"), Is.False);
        Assert.That(vm.VaultedCards.Any(c => c.GearName == "Boar Prime"), Is.False);

        vm.ShowOwned = true;

        Assert.Multiple(() =>
        {
            Assert.That(vm.ActiveCards.Any(c => c.GearName == "Ash Prime"), Is.True);
            Assert.That(vm.VaultedCards.Any(c => c.GearName == "Boar Prime"), Is.True);
        });

        vm.ShowOwned = false;

        Assert.Multiple(() =>
        {
            Assert.That(vm.ActiveCards.Any(c => c.GearName == "Ash Prime"), Is.False);
            Assert.That(vm.VaultedCards.Any(c => c.GearName == "Boar Prime"), Is.False);
        });
    }

    [Test]
    public async Task MarkingOwned_WithNoWishedParts_PersistsAndRemovesFromView()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]);
        var owned = OwnedStore();

        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
        Assert.That(vm.ActiveCards.Any(c => c.GearName == "Ash Prime"), Is.False,
            "newly-owned card disappears while owned gear is hidden");
    }

    // ── Confirm-tracking dialog flow ──────────────────────────────────────────

    [Test]
    public async Task MarkingOwned_WithWishedParts_RequestsConfirmation()
    {
        var (vm, owned, _, confirmCalls) = MakeConfirmScenario(OwnedConfirmChoice.KeepTracking);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        Assert.That(confirmCalls, Has.Count.EqualTo(1));
        Assert.That(confirmCalls[0], Is.EqualTo("Ash Prime"));
        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
    }

    [Test]
    public async Task ConfirmKeepTracking_MarksOwned_LeavesWishlist()
    {
        var (vm, owned, store, _) = MakeConfirmScenario(OwnedConfirmChoice.KeepTracking);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        Assert.That(card.IsOwned, Is.True);
        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
        store.Verify(s => s.RemoveItemAsync(It.IsAny<long>(), It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task ConfirmReleaseTracking_MarksOwned_RemovesPartsFromActiveWishlist()
    {
        var (vm, owned, store, _) = MakeConfirmScenario(OwnedConfirmChoice.ReleaseTracking);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        Assert.That(card.IsOwned, Is.True);
        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
        store.Verify(s => s.RemoveItemAsync(
            ActiveWishlistId,
            It.Is<string>(n => n.Contains("NEUROPTICS", StringComparison.OrdinalIgnoreCase)),
            default), Times.Once);
    }

    [Test]
    public async Task ConfirmCancel_PersistsNothing_AndRevertsIsOwned()
    {
        var (vm, owned, store, _) = MakeConfirmScenario(OwnedConfirmChoice.Cancel);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        Assert.That(card.IsOwned, Is.False, "misclick reverts the owned mark");
        owned.Verify(s => s.SetOwnedAsync(It.IsAny<string>(), It.IsAny<bool>(), default), Times.Never);
        store.Verify(s => s.RemoveItemAsync(It.IsAny<long>(), It.IsAny<string>(), default), Times.Never);
        Assert.That(vm.ActiveCards.Any(c => c.GearName == "Ash Prime"), Is.True);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (PrimeGearViewModel Vm, Mock<IOwnedGearStore> Owned, Mock<IWishlistStore> Store, List<string> ConfirmCalls)
        MakeConfirmScenario(OwnedConfirmChoice choice)
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: false)
        ]);
        var owned = OwnedStore();

        var confirmCalls = new List<string>();
        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object)
        {
            ConfirmOwnedTracking = gearName =>
            {
                confirmCalls.Add(gearName);
                return Task.FromResult(choice);
            }
        };
        return (vm, owned, store, confirmCalls);
    }

    private static Mock<IOwnedGearStore> OwnedStore(params string[] ownedGearNames)
    {
        var mock = new Mock<IOwnedGearStore>();
        mock.Setup(s => s.GetOwnedGearNamesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ownedGearNames);
        mock.Setup(s => s.SetOwnedAsync(It.IsAny<string>(), It.IsAny<bool>(), default))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static Mock<IPrimeGearReader> ReaderWith(IReadOnlyList<PrimeGearEntry> entries)
    {
        var mock = new Mock<IPrimeGearReader>();
        mock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entries);
        return mock;
    }

    private static Mock<IWishlistStore> StoreWithItems(IReadOnlyList<WishlistItem> items)
    {
        var mock = new Mock<IWishlistStore>();
        mock.Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ActiveWishlist);
        mock.Setup(s => s.GetItemsAsync(ActiveWishlistId, It.IsAny<CancellationToken>())).ReturnsAsync(items);
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
