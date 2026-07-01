## 1. Persistence

- [x] 1.1 Add `RelicCardWidthPx` (`double`) to `UserSettings` (`src/Application/WF.Reliquary.Application/Settings/UserSettings.cs`), default `300.0` in `UserSettings.Default`.
- [x] 1.2 Add the corresponding field to the serialized settings file schema and round-trip it in `LocalUserSettingsStore` (`src/Infrastructure/WF.Reliquary.Infrastructure/Settings/LocalUserSettingsStore.cs`), following the same load/save pattern used for `FontSizePt`.

## 2. Settings ViewModel

- [x] 2.1 In `SettingsViewModel.cs`, add a `RelicCardWidthPx` field/property holding the configured *base* width, mirroring `fontSizePt`/`EffectiveFontSizePt`: clamp to `[180, 600]` via `Math.Clamp` in the setter.
- [x] 2.2 Change `ApplyFontSize(double pt)` to accept (or read from a ViewModel field) the configured relic card base width instead of the literal `300`, keeping the existing ratio formula: `resources["RelicCardWidth"] = relicCardBasePx * ratio`. Leave the `MissionCardWidth` line using the literal `300` untouched.
- [x] 2.3 In the `RelicCardWidthPx` setter, re-invoke `ApplyFontSize(fontSizePt)` (using the current font size) so `RelicCardWidth` is recomputed from the new base at the currently active font ratio — do not set the resource to the raw unscaled base value.
- [x] 2.4 Add `SaveRelicCardWidthNow()` (load current settings, `with { RelicCardWidthPx = ... }`, save) mirroring `SaveFontSizeNow`/`SaveOrientationNow`.
- [x] 2.5 Call `ApplyFontSize` with the loaded base width in the constructor so `RelicCardWidth` reflects both the loaded font size and loaded card width base on startup.
- [x] 2.6 Confirm `RelicCardWidthPx` is excluded from `HasUnsavedChanges` (it should not touch `savedX`/dirty-tracking fields at all, same as font size).
- [x] 2.7 ~~Add increase/decrease `RelayCommand`s for font size and relic card width~~ — superseded: reverted to Avalonia's built-in `NumericUpDown` spinner (see 3.1/3.2), so no separate step commands are needed; removed from `SettingsViewModel`.

## 3. Settings View (XAML)

- [x] 3.1 Add a "Relic card width" label + `NumericUpDown` (`Minimum="180" Maximum="600" Increment="10" FormatString="0"`, `ButtonSpinnerLocation="Right"`, `Width="120"`) to the Appearance group in `SettingsView.axaml`, two-way bound to `Settings.RelicCardWidthPx`, positioned alongside the font-size and Relics/Missions layout controls.
- [x] 3.2 Set `ButtonSpinnerLocation="Right"` explicitly on the existing font-size `NumericUpDown` (`SettingsView.axaml`) so both numeric controls have their spinner in the same place.

## 4. Side-by-side minimum width

- [x] 4.1 In `RelicsMissionsView.axaml`, remove the hardcoded `MinWidth="316"` on the Relics column (column 0) of `SideBySideGrid`; leave the Missions column (column 2) at its existing literal minimum.
- [x] 4.2 In `RelicsMissionsView.axaml.cs`, set the Relics column's `MinWidth` from the current `RelicCardWidth` resource value (already font-scaled) plus card margin (16) at view construction, and recompute it whenever either the relic card width base or the font size changes (wire hooks from both `SettingsViewModel` auto-save paths/the shell, matching how orientation changes are already propagated live).

## 5. Verification

- [x] 5.1 Manually verify: changing the relic card width control resizes relic cards immediately, without enabling the Settings Save button.
- [x] 5.2 Manually verify: restart the app after setting a custom base width and confirm it's restored (rendered at the saved base when font size is Medium).
- [x] 5.3 Manually verify: changing font size still proportionally resizes relic cards, now scaled from the configured base instead of the literal `300`; mission card width still scales with font size off its fixed `300` base as before.
- [x] 5.4 Manually verify: with a non-Medium font size active, changing the relic card width base immediately reflects the new base scaled by the current font ratio (not the raw unscaled base).
- [x] 5.5 Manually verify: in the side-by-side layout, set a wide relic card base width and/or a large font size, switch to side-by-side, and confirm the splitter cannot drag the Relics panel narrower than one card column at the current effective width.
- [x] 5.6 Run the existing automated test suite and add/update tests covering the new setting's clamping, persistence, default value, and interaction with font-size scaling.
- [x] 5.7 Manually verify: both the relic card width and font size controls use the native `NumericUpDown` spinner positioned on the right; clicking up/down steps the value by its increment and clamps at the bounds.
