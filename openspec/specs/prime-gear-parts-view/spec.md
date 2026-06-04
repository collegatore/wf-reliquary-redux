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

### Requirement: Prime gear cards display

The Prime Gear tab SHALL display cards in two spatially separate sections: an **Active** section at the top containing non-vaulted gear cards, and a **Vaulted** section below containing vaulted gear cards. A visually distinct separator row SHALL sit between the two sections. The separator SHALL contain a **Show vaulted** checkbox that controls visibility of the Vaulted section. The Vaulted section SHALL be hidden by default. Each card SHALL show the gear name as its title and a checkbox list of component part labels. The card display set is the union of parts present in the current import and parts in the active wishlist that are absent from the current import (ghost items). The cards view SHALL reflect the current state of the database without requiring an application restart after a successful import.

#### Scenario: Card is shown for each prime gear entry

- **WHEN** the prime gear query returns N gear entries
- **THEN** N cards are visible across both sections of the Prime Gear tab, one per entry

#### Scenario: Non-vaulted cards appear in the Active section

- **WHEN** the Prime Gear tab is displayed
- **THEN** all non-vaulted gear cards appear in the Active section above the separator

#### Scenario: Vaulted cards appear in the Vaulted section

- **WHEN** the Prime Gear tab is displayed
- **THEN** all vaulted gear cards appear in the Vaulted section below the separator

#### Scenario: Vaulted section is hidden by default

- **WHEN** the application launches and the user navigates to the Prime Gear tab
- **THEN** the Vaulted section and its cards are not visible

#### Scenario: Show vaulted checkbox reveals the Vaulted section

- **WHEN** the user checks the **Show vaulted** checkbox in the separator
- **THEN** the Vaulted section becomes visible with its cards

#### Scenario: Show vaulted checkbox hides the Vaulted section

- **WHEN** the **Show vaulted** checkbox is checked and the user unchecks it
- **THEN** the Vaulted section is hidden again

#### Scenario: Separator is visually distinct

- **WHEN** the Prime Gear tab is displayed
- **THEN** the separator row between the Active and Vaulted sections has a visually distinct appearance (e.g. different background) that sets it apart from both card areas

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

### Requirement: Part checkbox state reflects active wishlist

Part checkboxes SHALL reflect membership in the currently active wishlist. Checked state SHALL be persisted between application sessions. Checking or unchecking a part SHALL immediately update the active wishlist.

#### Scenario: User checks a part checkbox

- **WHEN** the user ticks a part checkbox
- **THEN** the part is added to the active wishlist and the checkbox remains checked

#### Scenario: Application is restarted after checking parts

- **WHEN** the user restarts the application after checking one or more part checkboxes
- **THEN** previously checked parts remain checked, reflecting the persisted wishlist state
