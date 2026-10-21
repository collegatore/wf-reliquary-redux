# Spec Delta

## MODIFIED Requirements

### Requirement: Wishlist Relics tab exists in main navigation
The application SHALL display a tab labeled **"Relics and Missions"** in the main tab bar alongside the existing "Drop Tables" and "Prime Gear" tabs, replacing the former "Wishlist Relics" label. The tab SHALL contain two stacked panels: the relic cards panel on top and the farming missions panel below, separated by a **missions separator panel**. A shared **Relics control panel** appears above the relic cards panel. The Relics control panel SHALL begin with a "Relics" section heading followed by a **Filters** button and a **Sorting** button grouped as outlined button chips (heading and buttons as defined by the `control-bar-visual-style` capability), each toggling its own collapsible panel row below the control panel (specified by the `relics-filter-panel` and `relics-sorting-panel` capabilities). The Show vaulted and Show all rewards checkboxes SHALL NOT appear directly on the control panel; they live inside the Filters panel. The **missions separator panel** SHALL share the tab's control-bar background tint and SHALL begin with a "Missions" section heading followed by the "Show exact drop chances" checkbox chip. The missions separator panel SHALL be a compact divider bar (matching the height of the "Vaulted" separator bar on the Prime Gear tab) with its bottom corners squared where it meets the splitter, and SHALL be pinned to the bottom of the relic-cards section so it sits directly above the splitter. A draggable GridSplitter SHALL be placed immediately below the missions separator panel; dragging it SHALL resize the relic-cards scroll area and the farming missions scroll area (not the separator panel, whose height stays fixed). Each panel SHALL scroll independently within its own scrollable area.

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
- **THEN** the top control panel shows a "Relics" section heading followed by the Filters and Sorting buttons, shows no inline filter checkboxes, and does NOT show a "Missions" heading or "Show exact drop chances" checkbox

#### Scenario: Missions separator panel shows Missions label and checkbox
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the missions separator panel shows a "Missions" section heading followed by the "Show exact drop chances" checkbox chip

#### Scenario: Missions separator panel is a compact divider bar
- **WHEN** the missions separator panel is displayed
- **THEN** it uses the tab's control-bar background tint, is as tall as the "Vaulted" separator bar on the Prime Gear tab, and has squared bottom corners where it meets the splitter

#### Scenario: GridSplitter below separator resizes the card areas
- **WHEN** the user drags the thin strip immediately below the missions separator panel
- **THEN** the relic cards scroll area and the farming missions scroll area resize accordingly, while the separator panel's height stays fixed
