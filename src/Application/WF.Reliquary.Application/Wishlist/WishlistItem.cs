namespace WF.Reliquary.Application.Wishlist;

public sealed record WishlistItem(
    long WishlistId,
    string PartNormalizedName,
    string GearName,
    string PartDisplayName,
    bool IsGhost);
