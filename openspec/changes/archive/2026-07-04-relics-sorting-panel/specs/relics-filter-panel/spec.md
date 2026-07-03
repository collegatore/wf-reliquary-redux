# Relics Filter Panel

## ADDED Requirements

### Requirement: Filters button toggles a collapsible filter panel
The Relics control panel SHALL contain a **Filters** button (transparent background, borderless, matching the Prime Gear Filters button style). Pressing it SHALL toggle a collapsible filter panel rendered as its own row directly below the Relics control panel, using the same background tint and corner styling as the control panel. The filter panel SHALL be closed by default when the application starts, and its open/closed state SHALL NOT persist across sessions. Closing the panel SHALL NOT reset any filter state.

#### Scenario: Filters button opens the panel
- **WHEN** the user presses the Filters button while the filter panel is closed
- **THEN** the filter panel appears below the Relics control panel

#### Scenario: Closing the panel keeps filters applied
- **WHEN** the user checks Show vaulted and then presses the Filters button to close the panel
- **THEN** vaulted relics remain visible

#### Scenario: Panel is closed on startup
- **WHEN** the application starts
- **THEN** the filter panel is not visible until the Filters button is pressed

### Requirement: Filter panel hosts the Show vaulted and Show all rewards checkboxes
The filter panel SHALL contain the **Show vaulted** and **Show all rewards** checkboxes (moved from the Relics control panel), separated by a thin vertical divider, matching the Prime Gear filter panel layout. Both checkboxes SHALL keep their existing behavior: unchecked by default, session-only state, and immediate effect on the relic views. The Show all rewards checkbox (and its divider) SHALL be hidden while the **Table** view mode is active, with its checked state retained, exactly as it behaved on the control panel.

#### Scenario: Checkboxes appear inside the panel
- **WHEN** the filter panel is open in a card view mode
- **THEN** it shows the Show vaulted and Show all rewards checkboxes, and the Relics control panel itself shows neither checkbox

#### Scenario: Show all rewards hidden in Table mode inside the panel
- **WHEN** the filter panel is open and the user switches to Table view mode
- **THEN** the Show all rewards checkbox is not visible in the panel while Show vaulted remains visible

#### Scenario: Show all rewards state survives Table mode
- **WHEN** Show all rewards is checked, the user switches to Table mode and back to Compact mode
- **THEN** the checkbox reappears in the filter panel still checked
