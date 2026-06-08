## 1. Scaffold the benchmark fixture

- [x] 1.1 Add `WishlistFarmingMissionsDistilledTableBenchmarkTests.cs` under `tests/Integration/WF.Reliquary.IntegrationTests/`, `[TestFixture]` + `[Explicit("Performance benchmark — run on demand, not in the normal suite.")]`, with a class summary documenting the `dotnet test --filter "FullyQualifiedName~WishlistFarmingMissionsDistilledTableBenchmarkTests"` run command and that it is the multi-variant counterpart of `WishlistFarmingMissionsBenchmarkTests`.
- [x] 1.2 Copy the shared helpers from `WishlistFarmingMissionsBenchmarkTests` (`TempDatabasePath`, `Open`, `ScalarLong`, `Median`, `SeedFullImportAsync`, `SeedMaximalWishlist`) so the import + maximal-wishlist setup is byte-for-byte the same.

## 2. Shared timing routine across dedup-source variants

- [x] 2.1 Add one `RunBenchmarkAsync(label, materializeDistilled, swapTarget)` routine: seed the import, optionally materialize the distilled table, build the maximal wishlist, warm up once, time 25 iterations of query-execute + `BuildGroups`, take median/min/max — so every variant differs only by the distilled-table builder and the dedup-source swap.
- [x] 2.2 Parameterize distilled materialization as an optional index-builder delegate (`Action<string>?`); `null` for variants that do not materialize the table, so raw-table and view variants run against the import exactly as produced.

## 3. Distilled-table materialization (distilled variants only)

- [x] 3.1 Share `CreateDistilledTable`: `CREATE TABLE mission_rewards_distilled AS SELECT mission_source_id, rotation_id, reward_item_id, relic_id, SUM(chance_percent) AS chance_percent, MIN(source_order) AS source_order FROM mission_rewards GROUP BY mission_source_id, rotation_id, reward_item_id;` — defining SELECT copied verbatim from the `mission_rewards_merged` view.
- [x] 3.2 `MaterializeDistilledTableComposite`: after the CTAS, create `ix_distilled_relic(relic_id)` and `ix_distilled_join(mission_source_id, rotation_id, relic_id)`, then `ANALYZE`.
- [x] 3.3 `MaterializeDistilledTableSimple`: after the CTAS, create a single-column `ix_distilled_simple(mission_source_id)` only (mirroring the raw table's `IX_mission_rewards_mission_source_id`), then `ANALYZE`.
- [x] 3.4 In the distilled variants, assert `COUNT(mission_rewards_distilled)` equals `COUNT(mission_rewards_merged)` so the comparison is provably on identical dedup output.

## 4. The four variant test cases

- [x] 4.1 `Rundown_MaximalWishlist_RawTable_Timing` — no distilled table; swap `mission_rewards_merged` → `mission_rewards`.
- [x] 4.2 `Rundown_MaximalWishlist_MergedView_Timing` — no distilled table; run the shipped query unchanged (no swap).
- [x] 4.3 `Rundown_MaximalWishlist_DistilledTableCompositeIndex_Timing` — materialize composite-indexed table; swap to `mission_rewards_distilled`.
- [x] 4.4 `Rundown_MaximalWishlist_DistilledTableSimpleIndex_Timing` — materialize simple-indexed table; swap to `mission_rewards_distilled`.

## 5. Query swap, diagnostics, assertions

- [x] 5.1 `SwapMergedViewFor(sql, target)`: load `GetWishlistFarmingMissions.sql` via `SqlQueryLoader`, replace `mission_rewards_merged` → target, and guard that the occurrence count is > 0 (fail loudly if the token is gone).
- [x] 5.2 Execute via `ReliquaryDbContext.Database.SqlQueryRaw<FarmingRow>` then `SqliteWishlistFarmingMissionsReader.BuildGroups`, for parity with the live reader path.
- [x] 5.3 Emit a variant-labelled diagnostic block (mission_rewards rows, distilled count or "not materialized", relic totals/non-vaulted, mission sources, wishlist items, rundown groups, iterations, median, min/max) plus the `EXPLAIN QUERY PLAN` of that variant's query.
- [x] 5.4 Assert each variant's maximal-wishlist rundown returns a non-empty result.

## 6. Verify and record findings

- [x] 6.1 Run all four variants on demand and capture medians + `EXPLAIN QUERY PLAN` for each.
- [x] 6.2 Record the comparison and root-cause analysis in `design.md` (Result section) — raw ~25 ms, distilled+simple ~33 ms, view ~45–53 ms, distilled+composite ~60 ms; composite index's probe-multiplication is the cause of its regression; raw vs distilled+simple plans are line-for-line identical (difference is measurement noise).
- [x] 6.3 Write `findings.md` summarizing the findings and the objections to the shipped view path, with a recommendation (revert to a `mission_source_id`-indexed distilled table) flagged as its own future change.
- [x] 6.4 Confirm the normal test suite is unchanged: `dotnet test` does not pick up the `[Explicit]` benchmark, and no file under `src/` was modified.
