## Context

The app already has a Prime Gear tab that shows which prime items are wishlisted. The missing piece is the inverse view: given my wishlist, which relics should I actually run? Currently there is no way to see this without an external relic reference. The data is all present in the database — relics, relic_rewards, mission_rewards, wishlist_items — it just hasn't been surfaced in a view.

The UI uses Avalonia MVVM with a static `TabControl` in `MainWindow.axaml`. ViewModels are constructed imperatively in `MainWindow.axaml.cs` and registered on `MainWindowShellViewModel`. All data access goes through Application-layer port interfaces backed by SQLite.

## Goals / Non-Goals

**Goals:**
- New "Wishlist Relics" tab showing relic cards for any relic that contains a wishlisted part
- Each card: header = relic display name, body = all rewards with wishlisted ones in bold
- Cards ordered era-first (Lith → Meso → Neo → Axi), then alphabetically by name within era
- Checkbox control to show/hide vaulted relics (default: hidden)
- Vaulted relics rendered with same reduced-opacity treatment as vaulted prime gear
- Updates when the active wishlist changes (reacts to `OnActiveWishlistChanged`)

**Non-Goals:**
- Editing the wishlist from this tab
- Showing mission sources for each relic
- Filtering by relic era or reward tier
- Persisting the show-vaulted toggle across sessions

## Decisions

### 1. New read port `IWishlistRelicsReader` rather than extending an existing port

A dedicated interface keeps the query cohesive and follows the existing pattern (`IPrimeGearReader`, `SqlitePrimeGearReader`). The alternative of bolting this onto `IPrimeGearReader` would couple two distinct concerns.

### 2. Relic vault status computed at import time and stored on the `relics` table

Following the same pattern as `gear_vault_status`, vault status is computed during the drop-table import phase and persisted rather than re-derived at query time. The `relics` table gains an `is_vaulted INTEGER NOT NULL DEFAULT 0` column. The import step sets it to `1` for any relic whose `id` does not appear in `mission_rewards.relic_id`.

This is consistent with how prime gear vault status is handled and keeps read queries simple (no subquery needed). The alternative — computing it at query time via `NOT IN (SELECT DISTINCT relic_id FROM mission_rewards ...)` — was rejected to keep the approach uniform and avoid repeating the subquery on every wishlist load.

### 3. Single flat `ObservableCollection<WishlistRelicCardViewModel>` with era group headers in XAML

An alternative would be separate collections per era. A flat list with a converter or group header items is simpler and matches how the Prime Gear tab uses `WrapPanel`. Era ordering is enforced by the query's ORDER BY clause, so XAML just needs to show era-label separators when the era changes (or use a `ListBox` with `GroupDescriptions`).

**Decision:** Use a flat `ObservableCollection` sorted by era then name. Render era section headers in XAML using a similar separator pattern to the Active/Vaulted separator already in the Prime Gear tab.

### 4. Reload strategy: rebuild collection on wishlist change

`WishlistRelicsViewModel` subscribes to `MainWindowShellViewModel.OnActiveWishlistChanged` (same pattern as `PrimeGearViewModel`). On change, call `LoadAsync()` which clears and repopulates the collection.

## Risks / Trade-offs

- **Stale data if import happens mid-session** → Mitigated: import triggers full reload of all VMs via existing event chain.
- **Empty state when wishlist is empty** → Show a placeholder message "Add items to your wishlist to see relics here."
- **Performance with large relic sets** → The query is a single JOIN across small tables (hundreds of rows); no concern.

## Migration Plan

Schema change: add `is_vaulted INTEGER NOT NULL DEFAULT 0` column to the `relics` table. Because the value is always recomputed on import, no data migration is needed — existing rows default to `0` and will be corrected on the next import. New code: reader interface, SQLite implementation, ViewModel, XAML wiring.
