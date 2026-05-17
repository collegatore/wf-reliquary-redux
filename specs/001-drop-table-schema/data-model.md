# Data Model: Drop Table Schema Preparation

## Overview

The persistent model supports three primary behaviors:

- Resolve Prime Parts to base relics and reward tiers.
- Resolve reward tiers to refinement-specific relic chances by rule.
- Resolve relics to mission sources, rotations, and mission reward chances.

The downloaded drop table sample and its provenance are design artifacts for
this preparatory stage. They are not part of the persistent DB schema.

## Non-Persistent Preparation Artifact: Source Sample

Represents the local planning artifact used to design and validate the parser.
This metadata may appear in documentation, diagnostics, or import logs, but it
is not part of the PI1 persistent schema.

Fields:

- `SourceUrl`: original public URL.
- `RetrievedAt`: timestamp when the sample was downloaded.
- `SourceLastUpdatedOn`: date shown by the source page.
- `RawArtifactPath`: repo-relative path to the preserved HTML file.

Validation rules:

- `RawArtifactPath` must point to the local sample artifact during planning.
- `SourceLastUpdatedOn` may be empty only if the source page omits it.

## Entity: RewardItem

Represents a normalized reward name, regardless of where the reward appears.

Fields:

- `ItemId`: stable internal identifier.
- `Name`: display name from the source row.
- `NormalizedName`: case-insensitive and punctuation-normalized search key.
- `ItemKind`: `PrimePart`, `Relic`, `Blueprint`, `Resource`, `Mod`, or `Other`.
- `IsPrimePart`: true when the item is in PI1 Prime Part scope.

Relationships:

- Can appear in many `RelicReward` rows.
- Can appear in many `MissionReward` rows.
- May map to one `Relic` when `ItemKind` is `Relic`.

Validation rules:

- `Name` and `NormalizedName` are required.
- `NormalizedName` must be stable for repeated imports of the same source name.
- Prime Part detection must include Prime blueprints and named Prime parts from
  relic reward rows.

## Entity: Relic

Represents a base Void Relic.

Fields:

- `RelicId`: stable internal identifier.
- `Era`: era text, such as `Lith`, `Meso`, `Neo`, or `Axi`.
- `Code`: relic code, such as `C14`, `D8`, or `A1`.
- `DisplayName`: base label, such as `Lith C14 Relic`.
- `RewardItemId`: optional link to the matching `RewardItem` row.

Relationships:

- Has many `RelicReward` rows.
- Can be linked from many `MissionReward` rows where the mission reward item is
  this relic.

Validation rules:

- `Era`, `Code`, and `DisplayName` are required.
- `(Era, Code)` must identify one base relic in the active imported data set.

## Rule: Relic Refinement Chances

Relic refinements are not stored as separate records because every relic follows
the same tier-to-chance pattern.

| RewardTier | Intact | Exceptional | Flawless | Radiant |
|------------|--------|-------------|----------|---------|
| Common | 25.33% | 23.33% | 20.00% | 16.67% |
| Uncommon | 11.00% | 13.00% | 17.00% | 20.00% |
| Rare | 2.00% | 4.00% | 6.00% | 10.00% |

Validation rules:

- Refinement must be one of `Intact`, `Exceptional`, `Flawless`, or `Radiant`.
- Reward tier must be one of `Common`, `Uncommon`, or `Rare`.
- Displayed relic reward chance is derived from this table, not persisted per
  relic reward row.

## Entity: RelicReward

Represents one reward row inside a base relic.

Fields:

- `RelicRewardId`: stable internal identifier.
- `RelicId`: base relic that can award the item.
- `ItemId`: reward item.
- `RewardTier`: `Common`, `Uncommon`, or `Rare`.

Relationships:

- Belongs to one `Relic`.
- References one `RewardItem`.

Validation rules:

- Each relic should have exactly 6 reward rows: 3 `Common`, 2 `Uncommon`, and
  1 `Rare`.
- `RewardTier` is derived from the Intact relic chance in the source table:
  25.33% maps to `Common`, 11.00% maps to `Uncommon`, and 2.00% maps to `Rare`.
- Source rarity labels in the Relics section must not be used for reward tier
  assignment because they are unreliable for this purpose.
- Duplicate reward item names must remain separate rows if the source has
  separate reward rows.

## Entity: MissionSource

Represents one mission reward table header or source pool.

Fields:

- `MissionSourceId`: stable internal identifier.
- `DisplayLabel`: full source label exactly as shown.
- `Region`: parsed planet or region when available.
- `Node`: parsed node or location when available.
- `MissionType`: parsed mission type when available.
- `Qualifier`: event, extra pool, cache, Steel Path, or other qualifier when
  available.
- `SourceOrder`: source table order used to distinguish repeated labels and
  preserve traceability.

Relationships:

- Has zero or more `RewardRotation` rows.
- Has many `MissionReward` rows.

Validation rules:

- `DisplayLabel` is required.
- Parsed fields are optional and must never replace the full display label.
- `DisplayLabel` is descriptive source text, not a uniqueness key. The source
  can contain repeated mission source labels, so persistence and parsed models
  must use `MissionSourceId` and/or `SourceOrder` to distinguish rows.

## Entity: RewardRotation

Represents an optional mission reward grouping.

Fields:

- `RotationId`: stable internal identifier.
- `MissionSourceId`: parent mission source.
- `RotationLabel`: `A`, `B`, `C`, or source-provided value.
- `DisplayLabel`: full heading, such as `Rotation A`.

Relationships:

- Belongs to one `MissionSource`.
- Has many `MissionReward` rows.

Validation rules:

- `MissionSourceId` and `DisplayLabel` are required.
- Mission rewards may exist without a rotation.
- `RotationLabel` may repeat under different mission source rows, including
  mission source rows with the same `DisplayLabel`; rotation identity is scoped
  to the parent `MissionSourceId`.

## Entity: MissionReward

Represents one reward probability row in a mission reward pool.

Fields:

- `MissionRewardId`: stable internal identifier.
- `MissionSourceId`: mission source that can award the item.
- `RotationId`: optional rotation.
- `ItemId`: reward item.
- `RelicId`: optional direct link when the reward item is recognized as a
  relic.
- `RarityLabel`: source rarity label.
- `ChancePercent`: numeric percent from the source row.
- `SourceOrder`: row order within the mission table.

Relationships:

- Belongs to one `MissionSource`.
- Optionally belongs to one `RewardRotation`.
- References one `RewardItem`.
- Optionally references one `Relic`.

Validation rules:

- `ChancePercent` must be between 0 and 100 inclusive.
- `RotationId` is optional.
- `RelicId` is required when `ItemId` is recognized as a relic.
- Duplicate reward item names must remain separate rows if the source has
  separate rows.
- Duplicate mission source display labels must remain separate mission source
  rows if the source has separate tables or source-pool headings.

## State and Lifecycle

Import states:

1. `Parsed`: mission and relic sections were converted to normalized rows.
2. `Validated`: required fields, tier counts, and representative query paths
   were checked against the sample.
3. `Persisted`: normalized persistent rows were saved to SQLite.

Invalid rows should be reported with source context instead of silently dropped.

## Required Query Shapes

Prime Part to relics:

- Input: item search text.
- Output: Prime Part name, relic era, relic code, reward tier, and the
  refinement chance table for that tier.

Relic to mission sources:

- Input: relic era and code.
- Output: mission display label, parsed region, node, mission type, optional
  rotation, mission reward rarity label, and mission reward chance percent.

Prime Part acquisition path:

- Input: item search text.
- Output: candidate base relics, reward tier, derived refinement-specific relic
  chances, and mission sources for the base relics.
