## Context

The Prime Gear tab (`PrimeGearViewModel`, `MainWindow.axaml`) loads the full gear set once into `allCards` and projects it into two bound `ObservableCollection`s — `ActiveCards` and `VaultedCards` — via `RebuildVisibleCards()`. Filtering today is split across two code paths:

- **Bulk** (`RebuildVisibleCards`): `Clear()` + repopulate, used on initial load and the Show owned toggle.
- **Granular** (`OnCardOwnedChangedAsync` + `InsertInOrder`): single-card add/remove when a card is marked owned, with a fade before removal.

Adding a live title search means re-filtering on every keystroke. Doing that through the bulk `Clear()`+rebuild path flickers: an `ObservableCollection.Clear()` raises `Reset`, which tears down every container in the non-virtualizing `WrapPanel` (each card carries a nested parts `ItemsControl`), passing through a blank intermediate frame before repopulating. Debouncing reduces the *frequency* of that flash but not the mechanism — every accepted keystroke still blinks, and cards that survive the filter are destroyed and recreated needlessly.

## Goals / Non-Goals

**Goals:**
- Live, flicker-free title filtering on the Prime Gear tab.
- Unify owned filtering and the new search filtering into one mechanism and one code path.
- Preserve existing observable behavior of Show owned / Show vaulted and the mark-owned fade.

**Non-Goals:**
- Searching part labels, fuzzy/ranked matching, or search history.
- Virtualizing the gear grid.
- Changing the database, ports, or import pipeline.
- Animated reflow when cards enter/leave the filtered set (the existing opacity transition is enough; layout reflow is acceptable as-is).

## Decisions

### Decision: Filter by visibility toggling, not collection mutation

Each `PrimeGearCardViewModel` gains an `IsShown` flag (raising `PropertyChanged`). The bound collections hold the **full** card set for their section, built once at load and never mutated for filtering. The card container's `IsVisible` binds to `IsShown`; a collapsed (`IsVisible=false`) `ContentPresenter` measures to zero, so the `WrapPanel` closes the gap with no container teardown and no blank frame. Stable cards never re-render, so there is no flicker by construction.

**Alternatives considered:**
- *Diff/reconcile the `ObservableCollection`* (remove only stale, insert only new). Also flicker-free and keeps the realized set proportional to what is shown, but more bookkeeping and retains collection mutation. Kept in reserve if the realized-container cost ever bites.
- *`DataGridCollectionView.Filter`*: built-in filter API, but `Refresh()` raises `Reset` (re-flickers) and pulls in the `Avalonia.Controls.DataGrid` package the project does not currently reference. Rejected.

### Decision: One `ApplyFilters()` replaces the three filtering paths

`RebuildVisibleCards`, `InsertInOrder`, and the remove-side of `OnCardOwnedChangedAsync` collapse into a single `ApplyFilters()` that iterates `allCards` and sets each card's `IsShown` from the composite predicate:

```
IsShown = (showOwned || !card.IsOwned)
          && (searchInactive || card.GearName contains query, case-insensitive)
```

`ApplyFilters()` runs on: initial load, `ShowOwned` change, and `SearchText` change (post-debounce). The Show vaulted toggle stays a section-level `IsVisible` (header + vaulted `ItemsControl`) as it is today — it hides the whole section rather than per-card. Because cards never leave the collection, they remain in their sorted position, so no insertion-index math is needed.

### Decision: 3-character threshold + ~150ms debounce

Below 3 characters the predicate treats the search as inactive (all cards pass the search clause). At/above 3, a ~150ms debounce coalesces rapid typing into one `ApplyFilters()` pass. The threshold avoids near-everything result sets on 1–2 characters; the debounce avoids redundant passes. Both are efficiency measures layered on top of the already-flicker-free visibility model, not the primary defense against flicker.

### Decision: Preserve the mark-owned fade via sequencing

`IsVisible=false` collapses instantly and would kill the fade. The explicit mark-owned path keeps the two-step sequence: set `IsFadingOut=true` (opacity transition), `await` the fade duration, then set `IsShown=false` to collapse. The bulk paths (Show owned toggle, search) set `IsShown` directly with no fade, matching today's instant behavior for those toggles.

### Decision: Right-align the search box via a layout container change

The search box sits on the right of the control panel, opposite the wishlist selector and toggles. The current control panel is a horizontal `StackPanel` (`MainWindow.axaml`), which cannot push a child to the right. The panel is therefore restructured — e.g. a `DockPanel` with the existing controls grouped on the left and the search box `DockPanel.Dock="Right"`, or a `Grid` with a `*` spacer column — so the toggles keep their current left-to-right order and spacing while the search box anchors right. The hidden-match hint sits adjacent to the search box (below or beside it).

### Decision: Explicit count/hint signals derived during `ApplyFilters()`

Because the bound collections are always full, `Count` no longer reflects what is visible. `ApplyFilters()` computes, while iterating:
- a visible-count (or `HasVisibleCards`) for the empty-state when a search hides everything; and
- separate tallies of title matches hidden specifically because they are owned (and `!showOwned`) versus vaulted (and `!showVaulted`), exposed as bound properties driving the "N matches hidden by filters" hint. The hint and empty-state are only shown while a search is active.

## Risks / Trade-offs

- **All cards realized permanently** (owned/hidden ones collapsed rather than absent) → higher steady-state container count and heavier initial layout than today, since currently owned-hidden cards are not realized. At the few-hundred-card scale this is acceptable; mitigation if it ever bites is the diff/reconcile alternative.
- **Test contract changes** (membership → flag): ~4 `PrimeGearViewModelTests` assert a hidden card is absent from `ActiveCards`/`VaultedCards`; under visibility toggling the card is present with `IsShown=false`. These must be rewritten to assert the flag. → Update them as part of this change and add search/hint tests; this is the intended, reviewed contract shift.
- **Spec wording "removed from view"** for marking owned still holds (a collapsed card is not visible), so `prime-gear-owned-status` needs no spec change — only the implementation of "removed" changes. → Verify the fade scenario still reads correctly against the sequenced collapse.
- **Reflow on filter changes** (surviving cards shift to close gaps) is repositioning, not flicker, and is accepted. → No layout transition added in this change.
