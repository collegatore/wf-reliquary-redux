## Context

The farming-missions rundown is produced by `GetWishlistFarmingMissions.sql` (one row per mission × rotation × wishlisted relic) and shaped in memory by `SqliteWishlistFarmingMissionsReader`. That reader already does multi-level aggregation: it groups rows into missions, missions into rotations, sums relic chances into a per-rotation/per-pool cumulative chance, averages rotations into an effective chance, and computes a reward `ComputeFingerprint` used to group equivalent missions of the same type into one card.

`mission_rewards` faithfully mirrors the imported drop table, including the case where one reward is listed twice in the same table with separate chances (e.g. Axi Y1 in Void/Mot). Every row carries a `reward_item_id`; relic rewards additionally carry a `relic_id`, which is 1:1 with the reward item (a relic's display name *is* its reward-item name — see `HtmlDropTableParser`). Non-relic rewards such as Aya and Endo have a `reward_item_id` and a NULL `relic_id`. Today each duplicate occurrence becomes its own `WishlistFarmingRelicEntry`, so the relic appears twice in a card, and the fingerprint is computed over the un-merged rows.

The dataset is tiny: a full import is on the order of a few thousand `mission_rewards` rows, and the rundown query returns at most a few dozen rows for a typical wishlist. Pre-V1 rules: schema changes use normal incremental EF migrations (squash to a single init happens once at V1 prep), non-schema formats evolve in place, no back-compat shims.

A near-future feature is anticipated: a "where to farm X" rundown for non-relic resources (Aya, Endo), which will need the identical duplicate-merge keyed on `reward_item_id`. The design accommodates that second consumer now rather than re-deriving the merge later.

## Goals / Non-Goals

**Goals:**
- A reward that appears multiple times within one pool/rotation is shown once, with summed chance.
- Deduplication happens upstream of display, cumulative/effective chance, and the grouping fingerprint, so all three are consistent.
- Keep `mission_rewards` as the single, faithful, un-deduplicated source of truth — it remains the table a developer reads for manual cross-validation of the import.
- Express the merge once, in a form a future Aya/Endo rundown can reuse without re-deriving it.

**Non-Goals:**
- No change to relic-cards, prime-gear, or any chance shown outside the rundown.
- No de-duplication across different rotations or different missions — merging is scoped to a single pool or single rotation.
- No new user setting or UI layout change.
- No second *stored* copy of reward data — the merged form is computed, not materialized.

## Decisions

### Decision: Express the merge as a SQL view `mission_rewards_merged`, not an inline GROUP BY and not a physical distilled table

Add a view that collapses duplicate rewards at the universal grain and sums the chance:

```sql
CREATE VIEW mission_rewards_merged AS
SELECT mission_source_id, rotation_id, reward_item_id, relic_id,
       SUM(chance_percent) AS chance_percent, MIN(source_order) AS source_order
FROM mission_rewards
GROUP BY mission_source_id, rotation_id, reward_item_id;
```

`GetWishlistFarmingMissions.sql` then reads `FROM mission_rewards_merged` everywhere it reads `mission_rewards` today, and drops any per-row grouping of its own. The reader receives at most one row per (mission, rotation, reward) and needs no merge logic: `ComputeFingerprint`, the cumulative sums, and the effective-chance math all operate on already-merged rows unchanged. A future Aya/Endo rundown reads the same view, selecting by `reward_item_id`.

**Why a view over an inline GROUP BY:** an inline GROUP BY solves only the relic query; the anticipated resource query would have to re-derive the same merge. The view states the merge once and both consumers reuse it.

**Why a view over a physical distilled table:** the proposal floated a pre-aggregated table written at import, with `mission_rewards` retained as a backup. That stores reward data twice, adds an insert tier to `SqliteDropTableImportStore`, and creates a copy that can drift from the raw rows. A view stores nothing — it is recomputed on read — so there is exactly one stored copy (`mission_rewards`, the source of truth and the manual cross-validation table) and the merged form is provably consistent with it. At this data scale (~thousands of rows, read on user interaction) the recompute cost is below measurement noise. EF already maps these queries via `Database.SqlQueryRaw`, so reading a view requires no entity mapping.

### Decision: Merge grain is `(mission_source_id, rotation_id, reward_item_id)`

`reward_item_id` is the universal key: present on every row, 1:1 with `relic_id` for relic rewards, and the natural key for non-relic rewards (Aya, Endo). Grouping on it is exact for relics (identical to grouping on `relic_id`) and generalizes to resources for free. `relic_id` is carried through the SELECT as a functionally-dependent passthrough so the relic query keeps its existing join to `relics`. NULL `rotation_id` (non-rotation pools) groups as a single bucket per reward, which is the desired behavior.

### Decision: Add the view as a new incremental EF migration via `migrationBuilder.Sql`

Per the pre-V1 rule, schema changes use the normal incremental EF migration workflow — the init migration is not hand-edited (squashing into a single init is a one-time V1-prep step, not done here). Add a new migration whose `Up` issues `CREATE VIEW mission_rewards_merged` as a raw `migrationBuilder.Sql(...)` statement and whose `Down` issues `DROP VIEW`. EF does not model views, so no entity type is registered and the model snapshot is unaffected by the view itself.

## Risks / Trade-offs

- **`reward_item_id` ↔ `relic_id` 1:1 assumption** → The view carries `relic_id` as a passthrough on a group keyed by `reward_item_id`; this is only well-defined if a reward item maps to at most one relic. Confirmed in `HtmlDropTableParser` (a relic's reward-item name is its display name), but add a test asserting the invariant so a future parser change can't silently break the view.
- **Zero-chance rotation rows still survive** → The view operates on `mission_rewards` only; the rundown query's `LEFT JOIN` from `reward_rotations` is what preserves rotations with no wishlisted reward, and that join is unchanged. The existing zero-chance-rotation test must stay green.
- **Ordering** → The view aggregates away per-row `source_order`; `MIN(source_order)` is exposed so the rundown query's existing `ORDER BY ... RewardSourceOrder` stays stable.
- **Fingerprint stability** → Because the view merges before the reader runs `ComputeFingerprint`, two tables that differ only by how duplicates are split now produce the same fingerprint and group together — intended, but a (desirable) change in grouping output worth asserting in a test.

## Migration Plan

Add a new EF migration (`dotnet ef migrations add`) whose `Up` runs `CREATE VIEW mission_rewards_merged` via `migrationBuilder.Sql` and whose `Down` runs `DROP VIEW`. No data change, no new table, no import-code change — `mission_rewards` is written exactly as today. The migration applies cleanly on top of an existing local DB (it only adds a view), so no re-initialization notice is needed; the accumulated migrations are squashed into init later as a one-time V1-prep step.

## Verification

The view's per-query cost was asserted empirically (`WishlistFarmingMissionsBenchmarkTests`, `[Explicit]`), timing `GetMissionsForWishlistAsync` against the real preserved drop table (`TestData/warframe-drop-tables-sample.html`, ~10,319 `mission_rewards` rows) with the **maximal wishlist** (every non-vaulted part — the worst case, unreachable in practice).

**Measured result (worst case, median of 25):**

| Variant | Median | vs baseline |
|---|---|---|
| Baseline (no view) | ~37 ms | — |
| Plain view (**shipped**) | ~48–56 ms | +30–50% |
| View + composite index | ~55 ms | barely changed |
| View + index + `MATERIALIZED` CTE (aggregate once) | ~53 ms | barely changed |

The regression is the view re-aggregating the whole table on each of its three references. Neither a composite index on `(mission_source_id, rotation_id, reward_item_id)` (removed the temp-B-tree sort but not the cost) nor a `MATERIALIZED` CTE (one aggregation instead of three) recovered meaningfully — the dominant cost is the table-wide aggregation itself, versus the baseline's targeted per-mission index lookups. **Decision: ship the plain view.** The regression manifests only at a wishlist covering every part in the game; realistic wishlists touch few missions and run in a few milliseconds, where the delta is negligible. The index/CTE were trialled and reverted to keep the query and schema simplest.

## Open Questions

None.
