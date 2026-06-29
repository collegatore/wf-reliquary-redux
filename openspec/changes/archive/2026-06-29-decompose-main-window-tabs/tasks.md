## 1. Extract DropTablesView (no code-behind)

- [x] 1.1 Create `DropTablesView.axaml` (+ `.axaml.cs` `UserControl`) and move the Drop
  Tables tab content (MainWindow.axaml lines ~756–820) into it, preserving all bindings.
- [x] 1.2 Replace the Drop Tables `TabItem` content in `MainWindow.axaml` with
  `<local:DropTablesView/>`, keeping the existing custom `TabItem.Header` markup.
- [x] 1.3 Build and run the app; confirm the Drop Tables tab renders and all commands
  (download/read/import/clear/export) bind and work as before.

## 2. Extract PrimeGearView (flyout wiring)

- [x] 2.1 Create `PrimeGearView.axaml` (+ `.axaml.cs`) and move the Prime Gear tab
  content (MainWindow.axaml lines ~129–444) into it, including the `WishlistDropdownButton`.
- [x] 2.2 In `PrimeGearView.axaml.cs`, wire `shell.Wishlist.FlyoutCloseRequested` to
  `WishlistDropdownButton.Flyout?.Hide()` in `OnDataContextChanged`, mirroring
  `SettingsView`. Remove the equivalent wiring from `MainWindow` constructor (line ~206).
- [x] 2.3 Replace the Prime Gear `TabItem` content with `<local:PrimeGearView/>`.
- [x] 2.4 Build and run; confirm Prime Gear renders, the wishlist dropdown opens/closes,
  owned-gear fade and search behave as before.

## 3. Extract RelicsMissionsView (splitter code-behind)

- [x] 3.1 Create `RelicsMissionsView.axaml` (+ `.axaml.cs`) and move the Relics and
  Missions tab content (MainWindow.axaml lines ~445–755), keeping `RelicsMissionsGrid`,
  `RelicsScrollViewer`, and `RelicsSplitter` named elements.
- [x] 3.2 Move all splitter/min-size code-behind into `RelicsMissionsView.axaml.cs`:
  `splitterInitialized`, `OnRelicsGridFirstMeasured`, `OnSplitterDragCompleted`,
  `OnShellPropertyChanged` (RelicsPanelHeight branch), `OnWishlistRelicsPropertyChanged`,
  `ApplyRelicsPanelHeight`, `SetRelicsRowHeight`, `AvailableRelicsHeight`,
  `UpdateRelicsMinHeight`.
- [x] 3.3 Wire the view's own events (`SizeChanged` one-shot, splitter `DragCompleted`,
  `shell.PropertyChanged`, `shell.WishlistRelics.PropertyChanged`) from the view using
  the `DataContextChanged`/`Loaded` pattern; ensure unsubscribe/re-wire safety.
- [x] 3.4 Remove the moved members and their wiring from `MainWindow.axaml.cs` (`Wire()`).
- [x] 3.5 Replace the Relics and Missions `TabItem` content with
  `<local:RelicsMissionsView/>`.
- [x] 3.6 Build and run; confirm first-show sizing (default 1:2), splitter drag, min-
  height vs tallest card, Show-all-rewards re-measure, and splitter persistence across
  restart all behave as before.

## 4. Trim MainWindow and verify

- [x] 4.1 Confirm `MainWindow.axaml.cs` retains only: composition root, window size/
  position restore + save (incl. `OnWindowClosing` reading `shell.RelicsPanelHeight`),
  toast/error reporter, footer spinner, and the Settings tab-guard.
- [x] 4.2 Confirm `MainWindow.axaml` retains only chrome (DockPanel, menu, footer, toast
  overlay) and the `TabControl` with four `<local:XView/>` references.
- [x] 4.3 Audit for named-element references that moved scope (`FindControl`/`x:Name`
  lookups) in code and tests; update any that broke.
- [x] 4.4 Run the full build and the UI/integration test suite; fix any breakage so all
  tests pass with no behavior change.
- [x] 4.5 Sync the `main-window-tab-views` delta spec to main specs and archive the change.
