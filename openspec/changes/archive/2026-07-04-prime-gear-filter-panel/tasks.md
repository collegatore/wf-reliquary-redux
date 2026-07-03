## 1. ViewModel — filter panel state and Wishlisted Only filter

- [x] 1.1 Add `IsFilterPanelOpen` bool property to `PrimeGearViewModel` (default false, with `OnPropertyChanged`)
- [x] 1.2 Add `ToggleFilterPanelCommand` (`RelayCommand`) that flips `IsFilterPanelOpen`
- [x] 1.3 Add `ShowWishlistedOnly` bool property to `PrimeGearViewModel` (default false); setter calls `ClearSelection` then `ApplyFilters`
- [x] 1.4 Extend `ApplyFilters` to skip cards where `showWishlistedOnly` is true and the card has no checked parts (apply after the owned-hidden check)

## 2. ViewModel tests — Wishlisted Only filter

- [x] 2.1 Test: when `ShowWishlistedOnly` is false, cards with no checked parts are visible
- [x] 2.2 Test: when `ShowWishlistedOnly` is true, a card with no checked parts is hidden
- [x] 2.3 Test: when `ShowWishlistedOnly` is true, a card with at least one checked part is shown
- [x] 2.4 Test: unchecking `ShowWishlistedOnly` restores previously hidden cards
- [x] 2.5 Test: when `ShowWishlistedOnly` is true and `ShowOwned` is false, an owned card with checked parts is still hidden (owned filter takes precedence)

## 3. XAML — restructure primary control panel

- [x] 3.1 Remove the Show vaulted `CheckBox` row (and its separator) from the primary control panel `StackPanel`
- [x] 3.2 Remove the Show owned `CheckBox` row (and its separator) from the primary control panel `StackPanel`
- [x] 3.3 Add a **Filters** `Button` to the primary control panel (after the group-ops separator), bound to `PrimeGear.ToggleFilterPanelCommand`

## 4. XAML — add filter panel

- [x] 4.1 Add a new `Border` for the filter panel beneath the primary panel `Border`, with `IsVisible` bound to `PrimeGear.IsFilterPanelOpen`
- [x] 4.2 Add a Show vaulted `CheckBox` inside the filter panel, bound to `PrimeGear.ShowVaulted`
- [x] 4.3 Add a Show owned `CheckBox` inside the filter panel, bound to `PrimeGear.ShowOwned`
- [x] 4.4 Add a Wishlisted only `CheckBox` inside the filter panel, bound to `PrimeGear.ShowWishlistedOnly`
