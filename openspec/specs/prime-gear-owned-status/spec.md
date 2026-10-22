# prime-gear-owned-status Specification

## Purpose

Track which Prime gear a player already owns, scoped per profile, so owned gear can be marked from each gear card, optionally release its wished parts from tracking, and be hidden from view by default to keep the Prime Gear tab focused on gear still being chased.

## Requirements

### Requirement: Profile-scoped owned gear store

The application SHALL expose a profile-scoped store for Prime gear ownership. Like the wishlist store, it SHALL resolve the active profile internally (from the persisted last-profile preference) and operate within it without callers passing a profile id. The store SHALL expose operations to return the set of owned gear names for the active profile, mark a gear name as owned, and clear an owned mark. Owned gear SHALL be recorded by gear name as a soft reference with no foreign key to import tables, so owned marks survive drop-table re-imports.

#### Scenario: Marking gear as owned persists for the active profile

- **WHEN** a gear name is marked as owned for the active profile
- **THEN** a subsequent query of owned gear for that profile includes that gear name

#### Scenario: Clearing an owned mark removes it

- **WHEN** a gear name that was previously owned is cleared for the active profile
- **THEN** a subsequent query of owned gear for that profile does not include that gear name

#### Scenario: Owned marks are isolated per profile

- **WHEN** a gear name is marked owned in profile A and the active profile is then switched to profile B
- **THEN** the owned gear query for profile B does not include that gear name

#### Scenario: Marking owned is idempotent

- **WHEN** a gear name that is already owned is marked owned again for the active profile
- **THEN** the operation succeeds without error and the gear name appears exactly once in the owned set

#### Scenario: Owned marks survive drop-table re-import

- **WHEN** a gear name is marked owned and a subsequent drop-table import replaces the import tables
- **THEN** the owned gear query for that profile still includes that gear name

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

### Requirement: Confirm tracking when marking owned gear that has wished parts

When the user marks a gear as owned and that gear has one or more parts in the active wishlist (wished parts), the application SHALL present a modal dialog offering a three-way choice before the owned mark takes effect. The choices SHALL be: **Keep tracking** (mark owned, leave the wished parts in the active wishlist); **Release tracking** (mark owned and remove that gear's wished parts from the active wishlist); and **Cancel** (do not mark owned — it was a misclick). The dialog SHALL only appear on the transition to owned and only when wished parts exist; marking owned on a gear with no wished parts, and clearing an owned mark, SHALL never show the dialog.

#### Scenario: Dialog appears when marking owned gear that has wished parts

- **WHEN** the user marks a gear card owned and the gear has at least one part in the active wishlist
- **THEN** a modal dialog is shown offering the three choices before the owned mark is persisted

#### Scenario: No dialog when the gear has no wished parts

- **WHEN** the user marks a gear card owned and the gear has no parts in the active wishlist
- **THEN** no dialog is shown and the gear is marked owned directly

#### Scenario: No dialog when clearing an owned mark

- **WHEN** the user unmarks an owned gear card
- **THEN** no dialog is shown and the owned mark is cleared directly

#### Scenario: Keep tracking

- **WHEN** the dialog is shown and the user chooses **Keep tracking**
- **THEN** the gear is marked owned and its parts remain in the active wishlist

#### Scenario: Release tracking

- **WHEN** the dialog is shown and the user chooses **Release tracking**
- **THEN** the gear is marked owned and all of that gear's parts are removed from the active wishlist

#### Scenario: Cancel reverts the owned mark

- **WHEN** the dialog is shown and the user chooses **Cancel** (or dismisses the dialog)
- **THEN** the gear is not marked owned and the card's menu entry returns to "Mark as owned"

### Requirement: Owned gear hidden by default with Show owned gear toggle

Owned gear cards SHALL be hidden by default across both the Active and Vaulted sections. A **Show owned gear** checkbox SHALL appear in the collapsible filter panel (see `prime-gear-filter-panel`). When Show owned gear is checked, owned cards SHALL become visible and SHALL be rendered with a distinct **owned** treatment: muted (reduced opacity), outlined in an unintrusive green, and bearing a **✓ marker** immediately after the gear name. This owned treatment SHALL be independent of vaulted status, so an owned gear that is also vaulted SHALL still show the green outline and ✓ marker. Marking a gear owned while Show owned gear is unchecked SHALL remove it from view; unmarking it SHALL restore it. When a card is removed from view because it was just marked owned, it SHALL fade out before disappearing so the removal reads as a smooth transition rather than an abrupt jump.

#### Scenario: Owned cards are hidden by default

- **WHEN** the application launches and the user navigates to the Prime Gear tab
- **THEN** cards for owned gear are not visible

#### Scenario: Show owned gear checkbox is in the filter panel

- **WHEN** the filter panel is open on the Prime Gear tab
- **THEN** the Show owned gear checkbox appears inside the filter panel

#### Scenario: Show owned gear reveals owned cards

- **WHEN** the user opens the filter panel and checks the Show owned gear checkbox
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
