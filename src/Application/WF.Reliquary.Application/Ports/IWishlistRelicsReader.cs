using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Application.Ports;

public interface IWishlistRelicsReader
{
    Task<IReadOnlyList<WishlistRelicEntry>> GetRelicsForWishlistAsync(
        long wishlistId, CancellationToken cancellationToken = default);
}
