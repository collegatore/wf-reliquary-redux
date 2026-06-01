## Context

`HtmlDropTableParser.ItemKind` classifies reward items by name pattern alone. Any item whose name contains `" Prime "` or ends with `" Prime Blueprint"` receives `item_kind = 'PrimePart'`. This is incorrect for whole prime items (e.g. `Sagek Prime`) that are awarded directly by missions, not obtained by cracking a relic. Those items should be `'Other'`.

The parser calls `AddRewardItem` from two distinct sections: the relic rewards section (items inside relics) and the mission rewards section (items in mission reward pools). The fix is to pass this section context into classification so `PrimePart` is only assigned in the relic rewards path.

## Goals / Non-Goals

**Goals:**
- `PrimePart` is assigned only to items that appear as relic reward components AND match the prime name pattern.
- Items first seen as mission rewards keep `'Other'` even if their name matches the prime pattern.
- Items first seen as mission rewards but later encountered as relic rewards are promoted to `'PrimePart'` at parse time so the relic reward path takes precedence.

**Non-Goals:**
- No schema changes to SQLite.
- No changes to downstream consumers (`SqlitePrimeGearReader`, `PrimeGearViewModel`).
- No handling of edge cases beyond the HTML parser (e.g. manual `ParsedRewardItem` construction with incorrect kind is out of scope).

## Decisions

### Context flag on `AddRewardItem`

`AddRewardItem` receives a `bool isRelicReward` parameter. When `false` (mission reward context), prime-named items are classified `'Other'`. When `true` (relic reward context), the existing pattern match applies and the item receives `'PrimePart'`.

**Promotion on re-encounter**: if an item was already recorded as `'Other'` but is subsequently encountered in a relic reward, its kind is promoted to `'PrimePart'` by replacing the entry. This handles sources where a mission reward section appears before the relic section in the HTML.

**Alternative considered**: post-processing pass over all items after both sections are parsed, checking which names appear in `relicRewards`. Rejected because it requires holding a separate name set and an extra loop; the flag approach keeps the logic local to `AddRewardItem`.

**Alternative considered**: rename `ItemKind` to a two-parameter overload. Same outcome as the flag; the single method with a named bool reads clearly at the call site.

## Risks / Trade-offs

- [Risk] An item that genuinely is both a mission reward and a relic reward component gets `'PrimePart'` from the relic path regardless of order → Correct behaviour; the relic path is the canonical source of truth for part classification.
- [Risk] Future sources may introduce items whose name pattern matches prime parts but that are only ever mission rewards → They will remain `'Other'` under the new logic, which is the desired outcome.
