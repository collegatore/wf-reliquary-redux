## Requirements

### Requirement: Wishlist Relics tab exists in main navigation
The application SHALL display a tab labeled **"Relics and Missions"** in the main tab bar alongside the existing "Drop Tables" and "Prime Gear" tabs, replacing the former "Wishlist Relics" label. The tab SHALL contain two stacked panels: the relic cards panel on top and the farming missions panel below, separated by a divider. A shared control panel appears above the relic cards panel. The control panel SHALL be divided into two labelled groups: a **Relics** group containing the "Show vaulted" and "Show all rewards" checkboxes, and a **Missions** group containing the "Show exact drop chances" checkbox. The Relics group SHALL be preceded by a plain "Relics:" text label. Each panel SHALL scroll independently within its own scrollable area; the two panels share the available tab height equally.

#### Scenario: Tab is visible in main window
- **WHEN** the application is open
- **THEN** a tab labeled "Relics and Missions" is visible in the tab bar

#### Scenario: Tab displays both panels when selected
- **WHEN** the user selects the "Relics and Missions" tab
- **THEN** the relic cards panel is visible in the upper half and the farming missions panel is visible in the lower half

#### Scenario: Panels scroll independently
- **WHEN** the user scrolls in the relic cards panel
- **THEN** the farming missions panel position is unaffected, and vice versa

#### Scenario: Control panel shows Relics label
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the control panel shows a "Relics:" text label before the Relics-group checkboxes

#### Scenario: Control panel shows Missions label
- **WHEN** the "Relics and Missions" tab is displayed
- **THEN** the control panel shows a "Missions:" text label before the Missions-group checkboxes

---

### Requirement: Relic cards show rewards grouped by tier with wishlisted parts highlighted
Each relic card SHALL have the relic display name as its header, separated from the body by a single line separator. The card body SHALL group rewards by tier in order Common → Uncommon → Rare.

Each tier group SHALL be preceded by a tier label showing the tier name in title case ("Common", "Uncommon", "Rare") with no trailing colon. The tier label SHALL be rendered in the default text color, in semi-bold weight, uniform across all three tiers, so it reads as a header distinct from the reward names. The tier label SHALL NOT be italic and SHALL NOT be uppercased.

Each tier group SHALL display a vertical accent bar in the card's left gutter spanning the full height of the group (the tier label and all of its reward lines, including wrapped lines). The accent bar color SHALL encode the tier: Common = bronze, Uncommon = slate, Rare = gold. Reward names SHALL be indented beneath the tier label so the rewards read as the label's list; a reward name that wraps SHALL have its continuation lines align to the indented left edge.

Reward names SHALL be rendered verbatim, matching the in-game reward text, and SHALL wrap to multiple lines when too long to fit. Reward names SHALL NOT be truncated, ellipsized, or abbreviated.

Line separators SHALL NOT be rendered between tier groups; tier grouping is conveyed by the accent bar and the spacing between groups. (The single separator between the card header and body is retained.)

Wishlist status SHALL be conveyed by opacity, not font weight. When **Show all rewards** is enabled, any reward whose normalized name matches a wishlisted part SHALL be rendered at full strength, and every non-wishlisted reward SHALL be rendered dimmed (reduced opacity). No reward SHALL use bold weight to indicate wishlist status. When Show all rewards is disabled, only wishlisted rewards are shown and all SHALL be rendered at full strength.

By default, only wishlisted rewards are shown on each card. When Show all rewards is enabled, all rewards (wishlisted and non-wishlisted) are shown.

#### Scenario: Card header shows relic name
- **WHEN** a relic card is displayed
- **THEN** the card header contains the relic's display name (e.g., "Lith A1 Relic") separated from the rewards body by a single line separator

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

#### Scenario: No separators between tier groups
- **WHEN** a relic card has multiple tier groups
- **THEN** no line separator is rendered between or after the tier groups; grouping is shown by the accent bars and spacing

#### Scenario: Non-wishlisted rewards are dimmed when Show all rewards is enabled
- **WHEN** Show all rewards is checked
- **THEN** rewards matching the active wishlist render at full strength and all other rewards render dimmed, and no reward is bold

#### Scenario: All shown rewards are full strength when Show all rewards is disabled
- **WHEN** Show all rewards is unchecked
- **THEN** only wishlisted rewards are shown and all of them render at full strength

#### Scenario: Default view shows only wishlisted rewards
- **WHEN** Show all rewards is unchecked (default)
- **THEN** each relic card shows only the reward items that are in the active wishlist

---

### Requirement: Show all rewards toggle
The Wishlist Relics tab SHALL provide a **Show all rewards** checkbox in the control panel. The checkbox SHALL be unchecked by default. Toggling it SHALL immediately update all visible relic cards.

#### Scenario: Show all rewards is unchecked by default
- **WHEN** the application opens and the user navigates to the Wishlist Relics tab
- **THEN** the Show all rewards checkbox is unchecked and cards show only wishlisted rewards

#### Scenario: Checking Show all rewards updates cards immediately
- **WHEN** the user checks Show all rewards
- **THEN** all relic cards expand to show their full reward lists without requiring a reload

#### Scenario: Unchecking Show all rewards collapses cards immediately
- **WHEN** the user unchecks Show all rewards
- **THEN** all relic cards return to showing only wishlisted rewards without requiring a reload

---

### Requirement: Only relics containing at least one wishlisted part are shown
The view SHALL only display relics that contain at least one reward item matching a part in the active wishlist.

#### Scenario: Relic with no wishlisted rewards is excluded
- **WHEN** a relic has no reward items that match any part in the active wishlist
- **THEN** that relic is not shown in the Wishlist Relics tab

#### Scenario: Empty wishlist shows placeholder
- **WHEN** the active wishlist contains no items
- **THEN** a placeholder message is displayed instead of relic cards

---

### Requirement: Cards are ordered by era then alphabetically by name
Relic cards SHALL be presented in era order (Lith → Meso → Neo → Axi) and within each era in ascending alphabetical order by relic display name. There are no era section headers; cards flow in a single flat wrap layout.

#### Scenario: Lith relics appear before Meso relics
- **WHEN** both Lith and Meso relics are shown
- **THEN** all Lith relic cards appear before any Meso relic cards

#### Scenario: Cards within an era are alphabetical
- **WHEN** multiple relics from the same era are shown
- **THEN** they appear in ascending alphabetical order by display name

---

### Requirement: Vaulted relics can be shown or hidden via a checkbox
Above the relic cards there SHALL be a control panel containing a checkbox labeled **"Show vaulted"** (within the Relics group). By default the checkbox is unchecked (vaulted relics hidden). Checking it reveals vaulted relics.

A relic is considered vaulted if it does not appear as a reward in any active mission.

#### Scenario: Vaulted relics hidden by default
- **WHEN** the Wishlist Relics tab is first displayed
- **THEN** relics that are vaulted are not shown

#### Scenario: Checking show-vaulted reveals vaulted relics
- **WHEN** the user checks "Show vaulted"
- **THEN** vaulted relics that contain wishlisted parts become visible

#### Scenario: Unchecking show-vaulted hides vaulted relics again
- **WHEN** the user unchecks "Show vaulted" after they were shown
- **THEN** vaulted relics are hidden again

---

### Requirement: Vaulted relics are visually marked the same as vaulted prime gear
Vaulted relic cards SHALL use the same reduced-opacity visual treatment applied to vaulted prime gear cards.

#### Scenario: Vaulted relic card has reduced opacity
- **WHEN** a vaulted relic is visible (show-vaulted is checked)
- **THEN** its card is rendered at reduced opacity (matching the vaulted gear appearance)

#### Scenario: Non-vaulted relic card has full opacity
- **WHEN** a relic is not vaulted
- **THEN** its card is rendered at full opacity

---

### Requirement: View refreshes when active wishlist changes
The Wishlist Relics view SHALL reload its relic cards whenever the active wishlist is changed.

#### Scenario: Changing wishlist updates displayed relics
- **WHEN** the user changes the active wishlist (from another tab)
- **THEN** the Wishlist Relics view refreshes to show relics for the newly active wishlist

---

### Requirement: Relic card width scales with the selected font size
Each relic card's width SHALL scale proportionally with the application's content font size, so that the approximate number of characters that fit on one reward-name line remains roughly constant across the Small, Medium, and Large font settings. Card width SHALL update when the font size setting changes, without requiring an application restart.

#### Scenario: Larger font widens relic cards
- **WHEN** the user selects the Large font size
- **THEN** each relic card is rendered wider than it is at the Medium font size

#### Scenario: Smaller font narrows relic cards
- **WHEN** the user selects the Small font size
- **THEN** each relic card is rendered narrower than it is at the Medium font size

#### Scenario: Card width updates live with the font setting
- **WHEN** the user changes the font size while relic cards are visible
- **THEN** the relic cards resize to the new width without an application restart

#### Scenario: Wrapping budget stays roughly constant across sizes
- **WHEN** the same reward name is displayed at Small, Medium, and Large font sizes
- **THEN** it occupies approximately the same number of wrapped lines at each size
