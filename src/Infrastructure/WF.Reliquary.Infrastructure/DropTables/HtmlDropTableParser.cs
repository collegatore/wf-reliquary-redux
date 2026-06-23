using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed partial class HtmlDropTableParser : IDropTableParser
{
    /// <summary>
    /// Offloads the synchronous, regex-heavy <see cref="Parse"/> onto a Task
    /// </summary>
    public Task<ParsedDropTableImport> ParseAsync(
        StoredDropTableSource source,
        DateTimeOffset importedAt,
        CancellationToken cancellationToken = default)
        => Task.Run(() => Parse(source, importedAt), cancellationToken);

    /// <summary>
    /// Synchronous parse core. Kept public for direct, in-process testing; production
    /// callers go through <see cref="ParseAsync"/> via the <see cref="IDropTableParser"/> port.
    /// </summary>
    public ParsedDropTableImport Parse(StoredDropTableSource source, DateTimeOffset importedAt)
    {
        ArgumentNullException.ThrowIfNull(source);

        var warnings = new List<string>();
        var relicSection = ExtractSection(source.Html, "relicRewards")
            ?? throw new InvalidOperationException("Stored drop-table source does not contain the Relics section.");
        var missionSection = ExtractSection(source.Html, "missionRewards")
            ?? throw new InvalidOperationException("Stored drop-table source does not contain the Missions section.");

        var rewardItems = new Dictionary<string, ParsedRewardItem>(StringComparer.OrdinalIgnoreCase);
        var relicData = ParseRelicsAndRewards(relicSection, rewardItems, warnings);
        var missionData = ParseMissions(missionSection, relicData.Relics, rewardItems, warnings);

        if (relicData.Relics.Count == 0)
        {
            throw new InvalidOperationException("Stored drop-table source did not contain parseable Intact relic reward tables.");
        }

        if (missionData.Sources.Count == 0)
        {
            throw new InvalidOperationException("Stored drop-table source did not contain parseable mission reward tables.");
        }

        return new ParsedDropTableImport(
            source.Provenance.SourceUrl,
            source.Provenance.DownloadedAt,
            source.Provenance.LastUpdate,
            importedAt,
            rewardItems.Values.OrderBy(item => item.NormalizedName, StringComparer.Ordinal).ToArray(),
            relicData.Relics,
            relicData.Rewards,
            missionData.Sources,
            missionData.Rotations,
            missionData.Rewards,
            warnings);
    }

    private static RelicParseData ParseRelicsAndRewards(
        string sectionHtml,
        Dictionary<string, ParsedRewardItem> rewardItems,
        List<string> warnings)
    {
        var relics = new Dictionary<string, ParsedRelic>(StringComparer.OrdinalIgnoreCase);
        var rewards = new List<ParsedRelicReward>();
        string? currentRelic = null;
        string? currentRefinement = null;
        var sourceOrder = 0;

        foreach (var row in ReadRows(sectionHtml))
        {
            if (row.IsPureHeader)
            {
                var match = RelicHeadingRegex().Match(row.HeaderText);
                if (!match.Success)
                {
                    currentRelic = null;
                    currentRefinement = null;
                    continue;
                }

                currentRelic = $"{match.Groups["era"].Value} {match.Groups["code"].Value} Relic";
                currentRefinement = match.Groups["refinement"].Value;
                if (!relics.ContainsKey(currentRelic))
                {
                    relics.Add(
                        currentRelic,
                        new ParsedRelic(
                            match.Groups["era"].Value,
                            match.Groups["code"].Value,
                            currentRelic,
                            currentRelic));
                    AddRewardItem(rewardItems, currentRelic, isRelicReward: false);
                }

                continue;
            }

            if (!string.Equals(currentRefinement, "Intact", StringComparison.OrdinalIgnoreCase)
                || currentRelic is null
                || !TryReadRewardRow(row, out var rewardName, out var rarityLabel, out var chancePercent))
            {
                continue;
            }

            var tier = TierFromIntactChance(chancePercent);
            if (tier is null)
            {
                warnings.Add($"Relic {currentRelic} row {sourceOrder} has unsupported Intact chance {chancePercent:0.##}%.");
                continue;
            }

            AddRewardItem(rewardItems, rewardName, isRelicReward: true);
            rewards.Add(new ParsedRelicReward(
                currentRelic,
                rewardName,
                tier,
                rarityLabel,
                chancePercent,
                sourceOrder++));
        }

        if (relics.Count == 0)
        {
            warnings.Add("No relic headings could be split into era and code.");
        }

        return new RelicParseData(
            relics.Values.OrderBy(relic => relic.DisplayName, StringComparer.Ordinal).ToArray(),
            rewards);
    }

    private static MissionParseData ParseMissions(
        string sectionHtml,
        IReadOnlyList<ParsedRelic> parsedRelics,
        Dictionary<string, ParsedRewardItem> rewardItems,
        List<string> warnings)
    {
        var relicsByDisplayName = parsedRelics.ToDictionary(relic => relic.DisplayName, StringComparer.OrdinalIgnoreCase);
        var sources = new List<ParsedMissionSource>();
        var rotations = new List<ParsedRewardRotation>();
        var rewards = new List<ParsedMissionReward>();
        var seenRotations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ParsedMissionSource? currentSource = null;
        string? currentRotation = null;
        var sourceOrder = 0;
        var rewardOrder = 0;

        foreach (var row in ReadRows(sectionHtml))
        {
            if (row.IsPureHeader)
            {
                if (IsRotationHeading(row.HeaderText, out var rotationLabel))
                {
                    if (currentSource is null)
                    {
                        warnings.Add($"Rotation heading '{row.HeaderText}' appeared before a mission source.");
                        continue;
                    }

                    currentRotation = rotationLabel;
                    var rotationKey = $"{currentSource.DisplayLabel}|{currentSource.SourceOrder}|{currentRotation}";
                    if (seenRotations.Add(rotationKey))
                    {
                        rotations.Add(new ParsedRewardRotation(
                            currentSource.DisplayLabel,
                            currentSource.SourceOrder,
                            currentRotation,
                            row.HeaderText,
                            rotations.Count));
                    }

                    continue;
                }

                currentSource = CreateMissionSource(row.HeaderText, sourceOrder++, warnings);
                sources.Add(currentSource);
                currentRotation = null;
                continue;
            }

            if (currentSource is null || !TryReadRewardRow(row, out var rewardName, out var rarityLabel, out var chancePercent))
            {
                continue;
            }

            AddRewardItem(rewardItems, rewardName, isRelicReward: false);
            var relicDisplayName = relicsByDisplayName.ContainsKey(rewardName) ? rewardName : null;
            rewards.Add(new ParsedMissionReward(
                currentSource.DisplayLabel,
                currentSource.SourceOrder,
                currentRotation,
                rewardName,
                relicDisplayName,
                rarityLabel,
                chancePercent,
                rewardOrder++));
        }

        return new MissionParseData(sources, rotations, rewards);
    }

    private static ParsedMissionSource CreateMissionSource(string displayLabel, int sourceOrder, List<string> warnings)
    {
        var match = MissionLabelRegex().Match(displayLabel);
        if (!match.Success)
        {
            warnings.Add($"Mission label '{displayLabel}' could not be fully structured.");
            return new ParsedMissionSource(displayLabel, null, null, null, null, sourceOrder);
        }

        var qualifier = EmptyToNull(match.Groups["qpre"].Value) ?? EmptyToNull(match.Groups["qpost"].Value);

        return new ParsedMissionSource(
            displayLabel,
            EmptyToNull(match.Groups["region"].Value),
            EmptyToNull(match.Groups["node"].Value),
            EmptyToNull(match.Groups["type"].Value),
            qualifier,
            sourceOrder);
    }

    private static bool TryReadRewardRow(HtmlRow row, out string rewardName, out string rarityLabel, out decimal chancePercent)
    {
        rewardName = string.Empty;
        rarityLabel = string.Empty;
        chancePercent = 0;

        var cells = row.Cells.Where(cell => !string.IsNullOrWhiteSpace(cell.Text)).Select(cell => cell.Text).ToArray();
        if (cells.Length < 2)
        {
            return false;
        }

        var chanceCell = cells[^1];
        var chanceMatch = ChanceRegex().Match(chanceCell);
        if (!chanceMatch.Success)
        {
            return false;
        }

        rewardName = cells[^2];
        rarityLabel = chanceMatch.Groups["rarity"].Value.Trim();
        return decimal.TryParse(chanceMatch.Groups["chance"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out chancePercent);
    }

    private static void AddRewardItem(Dictionary<string, ParsedRewardItem> rewardItems, string name, bool isRelicReward)
    {
        var normalizedName = NormalizeName(name);
        var kind = ItemKind(name, isRelicReward);

        if (rewardItems.TryGetValue(normalizedName, out var existing))
        {
            if (kind == "PrimePart" && existing.ItemKind != "PrimePart")
            {
                rewardItems[normalizedName] = existing with { ItemKind = "PrimePart" };
            }
            return;
        }

        rewardItems.Add(normalizedName, new ParsedRewardItem(name, normalizedName, kind));
    }

    private static string ItemKind(string name, bool isRelicReward)
    {
        if (RelicNameRegex().IsMatch(name))
        {
            return "Relic";
        }

        if (isRelicReward
            && (name.Contains(" Prime ", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(" Prime Blueprint", StringComparison.OrdinalIgnoreCase)))
        {
            return "PrimePart";
        }

        if (name.EndsWith(" Blueprint", StringComparison.OrdinalIgnoreCase))
        {
            return "Blueprint";
        }

        return "Other";
    }

    private static string? TierFromIntactChance(decimal chancePercent)
    {
        return chancePercent switch
        {
            25.33m => "Common",
            11.00m => "Uncommon",
            2.00m => "Rare",
            _ => null
        };
    }

    private static string? ExtractSection(string html, string id)
    {
        var startMatch = Regex.Match(
            html,
            $"""<h3[^>]*id=["']{Regex.Escape(id)}["'][^>]*>.*?</h3>""",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (!startMatch.Success)
        {
            return null;
        }

        var start = startMatch.Index + startMatch.Length;
        var nextHeading = Regex.Match(html[start..], "<h3[^>]*id=[\"'][^\"']+[\"'][^>]*>", RegexOptions.IgnoreCase);
        var end = nextHeading.Success ? start + nextHeading.Index : html.Length;
        return html[start..end];
    }

    private static IEnumerable<HtmlRow> ReadRows(string html)
    {
        foreach (Match rowMatch in RowRegex().Matches(html))
        {
            var rowHtml = rowMatch.Groups["content"].Value;
            var cells = CellRegex()
                .Matches(rowHtml)
                .Select(match => new HtmlCell(
                    match.Groups["tag"].Value.Equals("th", StringComparison.OrdinalIgnoreCase),
                    CleanText(match.Groups["content"].Value)))
                .ToArray();

            if (cells.Length == 0 || cells.All(cell => string.IsNullOrWhiteSpace(cell.Text)))
            {
                continue;
            }

            yield return new HtmlRow(cells);
        }
    }

    private static bool IsRotationHeading(string text, out string rotationLabel)
    {
        var match = RotationRegex().Match(text);
        rotationLabel = match.Success ? match.Groups["label"].Value : string.Empty;
        return match.Success;
    }

    private static string NormalizeName(string text)
    {
        var normalized = NonWordRegex().Replace(text.Trim().ToUpperInvariant(), " ").Trim();
        return WhitespaceRegex().Replace(normalized, " ");
    }

    private static string CleanText(string html)
    {
        var withoutTags = TagRegex().Replace(html, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    private static string? EmptyToNull(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private sealed record HtmlCell(bool IsHeader, string Text);

    private sealed record HtmlRow(IReadOnlyList<HtmlCell> Cells)
    {
        public bool IsPureHeader => Cells.All(cell => cell.IsHeader) && Cells.Count > 0;

        public string HeaderText => string.Join(" ", Cells.Select(cell => cell.Text)).Trim();
    }

    private sealed record MissionParseData(
        IReadOnlyList<ParsedMissionSource> Sources,
        IReadOnlyList<ParsedRewardRotation> Rotations,
        IReadOnlyList<ParsedMissionReward> Rewards);

    private sealed record RelicParseData(
        IReadOnlyList<ParsedRelic> Relics,
        IReadOnlyList<ParsedRelicReward> Rewards);

    [GeneratedRegex("<tr[^>]*>(?<content>.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex RowRegex();

    [GeneratedRegex("<(?<tag>t[hd])[^>]*>(?<content>.*?)</\\k<tag>>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex CellRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex TagRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("[^A-Z0-9]+")]
    private static partial Regex NonWordRegex();

    [GeneratedRegex("^(?<era>Lith|Meso|Neo|Axi)\\s+(?<code>[A-Z0-9]+)\\s+Relic\\s+\\((?<refinement>[^)]+)\\)$", RegexOptions.IgnoreCase)]
    private static partial Regex RelicHeadingRegex();

    [GeneratedRegex("^(?<era>Lith|Meso|Neo|Axi)\\s+(?<code>[A-Z0-9]+)\\s+Relic$", RegexOptions.IgnoreCase)]
    private static partial Regex RelicNameRegex();

    [GeneratedRegex("^(?<rarity>.+?)\\s+\\((?<chance>[0-9]+(?:\\.[0-9]+)?)%\\)$")]
    private static partial Regex ChanceRegex();

    [GeneratedRegex("^Rotation\\s+(?<label>.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex RotationRegex();

    [GeneratedRegex("^(?:(?<qpre>[^/:]+):\\s*)?(?<region>[^/]+)/(?<node>.+?)\\s+\\((?<type>[^)]+)\\)(?:\\s+(?<qpost>.+))?$")]
    private static partial Regex MissionLabelRegex();
}
