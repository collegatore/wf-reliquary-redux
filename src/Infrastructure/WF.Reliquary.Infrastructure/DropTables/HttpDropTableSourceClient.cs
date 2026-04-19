using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class HttpDropTableSourceClient : IDropTableSourceClient
{
    private readonly HttpClient httpClient;
    private readonly Func<DateTimeOffset> getNow;

    public HttpDropTableSourceClient(HttpClient httpClient, Func<DateTimeOffset>? getNow = null)
    {
        this.httpClient = httpClient;
        this.getNow = getNow ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<DropTableSourceContent> DownloadAsync(Uri sourceUrl, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(sourceUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        return new DropTableSourceContent(sourceUrl, html, getNow());
    }
}
