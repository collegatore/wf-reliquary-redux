## ADDED Requirements

### Requirement: Prime gear title search

The Prime Gear tab SHALL provide a search box in the tab's control panel that filters the displayed gear cards by their title. The search box SHALL be placed on the **right-hand side** of the control panel, opposite the wishlist selector and the Show vaulted / Show owned toggles which remain on the left. The match SHALL be case-insensitive and SHALL use a `contains` predicate against the gear name only (component part labels SHALL NOT be searched). The search SHALL become active only when the query is **3 or more characters** long; with fewer than 3 characters the search SHALL have no effect and all cards SHALL be displayed subject to the other view filters. The view SHALL update **live as the user types** once the threshold is met, and input SHALL be debounced so that rapid typing produces a single filter pass rather than one per keystroke. Clearing the search box SHALL restore the unfiltered view.

#### Scenario: Search box is on the right of the control panel

- **WHEN** the Prime Gear tab is displayed
- **THEN** the search box appears on the right-hand side of the control panel, with the wishlist selector and the Show vaulted / Show owned toggles remaining on the left

#### Scenario: Search filters cards by title substring

- **WHEN** the user types `volt` into the search box and gear cards include `Volt Prime` and `Mag Prime`
- **THEN** the `Volt Prime` card is visible and the `Mag Prime` card is hidden

#### Scenario: Match is case-insensitive

- **WHEN** the user types `VOLT` into the search box and a `Volt Prime` card exists
- **THEN** the `Volt Prime` card is visible

#### Scenario: Search matches gear name only, not part labels

- **WHEN** the user types a query that matches a component part label (e.g. `barrel`) but no gear name
- **THEN** no card is shown as a match on that basis

#### Scenario: Below the threshold the search has no effect

- **WHEN** the search box contains fewer than 3 characters
- **THEN** all cards are displayed subject to the Show owned and Show vaulted filters, as if no search were entered

#### Scenario: View updates live as the user types

- **WHEN** the user is typing in the search box and the query reaches 3 or more characters
- **THEN** the set of visible cards updates to reflect the current query without requiring the user to submit

#### Scenario: Clearing the search restores the view

- **WHEN** the user clears the search box after a search was active
- **THEN** all cards are displayed again subject to the Show owned and Show vaulted filters

### Requirement: Search respects view filters with a hidden-match hint

An active title search SHALL respect the Show owned and Show vaulted filters: a card whose title matches the query but which is currently hidden by one of those filters SHALL remain hidden. When such hidden matches exist, the tab SHALL display a hint indicating how many title matches are hidden by the filters, distinguishing matches hidden because they are owned from matches hidden because they are vaulted, so the user knows which toggle to enable. The hint SHALL only appear while a search is active and at least one matching card is hidden by a filter.

#### Scenario: Owned match hidden by Show owned filter is counted in the hint

- **WHEN** a search is active, Show owned gear is unchecked, and a title match is an owned gear
- **THEN** that card remains hidden and the hint reports a hidden owned match

#### Scenario: Vaulted match hidden by Show vaulted filter is counted in the hint

- **WHEN** a search is active, Show vaulted gear is unchecked, and a title match is a vaulted gear
- **THEN** that card remains hidden and the hint reports a hidden vaulted match

#### Scenario: No hint when all matches are visible

- **WHEN** a search is active and every title match is visible under the current filters
- **THEN** no hidden-match hint is shown

#### Scenario: Enabling the toggle reveals the hidden match

- **WHEN** the hint reports a hidden owned match and the user checks Show owned gear
- **THEN** the matching owned card becomes visible and the hint no longer counts it

### Requirement: Empty state when no card matches the search

When an active search excludes every card from view, the Prime Gear tab SHALL display an empty-state indication that no gear matches the search, rather than an empty area with no explanation.

#### Scenario: No matching gear shows an empty state

- **WHEN** a search is active and no card's title matches the query under the current filters
- **THEN** the tab shows an indication that no gear matches the search

#### Scenario: Empty state clears when a match returns

- **WHEN** the empty state is shown and the user edits the query so that at least one card matches
- **THEN** the empty-state indication is removed and the matching card or cards are displayed
