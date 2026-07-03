using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.RecruitingMessage;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class AppearanceViewModelTests
{
    private static UserSettings DefaultSettings { get; } = new(
        SourceUrl: new Uri("https://example.test/drops.html"),
        WindowWidth: 900,
        WindowHeight: 600,
        FontSizePt: 14.0,
        RelicsPanelHeight: null,
        RecruitingMessageTemplate: RecruitingMessageTemplate.Default);

    [Test]
    public void RelicCardWidthPx_ClampsBelowMinimum()
    {
        var vm = CreateViewModel();

        vm.RelicCardWidthPx = 50;

        Assert.That(vm.RelicCardWidthPx, Is.EqualTo(180.0));
    }

    [Test]
    public void RelicCardWidthPx_ClampsAboveMaximum()
    {
        var vm = CreateViewModel();

        vm.RelicCardWidthPx = 9000;

        Assert.That(vm.RelicCardWidthPx, Is.EqualTo(600.0));
    }

    [Test]
    public void SelectedFontSize_RoundTripsToEffectiveFontSizePt()
    {
        var vm = CreateViewModel();

        vm.SelectedFontSize = FontSizeOption.Large;
        Assert.That(vm.EffectiveFontSizePt, Is.EqualTo(18.0));

        vm.SelectedFontSize = FontSizeOption.Small;
        Assert.That(vm.EffectiveFontSizePt, Is.EqualTo(11.0));

        vm.SelectedFontSize = FontSizeOption.Medium;
        Assert.That(vm.EffectiveFontSizePt, Is.EqualTo(14.0));
    }

    [Test]
    public void RelicCardWidthPx_Change_InvokesApplyRelicCardWidth()
    {
        var vm = CreateViewModel();
        double? applied = null;
        vm.ApplyRelicCardWidth = w => applied = w;

        vm.RelicCardWidthPx = 450;

        Assert.That(applied, Is.EqualTo(450.0));
    }

    [Test]
    public void SelectedOrientation_Change_InvokesApplyOrientation()
    {
        var vm = CreateViewModel();
        RelicsMissionsOrientation? applied = null;
        vm.ApplyOrientation = o => applied = o;

        vm.SelectedOrientation = RelicsMissionsOrientation.Stacked;

        Assert.That(applied, Is.EqualTo(RelicsMissionsOrientation.Stacked));
    }

    [Test]
    public void SelectedViewMode_Change_InvokesApplyViewMode()
    {
        var vm = CreateViewModel();
        RelicViewMode? applied = null;
        vm.ApplyViewMode = m => applied = m;

        vm.SelectedViewMode = RelicViewMode.Table;

        Assert.That(applied, Is.EqualTo(RelicViewMode.Table));
    }

    [Test]
    public async Task SelectedViewMode_Change_AutoSavesToStore()
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

        var vm = CreateViewModel(store.Object);

        vm.SelectedViewMode = RelicViewMode.Table;
        await SettleAsync();

        Assert.That(saved?.ViewMode, Is.EqualTo(RelicViewMode.Table));
    }

    [Test]
    public async Task RelicCardWidthPx_Change_AutoSavesToStore()
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

        var vm = CreateViewModel(store.Object);

        vm.RelicCardWidthPx = 450;
        await SettleAsync();

        Assert.That(saved?.RelicCardWidthPx, Is.EqualTo(450.0));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static AppearanceViewModel CreateViewModel(IUserSettingsStore? store = null)
    {
        store ??= Mock.Of<IUserSettingsStore>();
        return new AppearanceViewModel(store, DefaultSettings);
    }

    private static async Task SettleAsync()
    {
        await Task.Yield();
        await Task.Yield();
    }
}
