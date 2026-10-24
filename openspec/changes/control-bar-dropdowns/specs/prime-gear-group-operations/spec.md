# Spec Delta

## ADDED Requirements

### Requirement: Group-operations action row
While group operations is enabled, the Prime Gear control panel SHALL display a second row containing a **selection status**, a **Select all** action, a **Select none** action, a **Mark owned** action, and an **Unmark owned** action. This row SHALL be hidden while group operations is disabled. All four actions SHALL always be shown; an action with nothing to act on is inactive rather than hidden.

#### Scenario: Second row appears only in group-operations mode
- **WHEN** group operations is enabled
- **THEN** the control panel shows a second row with Select all, Select none, Mark owned, and Unmark owned actions and a selection status

#### Scenario: Second row hidden when group operations is off
- **WHEN** group operations is disabled
- **THEN** the second control row is not shown

#### Scenario: Unmark owned stays shown while owned gear is hidden
- **WHEN** group operations is enabled and Owned is checked in the Prime Gear Filters menu
- **THEN** Unmark owned is still shown, inactive, next to Mark owned

---

### Requirement: Selection status in the action row
The action row's selection status SHALL read "Click cards to select them" while no card is selected, and otherwise SHALL be a count of the currently selected cards.

#### Scenario: Selection count reflects selected cards
- **WHEN** the user selects three cards in group-operations mode
- **THEN** the control row reports three selected

#### Scenario: Hint is shown while nothing is selected
- **WHEN** the user enables group operations and no card is selected
- **THEN** the selection status reads "Click cards to select them"

#### Scenario: Hint returns when the selection empties
- **WHEN** cards are selected and the selection becomes empty (Select none, a bulk action completing, or a view change)
- **THEN** the selection status reads "Click cards to select them" again

---

### Requirement: Selection clears when the visible cards change
The group selection SHALL be cleared whenever the visible set of cards changes through a user view change: changing the search text, checking or unchecking any entry of the Prime Gear Filters menu, or switching the active wishlist. This ensures the selection never references cards that are no longer visible.

#### Scenario: Changing the search clears the selection
- **WHEN** cards are selected and the user changes the search text
- **THEN** the selection is cleared

#### Scenario: Changing a filter clears the selection
- **WHEN** cards are selected and the user checks or unchecks Vaulted, Owned, or Not wishlisted in the Filters menu
- **THEN** the selection is cleared

#### Scenario: Switching the active wishlist clears the selection
- **WHEN** cards are selected and the user switches the active wishlist
- **THEN** the selection is cleared

## MODIFIED Requirements

### Requirement: Bulk mark and unmark owned

In group-operations mode, **Mark owned** SHALL mark every selected card's gear as owned for the active profile, and **Unmark owned** SHALL clear the owned mark on every selected card's gear, each persisting via the owned gear store. When marked owned while the Filters menu hides Owned, the affected cards SHALL fade out and be removed from view using the same transition as single-card marking. After a bulk action completes, the selection SHALL be empty.

#### Scenario: Bulk mark owned marks all selected gear

- **WHEN** several cards are selected and the user invokes Mark owned
- **THEN** every selected gear is marked owned for the active profile and persisted

#### Scenario: Bulk unmark owned clears all selected gear

- **WHEN** several owned cards are selected and the user invokes Unmark owned
- **THEN** every selected gear's owned mark is cleared for the active profile and persisted

#### Scenario: Bulk-marked cards fade out when owned gear is hidden

- **WHEN** Owned is checked in the Filters menu and the user bulk-marks selected cards owned
- **THEN** those cards fade out and are removed from view

#### Scenario: Selection is empty after a bulk action

- **WHEN** a bulk Mark owned or Unmark owned action completes
- **THEN** no cards remain selected

### Requirement: Selection actions are inactive when they have nothing to act on
In group-operations mode, an action SHALL be inactive (no response to clicks, no hover feedback, an inactive look) whenever it has nothing to act on: **Select none** while no card is selected, **Mark owned** while no selected card is unowned, and **Unmark owned** while no selected card is owned. **Select all** SHALL stay active. Each action SHALL re-evaluate whenever the selection changes.

#### Scenario: Actions are inactive on entering group mode
- **WHEN** the user enables group operations and no card is selected
- **THEN** Select none, Mark owned, and Unmark owned are shown inactive and clicking them does nothing, while Select all is active

#### Scenario: Only unowned cards selected
- **WHEN** the selection contains only cards that are not owned
- **THEN** Select none and Mark owned are active and Unmark owned is inactive

#### Scenario: Only owned cards selected
- **WHEN** owned gear is shown and the selection contains only owned cards
- **THEN** Select none and Unmark owned are active and Mark owned is inactive

#### Scenario: Mixed selection
- **WHEN** the selection contains both owned and unowned cards
- **THEN** Select none, Mark owned, and Unmark owned are all active

#### Scenario: Actions deactivate when the selection empties
- **WHEN** cards are selected and the user invokes Select none
- **THEN** Select none, Mark owned, and Unmark owned become inactive again

## REMOVED Requirements

### Requirement: Group-operations control row
**Reason**: Unmark owned is no longer hidden while owned gear is hidden; it stays shown and inactive, so the row's controls never shift.
**Migration**: See "Group-operations action row" and "Selection status in the action row" in this capability.

### Requirement: Selection clears on view changes
**Reason**: Its triggers named the Show vaulted gear and Show owned gear checkboxes, which become entries of the Filters menu.
**Migration**: See "Selection clears when the visible cards change" in this capability.
