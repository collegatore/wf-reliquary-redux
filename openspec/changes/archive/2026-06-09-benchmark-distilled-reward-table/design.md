## Context

The shipped dedup design (`openspec/changes/archive/2026-06-08-dedupe-mission-reward-chances/design.md`) expresses reward merging as the view `mission_rewards_merged` and reads it three times from `GetWishlistFarmingMissions.sql`. Its `[Explicit]` benchmark, `WishlistFarmingMissionsBenchmarkTests`, timed the maximal-wishlist rundown against the real bundled drop table (~10,319 `mission_rewards` rows) and found the plain view regressed the worst case from ~37 ms to ~48–56 ms, with neither a composite index nor a `MATERIALIZED` CTE *on the view* recovering it — because the dominant cost is re-aggregating the whole table on every reference.

The variant never measured is a physically materialized distilled table populated once after import and indexed for the rundown's join/membership pattern, so the rundown does indexed lookups instead of table-wide aggregation. This change adds a sibling benchmark that measures exactly that, apples-to-apples with the view benchmark.

Constraints carried from the shipped design:
- The dataset is tiny; the benchmark exists to expose the worst case (maximal wishlist), not a realistic one.
- The merge grain is `(mission_source_id, rotation_id, reward_item_id)` with `relic_id` as a functionally-dependent passthrough and `MIN(source_order)` for stable ordering.
- Pre-V1 rules apply, but this change touches no schema or migration — the distilled table lives only inside the test database.

## Goals / Non-Goals

**Goals:**
- Produce a directly comparable median timing for the distilled-table variant under the same worst case as the view benchmark.
- Keep everything except the dedup source identical: same import, same maximal wishlist, same query structure, same grouping path, same warm-up and iteration routine, same diagnostics.
- Index the distilled table the way you actually would in production, so the number reflects the table approach at its best.

**Non-Goals:**
- No production code, schema, migration, or shipped-query change. The live view and reader stay as-is.
- No decision to adopt the table in this change — this is measurement only. Acting on the result is a follow-up.
- No micro-optimization sweep of index permutations; pick the index set the rundown's access pattern obviously wants and measure that.

## Decisions

### Decision: Materialize the distilled table inside the benchmark, not via a migration

The benchmark, after `SqliteDropTableImportStore.ReplaceAsync`, issues raw SQL on the test database to build `mission_rewards_distilled`:

```sql
CREATE TABLE mission_rewards_distilled AS
SELECT mission_source_id, rotation_id, reward_item_id, relic_id,
       SUM(chance_percent) AS chance_percent, MIN(source_order) AS source_order
FROM mission_rewards
GROUP BY mission_source_id, rotation_id, reward_item_id;
```

Same defining SELECT as the view, so the two variants dedup identically and only differ in storage. Doing it in-test (raw `SqliteCommand` / `ExecuteSqlRaw`) rather than as an EF migration keeps `src/` and the model snapshot untouched — this is an experiment, and the shipped design explicitly rejected adding a materialized table to production. **Alternative considered:** a real migration + entity. Rejected: it would alter shipped schema for a measurement, and `CREATE TABLE … AS SELECT` needs no entity mapping.

### Decision: Index for the rundown's access pattern, build indexes after the bulk insert

`GetWishlistFarmingMissions.sql` touches the dedup source in three shapes: `missions_with_wishlist` filters by `relic_id IN (...)`; the rotation `LEFT JOIN` matches `(mission_source_id, rotation_id, relic_id)`; the non-rotation `JOIN` matches `(mission_source_id, relic_id, rotation_id IS NULL)`. So create:

- `CREATE INDEX ix_distilled_relic ON mission_rewards_distilled(relic_id);` — for the membership subqueries.
- `CREATE INDEX ix_distilled_join ON mission_rewards_distilled(mission_source_id, rotation_id, relic_id);` — covering the join predicates.

Indexes are created *after* the `CREATE TABLE … AS SELECT` so the bulk load isn't paid per-row, then `ANALYZE mission_rewards_distilled` so SQLite's planner has stats (the view has none to give). **Alternative considered:** mirror the shipped composite `(mission_source_id, rotation_id, reward_item_id)`. Rejected as the primary index because the rundown joins on `relic_id`, not `reward_item_id`; the `reward_item_id` ordering helped the view only by avoiding a temp B-tree during aggregation, which a materialized table doesn't do at read time.

### Decision: Run a swapped copy of the shipped query, time the raw query path

The benchmark loads `GetWishlistFarmingMissions.sql` via `SqlQueryLoader`, string-replaces `mission_rewards_merged` → `mission_rewards_distilled`, and executes that against the test DB. `SqliteWishlistFarmingMissionsReader` is hard-wired to the shipped SQL file, so it can't be reused verbatim for the swapped query; the benchmark therefore runs the query itself (via `ReliquaryDbContext.Database.SqlQueryRaw<FarmingRow>` or a raw reader) and, for parity with the live path, feeds the rows through `SqliteWishlistFarmingMissionsReader.BuildGroups` (already `internal`-visible to the test project) so the timed work matches the view benchmark's reader work. The timing brackets the same span the view benchmark does: query execution plus group building. **Alternative considered:** add a `tableName` parameter to the production reader. Rejected: changes shipped code for a throwaway experiment; the string-swap keeps the query definitionally identical.

### Decision: Emit the same diagnostics plus the distilled row count

Print the identical block the view benchmark prints (mission_rewards rows, relic counts, mission sources, wishlist items, rundown groups, iterations, median, min/max, `EXPLAIN QUERY PLAN`) and add `mission_rewards_distilled` row count, so a reader can drop the two outputs side by side. The `EXPLAIN QUERY PLAN` is run on the swapped query and should show index lookups against the distilled table rather than the view's table-wide `SCAN … USING ... GROUP BY`.

## Risks / Trade-offs

- **Distilled rows differ from view rows** → If the `CREATE TABLE … AS SELECT` drifts from the view definition the comparison is invalid. Mitigation: copy the view's defining SELECT verbatim (same columns, same `GROUP BY`, same `SUM`/`MIN`), and assert the distilled row count equals the view's row count in the benchmark before timing.
- **Reader can't be reused for the swapped table** → Timing only the raw query would undercount versus the view benchmark, which times the full reader. Mitigation: call `BuildGroups` on the fetched rows inside the timed region so both benchmarks measure query + grouping.
- **Planner uses no stats without `ANALYZE`** → A fresh table with indexes but no stats can be planned pessimistically, understating the table's real performance. Mitigation: run `ANALYZE` after index creation, before warm-up.
- **One-off measurement, easy to bit-rot** → The benchmark depends on the shipped query's `mission_rewards_merged` token and the bundled sample. Mitigation: `[Explicit]`, documented run command in the class summary, and a guard that the string-replace actually matched (replaced count > 0) so a future rename fails loudly instead of silently benchmarking the view.

## Migration Plan

No deployment. Add one `[Explicit]` test file; it is inert until invoked with `dotnet test --filter "FullyQualifiedName~WishlistFarmingMissionsDistilledTableBenchmarkTests"`. Rollback is deleting the file. The shipped view, query, reader, and schema are untouched, so there is nothing to migrate or revert in production.

## Result (first run)

The fixture runs three variants under identical conditions — same import, same maximal wishlist, same warm-up, same 25-iteration median, same grouping path — differing only in which dedup source the rundown reads (and the distilled table is materialized for its variant only). Bundled sample: 10,319 `mission_rewards` rows → 9,873 distilled rows (identical count to the `mission_rewards_merged` view), maximal wishlist of 129 parts, 101 rundown groups.

| Variant | Median | min / max |
|---|---|---|
| Raw `mission_rewards` table (no dedup layer; query swapped to raw table) | ~25 ms | 21 / 34 |
| Distilled table + **simple** index on `mission_source_id` only | ~33 ms | 27 / 40 |
| `mission_rewards_merged` view (**shipped**; query read unchanged) | ~45–53 ms | 38 / 66 |
| Distilled table + **composite** `(mission_source_id, rotation_id, relic_id)` index + `ANALYZE` | ~61 ms | 51 / 77 |

(For cross-reference, the archived view benchmark on its hardware reported ~37 ms baseline and ~48–56 ms for the view. Absolute numbers are machine-dependent; only the within-run ordering is comparable.)

**The composite index is the cause, proven by the simple-index variant.** The two distilled variants differ only in their index — same table, same data, same query. Swapping the composite `(mission_source_id, rotation_id, relic_id)` index for a single-column `mission_source_id` index (the same shape the raw table carries) nearly halved the median (~61 ms → ~33 ms) and put the distilled table back in raw-table territory. The simple-index plan does `SEARCH mr USING INDEX ix_distilled_simple (mission_source_id=?)` — one probe per mission×rotation with relic membership resolved as a residual filter — instead of the composite plan's `SEARCH … (mission_source_id=? AND rotation_id=? AND relic_id=?)` nested under `SCAN wishlisted_relic_ids`, i.e. one probe per wishlisted relic. The "more precise" composite index is exactly what tempts the planner into the probe-multiplied plan at the maximal wishlist.

The distilled+simple-index and raw plans are **line-for-line identical** (only the index name differs), so the small residual difference between them is constant-factor measurement noise, not a real storage penalty — the ordering flips between runs (raw has been observed at 25 ms and 42 ms; distilled+simple at 33–35 ms), and the distilled table even has fewer rows (9,873 vs 10,319). The only large, stable, plan-driven gap is the composite index's probe multiplication; everything else lives inside the run-to-run variance of sub-50 ms timings on this tiny dataset.

**Why the raw table beats the indexed distilled table (~2×), from the plans:** the rundown's hot loop is the rotation `LEFT JOIN`, which matches `mr` on `mission_source_id`/`rotation_id` with `relic_id IN (wishlisted set)`. On the raw table the only usable index is single-column `IX_mission_rewards_mission_source_id`, so SQLite does **one** index probe per mission×rotation and resolves the relic membership as a cheap residual filter over that mission's handful of rows. On the distilled table the composite `ix_distilled_join (mission_source_id, rotation_id, relic_id)` lets the planner push `relic_id` into the seek, so it loops the wishlisted-relic set and does **one probe per wishlisted relic** — up to ~34 probes per mission×rotation at the maximal wishlist (visible as `SCAN wishlisted_relic_ids` nested inside the `SEARCH mr … relic_id=?`). The "more precise" composite index multiplies probe count by the size of the relic set, which is exactly what the maximal wishlist maximizes. The view (~53 ms) lands near the distilled table (~58 ms) for an unrelated reason — it re-runs the full `GROUP BY` aggregation three times (`MATERIALIZE mission_rewards_merged` ×2 plus the CTE). Different tax, similar total. The ordering inverts for realistic wishlists: with few wishlisted relics the probe multiplier is ~1 and all three collapse to a few milliseconds. The takeaway holds — table materialization does not recover the worst-case regression, the worst case is unreachable in practice, and anything keyed on `relic_id` is pessimal there. This benchmark supplies the previously-missing data point; whether to adopt the table is a separate decision.

## Open Questions

None. The result of this benchmark feeds a separate, later decision about whether to replace the view with a distilled table; that decision is out of scope here.
