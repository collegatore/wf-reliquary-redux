# split-panel-persistence Specification

## Purpose

Persists and restores the Relics and Missions tab's split layout — the stacked relics
panel height, the chosen layout orientation, and the side-by-side relics column width — so
the user's preferred arrangement survives across application launches.

## Requirements

### Requirement: Draggable Relics/Missions Split

The Relics and Missions tab SHALL provide a draggable splitter between the Relics panel and the Missions panel. Dragging the splitter changes the pixel height of the Relics panel; the Missions panel fills the remaining space.

#### Scenario: Splitter is interactable

- **WHEN** the user drags the splitter between the Relics and Missions panels
- **THEN** the Relics panel and Missions panel heights change in response

### Requirement: Relics Panel Height Persisted and Restored

The pixel height of the Relics panel SHALL be saved both when the user explicitly saves settings **and** automatically when the window is closed. It SHALL be restored directly on the next launch. On window resize after a drag, only the Missions (bottom) panel grows or shrinks; the Relics panel height is unaffected.

> **Implementation note:** Row initialisation (star → pixel) is deferred to the first `SizeChanged` event that reports a non-zero grid height. This handles the case where the Relics tab is not the active tab at startup — the inactive tab's grid has zero bounds until first shown. The handler unsubscribes itself immediately after initialising so it has zero overhead during subsequent resizes.

> **Implementation note:** Mission cards use a fixed `Width` (not `MinWidth`) so that the WrapPanel in the Missions panel does not trigger expensive text re-wrap calculations on every horizontal window resize event.

#### Scenario: Relics panel height saved on explicit Save

- **WHEN** the user adjusts the split and clicks Save in Settings
- **THEN** the Relics panel pixel height is written to persistent user settings

#### Scenario: Relics panel height saved on window close

- **WHEN** the user drags the splitter and then closes the window without visiting Settings
- **THEN** the Relics panel pixel height is written to persistent user settings

#### Scenario: Relics panel height restored on launch

- **WHEN** the application starts and a Relics panel height is saved
- **THEN** the Relics panel opens at exactly the saved pixel height

#### Scenario: Default height used when no setting exists

- **WHEN** the application starts and no Relics panel height is saved
- **THEN** the Relics and Missions panels open at a 1:2 height ratio (Relics occupies one third, Missions two thirds)

### Requirement: Relics Panel Minimum Height

The Relics panel SHALL enforce a minimum height equal to the rendered height of one card row, so the user cannot drag the splitter to a point where no cards are visible. The minimum SHALL update automatically when the **Show all rewards** toggle changes card height.

#### Scenario: Splitter cannot collapse relics below one card row

- **WHEN** the user drags the splitter upward beyond the height of the tallest visible relic card
- **THEN** the Relics panel stops shrinking at that card's height

#### Scenario: Minimum updates when Show all rewards is toggled

- **WHEN** the user toggles "Show all rewards" (changing the card height)
- **THEN** the Relics panel minimum height is recalculated to match the new card height

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
