# Relic View Modes — Delta

## MODIFIED Requirements

### Requirement: Table mode renders one row per relic
In **Table** mode, the relics panel SHALL render a table with one top-level row per relic showing: the clipped relic name, a wishlisted-tier marker listing the tiers that contain at least one wishlisted reward in Common → Uncommon → Rare order without parentheses (e.g. "C, R"), and two pictogram actions (rewards modal and Recruiting Chat message composer). When a tier contains more than one wishlisted reward, its marker SHALL be prefixed with the count (e.g. "C, 2 R" for one wishlisted Common and two wishlisted Rares). Rows SHALL follow the active sort mode and direction (see the `relics-sorting-panel` capability), matching the card modes; with default settings this is era order (Lith → Meso → Neo → Axi) then alphabetical by relic name. The **Show vaulted** filter SHALL apply, and vaulted rows SHALL use the same reduced-opacity treatment as vaulted cards.

#### Scenario: Row shows name, markers, and pictograms
- **WHEN** a relic with wishlisted Common and Rare rewards is displayed in Table mode
- **THEN** its row shows the clipped relic name, the marker "C, R", and the rewards-modal and Recruiting Chat message pictograms

#### Scenario: Marker counts multiple wishlisted rewards in a tier
- **WHEN** a relic with one wishlisted Common and two wishlisted Rare rewards is displayed in Table mode
- **THEN** its row shows the marker "C, 2 R"

#### Scenario: Rows follow the active sort
- **WHEN** the user activates a non-default sort (e.g. Wishlist score descending) while in Table mode
- **THEN** the rows re-order to match the same relic order the card modes would show

#### Scenario: Default sort orders rows by era then name
- **WHEN** Lith and Meso relics are displayed in Table mode with default sort settings
- **THEN** all Lith rows appear before any Meso rows, alphabetically within each era

#### Scenario: Vaulted rows are dimmed and filterable
- **WHEN** Show vaulted is checked and a vaulted relic is displayed in Table mode
- **THEN** its row renders at reduced opacity; unchecking Show vaulted hides it
