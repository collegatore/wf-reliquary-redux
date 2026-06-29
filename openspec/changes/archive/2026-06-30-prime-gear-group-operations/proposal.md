## Why

An experienced player typically already owns most Prime gear, so seeding their owned state on the Prime Gear tab means opening the per-card overflow menu and choosing "Mark as owned" on dozens of cards one at a time — tedious and slow. There is also no way to act on many cards at once. Separately, owned and vaulted gear are currently rendered with the *same* visual treatment (both dimmed to ~0.55 opacity), so once a card is both owned and vaulted — or once a vaulted card is bulk-marked owned — the user sees no change, making the bulk workflow impossible to verify at a glance.

## What Changes

- Add an **Enable group operations** toggle to the Prime Gear control panel. The toggle is ephemeral (always starts off).
- When group operations is on, each gear card's **⋮ overflow button is replaced by a selection checkbox**, and a **second control-panel row** appears with a selection count, **Select all** / **Select none** helpers, and explicit **Mark owned** / **Unmark owned** buttons that act on the currently selected cards.
- **Select all** selects every currently *visible* card (across both Active and Vaulted sections).
- Bulk **Mark owned** asks **once** how to handle any tracked wishlist parts on the selection (Keep tracking / Release tracking / Cancel) and applies that choice to the whole batch, rather than prompting per card.
- The group selection is **cleared** whenever the visible set changes: on search text change, on toggling Show vaulted / Show owned, on switching the active wishlist, and on leaving group-operations mode.
- **Owned styling** is given its own visual channel, independent of the vaulted treatment: owned cards keep their dim but additionally gain a **gold outline** (the same gold as the "rare" relic-reward tier) and a **✓ marker** next to the gear name.
- **Vaulted styling** no longer dims the card; vaulted gear is communicated by its dedicated **Vaulted** section/header alone. This removes the owned-vs-vaulted visual collision.

## Capabilities

### New Capabilities
- `prime-gear-group-operations`: A group-operations mode on the Prime Gear tab that lets the user multi-select gear cards and bulk mark/unmark them owned, with select-all/none helpers, a single batched tracked-parts prompt, and selection that clears on view changes.

### Modified Capabilities
- `prime-gear-owned-status`: The per-card overflow (⋮) menu is replaced by a selection checkbox while group operations is enabled; owned cards are now styled with a gold outline and a ✓ marker in addition to being muted.
- `prime-gear-vault-status`: Vaulted gear cards are no longer rendered muted/reduced-opacity; vaulted status is conveyed by the Vaulted section/header instead.

## Impact

- **Presentation**: `PrimeGearViewModel` / `PrimeGearCardViewModel` gain group-mode state, per-card selection, and bulk commands; `PrimeGearView.axaml` gains the toggle, the second control row, the card checkbox/⋮ swap, and the new owned/vaulted styling; owned/vaulted style classes live in `MainWindow.axaml`.
- **Styling precedence**: the card border's inline `BorderBrush="Gray"` must move into a style so the `.owned` gold-outline override can win (Avalonia local values beat style setters).
- **No data-layer change**: bulk mark/unmark reuses the existing `IOwnedGearStore.SetOwnedAsync` per selected card; no schema or store-interface change is required.
- **Tests**: new `PrimeGearViewModel` tests for selection, bulk actions, the single batched prompt, and selection-clearing behavior.
