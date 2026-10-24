# Prime Gear Parts View Specification

## Requirements

### Requirement: Prime gear parts query

The application SHALL expose a query that returns all prime gear entries, where each entry contains the gear name, an ordered list of its component part labels, and a vault status flag — all derived from `reward_items` rows with `item_kind = 'PrimePart'` and their associated relic/mission data.

#### Scenario: Query returns grouped gear entries

- **WHEN** the prime gear query is executed against a database containing imported prime part reward items
- **THEN** each distinct prime gear name appears exactly once with all of its component parts listed under it

#### Scenario: Gear name is derived from item name

- **WHEN** a reward item named `Afuris Prime Receiver` with `item_kind = 'PrimePart'` is stored
- **THEN** the query returns a gear entry with `GearName = "Afuris Prime"` and `"Receiver"` in its parts list

#### Scenario: Query returns empty result when no data imported

- **WHEN** the prime gear query is executed against a database with no imported reward items
- **THEN** it returns an empty list without error

### Requirement: Prime Gear tab

The presentation SHALL include a **Prime Gear** tab in the main window alongside the existing drop-table controls tab.

#### Scenario: User opens the application

- **WHEN** the application launches
- **THEN** the main window contains a tab labelled **Prime Gear** and a tab containing the drop-table controls

#### Scenario: User navigates to Prime Gear tab

- **WHEN** the user selects the Prime Gear tab
- **THEN** the prime gear cards view is displayed

### Requirement: Part checkbox state reflects active wishlist

Part checkboxes SHALL reflect membership in the currently active wishlist. Checked state SHALL be persisted between application sessions. Checking or unchecking a part SHALL immediately update the active wishlist.

#### Scenario: User checks a part checkbox

- **WHEN** the user ticks a part checkbox
- **THEN** the part is added to the active wishlist and the checkbox remains checked

#### Scenario: Application is restarted after checking parts

- **WHEN** the user restarts the application after checking one or more part checkboxes
- **THEN** previously checked parts remain checked, reflecting the persisted wishlist state

### Requirement: Prime gear title search

The Prime Gear tab SHALL provide a search box in the tab's control panel that filters the displayed gear cards by their title. The search box SHALL be placed on the **right-hand side** of the control panel, opposite the wishlist selector, the Filters button, and the Group operations button, which remain on the left. The match SHALL be case-insensitive and SHALL use a `contains` predicate against the gear name only (component part labels SHALL NOT be searched). The search SHALL become active only when the query is **3 or more characters** long; with fewer than 3 characters the search SHALL have no effect and all cards SHALL be displayed subject to the other view filters. The view SHALL update **live as the user types** once the threshold is met, and input SHALL be debounced so that rapid typing produces a single filter pass rather than one per keystroke. Clearing the search box SHALL restore the unfiltered view.

#### Scenario: Search box is on the right of the control panel

- **WHEN** the Prime Gear tab is displayed
- **THEN** the search box appears on the right-hand side of the control panel, with the wishlist selector and the Filters and Group operations buttons remaining on the left

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
- **THEN** all cards are displayed subject to the Filters menu, as if no search were entered

#### Scenario: View updates live as the user types

- **WHEN** the user is typing in the search box and the query reaches 3 or more characters
- **THEN** the set of visible cards updates to reflect the current query without requiring the user to submit

#### Scenario: Clearing the search restores the view

- **WHEN** the user clears the search box after a search was active
- **THEN** all cards are displayed again subject to the Filters menu

### Requirement: Empty state when no card matches the search

When an active search excludes every card from view, the Prime Gear tab SHALL display an empty-state indication that no gear matches the search, rather than an empty area with no explanation.

#### Scenario: No matching gear shows an empty state

- **WHEN** a search is active and no card's title matches the query under the current filters
- **THEN** the tab shows an indication that no gear matches the search

#### Scenario: Empty state clears when a match returns

- **WHEN** the empty state is shown and the user edits the query so that at least one card matches
- **THEN** the empty-state indication is removed and the matching card or cards are displayed

### Requirement: Prime gear cards display in Active and Vaulted sections
The Prime Gear tab SHALL display cards in two spatially separate sections: an **Active** section at the top containing non-vaulted gear cards, and a **Vaulted** section below containing vaulted gear cards, under a visually distinct **"Vaulted"** section header. The Vaulted section and its header SHALL be shown by default and hidden while Vaulted is checked in the Filters menu (see `prime-gear-filter-panel`).

#### Scenario: Card is shown for each prime gear entry
- **WHEN** the prime gear query returns N gear entries and nothing is hidden by the Filters menu
- **THEN** N cards are visible across both sections of the Prime Gear tab, one per entry

#### Scenario: Non-vaulted cards appear in the Active section
- **WHEN** the Prime Gear tab is displayed
- **THEN** all non-vaulted gear cards appear in the Active section above the Vaulted header

#### Scenario: Vaulted section is shown by default
- **WHEN** the application starts with no saved filters and the user navigates to the Prime Gear tab
- **THEN** the "Vaulted" header and the vaulted cards beneath it are visible

#### Scenario: Hide Vaulted hides the Vaulted section
- **WHEN** the user checks Vaulted in the Prime Gear Filters menu
- **THEN** the Vaulted section header and its cards are hidden

#### Scenario: Unchecking Hide Vaulted shows the Vaulted section again
- **WHEN** Vaulted is checked in the Filters menu and the user unchecks it
- **THEN** the Vaulted section header and its cards become visible again

### Requirement: Prime gear card content
Each prime gear card SHALL show the gear name as its title and a checkbox list of component part labels. The parts shown are the union of parts in the current import and parts in the active wishlist that are absent from it (ghost items). The cards view SHALL reflect the database after a successful import without an application restart.

#### Scenario: Card title matches gear name
- **WHEN** a card is displayed for `Afuris Prime`
- **THEN** the card title reads `Afuris Prime`

#### Scenario: Card lists all component parts as checkboxes
- **WHEN** a card for `Afuris Prime` is displayed and the entry includes parts `Receiver`, `Barrel`, `Link`, `Blueprint`
- **THEN** four checkboxes are shown with those labels

#### Scenario: Ghost parts appear on card alongside import parts
- **WHEN** the active wishlist contains a part for a gear that is still in the import, but that specific part is absent from the import
- **THEN** the card for that gear shows the ghost part alongside the normally imported parts

#### Scenario: Cards view updates after successful import
- **WHEN** the user triggers a successful drop-table import while the application is running
- **THEN** the Prime Gear cards view reloads and displays the newly imported gear entries without requiring an application restart

### Requirement: Search respects the Filters menu with a hidden-match hint
An active title search SHALL respect the Prime Gear Filters menu: a matching card hidden by a checked entry SHALL stay hidden. While a search is active and such matches exist, the tab SHALL show a hint counting them, separating owned matches from vaulted ones, so the user knows which entry to uncheck.

#### Scenario: Owned match hidden by the Owned filter is counted in the hint
- **WHEN** a search is active, Owned is checked in the Filters menu, and a title match is an owned gear
- **THEN** that card remains hidden and the hint reports a hidden owned match

#### Scenario: Vaulted match hidden by the Vaulted filter is counted in the hint
- **WHEN** a search is active, Vaulted is checked in the Filters menu, and a title match is a vaulted gear
- **THEN** that card remains hidden and the hint reports a hidden vaulted match

#### Scenario: No hint when all matches are visible
- **WHEN** a search is active and every title match is visible under the current filters
- **THEN** no hidden-match hint is shown

#### Scenario: Unchecking the entry reveals the hidden match
- **WHEN** the hint reports a hidden owned match and the user unchecks Owned in the Filters menu
- **THEN** the matching owned card becomes visible and the hint no longer counts it
