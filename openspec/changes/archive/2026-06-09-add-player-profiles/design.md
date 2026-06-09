## Context

Wishlists currently live in a single flat namespace. The `wishlists` table holds
`(id, name, created_at)`, items hang off `wishlist_id` (cascade delete), and the active
wishlist is tracked in the singleton `user_preferences.last_wishlist_id`. The
`WishlistViewModel` drives a selector on the Prime Gear tab; downstream views
(`PrimeGearViewModel`, `WishlistRelicsViewModel`, `WishlistFarmingMissionsViewModel`)
each resolve the active wishlist by calling the **no-argument** `IWishlistStore.GetActiveAsync()`.

Players with multiple accounts want their wishlists partitioned per account. This change
introduces **profiles** as a container one level above wishlists, with CRUD living in a
new Settings section. Profile CRUD intentionally mirrors the existing wishlist CRUD
(seeded Default, modal name dialog, can't-delete-last-one), but its control surface is in
Settings rather than the Prime Gear tab.

## Goals / Non-Goals

**Goals:**
- A first-class `profiles` concept that scopes wishlists; deleting a profile removes its
  wishlists and their items.
- Active profile persisted across sessions; each profile remembers its own active wishlist.
- Profile CRUD in a dedicated Settings section, behaviorally consistent with wishlist CRUD.
- Keep churn in downstream views minimal.

**Non-Goals:**
- No import/export, sharing, or syncing of profiles.
- No per-profile settings beyond wishlists (font size, source URL, etc. stay global).
- No UI for moving a wishlist between profiles.
- No renaming/restructuring of the existing wishlist selector placement.

## Decisions

### Scope by *persisted active profile*, not by threading `profileId` everywhere
The store internally resolves the "active profile" from a persisted preference, and the
existing `IWishlistStore` query methods (`GetAllAsync`, `GetActiveAsync`, `CreateAsync`,
`SetActiveAsync`, `SeedDefaultAsync`) keep their current no-`profileId` signatures but now
operate **within the active profile**.

- **Why:** `PrimeGearViewModel`, `WishlistRelicsViewModel`, and
  `WishlistFarmingMissionsViewModel` all call `GetActiveAsync()` with no arguments. If the
  store scopes to the active profile internally, those three views need **zero changes** —
  after a profile switch they simply reload and observe the new profile's active wishlist.
- **Alternative considered:** adding a `profileId` parameter to every wishlist-store
  method and threading the active profile id through all view models. Rejected as
  higher-churn and error-prone for no functional gain, since exactly one profile is active
  at a time.

### Active-wishlist memory stored per profile
Add a nullable `active_wishlist_id` column to `profiles` (FK → `wishlists`, `ON DELETE SET
NULL`). The singleton `user_preferences` gains `last_profile_id` (FK → `profiles`, `ON
DELETE SET NULL`) and **loses** `last_wishlist_id`.

- `IProfileStore.GetActiveAsync()` resolves `last_profile_id`, falling back to the
  earliest-created profile if it is null/dangling.
- `IWishlistStore.GetActiveAsync()` resolves the active profile, then that profile's
  `active_wishlist_id`, falling back to the profile's earliest-created wishlist.
- `IWishlistStore.SetActiveAsync(wishlistId)` writes `profiles.active_wishlist_id` for the
  active profile.
- **Why per-profile memory:** switching back to a profile should restore the wishlist the
  user last had selected there. Storing it on the profile row makes that natural and lets
  the FK clean up automatically when a wishlist is deleted.
- **Alternative considered:** keeping a single global `last_wishlist_id`. Rejected because
  it would forget each profile's selection on switch.

### New `IProfileStore` mirrors the wishlist store shape
`SeedDefaultAsync`, `GetAllAsync`, `GetActiveAsync`, `SetActiveAsync(profileId)`,
`CreateAsync(name)`, `RenameAsync(profileId, name)`, `DeleteAsync(profileId)`. Backed by a
new `SqliteProfileStore`, constructed via `ReliquaryServiceFactory.CreateProfileStore()`.
`DeleteAsync` mirrors wishlist deletion: callers must switch the active profile before
deleting the active one, and cascade removes the profile's wishlists and items.

### Bootstrap ordering
On startup (`MainWindow` production ctor): seed the default profile first, then seed the
default wishlist (which lands in the now-active profile). Order matters because wishlist
seeding needs an active profile to attach to.

### Presentation wiring
- New `ProfileViewModel`, structurally a sibling of `WishlistViewModel` (observable list,
  active entry, Create/Rename/Delete commands, `CanDelete = count > 1`). It reuses the
  existing `WishlistNameDialog` for name entry (the dialog is generic name-prompt UI).
- `WishlistViewModel.LoadAsync` is promoted to a public `ReloadAsync()` so the shell can
  refresh the selector after a profile switch.
- `MainWindowShellViewModel` wires `ProfileViewModel.OnActiveProfileChanged` to: reload the
  wishlist selector, then reload Prime Gear, Relics, and Farming views — the same fan-out
  already used for `OnActiveWishlistChanged`.
- The Profiles section renders in `SettingsView.axaml` as a new block (heading + selector
  + Create/Rename/Delete actions), separated by a `Separator` like the existing sections.
  It is **not** gated by the Settings Save/Back unsaved-changes flow — profile mutations
  persist immediately, exactly as wishlist mutations do today.

## Risks / Trade-offs

- **Migration of live user data** → The migration seeds one Default profile, assigns all
  existing wishlists to it, and moves the old `last_wishlist_id` into that profile's
  `active_wishlist_id`. Done in a single migration with explicit data-copy SQL before
  dropping `last_wishlist_id`, so no wishlist or selection is lost.
- **SQLite column drop requires table rebuild** → EF Core's SQLite provider handles the
  `user_preferences` rebuild when dropping `last_wishlist_id`; the migration must run the
  data copy into `profiles.active_wishlist_id` *before* the drop.
- **Settings section persists immediately while other Settings fields are deferred** →
  Mixed save semantics on one screen could confuse users. Mitigation: the Profiles section
  has its own clearly separated heading and its own action buttons, matching the
  already-familiar wishlist selector behavior; no Save button implies its actions.
- **Two stores touch `user_preferences`** → `IProfileStore` owns `last_profile_id` and
  `IWishlistStore` reads it to scope queries. Mitigation: `last_profile_id` is
  written only by the profile store; the wishlist store treats it as read-only input.

## Migration Plan

1. Add `ProfileEntity`; add `ProfileId` to `WishlistEntity`; add `LastProfileId` to
   `UserPreferencesEntity` and remove `LastWishlistId`; map all in `ReliquaryDbContext`.
2. Generate EF Core migration. In its `Up`:
   - Create `profiles` (`id`, `name`, `created_at`, `active_wishlist_id` nullable FK).
   - Insert a `Default` profile.
   - Add `profile_id` to `wishlists`; `UPDATE wishlists SET profile_id = <default id>`;
     add the FK with cascade delete.
   - Add `last_profile_id` to `user_preferences`; set it to the default profile id.
   - Copy existing `last_wishlist_id` into the default profile's `active_wishlist_id`,
     then drop `last_wishlist_id`.
3. Rollback (`Down`) reverses: restore `last_wishlist_id`, drop `profile_id` and the
   `profiles` table.

## Open Questions

- None blocking. Profile names are not required to be unique (matching wishlists); if
  uniqueness is later desired it can be added without schema-breaking change.
