using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Profiles;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class ProfileViewModelTests
{
    private static readonly ProfileEntry DefaultEntry =
        new(1, "Default", DateTimeOffset.UnixEpoch);

    private static readonly ProfileEntry SecondEntry =
        new(2, "Second", DateTimeOffset.UnixEpoch.AddSeconds(1));

    // ── CanDelete ─────────────────────────────────────────────────────────────

    [Test]
    public async Task CanDelete_IsFalse_WhenOnlyOneProfileExists()
    {
        var store = StoreWithProfiles([DefaultEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, NullPrompt);

        await SettleAsync();

        Assert.That(vm.CanDelete, Is.False);
    }

    [Test]
    public async Task CanDelete_IsTrue_WhenMultipleProfilesExist()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, NullPrompt);

        await SettleAsync();

        Assert.That(vm.CanDelete, Is.True);
    }

    // ── SelectCommand ─────────────────────────────────────────────────────────

    [Test]
    public async Task SelectAsync_ChangesActiveEntry()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, NullPrompt);
        await SettleAsync();

        var second = vm.Profiles.Single(p => p.Id == SecondEntry.Id);
        await vm.SelectAsync(second);

        Assert.That(vm.ActiveEntry, Is.SameAs(second));
    }

    [Test]
    public async Task SelectAsync_CallsSetActiveAsync_OnStore()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, NullPrompt);
        await SettleAsync();

        var second = vm.Profiles.Single(p => p.Id == SecondEntry.Id);
        await vm.SelectAsync(second);

        store.Verify(s => s.SetActiveAsync(SecondEntry.Id, default), Times.Once);
    }

    [Test]
    public async Task SelectAsync_InvokesOnActiveProfileChanged()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, NullPrompt);
        await SettleAsync();
        var changed = false;
        vm.OnActiveProfileChanged = () => { changed = true; return Task.CompletedTask; };

        var second = vm.Profiles.Single(p => p.Id == SecondEntry.Id);
        await vm.SelectAsync(second);

        Assert.That(changed, Is.True);
    }

    // ── CreateCommand ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateCommand_AddsProfileToCollection()
    {
        var newEntry = new ProfileEntry(3, "Alt Account", DateTimeOffset.UtcNow);
        var store = StoreWithProfiles([DefaultEntry], DefaultEntry);
        store.Setup(s => s.CreateAsync("Alt Account", default)).ReturnsAsync(newEntry);
        store.Setup(s => s.SetActiveAsync(newEntry.Id, default)).Returns(Task.CompletedTask);

        var vm = new ProfileViewModel(store.Object, _ => Task.FromResult<string?>("Alt Account"));
        await SettleAsync();

        vm.CreateCommand.Execute(null);
        await SettleAsync();

        Assert.That(vm.Profiles.Any(p => p.Name == "Alt Account"), Is.True);
    }

    [Test]
    public async Task CreateCommand_SeedsDefaultWishlist_ForNewProfile()
    {
        var newEntry = new ProfileEntry(3, "Alt Account", DateTimeOffset.UtcNow);
        var store = StoreWithProfiles([DefaultEntry], DefaultEntry);
        store.Setup(s => s.CreateAsync("Alt Account", default)).ReturnsAsync(newEntry);

        var seeded = false;
        var vm = new ProfileViewModel(store.Object, _ => Task.FromResult<string?>("Alt Account"))
        {
            SeedDefaultWishlist = () => { seeded = true; return Task.CompletedTask; },
        };
        await SettleAsync();

        vm.CreateCommand.Execute(null);
        await SettleAsync();

        Assert.That(seeded, Is.True);
    }

    [Test]
    public async Task SelectAsync_DoesNotSeedDefaultWishlist_OnPlainSwitch()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, NullPrompt);
        await SettleAsync();
        var seeded = false;
        vm.SeedDefaultWishlist = () => { seeded = true; return Task.CompletedTask; };

        var second = vm.Profiles.Single(p => p.Id == SecondEntry.Id);
        await vm.SelectAsync(second);

        Assert.That(seeded, Is.False);
    }

    [Test]
    public async Task CreateCommand_DoesNothing_WhenPromptIsCancelled()
    {
        var store = StoreWithProfiles([DefaultEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, NullPrompt);
        await SettleAsync();

        vm.CreateCommand.Execute(null);
        await SettleAsync();

        store.Verify(s => s.CreateAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task CreateCommand_DoesNothing_WhenNameDuplicatesExistingProfile()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], DefaultEntry);
        // Duplicate (case-insensitive) of "Second"; the view-model guard must ignore it.
        var vm = new ProfileViewModel(store.Object, _ => Task.FromResult<string?>("SECOND"));
        await SettleAsync();

        vm.CreateCommand.Execute(null);
        await SettleAsync();

        store.Verify(s => s.CreateAsync(It.IsAny<string>(), default), Times.Never);
        Assert.That(vm.Profiles, Has.Count.EqualTo(2));
    }

    // ── RenameCommand ─────────────────────────────────────────────────────────

    [Test]
    public async Task RenameCommand_AllowsRenamingToSameName()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, _ => Task.FromResult<string?>("Default"));
        await SettleAsync();

        vm.RenameCommand.Execute(null);
        await SettleAsync();

        store.Verify(s => s.RenameAsync(DefaultEntry.Id, "Default", default), Times.Once);
        Assert.That(vm.ActiveEntry!.Name, Is.EqualTo("Default"));
    }

    [Test]
    public async Task RenameCommand_DoesNothing_WhenNameDuplicatesAnotherProfile()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, _ => Task.FromResult<string?>("Second"));
        await SettleAsync();

        vm.RenameCommand.Execute(null);
        await SettleAsync();

        store.Verify(s => s.RenameAsync(It.IsAny<long>(), It.IsAny<string>(), default), Times.Never);
        Assert.That(vm.ActiveEntry!.Name, Is.EqualTo("Default"));
    }

    // ── DeleteCommand ─────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteCommand_RemovesProfileFromCollection()
    {
        var store = StoreWithProfiles([DefaultEntry, SecondEntry], SecondEntry);
        store.Setup(s => s.SetActiveAsync(DefaultEntry.Id, default)).Returns(Task.CompletedTask);
        store.Setup(s => s.DeleteAsync(SecondEntry.Id, default)).Returns(Task.CompletedTask);

        var vm = new ProfileViewModel(store.Object, NullPrompt);
        await SettleAsync();

        vm.DeleteCommand.Execute(null);
        await SettleAsync();

        Assert.That(vm.Profiles.Any(p => p.Id == SecondEntry.Id), Is.False);
    }

    [Test]
    public async Task DeleteCommand_CannotExecute_WhenOnlyOneProfileRemains()
    {
        var store = StoreWithProfiles([DefaultEntry], DefaultEntry);
        var vm = new ProfileViewModel(store.Object, NullPrompt);
        await SettleAsync();

        Assert.That(vm.DeleteCommand.CanExecute(null), Is.False);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Mock<IProfileStore> StoreWithProfiles(
        IReadOnlyList<ProfileEntry> all,
        ProfileEntry active)
    {
        var mock = new Mock<IProfileStore>();
        mock.Setup(s => s.GetAllAsync(default)).ReturnsAsync(all);
        mock.Setup(s => s.GetActiveAsync(default)).ReturnsAsync(active);
        mock.Setup(s => s.SetActiveAsync(It.IsAny<long>(), default)).Returns(Task.CompletedTask);
        mock.Setup(s => s.DeleteAsync(It.IsAny<long>(), default)).Returns(Task.CompletedTask);
        mock.Setup(s => s.RenameAsync(It.IsAny<long>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);
        return mock;
    }

    private static async Task SettleAsync()
    {
        await Task.Yield();
        await Task.Yield();
    }

    private static Task<string?> NullPrompt(NameDialogRequest _) => Task.FromResult<string?>(null);
}
