## Why

The Prime Gear tab can show a large number of gear cards, and the only way to narrow them today is the Show vaulted / Show owned toggles. Finding a specific piece of gear means scanning the whole grid. A title search lets the user jump straight to the gear they care about.

## What Changes

- Add a **search box** to the Prime Gear control panel that filters gear cards by a case-insensitive `contains` match on the gear name (title only — parts are not searched).
- Search updates the view **live as the user types**, but only once the query is **3 or more characters**; below that threshold the view behaves as if no search is active. Input is **debounced (~150ms)** so a fast typist triggers one filter pass, not one per keystroke.
- Filtering is implemented by **visibility toggling**: each card carries an `IsShown` flag bound to its container's `IsVisible`, rather than adding/removing cards from the bound collections. This avoids the grid flicker (blank intermediate frame + container teardown) that a `Clear()`+rebuild causes on every keystroke.
- **Retrofit the existing Show owned filter to the same visibility mechanism.** The owned filter currently mutates the bound collections (`RebuildVisibleCards` Clear+repopulate, `InsertInOrder`, and the remove-side of the owned-changed handler). These collapse into a single `ApplyFilters()` that sets per-card visibility. Observable behavior is unchanged.
- Search **respects** the Show owned / Show vaulted toggles: a title match that is currently hidden by a toggle stays hidden, but a **hint** ("N matches hidden by filters") is shown so the user knows to flip a toggle. Owned-hidden and vaulted-hidden matches are tallied separately so the hint can name which toggle to flip.
- The mark-owned **fade-out is preserved** by sequencing `IsFadingOut` (opacity transition) and then collapsing the card via `IsShown = false`, instead of removing it from the collection.
- Add a **visible-count / empty-state signal** so the tab can show a "no matching gear" state when a search excludes every card (the bound collections are always full now, so `Count` no longer indicates what is visible).

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities
- `prime-gear-parts-view`: Adds a title search requirement (live `contains` filtering with a 3-character threshold, debounce, toggle-respecting scope with a hidden-match hint, and an empty-state when no card matches).

## Impact

- **Presentation**: `PrimeGearViewModel` — new `SearchText` property, `ApplyFilters()` replacing `RebuildVisibleCards`/`InsertInOrder`/the owned-changed remove path, per-card `IsShown` flag on `PrimeGearCardViewModel`, debounce, and visible-count / hidden-match-tally properties. `MainWindow.axaml` — search box + hint in the Prime Gear control panel, container `IsVisible` binding, empty-state element.
- **Tests**: ~4 existing `PrimeGearViewModelTests` move from collection-membership assertions to `IsShown` flag assertions; new tests for search matching, the 3-character threshold, and the hidden-match hint tally.
- No database, port, or infrastructure changes. No new package dependencies (visibility toggling uses the built-in `IsVisible` property).
