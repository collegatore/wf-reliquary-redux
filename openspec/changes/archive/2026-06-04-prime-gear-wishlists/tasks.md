## 1. Schema — Extend CreateSchemaAsync

- [x] 1.1 Add `wishlists`, `wishlist_items`, and `user_preferences` table DDL to `CreateSchemaAsync` in `SqliteDropTableImportStore`, using `CREATE TABLE IF NOT EXISTS`
- [x] 1.2 Add a prominent comment in `ClearSnapshotAsync` marking the user data tables and stating they must not be added to the delete list

## 2. Application Layer — IWishlistStore Port

- [x] 2.1 Create `WishlistEntry` record (Id, Name, CreatedAt) in the Application layer
- [x] 2.2 Create `WishlistItem` record (WishlistId, PartNormalizedName, GearName, PartDisplayName, IsGhost) in the Application layer
- [x] 2.3 Define `IWishlistStore` interface with methods: `SeedDefaultAsync`, `GetAllAsync`, `GetActiveAsync`, `SetActiveAsync`, `CreateAsync`, `RenameAsync`, `DeleteAsync`, `GetItemsAsync`, `AddItemAsync`, `RemoveItemAsync`

## 3. Infrastructure Layer — SqliteWishlistStore

- [x] 3.1 Create `SqliteWishlistStore` implementing `IWishlistStore`, reusing the same connection string as `SqliteDropTableImportStore`
- [x] 3.2 Implement `SeedDefaultAsync`: if `wishlists` is empty, insert "Default" and set `user_preferences.last_wishlist_id`
- [x] 3.3 Implement `GetAllAsync`: return all wishlists ordered by creation date
- [x] 3.4 Implement `GetActiveAsync` / `SetActiveAsync`: read and write `user_preferences.last_wishlist_id`; fall back to first available wishlist if the stored ID is missing
- [x] 3.5 Implement `CreateAsync`: insert wishlist row, return new entry
- [x] 3.6 Implement `RenameAsync`: update wishlist name by ID
- [x] 3.7 Implement `DeleteAsync`: delete wishlist row (cascade removes items); caller is responsible for switching active wishlist first
- [x] 3.8 Implement `GetItemsAsync`: LEFT JOIN `wishlist_items` with `reward_items` on `normalized_name`; populate `IsGhost` flag from null match
- [x] 3.9 Implement `AddItemAsync`: INSERT OR IGNORE into `wishlist_items` with `gear_name` and `part_display_name` denormalised
- [x] 3.10 Implement `RemoveItemAsync`: DELETE from `wishlist_items` by `wishlist_id` + `part_normalized_name`

## 4. Presentation Layer — WishlistViewModel

- [x] 4.1 Create `WishlistViewModel` with: observable collection of wishlist names, active wishlist, and commands for Create, Rename, Delete, and Select
- [x] 4.2 Implement `CreateCommand`: open modal dialog for name input; on confirm call `IWishlistStore.CreateAsync` then `SetActiveAsync`, reload gear cards
- [x] 4.3 Implement `RenameCommand`: open modal dialog pre-filled with current name; on confirm call `IWishlistStore.RenameAsync`, update selector label
- [x] 4.4 Implement `DeleteCommand`: call `IWishlistStore.DeleteAsync`, switch active to next available wishlist, reload gear cards; command is disabled when only one wishlist exists
- [x] 4.5 Implement `SelectCommand`: call `IWishlistStore.SetActiveAsync`, reload gear cards
- [x] 4.6 Wire `WishlistViewModel` into the main `MainWindowViewModel` / DI composition root alongside `PrimeGearViewModel`
- [x] 4.7 Call `SeedDefaultAsync` at app startup before constructing `WishlistViewModel`

## 5. Presentation Layer — Update PrimeGearViewModel and PrimePartViewModel

- [x] 5.1 Add `IsGhost` property to `PrimePartViewModel`
- [x] 5.2 Update `PrimeGearViewModel.LoadAsync` to: load wishlist items from `IWishlistStore.GetItemsAsync`, load import parts from `IPrimeGearReader.GetAllAsync`, merge into a unified display set (ghost items added to their gear card, or a new ghost-only card if the gear has no import parts at all)
- [x] 5.3 Update `PrimePartViewModel.IsChecked` setter to call `IWishlistStore.AddItemAsync` / `RemoveItemAsync` on the active wishlist; pass `normalized_name`, `gear_name`, and `part_display_name` at construction time
- [x] 5.4 Update `PrimeGearViewModel.ReloadAsync` to re-fetch wishlist items when the active wishlist changes

## 6. UI — Wishlist Selector and Ghost Item Rendering

- [x] 6.1 Add a wishlist selector row above the gear cards in `MainWindow.axaml`; bind to `WishlistViewModel`
- [x] 6.2 Implement the dropdown with list entries at the top and a divider + management actions (Rename, Delete, New) at the bottom; disable Delete when `WishlistViewModel.CanDelete` is false
- [x] 6.3 Add ghost item visual marker to the part row template (e.g. warning icon, distinct foreground) conditioned on `IsGhost`
- [x] 6.4 Add `ToolTip.Tip` binding to ghost part rows: "This part is not present in the current drop table import"

## 7. Tests

- [x] 7.1 Add integration tests for `SqliteWishlistStore`: seed, CRUD on wishlists, add/remove items, ghost detection query, last-active persistence, cascade delete
- [x] 7.2 Add unit tests for `WishlistViewModel`: create/rename/delete commands, CanDelete guard, active selection updates
- [x] 7.3 Add unit tests for `PrimeGearViewModel` load logic: merged display set contains import parts + ghost items; checked state initialises from wishlist; check/uncheck calls store methods
