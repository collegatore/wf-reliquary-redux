## MODIFIED Requirements

### Requirement: Enable group operations toggle

The Prime Gear control panel SHALL include a **Group operations** button that toggles group-operations mode on and off. The button's active/inactive state SHALL be conveyed by the group-operations control row appearing (mode on) or disappearing (mode off) rather than by the button's own visual state. The toggle SHALL be ephemeral — it SHALL default to off each time the application starts and SHALL NOT be persisted across sessions. While the toggle is off, the tab SHALL behave exactly as before (per-card overflow menus, no second control row). Turning the toggle off SHALL clear any current group selection.

#### Scenario: Group operations button is present at launch

- **WHEN** the application launches and the user navigates to the Prime Gear tab
- **THEN** the control panel shows a "Group operations" button and the group-operations control row is not visible

#### Scenario: Toggle is not persisted

- **WHEN** the user enables group operations and then restarts the application
- **THEN** group operations is off again after restart

#### Scenario: Turning off group operations clears the selection

- **WHEN** one or more cards are selected and the user clicks the Group operations button to turn it off
- **THEN** the selection is cleared
