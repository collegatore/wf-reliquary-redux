## Context

The farming missions panel is built as a pipeline: SQL query → `BuildEntries` in `SqliteWishlistFarmingMissionsReader` → `WishlistFarmingMissionEntry` records → `WishlistFarmingMissionCardViewModel` instances → AXAML `DataTemplate`. The SQL query returns `mission_sources` rows but does not currently expose `mission_type` or `node`, so neither is available to the reader for grouping. Cards are one-to-one with mission sources.

`WishlistFarmingMissionsViewModel` currently exposes no display-mode state; all display decisions are baked into the card VMs.

## Goals / Non-Goals

**Goals:**
- Group same-type missions with identical reward tables into one card; surface mission node labels as a list within the card.
- Add `ShowExactDropChances` toggle to `WishlistFarmingMissionsViewModel` and wire it to the control panel and card template.
- Keep grouping and toggle entirely in the presentation + infrastructure layers; no new application ports or public contracts needed beyond the richer data model.

**Non-Goals:**
- Persisting the `ShowExactDropChances` toggle across sessions (opt-in, not in scope here).
- Changing how grouping affects sorting (groups are still ordered by effective chance descending).
- Altering the rotation mission card layout beyond the toggle.

## Decisions

### Decision 1 — Return groups from the reader, not raw entries

**Options considered:**
- A. Keep `IWishlistFarmingMissionsReader` returning `IReadOnlyList<WishlistFarmingMissionEntry>` and do grouping in the VM.
- B. Return `IReadOnlyList<WishlistFarmingMissionGroupEntry>` directly from the reader.

**Chosen: B.** The grouping key depends on the full relic-chance structure, which is already assembled inside `BuildEntries`. Doing it there avoids leaking raw SQL row structure upward and keeps the VM simple. The interface has exactly one implementation and one consumer (the VM), so changing the return type is a clean, low-risk refactor.

### Decision 2 — Grouping key is (MissionType, canonical-reward-fingerprint)

The canonical fingerprint for a non-rotation mission: sorted list of `"RelicName:ChancePercent"` strings joined with `|`. For a rotation mission: sorted rotations by label, each rotation contributing `"RotationLabel:[relic1:chance1|relic2:chance2]"`. Two missions are grouped only when their fingerprints are equal. Missions with `null` MissionType form singleton groups and keep the existing `DisplayLabel` as the group title.

**Rationale:** Warframe missions of the same type commonly share identical reward tables, but this is not guaranteed (e.g., Steel Path variants). Fingerprinting by actual rewards is stricter and correct; grouping by type alone would be wrong when reward tables differ.

### Decision 3 — SQL change: add mission_type and node to both SELECT branches

Both UNION branches already alias columns for the reader's `FarmingRow`. Adding `ms.mission_type AS MissionType` and `ms.node AS MissionNode` to both branches is the minimal change that makes the reader grouping-capable without altering the overall query structure or join graph.

### Decision 4 — ShowExactDropChances is a plain property on the VM, not a setting

The toggle is tab-local UI state: a `bool` property on `WishlistFarmingMissionsViewModel` firing `PropertyChanged`. The card VMs do not hold a reference to it; the AXAML binds card chance-column visibility directly to the parent VM via `{Binding DataContext.WishlistFarming.ShowExactDropChances, RelativeSource={RelativeSource AncestorType=...}}` or equivalent Avalonia ancestor binding. This avoids threading a reference through each card VM.

**Alternative considered:** pass `ShowExactDropChances` as a constructor parameter to each card VM and rebuild on toggle. Rejected — requires rebuilding the entire card collection on every toggle, which is heavy for a pure display concern.

### Decision 5 — Compact relic row renders names comma-separated in a single TextBlock

When `ShowExactDropChances` is off, the relic `ItemsControl` (one row per relic) is replaced by a single `TextBlock` whose `Text` is computed in the card VM as the comma-joined list of relic names. This avoids an `IValueConverter` and keeps AXAML clean.

**For rotation missions:** each rotation still shows its label row, but the relic sub-list is replaced by the single compact `TextBlock`.

## Risks / Trade-offs

- **Fingerprint brittleness** → Chances are stored as `double`; floating-point formatting must be consistent (e.g., `R6` format string) to avoid spurious mismatches. Mitigation: use a deterministic `ToString("R")` or round to 4 dp before concatenating.
- **Null MissionType** → Missions imported before the field was populated, or non-standard missions, may have null type. Mitigation: singleton group with `DisplayLabel` as title, tested explicitly.
- **Ancestor binding in Avalonia** → `RelativeSource AncestorType` works in Avalonia but requires the correct type reference. Mitigation: bind `ShowExactDropChances` to a computed property on each card VM that the parent VM updates, or use a simpler approach of passing the value as a property and refreshing on change. Final AXAML approach to be confirmed during implementation.
