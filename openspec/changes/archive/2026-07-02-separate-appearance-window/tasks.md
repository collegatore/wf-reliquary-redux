## 1. Extract AppearanceViewModel

- [x] 1.1 Create `AppearanceViewModel` (Presentation layer), constructed with `IUserSettingsStore` + initial `UserSettings`, hosting `EffectiveFontSizePt`, `SelectedFontSize`, `RelicCardWidthPx`, and `SelectedOrientation`, moved from `SettingsViewModel`.
- [x] 1.2 Move the `ApplyOrientation`/`ApplyRelicCardWidth` callback-delegate properties and the `SaveFontSizeNow`/`SaveOrientationNow`/`SaveRelicCardWidthNow` auto-save helpers from `SettingsViewModel` to `AppearanceViewModel`.
- [x] 1.3 Move the `ApplyFontSize(pt, relicCardBasePx)` static helper from `SettingsViewModel` to `AppearanceViewModel`; update its one external caller (`MainWindow.axaml.cs` constructor, where it seeds initial resources before any view model exists).
- [x] 1.4 Remove the now-unused members from `SettingsViewModel`: `EffectiveFontSizePt`, `SelectedFontSize`, `RelicCardWidthPx`, `SelectedOrientation`, `ApplyOrientation`, `ApplyRelicCardWidth`, `SyncOrientation`, and their backing fields (`fontSizePt`, `relicCardWidthPx`, `orientation`, `savedOrientation`, `sideBySideRelicsWidth`, `savedSideBySideRelicsWidth`).
- [x] 1.5 Refactor `SettingsViewModel.SaveAsync` to reload current settings from the store and overlay only `SourceUrl`, `RelicsPanelHeight`, and `LfgTemplate` (matching the existing load-then-`with` pattern already used by `SaveFontSizeNow`/`SaveOrientationNow`), since font size, card width, orientation, and side-by-side width are no longer tracked locally.

## 2. Build the Appearance window

- [x] 2.1 Create `AppearanceWindow.axaml` + `.axaml.cs`, a plain `Window` (not following the existing modal `ShowDialog` dialog pattern), hosting the font size selector, relic card width control, and layout orientation toggle moved verbatim (markup and behavior) from `SettingsView.axaml`.
- [x] 2.2 Bind `AppearanceWindow`'s controls to a new `AppearanceViewModel` instance passed into its constructor.
- [x] 2.3 Remove the Appearance section markup from `SettingsView.axaml` (font size selector, relic card width control, layout orientation toggle).

## 3. Modeless, single-instance lifecycle

- [x] 3.1 Add a nullable `AppearanceWindow?` reference (e.g. on `MainWindow` or the shell) to track whether one is already open.
- [x] 3.2 Add an "open or activate" command: if no window is tracked, construct one (with `AppearanceViewModel` wired to the shell's live orientation/card-width callbacks, same pattern as today's `settings.ApplyOrientation`/`ApplyRelicCardWidth` wiring in `MainWindowShellViewModel`), set `Owner = MainWindow`, and call `Show()` (not `ShowDialog`); if one is already tracked, call `Activate()` on it instead.
- [x] 3.3 Clear the tracked reference in the window's `Closed` event handler so the app can open a fresh one after the user closes it.

## 4. File menu entry point

- [x] 4.1 Add an `Appearance` `MenuItem` to the `File` menu in `MainWindow.axaml`, alongside the existing `Drop Tables` and `Settings` items.
- [x] 4.2 Add a command (e.g. `OpenAppearanceWindowCommand`) bound to the new menu item that invokes the open-or-activate logic from task 3.2.

## 5. Clean up now-dead Settings/orientation coupling

- [x] 5.1 Remove `Settings.SyncOrientation(Orientation, SideBySideRelicsWidth)` from `MainWindowShellViewModel.NavigateToSettings()` — no longer needed once `SettingsViewModel` doesn't track orientation. Keep `Settings.SyncRelicsPanelHeight(...)` in place, since Relics panel height stays in the Settings tab.
- [x] 5.2 Update `SettingsViewModelTests.cs` and `MainWindowShellViewModelTests.cs`: move any assertions covering font size, relic card width, or orientation behavior onto new tests targeting `AppearanceViewModel`; remove assertions for members deleted in task 1.4.

## 6. Window bounds: fixed size, offset position, no persistence

This section went through several iterations before landing here — see design.md Decision 7 for the
full history (a fire-and-forget persistence save that raced app shutdown and lost writes, then a
blocking fix that violated the codebase's no-blocking-outside-`MainWindow` rule enforced by
`UiConcurrencyContractTests`). The requirement was ultimately simplified to remove persistence
entirely rather than keep fixing it:

- [x] 6.1 `AppearanceWindow.axaml`: fixed `Width`/`Height`, `CanResize="False"`. No `UserSettings` fields, no `AppearanceViewModel` bounds properties, no `Closing` handler — all removed.
- [x] 6.2 `MainWindow.OpenOrActivateAppearanceWindow()`: set the new `AppearanceWindow`'s `Position` to `MainWindow.Position` offset by `(-50, +50)` (50px left, 50px down) before `Show()`, computed fresh every time from the main window's current position.

## 7. Fixed font size and default width

- [x] 7.1 `AppearanceWindow.axaml`: add a `Window.Styles` block overriding `TextBlock`/`TextBox` `FontSize` to the static `FontSizeSmall` resource (not the dynamic `ContentFontSize` one App.axaml uses app-wide), so the window's own controls never resize themselves when the user changes the font size setting they control. Remove the now-redundant per-`TextBlock` `FontSize="{StaticResource FontSizeMedium}"` overrides on the "pt"/"px" labels.
- [x] 7.2 Bump the Appearance window's default `Width` so the font size row (4 radio buttons + numeric input + "pt" label) fits without clipping.
- [x] 7.3 Fix: `RadioButton` content ("Small"/"Medium"/"Large"/"Custom"/"Stacked"/"Side by side") wasn't shrinking, because Fluent's `RadioButton` template renders its label via a `TemplateBinding` to `RadioButton.FontSize`, not a plain styled `TextBlock` the `Window.Styles` override could catch — set `FontSize="{StaticResource FontSizeSmall}"` on the `Window` root so it cascades via inheritance to `RadioButton` (and anything else) that doesn't have its own explicit override.
- [x] 7.4 Fix (applied manually by the user): the two `NumericUpDown` controls' theme-default height/spinner-button area didn't shrink along with the fixed small font, and blind `Width`/`Padding`/`MinHeight` adjustments made it worse (unreadable text, then oversized square spinner buttons). Final fix keeps `Width="120"`, adds `Height="24"` + `VerticalContentAlignment="Center"`, and overrides the theme resources `TextControlThemeMinHeight` and `ButtonSpinnerMinHeight` to `0` locally via `NumericUpDown.Resources` on each control — shrinking the actual theme-imposed minimum sizes for the text area and spinner buttons, rather than fighting them with padding.

## 8. Manual verification

- [x] 8.1 Open File → Appearance while the Relics and Missions tab is active; confirm changing font size, relic card width, and orientation all update the main window immediately.
- [x] 8.2 Open File → Appearance while a different tab (e.g. Prime Gear) is active; confirm the same live updates apply without switching tabs.
- [x] 8.3 Select File → Appearance twice; confirm the existing window is activated/focused rather than a second window opening.
- [x] 8.4 Close the Appearance window; confirm the main window and app remain running, and that reopening via File → Appearance works again.
- [x] 8.5 Confirm the Settings tab's Save/Back/unsaved-changes guard still behaves correctly for source URL, relics panel height, and LFG template with the Appearance controls removed.
- [x] 8.6 Confirm the Appearance window cannot be resized, and that it opens 50px left and 50px down from the main window's position — including after moving the main window and reopening the Appearance window.
- [x] 8.8 In the Appearance window, change the font size selection through Small/Medium/Large/Custom; confirm the Appearance window's own text stays the same fixed small size throughout, while the main window's text changes accordingly.
- [x] 8.9 At the Appearance window's default size, confirm the font size row (radio buttons, numeric input, "pt" label) is fully visible with no clipping or wrapping.
