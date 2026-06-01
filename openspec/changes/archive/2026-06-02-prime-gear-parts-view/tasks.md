## 1. Application Port

- [x] 1.1 Add `PrimeGearEntry` record to the application layer with `GearName` and `Parts` fields
- [x] 1.2 Add `IPrimeGearReader` port interface with `GetAllAsync`

## 2. Infrastructure Query

- [x] 2.1 Implement `IPrimeGearReader` in infrastructure using a SQLite query that groups `reward_items` by derived gear name (SUBSTR on ` Prime ` position) filtered to `item_kind = 'PrimePart'`
- [x] 2.2 Register `IPrimeGearReader` in the DI composition root

## 3. Tests

- [x] 3.1 Add unit test verifying gear name and part name are correctly derived from a known item name
- [x] 3.2 Add integration test seeding a set of prime part items and asserting the query returns the expected grouped entries
- [x] 3.3 Add integration test asserting the query returns an empty list when no items are present

## 4. Presentation ViewModels

- [x] 4.1 Add `PrimePartViewModel` with `PartName` and `IsChecked` properties
- [x] 4.2 Add `PrimeGearCardViewModel` with `GearName` and `ObservableCollection<PrimePartViewModel> Parts`
- [x] 4.3 Add `PrimeGearViewModel` that accepts `IPrimeGearReader`, loads entries on construction, and exposes `ObservableCollection<PrimeGearCardViewModel>`

## 5. Presentation UI

- [x] 5.1 Refactor `MainWindow.axaml` to use `TabControl` with a **Drop Tables** tab (existing content) and a **Prime Gear** tab
- [x] 5.2 Implement the Prime Gear tab content: wrapping `ItemsControl` of cards, each card a `Border` with gear name header and checkbox `ItemsControl` for parts
- [x] 5.3 Wire `PrimeGearViewModel` into `MainWindow` construction and bind the Prime Gear tab content to it
