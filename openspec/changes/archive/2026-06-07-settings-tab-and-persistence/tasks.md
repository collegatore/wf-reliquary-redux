## 1. Unified User Settings Store

- [x] 1.1 Define `UserSettings` record in `Application` with fields: `SourceUrl`, `WindowWidth`, `WindowHeight`, `FontSize` (enum: Small/Medium/Large), `RelicsPanelHeight` (nullable double, pixels)
- [x] 1.2 Add `IUserSettingsStore` port in `Application` with `LoadAsync` and `SaveAsync`
- [x] 1.3 Implement `LocalUserSettingsStore` in `Infrastructure` serialising to `user-settings.json`; when the file is absent use hardcoded defaults for all fields (no migration from any prior settings files)
- [x] 1.4 Register `IUserSettingsStore` / `LocalUserSettingsStore` in `ReliquaryServiceFactory` (Composition); remove separate `IDropTableSourceSettingsStore` wiring

## 2. Application Menu Bar

- [x] 2.1 Wrap the `MainWindow.axaml` root content in a `DockPanel`; add an Avalonia `<Menu>` docked to the top with a single `File` item containing a `Settings` `MenuItem` (gesture `Ctrl+,`)
- [x] 2.2 Bind `File → Settings` `MenuItem.Command` to a `NavigateToSettingsCommand` on `MainWindowShellViewModel`

## 3. Settings ViewModel

- [x] 3.1 Add `FontSizeOption` enum (Small / Medium / Large) to `Presentation`
- [x] 3.2 Create `SettingsViewModel` in `Presentation` with: `SelectedFontSize`, `SourceUrlText`, `RelicsPanelHeight` (nullable double, pixels), `HasUnsavedChanges`, `SaveCommand`, `BackCommand`, `ResetSplitCommand`
- [x] 3.3 Implement `SaveCommand`: write all settings via `IUserSettingsStore`; apply font size resource; raise event so `MainWindowShellViewModel` can propagate the new split ratio and URL
- [x] 3.4 Implement `BackCommand`: if `HasUnsavedChanges` show a confirmation dialog; on confirm (or no changes) raise navigation event; on cancel remain on Settings
- [x] 3.5 Implement `ResetSplitCommand`: set `RelicsPanelHeight` to null and mark unsaved changes (null signals 1:2 relics/missions default on next launch)

## 4. Settings View (AXAML)

- [x] 4.1 Create `SettingsView.axaml` with: font size `RadioButton` group (Small / Medium / Large), split proportion label + reset button, source URL `TextBox`, and Save / Back `Button` controls
- [x] 4.2 Add the Settings `TabItem` to `MainWindow.axaml` `TabControl` with `IsVisible="False"` on the header

## 5. Navigation Plumbing

- [x] 5.1 Add `SelectedTabIndex` (observable int) and `NavigateToSettingsCommand` to `MainWindowShellViewModel`; save previous index before switching to Settings tab
- [x] 5.2 Wire `BackCommand` result in `SettingsViewModel` to restore the previous tab index via `MainWindowShellViewModel`
- [x] 5.3 Bind `TabControl.SelectedIndex` in `MainWindow.axaml` to `Shell.SelectedTabIndex`

## 6. Font Size Application

- [x] 6.1 Define `ContentFontSize`, `FontSizeSmall`, `FontSizeMedium`, `FontSizeLarge` as double resources in `App.axaml`; set app-level `TextBlock` style with `FontSize="{DynamicResource ContentFontSize}"`
- [x] 6.2 Implement `FontSizeService` (or inline in `SettingsViewModel`) that writes the chosen size to `Application.Current.Resources["ContentFontSize"]` on Save and on startup

## 7. Window Size Persistence

- [x] 7.1 In `MainWindow.axaml.cs`: read `IUserSettingsStore` on startup; set `Width` / `Height` before `Show()` (fall back to 900 × 600 if absent)
- [x] 7.2 Subscribe to `Window.Closing` (or `OnClosed`); save current `Width` and `Height` to settings

## 8. Relics/Missions GridSplitter

- [x] 8.1 Change the Relics & Missions `Grid` row definitions from `"Auto,*,Auto,*"` to `"Auto,*,5,*"` and replace the `Separator` (row 2) with a `GridSplitter` spanning the row
- [x] 8.2 In the `Window.Opened` handler, apply `UserSettings.RelicsPanelHeight` directly to `RowDefinitions[1].Height` as an absolute `GridLength`; when null, set row 1 to `1*` and row 3 to `2*` for the 1:2 default
- [x] 8.3 On `GridSplitter.DragCompleted`, read `RowDefinitions[1].ActualHeight` and push it to `MainWindowShellViewModel` as the new `RelicsPanelHeight` so `SettingsViewModel` holds the latest pixel value when the Settings tab opens

## 9. Drop Tables Tab Cleanup

- [x] 9.1 Remove the "Drop-table source URL" `StackPanel` section and the `Separator` following it from the Drop Tables tab in `MainWindow.axaml`
- [x] 9.2 Remove `SourceUrlText`, `CanSaveUrl`, and `SaveSourceUrlCommand` from `MainWindowViewModel`; remove the corresponding `IDropTableSourceSettingsStore` dependency
- [x] 9.3 Update unit/integration tests that reference the removed URL-editing view-model properties

## 10. Tests and Verification

- [x] 10.1 Add unit tests for `LocalUserSettingsStore`: round-trip serialisation and hardcoded defaults when file is absent
- [x] 10.2 Add unit tests for `SettingsViewModel`: unsaved-changes detection, Save persists all fields, Back guard logic, ResetSplit sets `RelicsPanelHeight` to null
- [x] 10.3 Run `dotnet test` and confirm all existing tests pass with the removed `SaveSourceUrlCommand` and updated settings wiring
