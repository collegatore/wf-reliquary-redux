using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistViewModelTests
{
    private static readonly WishlistEntry DefaultEntry =
        new(1, "Default", DateTimeOffset.UnixEpoch);

    private static readonly WishlistEntry SecondEntry =
        new(2, "Second", DateTimeOffset.UnixEpoch.AddSeconds(1));

    // ── CanDelete ─────────────────────────────────────────────────────────────

    [Test]
    public async Task CanDelete_IsFalse_WhenOnlyOneWishlistExists()
    {
        var store = StoreWithWishlists([DefaultEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);

        await SettleAsync(vm);

        Assert.That(vm.CanDelete, Is.False);
    }

    [Test]
    public async Task CanDelete_IsTrue_WhenMultipleWishlistsExist()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);

        await SettleAsync(vm);

        Assert.That(vm.CanDelete, Is.True);
    }

    // ── SelectCommand ─────────────────────────────────────────────────────────

    [Test]
    public async Task SelectAsync_ChangesActiveEntry()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);

        var second = vm.Wishlists.Single(w => w.Id == SecondEntry.Id);
        await vm.SelectAsync(second);

        Assert.That(vm.ActiveEntry, Is.SameAs(second));
    }

    [Test]
    public async Task SelectAsync_CallsSetActiveAsync_OnStore()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);

        var second = vm.Wishlists.Single(w => w.Id == SecondEntry.Id);
        await vm.SelectAsync(second);

        store.Verify(s => s.SetActiveAsync(SecondEntry.Id, default), Times.Once);
    }

    [Test]
    public async Task SelectAsync_InvokesOnActiveWishlistChanged()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);
        var changed = false;
        vm.OnActiveWishlistChanged = () => { changed = true; return Task.CompletedTask; };

        var second = vm.Wishlists.Single(w => w.Id == SecondEntry.Id);
        await vm.SelectAsync(second);

        Assert.That(changed, Is.True);
    }

    // ── CreateCommand ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateCommand_AddsWishlistToCollection()
    {
        var newEntry = new WishlistEntry(3, "New List", DateTimeOffset.UtcNow);
        var store = StoreWithWishlists([DefaultEntry], DefaultEntry);
        store.Setup(s => s.CreateAsync("New List", default)).ReturnsAsync(newEntry);
        store.Setup(s => s.SetActiveAsync(newEntry.Id, default)).Returns(Task.CompletedTask);

        var vm = new WishlistViewModel(store.Object, _ => Task.FromResult<string?>("New List"));
        await SettleAsync(vm);

        vm.CreateCommand.Execute(null);
        await SettleAsync(vm);

        Assert.That(vm.Wishlists.Any(w => w.Name == "New List"), Is.True);
    }

    [Test]
    public async Task CreateCommand_DoesNothing_WhenPromptIsCancelled()
    {
        var store = StoreWithWishlists([DefaultEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);

        vm.CreateCommand.Execute(null);
        await SettleAsync(vm);

        store.Verify(s => s.CreateAsync(It.IsAny<string>(), default), Times.Never);
    }

    // ── DeleteCommand ─────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteCommand_RemovesWishlistFromCollection()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], SecondEntry);
        store.Setup(s => s.SetActiveAsync(DefaultEntry.Id, default)).Returns(Task.CompletedTask);
        store.Setup(s => s.DeleteAsync(SecondEntry.Id, default)).Returns(Task.CompletedTask);

        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);

        vm.DeleteCommand.Execute(null);
        await SettleAsync(vm);

        Assert.That(vm.Wishlists.Any(w => w.Id == SecondEntry.Id), Is.False);
    }

    [Test]
    public async Task DeleteCommand_CannotExecute_WhenOnlyOneWishlistRemains()
    {
        var store = StoreWithWishlists([DefaultEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);

        Assert.That(vm.DeleteCommand.CanExecute(null), Is.False);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Mock<IWishlistStore> StoreWithWishlists(
        IReadOnlyList<WishlistEntry> all,
        WishlistEntry active)
    {
        var mock = new Mock<IWishlistStore>();
        mock.Setup(s => s.GetAllAsync(default)).ReturnsAsync(all);
        mock.Setup(s => s.GetActiveAsync(default)).ReturnsAsync(active);
        mock.Setup(s => s.SetActiveAsync(It.IsAny<long>(), default)).Returns(Task.CompletedTask);
        mock.Setup(s => s.DeleteAsync(It.IsAny<long>(), default)).Returns(Task.CompletedTask);
        mock.Setup(s => s.RenameAsync(It.IsAny<long>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Yields briefly to allow fire-and-forget Tasks launched in constructors to settle.
    /// </summary>
    private static async Task SettleAsync(WishlistViewModel vm)
    {
        // Two yields are enough to let the initial LoadAsync Task progress through
        // its awaits in a cooperative scheduler environment.
        await Task.Yield();
        await Task.Yield();
        _ = vm; // suppress warning
    }

    private static Task<string?> NullPrompt(string? _) => Task.FromResult<string?>(null);
}
