using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.PrimeGear;
using WF.Reliquary.Application.Profiles;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class MainWindowShellViewModelTests
{
    // Tab indices mirror the TabControl declaration order in MainWindow.axaml:
    // Prime Gear=0, Relics and Missions=1, Drop Tables=2, Settings=3.
    private const int PrimeGearTabIndex = 0;
    private const int DropTablesTabIndex = 2;
    private const int SettingsTabIndex = 3;

    [Test]
    public void SelectedTabIndex_DefaultsToPrimeGear_OnConstruction()
    {
        var shell = CreateShell();

        Assert.That(shell.SelectedTabIndex, Is.EqualTo(PrimeGearTabIndex));
    }

    [Test]
    public void NavigateToDropTablesCommand_SelectsDropTablesTab()
    {
        var shell = CreateShell();

        shell.NavigateToDropTablesCommand.Execute(null);

        Assert.That(shell.SelectedTabIndex, Is.EqualTo(DropTablesTabIndex));
    }

    [Test]
    public void NavigateToSettingsCommand_SelectsSettingsTab()
    {
        var shell = CreateShell();

        shell.NavigateToSettingsCommand.Execute(null);

        Assert.That(shell.SelectedTabIndex, Is.EqualTo(SettingsTabIndex));
    }

    private static MainWindowShellViewModel CreateShell()
    {
        var dropTables = new MainWindowViewModel(
            Mock.Of<IDropTableSourceRefreshWorkflow>(),
            Mock.Of<IDropTableImportWorkflow>(),
            Mock.Of<IImportMetadataReader>(),
            Mock.Of<IDropTableLocalFileSourceWorkflow>(),
            Mock.Of<IDropTableSourceExporter>());

        var primeGearReader = new Mock<IPrimeGearReader>();
        primeGearReader.Setup(r => r.GetAllAsync(default)).ReturnsAsync([]);
        var primeGear = new PrimeGearViewModel(primeGearReader.Object, WishlistStore().Object, Mock.Of<IOwnedGearStore>());

        var wishlist = new WishlistViewModel(WishlistStore().Object, _ => Task.FromResult<string?>(null));

        var relics = new WishlistRelicsViewModel(
            Mock.Of<IWishlistRelicsReader>(),
            WishlistStore().Object);

        var farming = new WishlistFarmingMissionsViewModel(
            Mock.Of<IWishlistFarmingMissionsReader>(),
            WishlistStore().Object);

        var profiles = new ProfileViewModel(ProfileStore().Object, _ => Task.FromResult<string?>(null));

        var settings = new SettingsViewModel(Mock.Of<IUserSettingsStore>(), DefaultSettings);

        return new MainWindowShellViewModel(
            dropTables, primeGear, wishlist, relics, farming, profiles, settings);
    }

    private static UserSettings DefaultSettings { get; } = new(
        SourceUrl: new Uri("https://example.test/drops.html"),
        WindowWidth: 900,
        WindowHeight: 600,
        FontSize: UserFontSize.Medium,
        RelicsPanelHeight: null);

    private static Mock<IWishlistStore> WishlistStore()
    {
        var mock = new Mock<IWishlistStore>();
        mock.Setup(s => s.GetActiveAsync(default))
            .ReturnsAsync(new WishlistEntry(1, "Default", DateTimeOffset.UnixEpoch));
        mock.Setup(s => s.GetAllAsync(default))
            .ReturnsAsync([new WishlistEntry(1, "Default", DateTimeOffset.UnixEpoch)]);
        mock.Setup(s => s.GetItemsAsync(It.IsAny<long>(), default))
            .ReturnsAsync([]);
        return mock;
    }

    private static Mock<IProfileStore> ProfileStore()
    {
        var mock = new Mock<IProfileStore>();
        mock.Setup(s => s.GetActiveAsync(default))
            .ReturnsAsync(new ProfileEntry(1, "Default", DateTimeOffset.UnixEpoch));
        mock.Setup(s => s.GetAllAsync(default))
            .ReturnsAsync([new ProfileEntry(1, "Default", DateTimeOffset.UnixEpoch)]);
        return mock;
    }
}
