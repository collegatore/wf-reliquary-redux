## MODIFIED Requirements

### Requirement: "Show exact drop chances" toggle controls relic detail level
The "Relics and Missions" tab control panel SHALL include a "Show exact drop chances" checkbox placed in a dedicated **Missions** group, separated from the Relics controls by a visual separator and preceded by a "Missions:" text label. When checked, relic entries in farming mission cards render one line per relic with the relic name and its chance percentage. When unchecked (default), all relics for a given section SHALL be shown as a single compact row containing only relic names (comma-separated), with no chance percentages visible.

#### Scenario: Toggle on shows per-relic rows with chances
- **WHEN** "Show exact drop chances" is checked
- **THEN** each relic in a non-rotation mission card occupies its own row displaying the relic name and its formatted chance percentage

#### Scenario: Toggle on shows per-relic rows with chances in rotation cards
- **WHEN** "Show exact drop chances" is checked and a rotation mission card is displayed
- **THEN** each relic within a rotation section occupies its own row displaying the relic name and chance percentage

#### Scenario: Toggle off shows compact relic row without chances
- **WHEN** "Show exact drop chances" is unchecked
- **THEN** all relics in a non-rotation mission card's relic section are shown in a single row as comma-separated relic names, with no chance text visible

#### Scenario: Toggle off shows compact relic row within each rotation
- **WHEN** "Show exact drop chances" is unchecked and a rotation mission card is displayed
- **THEN** each rotation's relic list is replaced by a single compact row of comma-separated relic names, while the rotation label and cumulative chance remain visible

#### Scenario: Toggle default state is unchecked
- **WHEN** the "Relics and Missions" tab is first displayed in a session
- **THEN** the "Show exact drop chances" checkbox is unchecked and each card's relic section shows the compact comma-separated relic names

#### Scenario: Toggle state applies to all visible cards simultaneously
- **WHEN** the user changes the "Show exact drop chances" toggle
- **THEN** all currently visible farming mission cards update immediately to the new display mode without requiring a reload

#### Scenario: Toggle is visually grouped under the Missions label
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the "Show exact drop chances" checkbox appears after a separator following the Relics group, preceded by a "Missions:" label
