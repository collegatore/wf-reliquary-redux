## Context

All five changes are confined to the UI presentation layer — `MainWindow.axaml` and the global Avalonia theme/styles. No ViewModels, no data layer, and no database are affected.

Current control panel (Row 0 of `RelicsMissionsGrid`) is a single flat `StackPanel Orientation="Horizontal"` containing Show-vaulted, Show-all-rewards, and Show-exact-drop-chances checkboxes with `Border` separators. There is no visual grouping or labelling to distinguish Relics controls from Missions controls.

Mission cards (`ItemsControl` in Row 3's `StackPanel`) use `Margin="8"` on each card border. Between rows the vertical gap is card-bottom (8 px) + card-top (8 px) = 16 px. Above the first row it is only card-top (8 px) — half as much — hence the unevenness.

The `Ctrl+,` shortcut is registered in two places: `Window.KeyBindings` and `MenuItem.InputGesture` on the Settings `MenuItem`.

Dropdown buttons (inside `Flyout` / `MenuFlyout`) inherit Avalonia's default button background. Making them lighter requires a scoped style targeting `Button` elements inside `Flyout` popups.

## Goals / Non-Goals

**Goals:**
- Even top-margin on the first row of Mission cards (match the inter-row gap)
- Labelled Relics / Missions sub-groups in the shared control panel
- "Show vaulted relics" renamed to "Show vaulted"
- "Show exact drop chances" moved to the Missions sub-group
- `Ctrl+,` keyboard shortcut removed from Settings
- Lighter button backgrounds inside all `Flyout` popups

**Non-Goals:**
- Changing control-panel layout into two separate Border rows
- Any ViewModel or data-layer changes
- Changing the Wishlist dropdown button itself (only its flyout content buttons)

## Decisions

**Mission card top-margin fix:** Add `Margin="0,8,0,0"` to the missions `ItemsControl` (the one whose `ItemsPanel` is a `WrapPanel`). This adds 8 px above the first row, matching the inter-row gap of 8 + 8 = 16 px from the top of one card to the top of the next. No structural changes needed.

**Control panel restructure:** Keep the single `Border` + horizontal `StackPanel`. Insert a plain `TextBlock Text="Relics:"` before the first checkbox, and a `TextBlock Text="Missions:"` after the separator that currently precedes Show-exact-drop-chances. This requires no new containers — just two `TextBlock` additions and a label text change.

**Settings keyboard shortcut removal:** Remove the `<KeyBinding Gesture="Ctrl+OemComma" .../>` element from `Window.KeyBindings` and remove `InputGesture="Ctrl+OemComma"` from the Settings `MenuItem`. The menu item remains fully functional via click and the File menu remains intact.

**Lighter dropdown button backgrounds:** Add a `Window.Styles` entry targeting `Button` inside `Popup` (Avalonia's flyout host). Use a semi-transparent light brush (`#18808080` or similar — lighter than the current card/panel backgrounds) rather than `Transparent`, so buttons are visually distinct from the flyout background without being heavy.

## Risks / Trade-offs

- **Popup selector specificity**: Avalonia's flyout renders inside a `Popup` overlay; selector `Popup Button` should match all flyout buttons in the window. If the selector is too broad it could affect other popups (e.g., `MenuFlyout` tooltips). Mitigation: scope the style to `Flyout`-hosted buttons only; test with the Wishlist dropdown and any other flyouts present.
- **Mission card margin change**: Adding `Margin="0,8,0,0"` to the `ItemsControl` container rather than individual cards means the 8 px gap is fixed regardless of whether the panel is empty or populated. The empty-state `TextBlock` above already has `Margin="8,16"`, so the empty-state spacing is unaffected.
