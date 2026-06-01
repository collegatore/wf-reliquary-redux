## Why

Items like `Sagek Prime` are whole prime weapons that appear as mission rewards, not relic reward components. The current parser classifies any item with `" Prime "` in its name as `PrimePart` regardless of context, causing these items to appear in the Prime Gear parts view incorrectly.

## What Changes

- **BREAKING**: `item_kind = 'PrimePart'` is now only assigned to items that appear as relic rewards (inside a relic's reward pool) AND match the prime name pattern. Items that only appear as mission rewards retain `item_kind = 'Other'` even if their name contains `" Prime "`.
- If an item is first encountered as a mission reward and later found inside a relic, its kind is promoted to `PrimePart` at parse time.
- The `ItemKind` classification function gains a context parameter indicating whether the item is being added from the relic rewards section.

## Capabilities

### New Capabilities

<!-- None -->

### Modified Capabilities

- `drop-table-parsing-import`: The requirement for how `item_kind` is assigned to reward items changes — `PrimePart` now requires relic reward context in addition to a name pattern match.

## Impact

- **`HtmlDropTableParser`**: `ItemKind` and `AddRewardItem` gain a relic-reward context flag; items can be promoted from `Other` to `PrimePart` on subsequent relic reward encounters.
- **`SqliteDropTableImportStore`**: No schema changes — `item_kind` values and column are unchanged.
- **`PrimeGearViewModel` / `SqlitePrimeGearReader`**: No changes needed; they already filter on `item_kind = 'PrimePart'` and will simply stop seeing incorrectly classified items.
- **Tests**: Parser tests need cases covering prime-named mission-only rewards and promotion from mission to relic reward context.
