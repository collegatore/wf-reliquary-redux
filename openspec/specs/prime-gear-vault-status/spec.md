# Prime Gear Vault Status Specification

## Requirements

### Requirement: Prime gear vault status in query result

The prime gear query SHALL include an `IsVaulted` flag on each gear entry. A gear SHALL be considered vaulted when no relic that rewards any of its parts can be obtained from a mission. A relic is obtainable from a mission when at least one row in `mission_rewards` references it via `relic_id`.

#### Scenario: Gear whose relics are all in mission_rewards is not vaulted

- **WHEN** the prime gear query is executed and at least one relic that rewards a part of "Afuris Prime" appears in `mission_rewards`
- **THEN** the returned entry for "Afuris Prime" has `IsVaulted = false`

#### Scenario: Gear with no relics in mission_rewards is vaulted

- **WHEN** the prime gear query is executed and no relic that rewards any part of "Afuris Prime" appears in `mission_rewards`
- **THEN** the returned entry for "Afuris Prime" has `IsVaulted = true`

#### Scenario: Gear is not vaulted when any one of its relics is obtainable

- **WHEN** "Afuris Prime" has two relics in `relic_rewards` and exactly one of them appears in `mission_rewards`
- **THEN** the returned entry for "Afuris Prime" has `IsVaulted = false`

#### Scenario: Query returns empty result when no data imported

- **WHEN** the prime gear query is executed against a database with no imported reward items
- **THEN** it returns an empty list without error

### Requirement: Vaulted prime gear card visual indicator

Each prime gear card that represents a vaulted gear entry SHALL display a "Vaulted" badge adjacent to the gear name. Vaulted status SHALL be conveyed by the card's placement in the dedicated **Vaulted** section beneath the Vaulted section header. Vaulted cards SHALL NOT be rendered with reduced opacity solely on account of being vaulted; muting (reduced opacity) is reserved for the **owned** treatment so that owned and vaulted statuses are visually distinct and can coexist on the same card.

The **Show vaulted** toggle that controls visibility of vaulted gear SHALL be located in the collapsible filter panel (see `prime-gear-filter-panel`), not in the primary control panel.

#### Scenario: Vaulted gear card shows badge

- **WHEN** the prime gear cards view is displayed and a gear entry has `IsVaulted = true`
- **THEN** that gear's card shows a "Vaulted" badge

#### Scenario: Active gear card has no vaulted badge

- **WHEN** the prime gear cards view is displayed and a gear entry has `IsVaulted = false`
- **THEN** that gear's card does not show a "Vaulted" badge

#### Scenario: Vaulted card is not muted by vaulted status alone

- **WHEN** the prime gear cards view is displayed and a vaulted gear entry is not owned
- **THEN** that gear's card is rendered at full opacity, distinguished as vaulted by its placement in the Vaulted section rather than by reduced opacity

#### Scenario: Show vaulted toggle is in the filter panel

- **WHEN** the filter panel is open
- **THEN** the Show vaulted checkbox is visible inside the filter panel and not in the primary control panel
