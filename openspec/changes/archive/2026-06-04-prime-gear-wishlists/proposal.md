## Why

The Prime Gear tab shows parts users may want to farm, but checked state is purely in-memory and lost on restart. Users need a way to track which parts they are actively farming across sessions and across drop-table imports.

## What Changes

- Gear card checkboxes now represent membership in the currently active wishlist, not ephemeral UI state
- A wishlist selector row is added above the gear cards in the Prime Gear tab
- Users can create, rename, and delete named wishlists
- Parts in a wishlist that disappear from a new import are shown with a ghost marker rather than silently dropped
- Wishlist data persists across app restarts and survives drop-table re-imports
- The active wishlist selection is remembered between sessions

## Capabilities

### New Capabilities

- `wishlist-management`: Create, rename, delete, and select named wishlists; last active wishlist persisted across restarts
- `wishlist-items`: Add and remove Prime Gear parts from wishlists; parts referenced by stable normalized name so wishlist contents survive re-imports

### Modified Capabilities

- `prime-gear-parts-view`: Gear card checked state now reflects active wishlist membership; cards also render ghost entries for wishlisted parts absent from the current import

## Impact

- **Schema**: Three new tables (`wishlists`, `wishlist_items`, `user_preferences`) added to `drop-table-import.sqlite`; `CreateSchemaAsync` extended; `ClearSnapshotAsync` deliberately unchanged (user tables must not be cleared on import)
- **Application layer**: New port interface `IWishlistStore`
- **Infrastructure layer**: New `SqliteWishlistStore` implementing `IWishlistStore`
- **Presentation layer**: New `WishlistViewModel`; `PrimeGearViewModel` updated to source checked state from the active wishlist and include ghost entries; `MainWindow.axaml` updated with wishlist selector row
- **No breaking changes to existing import pipeline or drop-table queries**
