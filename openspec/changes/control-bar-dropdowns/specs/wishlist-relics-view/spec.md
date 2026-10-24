# Spec Delta

## ADDED Requirements

### Requirement: Cards follow the active sort order
Relic cards SHALL be presented in the order defined by the active sort mode and direction (see the `relics-sorting-panel` capability). With default settings (Era mode, descending) the order is era Axi → Neo → Meso → Lith, then ascending display name. There are no era section headers; cards flow in a single flat wrap layout regardless of sort mode.

#### Scenario: Default sort shows the newest era first
- **WHEN** the application runs with default sort settings and both Lith and Axi relics are shown
- **THEN** all Axi relic cards appear before any Lith relic cards, alphabetically within each era

#### Scenario: Cards re-order when the sort changes
- **WHEN** the user changes the sort mode or direction on the Relics bar
- **THEN** the visible cards re-order immediately without a reload

---

### Requirement: Vaulted relics are shown unless hidden from the Filters menu
Vaulted relics SHALL be shown by default. Checking **Vaulted** in the Relics Filters menu (see the `relics-filter-panel` capability) SHALL hide them; unchecking it SHALL show them again.

A relic is considered vaulted if it does not appear as a reward in any active mission.

#### Scenario: Vaulted relics shown by default
- **WHEN** the application starts with no saved filters and the Relics and Missions tab is displayed
- **THEN** vaulted relics that contain wishlisted parts are shown

#### Scenario: Hide Vaulted hides vaulted relics
- **WHEN** the user checks Vaulted in the Relics Filters menu
- **THEN** vaulted relics are hidden

#### Scenario: Unchecking Hide Vaulted shows vaulted relics again
- **WHEN** Vaulted is checked in the Filters menu and the user unchecks it
- **THEN** vaulted relics that contain wishlisted parts are shown again

## MODIFIED Requirements

### Requirement: Wishlist Relics tab exists in main navigation
The application SHALL display a tab labeled **"Relics and Missions"** in the main tab bar alongside the existing "Drop Tables" and "Prime Gear" tabs, replacing the former "Wishlist Relics" label. The tab SHALL contain two stacked panels: the relic cards panel on top and the farming missions panel below, separated by a **missions separator panel**. A shared **Relics control panel** appears above the relic cards panel. The Relics control panel SHALL begin with a "Relics" section heading followed by the **Filters** menu button, the **sort controls**, and the **Display** menu button (styled as defined by the `control-bar-visual-style` capability, with their behavior specified by the `relics-filter-panel`, `relics-sorting-panel`, and `relics-display-menu` capabilities). No checkbox SHALL appear directly on the control panel, and no panel row SHALL appear below it. The **missions separator panel** SHALL share the tab's control-bar background tint and SHALL begin with a "Missions" section heading followed by the "Show exact drop chances" checkbox chip. The missions separator panel SHALL be a compact divider bar (matching the height of the "Vaulted" separator bar on the Prime Gear tab) with its bottom corners squared where it meets the splitter, and SHALL be pinned to the bottom of the relic-cards section so it sits directly above the splitter. A draggable GridSplitter SHALL be placed immediately below the missions separator panel; dragging it SHALL resize the relic-cards scroll area and the farming missions scroll area (not the separator panel, whose height stays fixed). Each panel SHALL scroll independently within its own scrollable area.

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
- **THEN** the top control panel shows a "Relics" section heading followed by the Filters button, the sort button with its direction button, and the Display button, shows no checkboxes and no row below it, and does NOT show a "Missions" heading or "Show exact drop chances" checkbox

#### Scenario: Missions separator panel shows Missions label and checkbox
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the missions separator panel shows a "Missions" section heading followed by the "Show exact drop chances" checkbox chip

#### Scenario: Missions separator panel is a compact divider bar
- **WHEN** the missions separator panel is displayed
- **THEN** it uses the tab's control-bar background tint, is as tall as the "Vaulted" separator bar on the Prime Gear tab, and has squared bottom corners where it meets the splitter

#### Scenario: GridSplitter below separator resizes the card areas
- **WHEN** the user drags the thin strip immediately below the missions separator panel
- **THEN** the relic cards scroll area and the farming missions scroll area resize accordingly, while the separator panel's height stays fixed

### Requirement: Vaulted relics are visually marked the same as vaulted prime gear
Vaulted relic cards SHALL use the same reduced-opacity visual treatment applied to vaulted prime gear cards.

#### Scenario: Vaulted relic card has reduced opacity
- **WHEN** a vaulted relic is visible (Vaulted is not checked in the Filters menu)
- **THEN** its card is rendered at reduced opacity (matching the vaulted gear appearance)

#### Scenario: Non-vaulted relic card has full opacity
- **WHEN** a relic is not vaulted
- **THEN** its card is rendered at full opacity

## REMOVED Requirements

### Requirement: Show all rewards toggle
**Reason**: Show all rewards is a display option and moves from the Filters panel to the Display menu, where it is greyed out (not hidden) in Table mode and now persists.
**Migration**: See "Display menu offers Show all rewards" and "Show all rewards persists across sessions" in the `relics-display-menu` capability.

### Requirement: Cards follow the active sort
**Reason**: The default sort becomes Era descending, so the default card order now starts with Axi.
**Migration**: See "Cards follow the active sort order" in this capability.

### Requirement: Vaulted relics can be shown or hidden via a checkbox
**Reason**: Vaulted relics are now shown by default; the Show vaulted checkbox becomes the "Hide Vaulted" entry of the Filters menu.
**Migration**: See "Vaulted relics are shown unless hidden from the Filters menu" in this capability.
