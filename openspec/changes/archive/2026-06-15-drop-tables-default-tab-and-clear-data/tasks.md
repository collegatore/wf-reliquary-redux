## 1. Application Layer — Clear Operation

- [x] 1.1 Add `ClearAsync(CancellationToken ct = default)` to `IDropTableImportStore`
- [x] 1.2 Implement `ClearAsync` in `SqliteDropTableImportStore`: delete all import tables in FK order (same sequence as the delete block in `ReplaceAsync`) inside a single transaction

## 2. Presentation — ViewModel

- [x] 2.1 Add `bool HasImportedData` property to `MainWindowViewModel` (set in `RefreshImportStatusAsync` when version is non-null)
- [x] 2.2 Add `Func<Task<bool>>? ConfirmClear` callback property to `MainWindowViewModel`
- [x] 2.3 Add `ClearImportCommand` to `MainWindowViewModel`: invokes `ConfirmClear` first (aborts if unset or returns false), then delegates to `IDropTableImportStore.ClearAsync`, refreshes import status, and raises `OnImportCleared`; enabled only when `HasImportedData && !IsBusy`
- [x] 2.4 Update `IsBusy` setter in `MainWindowViewModel` to call `ClearImportCommand.RaiseCanExecuteChanged()`
- [x] 2.5 Add `Func<Task>? OnImportCleared` callback property to `MainWindowViewModel`
- [x] 2.6 Add `InitializeAsync()` to `MainWindowShellViewModel`: awaits `DropTables.LoadStatusAsync()`, then sets `SelectedTabIndex = DropTablesTabIndex` when `DropTables.HasImportedData` is false
- [x] 2.7 Wire `dropTables.OnImportCleared` in `MainWindowShellViewModel` constructor to reload Prime Gear, Wishlist Relics, and Wishlist Farming (same body as `OnImportSucceeded`)

## 3. Presentation — View

- [x] 3.1 Create `ConfirmClearImportDialog.axaml` (modal Window with "Confirm" and "Cancel" buttons, matching the shape of `ConfirmDiscardDialog`)
- [x] 3.2 Create `ConfirmClearImportDialog.axaml.cs` with a `static Task<bool> ShowAsync(Window owner)` method
- [x] 3.3 Add "Clear imported data" button to the Drop Tables Import section in `MainWindow.axaml`, bound to `DropTables.ClearImportCommand`

## 4. Composition / Wiring

- [x] 4.1 Update `MainWindow.axaml.cs` to call `_ = shell.InitializeAsync()` instead of `_ = shell.DropTables.LoadStatusAsync()`
- [x] 4.2 Update `ReliquaryServiceFactory` (or `MainWindow`) to pass `IDropTableImportStore` into `MainWindowViewModel` so it can call `ClearAsync`
- [x] 4.3 Wire `dropTablesVm.ConfirmClear` in `MainWindow.axaml.cs` to `() => ConfirmClearImportDialog.ShowAsync(this)`

## 5. Tests

- [x] 5.1 Add unit test: `MainWindowViewModel.ClearImportCommand` calls `ClearAsync` on the store when `ConfirmClear` returns true, updates `HasImportedData` to false, and invokes `OnImportCleared`
- [x] 5.2 Add unit test: `ClearImportCommand` does not call `ClearAsync` when `ConfirmClear` returns false
- [x] 5.3 Add unit test: `ClearImportCommand.CanExecute` is false when `HasImportedData` is false and true when it is true (and not busy)
- [x] 5.4 Add unit test: `MainWindowShellViewModel.InitializeAsync` sets `SelectedTabIndex` to `DropTablesTabIndex` when `HasImportedData` is false, leaves it at 0 when data exists
- [x] 5.5 Add integration test: `SqliteDropTableImportStore.ClearAsync` deletes all rows from all import tables and is idempotent (safe to call when already empty)
