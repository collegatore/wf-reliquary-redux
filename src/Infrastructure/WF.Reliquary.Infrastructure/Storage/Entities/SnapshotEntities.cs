namespace WF.Reliquary.Infrastructure.Storage.Entities;

internal sealed class RewardItemEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string ItemKind { get; set; } = string.Empty;
}

internal sealed class RelicEntity
{
    public long Id { get; set; }
    public string Era { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public long RewardItemId { get; set; }
    public bool IsVaulted { get; set; }
    public ICollection<RelicRewardEntity> RelicRewards { get; set; } = [];
}

internal sealed class RelicRewardEntity
{
    public long Id { get; set; }
    public long RelicId { get; set; }
    public long RewardItemId { get; set; }
    public string RewardTier { get; set; } = string.Empty;
    public string SourceRarityLabel { get; set; } = string.Empty;
    public double SourceChancePercent { get; set; }
    public int SourceOrder { get; set; }
}

internal sealed class MissionSourceEntity
{
    public long Id { get; set; }
    public string DisplayLabel { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? Node { get; set; }
    public string? MissionType { get; set; }
    public string? Qualifier { get; set; }
    public int SourceOrder { get; set; }
    public ICollection<RewardRotationEntity> RewardRotations { get; set; } = [];
    public ICollection<MissionRewardEntity> MissionRewards { get; set; } = [];
}

internal sealed class RewardRotationEntity
{
    public long Id { get; set; }
    public long MissionSourceId { get; set; }
    public string RotationLabel { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
    public int SourceOrder { get; set; }
}

internal sealed class MissionRewardEntity
{
    public long Id { get; set; }
    public long MissionSourceId { get; set; }
    public long? RotationId { get; set; }
    public long RewardItemId { get; set; }
    public long? RelicId { get; set; }
    public string RarityLabel { get; set; } = string.Empty;
    public double ChancePercent { get; set; }
    public int SourceOrder { get; set; }
}

internal sealed class GearVaultStatusEntity
{
    public string GearName { get; set; } = string.Empty;
    public bool IsVaulted { get; set; }
}

internal sealed class ImportMetadataEntity
{
    public long Id { get; set; }
    public string? SourceUrl { get; set; }
    public string? DownloadedAt { get; set; }
    public string? SourceLastUpdate { get; set; }
    public string ImportedAt { get; set; } = string.Empty;
}

internal sealed class SourceFileEntity
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string DownloadedAt { get; set; } = string.Empty;
    public string SourceLastUpdate { get; set; } = string.Empty;
}
