## 1. Reorder tabs and set startup default

- [x] 1.1 In `MainWindow.axaml`, reorder the `TabItem`s to: Prime Gear, Relics and Missions, Drop Tables, Settings (so Prime Gear is index 0 and the two menu tabs are last).
- [x] 1.2 Confirm `MainWindowShellViewModel.selectedTabIndex` still defaults to `0` so Prime Gear is the startup tab; update the index-map comment to the new order (Prime Gear=0, Relics and Missions=1, Drop Tables=2, Settings=3).
- [x] 1.3 Verify `SettingsTabIndex` remains `3` in both `MainWindowShellViewModel` and `OnTabSelectionChanged` (`MainWindow.axaml.cs`); confirm the initial `guardedTabIndex` (0) is not the Settings index.

## 2. Move Drop Tables to the File menu

- [x] 2.1 Add a `NavigateToDropTablesCommand` to `MainWindowShellViewModel` that sets `SelectedTabIndex` to the Drop Tables index (2), mirroring `NavigateToSettingsCommand` but without the discard guard.
- [x] 2.2 In `MainWindow.axaml`, add a **Drop Tables** `MenuItem` under the **File** `Menu`, bound to `NavigateToDropTablesCommand`, alongside the existing Settings item.

## 3. Header visibility for menu-accessed tabs

- [x] 3.1 Change the Settings `TabItem` header from `IsVisible="False"` to a `TextBlock` whose `IsVisible` binds to the owning `TabItem`'s `IsSelected` (via `RelativeSource` `AncestorType=TabItem`, or an equivalent `TabItem:selected` style).
- [x] 3.2 Give the Drop Tables `TabItem` the same header treatment so its "Drop Tables" label shows only while the tab is active.
- [x] 3.3 Confirm the Prime Gear and Relics and Missions headers remain plain, always-visible string headers.

## 4. Tests

- [x] 4.1 Add a test asserting the shell selects the Prime Gear tab (`SelectedTabIndex == 0`) on construction / startup.
- [x] 4.2 Add a test asserting `NavigateToDropTablesCommand` sets `SelectedTabIndex` to the Drop Tables index, and `NavigateToSettingsCommand` still selects Settings (3).
- [x] 4.3 Update any existing test or assertion that assumed Drop Tables was the startup/first tab.

## 5. Verify

- [x] 5.1 Run `dotnet build` and `dotnet test`; all green.
- [ ] 5.2 Launch the app: confirm it opens on Prime Gear, the strip shows only Prime Gear and Relics and Missions, File → Drop Tables and File → Settings each surface their tab with its header appearing in the strip, and the header hides again after leaving the tab.
