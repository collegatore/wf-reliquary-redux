## Why

Players already own much of the Prime gear shown on the Prime Gear tab, but the app gives no way to record that. Owned gear clutters the cards view and competes for attention with gear the player is still farming. Ownership is a property of a player's account, not of a single wishlist, so it belongs at the profile level — surviving wishlist switches and drop-table re-imports.

## What Changes

- A player can mark an entire Prime gear item as **owned** via an overflow menu (a vertical three-dot "⋮" button) on the right side of each gear card header. The menu has a single state-aware entry — **Mark as owned** / **Unmark as owned** — and a floating hint on the button.
- Owned status is scoped to the **active profile**, not to a wishlist. Switching wishlists within a profile preserves owned marks; switching profiles shows that profile's own owned set.
- When marking a gear owned that still has **wished parts** in the active wishlist, a modal dialog offers a three-way choice: **Keep tracking**, **Release tracking** (removes the gear's parts from the active wishlist), or **Cancel** (it was a misclick — no owned mark).
- Owned gear is **hidden by default** to declutter the view. A new **Show owned gear** checkbox in the Prime Gear control panel reveals owned cards (visually muted), mirroring the existing Show vaulted gear behavior. A card marked owned while owned gear is hidden **fades out** before being removed.
- Owned status persists across application sessions and survives drop-table re-imports (soft reference by gear name, no FK to import tables).

## Capabilities

### New Capabilities

- `prime-gear-owned-status`: Profile-scoped marking of Prime gear as owned, the overflow menu and floating hint on each gear card header, the confirm-tracking dialog for wished parts, the Show owned gear control-panel toggle, and the fade-out hide/reveal behavior for owned cards.

### Modified Capabilities

<!-- No requirement changes to existing specs; the owned menu and toggle are additive and captured entirely in the new capability. -->

## Impact

- **Application**: new port `IOwnedGearStore` (profile-scoped, mirroring `IWishlistStore`); new domain type for an owned-gear entry.
- **Infrastructure**: new `SqliteOwnedGearStore`; new `OwnedGearEntity`, DbSet, and model configuration; new EF Core migration `AddOwnedGear` adding an `owned_gear` table (FK `profile_id` → `profiles` `ON DELETE CASCADE`, unique on `(profile_id, gear_name)`).
- **Composition**: new `CreateOwnedGearStore` factory method.
- **Presentation**: `PrimeGearCardViewModel` gains an `IsOwned` property (with a `MarkOwnedCommand` and a state-aware menu label) that persists on toggle; `PrimeGearViewModel` gains a `ShowOwned` toggle and owned-filtering of displayed cards; new `OwnedGearTrackingDialog` for the three-way confirm; `MainWindow.axaml` adds the overflow menu + hint to the card header, the Show owned gear checkbox to the control panel, and a fade transition on cards.
- **Persistence note**: this change adds a schema migration (`owned_gear`); no re-initialization of existing data is required.
- **Tests**: infrastructure tests for `SqliteOwnedGearStore`, migration tests, and `PrimeGearViewModel` tests for owned toggle, filtering, and the confirm-tracking flow.
