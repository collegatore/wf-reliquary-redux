## ADDED Requirements

### Requirement: Reward-dedup rundown benchmark across dedup-source variants

The repository SHALL include an `[Explicit]` integration benchmark that measures the worst-case wishlist farming-missions rundown across multiple reward-deduplication sources under identical conditions, so a materialized distilled table can be compared apples-to-apples against the shipped `mission_rewards_merged` view (and against the raw, un-deduplicated table as a floor). The benchmark SHALL be excluded from the normal test suite and runnable on demand by name filter.

#### Scenario: All variants share one timing routine

- **WHEN** any variant runs
- **THEN** it seeds a full-size import from the bundled `TestData/warframe-drop-tables-sample.html` via `SqliteDropTableImportStore`, builds the maximal wishlist (every non-vaulted relic part), performs one warm-up call, then times 25 iterations and reports median, min, and max — the only differences between variants being whether (and how) the distilled table is materialized and which table/view the rundown query reads.

#### Scenario: Four dedup-source variants are measured

- **WHEN** the benchmark fixture executes
- **THEN** it provides four `[Explicit]` test cases over the same routine:
  - the raw, un-deduplicated `mission_rewards` table (query swapped to the raw table; no distilled table created),
  - the shipped `mission_rewards_merged` view (query read from file unchanged),
  - a materialized `mission_rewards_distilled` table indexed with the composite `(mission_source_id, rotation_id, relic_id)` index plus a `relic_id` index,
  - a materialized `mission_rewards_distilled` table indexed with a single-column `mission_source_id` index only.

#### Scenario: Distilled table is materialized only for its own variants

- **WHEN** a distilled-table variant runs
- **THEN** it builds a physical `mission_rewards_distilled` table holding one row per `(mission_source_id, rotation_id, reward_item_id)` with `SUM(chance_percent)` as `chance_percent`, `MIN(source_order)` as `source_order`, and `relic_id` carried as a passthrough — using the exact defining SELECT of the `mission_rewards_merged` view — then creates that variant's indexes and runs `ANALYZE` before any timed iteration
- **AND** the raw-table and view variants do NOT materialize the distilled table, so each runs against the import exactly as produced.

#### Scenario: Distilled row count matches the view

- **WHEN** a distilled-table variant materializes its table
- **THEN** it asserts `COUNT(mission_rewards_distilled)` equals `COUNT(mission_rewards_merged)` so the comparison is provably on identical dedup output.

#### Scenario: Rundown query targets the selected dedup source

- **WHEN** a timed iteration executes the rundown
- **THEN** it runs a copy of `GetWishlistFarmingMissions.sql` identical to the shipped query except that every reference to `mission_rewards_merged` is swapped for the variant's target table (or left unchanged for the view variant), guarding that the swap token actually matched so the benchmark can never silently measure the view by accident
- **AND** the rows are shaped through the same `SqliteWishlistFarmingMissionsReader.BuildGroups` path the live reader uses, so the measured cost differs only by the dedup-source swap.

#### Scenario: Diagnostics are emitted for comparison

- **WHEN** a variant finishes timing
- **THEN** it writes the row counts (`mission_rewards`, `mission_rewards_distilled` or "not materialized", relics, mission sources, wishlist items, rundown groups), the iteration count, the timing summary, and the `EXPLAIN QUERY PLAN` of that variant's query to the test output under a variant-labelled header
- **AND** it asserts the maximal-wishlist rundown returns a non-empty result.

#### Scenario: Shipped behavior is untouched

- **WHEN** the change is applied
- **THEN** no production code, schema, migration, or shipped query is modified — `mission_rewards`, the `mission_rewards_merged` view, `GetWishlistFarmingMissions.sql`, and `SqliteWishlistFarmingMissionsReader` are unchanged, and any distilled table exists only within a benchmark's database.
