## MODIFIED Requirements

### Requirement: Relic cards show rewards grouped by tier with wishlisted parts highlighted
Each relic card SHALL have the relic display name as its header, separated from the body by a line. The card body SHALL group rewards by tier in order Common → Uncommon → Rare. Each tier group SHALL begin with a label ("Common:", "Uncommon:", or "Rare:") and end with a line separator — **except the last tier group, which SHALL NOT have a trailing separator**. Any reward item whose normalized name matches a wishlisted part SHALL be rendered in bold text **only when Show all rewards is enabled**; when Show all rewards is disabled, all visible items are wishlisted and SHALL be rendered in normal weight.

By default, only wishlisted rewards are shown on each card. When **Show all rewards** is enabled, all rewards (wishlisted and non-wishlisted) are shown.

#### Scenario: Card header shows relic name
- **WHEN** a relic card is displayed
- **THEN** the card header contains the relic's display name (e.g., "Lith A1 Relic") separated from the rewards body by a line

#### Scenario: Card body groups rewards by tier
- **WHEN** a relic card is displayed
- **THEN** rewards are grouped under "Common:", "Uncommon:", and "Rare:" labels in that order

#### Scenario: Separator appears between tier groups but not after the last one
- **WHEN** a relic card has multiple tier groups
- **THEN** a line separator appears after each tier group except the last

#### Scenario: No trailing separator when only one tier group is present
- **WHEN** a relic card has only one tier group
- **THEN** no separator is rendered below the rewards list

#### Scenario: Wishlisted reward is bold when Show all rewards is enabled
- **WHEN** Show all rewards is checked and a reward item matches a part in the active wishlist
- **THEN** that reward item is rendered in bold text

#### Scenario: Wishlisted reward is normal weight when Show all rewards is disabled
- **WHEN** Show all rewards is unchecked
- **THEN** all visible reward items are rendered in normal (non-bold) weight

#### Scenario: Non-wishlisted reward is normal weight
- **WHEN** Show all rewards is checked and a reward item does not match any part in the active wishlist
- **THEN** that reward item is rendered in normal (non-bold) text

#### Scenario: Default view shows only wishlisted rewards
- **WHEN** Show all rewards is unchecked (default)
- **THEN** each relic card shows only the reward items that are in the active wishlist

#### Scenario: Show all rewards reveals full card contents
- **WHEN** the user checks Show all rewards
- **THEN** each relic card shows all rewards for that relic, with wishlisted rewards in bold and non-wishlisted in normal weight
