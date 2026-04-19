using System.Net;
using System.Text.RegularExpressions;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed partial class HtmlDropTableMetadataReader : IDropTableMetadataReader
{
    public string? ReadLastUpdate(string html)
    {
        var match = LastUpdateRegex().Match(html);

        if (!match.Success)
        {
            return null;
        }

        var value = WebUtility.HtmlDecode(match.Groups["value"].Value).Trim();
        return value.Length == 0 ? null : value;
    }

    [GeneratedRegex(@"Last\s+Update:\s*</b>\s*(?<value>[^<\r\n]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LastUpdateRegex();
}
