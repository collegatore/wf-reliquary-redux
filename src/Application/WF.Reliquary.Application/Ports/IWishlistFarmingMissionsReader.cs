using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Application.Ports;

public interface IWishlistFarmingMissionsReader
{
    Task<IReadOnlyList<WishlistFarmingMissionEntry>> GetMissionsForWishlistAsync(
        long wishlistId, CancellationToken cancellationToken = default);
}
