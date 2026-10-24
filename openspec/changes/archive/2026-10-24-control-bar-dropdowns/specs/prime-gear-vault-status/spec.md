# Spec Delta

## ADDED Requirements

### Requirement: Vaulted gear cards are marked by badge and section
Each prime gear card that represents a vaulted gear entry SHALL display a "Vaulted" badge adjacent to the gear name and sit in the dedicated **Vaulted** section beneath the Vaulted section header. Vaulted cards SHALL NOT be muted on account of being vaulted; reduced opacity is reserved for the **owned** treatment, so the two statuses stay distinct and can coexist on one card. Vaulted gear is hidden from the Filters menu (see `prime-gear-filter-panel`).

#### Scenario: Vaulted gear card shows badge
- **WHEN** the prime gear cards view is displayed and a gear entry has `IsVaulted = true`
- **THEN** that gear's card shows a "Vaulted" badge

#### Scenario: Active gear card has no vaulted badge
- **WHEN** the prime gear cards view is displayed and a gear entry has `IsVaulted = false`
- **THEN** that gear's card does not show a "Vaulted" badge

#### Scenario: Vaulted card is not muted by vaulted status alone
- **WHEN** the prime gear cards view is displayed and a vaulted gear entry is not owned
- **THEN** that gear's card is rendered at full opacity, distinguished as vaulted by its placement in the Vaulted section rather than by reduced opacity

## REMOVED Requirements

### Requirement: Vaulted prime gear card visual indicator
**Reason**: It placed the Show vaulted toggle in the filter panel, which is replaced by the Hide Vaulted entry of the Filters menu.
**Migration**: See "Vaulted gear cards are marked by badge and section" in this capability, and `prime-gear-filter-panel` for hiding vaulted gear.
