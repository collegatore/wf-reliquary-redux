namespace WF.Reliquary.Application.Wishlist;

public sealed record WishlistFarmingRotationEntry(
    string RotationDisplayLabel,
    double CumulativeChance,
    IReadOnlyList<WishlistFarmingRelicEntry> Relics);
