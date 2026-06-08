## Why

The shipped reward-dedup design (`2026-06-08-dedupe-mission-reward-chances`) chose a SQL view (`mission_rewards_merged`) over a physically materialized "distilled" table, and rejected the table partly on a stored-twice/drift argument and partly on benchmark numbers showing the plain view regressed the worst-case rundown by +30–50% (~37 ms → ~48–56 ms) while a composite index and a `MATERIALIZED` CTE on the view both failed to recover it. The one variant never actually measured is the one the proposal originally floated: a real pre-aggregated table written after import and indexed for the rundown's access pattern. We want that empirical number to confirm (or overturn) the view decision, without disturbing the shipped behavior.

## What Changes

- Add an `[Explicit]` integration benchmark that mirrors `WishlistFarmingMissionsBenchmarkTests` exactly — same bundled full-size drop table, same maximal (all non-vaulted parts) wishlist, same warm-up, same 25-iteration median, same `EXPLAIN QUERY PLAN` dump — but routes the rundown through a physically materialized **distilled table** instead of the `mission_rewards_merged` view.
- After the normal import, the benchmark builds `mission_rewards_distilled` (one row per `(mission_source_id, rotation_id, reward_item_id)` with summed `chance_percent` and `MIN(source_order)`, `relic_id` carried as passthrough) and creates indexes matched to the rundown query's access pattern.
- The benchmark runs a copy of `GetWishlistFarmingMissions.sql` rewritten to read `FROM mission_rewards_distilled`, so the table and view variants are compared on an otherwise identical query and grouping path.
- No production code, schema, migration, or shipped query changes. The distilled table is created and indexed entirely within the test; `mission_rewards`, the view, and the live reader are untouched.

## Capabilities

### New Capabilities
- `distilled-reward-table-benchmark`: An explicit, on-demand integration benchmark that measures the worst-case farming-missions rundown when reward dedup is served from a materialized, indexed distilled table, comparable apples-to-apples against the shipped view benchmark.

### Modified Capabilities
<!-- None. This change adds a measurement harness only; it does not alter any shipped requirement. The wishlist-farming-missions behavior, the dedup view, and their specs are unchanged. -->

## Impact

- **New test file** under `tests/Integration/WF.Reliquary.IntegrationTests/` (e.g. `WishlistFarmingMissionsDistilledTableBenchmarkTests.cs`), `[Explicit]` so it never runs in the normal suite.
- Reuses existing infrastructure read-only: `SqliteDropTableImportStore`, `SqliteWishlistStore`, `HtmlDropTableParser`, `SqlQueryLoader`, and `TestData/warframe-drop-tables-sample.html`.
- No changes to `src/`. No new migration. No change to `mission_rewards_merged`, `GetWishlistFarmingMissions.sql`, or `SqliteWishlistFarmingMissionsReader`.
