namespace WF.Reliquary.Application.Wishlist;

public sealed record WishlistEntry(long Id, string Name, DateTimeOffset CreatedAt);
