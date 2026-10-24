# Spec Delta

## ADDED Requirements

### Requirement: Owned gear shown with an owned treatment
Owned gear cards SHALL be shown by default in both the Active and Vaulted sections, rendered with a distinct **owned** treatment: muted (reduced opacity), outlined in an unintrusive green, and bearing a **✓ marker** immediately after the gear name. The owned treatment SHALL be independent of vaulted status, so an owned gear that is also vaulted SHALL still show the green outline and ✓ marker.

#### Scenario: Owned cards are shown by default
- **WHEN** the application starts with no saved filters and the user navigates to the Prime Gear tab
- **THEN** cards for owned gear are visible with the owned treatment

#### Scenario: Owned card shows the owned treatment
- **WHEN** an owned gear card is displayed
- **THEN** that card is rendered muted, outlined in green, and shows a ✓ marker immediately after the gear name

#### Scenario: Owned-and-vaulted card shows the owned treatment
- **WHEN** an owned gear that is also vaulted is displayed
- **THEN** that card shows the green outline and ✓ marker, distinguishing it from a vaulted gear that is not owned

---

### Requirement: Owned gear can be hidden from the Filters menu
Checking **Owned** in the Prime Gear Filters menu (see `prime-gear-filter-panel`) SHALL hide owned gear cards in both the Active and Vaulted sections; unchecking it SHALL show them again. While Owned is checked, marking a gear owned SHALL remove its card from view and unmarking it SHALL restore it. A card removed from view because it was just marked owned SHALL fade out before disappearing, so the removal reads as a smooth transition rather than an abrupt jump.

#### Scenario: Hide Owned hides owned cards
- **WHEN** the user checks Owned in the Prime Gear Filters menu
- **THEN** cards for owned gear are hidden

#### Scenario: Unchecking Owned shows owned cards again
- **WHEN** Owned is checked in the Filters menu and the user unchecks it
- **THEN** cards for owned gear are visible again with the owned treatment

#### Scenario: Marking gear owned removes it from view when owned cards are hidden
- **WHEN** Owned is checked in the Filters menu and the user marks a visible gear card as owned
- **THEN** that gear's card fades out and is then removed from view

#### Scenario: Owned filtering applies to both Active and Vaulted sections
- **WHEN** Owned is checked in the Filters menu and an owned gear is vaulted
- **THEN** that gear's card is hidden from the Vaulted section just as an owned active gear is hidden from the Active section

## REMOVED Requirements

### Requirement: Owned gear hidden by default with Show owned gear toggle
**Reason**: Owned gear is now shown by default; the Show owned gear checkbox becomes the "Hide Owned" entry of the Filters menu.
**Migration**: See "Owned gear shown with an owned treatment" and "Owned gear can be hidden from the Filters menu" in this capability.
