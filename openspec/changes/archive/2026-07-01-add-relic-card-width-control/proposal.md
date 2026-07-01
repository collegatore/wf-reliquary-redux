## Why

Relic card width is currently a fixed `300` pixel literal, used as the base value that `RelicCardWidth` scales from as the font-size ratio changes (`RelicCardWidth = 300 * (fontPt / mediumPt)`). Users on wide displays or with dense wishlists have no way to change that base — they can only get a wider or narrower card as a side effect of changing text size, which couples two unrelated preferences. A dedicated, persisted base-width control in the Settings tab lets users tune card density independently, while preserving today's font-scaling behavior.

## What Changes

- Add a **Relic card width** control to Settings → Appearance, alongside the existing font-size and Relics/Missions layout controls. A numeric input (px) sets the *base* width — the effective card width when font size is Medium (14pt) — applies live, and auto-saves on change, matching the existing font-size/orientation UX (no Save button required).
- Add `RelicCardWidthPx` to persisted `UserSettings`, defaulting to `300` (today's fixed literal), restored on launch.
- Replace the hardcoded `300` literal in `SettingsViewModel.ApplyFontSize`'s `RelicCardWidth = 300 * (pt / mediumPt)` computation with the configured `RelicCardWidthPx`, so relic cards continue to scale with font size exactly as today, just from a user-chosen base instead of a fixed one. `MissionCardWidth` is unaffected and continues to use the fixed `300` base.
- Update the side-by-side layout's minimum column width for the Relics column so it stays derived from the current *effective* (font-scaled) relic card width (declaratively, not via runtime measurement), instead of the current hardcoded `316`.
- Give both the new relic card width control and the existing font-size numeric input the same native Avalonia `NumericUpDown` spinner, explicitly positioned on the right of the numeric value (`ButtonSpinnerLocation="Right"`) for both controls, so their up/down spinner sits in a consistent place.

## Capabilities

### New Capabilities
- `relic-card-width-setting`: A persisted, live-applied, auto-saved numeric control (Settings → Appearance) for the relic card's base width (at Medium font size), with clamped bounds and a sensible default, using the standard `NumericUpDown` spinner on the right. Relic cards continue to scale with font size from this base, exactly as they scale from the fixed `300` today.

### Modified Capabilities
- `relics-missions-layout-orientation`: The "Side-by-Side Minimum Width" requirement changes from a fixed literal (`316`, based on the old fixed `300`px card width) to a value derived from the current effective (font-scaled) relic card width, so the splitter minimum stays correct regardless of the chosen base width or font size.
- `settings-tab`: The font-size numeric input's requirement is extended to specify that its spinner is explicitly positioned on the right (`ButtonSpinnerLocation="Right"`), matching the new relic card width control's layout.

## Impact

- `src/Application/WF.Reliquary.Application/Settings/UserSettings.cs`: new `RelicCardWidthPx` property + default.
- `src/Presentation/WF.Reliquary.Presentation/SettingsViewModel.cs`: new property mirroring `EffectiveFontSizePt`'s auto-save pattern; `ApplyFontSize`'s `RelicCardWidth` computation now reads the configured base instead of the literal `300`, and changing the new setting re-runs the same ratio computation.
- `src/Presentation/WF.Reliquary.Presentation/SettingsView.axaml`: new `NumericUpDown` control under Appearance (`ButtonSpinnerLocation="Right"`), and the existing font-size `NumericUpDown` given the same explicit `ButtonSpinnerLocation="Right"`.
- `src/Presentation/WF.Reliquary.Presentation/App.axaml`: `RelicCardWidth` resource default stays `300`, still computed from font ratio, now anchored to the configured base instead of a literal.
- `src/Presentation/WF.Reliquary.Presentation/RelicsMissionsView.axaml(.cs)`: side-by-side Relics column `MinWidth` derived from the current effective relic card width instead of the hardcoded `316`.
- Infrastructure settings store (`LocalUserSettingsStore` and its serialized settings file schema) gains the new field.
