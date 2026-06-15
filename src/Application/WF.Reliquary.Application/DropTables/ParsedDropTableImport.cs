namespace WF.Reliquary.Application.DropTables;

public sealed record ParsedDropTableImport(
    Uri SourceUrl,
    DateTimeOffset DownloadedAt,
    string SourceLastUpdate,
    DateTimeOffset ImportedAt,
    IReadOnlyList<ParsedRewardItem> RewardItems,
    IReadOnlyList<ParsedRelic> Relics,
    IReadOnlyList<ParsedRelicReward> RelicRewards,
    IReadOnlyList<ParsedMissionSource> MissionSources,
    IReadOnlyList<ParsedRewardRotation> RewardRotations,
    IReadOnlyList<ParsedMissionReward> MissionRewards,
    IReadOnlyList<string> Warnings)
{
    public DropTableImportCounts Counts => new(
        RewardItems.Count,
        Relics.Count,
        RelicRewards.Count,
        MissionSources.Count,
        RewardRotations.Count,
        MissionRewards.Count);
}

public sealed record ParsedRewardItem(
    string Name,
    string NormalizedName,
    string ItemKind);

public sealed record ParsedRelic(
    string Era,
    string Code,
    string DisplayName,
    string RewardItemName);

public sealed record ParsedRelicReward(
    string RelicDisplayName,
    string RewardItemName,
    string RewardTier,
    string SourceRarityLabel,
    decimal SourceChancePercent,
    int SourceOrder);

public sealed record ParsedMissionSource(
    string DisplayLabel,
    string? Region,
    string? Node,
    string? MissionType,
    string? Qualifier,
    int SourceOrder);

public sealed record ParsedRewardRotation(
    string MissionSourceLabel,
    int MissionSourceOrder,
    string RotationLabel,
    string DisplayLabel,
    int SourceOrder);

public sealed record ParsedMissionReward(
    string MissionSourceLabel,
    int MissionSourceOrder,
    string? RotationLabel,
    string RewardItemName,
    string? RelicDisplayName,
    string RarityLabel,
    decimal ChancePercent,
    int SourceOrder);
