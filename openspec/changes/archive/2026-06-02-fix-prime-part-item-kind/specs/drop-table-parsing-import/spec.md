## MODIFIED Requirements

### Requirement: Relic Reward Parsing

The system SHALL parse the source `Relics` section into base relics, Prime Part reward items, and relic reward rows suitable for PI1 Prime Part acquisition queries. An item SHALL only be classified as `item_kind = 'PrimePart'` when it appears as a relic reward component AND its name matches the prime part naming pattern. Items that match the prime name pattern but only appear as mission rewards SHALL NOT be classified as `PrimePart`.

#### Scenario: Base relic is parsed from refinement table

- **WHEN** a relic reward table is headed by a label such as `Lith C14 Relic (Intact)`
- **THEN** the parser records the base relic era, code, and display name without treating each refinement level as a separate relic

#### Scenario: Prime Part reward row is parsed

- **WHEN** an Intact relic table contains a Prime Part reward row
- **THEN** the parser records the reward item as `item_kind = 'PrimePart'`, source rarity, source chance, and derived reward tier

#### Scenario: Reward tier is derived from Intact chance

- **WHEN** an Intact relic reward row has chance `25.33%`, `11.00%`, or `2.00%`
- **THEN** the parser maps the relic reward tier to Common, Uncommon, or Rare respectively

#### Scenario: Non-Prime relic reward is encountered

- **WHEN** a relic reward row does not represent a PI1 Prime Part or blueprint
- **THEN** the parser preserves the reward item row with its source context and classifies it outside the Prime Part subset

#### Scenario: Prime-named item appears only as a mission reward

- **WHEN** an item whose name matches the prime part naming pattern (e.g. `Sagek Prime`) appears only in the Missions section and not in any relic reward pool
- **THEN** the parser classifies it as `item_kind = 'Other'`, not `'PrimePart'`

#### Scenario: Prime-named item appears in both mission and relic rewards

- **WHEN** an item whose name matches the prime part naming pattern appears in the Missions section and also appears inside a relic reward pool
- **THEN** the parser classifies it as `item_kind = 'PrimePart'` regardless of which section was encountered first
