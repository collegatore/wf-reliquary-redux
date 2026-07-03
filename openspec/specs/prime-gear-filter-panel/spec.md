# Prime Gear Filter Panel Specification

## Purpose

<!-- TBD: describe the purpose of the filter panel capability -->

## Requirements

### Requirement: Collapsible filter panel

The Prime Gear tab SHALL have a collapsible filter panel beneath the primary control panel (the wishlist selector / search row). The filter panel SHALL be hidden by default when the tab is first shown. A **Filters** button in the primary control panel SHALL toggle the filter panel open and closed. The filter panel's open/closed state SHALL be ephemeral — it SHALL NOT be persisted across application restarts.

#### Scenario: Filter panel is hidden by default

- **WHEN** the user navigates to the Prime Gear tab
- **THEN** the filter panel is not visible

#### Scenario: Filters button opens the filter panel

- **WHEN** the filter panel is closed and the user clicks the Filters button
- **THEN** the filter panel becomes visible

#### Scenario: Filters button closes the filter panel

- **WHEN** the filter panel is open and the user clicks the Filters button
- **THEN** the filter panel is hidden

#### Scenario: Filter panel state resets on restart

- **WHEN** the user opens the filter panel and then restarts the application
- **THEN** the filter panel is hidden again on next launch

### Requirement: Show vaulted and Show owned controls in filter panel

The **Show vaulted** and **Show owned** checkboxes SHALL be located inside the filter panel. They SHALL NOT appear in the primary control panel. Their behavior (what they show/hide) is unchanged from the existing specs.

#### Scenario: Show vaulted is in the filter panel

- **WHEN** the filter panel is open on the Prime Gear tab
- **THEN** a Show vaulted checkbox is visible inside the filter panel

#### Scenario: Show owned is in the filter panel

- **WHEN** the filter panel is open on the Prime Gear tab
- **THEN** a Show owned checkbox is visible inside the filter panel

#### Scenario: Show vaulted and Show owned are not in the primary panel

- **WHEN** the Prime Gear tab is displayed and the filter panel is closed
- **THEN** no Show vaulted or Show owned checkbox is visible in the primary control panel

### Requirement: Wishlisted only filter

The filter panel SHALL include a **Wishlisted only** checkbox. When checked, only gear cards that have at least one part checkbox checked (i.e. at least one part in the active wishlist) SHALL be shown. Cards with no checked parts SHALL be hidden. When unchecked, this filter has no effect and cards are shown according to the other active filters.

#### Scenario: Wishlisted only is unchecked by default

- **WHEN** the filter panel is opened for the first time
- **THEN** the Wishlisted only checkbox is unchecked

#### Scenario: Wishlisted only hides gear with no checked parts

- **WHEN** Wishlisted only is checked and a gear card has no parts checked
- **THEN** that gear card is not visible

#### Scenario: Wishlisted only shows gear with at least one checked part

- **WHEN** Wishlisted only is checked and a gear card has at least one part checked
- **THEN** that gear card is visible (subject to other active filters)

#### Scenario: Unchecking Wishlisted only restores hidden cards

- **WHEN** Wishlisted only is checked and the user unchecks it
- **THEN** gear cards that were hidden solely because of this filter become visible again

#### Scenario: Wishlisted only interacts with owned filter

- **WHEN** both Wishlisted only and Show owned are unchecked, and a gear card is owned with at least one part checked
- **THEN** that card is hidden (owned filter takes precedence; owned-hidden cards are not shown regardless of wishlist state)

#### Scenario: Wishlisted only state resets on restart

- **WHEN** the user checks Wishlisted only and then restarts the application
- **THEN** Wishlisted only is unchecked on next launch
