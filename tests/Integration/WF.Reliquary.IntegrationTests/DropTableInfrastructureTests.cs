using System.Net;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;
using NUnit.Framework;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class DropTableInfrastructureTests
{
    [Test]
    public async Task LocalStore_SavesRawHtmlAndProvenance()
    {
        var root = CreateTempDirectory();
        var store = new LocalDropTableSourceStore(root);
        var sourceUrl = new Uri("https://example.test/drop-tables.html");
        var downloadedAt = new DateTimeOffset(2026, 4, 19, 12, 0, 0, TimeSpan.Zero);

        await store.SaveAsync(
            new DropTableSourceContent(sourceUrl, "<b>Last Update:</b> 08 April, 2026", downloadedAt, "08 April, 2026"),
            DropTableSourceDefaults.FileName);

        var stored = await store.LoadAsync(DropTableSourceDefaults.FileName);

        Assert.Multiple(() =>
        {
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.FilePath, Does.EndWith(DropTableSourceDefaults.FileName));
            Assert.That(stored.Html, Does.Contain("08 April, 2026"));
            Assert.That(stored.Provenance?.SourceUrl, Is.EqualTo(sourceUrl));
            Assert.That(stored.Provenance?.DownloadedAt, Is.EqualTo(downloadedAt));
            Assert.That(stored.Provenance?.LastUpdate, Is.EqualTo("08 April, 2026"));
        });
    }

    [Test]
    public void LocalStore_RejectsSaveWithoutLastUpdateProvenance()
    {
        var root = CreateTempDirectory();
        var store = new LocalDropTableSourceStore(root);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.SaveAsync(
                new DropTableSourceContent(new Uri("https://example.test/drop-tables.html"), "<html></html>", DateTimeOffset.UnixEpoch),
                DropTableSourceDefaults.FileName));

        Assert.That(exception?.Message, Does.Contain("Last Update"));
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

    [Test]
    public async Task LocalSettingsStore_SavesUserEditableSourceUrl()
    {
        var root = CreateTempDirectory();
        var store = new LocalDropTableSourceSettingsStore(root);
        var sourceUrl = new Uri("https://example.test/custom-drop-tables.html");

        await store.SaveAsync(new DropTableSourceSettings(sourceUrl));

        var loaded = await store.LoadAsync();

        Assert.That(loaded.SourceUrl, Is.EqualTo(sourceUrl));
    }

    [Test]
    public async Task LocalSettingsStore_UsesDefaultUrlWhenSettingsDoNotExist()
    {
        var root = CreateTempDirectory();
        var store = new LocalDropTableSourceSettingsStore(root);

        var loaded = await store.LoadAsync();

        Assert.That(loaded.SourceUrl.ToString(), Is.EqualTo(DropTableSourceDefaults.SourceUrl));
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
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
