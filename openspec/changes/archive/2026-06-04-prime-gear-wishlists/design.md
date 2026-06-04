## Context

The Prime Gear tab currently shows gear cards with per-part checkboxes, but checked state is purely in-memory (`PrimePartViewModel.IsChecked`) and lost on restart. The drop-table import pipeline fully replaces the `reward_items` table on each run, resetting all integer primary keys. Any persistence mechanism that references item IDs directly would break on the next import.

The existing database is a single SQLite file (`drop-table-import.sqlite`). Import data is wiped and replaced by `ClearSnapshotAsync`, which deletes a fixed list of named tables. Tables not on that list survive imports untouched.

`reward_items` already enforces `normalized_name TEXT NOT NULL UNIQUE` — a stable, name-derived key that survives re-imports.

## Goals / Non-Goals

**Goals:**
- Persist part checked state in named wishlists that survive both app restarts and drop-table re-imports
- Allow users to create, rename, and delete wishlists
- Surface parts that are wishlisted but absent from the current import (ghost items), with a clear visual marker and tooltip
- Remember the last active wishlist across sessions
- Keep the import pipeline completely unmodified

**Non-Goals:**
- Sharing wishlists between users
- Wishlist reordering or prioritisation
- Filtering/searching within a wishlist
- Tracking obtained parts (checked = "I want this", not "I have this")

## Decisions

### Single SQLite file for user data

**Decision:** Add wishlist tables to `drop-table-import.sqlite` rather than a separate file.

**Rationale:** Ghost item detection requires a LEFT JOIN between `wishlist_items` and `reward_items`. SQLite cannot JOIN across separate connection strings; cross-file detection would require loading both sets into application memory and joining in C#. Keeping everything in one file makes ghost detection a single idiomatic LEFT JOIN query. The real risk (import pipeline accidentally wiping user data) is mitigated by the fact that `ClearSnapshotAsync` already deletes only explicitly named tables — adding a comment there is sufficient protection.

**Alternatives considered:** Separate `user-data.sqlite`. Rejected due to the cross-file query complexity it introduces.

---

### `normalized_name` as the stable wishlist item key

**Decision:** `wishlist_items` references items by `part_normalized_name TEXT` (matching `reward_items.normalized_name`), not by integer ID.

**Rationale:** Integer IDs in `reward_items` are `AUTOINCREMENT` and reset on every import. `normalized_name` is derived from the display name and is already enforced as UNIQUE. It is stable across imports as long as DE does not rename the item.

**Consequence:** If DE renames a Prime Part, the normalized name changes and the wishlist item becomes a permanent ghost. This is an acceptable edge case given how rarely DE renames parts.

---

### Denormalised display fields in `wishlist_items`

**Decision:** Store `gear_name` and `part_display_name` directly on `wishlist_items`.

**Rationale:** Ghost items have no matching row in `reward_items`. The card renderer needs a gear name and part label to display them. Denormalising these fields at insert time lets ghost items render without any fallback logic or null-handling.

---

### Seeded "Default" wishlist; last wishlist always exists

**Decision:** On first DB open (empty `wishlists` table), seed a "Default" wishlist and record it in `user_preferences`. Prevent deleting the last remaining wishlist.

**Rationale:** Gear cards have no meaning outside a wishlist context. There is no valid zero-wishlist state. Seeding "Default" on first open eliminates the zero-state entirely. Disabling the delete action when only one list remains keeps the invariant in place without error dialogs.

---

### Wishlist management in dropdown bottom section; modals for name input

**Decision:** All management actions (Rename, Delete, New) live in the bottom section of the wishlist selector dropdown, separated by a divider. Name-input flows (New and Rename) use modal dialogs.

**Rationale:** Rename and Delete are infrequent. Keeping them in the dropdown rather than the toolbar avoids cluttering the Prime Gear tab header. Modal dialogs for name input are the simplest Avalonia pattern and make confirm/cancel intent explicit.

---

### Immediate persistence on check/uncheck

**Decision:** Toggling a checkbox fires a database write immediately (INSERT or DELETE on `wishlist_items`). No explicit Save button.

**Rationale:** Gear cards represent wishlist state directly. An intermediate "pending" state would require reconciliation logic and a save/discard mechanism. Immediate writes keep the model simple and match the expected desktop behaviour for this kind of toggle.

---

### `IWishlistStore` port interface

**Decision:** Introduce `IWishlistStore` in the Application layer with implementations in Infrastructure, following the existing `IDropTableImportStore` / `IPrimeGearReader` pattern.

**Rationale:** Consistent with existing architecture. Enables unit testing of `WishlistViewModel` and `PrimeGearViewModel` without a real database.

## Risks / Trade-offs

**Risk: Developer accidentally adds wishlist tables to `ClearSnapshotAsync`**
→ Add a prominent comment in `ClearSnapshotAsync` warning that tables below a certain line are user data and must not be cleared. Low likelihood; easy to catch in code review.

**Risk: Part normalized name changes across a DE import (rename)**
→ The wishlist item becomes a permanent ghost. User must manually uncheck and re-add. Acceptable — DE renames are extremely rare and the ghost marker makes the situation visible.

**Risk: Schema migration drops wishlist tables**
→ Existing migration checks in `CreateSchemaAsync` target specific legacy columns/indexes. New wishlist tables use new names. No collision risk unless a future migration explicitly targets them.

## Migration Plan

No migration required. This is a purely additive schema change. On first launch after the update:

1. `CreateSchemaAsync` runs (it runs on every import, but wishlist table creation is idempotent via `CREATE TABLE IF NOT EXISTS`)
2. On app startup, the wishlist store seeds "Default" if `wishlists` is empty
3. Existing import data is unaffected

**Rollback:** Remove the wishlist tables via a schema migration drop. User wishlist data is lost on rollback. No other data is affected.

## Open Questions

None — all design decisions were resolved during exploration.
