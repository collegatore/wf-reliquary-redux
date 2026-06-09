## Context

The Prime Gear tab renders one card per gear with a checkbox list of parts; part checkboxes drive the **active wishlist** (`PrimeGearViewModel` + `PrimePartViewModel`, persisting through `IWishlistStore`). The recently-added profiles feature (`de8a2a3`) introduced `profiles` as a container above `wishlists`, with the active profile resolved from `user_preferences.last_profile_id`. Wishlist scoping already hides profile awareness inside `IWishlistStore`.

Ownership is conceptually account-level: a player owns a gear regardless of which wishlist is active. So owned state belongs at the **profile** level, parallel to (not inside) wishlists. This change adds a profile-scoped owned-gear store and surfaces ownership on the existing cards, following the established four-layer pattern (Application port → Infrastructure SQLite store → Composition factory → Presentation VM/AXAML) and the Show vaulted gear UI precedent.

## Goals / Non-Goals

**Goals:**
- Record Prime gear ownership per profile, surviving wishlist switches and drop-table re-imports.
- Add an owned overflow menu (a ⋮ button with a single state-aware entry) to each card header (right-aligned) with a floating hint.
- Add a Show owned gear control-panel toggle that hides owned cards by default and reveals them muted.
- Hide a just-owned card with a smooth fade rather than an abrupt removal.
- Reuse existing patterns: profile-scoped store like `IWishlistStore`, vault-status UI like Show vaulted gear, command pattern like the wishlist menu.

**Non-Goals:**
- Per-part ownership. Ownership is whole-gear only (a card-level concept), matching the user's "mark Prime Gear as owned" framing.
- Coupling ownership to wishlists or auto-removing owned parts from a wishlist.
- Any change to existing wishlist, vault, or part-checkbox behavior (additive only).
- Cross-profile aggregate views of owned gear.

## Decisions

### Ownership granularity: whole gear, keyed by gear name
Owned status is stored per `gear_name`, not per part. The card header is the natural home for a single owned toggle, and players think in terms of owning a complete weapon/frame. `gear_name` is a **soft reference** (plain TEXT, no FK to import tables) exactly like `wishlist_items.part_normalized_name`, so owned marks survive imports that truncate `reward_items`. Alternative — FK to a gear table — rejected: there is no stable gear table, and re-imports would orphan marks.

### New `owned_gear` table scoped by `profile_id`
Schema: `id` (PK, autoincrement), `profile_id` (INTEGER, FK → `profiles.id` `ON DELETE CASCADE`), `gear_name` (TEXT NOT NULL), with a unique index on `(profile_id, gear_name)`. Cascade delete means removing a profile removes its owned set, consistent with how `wishlists` cascade. The unique index makes "mark owned" idempotent via insert-or-ignore semantics and makes "clear" a targeted delete.

### New `IOwnedGearStore` port mirroring `IWishlistStore` profile resolution
The port resolves the active profile internally from `last_profile_id` (falling back to earliest profile), so Presentation never passes a profile id — identical to how `IWishlistStore` works. Surface:
- `Task<IReadOnlyList<string>> GetOwnedGearNamesAsync(ct)` — owned gear names for the active profile.
- `Task SetOwnedAsync(string gearName, bool owned, ct)` — insert-or-ignore when owned, delete when not.

Implemented by `SqliteOwnedGearStore` using EF Core (`ExecuteDeleteAsync` for clear; add + `SaveChangesAsync` guarded by the unique index for mark), following `SqliteProfileStore`/`SqliteWishlistStore` style. Active-profile resolution duplicates the small helper already used by the wishlist store rather than introducing shared infrastructure, keeping the change localized.

### Presentation: owned lives on the card, filtering lives on the view model
- `PrimeGearCardViewModel` becomes mutable enough to carry `IsOwned` (an `INotifyPropertyChanged` property that persists on set via the store, mirroring `PrimePartViewModel.IsChecked`). It also exposes a `MarkOwnedCommand` (toggles `IsOwned`) and an `OwnedMenuLabel` ("Mark as owned" / "Unmark as owned") for the overflow menu, an `IsFadingOut` flag for the hide animation, an `IOwnedGearStore` reference, and a callback so the parent VM can re-evaluate visibility.
- `PrimeGearViewModel` loads the owned set during `LoadAsync`, sets `IsOwned` on each card, and gains a `ShowOwned` bool. Owned filtering is applied when building/refreshing the `ActiveCards`/`VaultedCards` collections.

**Filtering approach:** keep the full built card list (`allCards`) separate from the bound `ActiveCards`/`VaultedCards`. Two update paths:
- **Bulk** (initial load, `ShowOwned` toggle) — `RebuildVisibleCards` clears and repopulates both collections from `allCards`, excluding owned cards unless `ShowOwned` is true. Cheap relative to a DB reload and only runs when many cards' visibility may change.
- **Single-card** (one card's owned state toggled) — a targeted add/remove of just that card (`OnCardOwnedChangedAsync` + ordered insert), avoiding regeneration of every card container. This is what removes the perceptible "grid rebuild" lag when marking one gear owned.

Owned cards, when shown, render muted by reusing the reduced-opacity treatment already applied to vaulted cards.

### Masking the hide latency: optimistic persist + fade + targeted update
Marking a gear owned while owned cards are hidden must feel instant. Three things combine:
1. **Overlapped persist** — `PersistOwnedAsync` starts the `SetOwnedAsync` DB write but updates the UI (fade + targeted remove) without waiting on it, then awaits the write at the end. The SQLite latency overlaps the animation instead of preceding the visual response.
2. **Fade-out** — the card's `IsFadingOut` flag drives a `Border` opacity transition (class-toggled, ~0.18s) to 0 before the card is removed from the bound collection. Opacity animations are composited on the render thread, so the cost is negligible.
3. **Targeted removal** — only the affected card leaves the collection, so other cards do not re-render.

The fade duration is an injectable `OwnedHideFadeDuration` (default `TimeSpan.Zero` so tests update synchronously; the view sets ~180 ms).

### Confirm-tracking dialog when owning gear with wished parts
Marking a gear owned often means the player no longer needs to farm its parts, but sometimes they want to keep the wishlist entries (e.g. for a duplicate or a friend). When the owned transition (`false`→`true`) happens on a card that has **wished parts** (parts checked in the active wishlist), a modal three-way dialog gates the action:
1. **Keep tracking** → persist owned; wishlist untouched.
2. **Release tracking** → persist owned; remove that gear's parts from the **active wishlist** via `IWishlistStore.RemoveItemAsync` for each wished part (scoped to the active wishlist, consistent with how part checkboxes already track only the active wishlist).
3. **Cancel** (it was a misclick) → do nothing; revert the owned mark.

The "hide card" effect is implicit (any owned card is hidden when Show owned gear is off); the dialog message conveys it, so the buttons stay short. The dialog follows the existing `ConfirmDiscardDialog` pattern: a `Window` subclass with a static `ShowAsync(Window owner, string gearName)` returning a typed result (`OwnedConfirmChoice`). `Cancel` is the enum's `0` value so dismissing the window via its chrome resolves to `Cancel`. Because view models must not reference `Window`, `PrimeGearViewModel`/`PrimeGearCardViewModel` expose a `Func<…, Task<OwnedConfirmChoice>>?` callback that `MainWindow.axaml.cs` wires to `OwnedGearTrackingDialog.ShowAsync(this, gearName)` — mirroring how `SettingsViewModel.ConfirmDiscard` is wired at `MainWindow.axaml.cs:82`. The dialog is only invoked when wished parts exist; clearing an owned mark and owning a gear with no wished parts bypass it entirely.

A Cancel must visibly revert the owned mark (and the menu label). The setter commits `IsOwned` immediately (so the action registers), and on Cancel `HandleOwnTransitionAsync` resets it back to `false` and raises `PropertyChanged` for both `IsOwned` and `OwnedMenuLabel` — a genuine `true`→`false` change so any bound control reliably reverts. Re-entrancy is avoided by short-circuiting when the value is unchanged and only running the confirm path on the `false`→`true` edge.

### UI placement
- **Owned overflow menu**: card header changes from a single `TextBlock` to a two-column layout (gear name stretch, ⋮ button right). The button is a flat, transparent `Button` with a `MenuFlyout` holding one `MenuItem` bound to `OwnedMenuLabel`/`MarkOwnedCommand`. The flyout uses `Placement="BottomEdgeAlignedRight"` with a small `HorizontalOffset` so it opens toward the gear name instead of centered under the button (the menu is far wider than the button, so right-aligning yields roughly an 80% leftward shift, width-independent). The floating hint uses `ToolTip.Tip` (bound to `OwnedMenuLabel`) with a show delay.
- **Show owned gear**: a new checkbox + label `StackPanel`, preceded by a vertical separator, added to the control-panel row that holds Show vaulted gear.
- **Fade**: each card `Border` carries an `Opacity` `Transition` and a `fading` style class bound to `IsFadingOut`; the muted (`owned`, `gear-vaulted`) opacities are class-driven so the `fading` class (declared last, Opacity 0) wins during the animation.

### No re-initialization required
Per the pre-V1 persistence rule, schema changes use the normal EF migration workflow. `owned_gear` is additive — existing data is untouched — so no wipe/re-init is needed. A standard `dotnet ef migrations add AddOwnedGear` produces the migration.

## Risks / Trade-offs

- **Rebuild cost on single-card toggle** → A full clear/repopulate regenerates every card container and was perceptible. Mitigated by the targeted add/remove path for single-card owned changes; the full `RebuildVisibleCards` runs only on load and `ShowOwned` toggle, where it is justified.
- **Owned card vanishes on mark when hidden** → Intended (declutter), and softened by the fade so it does not jump. The Show owned gear toggle makes it recoverable, and the muted styling on reveal signals it's still tracked. Mirrors how a part disappearing into a hidden section already works for vaulted.
- **Owned + vaulted interaction** → A gear can be both owned and vaulted. Owned filtering is applied independently to both sections, so an owned vaulted gear stays hidden under both toggles being off and appears (muted) only when both Show owned and Show vaulted are on. This is consistent and requires no special-casing.
- **Async setter reverting the owned mark** → On Cancel the `IsOwned` setter resets itself and re-raises `PropertyChanged` (for `IsOwned` and `OwnedMenuLabel`). Re-entrancy is avoided by short-circuiting when the value is unchanged and only running the confirm path on the `false`→`true` edge.
- **Optimistic persist ordering** → The UI updates before the DB write completes. For a local SQLite owned-gear toggle a failure is highly unlikely and pre-V1 acceptable; the write is still awaited (just last) so errors are not swallowed silently.
- **Release tracking scope** → "Release tracking" removes the gear's parts only from the **active wishlist**, not from every wishlist in the profile, matching the fact that the card's part checkboxes reflect only the active wishlist. A player tracking the gear in another wishlist keeps it there. Documented so the narrower scope is intentional, not an oversight.
- **Duplicated active-profile resolution** → `SqliteOwnedGearStore` repeats the profile-resolution query rather than sharing it. Trade-off accepted to keep the change self-contained; a shared helper can be extracted later if a third profile-scoped store appears.
