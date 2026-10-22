# Spec Delta

## MODIFIED Requirements

### Requirement: Click-to-select cards in group-operations mode

While group operations is enabled, each Prime gear card SHALL become a single selection target: clicking anywhere on the card SHALL toggle its selection, and the card's own interactive controls (the per-part checkboxes and the overflow-menu button) SHALL be suppressed so they do not respond while group operations is enabled. To make this discoverable, while group operations is enabled each card SHALL show a selection checkbox in the card header in place of the overflow-menu button, checked exactly when the card is selected; the card's parts list SHALL be visibly dimmed; and the whole card SHALL show an outline while the pointer is over it. A selected card SHALL be indicated by a subtle background wash, distinct from the owned and vaulted treatments, together with its checked selection checkbox. Selecting a card SHALL NOT change its owned state. While group operations is disabled, cards SHALL behave normally — the overflow menu and part checkboxes SHALL be interactive, no selection checkbox is shown, the parts list is not dimmed, and clicking the card body SHALL NOT select it.

#### Scenario: Clicking a card selects it in group mode

- **WHEN** group operations is enabled and the user clicks anywhere on a gear card
- **THEN** the card becomes selected and is shown with the selection wash

#### Scenario: Clicking a selected card deselects it

- **WHEN** group operations is enabled and the user clicks a card that is already selected
- **THEN** the card becomes deselected

#### Scenario: Card controls are suppressed in group mode

- **WHEN** group operations is enabled
- **THEN** the per-part checkboxes and the overflow-menu button on each card do not respond to clicks

#### Scenario: Selecting a card does not change its owned state

- **WHEN** the user selects a gear card in group mode
- **THEN** the card becomes selected and its owned state is unchanged

#### Scenario: Cards behave normally when group operations is off

- **WHEN** group operations is disabled
- **THEN** clicking a card body does not select it, and the overflow menu and part checkboxes are interactive

#### Scenario: Selection checkbox replaces the overflow menu in group mode

- **WHEN** the user enables group operations
- **THEN** every card header shows an unchecked selection checkbox where the overflow-menu button was, and no overflow-menu button is shown

#### Scenario: Selection checkbox follows the selection

- **WHEN** group operations is enabled and the user selects a card, then invokes Select none
- **THEN** that card's selection checkbox is checked after the selection and unchecked after Select none

#### Scenario: Clicking the selection checkbox toggles selection

- **WHEN** group operations is enabled and the user clicks a card's selection checkbox
- **THEN** the card's selection toggles exactly as when clicking elsewhere on the card

#### Scenario: Parts list is dimmed in group mode

- **WHEN** the user enables group operations
- **THEN** each card's parts list is rendered dimmed, and it returns to full strength when group operations is disabled

#### Scenario: Card shows an outline on hover in group mode

- **WHEN** group operations is enabled and the pointer moves over a gear card
- **THEN** an outline appears around that whole card, and disappears when the pointer leaves

#### Scenario: No hover outline when group operations is off

- **WHEN** group operations is disabled and the pointer moves over a gear card
- **THEN** no selection hover outline appears

### Requirement: Group-operations control row

While group operations is enabled, the Prime Gear control panel SHALL display a second row containing a **selection status**, a **Select all** action, a **Select none** action, a **Mark owned** action, and an **Unmark owned** action. This row SHALL be hidden while group operations is disabled. While no card is selected, the selection status SHALL read "Click cards to select them"; while one or more cards are selected, it SHALL be a selection count reflecting the number of currently selected cards. The **Unmark owned** action SHALL be shown only while Show owned gear is checked, because owned cards (the only ones it affects) are hidden, and so cannot be selected, otherwise.

#### Scenario: Second row appears only in group-operations mode

- **WHEN** group operations is enabled with Show owned gear checked
- **THEN** the control panel shows a second row with Select all, Select none, Mark owned, and Unmark owned actions and a selection status

#### Scenario: Second row hidden when group operations is off

- **WHEN** group operations is disabled
- **THEN** the second control row is not shown

#### Scenario: Selection count reflects selected cards

- **WHEN** the user selects three cards in group-operations mode
- **THEN** the control row reports three selected

#### Scenario: Hint is shown while nothing is selected

- **WHEN** the user enables group operations and no card is selected
- **THEN** the selection status reads "Click cards to select them"

#### Scenario: Unmark owned follows Show owned gear
- **WHEN** group operations is enabled and the user unchecks Show owned gear
- **THEN** Unmark owned is hidden while the other actions stay, and it reappears when Show owned gear is checked again

#### Scenario: Hint returns when the selection empties

- **WHEN** cards are selected and the selection becomes empty (Select none, a bulk action completing, or a view change)
- **THEN** the selection status reads "Click cards to select them" again

## ADDED Requirements

### Requirement: Selection actions are inactive when they have nothing to act on
In group-operations mode, an action SHALL be inactive (no response to clicks, no hover feedback, an inactive look) whenever it has nothing to act on: **Select none** while no card is selected, **Mark owned** while no selected card is unowned, and **Unmark owned** while no selected card is owned. **Select all** SHALL stay active. Each action SHALL re-evaluate whenever the selection changes.

#### Scenario: Actions are inactive on entering group mode
- **WHEN** the user enables group operations and no card is selected
- **THEN** Select none, Mark owned, and Unmark owned are shown inactive and clicking them does nothing, while Select all is active

#### Scenario: Only unowned cards selected
- **WHEN** the selection contains only cards that are not owned
- **THEN** Select none and Mark owned are active and Unmark owned is inactive

#### Scenario: Only owned cards selected
- **WHEN** Show owned gear is checked and the selection contains only owned cards
- **THEN** Select none and Unmark owned are active and Mark owned is inactive

#### Scenario: Mixed selection
- **WHEN** the selection contains both owned and unowned cards
- **THEN** Select none, Mark owned, and Unmark owned are all active

#### Scenario: Actions deactivate when the selection empties
- **WHEN** cards are selected and the user invokes Select none
- **THEN** Select none, Mark owned, and Unmark owned become inactive again
