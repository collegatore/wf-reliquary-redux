using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Application.Ports;

// Wishlists are scoped to the active profile. SeedDefaultAsync, GetAllAsync,
// GetActiveAsync, CreateAsync, and SetActiveAsync all resolve the active profile
// internally (from the persisted last-profile preference) and operate within it.
public interface IWishlistStore
{
    /// <summary>
    /// If the active profile has no wishlists, creates a "Default" wishlist within it
    /// and sets it as active. Safe to call on every startup; no-ops when the active
    /// profile already has wishlists.
    /// </summary>
    Task SeedDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all wishlists in the active profile ordered by creation date.</summary>
    Task<IReadOnlyList<WishlistEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the active profile's currently active wishlist, or falls back to the
    /// profile's earliest-created wishlist if the stored preference points to a deleted
    /// wishlist. Returns null only when the active profile has no wishlists at all.
    /// </summary>
    Task<WishlistEntry?> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists the active wishlist preference for the active profile.</summary>
    Task SetActiveAsync(long wishlistId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new wishlist in the active profile with the given name and returns it.</summary>
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
