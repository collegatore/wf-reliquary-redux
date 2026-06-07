using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Infrastructure.Settings;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class UserSettingsStoreTests
{
    private string tempDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task LoadAsync_WhenFileAbsent_ReturnsDefaults()
    {
        var store = new LocalUserSettingsStore(tempDir);

        var settings = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(settings.SourceUrl.ToString(), Is.EqualTo(DropTableSourceDefaults.SourceUrl));
            Assert.That(settings.WindowWidth, Is.EqualTo(900));
            Assert.That(settings.WindowHeight, Is.EqualTo(600));
            Assert.That(settings.FontSize, Is.EqualTo(UserFontSize.Medium));
            Assert.That(settings.RelicsPanelHeight, Is.Null);
        });
    }

    [Test]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsAllFields()
    {
        var store = new LocalUserSettingsStore(tempDir);
        var sourceUrl = new Uri("https://example.test/custom.html");

        var original = new UserSettings(
            SourceUrl: sourceUrl,
            WindowWidth: 1280,
            WindowHeight: 800,
            FontSize: UserFontSize.Large,
            RelicsPanelHeight: 320.5);

        await store.SaveAsync(original);
        var loaded = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(loaded.SourceUrl, Is.EqualTo(sourceUrl));
            Assert.That(loaded.WindowWidth, Is.EqualTo(1280));
            Assert.That(loaded.WindowHeight, Is.EqualTo(800));
            Assert.That(loaded.FontSize, Is.EqualTo(UserFontSize.Large));
            Assert.That(loaded.RelicsPanelHeight, Is.EqualTo(320.5));
        });
    }

    [Test]
    public async Task SaveAsync_ThenLoadAsync_WhenRelicsPanelHeightIsNull_RemainsNull()
    {
        var store = new LocalUserSettingsStore(tempDir);

        var original = UserSettings.Default with { RelicsPanelHeight = null };
        await store.SaveAsync(original);
        var loaded = await store.LoadAsync();

        Assert.That(loaded.RelicsPanelHeight, Is.Null);
    }

    [Test]
    public async Task AsDropTableSourceSettingsStore_LoadAsync_ReturnsSourceUrl()
    {
        var store = new LocalUserSettingsStore(tempDir);
        var customUrl = new Uri("https://example.test/drops.html");
        await store.SaveAsync(UserSettings.Default with { SourceUrl = customUrl });

        var dropTableStore = (WF.Reliquary.Application.Ports.IDropTableSourceSettingsStore)store;
        var dropTableSettings = await dropTableStore.LoadAsync();

        Assert.That(dropTableSettings.SourceUrl, Is.EqualTo(customUrl));
    }

    [Test]
    public async Task AsDropTableSourceSettingsStore_WhenFileAbsent_ReturnsDefaultUrl()
    {
        var store = new LocalUserSettingsStore(tempDir);
        var dropTableStore = (WF.Reliquary.Application.Ports.IDropTableSourceSettingsStore)store;

        var settings = await dropTableStore.LoadAsync();

        Assert.That(settings.SourceUrl.ToString(), Is.EqualTo(DropTableSourceDefaults.SourceUrl));
    }
}
