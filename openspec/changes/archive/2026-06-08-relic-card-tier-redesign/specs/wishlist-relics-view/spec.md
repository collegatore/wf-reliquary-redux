## MODIFIED Requirements

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

## ADDED Requirements

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
