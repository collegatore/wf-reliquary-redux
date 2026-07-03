# Wishlist Relics View — Delta

## MODIFIED Requirements

### Requirement: Wishlist Relics tab exists in main navigation
The application SHALL display a tab labeled **"Relics and Missions"** in the main tab bar alongside the existing "Drop Tables" and "Prime Gear" tabs, replacing the former "Wishlist Relics" label. The tab SHALL contain two stacked panels: the relic cards panel on top and the farming missions panel below, separated by a **missions separator panel**. A shared **Relics control panel** appears above the relic cards panel. The Relics control panel SHALL contain a **Relics** group with a plain "Relics:" text label followed by a **Filters** button and a **Sorting** button (transparent, borderless, matching the Prime Gear control-bar button style), each toggling its own collapsible panel row below the control panel (specified by the `relics-filter-panel` and `relics-sorting-panel` capabilities). The Show vaulted and Show all rewards checkboxes SHALL NOT appear directly on the control panel; they live inside the Filters panel. The **missions separator panel** SHALL share the tab's control-bar background tint and SHALL contain a **Missions** group with a plain "Missions:" text label followed by the "Show exact drop chances" checkbox. The missions separator panel SHALL be a compact divider bar (matching the height of the "Vaulted" separator bar on the Prime Gear tab) with its bottom corners squared where it meets the splitter, and SHALL be pinned to the bottom of the relic-cards section so it sits directly above the splitter. A draggable GridSplitter SHALL be placed immediately below the missions separator panel; dragging it SHALL resize the relic-cards scroll area and the farming missions scroll area (not the separator panel, whose height stays fixed). Each panel SHALL scroll independently within its own scrollable area.

#### Scenario: Tab is visible in main window
- **WHEN** the application is open
- **THEN** a tab labeled "Relics and Missions" is visible in the tab bar

#### Scenario: Tab displays both panels when selected
- **WHEN** the user selects the "Relics and Missions" tab
- **THEN** the relic cards panel is visible in the upper half and the farming missions panel is visible in the lower half

#### Scenario: Panels scroll independently
- **WHEN** the user scrolls in the relic cards panel
- **THEN** the farming missions panel position is unaffected, and vice versa

#### Scenario: Relics control panel shows Relics label and panel buttons
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the top control panel shows a "Relics:" text label followed by the Filters and Sorting buttons, shows no inline filter checkboxes, and does NOT show a "Missions:" label or "Show exact drop chances" checkbox

#### Scenario: Missions separator panel shows Missions label and checkbox
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the missions separator panel shows a "Missions:" text label followed by the "Show exact drop chances" checkbox

#### Scenario: Missions separator panel is a compact divider bar
- **WHEN** the missions separator panel is displayed
- **THEN** it uses the tab's control-bar background tint, is as tall as the "Vaulted" separator bar on the Prime Gear tab, and has squared bottom corners where it meets the splitter

#### Scenario: GridSplitter below separator resizes the card areas
- **WHEN** the user drags the thin strip immediately below the missions separator panel
- **THEN** the relic cards scroll area and the farming missions scroll area resize accordingly, while the separator panel's height stays fixed

### Requirement: Show all rewards toggle
The Wishlist Relics tab SHALL provide a **Show all rewards** checkbox inside the collapsible Filters panel (see the `relics-filter-panel` capability). The checkbox SHALL be unchecked by default. Toggling it SHALL immediately update all visible relic cards. The checkbox SHALL apply to the Verbatim and Compact view modes; while **Table** mode is active, the checkbox (and its label) SHALL be hidden. Its checked state SHALL be retained while hidden, so switching back to a card mode restores the previous behavior.

#### Scenario: Show all rewards is unchecked by default
- **WHEN** the application opens and the user navigates to the Wishlist Relics tab
- **THEN** the Show all rewards checkbox is unchecked and cards show only wishlisted rewards

#### Scenario: Checking Show all rewards updates cards immediately
- **WHEN** the user checks Show all rewards in the Filters panel
- **THEN** all relic cards expand to show their full reward lists without requiring a reload

#### Scenario: Unchecking Show all rewards collapses cards immediately
- **WHEN** the user unchecks Show all rewards in the Filters panel
- **THEN** all relic cards return to showing only wishlisted rewards without requiring a reload

#### Scenario: Checkbox hidden in Table mode
- **WHEN** the user switches the view mode to Table
- **THEN** the Show all rewards checkbox is not visible in the Filters panel

#### Scenario: Checkbox state survives Table mode
- **WHEN** Show all rewards is checked, the user switches to Table mode and then back to Compact mode
- **THEN** the checkbox reappears still checked and non-wishlisted rewards render dimmed

### Requirement: Vaulted relics can be shown or hidden via a checkbox
The collapsible Filters panel (see the `relics-filter-panel` capability) SHALL contain a checkbox labeled **"Show vaulted"**. By default the checkbox is unchecked (vaulted relics hidden). Checking it reveals vaulted relics.

A relic is considered vaulted if it does not appear as a reward in any active mission.

#### Scenario: Vaulted relics hidden by default
- **WHEN** the Wishlist Relics tab is first displayed
- **THEN** relics that are vaulted are not shown

#### Scenario: Checking show-vaulted reveals vaulted relics
- **WHEN** the user checks "Show vaulted" in the Filters panel
- **THEN** vaulted relics that contain wishlisted parts become visible

#### Scenario: Unchecking show-vaulted hides vaulted relics again
- **WHEN** the user unchecks "Show vaulted" after they were shown
- **THEN** vaulted relics are hidden again

## REMOVED Requirements

### Requirement: Cards are ordered by era then alphabetically by name
**Reason**: Relic ordering is now user-selectable via the sorting panel; a fixed era-then-name order is no longer a standalone requirement.
**Migration**: Superseded by the `relics-sorting-panel` capability. Era mode in ascending direction (the default for fresh settings) reproduces the previous fixed order exactly. The flat wrap layout (no era section headers) is restated in the ADDED requirement below.

## ADDED Requirements

### Requirement: Cards follow the active sort
Relic cards SHALL be presented in the order defined by the active sort mode and direction (see the `relics-sorting-panel` capability). With default settings (Era mode, ascending) the order is era Lith → Meso → Neo → Axi, then ascending display name — identical to the previous fixed order. There are no era section headers; cards flow in a single flat wrap layout regardless of sort mode.

#### Scenario: Default sort reproduces the previous order
- **WHEN** the application runs with default sort settings and both Lith and Meso relics are shown
- **THEN** all Lith relic cards appear before any Meso relic cards, alphabetically within each era

#### Scenario: Cards re-order when the sort changes
- **WHEN** the user changes the sort mode or direction in the sorting panel
- **THEN** the visible cards re-order immediately without a reload
