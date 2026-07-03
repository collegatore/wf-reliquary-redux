## Context

The Prime Gear tab currently has a single control panel (the wishlist selector / search row) that contains the wishlist dropdown, a search box, Show vaulted, Show owned, and Enable group operations. Adding a Wishlisted-only filter would push the row further and increase clutter. The solution is a collapsible secondary panel for filter-specific controls.

The ViewModel (`PrimeGearViewModel`) already owns `ShowVaulted`, `ShowOwned`, and `ApplyFilters`. Adding filter-panel state and the new filter is a localised extension to that class. No new ports, stores, or infrastructure changes are needed.

## Goals / Non-Goals

**Goals:**
- Add `IsFilterPanelOpen` bool to `PrimeGearViewModel` (defaults false, not persisted).
- Add `ShowWishlistedOnly` bool to `PrimeGearViewModel` (defaults false, not persisted).
- Extend `ApplyFilters` to hide a card when `ShowWishlistedOnly` is true and the card has no checked parts.
- Move Show vaulted and Show owned checkboxes into the new filter panel in the XAML.
- Add a Filters toggle button to the primary control panel.
- Add the filter panel Border (conditionally visible) with the three checkboxes.

**Non-Goals:**
- Persisting filter-panel open/closed state or the Wishlisted-only toggle across restarts.
- Changing the behavior of Show vaulted or Show owned (same logic, new location).
- Any new infrastructure, DB queries, or port changes.

## Decisions

### Filter-panel visibility via a ViewModel bool, not a XAML trigger

`IsFilterPanelOpen` is a plain bool on `PrimeGearViewModel` bound to `IsVisible` on the filter panel Border. The Filters button binds to a `ToggleFilterPanelCommand`. This keeps the pattern consistent with `IsGroupMode` / group-ops panel (which is already done this way) and stays testable without UI.

*Alternative considered*: XAML-only toggle (e.g. a ToggleButton binding `IsChecked` directly to `IsVisible` on the panel). Rejected because it would be invisible to unit tests and inconsistent with the existing pattern.

### Wishlisted-only check in `ApplyFilters`

The new filter slot is a simple predicate added to the `ApplyFilters` loop: `if (showWishlistedOnly && !card.Parts.Any(p => p.IsChecked)) skip`. This runs after the owned check so the owned filter's hidden-match hint logic remains the primary gate.

Filter application order:
1. Owned hidden (`card.IsOwned && !showOwned`) → hide, count as hiddenOwned if search active.
2. Wishlisted only (`showWishlistedOnly && no checked parts`) → hide.
3. Search (`searchActive && !name contains query`) → hide, count as hiddenVaulted if vaulted.

Owned-hidden cards are hidden regardless of wishlist state (they may have parts checked but the user has asked not to see them).

### No new capability for group-ops button relocation

The group-operations checkbox ("Enable group operations") stays in the primary control panel. Only the two visibility-filter checkboxes move. This preserves discoverability of the group-ops toggle without requiring the filter panel to be open for common actions.

## Risks / Trade-offs

- **Wishlist-state staleness**: `ShowWishlistedOnly` reads part `IsChecked` at filter-apply time. If a part checkbox changes while the filter is active, `ApplyFilters` is not automatically re-triggered — this is the same pattern as the owned filter. The filter re-applies on the next state change that calls `ApplyFilters` (e.g. search, owned toggle, reload). This is acceptable given the filter is opt-in and explicit.
  → Mitigation: document the known timing gap; no action required for v1.

- **Hidden-match hint doesn't account for wishlisted filter**: The existing `HiddenMatchHint` shows counts for owned-hidden and vaulted-hidden search matches. Wishlisted-hidden cards are not broken out in the hint. This is intentional to avoid complicating the hint for a less-common filter.
  → Acceptable trade-off; can be revisited if users find it confusing.
