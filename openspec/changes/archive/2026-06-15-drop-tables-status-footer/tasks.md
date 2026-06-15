## 1. ViewModel — condensed import status and footer visibility

- [x] 1.1 Add `FooterImportStatusText` property to `MainWindowViewModel` — returns "Source data not imported" (no import) or "Drop Tables version: `<lastUpdate>`" (import exists)
- [x] 1.2 Update `RefreshImportStatusAsync` in `MainWindowViewModel` to set `FooterImportStatusText` alongside the existing `ImportStatusText` and `HasImportedData` updates
- [x] 1.3 Add `ShowFooterImportStatus` computed property to `MainWindowShellViewModel` — returns `!DropTables.IsBusy && SelectedTabIndex != DropTablesTabIndex`
- [x] 1.4 Subscribe to `DropTables.PropertyChanged` for `IsBusy` in `MainWindowShellViewModel` (alongside the existing `HasImportedData` subscription) so `ShowFooterImportStatus` raises `PropertyChanged` when busy state changes

## 2. AXAML — extend footer

- [x] 2.1 In `MainWindow.axaml` footer `StackPanel`, add a vertical separator `Border` (width 1, gray) after the profile `TextBlock` elements, matching the separator style used in the tab control panels
- [x] 2.2 Add a spinning arc indicator (14×14 `Path` with a 270° arc, driven by a `DispatcherTimer` in code-behind rotating a `RotateTransform`) with `IsVisible="{Binding IsFooterBusy}"` — Avalonia 12 has no registered animator for `ITransform`, so XAML-only rotation is not possible; accent-colored stroke via `SystemAccentColor`
- [x] 2.3 Add a `TextBlock` bound to `{Binding DropTables.OperationStatusText}` with `IsVisible="{Binding DropTables.IsBusy}"` and `TextTrimming="CharacterEllipsis"`
- [x] 2.4 Add a `TextBlock` bound to `{Binding DropTables.FooterImportStatusText}` with `IsVisible="{Binding ShowFooterImportStatus}"` and `TextTrimming="CharacterEllipsis"`

## 3. ViewModel — split import stats out of OperationStatusText

- [x] 3.1 In `MainWindowViewModel.ImportAsync`, after a successful import set `ImportStatusText` directly with the full stats line ("Source data imported. Drop Tables version: X. Relics: N; relic rewards: N; mission sources: N; mission rewards: N.") and set `OperationStatusText` to a concise line ("Import completed. Drop Tables version: X.") — no counts in the operation status
- [x] 3.2 Update `RefreshImportStatusAsync` so it does not overwrite a stats-bearing `ImportStatusText` on its own; it should only write the plain "Source data imported. Drop Tables version: X." form when called from `LoadStatusAsync` or `ClearImportAsync` (where no stats are available)

## 4. AXAML — update Drop Tables tab body

- [x] 4.1 Remove the `OperationStatusText` `TextBlock` from the bottom of the Drop Tables tab body (the element bound to `{Binding DropTables.OperationStatusText}`)
- [x] 4.2 Rename `StatusText` → `SourceStatusText` throughout: property declaration and backing field in `MainWindowViewModel`, all references in `MainWindowViewModel` (including `ApplyStatus`), and the AXAML binding in `MainWindow.axaml`

## 5. Verification

- [x] 5.1 Run `dotnet build` and confirm zero errors
- [x] 5.2 With no import data, navigate to Prime Gear tab and confirm footer shows "Source data not imported"
- [x] 5.3 Navigate to Drop Tables tab and confirm footer status region is empty
- [x] 5.4 Trigger a download and confirm footer shows spinner + "Downloading source from URL…" from any tab
- [x] 5.5 After importing, navigate away from Drop Tables and confirm footer shows "Drop Tables version: `<last update>`"
- [x] 5.6 After importing, open the Drop Tables tab and confirm `ImportStatusText` includes the full relic/reward/mission counts
- [x] 5.7 Confirm no standalone operation-status TextBlock remains in the Drop Tables tab body
- [x] 5.8 Confirm AXAML Source acquisition TextBlock binds `SourceStatusText` (not `StatusText`) and wording is unchanged
