## 1. Data model & schema

- [x] 1.1 Add `ProfileEntity` (`Id`, `Name`, `CreatedAt`, `ActiveWishlistId` nullable) to `UserDataEntities.cs`
- [x] 1.2 Add `ProfileId` to `WishlistEntity`; remove `LastWishlistId` and add `LastProfileId` to `UserPreferencesEntity`
- [x] 1.3 Map `profiles` in `ReliquaryDbContext.OnModelCreating` (table, keys, `active_wishlist_id` FK → wishlists `SetNull`)
- [x] 1.4 Add `DbSet<ProfileEntity> Profiles`; map `wishlists.profile_id` FK (cascade delete) and `user_preferences.last_profile_id` FK (SetNull)
- [x] 1.5 Generate EF Core migration `AddPlayerProfiles`

## 2. Migration data move

- [x] 2.1 In migration `Up`: create `profiles`, insert a Default profile, and capture its id
- [x] 2.2 Add `wishlists.profile_id`, set all existing rows to the Default profile id, then add the cascade FK
- [x] 2.3 Add `user_preferences.last_profile_id` set to the Default profile id; copy existing `last_wishlist_id` into the Default profile's `active_wishlist_id`, then drop `last_wishlist_id`
- [x] 2.4 Implement `Down` to reverse the changes (restore `last_wishlist_id`, drop `profile_id` and `profiles`)
- [x] 2.5 Update `ReliquaryDbContextModelSnapshot.cs` to match

## 3. Application ports

- [x] 3.1 Add `IProfileStore` with `SeedDefaultAsync`, `GetAllAsync`, `GetActiveAsync`, `SetActiveAsync(profileId)`, `CreateAsync(name)`, `RenameAsync(profileId, name)`, `DeleteAsync(profileId)`
- [x] 3.2 Add a `ProfileEntry` record (`Id`, `Name`, `CreatedAt`) under `Application`
- [x] 3.3 Document on `IWishlistStore` that `SeedDefaultAsync`/`GetAllAsync`/`GetActiveAsync`/`CreateAsync`/`SetActiveAsync` now operate within the active profile

## 4. Infrastructure stores

- [x] 4.1 Add `SqliteProfileStore : IProfileStore` mirroring `SqliteWishlistStore` (seed Default, list ordered by created, active via `last_profile_id` with first-profile fallback, create/rename/delete)
- [x] 4.2 Update `SqliteWishlistStore` to resolve the active profile (`last_profile_id`, fallback earliest) and scope `GetAllAsync`/`GetActiveAsync`/`CreateAsync`/`SeedDefaultAsync` to it
- [x] 4.3 Change `SqliteWishlistStore.SetActiveAsync`/`GetActiveAsync` to read/write `profiles.active_wishlist_id` (with earliest-wishlist-in-profile fallback) instead of `user_preferences.last_wishlist_id`

## 5. Composition

- [x] 5.1 Add `ReliquaryServiceFactory.CreateProfileStore()`
- [x] 5.2 In `MainWindow` production ctor, seed the default profile before seeding the default wishlist

## 6. Presentation

- [x] 6.1 Add `ProfileViewModel` + `ProfileEntryViewModel` mirroring `WishlistViewModel` (list, active entry, Create/Rename/Delete, `CanDelete = Count > 1`), reusing `WishlistNameDialog`
- [x] 6.2 Promote `WishlistViewModel.LoadAsync` to a public `ReloadAsync()`
- [x] 6.3 Add `OnActiveProfileChanged` to `ProfileViewModel`; in `MainWindowShellViewModel` wire it to reload the wishlist selector then Prime Gear, Relics, and Farming views
- [x] 6.4 Inject `ProfileViewModel` into `MainWindowShellViewModel` and construct it in `MainWindow`
- [x] 6.5 Add a Profiles section to `SettingsView.axaml` (heading, selector, Create/Rename/Delete actions, separated by `Separator`); bind to `Settings`-sibling `Profiles` VM

## 7. Tests

- [x] 7.1 Profile store tests: seed Default, create/rename/delete, can't-delete-last, active persistence, per-profile active wishlist
- [x] 7.2 Wishlist store tests: queries scoped to active profile; switching profile changes listed/active wishlists; cascade delete removes a profile's wishlists and items
- [x] 7.3 Migration test: existing wishlists land in Default profile and prior `last_wishlist_id` becomes the Default profile's active wishlist
- [x] 7.4 `ProfileViewModel` tests mirroring `WishlistViewModel` tests
- [x] 7.5 Run full build and test suite; fix regressions

## 8. Docs

- [x] 8.1 Update `docs/technical-reference.md` (data model / schema section) to describe profiles and wishlist scoping
