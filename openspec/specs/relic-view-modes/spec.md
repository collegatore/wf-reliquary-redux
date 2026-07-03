# relic-view-modes Specification

## Purpose

Defines the three relic view modes available in the Relics and Missions tab: **Verbatim** (original card rendering with full tier groups and verbatim reward names), **Compact** (default; cards with clipped one-line rewards, bold tier letter markers, and no tier label rows), and **Table** (a TreeDataGrid showing one row per relic with wishlisted-tier markers and expandable reward rows). The selected mode is controlled by a three-way selector in the Appearance window, persisted in user settings, and applies immediately to the relics panel as a live preview.

## Requirements

### Requirement: Relic view mode selector
The **Appearance window** SHALL contain a three-way relic view mode selector with the options **Verbatim**, **Compact**, and **Table**, alongside the existing layout orientation control. Selecting a mode SHALL immediately switch the relics panel content in the main window (live preview, per the Appearance window's existing behavior) without requiring a reload, in both the stacked and side-by-side layouts. The default mode SHALL be **Compact**.

#### Scenario: Selector offers three modes
- **WHEN** the Appearance window is open
- **THEN** it shows a relic view mode selector with exactly the options Verbatim, Compact, and Table

#### Scenario: Switching modes updates the relics panel immediately
- **WHEN** the user changes the selected view mode in the Appearance window
- **THEN** the relics panel in the main window re-renders in the newly selected mode without a reload

#### Scenario: Compact is the default mode
- **WHEN** the application runs with no previously saved view mode
- **THEN** the relics panel renders in Compact mode

---

### Requirement: View mode persists across sessions
The selected relic view mode SHALL be persisted in the user settings and restored on application startup.

#### Scenario: Mode is restored on restart
- **WHEN** the user selects Table mode and restarts the application
- **THEN** the relics panel renders in Table mode

#### Scenario: Existing settings files remain valid
- **WHEN** the application loads a settings file saved before this capability existed
- **THEN** settings load successfully and the view mode falls back to Compact

---

### Requirement: Compact mode renders clipped one-line rewards on cards
In **Compact** mode, relics SHALL render as cards that keep the card border, header, width scaling, and vaulted styling of Verbatim cards, but the card body SHALL be a flat list of reward lines with no per-tier label rows and no accent bars. Each reward line SHALL show a leading **bold** tier letter marker — `C` for Common, `U` for Uncommon, `R` for Rare, no parentheses — followed by the clipped reward name. Rewards SHALL be ordered by tier (Common → Uncommon → Rare). The **Show all rewards** toggle and non-wishlisted dimming SHALL behave exactly as in Verbatim mode.

#### Scenario: Compact card has no tier label rows
- **WHEN** a relic card is displayed in Compact mode
- **THEN** no "Common" / "Uncommon" / "Rare" label rows are rendered and each reward occupies one line beginning with its tier letter marker

#### Scenario: Bold tier letter marker encodes the tier
- **WHEN** a Rare reward is displayed in Compact mode
- **THEN** its line begins with a bold "R" and no accent bar is rendered

#### Scenario: Show all rewards dims non-wishlisted rewards in Compact mode
- **WHEN** Show all rewards is checked in Compact mode
- **THEN** non-wishlisted reward lines render dimmed and wishlisted lines render at full strength

---

### Requirement: Compact word clipping
Compact mode (cards and Table rows) SHALL clip display names as a presentation-layer transform, leaving stored/domain names untouched:
- Relic names SHALL drop the trailing word "Relic".
- Reward names SHALL drop the word "Prime".
- Reward names SHALL drop the trailing word "Blueprint" **unless** the original name ends with "Prime Blueprint" (a main blueprint), in which case "Blueprint" SHALL be retained.

#### Scenario: Relic header is clipped
- **WHEN** the relic "Axi A15 Relic" is displayed in Compact or Table mode
- **THEN** its displayed name is "Axi A15"

#### Scenario: Component blueprint name is clipped
- **WHEN** the reward "Revenant Prime Neuroptics Blueprint" is displayed in compact wording
- **THEN** it renders as "Revenant Neuroptics"

#### Scenario: Main blueprint keeps the word Blueprint
- **WHEN** the reward "Revenant Prime Blueprint" is displayed in compact wording
- **THEN** it renders as "Revenant Blueprint"

#### Scenario: Non-Prime trailing Blueprint is clipped
- **WHEN** the reward "Forma Blueprint" is displayed in compact wording
- **THEN** it renders as "Forma"

---

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

---

### Requirement: Table rows expand into reward rows
Each top-level relic row SHALL be expandable into child rows, one per **wishlisted** reward, showing the clipped reward name with its leading **bold** tier letter marker (matching Compact mode's marker style). Non-wishlisted rewards SHALL NOT appear as child rows at all (omitted, not dimmed). Child rows SHALL be ordered by tier (Common → Uncommon → Rare) and SHALL NOT show pictogram actions.

#### Scenario: Expanding a row reveals only wishlisted rewards
- **WHEN** the user expands a relic row in Table mode
- **THEN** one child row appears per wishlisted reward, with its clipped name and leading bold tier letter marker; non-wishlisted rewards do not appear

#### Scenario: Collapsing a row hides its reward rows
- **WHEN** the user collapses an expanded relic row
- **THEN** its child reward rows are removed from view

---

### Requirement: Rewards modal pictogram
Each top-level relic row in Table mode SHALL provide a pictogram action that opens a modal dialog showing that relic rendered as a **Verbatim** card in all-rewards mode (verbatim names, tier groups). Non-wishlisted rewards SHALL render dimmed exactly as on the main-panel Verbatim card; wishlisted rewards SHALL receive no additional highlight. The dialog SHALL be closable only via the window's native close control (no in-content Close button), returning the user to the table unchanged.

#### Scenario: Pictogram opens the Verbatim card modal
- **WHEN** the user activates the rewards pictogram on a relic row
- **THEN** a modal dialog opens showing that relic's Verbatim card with all rewards; non-wishlisted rewards are dimmed and wishlisted rewards render at full strength with no background highlight

#### Scenario: Closing the modal preserves table state
- **WHEN** the user closes the rewards modal via the window's native close control
- **THEN** the table is unchanged, including row expansion states

---

### Requirement: Recruiting Chat message pictogram
Each top-level relic row in Table mode SHALL provide a pictogram action that opens the existing Recruiting Chat message composer for that relic, equivalent to the "Compose Recruiting Chat message…" action on relic cards.

#### Scenario: Pictogram opens the Recruiting Chat message composer
- **WHEN** the user activates the Recruiting Chat message pictogram on a relic row
- **THEN** the Recruiting Chat message composer dialog opens for that relic
