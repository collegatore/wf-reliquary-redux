# Spec Delta

## MODIFIED Requirements

### Requirement: Owned overflow menu on gear card header

Each Prime gear card SHALL display an overflow-menu button — a vertical three-dot ("⋮") affordance — on the right side of the card header, aligned opposite the gear name. Activating the button SHALL open a menu containing a single entry whose label reflects the action it will perform: **Mark as owned** when the gear is not owned, and **Unmark as owned** when it is. Invoking the entry SHALL toggle the gear's owned state for the active profile and immediately persist the change to the owned gear store. The menu SHALL open offset toward the gear name rather than centered directly under the button. A floating hint SHALL be available on the button conveying the action it performs. **While group operations is enabled, the overflow-menu button SHALL be replaced by the card's selection checkbox** (see the prime-gear-group-operations capability), so per-card owned actions are performed through the group-operations control row instead; the button is restored when group operations is disabled.

#### Scenario: Overflow-menu button appears on the right of the header

- **WHEN** a prime gear card is displayed and group operations is disabled
- **THEN** a three-dot overflow-menu button is shown on the right side of the card header, opposite the gear name

#### Scenario: Overflow-menu button is suppressed in group mode

- **WHEN** a prime gear card is displayed and group operations is enabled
- **THEN** the overflow-menu button is not shown; the card's selection checkbox occupies its place in the header

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
