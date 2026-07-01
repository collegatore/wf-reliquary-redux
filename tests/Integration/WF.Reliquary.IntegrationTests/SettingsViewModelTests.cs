using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Lfg;
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
        FontSizePt: 14.0,
        RelicsPanelHeight: null,
        LfgTemplate: LfgMessageTemplate.Default);

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
    public void HasUnsavedChanges_IsFalse_WhenFontSizeChanged()
    {
        // Font size is auto-saved on selection; it does not enter the unsaved-changes guard.
        var vm = CreateViewModel(DefaultSettings);

        vm.SelectedFontSize = FontSizeOption.Large;

        Assert.That(vm.HasUnsavedChanges, Is.False);
    }

    [Test]
    public void HasUnsavedChanges_IsFalse_WhenRelicCardWidthChanged()
    {
        // Relic card width is auto-saved on change; it does not enter the unsaved-changes guard.
        var vm = CreateViewModel(DefaultSettings);

        vm.RelicCardWidthPx = 450;

        Assert.That(vm.HasUnsavedChanges, Is.False);
    }

    [Test]
    public void RelicCardWidthPx_ClampsBelowMinimum()
    {
        var vm = CreateViewModel(DefaultSettings);

        vm.RelicCardWidthPx = 50;

        Assert.That(vm.RelicCardWidthPx, Is.EqualTo(180.0));
    }

    [Test]
    public void RelicCardWidthPx_ClampsAboveMaximum()
    {
        var vm = CreateViewModel(DefaultSettings);

        vm.RelicCardWidthPx = 9000;

        Assert.That(vm.RelicCardWidthPx, Is.EqualTo(600.0));
    }

    [Test]
    public void SyncRelicsPanelHeight_UpdatesRelicsPanelHeight_DoesNotMarkUnsaved()
    {
        // The splitter cannot be adjusted from within Settings, so syncing the
        // live position on entry must never trigger the leave-guard dialog.
        var vm = CreateViewModel(DefaultSettings);

        vm.SyncRelicsPanelHeight(250);

        Assert.Multiple(() =>
        {
            Assert.That(vm.RelicsPanelHeight, Is.EqualTo(250));
            Assert.That(vm.HasUnsavedChanges, Is.False);
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
    public async Task BackAsync_WhenOnlyFontSizeChanged_NavigatesWithoutConfirm()
    {
        // Font size is auto-saved; changing it must not trigger the leave-guard.
        var vm = CreateViewModel(DefaultSettings);
        vm.SelectedFontSize = FontSizeOption.Small;
        var confirmCalled = false;
        vm.ConfirmDiscard = () => { confirmCalled = true; return Task.FromResult(true); };
        vm.OnNavigateBack = () => Task.CompletedTask;

        await vm.BackCommand.ExecuteAsync();

        Assert.That(confirmCalled, Is.False);
    }

    [Test]
    public async Task BackAsync_WhenOnlyRelicCardWidthChanged_NavigatesWithoutConfirm()
    {
        // Relic card width is auto-saved; changing it must not trigger the leave-guard.
        var vm = CreateViewModel(DefaultSettings);
        vm.RelicCardWidthPx = 450;
        var confirmCalled = false;
        vm.ConfirmDiscard = () => { confirmCalled = true; return Task.FromResult(true); };
        vm.OnNavigateBack = () => Task.CompletedTask;

        await vm.BackCommand.ExecuteAsync();

        Assert.That(confirmCalled, Is.False);
    }

    [Test]
    public async Task BackAsync_WithUnsavedChanges_ConfirmsBeforeNavigating()
    {
        var vm = CreateViewModel(DefaultSettings);
        vm.SourceUrlText = "https://changed.example/";
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
        vm.SourceUrlText = "https://changed.example/";
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
        vm.SourceUrlText = "https://changed.example/";
        vm.ConfirmDiscard = () => Task.FromResult(true);
        vm.OnNavigateBack = () => Task.CompletedTask;

        await vm.BackCommand.ExecuteAsync();

        Assert.Multiple(() =>
        {
            Assert.That(vm.SourceUrlText, Is.EqualTo(DefaultSettings.SourceUrl.ToString()));
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
    public async Task SaveAsync_WithValidSettings_PersistsCurrentFontSizeAndClearsUnsavedFlag()
    {
        var store = new Mock<IUserSettingsStore>();
        UserSettings? saved = null;
        store
            .Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultSettings);
        store
            .Setup(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()))
            .Callback<UserSettings, CancellationToken>((s, _) => saved = s)
            .Returns(Task.CompletedTask);

        var vm = CreateViewModel(DefaultSettings, store.Object);
        vm.SelectedFontSize = FontSizeOption.Large;          // auto-saved; does not dirty the form
        vm.SourceUrlText = "https://changed.example/";       // this enables the Save button
        vm.OnSaved = _ => { };

        await vm.SaveCommand.ExecuteAsync();

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.FontSizePt, Is.EqualTo(18.0));
            Assert.That(vm.HasUnsavedChanges, Is.False);
        });
    }

    [Test]
    public async Task SaveAsync_WithValidSettings_PersistsCurrentRelicCardWidth()
    {
        // A plain Save (triggered by an unrelated guarded field) must not clobber the
        // already-auto-saved relic card width back to the record's default.
        var store = new Mock<IUserSettingsStore>();
        UserSettings? saved = null;
        store
            .Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultSettings);
        store
            .Setup(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()))
            .Callback<UserSettings, CancellationToken>((s, _) => saved = s)
            .Returns(Task.CompletedTask);

        var vm = CreateViewModel(DefaultSettings, store.Object);
        vm.RelicCardWidthPx = 450;                            // auto-saved; does not dirty the form
        vm.SourceUrlText = "https://changed.example/";        // this enables the Save button
        vm.OnSaved = _ => { };

        await vm.SaveCommand.ExecuteAsync();

        Assert.That(saved!.RelicCardWidthPx, Is.EqualTo(450.0));
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
