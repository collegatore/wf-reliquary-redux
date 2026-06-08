## Why

Some mission reward tables list the same relic more than once with separate drop chances (e.g. Axi Y1 appears twice in Void/Mot). The farming-missions rundown currently renders each occurrence as its own relic row, so the same relic shows up multiple times in a card and the reward fingerprint used for mission grouping is computed off the un-merged rows. The user expects one row per relic, with the duplicate chances summed.

## What Changes

- In the farming-missions rundown, occurrences of the **same relic within the same mission reward pool / rotation** are collapsed into a single relic entry whose chance is the **sum** of the duplicate chances.
- Deduplication is applied before per-relic display, before cumulative/effective-chance computation, and before the reward fingerprint that drives mission-type grouping, so duplicate-but-equivalent tables continue to group together.
- The merge is expressed as a SQL view `mission_rewards_merged` (grouping on `mission_source_id, rotation_id, reward_item_id`, summing chance), which the rundown query reads in place of `mission_rewards`. The view stores no data and is keyed on the universal `reward_item_id` grain so a future non-relic resource rundown (Aya, Endo) can reuse it.
- `mission_rewards` continues to store the raw, un-deduplicated rows as the single source of truth and manual cross-validation table; the merged form is computed by the view, not materialized.
- No change to the relic-cards view, prime-gear view, or any chance shown outside the farming-missions rundown.

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities
- `wishlist-farming-missions-view`: A relic that appears multiple times in a mission's reward pool or in a single rotation SHALL be shown as one relic entry with the summed chance; effective-chance and reward-fingerprint computations operate on the deduplicated entries.

## Impact

- **Schema**: a new EF Core migration adding `CREATE VIEW mission_rewards_merged` (`migrationBuilder.Sql`). No new table, no import-code change; `mission_rewards` is written exactly as today.
- **Query**: `GetWishlistFarmingMissions.sql` reads `FROM mission_rewards_merged` instead of `mission_rewards`. `SqliteWishlistFarmingMissionsReader` is unchanged — it now receives already-merged rows.
- **Tests**: `WishlistFarmingMissionsInfrastructureTests` (duplicate-reward case, merged-tables-group-together case) plus a test asserting the `reward_item_id` ↔ `relic_id` 1:1 invariant the view relies on.
- No user-facing settings or UI layout change.
