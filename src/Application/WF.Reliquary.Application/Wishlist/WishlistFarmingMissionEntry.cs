namespace WF.Reliquary.Application.Wishlist;

public sealed record WishlistFarmingMissionEntry(
    string DisplayLabel,
    double EffectiveChance,
    bool HasRotations,
    IReadOnlyList<WishlistFarmingRotationEntry> Rotations,
    IReadOnlyList<WishlistFarmingRelicEntry> Relics);
