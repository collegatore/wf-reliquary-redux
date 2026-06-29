## Why

`MainWindow.axaml` (~850 lines) and `MainWindow.axaml.cs` (~496 lines) hold the markup
and code-behind for every tab in one file, mixing the composition root, window chrome,
and four unrelated tab surfaces. The Settings tab already proves the better pattern — it
lives in its own `SettingsView` UserControl. Decomposing the remaining tabs the same way
shrinks `MainWindow` to its real responsibilities (composition + chrome), isolates each
tab's delicate code-behind (notably the Relics/Missions splitter), and is a prerequisite
for cleanly adding a second Relics/Missions layout later.

## What Changes

- Extract the **Prime Gear** tab into a `PrimeGearView` UserControl, moving its
  `WishlistDropdownButton` flyout-close wiring into the view (mirroring how
  `SettingsView` wires `ProfileDropdownButton`).
- Extract the **Relics and Missions** tab into a `RelicsMissionsView` UserControl,
  moving ALL splitter/min-size code-behind (`splitterInitialized`,
  `OnRelicsGridFirstMeasured`, `OnSplitterDragCompleted`, `ApplyRelicsPanelHeight`,
  `SetRelicsRowHeight`, `AvailableRelicsHeight`, `UpdateRelicsMinHeight`, and the
  related shell `PropertyChanged` wiring) out of `MainWindow`.
- Extract the **Drop Tables** tab into a `DropTablesView` UserControl (pure bindings,
  no code-behind).
- `MainWindow` retains only window-level concerns: the composition root (building the
  shell and all VMs, dialog callbacks), window size/position restore + save, the
  toast/error reporter, the footer spinner, and the Settings tab-guard. Its
  `TabControl` references the four views as `<local:XView/>`.
- Each extracted view inherits the shell as its `DataContext` (no explicit DataContext
  assignment) and binds to the same shell sub-view-models as today.
- No user-visible behavior change. Splitter persistence continues to route through
  `shell.RelicsPanelHeight`, so the window-closing save logic is untouched.

## Capabilities

### New Capabilities
- `main-window-tab-views`: each main-window tab is a self-contained Avalonia
  UserControl that inherits the shell DataContext, with `MainWindow` reduced to the
  composition root plus window chrome.

### Modified Capabilities
<!-- None. This is a behavior-preserving refactor; main-window-navigation requirements
     (startup tab, strip composition, header visibility) are unchanged. -->

## Impact

- `src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml` — reduced to chrome +
  tab references.
- `src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml.cs` — reduced to
  composition root, window persistence, spinner, error reporter, tab-guard.
- New files: `PrimeGearView.axaml(.cs)`, `RelicsMissionsView.axaml(.cs)`,
  `DropTablesView.axaml(.cs)`.
- No changes to view-models, application, infrastructure, or persisted settings.
- Existing UI integration tests must continue to pass unchanged; named elements move
  scope from `MainWindow` to their owning view.
