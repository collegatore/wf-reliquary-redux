## MODIFIED Requirements

### Requirement: Wishlist Relics tab exists in main navigation
The application SHALL display a tab labeled **"Relics and Missions"** in the main tab bar alongside the existing "Drop Tables" and "Prime Gear" tabs, replacing the former "Wishlist Relics" label. The tab SHALL contain two stacked panels: the relic cards panel on top and the farming missions panel below, separated by a **missions separator panel**. A shared **Relics control panel** appears above the relic cards panel. The Relics control panel SHALL contain a **Relics** group with a plain "Relics:" text label followed by the "Show vaulted" and "Show all rewards" checkboxes. The **missions separator panel** SHALL share the tab's control-bar background tint and SHALL contain a **Missions** group with a plain "Missions:" text label followed by the "Show exact drop chances" checkbox. The missions separator panel SHALL be a compact divider bar (matching the height of the "Vaulted" separator bar on the Prime Gear tab) with its bottom corners squared where it meets the splitter, and SHALL be pinned to the bottom of the relic-cards section so it sits directly above the splitter. A draggable GridSplitter SHALL be placed immediately below the missions separator panel; dragging it SHALL resize the relic-cards scroll area and the farming missions scroll area (not the separator panel, whose height stays fixed). Each panel SHALL scroll independently within its own scrollable area.

#### Scenario: Tab is visible in main window
- **WHEN** the application is open
- **THEN** a tab labeled "Relics and Missions" is visible in the tab bar

#### Scenario: Tab displays both panels when selected
- **WHEN** the user selects the "Relics and Missions" tab
- **THEN** the relic cards panel is visible in the upper half and the farming missions panel is visible in the lower half

#### Scenario: Panels scroll independently
- **WHEN** the user scrolls in the relic cards panel
- **THEN** the farming missions panel position is unaffected, and vice versa

#### Scenario: Relics control panel shows Relics label and checkboxes
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the top control panel shows a "Relics:" text label followed by the "Show vaulted" and "Show all rewards" checkboxes, and does NOT show a "Missions:" label or "Show exact drop chances" checkbox

#### Scenario: Missions separator panel shows Missions label and checkbox
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the missions separator panel shows a "Missions:" text label followed by the "Show exact drop chances" checkbox

#### Scenario: Missions separator panel is a compact divider bar
- **WHEN** the missions separator panel is displayed
- **THEN** it uses the tab's control-bar background tint, is as tall as the "Vaulted" separator bar on the Prime Gear tab, and has squared bottom corners where it meets the splitter

#### Scenario: GridSplitter below separator resizes the card areas
- **WHEN** the user drags the thin strip immediately below the missions separator panel
- **THEN** the relic cards scroll area and the farming missions scroll area resize accordingly, while the separator panel's height stays fixed

---

### Requirement: Relic cards show rewards grouped by tier with wishlisted parts highlighted
Each relic card SHALL have the relic display name as its header, directly above the rewards body with **no** line separator between the header and the body. The card body SHALL group rewards by tier in order Common → Uncommon → Rare.

Each tier group SHALL be preceded by a tier label showing the tier name in title case ("Common", "Uncommon", "Rare") with no trailing colon. The tier label SHALL be rendered in the default text color, in semi-bold weight, uniform across all three tiers, so it reads as a header distinct from the reward names. The tier label SHALL NOT be italic and SHALL NOT be uppercased.

Each tier group SHALL display a vertical accent bar in the card's left gutter spanning the full height of the group (the tier label and all of its reward lines, including wrapped lines). The accent bar color SHALL encode the tier: Common = bronze, Uncommon = slate, Rare = gold. Reward names SHALL be indented beneath the tier label so the rewards read as the label's list; a reward name that wraps SHALL have its continuation lines align to the indented left edge.

Reward names SHALL be rendered verbatim, matching the in-game reward text, and SHALL wrap to multiple lines when too long to fit. Reward names SHALL NOT be truncated, ellipsized, or abbreviated.

Line separators SHALL NOT be rendered between tier groups, nor between the card header and the body; tier grouping is conveyed by the accent bar and the spacing between groups.

Wishlist status SHALL be conveyed by opacity, not font weight. When **Show all rewards** is enabled, any reward whose normalized name matches a wishlisted part SHALL be rendered at full strength, and every non-wishlisted reward SHALL be rendered dimmed (reduced opacity). No reward SHALL use bold weight to indicate wishlist status. When Show all rewards is disabled, only wishlisted rewards are shown and all SHALL be rendered at full strength.

By default, only wishlisted rewards are shown on each card. When Show all rewards is enabled, all rewards (wishlisted and non-wishlisted) are shown.

#### Scenario: Card header shows relic name
- **WHEN** a relic card is displayed
- **THEN** the card header contains the relic's display name (e.g., "Lith A1 Relic") directly above the rewards body with no separator line between them

#### Scenario: Card body groups rewards by tier in order
- **WHEN** a relic card is displayed
- **THEN** rewards are grouped under "Common", "Uncommon", and "Rare" labels in that order

#### Scenario: Tier label is semi-bold and not italic or uppercase
- **WHEN** a tier group is displayed
- **THEN** its tier label is shown in title case with no trailing colon, in semi-bold weight in the default text color, uniform across all tiers, in a non-italic, non-uppercase style that reads as a header above the reward names

#### Scenario: Tier accent bar color encodes the tier
- **WHEN** the Common, Uncommon, and Rare tier groups are displayed
- **THEN** each group shows a left-gutter accent bar colored bronze, slate, and gold respectively

#### Scenario: Reward names are indented beneath the tier label
- **WHEN** a tier group's rewards are displayed
- **THEN** the reward names are indented beneath the tier label; if a name wraps, its continuation lines align to the indented edge

#### Scenario: Long reward names are shown verbatim without truncation
- **WHEN** a reward has a long verbatim name (e.g., "Revenant Prime Neuroptics Blueprint")
- **THEN** the full name is displayed, wrapping as needed, with no ellipsis or abbreviation

#### Scenario: No separators between tier groups or under the header
- **WHEN** a relic card has multiple tier groups
- **THEN** no line separator is rendered between or after the tier groups, nor between the card header and the body; grouping is shown by the accent bars and spacing

#### Scenario: Non-wishlisted rewards are dimmed when Show all rewards is enabled
- **WHEN** Show all rewards is checked
- **THEN** rewards matching the active wishlist render at full strength and all other rewards render dimmed, and no reward is bold

#### Scenario: All shown rewards are full strength when Show all rewards is disabled
- **WHEN** Show all rewards is unchecked
- **THEN** only wishlisted rewards are shown and all of them render at full strength

#### Scenario: Default view shows only wishlisted rewards
- **WHEN** Show all rewards is unchecked (default)
- **THEN** each relic card shows only the reward items that are in the active wishlist
