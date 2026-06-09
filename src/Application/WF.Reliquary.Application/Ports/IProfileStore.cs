using WF.Reliquary.Application.Profiles;

namespace WF.Reliquary.Application.Ports;

public interface IProfileStore
{
    /// <summary>
    /// If no profiles exist, creates a "Default" profile and sets it as active.
    /// Safe to call on every startup; no-ops when profiles already exist.
    /// </summary>
    Task SeedDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all profiles ordered by creation date.</summary>
    Task<IReadOnlyList<ProfileEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the currently active profile, or falls back to the first available
    /// profile if the stored preference points to a deleted profile.
    /// Returns null only when no profiles exist at all.
    /// </summary>
    Task<ProfileEntry?> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists the active profile preference.</summary>
    Task SetActiveAsync(long profileId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new profile with the given name and returns it.</summary>
    Task<ProfileEntry> CreateAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Renames the specified profile.</summary>
    Task RenameAsync(long profileId, string newName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified profile. All wishlists and their items are removed via cascade.
    /// Callers must switch the active profile before calling this when deleting the active one.
    /// </summary>
    Task DeleteAsync(long profileId, CancellationToken cancellationToken = default);
}
