# split-panel-persistence Specification

## ADDED Requirements

### Requirement: Layout Orientation Persisted and Restored

The chosen Relics/Missions layout orientation (stacked or side-by-side) SHALL be saved to
persistent user settings — both on explicit Save and automatically on window close — and
SHALL be restored on the next launch. The existing stacked-layout height persistence is
unaffected by this requirement.

#### Scenario: Orientation saved and restored

- **WHEN** the user switches the Relics/Missions layout and later relaunches the
  application
- **THEN** the tab opens in the orientation that was last selected

#### Scenario: Orientation saved on window close

- **WHEN** the user switches the layout and then closes the window without visiting
  Settings
- **THEN** the selected orientation is written to persistent user settings

### Requirement: Side-by-Side Split Position Persisted Independently

The side-by-side layout's split position SHALL be persisted as the relics column's width
in pixels and restored independently of the stacked layout's relics pixel height, so that
switching between orientations preserves each one's last split position. The relics column
is a fixed-width column and the missions column absorbs the remaining space; consequently
resizing the application window keeps the relics column static and only changes the
missions column.

#### Scenario: Side-by-side position remembered separately

- **WHEN** the user sets a split position in side-by-side, switches to stacked and
  adjusts that split, then switches back to side-by-side
- **THEN** the side-by-side layout restores its own last relics column width, unaffected
  by the stacked adjustment

#### Scenario: Default side-by-side position when none saved

- **WHEN** the side-by-side layout is shown and no side-by-side relics column width is
  saved
- **THEN** the relics column opens at a sensible default fixed width

#### Scenario: Window resize keeps the relics column static

- **WHEN** the side-by-side layout is shown and the user resizes the application window
- **THEN** the relics column keeps its fixed width and only the missions column grows or
  shrinks
