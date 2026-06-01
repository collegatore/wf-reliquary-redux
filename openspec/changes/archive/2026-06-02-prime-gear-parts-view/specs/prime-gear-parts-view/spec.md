## ADDED Requirements

### Requirement: Prime gear parts query

The application SHALL expose a query that returns all prime gear entries, where each entry contains the gear name and an ordered list of its component part labels, derived from `reward_items` rows with `item_kind = 'PrimePart'`.

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

### Requirement: Prime gear cards display

The Prime Gear tab SHALL display one card per prime gear entry. Each card SHALL show the gear name as its title and a checkbox list of component part labels.

#### Scenario: Card is shown for each prime gear entry

- **WHEN** the prime gear query returns N gear entries
- **THEN** N cards are visible in the Prime Gear tab, one per entry

#### Scenario: Card title matches gear name

- **WHEN** a card is displayed for `Afuris Prime`
- **THEN** the card title reads `Afuris Prime`

#### Scenario: Card lists all component parts as checkboxes

- **WHEN** a card for `Afuris Prime` is displayed and the entry includes parts `Receiver`, `Barrel`, `Link`, `Blueprint`
- **THEN** four checkboxes are shown with those labels

### Requirement: Part checkbox state is ephemeral

Part checkboxes SHALL be interactive UI state only. Checked state SHALL NOT be persisted between application sessions.

#### Scenario: User checks a part checkbox

- **WHEN** the user ticks a part checkbox
- **THEN** the checkbox appears checked for the remainder of the session

#### Scenario: Application is restarted after checking parts

- **WHEN** the user restarts the application after checking one or more part checkboxes
- **THEN** all checkboxes appear unchecked
