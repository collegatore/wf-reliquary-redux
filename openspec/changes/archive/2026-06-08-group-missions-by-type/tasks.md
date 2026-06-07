## 1. SQL — Expose mission_type and node

- [x] 1.1 Add `ms.mission_type AS MissionType` and `ms.node AS MissionNode` to the rotation-missions branch of `GetWishlistFarmingMissions.sql`
- [x] 1.2 Add `ms.mission_type AS MissionType` and `ms.node AS MissionNode` to the non-rotation branch of `GetWishlistFarmingMissions.sql`

## 2. Application model — Group entry type

- [x] 2.1 Add `MissionType` (nullable string) and `Node` (nullable string) properties to `WishlistFarmingMissionEntry`
- [x] 2.2 Add `WishlistFarmingMissionGroupEntry` record to the Application layer with: `GroupTitle` (string), `Nodes` (`IReadOnlyList<string>`), `EffectiveChance` (double), `HasRotations` (bool), `Rotations` (`IReadOnlyList<WishlistFarmingRotationEntry>`), `Relics` (`IReadOnlyList<WishlistFarmingRelicEntry>`)
- [x] 2.3 Update `IWishlistFarmingMissionsReader` to return `IReadOnlyList<WishlistFarmingMissionGroupEntry>` instead of `IReadOnlyList<WishlistFarmingMissionEntry>`

## 3. Infrastructure — Reader changes

- [x] 3.1 Add `MissionType` and `MissionNode` properties to `FarmingRow` in `SqliteWishlistFarmingMissionsReader`
- [x] 3.2 Thread `MissionType` and `Node` through `BuildRotationMission` and `BuildNonRotationMission` so each returns an entry that includes them
- [x] 3.3 Implement grouping in `BuildEntries`: compute a canonical reward fingerprint (sorted `"RelicName:ChancePercent"` per rotation) per entry, then group by `(MissionType, fingerprint)`; missions with null `MissionType` form singleton groups
- [x] 3.4 Emit `WishlistFarmingMissionGroupEntry` records from `BuildEntries`, with `GroupTitle` = `MissionType` for typed groups and `DisplayLabel` for null-type singletons, and `Nodes` = list of `Node ?? DisplayLabel` for each member

## 4. Presentation — Card ViewModel

- [x] 4.1 Add `WishlistFarmingMissionGroupCardViewModel` class with properties: `GroupTitle`, `FormattedEffectiveChance`, `HasRotations`, `NodesLabel` (comma-joined node strings), `Rotations` (existing type), `Relics` (existing type), `CompactRelicNames` (comma-joined relic names for non-rotation), and per-rotation `CompactRelicNames` accessible from `WishlistFarmingRotationViewModel`
- [x] 4.2 Add `CompactRelicNames` (string, comma-joined relic names) to `WishlistFarmingRotationViewModel`
- [x] 4.3 Add `ShowExactDropChances` bool property to `WishlistFarmingMissionsViewModel` (default `true`), firing `PropertyChanged`
- [x] 4.4 Update `WishlistFarmingMissionsViewModel.ReloadAsync` to use `WishlistFarmingMissionGroupCardViewModel` and build from `WishlistFarmingMissionGroupEntry`; remove `WishlistFarmingMissionCardViewModel`

## 5. AXAML — Control panel and card template

- [x] 5.1 Add "Show exact drop chances" checkbox to the "Relics and Missions" tab control panel in `MainWindow.axaml`, bound to `WishlistFarming.ShowExactDropChances`
- [x] 5.2 Update the mission card `DataTemplate` in `MainWindow.axaml` to bind to `WishlistFarmingMissionGroupCardViewModel`: change header to `GroupTitle`, add a `TextBlock` for `NodesLabel` below the separator
- [x] 5.3 In the non-rotation relic `ItemsControl`, add an `IsVisible` binding to `WishlistFarming.ShowExactDropChances`; add a sibling `TextBlock` bound to `CompactRelicNames` with `IsVisible` bound to `!WishlistFarming.ShowExactDropChances`
- [x] 5.4 In the rotation card's per-relic `ItemsControl` (inside each rotation's `DataTemplate`), apply the same visibility pair: `ItemsControl` visible when chances on, compact `TextBlock` visible when chances off
- [x] 5.5 Verify that chance columns in the rotation header row (cumulative chance label) remain visible regardless of the toggle (toggle only affects per-relic detail, not rotation-level summary)

## 6. Tests — Update and extend

- [x] 6.1 Update `SqliteWishlistFarmingMissionsReader` unit/integration tests to assert grouping: two missions of same type with identical rewards → one group entry with two nodes
- [x] 6.2 Add test: two missions of same type with different rewards → two separate group entries
- [x] 6.3 Add test: mission with null `MissionType` → singleton group using `DisplayLabel` as title
- [x] 6.4 Update any existing tests that assert on `WishlistFarmingMissionEntry` list shape to use `WishlistFarmingMissionGroupEntry`
