# Proposal

## Why

Filters and Sorting on the Relics and Prime Gear bars each open a full extra row under the bar. This takes vertical space and doesn't match the compact icon-dropdown controls people expect from modern apps. The filter wording is also ambiguous: "Show vaulted" reads as adding content rather than filtering. And the default state already hides things, so there's no clear "nothing is filtered" baseline. Show all rewards sits among the filters although it is a display option, and the relic view mode can only be changed in a separate Appearance window.

This change builds on `stock-dropdown-controls`, which provides the icon set and the shared `app-menu` look. Every control here is a stock Avalonia control: `ComboBox`, `DropDownButton` with `MenuFlyout`, and plain buttons.

## What Changes

- **Filters become a funnel-icon menu button** on both tabs, replacing the filter row. It is a `DropDownButton` whose menu lists checkbox items under a non-interactive "Hide" caption. The menu stays open while items are toggled. A checked item hides that category:
  - Relics: **Vaulted**.
  - Prime Gear: **Vaulted**, **Owned**, **Not wishlisted**.
  - Default: nothing hidden. **BREAKING (defaults)**: vaulted relics, vaulted gear and owned gear are now visible on first run.
  - The view-model flags invert where the wording inverts (`ShowVaulted` becomes `HideVaulted`, `ShowOwned` becomes `HideOwned`). "Wishlisted only" keeps its polarity as "Hide Not wishlisted".
  - Filter state now **persists** in user settings. It was session-only before, so nothing needs migrating.
- **Sort becomes a split control** on the Relics bar, replacing the sorting row:
  - The existing sort-mode `ComboBox` (Era / Alphabetical / Wishlist score) moves onto the bar and gains a fixed double-arrow sort icon before the mode name.
  - A separate direction button shows a single arrow with bars that flips with the direction.
  - **BREAKING (defaults)**: the default sort becomes **Era descending** (Axi → Lith). Picking Era from the menu also defaults to descending. Settings without a sort still load as Era descending. Installs that already saved Era ascending keep it.
- **Display menu button (eye icon)** is added to the Relics bar. It is a `DropDownButton` whose menu holds the relic view mode as radio items (Verbatim / Compact / Table) and **Show all rewards** as a checkbox item, which is greyed out in Table mode. The view mode selector is **removed from the Appearance window**.
- **Non-default indicator:** the Filters, Sort and Display controls tint their icon with the accent color whenever their state differs from the default. After a short hover delay, a tooltip describes the current state, e.g. "Hiding: vaulted, owned" or "Sorted by Wishlist score, descending".
- **Group operations:** *Unmark owned* is always shown, joined with *Mark owned*, and stays disabled while no selected card is owned. The rule that hides it unless owned gear is shown is removed, along with the stand-alone Mark owned pill. The group-operations row itself stays a row.
- The filter and sorting panel rows, and their open/closed toggle state, are removed.

## Capabilities

### New Capabilities
- `relics-display-menu`: the Display dropdown on the Relics bar. Covers the view mode choice, the Show all rewards toggle and its Table-mode state, defaults, and the non-default indicator.

### Modified Capabilities
- `relics-filter-panel`: the filter row becomes a funnel dropdown with a "Hide Vaulted" entry, default nothing hidden, persisted. Show all rewards moves out to the Display menu.
- `prime-gear-filter-panel`: the filter row becomes a funnel dropdown with Hide Vaulted / Owned / Not wishlisted, default nothing hidden, persisted.
- `relics-sorting-panel`: the sorting row becomes a split mode/direction control with distinct icons, the default becomes Era descending, and Era's mode-change default becomes descending. The "button label reflects the active sort" requirement is replaced by the non-default indicator and the tooltip.
- `control-bar-visual-style`: icon dropdown buttons on the bars, the accent indicator and hover tooltip, removal of the filter and sorting rows from the bar list and chip list, and the Mark/Unmark owned pair always joined.
- `prime-gear-group-operations`: Unmark owned is always shown and disabled when no owned card is selected.
- `relic-view-modes`: the view mode selector moves from the Appearance window to the Display menu, and references to "Show vaulted" become the Hide Vaulted filter.
- `appearance-window`: the relic view mode selector is removed from the window's control list.
- `color-palette-setting`: scenarios that use the sort-mode ComboBox now use the sort-mode dropdown, and the accent indicator follows the palette.
- `prime-gear-owned-status`, `prime-gear-vault-status`, `prime-gear-parts-view`, `wishlist-relics-view`, `split-panel-persistence`: wording that refers to the Show owned / Show vaulted / Wishlisted only checkboxes is updated to the Hide filters, where their requirements depend on it.

## Impact

- **Presentation:** `RelicsPanelView.axaml`, `PrimeGearView.axaml`, `AppearanceWindow.axaml`, `WishlistRelicsViewModel.cs`, `PrimeGearViewModel.cs`, `AppearanceViewModel.cs`, and `Styles/ControlBars.axaml`. The filter and sort panel toggle commands and `Is*PanelOpen` state are removed.
- **Application / Infrastructure:** `UserSettings` gains the filter flags. `LocalUserSettingsStore` reads and writes them, with defaults for older files. The default `SortDirection` changes to descending.
- **Tests:** settings round-trip and defaults, sort default direction on mode change, filter inversion, Unmark owned enablement.
- **UI snapshot tool:** stand-ins and scenes are updated for the new bar controls.
- **Depends on** `stock-dropdown-controls` (icons and the `app-menu` look). The menu check and radio markers in palette colors are added here, since this change is the first to use toggle menu items.
