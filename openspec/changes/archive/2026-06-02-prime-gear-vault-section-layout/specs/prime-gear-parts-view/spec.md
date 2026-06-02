## MODIFIED Requirements

### Requirement: Prime gear cards display

The Prime Gear tab SHALL display cards in two spatially separate sections: an **Active** section at the top containing non-vaulted gear cards, and a **Vaulted** section below containing vaulted gear cards. A visually distinct separator row SHALL sit between the two sections. The separator SHALL contain a **Show vaulted** checkbox that controls visibility of the Vaulted section. The Vaulted section SHALL be hidden by default. Each card SHALL show the gear name as its title and a checkbox list of component part labels. The cards view SHALL reflect the current state of the database without requiring an application restart after a successful import.

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

#### Scenario: Cards view updates after successful import

- **WHEN** the user triggers a successful drop-table import while the application is running
- **THEN** the Prime Gear cards view reloads and displays the newly imported gear entries without requiring an application restart
