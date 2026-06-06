## Context

The Wishlist Relics tab currently shows only the relic side of the farming equation: which relics contain wishlisted parts. The data to answer the other side — which missions drop those relics — is already fully imported into the database (`mission_sources`, `reward_rotations`, `mission_rewards`). No schema migration is needed; this change is purely a new read path and presentation layer on top of existing data.

The existing `WishlistRelicsViewModel` / `IWishlistRelicsReader` pair is the pattern to follow: a port in Application, a SQLite reader in Infrastructure, a ViewModel in Presentation.

## Goals / Non-Goals

**Goals:**
- Surface per-mission cumulative wishlist-relic drop chances, computed entirely in the read path
- Show rotation-level breakdown for endless missions, including zero-chance rotations
- Rank missions by effective chance descending
- Integrate into the existing Wishlist tab without adding a new top-level tab

**Non-Goals:**
- Any write operations or user interaction beyond scrolling/reading
- Filtering by mission type, region, or relic era
- Showing the part-level probability inside a relic (what you get when you crack it — that belongs to the relics panel)
- Vaulted-relic handling (no mission rewards a vaulted relic, so it is excluded by the join topology automatically)

## Decisions

### 1. Single flat SQL query, grouping in C#

**Decision:** `GetWishlistFarmingMissions.sql` returns one row per `(mission, rotation, relic)` triple. Rotations with no wishlisted relics appear as rows with `NULL` relic columns (via `LEFT JOIN`). Non-rotation missions use a `UNION ALL` branch with `NULL` rotation columns. C# groups the flat rows into the domain model and computes averages.

**Rationale:** Consistent with `GetWishlistRelics.sql` and `BuildEntries` pattern already in the codebase. Avoids SQLite window functions (which would add query complexity for little gain). Grouping and averaging over a few hundred rows in C# is negligible. Sorting descending by effective chance happens in C# after grouping, keeping SQL focused on data retrieval.

**Alternative considered:** Two queries (mission list + rotation details). Rejected: requires correlating two result sets in C# and adds a second round-trip.

### 2. Effective chance definition

**Decision:**
- Non-rotation mission: `EffectiveChance = SUM(chance_percent)` for all wishlisted relics in the pool.
- Rotation mission: `EffectiveChance = AVG(cumulative_chance_per_rotation)` across **all** rotations, including those with 0%.

**Rationale:** A player farming a rotation mission must cycle through all rotations. Including zero-chance rotations in the denominator correctly models the dilution effect (a survival mission where only rotation A has wishlisted relics is three times worse than the rotation A chance alone suggests). The zero-chance rotations are also shown explicitly in the UI so the player can see why the average is lower.

**Alternative considered:** Averaging only over rotations that have wishlisted relics. Rejected: overstates farming efficiency and misleads the player.

### 3. Tab layout — two-panel split, not a new tab

**Decision:** The existing Wishlist Relics tab becomes a two-column `Grid` (`* *`). The left column is unchanged (existing relic cards + controls). The right column is a new `ScrollViewer` hosting the farming missions panel. Both panels reload together when the active wishlist changes.

**Rationale:** The user identified the workflow as: pick relics based on their rewards, immediately see where those relics drop. Side-by-side makes both directions of that workflow instant. A separate tab would require navigation and context-switching.

**Alternative considered:** Toggle/radio between "Relics" and "Missions" views in the same area. Rejected: forces the user to choose one view at a time, breaking the cross-reference workflow.

### 4. Domain model nesting

```
WishlistFarmingMissionEntry
  DisplayLabel        string
  EffectiveChance     double          // ordering metric; avg or direct sum
  HasRotations        bool
  Rotations           IReadOnlyList<WishlistFarmingRotationEntry>   // empty when HasRotations=false
  Relics              IReadOnlyList<WishlistFarmingRelicEntry>      // empty when HasRotations=true

WishlistFarmingRotationEntry
  RotationDisplayLabel  string        // e.g. "Rotation A"
  CumulativeChance      double        // sum of wishlisted relic chances in this rotation
  Relics                IReadOnlyList<WishlistFarmingRelicEntry>    // may be empty (zero-chance rotation)

WishlistFarmingRelicEntry
  RelicName           string
  ChancePercent       double
```

`Relics` at mission level and at rotation level are mutually exclusive: non-rotation missions use the mission-level list; rotation missions populate only the rotation-level lists. This avoids nullable/union types and keeps the ViewModel mapping straightforward.

### 5. Query row format

Each row returned by the SQL query carries:

| Column | Type | Notes |
|---|---|---|
| `MissionSourceId` | long | Grouping key |
| `MissionDisplayLabel` | string | |
| `MissionSourceOrder` | int | For stable sort before C# reorders by chance |
| `RotationId` | long? | NULL for non-rotation missions |
| `RotationDisplayLabel` | string? | NULL for non-rotation missions |
| `RotationSourceOrder` | int? | NULL for non-rotation missions |
| `RelicName` | string? | NULL for zero-chance rotations |
| `RelicChancePercent` | double? | NULL for zero-chance rotations |

The UNION ALL in SQL separates the two structural cases. The LEFT JOIN on `mission_rewards` within the rotation branch ensures every rotation row appears even when no wishlisted relics are rewarded there.

## Risks / Trade-offs

**Cumulative chance can exceed 100%** → No mitigation needed. A pool's `chance_percent` values sum to 100%, and only a fraction of slots will be wishlisted relics, so the sum of wishlisted-relic chances is always ≤ 100% within a single pool.

**Large wishlist → many matching missions** → No pagination is planned. In practice the Warframe drop table has ~400 mission sources; even with a full wishlist the result set is a few hundred rows of text. A `ScrollViewer` handles it.

**Mission ordering is by effective chance, not by player proximity** → Accepted trade-off. Sorting by geography would require external data not in the drop table.

## Open Questions

None — all design decisions were settled during exploration.
