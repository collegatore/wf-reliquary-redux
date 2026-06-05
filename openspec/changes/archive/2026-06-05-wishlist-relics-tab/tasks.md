## 1. Application Layer — Data Model & Port

- [x] 1.1 Create `WishlistRelicReward` record: `(string RewardName, bool IsWishlisted)`
- [x] 1.2 Create `WishlistRelicEntry` record: `(string DisplayName, string Era, bool IsVaulted, IReadOnlyList<WishlistRelicReward> Rewards)`
- [x] 1.3 Create `IWishlistRelicsReader` port interface with `GetRelicsForWishlistAsync(long wishlistId): Task<IReadOnlyList<WishlistRelicEntry>>`

## 2. Infrastructure — Schema & Import

- [x] 2.1 Add `is_vaulted INTEGER NOT NULL DEFAULT 0` column to the `relics` table DDL in `SqliteDropTableImportStore`
- [x] 2.2 After inserting mission_rewards during import, UPDATE `relics SET is_vaulted = 1` for any relic whose `id` does not appear in `mission_rewards.relic_id`

## 3. Infrastructure — SQLite Reader

- [x] 3.1 Create `SqliteWishlistRelicsReader` implementing `IWishlistRelicsReader`
- [x] 3.2 Write SQL query joining `wishlist_items → reward_items → relic_rewards → relics` to find relics containing wishlisted parts, filtering to relics where at least one reward matches a wishlist item; read `relics.is_vaulted` for vault status
- [x] 3.3 Order results: `CASE era WHEN 'Lith' THEN 1 WHEN 'Meso' THEN 2 WHEN 'Neo' THEN 3 WHEN 'Axi' THEN 4 END`, then `display_name ASC`
- [x] 3.4 For each relic, include all rewards (not just the matched ones), marking each `IsWishlisted` by joining against wishlist item normalized names

## 4. Presentation — ViewModels

- [x] 4.1 Create `WishlistRelicRewardViewModel`: `(string RewardName, bool IsWishlisted)`
- [x] 4.2 Create `WishlistRelicCardViewModel`: `(string RelicName, string Era, bool IsVaulted, ObservableCollection<WishlistRelicRewardViewModel> Rewards)`
- [x] 4.3 Create `WishlistRelicsViewModel` with `ObservableCollection<WishlistRelicCardViewModel> Cards`, `bool ShowVaulted` (default false), and `bool IsEmpty`
- [x] 4.4 Implement `LoadAsync(long wishlistId)` on `WishlistRelicsViewModel`: call reader, populate Cards (filtered by `ShowVaulted`), set `IsEmpty`
- [x] 4.5 When `ShowVaulted` changes, re-filter visible cards from the full loaded set (no DB round-trip needed)
- [x] 4.6 Add `WishlistRelicsViewModel` property to `MainWindowShellViewModel`; wire `OnActiveWishlistChanged` → `wishlistRelics.LoadAsync(newWishlistId)`

## 5. Presentation — XAML

- [x] 5.1 Add `WishlistRelicsViewModel` construction in `MainWindow.axaml.cs` and pass to `MainWindowShellViewModel`
- [x] 5.2 Add new `<TabItem Header="Wishlist Relics">` in `MainWindow.axaml` with `DataContext` bound to `WishlistRelics`
- [x] 5.3 Add control panel above cards: `CheckBox` bound to `ShowVaulted`, labeled "Show Vaulted Relics"
- [x] 5.4 Add empty-state placeholder text (e.g., "Add items to your wishlist to see relics here.") visible when `IsEmpty` is true
- [x] 5.5 Add era section headers (Lith / Meso / Neo / Axi) before the first card of each era group — use a `ItemsControl` with a group header template or emit separator items from the ViewModel
- [x] 5.6 Style relic cards: header = relic display name, body = rewards list
- [x] 5.7 Bold wishlisted reward items (`FontWeight="Bold"` when `IsWishlisted`)
- [x] 5.8 Apply `Opacity="0.55"` to vaulted relic cards (matching the existing vaulted gear treatment)

## 6. DI / Registration

- [x] 6.1 Register `IWishlistRelicsReader` → `SqliteWishlistRelicsReader` in the DI container / service registration
