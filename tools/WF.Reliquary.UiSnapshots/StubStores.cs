using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Profiles;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.UiSnapshots;

/// <summary>
/// In-memory stores behind the real selector view models (<c>WishlistViewModel</c>,
/// <c>ProfileViewModel</c>), whose entry types the views' typed templates need. Reads
/// return a fixed list; writes are no-ops, since a static render never invokes them.
/// </summary>
internal sealed class StubWishlistStore(params string[] names) : IWishlistStore
{
    private readonly List<WishlistEntry> entries =
        names.Select((n, i) => new WishlistEntry(i + 1, n, DateTimeOffset.UnixEpoch)).ToList();

    public Task SeedDefaultAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<WishlistEntry>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WishlistEntry>>(entries);
    public Task<WishlistEntry?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(entries.FirstOrDefault());
    public Task SetActiveAsync(long wishlistId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<WishlistEntry> CreateAsync(string name, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
    public Task RenameAsync(long wishlistId, string newName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(long wishlistId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<WishlistItem>> GetItemsAsync(long wishlistId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WishlistItem>>([]);
    public Task AddItemAsync(long wishlistId, string partNormalizedName, string gearName, string partDisplayName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
    public Task RemoveItemAsync(long wishlistId, string partNormalizedName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

internal sealed class StubProfileStore(params string[] names) : IProfileStore
{
    private readonly List<ProfileEntry> entries =
        names.Select((n, i) => new ProfileEntry(i + 1, n, DateTimeOffset.UnixEpoch)).ToList();

    public Task SeedDefaultAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<ProfileEntry>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProfileEntry>>(entries);
    public Task<ProfileEntry?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(entries.FirstOrDefault());
    public Task SetActiveAsync(long profileId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<ProfileEntry> CreateAsync(string name, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
    public Task RenameAsync(long profileId, string newName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(long profileId, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
