namespace WF.Reliquary.Application.Wishlist;

public sealed record WishlistRelicEntry(
    string DisplayName,
    string Era,
    bool IsVaulted,
    IReadOnlyList<WishlistRelicReward> Rewards);
