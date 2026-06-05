## MODIFIED Requirements

### Requirement: Relic cards show rewards with wishlisted parts highlighted
Each relic card SHALL have the relic display name as its header, separated from the body by a line. The card body SHALL group rewards by tier in order Common → Uncommon → Rare. Each tier group SHALL begin with a label ("Common:", "Uncommon:", or "Rare:") and end with a line separator. Any reward item whose normalized name matches a wishlisted part SHALL be rendered in bold text.

By default, only wishlisted rewards are shown on each card. When **Show all rewards** is enabled, all rewards (wishlisted and non-wishlisted) are shown.

#### Scenario: Card header shows relic name
- **WHEN** a relic card is displayed
- **THEN** the card header contains the relic's display name (e.g., "Lith A1 Relic") separated from the rewards body by a line

#### Scenario: Card body groups rewards by tier
- **WHEN** a relic card is displayed
- **THEN** rewards are grouped under "Common:", "Uncommon:", and "Rare:" labels in that order, each group followed by a line separator

#### Scenario: Wishlisted reward is bold
- **WHEN** a reward item in a relic card matches a part in the active wishlist
- **THEN** that reward item is rendered in bold text

#### Scenario: Non-wishlisted reward is normal weight
- **WHEN** a reward item in a relic card does not match any part in the active wishlist
- **THEN** that reward item is rendered in normal (non-bold) text

#### Scenario: Default view shows only wishlisted rewards
- **WHEN** Show all rewards is unchecked (default)
- **THEN** each relic card shows only the reward items that are in the active wishlist

#### Scenario: Show all rewards reveals full card contents
- **WHEN** the user checks Show all rewards
- **THEN** each relic card shows all rewards for that relic, with wishlisted rewards in bold and non-wishlisted in normal weight

## ADDED Requirements

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
