using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Application.Ports;

public interface IWishlistFarmingMissionsReader
{
    Task<IReadOnlyList<WishlistFarmingMissionGroupEntry>> GetMissionsForWishlistAsync(
        long wishlistId, CancellationToken cancellationToken = default);
}
