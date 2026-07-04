using System.Net;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Storage;
using NUnit.Framework;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class DropTableInfrastructureTests
{
    [Test]
    public async Task LocalStore_SavesHtmlAndProvenanceToDb()
    {
        var dbPath = TempDatabasePath();
        var store = new LocalDropTableSourceStore(dbPath);
        var sourceUrl = new Uri("https://example.test/drop-tables.html");
        var downloadedAt = new DateTimeOffset(2026, 4, 19, 12, 0, 0, TimeSpan.Zero);

        await store.SaveAsync(
            new DropTableSourceContent(sourceUrl, "<b>Last Update:</b> 08 April, 2026", downloadedAt, "08 April, 2026"));

        var stored = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.Html, Does.Contain("08 April, 2026"));
            Assert.That(stored.SourceUrl, Is.EqualTo(sourceUrl));
            Assert.That(stored.DownloadedAt, Is.EqualTo(downloadedAt));
            Assert.That(stored.LastUpdate, Is.EqualTo("08 April, 2026"));
        });
    }

    [Test]
    public async Task LocalStore_ReturnsNullWhenNoSourcesStored()
    {
        var dbPath = TempDatabasePath();
        var store = new LocalDropTableSourceStore(dbPath);

        var stored = await store.LoadAsync();

        Assert.That(stored, Is.Null);
    }

    [Test]
    public async Task LocalStore_HasAnyAsync_ReturnsTrueAfterSave()
    {
        var dbPath = TempDatabasePath();
        var store = new LocalDropTableSourceStore(dbPath);

        Assert.That(await store.HasAnyAsync(), Is.False);

        await store.SaveAsync(
            new DropTableSourceContent(new Uri("file:///test.html"), "<b>Last Update:</b> 08 April, 2026", DateTimeOffset.UtcNow, "08 April, 2026"));

        Assert.That(await store.HasAnyAsync(), Is.True);
    }

    [Test]
    public async Task LocalStore_LoadAsync_ReturnsMostRecentRow()
    {
        var dbPath = TempDatabasePath();
        var store = new LocalDropTableSourceStore(dbPath);

        await store.SaveAsync(
            new DropTableSourceContent(new Uri("file:///test.html"), "<b>Last Update:</b> 07 April, 2026", DateTimeOffset.UtcNow, "07 April, 2026"));
        await store.SaveAsync(
            new DropTableSourceContent(new Uri("file:///test.html"), "<b>Last Update:</b> 08 April, 2026", DateTimeOffset.UtcNow, "08 April, 2026"));

        var stored = await store.LoadAsync();

        Assert.That(stored?.LastUpdate, Is.EqualTo("08 April, 2026"));
    }

    [Test]
    public async Task LocalStore_StoresLocalFileAcquisitionWithFileUri()
    {
        var dbPath = TempDatabasePath();
        var store = new LocalDropTableSourceStore(dbPath);
        var fileUri = new Uri(@"C:\Users\test\drop-tables.html");

        await store.SaveAsync(
            new DropTableSourceContent(fileUri, "<b>Last Update:</b> 08 April, 2026", DateTimeOffset.UtcNow, "08 April, 2026"));

        var stored = await store.LoadAsync();

        Assert.That(stored?.SourceUrl, Is.EqualTo(fileUri));
    }

    [Test]
    public void MetadataReader_ExtractsLastUpdate()
    {
        var reader = new HtmlDropTableMetadataReader();

        var lastUpdate = reader.ReadLastUpdate("<p><b>Last Update:</b> \r\n08 April, 2026\r\n<h3>Disclaimer:</h3>");

        Assert.That(lastUpdate, Is.EqualTo("08 April, 2026"));
    }

    [Test]
    public void MetadataReader_ReturnsNullWhenMarkerIsMissing()
    {
        var reader = new HtmlDropTableMetadataReader();

        Assert.That(reader.ReadLastUpdate("<html>No update marker</html>"), Is.Null);
    }

    [Test]
    public async Task HttpClient_DownloadsContentWithoutLiveNetwork()
    {
        var sourceUrl = new Uri("https://example.test/drop-tables.html");
        var downloadedAt = new DateTimeOffset(2026, 4, 19, 12, 0, 0, TimeSpan.Zero);
        using var httpClient = new HttpClient(new StubHttpMessageHandler("<html>drop table</html>"));
        var client = new HttpDropTableSourceClient(httpClient, () => downloadedAt);

        var content = await client.DownloadAsync(sourceUrl);

        Assert.Multiple(() =>
        {
            Assert.That(content.SourceUrl, Is.EqualTo(sourceUrl));
            Assert.That(content.Html, Is.EqualTo("<html>drop table</html>"));
            Assert.That(content.DownloadedAt, Is.EqualTo(downloadedAt));
        });
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string TempDatabasePath()
    {
        var dir = CreateTempDirectory();
        var dbPath = Path.Combine(dir, "test.sqlite");
        new SqliteDatabaseInitializer(dbPath).InitializeAsync().GetAwaiter().GetResult();
        return dbPath;
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string content;

        public StubHttpMessageHandler(string content)
        {
            this.content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };

            return Task.FromResult(response);
        }
    }
}
