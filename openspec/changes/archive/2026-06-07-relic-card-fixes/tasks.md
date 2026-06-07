## 1. ViewModel changes

- [x] 1.1 Add `ShowWishlistedStyle` property to `WishlistRelicRewardViewModel` — constructor receives `bool showAllRewards` and stores `IsWishlisted && showAllRewards`
- [x] 1.2 Add `IsLast` property (with `internal set`) to `WishlistRelicTierGroupViewModel`
- [x] 1.3 Update `WishlistRelicCardViewModel` constructor to set `IsLast = true` on the final element of `TierGroups` after building the list

## 2. AXAML changes

- [x] 2.1 In `MainWindow.axaml`, change reward `TextBlock` binding from `Classes.wishlisted="{Binding IsWishlisted}"` to `Classes.wishlisted="{Binding ShowWishlistedStyle}"`
- [x] 2.2 In `MainWindow.axaml`, add `IsVisible="{Binding !IsLast}"` to the `<Separator>` inside the `WishlistRelicTierGroupViewModel` DataTemplate

## 3. Constructor wiring

- [x] 3.1 Update `WishlistRelicsViewModel.RebuildCards()` to pass `showAllRewards` when constructing each `WishlistRelicRewardViewModel` (or thread it through the card constructor — whichever approach was chosen in design)
