## MODIFIED Requirements

### Requirement: Vaulted prime gear card visual indicator

Each prime gear card that represents a vaulted gear entry SHALL display a "Vaulted" badge adjacent to the gear name. Vaulted status SHALL be conveyed by the card's placement in the dedicated **Vaulted** section beneath the Vaulted section header. Vaulted cards SHALL NOT be rendered with reduced opacity solely on account of being vaulted; muting (reduced opacity) is reserved for the **owned** treatment so that owned and vaulted statuses are visually distinct and can coexist on the same card.

#### Scenario: Vaulted gear card shows badge

- **WHEN** the prime gear cards view is displayed and a gear entry has `IsVaulted = true`
- **THEN** that gear's card shows a "Vaulted" badge

#### Scenario: Active gear card has no vaulted badge

- **WHEN** the prime gear cards view is displayed and a gear entry has `IsVaulted = false`
- **THEN** that gear's card does not show a "Vaulted" badge

#### Scenario: Vaulted card is not muted by vaulted status alone

- **WHEN** the prime gear cards view is displayed and a vaulted gear entry is not owned
- **THEN** that gear's card is rendered at full opacity, distinguished as vaulted by its placement in the Vaulted section rather than by reduced opacity
