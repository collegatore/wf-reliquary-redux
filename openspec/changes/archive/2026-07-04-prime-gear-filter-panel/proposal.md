## Why

The Prime Gear control panel is getting crowded: Show vaulted, Show owned, and the new Wishlisted-only filter all compete for horizontal space with the wishlist selector and search box. Moving visibility toggles into a collapsible filter panel keeps the primary panel scannable while making it easy to add more filters in the future.

## What Changes

- A **Filters** button is added to the existing Prime Gear control panel (the wishlist selector / search row). Clicking it toggles a secondary filter panel open or closed.
- The **Show vaulted** and **Show owned** checkboxes are moved out of the primary panel and into the new filter panel.
- A new **Wishlisted only** filter is added inside the filter panel. When enabled, only gear cards that have at least one part checkbox checked are shown.
- The filter panel is hidden by default and its visibility is ephemeral (not persisted across sessions).

## Capabilities

### New Capabilities

- `prime-gear-filter-panel`: The collapsible filter panel on the Prime Gear tab — its toggle button, open/closed state, and the three filters it hosts (Show vaulted, Show owned, Wishlisted only).

### Modified Capabilities

- `prime-gear-vault-status`: The Show vaulted toggle moves from the primary control panel into the filter panel. The toggle's behavior is unchanged; only its location changes.
- `prime-gear-owned-status`: The Show owned toggle moves from the primary control panel into the filter panel. The toggle's behavior is unchanged; only its location changes.

## Impact

- `PrimeGearView.axaml`: primary panel loses two CheckBox rows; gains a Filters toggle button; new filter panel Border added beneath the primary panel.
- `PrimeGearViewModel.cs`: new `IsFilterPanelOpen` bool property; new `ShowWishlistedOnly` bool property; `ApplyFilters` updated to respect the wishlisted filter.
- `PrimeGearViewModelTests.cs`: tests for `ShowWishlistedOnly` filter behavior.
