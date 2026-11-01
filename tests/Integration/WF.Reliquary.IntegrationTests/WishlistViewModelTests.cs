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

    // ── SelectAsync / SelectedEntry ───────────────────────────────────────────

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

    [Test]
    public async Task SelectedEntry_Set_ActivatesAndPersistsEntry()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);

        var second = vm.Wishlists.Single(w => w.Id == SecondEntry.Id);
        vm.SelectedEntry = second;
        await SettleAsync(vm);

        Assert.That(vm.ActiveEntry, Is.SameAs(second));
        Assert.That(vm.SelectedEntry, Is.SameAs(second));
        store.Verify(s => s.SetActiveAsync(SecondEntry.Id, default), Times.Once);
    }

    [Test]
    public async Task SelectedEntry_SetNull_IsIgnored()
    {
        // The selector pushes null back while ReloadAsync clears the list.
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);
        var active = vm.ActiveEntry;

        vm.SelectedEntry = null;
        await SettleAsync(vm);

        Assert.That(vm.ActiveEntry, Is.SameAs(active));
        store.Verify(s => s.SetActiveAsync(It.IsAny<long>(), default), Times.Never);
    }

    [Test]
    public async Task SelectedEntry_SetToActiveEntry_DoesNotPersist()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);

        vm.SelectedEntry = vm.ActiveEntry;
        await SettleAsync(vm);

        store.Verify(s => s.SetActiveAsync(It.IsAny<long>(), default), Times.Never);
    }

    [Test]
    public async Task ReloadAsync_UpdatesSelectedEntry_WithoutPersisting()
    {
        // A profile switch reloads the list; that must not write the active wishlist back.
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, NullPrompt);
        await SettleAsync(vm);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        store.Setup(s => s.GetActiveAsync(default)).ReturnsAsync(SecondEntry);

        await vm.ReloadAsync();

        Assert.That(vm.SelectedEntry!.Id, Is.EqualTo(SecondEntry.Id));
        Assert.That(changed, Does.Contain(nameof(WishlistViewModel.SelectedEntry)));
        store.Verify(s => s.SetActiveAsync(It.IsAny<long>(), default), Times.Never);
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

    [Test]
    public async Task CreateCommand_DoesNothing_WhenNameDuplicatesExistingWishlist()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        // A duplicate name the dialog should have blocked; the view-model guard must
        // ignore it (case-insensitive match against "Second").
        var vm = new WishlistViewModel(store.Object, _ => Task.FromResult<string?>("second"));
        await SettleAsync(vm);

        vm.CreateCommand.Execute(null);
        await SettleAsync(vm);

        store.Verify(s => s.CreateAsync(It.IsAny<string>(), default), Times.Never);
        Assert.That(vm.Wishlists, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task CreateCommand_AddsWishlist_WhenNameNotUsedInProfile()
    {
        // Taken names are scoped to the active profile's list, so a name not present
        // here is allowed even if another profile uses it.
        var newEntry = new WishlistEntry(3, "Second", DateTimeOffset.UtcNow);
        var store = StoreWithWishlists([DefaultEntry], DefaultEntry);
        store.Setup(s => s.CreateAsync("Second", default)).ReturnsAsync(newEntry);

        var vm = new WishlistViewModel(store.Object, _ => Task.FromResult<string?>("Second"));
        await SettleAsync(vm);

        vm.CreateCommand.Execute(null);
        await SettleAsync(vm);

        Assert.That(vm.Wishlists.Any(w => w.Name == "Second"), Is.True);
    }

    // ── RenameCommand ─────────────────────────────────────────────────────────

    [Test]
    public async Task RenameCommand_AllowsRenamingToSameName()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        // Re-confirming the current name (taken names exclude the active entry) is a
        // valid no-op rename.
        var vm = new WishlistViewModel(store.Object, _ => Task.FromResult<string?>("Default"));
        await SettleAsync(vm);

        vm.RenameCommand.Execute(null);
        await SettleAsync(vm);

        store.Verify(s => s.RenameAsync(DefaultEntry.Id, "Default", default), Times.Once);
        Assert.That(vm.ActiveEntry!.Name, Is.EqualTo("Default"));
    }

    [Test]
    public async Task RenameCommand_DoesNothing_WhenNameDuplicatesAnotherWishlist()
    {
        var store = StoreWithWishlists([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new WishlistViewModel(store.Object, _ => Task.FromResult<string?>("Second"));
        await SettleAsync(vm);

        vm.RenameCommand.Execute(null);
        await SettleAsync(vm);

        store.Verify(s => s.RenameAsync(It.IsAny<long>(), It.IsAny<string>(), default), Times.Never);
        Assert.That(vm.ActiveEntry!.Name, Is.EqualTo("Default"));
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

    private static Task<string?> NullPrompt(NameDialogRequest _) => Task.FromResult<string?>(null);
}
