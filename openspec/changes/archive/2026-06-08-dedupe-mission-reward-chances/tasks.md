## 1. Baseline benchmark — BEFORE any change (STOP after)

- [x] 1.1 Seed a **full-size** benchmark DB from the real preserved drop table: read `TestData/warframe-drop-tables-sample.html` from the test project (copied to the test output directory via the csproj `Content` item — resolve as `Path.Combine(AppContext.BaseDirectory, "TestData", "warframe-drop-tables-sample.html")`, not from the `specs/` directory), parse it with `HtmlDropTableParser`, then persist with `SqliteDropTableImportStore.ReplaceAsync`. This is the representative dataset — do not use the 3-relic `BuildImport()` fixture, which is too small to time.
- [x] 1.2 Build the **maximal wishlist**: insert a wishlist item for every distinct `reward_items.normalized_name` that is a relic reward of a non-vaulted relic (query `reward_items → relic_rewards → relics WHERE is_vaulted = 0`). This makes `wishlisted_relic_ids` cover all non-vaulted relics and expands `missions_with_wishlist` to effectively all missions — the worst case for the rundown query.
- [x] 1.3 Add an integration test (category-tagged or `[Explicit]` so it does not run in the normal suite) that warms up once, then times `GetMissionsForWishlistAsync` over several iterations and records the median wall-clock time. Capture `EXPLAIN QUERY PLAN` for the current query for the record. — `WishlistFarmingMissionsBenchmarkTests`, baseline median **36.6 ms** (see report).
- [x] 1.4 **STOP.** Report the baseline median timing (and query plan) and wait for the developer to confirm before implementing the view. — Reported (~37 ms baseline); developer confirmed "proceed as planned".

## 2. Schema: the merged view

- [x] 2.1 Add a new EF migration (`dotnet ef migrations add AddMissionRewardsMergedView`) whose `Up` issues `CREATE VIEW mission_rewards_merged` via `migrationBuilder.Sql`, grouping `mission_rewards` by `mission_source_id, rotation_id, reward_item_id` with `SUM(chance_percent) AS chance_percent`, `MIN(source_order) AS source_order`, and `relic_id` passed through; `Down` issues `DROP VIEW`. Do not hand-edit the init migration (squashing is a one-time V1-prep step).
- [x] 2.2 Apply the migration to the local DB (`dotnet ef database update`) and confirm the view exists and returns merged rows for a known duplicate (e.g. Axi Y1 in Void/Mot).

## 3. Query: read from the view

- [x] 3.1 In `GetWishlistFarmingMissions.sql`, replace each `mission_rewards mr` reference (both `UNION ALL` arms and the `missions_with_wishlist` CTE) with `mission_rewards_merged mr`; remove no other logic — the LEFT JOIN preserving zero-chance rotations stays as-is.
- [x] 3.2 Confirm `SqliteWishlistFarmingMissionsReader` needs no change because it now receives at most one row per (mission, rotation, reward).

## 4. Re-benchmark and compare

- [x] 4.1 Re-run the same benchmark test (same import, same maximal wishlist, same iteration count) against the view-backed query; record the median and the new `EXPLAIN QUERY PLAN`.
- [x] 4.2 Report the before/after comparison. **Result:** worst-case (maximal wishlist, ~10.3k rows) median ~37 ms → ~48–56 ms. Plan shows the view fully re-aggregated per reference (3×). A composite index and a `MATERIALIZED` CTE were both trialled and recovered little (the cost is the table-wide aggregation, not the sort/repetition); both were reverted. Regression only manifests at an unreachable worst case (every part wishlisted); typical wishlists are a few ms. **Decision: accept the plain view** (option 3).

## 5. Correctness tests

- [x] 5.1 Add an integration test for a non-rotation mission listing the same wishlisted relic twice (e.g. 2% and 1%): assert one relic entry at 3% and pool effective chance 3%.
- [x] 5.2 Add a test for a rotation listing the same wishlisted relic twice: assert one relic entry at the summed chance in that rotation's section.
- [x] 5.3 Add a test that two same-type missions whose tables are identical only after duplicate-merge are grouped into a single card.
- [x] 5.4 Add a test asserting the `reward_item_id` ↔ `relic_id` 1:1 invariant the view relies on (no reward item maps to more than one relic).
- [x] 5.5 Confirm the existing zero-chance-rotation test still passes (rotations with no wishlisted reward still shown at 0%).
- [x] 5.6 Run `dotnet test` and confirm the full suite is green.
