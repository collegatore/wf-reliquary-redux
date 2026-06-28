## 1. Domain model — font size field

- [x] 1.1 Replace `UserFontSize FontSize` on `UserSettings` with `double FontSizePt` (default 14.0); remove any `CustomFontSizePt` field
- [x] 1.2 Keep `UserFontSize` enum (Small/Medium/Large only) inside `LocalUserSettingsStore.SettingsFile` DTO for backward-compat reading of old JSON; add `double? FontSizePt = null` to that DTO as the primary field
- [x] 1.3 On load: prefer `FontSizePt > 0`; fall back to `LegacyFontSizeToPt(FontSize)` for old files
- [x] 1.4 On save: write `FontSizePt` (primary) and derive `FontSize` enum via `PtToLegacyFontSize` for backward compat

## 2. SettingsViewModel — font size as sole backing field

- [x] 2.1 Replace `savedFontSize`/`fontSizeOption`/`customFontSizePt` with a single `double fontSizePt` backing field
- [x] 2.2 Add `EffectiveFontSizePt` property (two-way, bound by `NumericUpDown`): clamps to 8–32, raises `SelectedFontSize` changed if the derived option changes, applies live, fires `_ = SaveFontSizeNow()`
- [x] 2.3 Add `SelectedFontSize` derived property: getter maps `fontSizePt` to `Small/Medium/Large/Custom`; setter maps preset to its `double` constant and assigns `EffectiveFontSizePt` (Custom setter is a no-op)
- [x] 2.4 Add `FontSizeOption.Custom` to the presentation-layer `FontSizeOption` enum (presentation-only derived state)
- [x] 2.5 Remove font size from `HasUnsavedChanges` and from `TryDiscardAsync` rollback
- [x] 2.6 Keep `fontSizePt` included in the record written by `SaveAsync` so the main Save always persists the current effective size
- [x] 2.7 Add private `async Task SaveFontSizeNow()`: loads persisted settings, replaces only `FontSizePt`, saves

## 3. SettingsView.axaml — font size UI

- [x] 3.1 Add `Custom` radio button to the Font size section, bound via `EnumToBoolConverter` to `Settings.SelectedFontSize`
- [x] 3.2 Add `NumericUpDown` always visible (no `IsVisible` binding), bound `TwoWay` to `Settings.EffectiveFontSizePt`; range 8–32, `FormatString="0"`, `Width=120`
- [x] 3.3 Selecting a preset radio auto-updates `EffectiveFontSizePt` to that preset's value; pressing up/down switches to Custom when the value no longer matches a preset
- [x] 3.4 Pin `FontSize="{StaticResource FontSizeMedium}"` on the "Font size" section `TextBlock` and the "pt" label so they stay static while `RadioButton` labels also stay static (both exempt from the global `ContentFontSize` `TextBlock` style for different reasons — fixed here via explicit attribute)

## 4. SettingsView.axaml — Ctrl+S shortcut

- [x] 4.1 Add `<UserControl.KeyBindings><KeyBinding Gesture="Ctrl+S" Command="{Binding Settings.SaveCommand}" /></UserControl.KeyBindings>` to `SettingsView.axaml`

## 5. LfgTemplateEditorViewModel — Reload from saved

- [x] 5.1 Add `ReloadFromSavedCommand` (`RelayCommand`) to `LfgTemplateEditorViewModel`
- [x] 5.2 Handler: assign `patternText` from `savedTemplate.Pattern`, clear and re-populate `Groups` from `savedTemplate.Groups`, clear `ParseError`/`ParseWarning`, call `NotifySaved(savedTemplate)` to clear dirty state
- [x] 5.3 Add "Reload from saved" button to LFG editor actions row in `SettingsView.axaml`, bound to `LfgEditor.ReloadFromSavedCommand`

## 6. Tests

- [x] 6.1 Update `UserSettingsStoreTests`: replace `FontSize: UserFontSize.X` with `FontSizePt: <double>`; replace `.FontSize` assertions with `.FontSizePt`
- [x] 6.2 Update `SettingsViewModelTests`: replace font-size-triggers-dirty tests with correct behavior (font size does not dirty; font size changes navigate away without confirm guard); fix `SaveAsync` test to dirty a URL field to enable Save
- [x] 6.3 Update `MainWindowShellViewModelTests`: replace `FontSize: UserFontSize.Medium` with `FontSizePt: 14.0`

## 7. Smoke-test

- [x] 7.1 Build the solution with 0 errors; all integration tests pass (204/205, 1 skipped)
