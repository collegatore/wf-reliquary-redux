## ADDED Requirements

### Requirement: Enable group operations toggle

The Prime Gear control panel SHALL include an **Enable group operations** toggle. The toggle SHALL be ephemeral — it SHALL default to off each time the application starts and SHALL NOT be persisted across sessions. While the toggle is off, the tab SHALL behave exactly as before (per-card overflow menus, no second control row). Turning the toggle off SHALL clear any current group selection.

#### Scenario: Toggle present and off at launch

- **WHEN** the application launches and the user navigates to the Prime Gear tab
- **THEN** the control panel shows an "Enable group operations" toggle in the off state

#### Scenario: Toggle is not persisted

- **WHEN** the user enables group operations and then restarts the application
- **THEN** group operations is off again after restart

#### Scenario: Turning off group operations clears the selection

- **WHEN** one or more cards are selected and the user turns off the Enable group operations toggle
- **THEN** the selection is cleared

### Requirement: Click-to-select cards in group-operations mode

While group operations is enabled, each Prime gear card SHALL become a single selection target: clicking anywhere on the card SHALL toggle its selection, and the card's own interactive controls (the per-part checkboxes and the overflow-menu button) SHALL be suppressed so they do not respond while group operations is enabled. A selected card SHALL be indicated by a subtle background wash, distinct from the owned and vaulted treatments. Selecting a card SHALL NOT change its owned state. While group operations is disabled, cards SHALL behave normally — the overflow menu and part checkboxes SHALL be interactive and clicking the card body SHALL NOT select it.

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

### Requirement: Group-operations control row

While group operations is enabled, the Prime Gear control panel SHALL display a second row containing a **selection count**, a **Select all** action, a **Select none** action, a **Mark owned** action, and an **Unmark owned** action. This row SHALL be hidden while group operations is disabled. The selection count SHALL reflect the number of currently selected cards.

#### Scenario: Second row appears only in group-operations mode

- **WHEN** group operations is enabled
- **THEN** the control panel shows a second row with Select all, Select none, Mark owned, and Unmark owned actions and a selection count

#### Scenario: Second row hidden when group operations is off

- **WHEN** group operations is disabled
- **THEN** the second control row is not shown

#### Scenario: Selection count reflects selected cards

- **WHEN** the user selects three cards in group-operations mode
- **THEN** the control row reports three selected

### Requirement: Select all and Select none

In group-operations mode, **Select all** SHALL select every currently visible gear card across both the Active and Vaulted sections; cards hidden by the current filters (owned, vaulted, or search) SHALL NOT be selected. **Select none** SHALL clear the entire selection.

#### Scenario: Select all selects every visible card

- **WHEN** the user invokes Select all in group-operations mode
- **THEN** every card currently visible under the active filters becomes selected

#### Scenario: Select all does not select hidden cards

- **WHEN** some cards are hidden by the current filters and the user invokes Select all
- **THEN** only the visible cards are selected and the hidden cards remain unselected

#### Scenario: Select none clears the selection

- **WHEN** one or more cards are selected and the user invokes Select none
- **THEN** no cards are selected

### Requirement: Bulk mark and unmark owned

In group-operations mode, **Mark owned** SHALL mark every selected card's gear as owned for the active profile, and **Unmark owned** SHALL clear the owned mark on every selected card's gear, each persisting via the owned gear store. When marked owned while Show owned gear is unchecked, the affected cards SHALL fade out and be removed from view using the same transition as single-card marking. After a bulk action completes, the selection SHALL be empty.

#### Scenario: Bulk mark owned marks all selected gear

- **WHEN** several cards are selected and the user invokes Mark owned
- **THEN** every selected gear is marked owned for the active profile and persisted

#### Scenario: Bulk unmark owned clears all selected gear

- **WHEN** several owned cards are selected and the user invokes Unmark owned
- **THEN** every selected gear's owned mark is cleared for the active profile and persisted

#### Scenario: Bulk-marked cards fade out when owned gear is hidden

- **WHEN** Show owned gear is unchecked and the user bulk-marks selected cards owned
- **THEN** those cards fade out and are removed from view

#### Scenario: Selection is empty after a bulk action

- **WHEN** a bulk Mark owned or Unmark owned action completes
- **THEN** no cards remain selected

### Requirement: Single batched prompt for tracked parts on bulk mark

When a bulk **Mark owned** is invoked and at least one selected card has parts in the active wishlist (wished parts), the application SHALL present the tracking-choice dialog **exactly once** and apply the chosen outcome to the entire batch. The choices SHALL be **Keep tracking** (mark all selected owned, leave wished parts in the active wishlist), **Release tracking** (remove every selected gear's wished parts from the active wishlist, then mark all selected owned), and **Cancel** (mark none of the selected gear). When no selected card has wished parts, no dialog SHALL be shown and all selected gear SHALL be marked owned directly.

#### Scenario: Single dialog when any selected card has wished parts

- **WHEN** the user invokes bulk Mark owned and at least one selected card has wished parts
- **THEN** the tracking-choice dialog is shown once before any owned mark is persisted

#### Scenario: No dialog when no selected card has wished parts

- **WHEN** the user invokes bulk Mark owned and none of the selected cards have wished parts
- **THEN** no dialog is shown and all selected gear is marked owned directly

#### Scenario: Keep tracking applies to the whole batch

- **WHEN** the batched dialog is shown and the user chooses Keep tracking
- **THEN** all selected gear is marked owned and their wished parts remain in the active wishlist

#### Scenario: Release tracking applies to the whole batch

- **WHEN** the batched dialog is shown and the user chooses Release tracking
- **THEN** all selected gear is marked owned and every selected gear's wished parts are removed from the active wishlist

#### Scenario: Cancel aborts the entire bulk mark

- **WHEN** the batched dialog is shown and the user chooses Cancel
- **THEN** no selected gear is marked owned and the selection is left unchanged

### Requirement: Selection clears on view changes

The group selection SHALL be cleared whenever the visible set of cards changes through a user view change: changing the search text, toggling Show vaulted gear, toggling Show owned gear, or switching the active wishlist. This ensures the selection never references cards that are no longer visible.

#### Scenario: Changing the search clears the selection

- **WHEN** cards are selected and the user changes the search text
- **THEN** the selection is cleared

#### Scenario: Toggling Show vaulted clears the selection

- **WHEN** cards are selected and the user toggles Show vaulted gear
- **THEN** the selection is cleared

#### Scenario: Toggling Show owned clears the selection

- **WHEN** cards are selected and the user toggles Show owned gear
- **THEN** the selection is cleared

#### Scenario: Switching the active wishlist clears the selection

- **WHEN** cards are selected and the user switches the active wishlist
- **THEN** the selection is cleared
