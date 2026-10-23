# Tasks

## 1. Palette setting and persistence

- [x] 1.1 Add `ColorPalette { Linen, Stone }` to `Application/Settings` and `ColorPalette Palette = ColorPalette.Linen` to `UserSettings`; verify `dotnet build` succeeds
- [x] 1.2 In `LocalUserSettingsStore`, add `string? Palette` to `SettingsFile`, write the enum name on save and parse it with `Enum.TryParse(ignoreCase: true)` on load, defaulting to Linen (D4); add `UserSettingsStoreTests` for the round trip, a file without the field (→ Linen), and an unknown value (→ Linen with the other settings intact); verify `dotnet test` passes

## 2. Palettes and live application

- [x] 2.1 Create `Styles/Palettes/Linen.axaml` and `Styles/Palettes/Stone.axaml` with the D1 keys and values; restructure `App.axaml`'s resources to merge Linen, and remove the brush declarations from `ControlBars.axaml` and `CardSelectedBrush` from `App.axaml`'s direct resources; verify the build succeeds and the snapshot tool's default scenes render in Linen colors
- [x] 2.2 Add public static `PaletteApplier.Apply(ColorPalette)` that replaces the merged palette dictionary (D1); verify with the snapshot tool (2.5) that applying Stone recolors bars, chips, and the selection wash
- [x] 2.3 Add the chip tick styles (`CheckBox.chip:checked` and its pointer-over / pressed variants on `Border#NormalRectangle` → `SelectionAccentBrush`, D2) and the bar search-box style (`Border.control-bar TextBox`, D3); verify in snapshots that chip ticks follow the palette while the search box matches the wishlist selector's background
- [x] 2.4 Add `SelectedPalette` to `AppearanceViewModel` (apply + auto-save, D5) and apply `initialSettings.Palette` in `MainWindow`'s constructor next to `ApplyFontSize`; add `AppearanceViewModelTests` for the auto-save, following the existing `SelectedViewMode_Change_AutoSavesToStore` test; verify `dotnet test` passes
- [x] 2.5 Give the snapshot tool's `Scene` an optional palette applied before rendering, and add Stone variants of the Prime Gear (including hover) and side-by-side Relics scenes (D6); verify the Stone scenes show stone bars, sage hover, and heather checked chips, and the Linen scenes linen, fern, and ink

## 3. Dropdowns

- [x] 3.1 Create `Styles/Dropdowns.axaml` (included from `App.axaml` after `ControlBars.axaml`) with the closed-`ComboBox` and `ComboBoxItem` styles from D3b, moving the restyle's `Border.control-bar ComboBox` styles there without the bar scope; verify in the running app that the sort-mode, Drop Tables source, Settings, and recruiting-dialog ComboBoxes show the field look, the hover tint on the field and on pointed-at items, and the selection color on the selected item, with readable text, under both palettes
- [x] 3.2 Move the `FlyoutPresenter Button` style from `MainWindow.axaml` into `Dropdowns.axaml` with the hover/pressed tints, bind `Classes.active` to `IsActive` on the Wishlist and Profile popup entries, add the `Button.active` selection style, and remove the ✓ glyph and its placeholder from both entry templates (D3b); verify both popups mark the active entry with the selection color only, entry names stay aligned, and other entries show the hover tint
- [x] 3.3 Rename `Button.bar-field` to `Button.dropdown-field`, move it into `Dropdowns.axaml`, and apply it to the Wishlist selector and the Settings Profile selector; verify the two selectors look the same and the snapshot tool's Prime Gear scenes are otherwise unchanged
- [x] 3.4 Set `FlyoutPresenterClasses="card-menu"` on the three card `MenuFlyout`s and add the `MenuFlyoutPresenter.card-menu MenuItem` styles from D3c (reset `:selected`, hover tint on `:pointerover`, pressed tint), plus `CardMenuKeyboardHighlight` and the `keyboard-nav` style for keyboard highlighting (D3c: arrow keys never set `:focus-visible`); verify (headless) that each card menu opens with no item highlighted, highlights on hover and on the Down arrow key, opens plain again after reopening, uses the palette tints, and that the File menu is unchanged

- [x] 3.5 Ask the user the design's open question on how inactive controls should look and record the answer in design.md; then add `ControlInactiveForegroundBrush` and `ControlInactiveOutlineBrush` to both palettes, and replace the interim 45% opacity style for disabled bar buttons (from `group-selection-discoverability`) with the chosen treatment, applying it to disabled chips and dropdowns too; verify with the snapshot tool that the inactive group-operations actions follow each palette and show no hover

## 4. Appearance window

- [x] 4.1 Add a "Palette" row with Linen and Stone radio buttons to `AppearanceWindow.axaml`, bound to `SelectedPalette` through `EnumToBoolConverter`, and grow the fixed window height to fit; verify in the running app that the row shows the active palette, that switching recolors the main window immediately on every tab, and that nothing in the window is clipped

## 5. Documentation

- [x] 5.1 Update `docs/technical-reference.md`'s control-bar styling note: palettes live in `Styles/Palettes/`, every palette defines the same keys, `PaletteApplier` swaps them, dropdowns are styled app-wide in `Styles/Dropdowns.axaml` (`dropdown-field` replaces `bar-field`), card menus opt in with `FlyoutPresenterClasses="card-menu"`, and the snapshot tool renders per palette; add a `CHANGELOG.md` entry for the palette setting and the new default look under an `## [Unreleased]` section above `[1.0.0]` (Keep a Changelog format; create the section if it's still missing); verify the note names files, classes, and keys that exist

## 6. Integration check

- [x] 6.1 In the running app: switch palettes with each tab open and with a dropdown and a card menu open, restart and confirm the choice persists, check a selected owned gear card's wash under both palettes, and confirm gear-card part checkboxes keep the standard accent; run `dotnet test` and verify it passes (Confirmed by the user in the running app, 2026-10-23.)
