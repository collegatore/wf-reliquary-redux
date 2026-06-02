## MODIFIED Requirements

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

### Requirement: Prime gear cards display

The Prime Gear tab SHALL display one card per prime gear entry. Each card SHALL show the gear name as its title and a checkbox list of component part labels. The cards view SHALL reflect the current state of the database without requiring an application restart after a successful import.

#### Scenario: Card is shown for each prime gear entry

- **WHEN** the prime gear query returns N gear entries
- **THEN** N cards are visible in the Prime Gear tab, one per entry

#### Scenario: Card title matches gear name

- **WHEN** a card is displayed for `Afuris Prime`
- **THEN** the card title reads `Afuris Prime`

#### Scenario: Card lists all component parts as checkboxes

- **WHEN** a card for `Afuris Prime` is displayed and the entry includes parts `Receiver`, `Barrel`, `Link`, `Blueprint`
- **THEN** four checkboxes are shown with those labels

#### Scenario: Cards view updates after successful import

- **WHEN** the user triggers a successful drop-table import while the application is running
- **THEN** the Prime Gear cards view reloads and displays the newly imported gear entries without requiring an application restart
