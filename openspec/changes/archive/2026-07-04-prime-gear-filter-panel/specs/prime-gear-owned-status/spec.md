## MODIFIED Requirements

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
