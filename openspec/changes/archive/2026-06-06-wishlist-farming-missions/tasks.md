## 1. Application Layer — Domain Models and Port

- [x] 1.1 Create `WishlistFarmingRelicEntry` record (`RelicName`, `ChancePercent`) in `WF.Reliquary.Application/Wishlist/`
- [x] 1.2 Create `WishlistFarmingRotationEntry` record (`RotationDisplayLabel`, `CumulativeChance`, `IReadOnlyList<WishlistFarmingRelicEntry> Relics`) in `WF.Reliquary.Application/Wishlist/`
- [x] 1.3 Create `WishlistFarmingMissionEntry` record (`DisplayLabel`, `EffectiveChance`, `HasRotations`, `IReadOnlyList<WishlistFarmingRotationEntry> Rotations`, `IReadOnlyList<WishlistFarmingRelicEntry> Relics`) in `WF.Reliquary.Application/Wishlist/`
- [x] 1.4 Create `IWishlistFarmingMissionsReader` port in `WF.Reliquary.Application/Ports/` with method `GetMissionsForWishlistAsync(long wishlistId, CancellationToken) → Task<IReadOnlyList<WishlistFarmingMissionEntry>>`

## 2. Infrastructure — SQL Query

- [x] 2.1 Create `GetWishlistFarmingMissions.sql` in `Storage/Queries/`. The query uses two `UNION ALL` branches and a `@wishlist_id` parameter:
  - **Rotation branch**: `reward_rotations` drives the rows; `LEFT JOIN mission_rewards` filtered to wishlisted relic ids so zero-chance rotations appear with NULL relic columns
  - **Non-rotation branch**: `mission_rewards` filtered to missions without any `reward_rotations` rows and to wishlisted relic ids; rotation columns are literal `NULL`
  - Columns: `MissionSourceId`, `MissionDisplayLabel`, `MissionSourceOrder`, `RotationId`, `RotationDisplayLabel`, `RotationSourceOrder`, `RelicName`, `RelicChancePercent`
  - `ORDER BY MissionSourceOrder, RotationSourceOrder, source_order` (stable pre-sort before C# reorders by effective chance)

## 3. Infrastructure — Reader

- [x] 3.1 Create `SqliteWishlistFarmingMissionsReader` implementing `IWishlistFarmingMissionsReader` in `WF.Reliquary.Infrastructure/DropTables/`
- [x] 3.2 Implement `BuildEntries(List<FarmingRow> rows)` static method that:
  - Groups rows by `MissionSourceId`
  - For rotation missions (any row has non-null `RotationId`): groups sub-rows by `RotationId`, sums `RelicChancePercent` per rotation (treating NULL relic rows as 0), sets `EffectiveChance = rotations.Average(r => r.CumulativeChance)`, populates `Rotations`, leaves mission-level `Relics` empty
  - For non-rotation missions: sums all `RelicChancePercent` rows into mission-level `Relics`, sets `EffectiveChance = sum`, leaves `Rotations` empty
  - Sorts the result descending by `EffectiveChance`
- [x] 3.3 Register `SqliteWishlistFarmingMissionsReader` as `IWishlistFarmingMissionsReader` in `ReliquaryServiceFactory`

## 4. Presentation — View Models

- [x] 4.1 Create `WishlistFarmingMissionRelicViewModel` (properties: `RelicName`, `FormattedChance` e.g. `"11.11%"`) in `WF.Reliquary.Presentation/`
- [x] 4.2 Create `WishlistFarmingRotationViewModel` (properties: `RotationLabel`, `FormattedCumulativeChance`, `ObservableCollection<WishlistFarmingMissionRelicViewModel> Relics`) in `WF.Reliquary.Presentation/`
- [x] 4.3 Create `WishlistFarmingMissionCardViewModel` (properties: `DisplayLabel`, `FormattedEffectiveChance`, `HasRotations`, `ObservableCollection<WishlistFarmingRotationViewModel> Rotations`, `ObservableCollection<WishlistFarmingMissionRelicViewModel> Relics`) in `WF.Reliquary.Presentation/`
- [x] 4.4 Create `WishlistFarmingMissionsViewModel` implementing `INotifyPropertyChanged` with:
  - Constructor takes `IWishlistFarmingMissionsReader` and `IWishlistStore`
  - `ObservableCollection<WishlistFarmingMissionCardViewModel> Cards`
  - `bool IsEmpty` (notifying property)
  - `Task ReloadAsync()` — mirrors structure of `WishlistRelicsViewModel.ReloadAsync`

## 5. Presentation — AXAML

- [x] 5.1 Rename the tab header from "Wishlist Relics" to "Relics and Missions" in the main window AXAML; update the Wishlist tab AXAML to a two-column `Grid` (`* *`); wrap the existing relic cards `ScrollViewer` in the left column; add a new right column
- [x] 5.2 Add the farming missions panel in the right column: a `ScrollViewer` containing an `ItemsControl` bound to `WishlistFarmingMissionsViewModel.Cards`, with a placeholder visible when `IsEmpty = true`
- [x] 5.3 Create the `DataTemplate` for `WishlistFarmingMissionCardViewModel`: card header with `DisplayLabel` and `FormattedEffectiveChance`; conditional content via `HasRotations` — rotation sections (`WishlistFarmingRotationViewModel`) each showing rotation label, cumulative chance, and relic list when `HasRotations = true`; flat relic list when `HasRotations = false`; follow existing card visual style (border, header separator, tier-group indent)
- [x] 5.4 Wire `WishlistFarmingMissionsViewModel` into the Wishlist tab's code-behind or parent view model; ensure `ReloadAsync` is called when the active wishlist changes (matching how `WishlistRelicsViewModel.ReloadAsync` is triggered)

## 6. Tests

- [x] 6.1 Add integration tests for `SqliteWishlistFarmingMissionsReader`: verify non-rotation mission effective chance equals pool sum; verify rotation mission effective chance equals average across all rotations including zero-chance; verify ordering is descending by effective chance
- [x] 6.2 Add unit tests for `BuildEntries`: test grouping of rotation vs non-rotation rows, zero-chance rotation inclusion, and sort order
