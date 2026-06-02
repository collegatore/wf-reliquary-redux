## 1. ViewModel

- [x] 1.1 Replace `Cards` collection on `PrimeGearViewModel` with `ActiveCards` and `VaultedCards` (`ObservableCollection<PrimeGearCardViewModel>` each)
- [x] 1.2 Add `ShowVaulted` bool property with `INotifyPropertyChanged` notification (default `false`) to `PrimeGearViewModel`
- [x] 1.3 Update `LoadAsync` to populate `ActiveCards` (non-vaulted) and `VaultedCards` (vaulted) separately
- [x] 1.4 Update `ReloadAsync` to clear and repopulate both collections

## 2. UI Layout

- [x] 2.1 Replace the single `ItemsControl`/`WrapPanel` in the Prime Gear tab with a vertical `StackPanel` containing: Active cards `ItemsControl`, separator `Border`, Vaulted cards `ItemsControl`
- [x] 2.2 Bind Active `ItemsControl` to `PrimeGear.ActiveCards`
- [x] 2.3 Implement the separator `Border` with a distinct background, a label (e.g. "Vaulted"), and a `CheckBox` bound two-way to `PrimeGear.ShowVaulted`
- [x] 2.4 Bind Vaulted `ItemsControl` to `PrimeGear.VaultedCards` and bind its `IsVisible` to `PrimeGear.ShowVaulted`
