## 1. Card visibility flag

- [x] 1.1 Add an `IsShown` bool property (with `OnPropertyChanged`) to `PrimeGearCardViewModel`, defaulting to visible.
- [x] 1.2 Bind the gear card container's `IsVisible` to `IsShown` in `MainWindow.axaml` for both the Active and Vaulted `ItemsControl` item templates.

## 2. Unified filtering in PrimeGearViewModel

- [x] 2.1 Add a `SearchText` property that drives filtering, treating queries shorter than 3 characters as "no search".
- [x] 2.2 Implement `ApplyFilters()` that iterates `allCards` and sets each card's `IsShown` from `(showOwned || !IsOwned) && (searchInactive || GearName contains query, case-insensitive)`.
- [x] 2.3 Change the bound `ActiveCards`/`VaultedCards` to be populated with the full per-section card set at load time and no longer be cleared/repopulated for filtering.
- [x] 2.4 Call `ApplyFilters()` on initial load and from the `ShowOwned` setter; remove `RebuildVisibleCards`, `InsertInOrder`, and the remove-side logic of `OnCardOwnedChangedAsync`, folding owned visibility into `ApplyFilters()`.
- [x] 2.5 Debounce `SearchText`-driven `ApplyFilters()` (~150ms) so rapid typing triggers a single filter pass; ensure debounce is test-friendly (e.g. zero/synchronous in tests, as `OwnedHideFadeDuration` is).

## 3. Mark-owned fade preservation

- [x] 3.1 In the explicit mark-owned path (owned while Show owned is unchecked), sequence `IsFadingOut = true` → await the fade duration → `IsShown = false`, instead of removing the card from the collection.
- [x] 3.2 Verify the Show owned toggle and search paths set `IsShown` directly with no fade (instant), matching today's toggle behavior.

## 4. Hidden-match hint and empty state

- [x] 4.1 During `ApplyFilters()`, compute separate tallies of title matches hidden because owned (`!showOwned`) vs hidden because vaulted (`!showVaulted`); expose as bound properties.
- [x] 4.2 Add a `HasVisibleCards` / visible-count signal for the empty state, computed during `ApplyFilters()`.
- [x] 4.3 Restructure the Prime Gear control panel container (horizontal `StackPanel` → `DockPanel` or `Grid` with spacer) so the existing wishlist selector and toggles stay left and the search box anchors to the **right-hand side**.
- [x] 4.4 Add the search box (right side), the "N matches hidden by filters" hint adjacent to it, and the "no matching gear" empty-state element to the Prime Gear tab in `MainWindow.axaml`; show hint and empty state only while a search is active.

## 5. Tests

- [x] 5.1 Update `LoadAsync_ReflectsPersistedOwnedState_AndHidesOwnedByDefault` to assert the owned card is present with `IsShown == false` (not absent).
- [x] 5.2 Update `ShowOwned_RevealsAndHidesOwnedCards_InBothSections` to assert `IsShown` toggling rather than collection membership.
- [x] 5.3 Update `MarkingOwned_WithNoWishedParts_PersistsAndRemovesFromView` to assert the card becomes hidden (`IsShown == false`) rather than removed.
- [x] 5.4 Review remaining `PrimeGearViewModelTests` (`.Single()`/`.Any()` usages) for the full-collection model and adjust any that assumed filtered collections.
- [x] 5.5 Add tests for search: `contains` match, case-insensitivity, gear-name-only (no part-label match), the 3-character threshold (no effect below), and clearing restores the view.
- [x] 5.6 Add tests for the hidden-match hint tallies (owned vs vaulted) and the empty-state / visible-count signal.

## 6. Verification

- [x] 6.1 Build the solution and run the test suite; confirm all Prime Gear tests pass.
- [x] 6.2 Manually verify (per user preference, via build/test rather than launching unprompted): live filtering past 3 chars, no flicker on owned toggle or typing, fade on mark-owned, hint counts, and empty state.
