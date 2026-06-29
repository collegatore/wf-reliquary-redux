## MODIFIED Requirements

### Requirement: Owned overflow menu on gear card header

Each Prime gear card SHALL display an overflow-menu button — a vertical three-dot ("⋮") affordance — on the right side of the card header, aligned opposite the gear name. Activating the button SHALL open a menu containing a single entry whose label reflects the action it will perform: **Mark as owned** when the gear is not owned, and **Unmark as owned** when it is. Invoking the entry SHALL toggle the gear's owned state for the active profile and immediately persist the change to the owned gear store. The menu SHALL open offset toward the gear name rather than centered directly under the button. A floating hint SHALL be available on the button conveying the action it performs. **While group operations is enabled, the overflow-menu button SHALL be suppressed** along with the card's other controls (see the prime-gear-group-operations capability), so per-card owned actions are performed through the group-operations control row instead; the button is restored when group operations is disabled.

#### Scenario: Overflow-menu button appears on the right of the header

- **WHEN** a prime gear card is displayed and group operations is disabled
- **THEN** a three-dot overflow-menu button is shown on the right side of the card header, opposite the gear name

#### Scenario: Overflow-menu button is suppressed in group mode

- **WHEN** a prime gear card is displayed and group operations is enabled
- **THEN** the overflow-menu button does not respond to clicks (the card body is the selection target instead)

#### Scenario: Menu entry reads "Mark as owned" for unowned gear

- **WHEN** the overflow menu is opened on a gear card that is not owned in the active profile
- **THEN** the menu shows a single entry labelled "Mark as owned"

#### Scenario: Menu entry reads "Unmark as owned" for owned gear

- **WHEN** the overflow menu is opened on a gear card that is owned in the active profile
- **THEN** the menu shows a single entry labelled "Unmark as owned"

#### Scenario: Invoking "Mark as owned" marks the gear owned

- **WHEN** the user invokes the "Mark as owned" entry on a gear card
- **THEN** the gear is marked owned for the active profile

#### Scenario: Invoking "Unmark as owned" clears the owned mark

- **WHEN** the user invokes the "Unmark as owned" entry on an owned gear card
- **THEN** the owned mark is cleared for the active profile

#### Scenario: Owned state persists across sessions

- **WHEN** the user restarts the application after marking gear as owned
- **THEN** the previously owned gear's menu entry reads "Unmark as owned", reflecting the persisted state

#### Scenario: Floating hint explains the overflow button

- **WHEN** the user hovers the overflow-menu button on a gear card
- **THEN** a floating hint is shown indicating the owned action it performs

### Requirement: Owned gear hidden by default with Show owned gear toggle

Owned gear cards SHALL be hidden by default across both the Active and Vaulted sections. A **Show owned gear** checkbox SHALL appear in the Prime Gear control panel alongside the existing Show vaulted gear checkbox. When Show owned gear is checked, owned cards SHALL become visible and SHALL be rendered with a distinct **owned** treatment: muted (reduced opacity), outlined in an unintrusive green, and bearing a **✓ marker** immediately after the gear name. This owned treatment SHALL be independent of vaulted status, so an owned gear that is also vaulted SHALL still show the green outline and ✓ marker. Marking a gear owned while Show owned gear is unchecked SHALL remove it from view; unmarking it SHALL restore it. When a card is removed from view because it was just marked owned, it SHALL fade out before disappearing so the removal reads as a smooth transition rather than an abrupt jump.

#### Scenario: Owned cards are hidden by default

- **WHEN** the application launches and the user navigates to the Prime Gear tab
- **THEN** cards for owned gear are not visible

#### Scenario: Show owned gear checkbox is in the control panel

- **WHEN** the Prime Gear tab is displayed
- **THEN** the Show owned gear checkbox appears in the same control panel as the Show vaulted gear checkbox

#### Scenario: Show owned gear reveals owned cards

- **WHEN** the user checks the Show owned gear checkbox
- **THEN** cards for owned gear become visible

#### Scenario: Show owned gear hides owned cards again

- **WHEN** the Show owned gear checkbox is checked and the user unchecks it
- **THEN** cards for owned gear are hidden again

#### Scenario: Revealed owned cards show the owned treatment

- **WHEN** Show owned gear is checked and an owned gear card is displayed
- **THEN** that card is rendered muted, outlined in green, and shows a ✓ marker immediately after the gear name

#### Scenario: Owned-and-vaulted card shows the owned treatment

- **WHEN** Show owned gear is checked and an owned gear that is also vaulted is displayed
- **THEN** that card shows the green outline and ✓ marker, distinguishing it from a vaulted gear that is not owned

#### Scenario: Marking gear owned removes it from view when owned cards are hidden

- **WHEN** Show owned gear is unchecked and the user marks a visible gear card as owned
- **THEN** that gear's card fades out and is then removed from view

#### Scenario: Owned filtering applies to both Active and Vaulted sections

- **WHEN** Show owned gear is unchecked and an owned gear is vaulted
- **THEN** that gear's card is hidden from the Vaulted section just as an owned active gear is hidden from the Active section
