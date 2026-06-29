# relics-missions-layout-orientation Specification

## Purpose

Lets the user view the Relics and Missions tab in either a stacked (top/bottom) or a
side-by-side (left/right) layout, choosing between them from a control in Settings →
Appearance, so wide displays can be used more effectively.

## Requirements

### Requirement: Side-by-Side Layout Option

The Relics and Missions tab SHALL offer a side-by-side layout in which the Relics panel
occupies a left column and the Missions panel occupies a right column, separated by a
draggable vertical splitter. This is in addition to the existing stacked (top/bottom)
layout. In the side-by-side layout, each panel SHALL retain its own control bar and its
own independently scrolling card area.

#### Scenario: Side-by-side arranges panels left and right

- **WHEN** the Relics and Missions tab is shown in the side-by-side layout
- **THEN** the Relics panel is on the left and the Missions panel is on the right,
  separated by a vertical splitter, and each panel scrolls independently

#### Scenario: Same content in both layouts

- **WHEN** the user views the tab in either layout
- **THEN** the same relic cards, mission cards, and control-bar options are present;
  only their arrangement differs

### Requirement: Layout Orientation Toggle

The application SHALL provide a user control, located in Settings under the Appearance
group alongside the font-size control, to switch the Relics and Missions tab between the
stacked and side-by-side layouts. The control SHALL be a set of radio options (Stacked /
Side by side). Selecting a layout SHALL apply immediately and auto-save (the same way the
font-size control behaves), without reloading the underlying relic or mission data, so the
Relics and Missions tab reflects the chosen layout the next time it is shown.

#### Scenario: User switches to side-by-side

- **WHEN** the user selects the side-by-side layout in Settings → Appearance while the
  saved layout is stacked
- **THEN** the choice is applied and saved immediately, and the Relics and Missions tab
  shows the side-by-side layout, preserving the currently displayed relics and missions

#### Scenario: User switches back to stacked

- **WHEN** the user selects the stacked layout in Settings → Appearance while the saved
  layout is side-by-side
- **THEN** the choice is applied and saved immediately, and the Relics and Missions tab
  shows the stacked layout

### Requirement: Side-by-Side Minimum Width

In the side-by-side layout, each panel SHALL enforce a minimum width sufficient to show
one full relic/mission card column, so the user cannot drag the vertical splitter to a
point where a panel cannot display its cards. Because card width is fixed, this minimum
SHALL be expressed declaratively and SHALL NOT require runtime measurement of card sizes.

#### Scenario: Splitter cannot collapse a panel below one card width

- **WHEN** the user drags the vertical splitter toward one side beyond that panel's
  minimum width
- **THEN** that panel stops shrinking at its minimum width and remains able to show a
  card column

### Requirement: Default Layout Orientation

The Relics and Missions tab SHALL open in the stacked layout when no layout orientation
has been chosen or persisted, preserving today's default behavior.

#### Scenario: First run defaults to stacked

- **WHEN** the application starts and no layout orientation preference exists
- **THEN** the Relics and Missions tab opens in the stacked layout
