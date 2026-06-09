## Why

Some players run more than one Warframe account and want to track each account's
progress separately. Today all wishlists live in a single flat namespace, so there is
no way to keep one account's farming goals from mixing with another's. Introducing
player profiles lets a user partition their wishlists per account.

## What Changes

- Introduce **player profiles**: named containers that scope wishlists. A "Default"
  profile is seeded on first launch.
- **BREAKING (data model):** Wishlists become scoped to a profile. Every wishlist now
  belongs to exactly one profile; deleting a profile cascades to its wishlists and items.
- The **active profile** is persisted across sessions and restored on launch. Switching
  the active profile reloads the wishlist selector to show only that profile's wishlists
  and re-activates that profile's last-active wishlist.
- Add a **Profiles** management section in **Settings** (separate from the existing
  Font size / Source URL sections) supporting create, rename, delete, and switch —
  mirroring the existing wishlist CRUD behavior. Deletion is blocked when only one
  profile remains.
- The wishlist selector on the Prime Gear tab is unchanged in placement, but now operates
  within the active profile.
- A migration adds the `profiles` table, a `profile_id` column on `wishlists`, and a
  `last_profile_id` preference; existing wishlists are assigned to the seeded Default
  profile so no data is lost.

## Capabilities

### New Capabilities
- `player-profiles`: Creating, renaming, deleting, and switching named player profiles;
  seeding a Default profile; persisting and restoring the active profile across sessions;
  and the Settings UI section that hosts these actions.

### Modified Capabilities
- `wishlist-management`: Wishlists are now scoped to the active profile. The default
  wishlist is seeded per profile, the wishlist selector lists only the active profile's
  wishlists, and the active wishlist is tracked per profile.

## Impact

- **Data layer:** new `profiles` table; `wishlists.profile_id` FK (cascade delete);
  `user_preferences.last_profile_id`; new EF Core migration. Affects
  `ReliquaryDbContext`, `UserDataEntities`, and the migration snapshot.
- **Application ports:** new `IProfileStore`; `IWishlistStore` methods become
  profile-scoped (e.g. `GetAllAsync(profileId)`, `CreateAsync(profileId, name)`,
  `SeedDefaultAsync(profileId)`).
- **Presentation:** new `ProfileViewModel` (and a name dialog reused from wishlists);
  wiring in `SettingsView`/`SettingsViewModel` and `MainWindowShellViewModel` to reload
  wishlist-dependent views when the active profile changes.
- **Composition:** new `CreateProfileStore()` factory; `MainWindow` bootstrap seeds the
  default profile before the default wishlist.
- **Specs:** `wishlist-management` requirements updated; new `player-profiles` spec.
