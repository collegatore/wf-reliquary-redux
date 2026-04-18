# Drop Table Schema Analysis

**Source URL**: https://warframe-web-assets.nyc3.cdn.digitaloceanspaces.com/uploads/cms/hnfvc0o3jnfvc873njb03enrf56.html
**Local sample**: `artifacts/warframe-drop-tables-sample.html`
**Retrieved**: 2026-04-18
**Source last update**: 08 April, 2026

## Sample Structure

The downloaded HTML is a generated Warframe PC drop table page. The parts relevant to Product Increment 1 are:

- `Missions`, identified by `h3 id="missionRewards"`.
- `Relics`, identified by `h3 id="relicRewards"`.

The sample contains 439 mission reward tables, 10,319 mission reward rows, 3,475 mission reward rows whose reward is a relic, 3,014 relic refinement tables, 18,086 relic reward rows, and 15,840 Prime Part reward rows.

## Observed Mission Reward Shape

Mission reward tables are headed by source labels such as `Mercury/Apollodorus (Survival)`. Some source labels include qualifiers such as `Event:` or extra reward pools. Mission source labels commonly encode:

- Planet or region.
- Node or location.
- Mission type in parentheses.
- Optional special qualifier such as event, extra, cache, or variant text.

Rows may be grouped by rotation headings such as `Rotation A`, `Rotation B`, and `Rotation C`. Some mission sources have no rotation heading, so rotation must be optional.

Each mission reward row has:

- Reward item name, such as `Lith C14 Relic`.
- Rarity label, such as `Rare` or `Uncommon`.
- Drop percentage, such as `7.69%`.
- Source row context: mission table plus optional rotation.

Representative rows:

| Mission Source | Rotation | Reward | Rarity | Chance |
|----------------|----------|--------|--------|--------|
| Mercury/Apollodorus (Survival) | B | Lith C14 Relic | Rare | 7.69% |
| Mercury/Apollodorus (Survival) | C | Lith C14 Relic | Uncommon | 11.06% |
| Mercury/Lares (Defense) | A | Lith K12 Relic | Rare | 9.09% |

## Observed Relic Reward Shape

Relic reward tables are headed by relic refinement labels such as `Axi A1 Relic (Intact)` and `Axi A1 Relic (Exceptional)`. These source tables are useful for parsing, but refinement labels do not need their own persistent records because all relics use the same reward-tier chance table.

Each relic reward row has:

- Reward item name, usually a Prime Part or blueprint in PI1 scope.
- Source rarity label and drop percentage.
- Source row context: relic era, relic code, and refinement level.
- Persistent reward tier derived from the Intact table's chance, not from the source rarity label.

Representative rows:

| Source Refinement Table | Reward | Source Rarity | Source Chance |
|---------------|--------|--------|--------|
| Axi A1 Relic (Intact) | Akstiletto Prime Barrel | Uncommon | 11.00% |
| Axi A1 Relic (Intact) | Trinity Prime Systems Blueprint | Uncommon | 25.33% |
| Axi A1 Relic (Intact) | Nikana Prime Blueprint | Rare | 2.00% |
| Axi A1 Relic (Exceptional) | Akstiletto Prime Barrel | Uncommon | 13.00% |

Persistent reward-tier derivation uses Intact relic chances:

| Intact Chance | RewardTier |
|---------------|------------|
| 25.33% | Common |
| 11.00% | Uncommon |
| 2.00% | Rare |

Refinement-specific chances are then derived from the fixed relic tier table:

| RewardTier | Intact | Exceptional | Flawless | Radiant |
|------------|--------|-------------|----------|---------|
| Common | 25.33% | 23.33% | 20.00% | 16.67% |
| Uncommon | 11.00% | 13.00% | 17.00% | 20.00% |
| Rare | 2.00% | 4.00% | 6.00% | 10.00% |

## Proposed Logical DB Structure

This is a query-ready logical structure for planning. Column names are deliberately descriptive and can be adapted during implementation.

### Source sample provenance

The source URL, retrieval date, source last-update date, and raw artifact path are preparation-stage metadata. They are preserved in design artifacts and may be used by diagnostics or import logs, but they are not part of the PI1 persistent DB schema.

### reward_items

Stores normalized reward names once, regardless of where they appear.

| Field | Purpose |
|-------|---------|
| item_id | Stable internal identifier. |
| name | Display name from the source row. |
| normalized_name | Search-normalized name. |
| item_kind | Classification such as PrimePart, Relic, Blueprint, Resource, Mod, or Other. |
| is_prime_part | Convenience flag for first-increment queries. |

### relics

Stores the base relic independent of refinement.

| Field | Purpose |
|-------|---------|
| relic_id | Stable internal identifier. |
| era | Lith, Meso, Neo, Axi, or other era text if the source adds one. |
| code | Relic code such as C14, D8, or A1. |
| display_name | Base display name, such as Lith C14 Relic. |

### relic_rewards

Stores reward slots inside base relics.

| Field | Purpose |
|-------|---------|
| relic_reward_id | Stable internal identifier. |
| relic_id | Base relic that can award the item. |
| item_id | Reward item, usually a Prime Part in PI1 scope. |
| reward_tier | Common, Uncommon, or Rare. |
| reward_slot | Stable ordinal position within the relic reward set. |

### mission_sources

Stores mission reward table headers as source pools.

| Field | Purpose |
|-------|---------|
| mission_source_id | Stable internal identifier. |
| display_label | Full source label exactly as represented in the table header. |
| region | Parsed planet or region when available. |
| node | Parsed node or location when available. |
| mission_type | Parsed mission type when available. |
| qualifier | Event, Extra, Cache, Steel Path, or other source qualifier when available. |

### reward_rotations

Stores optional reward pool groupings for mission sources.

| Field | Purpose |
|-------|---------|
| rotation_id | Stable internal identifier. |
| mission_source_id | Parent mission source. |
| rotation_label | A, B, C, or source-provided label. |
| display_label | Full source label, such as Rotation A. |

### mission_rewards

Stores reward probabilities from mission reward pools.

| Field | Purpose |
|-------|---------|
| mission_reward_id | Stable internal identifier. |
| mission_source_id | Mission source that can award the item. |
| rotation_id | Optional rotation, empty for unrotated pools. |
| item_id | Reward item. For PI1 source lookups, this is often a relic item. |
| relic_id | Optional direct link when the reward item is recognized as a relic. |
| rarity_label | Source rarity label. |
| chance_percent | Numeric drop chance from the source. |
| source_order | Row order within the source table for traceability. |

## Relationship Summary

- One relic has many relic reward rows.
- One reward item can appear in many relic reward rows and many mission reward rows.
- One mission source has zero or more rotations.
- One mission source has many mission reward rows.
- One rotation has many mission reward rows.
- Mission rewards that award relics link back to the recognized base relic so the app can connect Prime Part searches to mission sources.

## Primary Query Paths

Prime Part to relics:

1. Find `reward_items` where `is_prime_part` is true and name matches the user search.
2. Join through `relic_rewards` to `relics`.
3. Display relic era, code, reward tier, and derived refinement-specific chances.

Relic to mission sources:

1. Find `relics` matching era and code.
2. Join through `mission_rewards` where `relic_id` matches.
3. Join to `mission_sources` and optional `reward_rotations`.
4. Display mission source, mission type, rotation when present, rarity label, and mission reward chance.

Prime Part to acquisition path:

1. Resolve the Prime Part to candidate relics via relic rewards.
2. Resolve each candidate relic to mission source rows via mission rewards.
3. Present the combined path while keeping derived relic reward chance separate from mission source chance.

## Modeling Notes

- Relic reward tier and mission reward chance must remain separate values; multiplying or ranking derived relic chances with mission chances is a later product decision.
- Relic refinement chances are derived from the fixed reward-tier table rather than stored as separate refinement records.
- Rotation is optional because some mission reward tables list direct rewards without A/B/C grouping.
- Duplicate item names in the same reward pool should remain as separate source rows when the source contains separate rows.
- Mission source parsing should preserve the full display label even when region, node, or mission type parsing succeeds.
- Non-relic mission rewards should remain stored because complete reward pools help validate percentages and preserve source fidelity, but PI1 queries primarily consume rows where the reward item is a relic.
