# Findings: reward dedup — view vs. materialized distilled table

## Summary

The shipped reward-deduplication design (`archive/2026-06-08-dedupe-mission-reward-chances`) chose a SQL view, `mission_rewards_merged`, over a physically materialized "distilled" table. It rejected the table on two grounds — that the table re-aggregation cost was "below measurement noise," and that a table "stores reward data twice" and can drift. The four-variant benchmark in this change measured the one variant the original work never built (a real precomputed, indexed table), and the results undercut both grounds. **On the facts now in evidence, a distilled table indexed simply on `mission_source_id` is the stronger design; the view was selected on a performance assumption the benchmark contradicts and a storage objection that does not actually differentiate the two options.**

## What was measured

All four variants run one shared routine — same bundled drop table (10,319 `mission_rewards` rows → 9,873 deduped, **only ~446 duplicates in the whole dataset**), same maximal wishlist (every non-vaulted part — the unreachable worst case), same warm-up, same 25-iteration median, same grouping path (`SqliteWishlistFarmingMissionsReader.BuildGroups`). The only differences are whether/how the distilled table is materialized and which dedup source the rundown reads.

| Variant | Median (representative) | mr access in the hot rotation `LEFT JOIN` |
|---|---|---|
| Raw `mission_rewards` table | ~25 ms | `SEARCH … (mission_source_id=?)` — one probe per mission×rotation |
| Distilled + **simple** index on `mission_source_id` | ~33 ms | `SEARCH … ix_distilled_simple (mission_source_id=?)` — one probe per mission×rotation |
| `mission_rewards_merged` **view** (shipped) | ~45–53 ms | re-aggregates the full table 3× per query |
| Distilled + **composite** `(mission_source_id, rotation_id, relic_id)` index | ~60 ms | `SEARCH … (mission_source_id=? AND rotation_id=? AND relic_id=?)` nested under `SCAN wishlisted_relic_ids` — one probe per wishlisted relic |

(Absolute numbers are machine-dependent and noisy at sub-50 ms on this tiny dataset; only within-run ordering and large, plan-driven gaps are meaningful.)

## Finding 1 — the view's per-query cost is real, not noise

The view is referenced three times in `GetWishlistFarmingMissions.sql` (the `missions_with_wishlist` CTE plus the rotation and non-rotation branches). SQLite does not reuse a view's result across references, so the `EXPLAIN QUERY PLAN` shows `MATERIALIZE mission_rewards_merged` twice plus the CTE — i.e. **the full ~10k-row `GROUP BY` runs three times on every rundown load.** The shipped design assumed this was below measurement noise; the benchmark shows the view is measurably slower than both the raw table and a simply-indexed distilled table. The aggregation collapses only ~446 of ~10,319 rows (~4%); the other 96% pass through the GROUP BY contributing nothing but cost — and that cost is paid on the interactive read path, every selection.

## Finding 2 — a materialized table beats the view, but only if indexed simply

A distilled table moves the aggregation from "every read" to "once at import." With a single-column `mission_source_id` index it reads faster than the view (~33 vs ~45–53 ms). **Critical caveat:** the "obvious" composite `(mission_source_id, rotation_id, relic_id)` index makes the table *slower than the view* (~60 ms). The composite index lets the planner push `relic_id IN (~34 wishlisted relics)` into the seek, turning one index probe per mission×rotation into one probe **per wishlisted relic** — the maximal wishlist is exactly the input that maximizes that multiplier. So the table wins only with index discipline; the wrong index is a regression. (See Finding 5 for why this footgun only bites at the unreachable worst case.)

## Finding 3 — "stores twice / drift" does not differentiate view from table

The original objection ("a physical table stores reward data twice and can drift") was raised against the proposal of *keeping a distilled table for workflows plus the raw table as a debugging backup*. But:

- **Raw-as-backup is a constant across both designs.** The shipped view design *also* retains `mission_rewards` as "the single, faithful, un-deduplicated source of truth — the table a developer reads for manual cross-validation." Raw is kept either way. So "stores twice" was conflating two separate things: "keep raw as the debug backup" (true in both designs, not a cost of the table) and "add a small precomputed distilled table" (the only actual delta).
- **The only real delta is one ~9,873-row derived table** (a few hundred KB) plus a little import wiring — weighed against a triple full-table aggregation on every interactive read.
- **Drift is bounded.** The distilled table is not an independently-maintained copy. The only write path, `SqliteDropTableImportStore.ReplaceAsync`, wipes and rebuilds everything in one transaction, and it *already* materializes derived data the same way after the raw inserts (`InsertGearVaultStatus.sql`, `UpdateRelicVaultStatus.sql`). A distilled-table build (`CREATE TABLE … AS SELECT` + indexes) is the same established pattern, regenerated atomically alongside raw. A cheap invariant test (`COUNT(distilled) == COUNT(merged)`) pins consistency. Residual drift risk reduces to "someone adds a second write path and forgets," which a test guards.

## Finding 4 — the view's only surviving advantage is simplicity

A view cannot drift because it stores nothing, and `CREATE VIEW` is a one-line migration with no import wiring and no index to misjudge. That is a genuine but narrow advantage. Against it: the view re-aggregates the whole table three times per read, and the table's drift risk is cheaply enforceable. The simplicity edge does not outweigh paying a triple aggregation on the path that repeats.

## Finding 5 — at realistic wishlists, all variants converge

The differences above are worst-case (maximal wishlist ≈ every non-vaulted relic). Realistic wishlists touch few missions and few relics, the probe multiplier collapses to ~1, and all variants run in a few milliseconds — below human perception. So this is not a user-facing performance emergency; it is an argument about where dedup cost belongs (import vs. read) and whether the view was chosen on accurate premises.

## Objections to the shipped path

1. **The view was justified on a false premise.** Its rationale stated the re-aggregation cost was below measurement noise. The benchmark measures it as the slowest non-pathological variant — the view re-runs the full GROUP BY three times per read to merge ~446 rows.
2. **The "stores twice / drift" objection was circular.** It was leveled against the distilled-table-plus-raw-backup idea, but the shipped view design keeps the raw backup too. It never distinguished the two options; it described an intended feature (a faithful debug mirror) as a cost.
3. **The aggregation is on the wrong side of the read/write split.** The import is a rare, already-heavy batch that already materializes derived tables. Recomputing the dedup on every interactive selection — instead of once at import — inverts the natural placement.
4. **The reuse argument is speculative (YAGNI).** The remaining pro-view rationale — "express the merge once so a future Aya/Endo resource rundown reuses the view" — depends on a consumer that does not exist. If it lands, the merge is small enough to extract as a shared SQL snippet then; today it buys nothing.

## Recommendation

Revert the `mission_rewards_merged` view to a materialized `mission_rewards_distilled` table, built in `ReplaceAsync` alongside the existing derived-data materialization, **indexed on `mission_source_id` only** (not the composite). Retain raw `mission_rewards` as the faithful debugging/cross-validation backup, as both designs already do. Keep the `COUNT(distilled) == COUNT(merged-equivalent)` invariant as a test, and document the index discipline so the composite index is not reintroduced as an "optimization." This is a deliberate reversal of a shipped decision and should be proposed and reviewed as its own OpenSpec change.

## Scope note

This change adds only the `[Explicit]` benchmark that produced these findings. It does not modify production code, schema, migrations, or the shipped query. Acting on the recommendation is a separate change.
