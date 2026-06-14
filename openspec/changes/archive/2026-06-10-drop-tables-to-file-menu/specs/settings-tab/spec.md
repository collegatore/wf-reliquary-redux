## MODIFIED Requirements

### Requirement: Settings Tab Navigation via File Menu

The application SHALL provide a top-level **File** menu in an OS-level menu bar. The File menu SHALL contain a **Settings** item that navigates the user to the Settings tab. The Settings tab header SHALL be hidden from the tab strip while the Settings tab is inactive, and SHALL be visible in the tab strip while the Settings tab is active.

#### Scenario: File menu is visible

- **WHEN** the application window is open
- **THEN** a menu bar with a "File" menu is shown at the top of the window

#### Scenario: Opening Settings via menu

- **WHEN** the user selects File → Settings
- **THEN** the Settings tab content is displayed and the Settings header becomes visible in the tab strip

#### Scenario: Settings header hidden while inactive

- **WHEN** the Settings tab is not the active tab
- **THEN** the tab strip does not show a Settings header
