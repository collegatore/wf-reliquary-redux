using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.Storage;

/// <summary>
/// Shared seeding of the "Default" profile. Used by both the profile store and the
/// wishlist store so that seeding the wishlist default never silently no-ops just
/// because the profile store has not run yet. Idempotent.
/// </summary>
internal static class DefaultProfileSeeder
{
    /// <summary>
    /// Ensures at least one profile exists and a `last_profile_id` preference is set,
    /// then returns the active (or earliest-created) profile id.
    /// </summary>
    public static async Task<long> EnsureAsync(ReliquaryDbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.Profiles
            .OrderBy(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var profile = new ProfileEntity
        {
            Name = "Default",
            CreatedAt = DateTimeOffset.UtcNow.ToString("O")
        };
        context.Profiles.Add(profile);
        await context.SaveChangesAsync(cancellationToken);

        var prefs = await context.UserPreferences.FindAsync([1L], cancellationToken);
        if (prefs is null)
        {
            context.UserPreferences.Add(new UserPreferencesEntity { Id = 1, LastProfileId = profile.Id });
        }
        else
        {
            prefs.LastProfileId = profile.Id;
        }
        await context.SaveChangesAsync(cancellationToken);

        return profile.Id;
    }
}
