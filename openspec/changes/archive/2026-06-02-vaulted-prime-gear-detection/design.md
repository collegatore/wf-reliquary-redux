## Context

The prime gear view currently lists all prime gear from the local SQLite database with no distinction between gear whose relics are still in active mission rotations and gear whose relics have been removed (vaulted). The database already stores relics, relic rewards, and mission rewards, so vault status can be derived from existing data without schema changes.

Current data model relevant to vaulting:
- `reward_items` — prime part items (`item_kind = 'PrimePart'`)
- `relic_rewards` — links relics to the parts they reward
- `mission_rewards` — links mission sources to the relics that can drop from them (via `relic_id`)

A relic is *obtainable from a mission* when there exists at least one row in `mission_rewards` where `relic_id` matches the relic's id. A gear is vaulted when no relic that rewards any of its parts appears in `mission_rewards`.

Vault status is stable between drop table imports — it can only change when new drop table data is imported. Computing it at query time on every app startup is therefore unnecessary work. Since vault status is a gear-level concept (a whole gear set is vaulted or not; individual parts of the same gear sharing the same relics don't split), storing one row per gear name is the right granularity.

## Goals / Non-Goals

**Goals:**
- Extend `PrimeGearEntry` with an `IsVaulted` flag derived from the database
- Surface vault status on prime gear cards in the UI (visual indicator)
- Compute vault status once at import time and persist it at gear granularity; reads are cheap lookups

**Non-Goals:**
- Tracking when gear was vaulted or predicting future vault schedules
- User-controlled vault override (marking gear manually)
- Filtering or sorting cards by vault status (display only, for now)

## Decisions

### Separate `gear_vault_status` table, populated at import time via SQL CTE

Add a `gear_vault_status (gear_name TEXT PRIMARY KEY, is_vaulted INTEGER NOT NULL)` table. At the end of `SqliteDropTableImportStore.InsertSnapshotAsync`, after all relics and mission_rewards rows are inserted, populate it with a single CTE-based INSERT:

```sql
WITH gear_names AS (
    SELECT DISTINCT
        TRIM(SUBSTR(name, 1, INSTR(name, ' Prime ') + 5)) AS gear_name
    FROM reward_items
    WHERE item_kind = 'PrimePart'
),
obtainable_gears AS (
    SELECT DISTINCT
        TRIM(SUBSTR(ri.name, 1, INSTR(ri.name, ' Prime ') + 5)) AS gear_name
    FROM reward_items ri
    JOIN relic_rewards rr ON rr.reward_item_id = ri.id
    JOIN mission_rewards mr ON mr.relic_id = rr.relic_id
    WHERE ri.item_kind = 'PrimePart'
)
INSERT INTO gear_vault_status (gear_name, is_vaulted)
SELECT
    g.gear_name,
    CASE WHEN o.gear_name IS NULL THEN 1 ELSE 0 END
FROM gear_names g
LEFT JOIN obtainable_gears o ON o.gear_name = g.gear_name
```

`TRIM(SUBSTR(name, 1, INSTR(name, ' Prime ') + 5))` is the SQL equivalent of the C# `DeriveGearAndPart` gear-name extraction and only needs to be expressed once (at import time). The read path never touches this derivation.

**Alternative considered — correlated subquery at read time:** Run the EXISTS check on every app startup. Rejected because vault status cannot change between imports, so recomputing it on every read is wasted work.

**Alternative considered — `is_vaulted` column on `reward_items`:** Store vault status per prime part row and aggregate at read time with `group.All(x => x.IsVaulted)`. Rejected because vault is a gear-level property; redundantly storing the same flag across all parts of a gear is the wrong granularity, and the gear-level aggregation in C# is unnecessary work that the separate table eliminates.

### `SqlitePrimeGearReader` uses `gear_vault_status` as the join anchor

The reader query joins `gear_vault_status` (one row per gear) with `reward_items` (one row per part) on the derived gear name, so the C# grouping key is `gear_name` from the table rather than recomputed from item names:

```sql
SELECT
    gvs.gear_name,
    ri.name AS item_name,
    gvs.is_vaulted
FROM gear_vault_status gvs
JOIN reward_items ri
    ON ri.item_kind = 'PrimePart'
    AND TRIM(SUBSTR(ri.name, 1, INSTR(ri.name, ' Prime ') + 5)) = gvs.gear_name
ORDER BY gvs.gear_name, ri.name
```

`DeriveGearAndPart` is still used in C# to extract the *part name* from the item name; gear name and vault status come from the table.

### `PrimeGearEntry` record gets `IsVaulted`

Add `bool IsVaulted` as a new property on the existing record. Non-breaking addition; all existing construction sites just need to supply the new value.

### UI — "Vaulted" badge on card header

Display a small "Vaulted" text badge next to the gear name on the card. Apply muted/dimmed opacity to the entire card to help it recede visually behind active gear, without hiding it entirely (users may still want to track parts for vault rotations).

**Alternative considered — separate sections:** Split the list into "Available" and "Vaulted" groups. Deferred; sorting/grouping is out of scope for this change.

## Risks / Trade-offs

- **Stale data**: Vault status reflects the last imported drop table. If the drop table is out of date, vault indicators will be wrong. → Mitigation: existing import-date display already signals staleness; no extra mitigation needed here.
- **Gear with no relics in DB**: If `relic_rewards` has no rows for a gear (data gap), it will be shown as vaulted even if it isn't. → Acceptable for now; treated the same as truly vaulted.
- **SQL gear name expression**: `TRIM(SUBSTR(name, 1, INSTR(name, ' Prime ') + 5))` must stay in sync with C# `DeriveGearAndPart`. Both are simple and unlikely to diverge, but they are two expressions of the same rule. The SQL expression only appears in `InsertSnapshotAsync` and the reader query, so any future change touches two adjacent files.

## Migration Plan

`gear_vault_status` is a new table. `SqliteDropTableImportStore` clears and recreates all rows on every import, so existing databases automatically gain the table on the next import. No separate migration step is needed. `PrimeGearEntry` record change is compile-time-safe; all call sites are in the same solution.
