## MODIFIED Requirements

### Requirement: Wishlist Relics tab exists in main navigation
The application SHALL display a tab labeled **"Relics and Missions"** in the main tab bar alongside the existing "Drop Tables" and "Prime Gear" tabs, replacing the former "Wishlist Relics" label. The tab SHALL contain two stacked panels: the relic cards panel on top and the farming missions panel below, separated by a divider. A shared control panel appears above the relic cards panel. The control panel SHALL be divided into two labelled groups: a **Relics** group containing the "Show vaulted" and "Show all rewards" checkboxes, and a **Missions** group containing the "Show exact drop chances" checkbox. The Relics group SHALL be preceded by a plain "Relics:" text label. Each panel SHALL scroll independently within its own scrollable area; the two panels share the available tab height equally.

#### Scenario: Tab is visible in main window
- **WHEN** the application is open
- **THEN** a tab labeled "Relics and Missions" is visible in the tab bar

#### Scenario: Tab displays both panels when selected
- **WHEN** the user selects the "Relics and Missions" tab
- **THEN** the relic cards panel is visible in the upper half and the farming missions panel is visible in the lower half

#### Scenario: Panels scroll independently
- **WHEN** the user scrolls in the relic cards panel
- **THEN** the farming missions panel position is unaffected, and vice versa

#### Scenario: Control panel shows Relics label
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the control panel shows a "Relics:" text label before the Relics-group checkboxes

#### Scenario: Control panel shows Missions label
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the control panel shows a "Missions:" text label before the Missions-group checkboxes

---

### Requirement: Vaulted relics can be shown or hidden via a checkbox
Above the relic cards there SHALL be a control panel containing a checkbox labeled **"Show vaulted"** (within the Relics group). By default the checkbox is unchecked (vaulted relics hidden). Checking it reveals vaulted relics.

A relic is considered vaulted if it does not appear as a reward in any active mission.

#### Scenario: Vaulted relics hidden by default
- **WHEN** the Wishlist Relics tab is first displayed
- **THEN** relics that are vaulted are not shown

#### Scenario: Checking show-vaulted reveals vaulted relics
- **WHEN** the user checks "Show vaulted"
- **THEN** vaulted relics that contain wishlisted parts become visible

#### Scenario: Unchecking show-vaulted hides vaulted relics again
- **WHEN** the user unchecks "Show vaulted" after they were shown
- **THEN** vaulted relics are hidden again
