namespace WF.Reliquary.Application.Wishlist;

public sealed record WishlistFarmingMissionGroupEntry(
    string GroupTitle,
    IReadOnlyList<string> Nodes,
    double EffectiveChance,
    bool HasRotations,
    IReadOnlyList<WishlistFarmingRotationEntry> Rotations,
    IReadOnlyList<WishlistFarmingRelicEntry> Relics);
