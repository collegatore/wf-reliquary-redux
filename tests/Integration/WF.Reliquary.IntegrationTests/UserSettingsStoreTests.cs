using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.RecruitingMessage;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Infrastructure.Settings;
using static WF.Reliquary.Application.Settings.RelicsMissionsOrientation;

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
            Assert.That(settings.WindowWidth, Is.EqualTo(1280));
            Assert.That(settings.WindowHeight, Is.EqualTo(800));
            Assert.That(settings.FontSizePt, Is.EqualTo(14.0));
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
            FontSizePt: 18.0,
            RelicsPanelHeight: 320.5,
            RecruitingMessageTemplate: RecruitingMessageTemplate.Default);

        await store.SaveAsync(original);
        var loaded = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(loaded.SourceUrl, Is.EqualTo(sourceUrl));
            Assert.That(loaded.WindowWidth, Is.EqualTo(1280));
            Assert.That(loaded.WindowHeight, Is.EqualTo(800));
            Assert.That(loaded.FontSizePt, Is.EqualTo(18.0));
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
    public async Task LoadAsync_WhenFileAbsent_DefaultsToSideBySideOrientation()
    {
        var store = new LocalUserSettingsStore(tempDir);

        var settings = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(settings.Orientation, Is.EqualTo(SideBySide));
            Assert.That(settings.SideBySideRelicsWidth, Is.Null);
        });
    }

    [Test]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsOrientation()
    {
        var store = new LocalUserSettingsStore(tempDir);

        await store.SaveAsync(UserSettings.Default with
        {
            Orientation = SideBySide,
            SideBySideRelicsWidth = 0.6,
        });
        var loaded = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Orientation, Is.EqualTo(SideBySide));
            Assert.That(loaded.SideBySideRelicsWidth, Is.EqualTo(0.6));
        });
    }

    [Test]
    public async Task LoadAsync_WhenOrientationMissingFromFile_DefaultsToSideBySide()
    {
        // Simulate a settings file written by an older version that lacks Orientation.
        var path = Path.Combine(tempDir, "user-settings.json");
        await File.WriteAllTextAsync(path,
            """{"sourceUrl":"https://example.test/","windowWidth":900,"windowHeight":600,"fontSizePt":14}""");
        var store = new LocalUserSettingsStore(tempDir);

        var loaded = await store.LoadAsync();

        Assert.That(loaded.Orientation, Is.EqualTo(SideBySide));
    }

    [Test]
    public async Task LoadAsync_WhenFileAbsent_DefaultsToCompactViewMode()
    {
        var store = new LocalUserSettingsStore(tempDir);

        var settings = await store.LoadAsync();

        Assert.That(settings.ViewMode, Is.EqualTo(RelicViewMode.Compact));
    }

    [Test]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsViewMode()
    {
        var store = new LocalUserSettingsStore(tempDir);

        await store.SaveAsync(UserSettings.Default with { ViewMode = RelicViewMode.Table });
        var loaded = await store.LoadAsync();

        Assert.That(loaded.ViewMode, Is.EqualTo(RelicViewMode.Table));
    }

    [Test]
    public async Task LoadAsync_WhenViewModeMissingFromFile_DefaultsToCompact()
    {
        // Simulate a settings file written by an older version that lacks ViewMode.
        var path = Path.Combine(tempDir, "user-settings.json");
        await File.WriteAllTextAsync(path,
            """{"sourceUrl":"https://example.test/","windowWidth":900,"windowHeight":600,"fontSizePt":14}""");
        var store = new LocalUserSettingsStore(tempDir);

        var loaded = await store.LoadAsync();

        Assert.That(loaded.ViewMode, Is.EqualTo(RelicViewMode.Compact));
    }

    [Test]
    public async Task LoadAsync_WhenFileAbsent_DefaultsRelicCardWidthTo200()
    {
        var store = new LocalUserSettingsStore(tempDir);

        var settings = await store.LoadAsync();

        Assert.That(settings.RelicCardWidthPx, Is.EqualTo(200.0));
    }

    [Test]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsRelicCardWidth()
    {
        var store = new LocalUserSettingsStore(tempDir);

        await store.SaveAsync(UserSettings.Default with { RelicCardWidthPx = 420.0 });
        var loaded = await store.LoadAsync();

        Assert.That(loaded.RelicCardWidthPx, Is.EqualTo(420.0));
    }

    [Test]
    public async Task LoadAsync_WhenRelicCardWidthMissingFromFile_DefaultsTo200()
    {
        // Simulate a settings file written by an older version that lacks RelicCardWidthPx.
        var path = Path.Combine(tempDir, "user-settings.json");
        await File.WriteAllTextAsync(path,
            """{"sourceUrl":"https://example.test/","windowWidth":900,"windowHeight":600,"fontSizePt":14}""");
        var store = new LocalUserSettingsStore(tempDir);

        var loaded = await store.LoadAsync();

        Assert.That(loaded.RelicCardWidthPx, Is.EqualTo(200.0));
    }

    [Test]
    public async Task LoadAsync_WhenFileAbsent_DefaultsToEraSortModeAscending()
    {
        var store = new LocalUserSettingsStore(tempDir);

        var settings = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(settings.SortMode, Is.EqualTo(RelicSortMode.Era));
            Assert.That(settings.SortDescending, Is.False);
        });
    }

    [Test]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsSortModeAndDirection()
    {
        var store = new LocalUserSettingsStore(tempDir);

        await store.SaveAsync(UserSettings.Default with
        {
            SortMode = RelicSortMode.WishlistScore,
            SortDescending = true,
        });
        var loaded = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(loaded.SortMode, Is.EqualTo(RelicSortMode.WishlistScore));
            Assert.That(loaded.SortDescending, Is.True);
        });
    }

    [Test]
    public async Task LoadAsync_WhenSortFieldsMissingFromFile_DefaultsToEraAscending()
    {
        // Simulate a settings file written by an older version that lacks SortMode/SortDescending.
        var path = Path.Combine(tempDir, "user-settings.json");
        await File.WriteAllTextAsync(path,
            """{"sourceUrl":"https://example.test/","windowWidth":900,"windowHeight":600,"fontSizePt":14}""");
        var store = new LocalUserSettingsStore(tempDir);

        var loaded = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(loaded.SortMode, Is.EqualTo(RelicSortMode.Era));
            Assert.That(loaded.SortDescending, Is.False);
        });
    }

    [Test]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsPalette()
    {
        var store = new LocalUserSettingsStore(tempDir);

        await store.SaveAsync(UserSettings.Default with { Palette = ColorPalette.Stone });
        var loaded = await store.LoadAsync();

        Assert.That(loaded.Palette, Is.EqualTo(ColorPalette.Stone));
    }

    [Test]
    public async Task LoadAsync_WhenPaletteMissingFromFile_DefaultsToLinen()
    {
        // Simulate a settings file written by a version without the palette setting.
        var path = Path.Combine(tempDir, "user-settings.json");
        await File.WriteAllTextAsync(path,
            """{"sourceUrl":"https://example.test/","windowWidth":900,"windowHeight":600,"fontSizePt":14}""");
        var store = new LocalUserSettingsStore(tempDir);

        var loaded = await store.LoadAsync();

        Assert.That(loaded.Palette, Is.EqualTo(ColorPalette.Linen));
    }

    [Test]
    public async Task LoadAsync_WhenPaletteUnrecognized_DefaultsToLinen_AndKeepsOtherSettings()
    {
        var path = Path.Combine(tempDir, "user-settings.json");
        await File.WriteAllTextAsync(path,
            """{"sourceUrl":"https://example.test/","windowWidth":900,"windowHeight":600,"fontSizePt":18,"viewMode":"Table","palette":"Neon"}""");
        var store = new LocalUserSettingsStore(tempDir);

        var loaded = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Palette, Is.EqualTo(ColorPalette.Linen));
            Assert.That(loaded.FontSizePt, Is.EqualTo(18), "an unknown palette must not reset other settings");
            Assert.That(loaded.ViewMode, Is.EqualTo(RelicViewMode.Table));
            Assert.That(loaded.WindowWidth, Is.EqualTo(900));
        });
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

    [Test]
    public async Task LoadAsync_WhenFileAbsent_ReturnsDefaultRecruitingMessageTemplate()
    {
        var store = new LocalUserSettingsStore(tempDir);

        var settings = await store.LoadAsync();

        Assert.That(settings.RecruitingMessageTemplate.Pattern, Is.EqualTo(RecruitingMessageTemplate.Default.Pattern));
        Assert.That(settings.RecruitingMessageTemplate.Groups, Has.Count.EqualTo(RecruitingMessageTemplate.Default.Groups.Count));
    }

    [Test]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsRecruitingMessageTemplate()
    {
        var store = new LocalUserSettingsStore(tempDir);

        var customTemplate = new RecruitingMessageTemplate(
            "{participation} [{relic_name}]",
            [new RecruitingMessageOptionGroup("participation", [new RecruitingMessageOption("Host", "H")], "H")]);

        await store.SaveAsync(UserSettings.Default with { RecruitingMessageTemplate = customTemplate });
        var loaded = await store.LoadAsync();

        Assert.That(loaded.RecruitingMessageTemplate.Pattern, Is.EqualTo("{participation} [{relic_name}]"));
        Assert.That(loaded.RecruitingMessageTemplate.Groups, Has.Count.EqualTo(1));
        Assert.That(loaded.RecruitingMessageTemplate.Groups[0].Placeholder, Is.EqualTo("participation"));
        Assert.That(loaded.RecruitingMessageTemplate.Groups[0].DefaultValue, Is.EqualTo("H"));
        Assert.That(loaded.RecruitingMessageTemplate.Groups[0].Options[0].Descriptor, Is.EqualTo("Host"));
        Assert.That(loaded.RecruitingMessageTemplate.Groups[0].Options[0].Value, Is.EqualTo("H"));
    }
}
