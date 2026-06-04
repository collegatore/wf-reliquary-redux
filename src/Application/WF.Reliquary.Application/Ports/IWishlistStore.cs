using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Application.Ports;

public interface IWishlistStore
{
    /// <summary>
    /// If no wishlists exist, creates a "Default" wishlist and sets it as active.
    /// Safe to call on every startup; no-ops when wishlists already exist.
    /// </summary>
    Task SeedDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all wishlists ordered by creation date.</summary>
    Task<IReadOnlyList<WishlistEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the currently active wishlist, or falls back to the first available
    /// wishlist if the stored preference points to a deleted wishlist.
    /// Returns null only when no wishlists exist at all.
    /// </summary>
    Task<WishlistEntry?> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists the active wishlist preference.</summary>
    Task SetActiveAsync(long wishlistId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new wishlist with the given name and returns it.</summary>
    Task<WishlistEntry> CreateAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Renames the specified wishlist.</summary>
    Task RenameAsync(long wishlistId, string newName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified wishlist. All items are removed via cascade.
    /// Callers must switch the active wishlist before calling this when deleting the active one.
    /// </summary>
    Task DeleteAsync(long wishlistId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns items in the specified wishlist. Items absent from the current import
    /// are returned with <see cref="WishlistItem.IsGhost"/> set to true.
    /// </summary>
    Task<IReadOnlyList<WishlistItem>> GetItemsAsync(long wishlistId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a part to the wishlist. Silently succeeds if it is already present.
    /// Stores gear name and part display name for ghost rendering.
    /// </summary>
    Task AddItemAsync(long wishlistId, string partNormalizedName, string gearName, string partDisplayName, CancellationToken cancellationToken = default);

    /// <summary>Removes a part from the wishlist. Silently succeeds if it is not present.</summary>
    Task RemoveItemAsync(long wishlistId, string partNormalizedName, CancellationToken cancellationToken = default);
}
