## Context

The app is an Avalonia desktop application (Windows, net10.0) following a layered architecture: `Application` (domain/ports), `Infrastructure` (adapters), `Presentation` (Avalonia MVVM), `Composition` (wiring). User-facing settings are currently limited to a drop-table source URL, stored in a per-feature JSON file (`LocalDropTableSourceSettingsStore`). Window layout is fully static: fixed 900×600 default, hard-coded split rows in `MainWindow.axaml`, and no font size control. The main window has no OS menu bar.

## Goals / Non-Goals

**Goals:**
- Persist window size, font size choice, and relics/missions split ratio across launches
- Introduce a unified `IUserSettingsStore` backed by a single JSON file that supersedes the ad-hoc settings stores
- Add a Settings tab (reachable via File → Settings menu item) with Save / Back flow
- Migrate the drop-table source URL field from the Drop Tables tab into the Settings tab
- Add a `GridSplitter` to the Relics & Missions tab to allow user-controlled split proportions
- Apply font size as a switchable Avalonia style resource, affecting all text app-wide

**Non-Goals:**
- Dark/light theme switching
- Per-tab layout persistence beyond the relics/missions split
- Migrating the URL setting storage format (it will be absorbed into the unified store)
- Any changes to Application-layer drop-table refresh workflow logic

## Decisions

### 1. Unified `IUserSettingsStore` in Infrastructure

**Decision**: Introduce a single `UserSettings` record and `LocalUserSettingsStore : IUserSettingsStore` that serialises all user preferences (window size, font size, Relics panel height, source URL) into one JSON file (`user-settings.json`), replacing the existing `LocalDropTableSourceSettingsStore`.

**Rationale**: A single settings file is simpler to load/save atomically and avoids per-feature settings proliferation. The source URL is a user preference, not a domain entity; it belongs alongside other prefs.

**Alternative considered**: Keep `LocalDropTableSourceSettingsStore` and add new files alongside it. Rejected because it spreads settings across multiple files and means the Settings tab would need to save to two stores atomically.

**Pre-V1 fresh-start rule**: The app is pre-release. When `user-settings.json` is absent, `LocalUserSettingsStore` uses hardcoded defaults for all fields (URL default, Medium font, 1:2 relics/missions split, 900×600 window). No attempt is made to read or migrate any previously existing settings files.

### 2. Settings tab as a regular TabItem, hidden from the tab strip

**Decision**: Add a `SettingsTabItem` to the existing `TabControl`. Its tab header is hidden via `IsVisible="False"` on the `TabItem`. Navigation is achieved by setting `SelectedIndex` on the `TabControl` via `MainWindowShellViewModel`. "File → Settings" activates this tab; the "Back" button returns to the previously active tab index.

**Rationale**: Avalonia's `TabControl` already handles content swapping. A hidden tab avoids a separate overlay or dialog while keeping the tab strip clean. The "Back" button UX maps naturally onto "go to previous index."

**Alternative considered**: Separate overlay `Panel` with `ZIndex`. Rejected because it requires manual show/hide logic and breaks keyboard navigation.

### 3. Font size via Application-level FontSize resource override

**Decision**: Define three named `FontSize` doubles in `App.axaml` as static resources (`FontSizeSmall`, `FontSizeMedium`, `FontSizeLarge`). The `Application.Current` resource dictionary is updated at runtime (and on startup) to set a `ContentFontSize` resource key. All `TextBlock` and `TextBox` styles in `App.axaml` bind their `FontSize` to `{DynamicResource ContentFontSize}`. This produces an instant app-wide update without re-rendering the whole tree.

**Rationale**: Changing a single dynamic resource is the lightest-weight Avalonia mechanism for global style changes. It avoids VM-side font size bindings on every control.

**Alternative considered**: Setting `Window.FontSize` directly. Rejected because `Window.FontSize` does not inherit into `TextBlock` by default in Avalonia (unlike WPF).

### 4. Relics/Missions split via GridSplitter + ratio persistence

**Decision**: Replace the static `Separator` in row 2 of the Relics & Missions `Grid` with a `GridSplitter`. After the user drags the splitter, Avalonia converts the adjacent `*` rows to absolute pixel `GridLength` values — this is intentional. On window resize, the bottom (Missions) panel grows or shrinks while the top (Relics) panel stays at its dragged pixel height; no attempt is made to maintain proportionality during live resize. The Relics panel pixel height (`RowDefinitions[1].ActualHeight`) is captured on `GridSplitter.DragCompleted` and saved as-is in `UserSettings.RelicsPanelHeight` (a nullable double). On restore, the saved pixel height is applied directly in the `Window.Opened` handler; no arithmetic against the window height is needed. When the field is null, a 1:2 relics/missions split is used as the default.

**Rationale**: Storing the pixel height directly matches how the splitter actually works after a drag. Storing a ratio would imply proportional restore behaviour that does not exist. Standard desktop splitter behaviour — the top panel is fixed, the bottom grows/shrinks — is what users expect.

**Alternative considered**: A separate `SplitView` control. Not worth the added complexity for a two-panel split.

### 5. Window size persistence via `OnClosing` / startup apply

**Decision**: Subscribe to `Window.Closing` in `MainWindow.axaml.cs` and save current `Width`/`Height` to `IUserSettingsStore`. On startup, after `InitializeComponent()`, read settings and set `Window.Width` / `Window.Height` before the window is shown.

**Rationale**: Standard Avalonia pattern. Simple enough that it doesn't require a dedicated capability layer — handled in code-behind.

### 6. Application menu bar (Windows)

**Decision**: Add an Avalonia `<Menu>` docked at the top of a `DockPanel` root layout. The existing `TabControl` becomes the fill panel. The menu has a single top-level `File` item with one child: `Settings` (keyboard shortcut `Ctrl+,`).

**Rationale**: Avalonia's cross-platform `Menu` control renders appropriately on Windows. `NativeMenu` (macOS-only) is not needed here.

## Risks / Trade-offs

- **GridSplitter row height restoration** — pixel heights must be set after the window has measured its client area, otherwise the available height is unknown. Mitigation: apply in the `Window.Opened` handler, not the constructor.
- **Stale legacy settings file** — `drop-table-source.settings.json` may exist on developer machines from prior runs. It is ignored entirely; `LocalUserSettingsStore` does not read it. No mitigation needed beyond awareness.
- **Font size DynamicResource on startup flash** — if `ContentFontSize` is set slightly after the window renders, text may briefly show at default size. Mitigation: set the resource before `Show()` is called (in the `App.OnFrameworkInitializationCompleted` override or before `DataContext` is assigned).

## Implementation Order

1. Add `IUserSettingsStore` port in `Application`; add `LocalUserSettingsStore` in `Infrastructure` (all fields default to hardcoded values when `user-settings.json` is absent)
2. Update `ReliquaryServiceFactory` (Composition) to provide `IUserSettingsStore`; remove separate `LocalDropTableSourceSettingsStore` wiring
3. Update `MainWindow.axaml.cs` to read/apply settings on startup and save on close
4. Add `SettingsViewModel` and `SettingsView` (AXAML) in `Presentation`
5. Add `GridSplitter` to Relics & Missions tab; plumb ratio in/out of `MainWindowShellViewModel`
6. Remove URL field from `Drop Tables` tab; update `MainWindowViewModel` to no longer own URL editing
7. Update specs for `drop-table-source-refresh` UI scenarios (URL field is now in Settings tab)
