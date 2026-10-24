# Spec Delta

## MODIFIED Requirements

### Requirement: Relic view mode selector
The **Display menu** on the Relics bar (see the `relics-display-menu` capability) SHALL offer a three-way relic view mode choice with the options **Verbatim**, **Compact**, and **Table**. Selecting a mode SHALL immediately switch the relics panel content without requiring a reload, in both the stacked and side-by-side layouts. The Appearance window SHALL NOT offer the view mode. The default mode SHALL be **Compact**.

#### Scenario: Selector offers three modes
- **WHEN** the user opens the Display menu on the Relics bar
- **THEN** it offers exactly the view modes Verbatim, Compact, and Table

#### Scenario: Switching modes updates the relics panel immediately
- **WHEN** the user chooses a different view mode in the Display menu
- **THEN** the relics panel re-renders in the newly selected mode without a reload

#### Scenario: Compact is the default mode
- **WHEN** the application runs with no previously saved view mode
- **THEN** the relics panel renders in Compact mode

### Requirement: Table mode renders one row per relic
In **Table** mode, the relics panel SHALL render a table with one top-level row per relic showing: the clipped relic name, a wishlisted-tier marker listing the tiers that contain at least one wishlisted reward in Common → Uncommon → Rare order without parentheses (e.g. "C, R"), and two pictogram actions (rewards modal and Recruiting Chat message composer). When a tier contains more than one wishlisted reward, its marker SHALL be prefixed with the count (e.g. "C, 2 R" for one wishlisted Common and two wishlisted Rares). Rows SHALL follow the active sort mode and direction (see the `relics-sorting-panel` capability), matching the card modes; with default settings this is era order (Axi → Neo → Meso → Lith) then alphabetical by relic name. The **Vaulted** filter SHALL apply, and vaulted rows SHALL use the same reduced-opacity treatment as vaulted cards.

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
- **THEN** all Meso rows appear before any Lith rows, alphabetically within each era

#### Scenario: Vaulted rows are dimmed and filterable
- **WHEN** a vaulted relic is displayed in Table mode with nothing hidden by the Filters menu
- **THEN** its row renders at reduced opacity; checking Vaulted in the Filters menu hides it
