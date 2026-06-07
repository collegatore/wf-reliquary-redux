namespace WF.Reliquary.Application.Wishlist;

public sealed record WishlistFarmingMissionEntry(
    string DisplayLabel,
    string? MissionType,
    string? Node,
    string? Region,
    double EffectiveChance,
    bool HasRotations,
    IReadOnlyList<WishlistFarmingRotationEntry> Rotations,
    IReadOnlyList<WishlistFarmingRelicEntry> Relics);
