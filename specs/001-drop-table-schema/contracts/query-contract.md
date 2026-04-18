# Query Contract: Prime Part and Relic Acquisition Paths

## Purpose

Define the application query behaviors required by Product Increment 1. These
queries can be backed by SQLite later, but the contract is expressed in
application terms so UI and persistence remain decoupled.

## Query: Find Prime Part Relics

Input:

| Field | Required | Description |
|-------|----------|-------------|
| `searchText` | Yes | Player-entered Prime Part search text. |

Output row:

| Field | Description |
|-------|-------------|
| `primePartName` | Matched Prime Part display name. |
| `relicEra` | Relic era, such as Lith, Meso, Neo, or Axi. |
| `relicCode` | Relic code, such as C14 or A1. |
| `rewardTier` | `Common`, `Uncommon`, or `Rare`. |
| `intactChancePercent` | Derived chance for Intact relics. |
| `exceptionalChancePercent` | Derived chance for Exceptional relics. |
| `flawlessChancePercent` | Derived chance for Flawless relics. |
| `radiantChancePercent` | Derived chance for Radiant relics. |

Behavior:

- Search must use normalized item names.
- Results must include the base relic and reward tier for every matching Prime
  Part.
- Relic reward chances must be derived from reward tier and refinement, not
  stored as separate refinement records.
- Derived relic reward chances must not be combined with mission reward chances.

## Query: Find Relic Mission Sources

Input:

| Field | Required | Description |
|-------|----------|-------------|
| `era` | Yes | Relic era. |
| `code` | Yes | Relic code. |

Output row:

| Field | Description |
|-------|-------------|
| `relicDisplayName` | Base relic display name. |
| `missionDisplayLabel` | Full source mission label. |
| `region` | Parsed region when available. |
| `node` | Parsed node when available. |
| `missionType` | Parsed mission type when available. |
| `rotation` | Optional rotation label. |
| `missionRewardRarity` | Rarity label in the mission reward table. |
| `missionRewardChancePercent` | Chance that the mission pool awards the relic. |

Behavior:

- Results must include rotated and unrotated mission reward rows.
- Results must preserve the full mission display label even when parsed fields
  are available.
- Duplicate source rows must remain visible if the source contains them.

## Query: Build Prime Part Acquisition Path

Input:

| Field | Required | Description |
|-------|----------|-------------|
| `searchText` | Yes | Player-entered Prime Part search text. |

Output:

- Matched Prime Part.
- Candidate base relics, reward tier, and derived refinement-specific relic
  reward chances.
- Mission source rows for each base relic.
- Separate probability fields for derived relic reward chance and mission reward
  chance.

Behavior:

- The query may group output by Prime Part, then base relic, then mission source.
- The query must not rank missions by efficiency until a later feature defines
  ranking rules.
- Missing mission source rows must be represented as "no known mission source in
  the active data set" rather than dropping the relic result.
