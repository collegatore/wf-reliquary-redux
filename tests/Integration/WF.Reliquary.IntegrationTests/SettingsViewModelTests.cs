using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class SettingsViewModelTests
{
    private static UserSettings DefaultSettings { get; } = new(
        SourceUrl: new Uri("https://example.test/drops.html"),
        WindowWidth: 900,
        WindowHeight: 600,
        FontSize: UserFontSize.Medium,
        RelicsPanelHeight: null);

    [Test]
    public void HasUnsavedChanges_IsFalse_OnConstruction()
    {
        var vm = CreateViewModel(DefaultSettings);

        Assert.That(vm.HasUnsavedChanges, Is.False);
    }

    [Test]
    public void HasUnsavedChanges_IsTrue_WhenSourceUrlEdited()
    {
        var vm = CreateViewModel(DefaultSettings);

        vm.SourceUrlText = "https://changed.example/";

        Assert.That(vm.HasUnsavedChanges, Is.True);
    }

    [Test]
    public void HasUnsavedChanges_IsTrue_WhenFontSizeChanged()
    {
        var vm = CreateViewModel(DefaultSettings);

        vm.SelectedFontSize = FontSizeOption.Large;

        Assert.That(vm.HasUnsavedChanges, Is.True);
    }

    [Test]
    public void SyncRelicsPanelHeight_UpdatesRelicsPanelHeight_AndMarksUnsaved()
    {
        var vm = CreateViewModel(DefaultSettings);

        vm.SyncRelicsPanelHeight(250);

        Assert.Multiple(() =>
        {
            Assert.That(vm.RelicsPanelHeight, Is.EqualTo(250));
            Assert.That(vm.HasUnsavedChanges, Is.True);
        });
    }

    [Test]
    public async Task BackAsync_WhenNoUnsavedChanges_NavigatesWithoutConfirm()
    {
        var vm = CreateViewModel(DefaultSettings);
        var confirmCalled = false;
        var navigatedBack = false;
        vm.ConfirmDiscard = () => { confirmCalled = true; return Task.FromResult(true); };
        vm.OnNavigateBack = () => { navigatedBack = true; return Task.CompletedTask; };

        await vm.BackCommand.ExecuteAsync();

        Assert.Multiple(() =>
        {
            Assert.That(confirmCalled, Is.False);
            Assert.That(navigatedBack, Is.True);
        });
    }

    [Test]
    public async Task BackAsync_WithUnsavedChanges_ConfirmsBeforeNavigating()
    {
        var vm = CreateViewModel(DefaultSettings);
        vm.SelectedFontSize = FontSizeOption.Small;
        var navigatedBack = false;
        vm.ConfirmDiscard = () => Task.FromResult(true);
        vm.OnNavigateBack = () => { navigatedBack = true; return Task.CompletedTask; };

        await vm.BackCommand.ExecuteAsync();

        Assert.That(navigatedBack, Is.True);
    }

    [Test]
    public async Task BackAsync_WithUnsavedChanges_WhenUserCancels_StaysOnSettings()
    {
        var vm = CreateViewModel(DefaultSettings);
        vm.SelectedFontSize = FontSizeOption.Small;
        var navigatedBack = false;
        vm.ConfirmDiscard = () => Task.FromResult(false);
        vm.OnNavigateBack = () => { navigatedBack = true; return Task.CompletedTask; };

        await vm.BackCommand.ExecuteAsync();

        Assert.That(navigatedBack, Is.False);
    }

    [Test]
    public async Task BackAsync_WithUnsavedChanges_WhenUserDiscards_RollsBackEdits()
    {
        var vm = CreateViewModel(DefaultSettings);
        vm.SelectedFontSize = FontSizeOption.Large;
        vm.ConfirmDiscard = () => Task.FromResult(true);
        vm.OnNavigateBack = () => Task.CompletedTask;

        await vm.BackCommand.ExecuteAsync();

        Assert.Multiple(() =>
        {
            Assert.That(vm.SelectedFontSize, Is.EqualTo(FontSizeOption.Medium));
            Assert.That(vm.HasUnsavedChanges, Is.False);
        });
    }

    [Test]
    public async Task SaveAsync_WithInvalidUrl_SetsValidationError_DoesNotSave()
    {
        var store = new Mock<IUserSettingsStore>();
        var vm = CreateViewModel(DefaultSettings, store.Object);
        vm.SourceUrlText = "/relative/path";

        await vm.SaveCommand.ExecuteAsync();

        Assert.Multiple(() =>
        {
            Assert.That(vm.UrlValidationError, Is.Not.Empty);
            store.Verify(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task SaveAsync_WithValidSettings_PersistsAndClearsUnsavedFlag()
    {
        var store = new Mock<IUserSettingsStore>();
        UserSettings? saved = null;
        store
            .Setup(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()))
            .Callback<UserSettings, CancellationToken>((s, _) => saved = s)
            .Returns(Task.CompletedTask);

        var vm = CreateViewModel(DefaultSettings, store.Object);
        vm.SelectedFontSize = FontSizeOption.Large;
        vm.OnSaved = _ => { };

        await vm.SaveCommand.ExecuteAsync();

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.FontSize, Is.EqualTo(UserFontSize.Large));
            Assert.That(vm.HasUnsavedChanges, Is.False);
        });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static SettingsViewModel CreateViewModel(UserSettings settings, IUserSettingsStore? store = null)
    {
        store ??= Mock.Of<IUserSettingsStore>();
        return new SettingsViewModel(store, settings);
    }
}

// Convenience to await a Toolkit async command with no parameter in tests.
file static class AsyncRelayCommandExtensions
{
    public static Task ExecuteAsync(this CommunityToolkit.Mvvm.Input.IAsyncRelayCommand command)
    {
        return command.ExecuteAsync(null);
    }
}
