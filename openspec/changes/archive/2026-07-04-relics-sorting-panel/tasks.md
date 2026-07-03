# Tasks — Relics Sorting Panel

## 1. Settings plumbing

- [x] 1.1 Add `RelicSortMode` enum (`Era`, `Alphabetical`, `WishlistScore`) in `WF.Reliquary.Application.Settings` beside `RelicViewMode`
- [x] 1.2 Add defaulted trailing parameters `RelicSortMode SortMode = RelicSortMode.Era` and `bool SortDescending = false` to `UserSettings`
- [x] 1.3 Add matching nullable fields to `LocalUserSettingsStore.SettingsFile` (enum with `JsonStringEnumConverter`, following the `ViewMode` precedent) and map them in load/save
- [x] 1.4 Test: settings round-trip persists sort mode + direction; a settings file without the new fields loads as Era/ascending

## 2. SQL and reader base order

- [x] 2.1 Remove the era `CASE` key from `GetWishlistRelics.sql` ORDER BY, keeping `display_name`, tier `CASE`, and `source_order`
- [x] 2.2 Update the "order comes from the reader" comment in `WishlistRelicsViewModel.RebuildViews` to describe the new split (reader guarantees grouping contiguity + reward order; ViewModel owns relic order)
- [x] 2.3 Rewrite `Rows_FollowEraThenInputOrder` (and any other tests assuming era-ordered reader output) as ViewModel-sort assertions

## 3. ViewModel sorting

- [x] 3.1 Add `SortMode` and `SortDescending` properties to `WishlistRelicsViewModel` that trigger `RebuildViews` on change; selecting a different mode applies that mode's default direction (asc for Era/Alphabetical, desc for WishlistScore); re-selecting the same mode leaves direction untouched
- [x] 3.2 Implement the sort in `RebuildViews` before the Cards/TableRows fan-out: Era (era rank, name), Alphabetical (name), WishlistScore (Σ wishlisted rewards × 3/4/5 with era-rank + name tie-break); direction reverses the primary key chain only, names always ascending except in Alphabetical mode
- [x] 3.3 Add `IsFilterPanelOpen` / `IsSortPanelOpen` flags with toggle commands (Prime Gear pattern), plus a computed Sorting-button label (e.g. "Sorting: Score ↓")
- [x] 3.4 Wire startup load and on-change save of sort mode + direction through the shell, following the `ViewMode` settings wiring; startup restores persisted values without applying default-direction
- [x] 3.5 Tests: score computation (multi-reward, non-wishlisted excluded), each mode × direction ordering incl. half-reversed Era and score tie-breaks, default-direction-on-mode-change, same-mode reselect no-op, sort consistency between Cards and TableRows

## 4. XAML

- [x] 4.1 Replace the inline checkboxes on the Relics control bar in `RelicsPanelView.axaml` with transparent `Filters` and `Sorting` buttons (Prime Gear button style); keep the "Relics:" label and busy indicator
- [x] 4.2 Add the Filters panel row (Show vaulted + Show all rewards checkboxes with divider, Show-all-rewards hidden in Table mode) bound to `IsFilterPanelOpen`
- [x] 4.3 Add the Sorting panel row (sort-mode ComboBox + direction toggle button) bound to `IsSortPanelOpen`; both panels stack when open
- [x] 4.4 Build and visually verify all three view modes against the specs (panels toggle, sort applies, labels update)

## 5. Spec sync

- [ ] 5.1 Verify all delta scenarios pass, then sync delta specs to main specs (`/opsx:sync`) or archive the change
