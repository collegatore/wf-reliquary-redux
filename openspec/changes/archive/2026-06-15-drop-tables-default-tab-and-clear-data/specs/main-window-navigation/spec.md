## MODIFIED Requirements

### Requirement: Default Startup Tab

On application start, the main window SHALL select the **Drop Tables** tab when no drop-table import data exists. When import data exists, it SHALL select the **Prime Gear** tab. The initial tab is determined after the startup status load completes.

#### Scenario: Drop Tables is shown on startup with no imported data

- **WHEN** the application window first opens and no import data exists in the database
- **THEN** the Drop Tables tab is the selected tab and its content is displayed

#### Scenario: Prime Gear is shown on startup when import data exists

- **WHEN** the application window first opens and import data exists in the database
- **THEN** the Prime Gear tab is the selected tab and its content is displayed

### Requirement: Menu-Accessed Tab Header Visibility

A tab that is reached through the File menu (**Settings** and **Drop Tables**) SHALL hide its header from the tab strip while that tab is inactive, and SHALL show its header in the tab strip while that tab is the active tab. This lets the user see which menu-accessed tab is current and click a strip tab to leave it.

The **Drop Tables** tab header is an exception: it SHALL remain visible in the tab strip even while inactive when no drop-table import data exists, to guide users toward importing data.

#### Scenario: Menu-accessed header hidden while inactive

- **WHEN** a menu-accessed tab (Drop Tables or Settings) is not the active tab and drop-table import data exists
- **THEN** that tab's header is not visible in the tab strip

#### Scenario: Menu-accessed header visible while active

- **WHEN** the user activates a menu-accessed tab (Drop Tables or Settings) via the File menu
- **THEN** that tab's header becomes visible in the tab strip alongside the always-visible tab headers

#### Scenario: Header hides again after leaving

- **WHEN** a menu-accessed tab is active and the user navigates to a different tab and import data exists
- **THEN** the previously active menu-accessed tab's header is no longer visible in the tab strip

#### Scenario: Drop Tables header visible while inactive with no import data

- **WHEN** the Drop Tables tab is not the active tab and no drop-table import data exists
- **THEN** the Drop Tables tab header remains visible in the tab strip

#### Scenario: Drop Tables header reverts to hide-when-inactive after import

- **WHEN** a drop-table import completes successfully and the user navigates away from the Drop Tables tab
- **THEN** the Drop Tables tab header is no longer visible in the tab strip
