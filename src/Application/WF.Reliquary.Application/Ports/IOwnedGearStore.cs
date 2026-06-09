namespace WF.Reliquary.Application.Ports;

// Owned gear is scoped to the active profile. Both methods resolve the active
// profile internally (from the persisted last-profile preference, falling back to
// the earliest profile) so callers never pass a profile id — mirroring IWishlistStore.
public interface IOwnedGearStore
{
    /// <summary>Returns the gear names marked owned in the active profile.</summary>
    Task<IReadOnlyList<string>> GetOwnedGearNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a gear as owned (when <paramref name="owned"/> is true) or clears the
    /// owned mark (when false) for the active profile. Marking is idempotent.
    /// Gear is recorded by name as a soft reference, so owned marks survive imports.
    /// </summary>
    Task SetOwnedAsync(string gearName, bool owned, CancellationToken cancellationToken = default);
}
