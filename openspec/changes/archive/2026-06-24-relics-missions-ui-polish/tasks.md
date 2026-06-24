## 1. Mission cards spacing fix

- [x] 1.1 Add `Margin="0,8,0,0"` to the missions `ItemsControl` (the one with `WrapPanel` inside Row 3 of `RelicsMissionsGrid`) so the first row of mission cards has the same 8 px gap above it as between subsequent rows

## 2. Control panel — Relics group label and checkbox rename

- [x] 2.1 Insert `TextBlock Text="Relics:" VerticalAlignment="Center"` as the first child of the control-panel `StackPanel`, before the "Show vaulted relics" checkbox group
- [x] 2.2 Change the `TextBlock` label text from `"Show vaulted relics"` to `"Show vaulted"` in the control panel

## 3. Control panel — Missions group

- [x] 3.1 Insert `TextBlock Text="Missions:" VerticalAlignment="Center"` after the `Border` separator that precedes the "Show exact drop chances" checkbox group in the control panel

## 4. Remove Settings keyboard shortcut

- [x] 4.1 Remove the `<KeyBinding Gesture="Ctrl+OemComma" Command="{Binding NavigateToSettingsCommand}" />` element from `Window.KeyBindings` in `MainWindow.axaml`
- [x] 4.2 Remove the `InputGesture="Ctrl+OemComma"` attribute from the Settings `MenuItem` in the File menu

## 5. Lighter dropdown button backgrounds

- [x] 5.1 Add a `Window.Styles` entry in `MainWindow.axaml` targeting `Popup Button` (or `FlyoutPresenter Button`) that sets `Background` to a lighter semi-transparent brush (e.g., `#18808080`) so flyout buttons are visually lighter than the current default
- [x] 5.2 Verify the style applies to the Wishlist dropdown flyout buttons and does not adversely affect any `MenuFlyout` items or other popup elements
