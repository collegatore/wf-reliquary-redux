namespace WF.Reliquary.Infrastructure.Storage.Entities;

internal sealed class WishlistEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

// part_normalized_name is a soft reference to reward_items.normalized_name —
// no FK constraint so wishlist items survive import cycles that clear reward_items.
internal sealed class WishlistItemEntity
{
    public long Id { get; set; }
    public long WishlistId { get; set; }
    public string PartNormalizedName { get; set; } = string.Empty;
    public string GearName { get; set; } = string.Empty;
    public string PartDisplayName { get; set; } = string.Empty;
    public string AddedAt { get; set; } = string.Empty;
}

internal sealed class UserPreferencesEntity
{
    public long Id { get; set; }
    public long? LastWishlistId { get; set; }
}
