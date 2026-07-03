# Design — Relics Sorting Panel

## Context

Relic ordering currently lives in `GetWishlistRelics.sql` (`ORDER BY` era CASE, display_name, tier CASE, source_order). `WishlistRelicsViewModel.RebuildViews()` preserves reader order and fans the same filtered list out to `Cards` (Verbatim/Compact) and `TableRows` (Table), so all view modes render the same sequence. The Prime Gear tab already established the collapsible-panel pattern this change reuses: a transparent button on the control bar toggles an `IsFilterPanelOpen` flag that shows a second `Border` row (commit 82484b9).

`UserSettings` is a record with defaulted trailing parameters persisted by `LocalUserSettingsStore`; `RelicViewMode ViewMode` was added exactly that way, so new settings fields are backward-compatible by construction.

## Goals / Non-Goals

**Goals:**
- User-selectable relic sort (mode + direction) applied identically in all three view modes.
- Sorting panel and Filters panel behind control-bar buttons, matching the Prime Gear pattern.
- Persist sort mode + direction across sessions.
- Single sort point in the ViewModel; SQL keeps only correctness-relevant ordering.

**Non-Goals:**
- Sorting the farming-missions panel (unchanged).
- Persisting filter checkbox states (`ShowVaulted`/`ShowAllRewards` stay session-only).
- Sorting rewards *within* a relic (fixed tier order, unchanged).
- Mission/relic data or schema changes; no EF migration.

## Decisions

### D1: Sort in the ViewModel, not SQL
`RebuildViews()` sorts the visible entries once, before `RebuildCards`/`RebuildTableRows`, so every view mode stays consistent for free and re-sorting never requires a DB round-trip. All data the score needs (`RewardTier`, `IsWishlisted`) is already on `WishlistRelicEntry`.

*Alternative considered:* parameterized ORDER BY in SQL — rejected: requires dynamic SQL or query variants, a reload per sort change, and duplicates tier-multiplier logic in a harder-to-test layer.

### D2: Trim SQL ORDER BY, don't delete it
`GetWishlistRelics.sql` keeps `ORDER BY r.display_name, tier CASE, rr.source_order` and drops only the era CASE:
- `display_name` is a **correctness** requirement: `SqliteWishlistRelicsReader.BuildEntries` groups rows by adjacency (new entry when `DisplayName` changes); without contiguity the reader would emit duplicate entries.
- `tier CASE` + `source_order` provide deterministic within-relic reward order; `source_order` exists only in SQL (not carried into the row DTO), so it cannot move to the ViewModel.
- The era CASE is purely presentational and becomes dead once the ViewModel always sorts.

The now-obsolete "order comes from the reader" comment in `RebuildViews` is updated.

### D3: Sort semantics — direction applies to the primary key chain; names always A→Z
| Mode | Ascending | Descending |
|---|---|---|
| Era | Lith→Axi, names A→Z | Axi→Lith, names A→Z (half-reversed) |
| Alphabetical | names A→Z | names Z→A (name *is* the primary key) |
| Wishlist score | score low→high; ties: era Lith→Axi, names A→Z | score high→low; ties: era Axi→Lith, names A→Z |

Wishlist score = Σ over rewards with `IsWishlisted` of tier multiplier: Common 3, Uncommon 4, Rare 5. Score ties break by era rank (following the active direction) then name (always ascending).

*Alternative considered:* full-reversal descending (names Z→A too) — rejected per user decision; half-reversed keeps names scannable.

### D4: Mode selection applies the mode's default direction
Selecting a mode in the dropdown resets direction to that mode's default: **asc** for Era and Alphabetical, **desc** for Wishlist score. The direction button then toggles freely. This rule fires only on user-initiated mode changes — on startup the persisted mode + direction load as-is (no default-direction override).

### D5: Persistence via defaulted UserSettings fields
Two new trailing record parameters on `UserSettings`: `RelicSortMode SortMode = RelicSortMode.Era` and `bool SortDescending = false`, plus matching nullable fields in `LocalUserSettingsStore`'s persistence DTO. `RelicSortMode` is a new enum in `WF.Reliquary.Application.Settings` beside `RelicViewMode`. Existing settings files load with defaults — no migration.

Wiring follows the `ViewMode` precedent: the shell loads settings into `WishlistRelicsViewModel` at startup and saves on change.

### D6: UI structure clones the Prime Gear filter panel
Control bar keeps the "Relics:" label and busy indicator, and replaces the two inline checkboxes with two transparent buttons (`Filters`, `Sorting`), each toggling an `IsFilterPanelOpen`/`IsSortPanelOpen` flag that shows its own `Border` row below the bar. Both panels may be open simultaneously (they stack, like Prime Gear's filter + group-operations rows). Panel open state is session-only and closing a panel does not reset its controls.

- Filters panel: Show vaulted + Show all rewards checkboxes; the existing "Show all rewards hidden while Table mode is active" visibility rule moves into the panel unchanged.
- Sorting panel: sort-mode `ComboBox` + direction toggle `Button` showing the current direction ("Ascending" / "Descending", no arrow glyphs).
- The Sorting button label is a plain "Sorting" with no mode or direction suffix. *Originally designed to reflect active state (e.g. "Sorting: Score ↓"); rejected after visual testing — the composite label read as noisy and the panel is a one-click reveal.*

## Risks / Trade-offs

- [Reader base order changes (era key removed) could break tests that assume era-ordered reader output] → `Rows_FollowEraThenInputOrder` and any similar assertions are rewritten as ViewModel-sort tests in the same change; reader tests assert grouping correctness, not era order.
- [LINQ sort must be stable for the "names always A→Z" tie behavior] → `OrderBy`/`ThenBy` chains are explicitly keyed on era rank and name rather than relying on incoming order, so SQL base order is irrelevant to the result.
- [Sorting button label lengthening the control bar at large font sizes] → resolved by using a plain "Sorting" label with no mode or direction suffix (see D6).
- [Selecting the already-active mode in the ComboBox should not flip a user-toggled direction] → default-direction reset triggers only when the mode actually changes (guard on value inequality, matching existing property-setter style).

## Open Questions

None — direction semantics, defaults, persistence scope, and panel behavior were settled during exploration.
