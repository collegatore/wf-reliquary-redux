# Proposal

## Why

Filters and Sorting on the Relics and Prime Gear bars each open a full extra row under the bar. This takes vertical space and doesn't match the compact icon-dropdown controls people expect from modern apps. The filter wording is also ambiguous: "Show vaulted" reads as adding content rather than filtering. And the default state already hides things, so there's no clear "nothing is filtered" baseline. Show all rewards sits among the filters although it is a display option, and the relic view mode can only be changed in a separate Appearance window.

This change builds on `stock-dropdown-controls` (archived), which provides the icon set (`Icon.Funnel`, `Icon.Sort`, `Icon.SortAscending`, `Icon.SortDescending`, `Icon.Eye`) and the shared `app-menu` look. Every control here is a stock Avalonia control: `ComboBox`, a `Button` with a `MenuFlyout`, and plain buttons.

## What Changes

- **Filters become an icon-only funnel menu button** on both tabs, replacing the filter row. Its menu lists checkbox items under a non-interactive "Hide" caption, and it stays open while items are toggled. A checked item hides that category:
  - Relics: **Vaulted**.
  - Prime Gear: **Vaulted**, **Owned**, **Not wishlisted**.
  - Default: nothing hidden. **BREAKING (defaults)**: vaulted relics, vaulted gear and owned gear are now visible on first run.
  - The view-model flags invert where the wording inverts (`ShowVaulted` becomes `HideVaulted`, `ShowOwned` becomes `HideOwned`). "Wishlisted only" keeps its polarity as "Hide Not wishlisted".
  - Filter state now **persists** in user settings, separately per tab. It was session-only before, so nothing needs migrating.
- **Sort becomes a split control** on the Relics bar, replacing the sorting row:
  - An icon-only sort button with a fixed double-arrow icon opens a menu of Era / Alphabetical / Wishlist score (radio entries). The mode's name is not shown on the bar; the tooltip states it.
  - A separate, icon-only direction button shows a single arrow with bars, which flips with the direction.
  - **BREAKING (defaults)**: the default sort becomes **Era descending** (Axi → Lith). Picking Era from the menu also defaults to descending. Settings without a sort direction load as Era descending. Installs that already saved Era ascending keep it.
- **An icon-only Display menu button (eye icon)** is added to the Relics bar. Its menu holds the relic view mode as radio items (Verbatim / Compact / Table) and **Show all rewards** as a checkbox item, which is greyed out in Table mode. The view mode selector is **removed from the Appearance window**. Show all rewards now persists like the view mode.
- **Non-default indicator:** the Filters, Sort and Display controls tint their icon with the palette's selection accent whenever their state differs from the default. After a short hover delay, a tooltip names the control and describes its current state, e.g. "Filters: hiding vaulted, owned" or "Sorted by Wishlist score, descending".
- **Menu markers follow the palette:** the check and radio markers in these menus use the palette's selection accent, like a checked chip's tick.
- **Group operations:** *Unmark owned* is always shown, joined with *Mark owned*, and stays disabled while no selected card is owned. The rule that hides it unless owned gear is shown is removed, along with the stand-alone Mark owned pill. The group-operations row itself stays a row.
- The filter and sorting panel rows, their toggle buttons and their open/closed state are removed.

Non-goals: the missions bar's "Show exact drop chances" chip stays a chip (a Display menu there could follow later), and the Prime Gear search box is unchanged.

## Capabilities

### New Capabilities
- `relics-display-menu`: the Display menu button on the Relics bar. Covers the view mode choice, the Show all rewards option and its Table-mode state, persistence, and the non-default indicator.

### Modified Capabilities
- `relics-filter-panel`: the filter row becomes a funnel menu with a "Hide Vaulted" entry, default nothing hidden, persisted. Show all rewards moves out to the Display menu.
- `prime-gear-filter-panel`: the filter row becomes a funnel menu with Hide Vaulted / Owned / Not wishlisted, default nothing hidden, persisted.
- `relics-sorting-panel`: the sorting row becomes an icon-only sort menu button plus a direction button on the bar, with distinct icons. The default becomes Era descending, and Era's mode-change default becomes descending. The "button label reflects the active sort" requirement is replaced by the non-default indicator and the tooltip.
- `control-bar-visual-style`:
  - Icon-only menu buttons on the bars.
  - The non-default accent and the hover tooltip.
  - The filter and sorting rows removed from the pill, separator and chip rules.
  - The Mark/Unmark owned pair always joined.
  - "Control-bar dropdowns share the wishlist selector's field look" is removed: no dropdown is left on a bar besides the Wishlist selector.
- `color-palette-setting`:
  - Scenarios that used filter chips now use the remaining Show exact drop chances chip.
  - The dropdown rules no longer use the sort-mode dropdown as their example (it is now a menu button).
  - Menu check and radio markers use the selection accent.
- `prime-gear-group-operations`: Unmark owned is always shown. Wording moves from the Show checkboxes to the Hide filters.
- `prime-gear-owned-status`: owned gear is visible by default and hidden by the Hide Owned filter.
- `prime-gear-vault-status`, `prime-gear-parts-view`: the Vaulted section is visible by default and hidden by the Hide Vaulted filter. The search hint refers to the Hide filters.
- `wishlist-relics-view`:
  - The Relics bar shows the new controls.
  - Vaulted relics are visible by default.
  - Show all rewards moves to the Display menu.
  - Cards follow the new default sort.
- `relic-view-modes`: the view mode selector moves to the Display menu, the Table mode default order follows Era descending, and references to "Show vaulted" become the Hide Vaulted filter.
- `appearance-window`: the relic view mode selector is removed from the window's control list.

## Impact

- **Presentation:**
  - Views: `RelicsPanelView.axaml`, `PrimeGearView.axaml`, `AppearanceWindow.axaml`.
  - View models: `WishlistRelicsViewModel`, `PrimeGearViewModel`, `AppearanceViewModel`, `MainWindowShellViewModel`.
  - Styles: `Styles/Dropdowns.axaml`, `Styles/ControlBars.axaml`.
  - The filter and sort panel toggle commands and `Is*PanelOpen` state are removed.
- **Application / Infrastructure:**
  - `UserSettings` gains the Relics and Prime Gear filter flags and `ShowAllRewards`.
  - `LocalUserSettingsStore` reads and writes them, with defaults for older files.
  - A missing sort direction now defaults to descending.
- **Tests:** settings round-trip and defaults, sort default direction on mode change, filter inversion and persistence, non-default detection, Unmark owned enablement.
- **UI snapshot tool:** stand-ins and scenes are updated for the new bar controls and their open menus.
- **Docs:** the user guide's filter and sort section, `CHANGELOG.md`, and the technical reference. The Purpose sections of the three renamed-in-spirit panel specs are updated when the change is archived.
