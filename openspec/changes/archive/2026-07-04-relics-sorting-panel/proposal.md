# Relics Sorting Panel

## Why

Relics on the Relics and Missions tab are always shown in a fixed order (era Lith→Meso→Neo→Axi, then name), baked into the SQL query. Users planning fissure runs want to reorder relics by what matters in the moment — e.g. the relics with the most/rarest wishlisted rewards first. There is no way to do that today.

Additionally, the Relics control bar still uses inline checkboxes while the Prime Gear tab was just converted to a Filters-button + collapsible-panel pattern; this change brings the Relics bar in line.

## What Changes

- Add a **Sorting** button to the Relics control bar that toggles a collapsible sorting panel (mirroring the Prime Gear Filters pattern). The panel hosts a sort-mode dropdown and an asc/desc direction button. Sorting applies to all three relic view modes (Verbatim, Compact, Table).
- Three sort modes:
  1. **Era** — era rank Lith→Meso→Neo→Axi, then name (current behavior; remains the default).
  2. **Alphabetical** — pure relic display-name sort.
  3. **Wishlist score** — sum over wishlisted rewards of a tier multiplier (Common 3, Uncommon 4, Rare 5).
- Direction semantics: direction applies to the primary key chain only; relic names always sort A→Z, except in Alphabetical mode where name is the primary key. Selecting a mode applies that mode's default direction (asc for Era/Alphabetical, desc for Wishlist score); the direction button then toggles freely.
- Persist the last-used sort mode + direction in `UserSettings` (alongside the already-persisted view mode). Panel open/closed state is session-only; closing the panel does not reset the sort. The Sorting button label reflects the active mode and direction.
- Move the **Show vaulted** and **Show all rewards** checkboxes into a collapsible **Filters** panel behind a Filters button, cloning the Prime Gear filter-panel pattern. The "Show all rewards hidden in Table mode" rule carries into the panel. Filter checkbox states remain session-only.
- Move relic-level ordering out of SQL into the ViewModel: `GetWishlistRelics.sql` drops its era `CASE` sort key (keeping `display_name` for row-grouping contiguity and tier/`source_order` for within-relic reward order); `WishlistRelicsViewModel.RebuildViews` sorts the visible entries before fanning out to Cards and TableRows, so all view modes stay consistent from a single sort point.

## Capabilities

### New Capabilities

- `relics-sorting-panel`: Sorting button + collapsible panel on the Relics control bar; sort modes (Era, Alphabetical, Wishlist score), direction semantics and per-mode defaults, wishlist-score formula, and persistence of the sort mode + direction.
- `relics-filter-panel`: Filters button + collapsible panel on the Relics control bar hosting the Show vaulted and Show all rewards checkboxes.

### Modified Capabilities

- `wishlist-relics-view`: The Relics control panel shows Filters and Sorting buttons instead of inline checkboxes ("Wishlist Relics tab exists in main navigation" requirement); "Cards are ordered by era then alphabetically by name" becomes "Cards follow the active sort" with Era/asc as the default; the Show all rewards and Show vaulted checkbox requirements relocate the checkboxes into the Filters panel.
- `relic-view-modes`: Table mode's "Rows SHALL be ordered by era then alphabetically" requirement changes to "Rows follow the active sort, matching the card modes".

## Impact

- `src/Presentation/WF.Reliquary.Presentation/WishlistRelicsViewModel.cs` — sort mode/direction state, sorting in `RebuildViews`, panel-toggle commands, settings wiring.
- `src/Presentation/WF.Reliquary.Presentation/RelicsPanelView.axaml` — control bar restructure, Filters and Sorting sub-panels.
- `src/Application/WF.Reliquary.Application/Settings/UserSettings.cs` and `src/Infrastructure/WF.Reliquary.Infrastructure/Settings/LocalUserSettingsStore.cs` — new persisted sort mode + direction (backward-compatible defaulted record parameters).
- `src/Infrastructure/WF.Reliquary.Infrastructure/Storage/Queries/GetWishlistRelics.sql` — era sort key removed.
- Tests: `WishlistRelicsTableRowsTests.Rows_FollowEraThenInputOrder` rewritten as ViewModel-sort coverage; new unit/integration tests for sort modes, direction defaults, score computation, and settings round-trip.
- No schema/EF migration; no breaking changes (settings additions are defaulted).
