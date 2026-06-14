## ADDED Requirements

### Requirement: Default Startup Tab

On application start, the main window SHALL select the **Prime Gear** tab. The **Drop Tables** tab SHALL NOT be the selected tab on startup.

#### Scenario: Prime Gear is shown on startup

- **WHEN** the application window first opens
- **THEN** the Prime Gear tab is the selected tab and its content is displayed

#### Scenario: Drop Tables is not shown on startup

- **WHEN** the application window first opens
- **THEN** the Drop Tables tab is not the selected tab and its content is not displayed

### Requirement: Tab Strip Composition

The always-visible tab strip SHALL contain the **Prime Gear** and **Relics and Missions** tabs. The **Drop Tables** and **Settings** tabs SHALL be reached through the **File** menu rather than from a permanently visible strip header.

#### Scenario: Everyday tabs are in the strip

- **WHEN** the application window is open and no menu-accessed tab is active
- **THEN** the tab strip shows the Prime Gear and Relics and Missions headers and does not show the Drop Tables or Settings headers

#### Scenario: Drop Tables is reachable from the File menu

- **WHEN** the user opens the File menu
- **THEN** a Drop Tables item is shown that, when selected, displays the Drop Tables tab content

### Requirement: Menu-Accessed Tab Header Visibility

A tab that is reached through the File menu (**Settings** and **Drop Tables**) SHALL hide its header from the tab strip while that tab is inactive, and SHALL show its header in the tab strip while that tab is the active tab. This lets the user see which menu-accessed tab is current and click a strip tab to leave it.

#### Scenario: Menu-accessed header hidden while inactive

- **WHEN** a menu-accessed tab (Drop Tables or Settings) is not the active tab
- **THEN** that tab's header is not visible in the tab strip

#### Scenario: Menu-accessed header visible while active

- **WHEN** the user activates a menu-accessed tab (Drop Tables or Settings) via the File menu
- **THEN** that tab's header becomes visible in the tab strip alongside the always-visible tab headers

#### Scenario: Header hides again after leaving

- **WHEN** a menu-accessed tab is active and the user navigates to a different tab
- **THEN** the previously active menu-accessed tab's header is no longer visible in the tab strip
