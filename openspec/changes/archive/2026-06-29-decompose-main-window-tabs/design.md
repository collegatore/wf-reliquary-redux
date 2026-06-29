## Context

`MainWindow.axaml` (~850 lines) and `MainWindow.axaml.cs` (~496 lines) currently hold
all four tabs inline plus the composition root and window chrome. The Settings tab is
already extracted as `SettingsView` (a `UserControl`), and it demonstrates the two
patterns this change generalizes:

1. **DataContext inheritance** — `SettingsView` never sets a DataContext; it binds
   against the shell it inherits (`{Binding Settings.SaveCommand}`, `{Binding Profiles}`).
2. **Named-element wiring on DataContextChanged** — in `SettingsView.axaml.cs`,
   `OnDataContextChanged` wires `shell.Profiles.FlyoutCloseRequested` to
   `ProfileDropdownButton.Flyout?.Hide()`.

The remaining tabs vary in code-behind weight:
- **Drop Tables** — pure bindings, no named elements, no code-behind.
- **Prime Gear** — one named element (`WishlistDropdownButton`) needing flyout-close
  wiring.
- **Relics and Missions** — the heavy one: a `GridSplitter` with star→pixel conversion,
  tallest-card min-height enforcement, drag-completion persistence, and shell
  `PropertyChanged` reactions, all currently in `MainWindow.axaml.cs`.

This is a behavior-preserving refactor and a prerequisite for the separate
`relics-missions-side-by-side-layout` change.

## Goals / Non-Goals

**Goals:**
- Each tab's content lives in its own `UserControl` (`.axaml` + `.axaml.cs`).
- All tab-specific code-behind moves into the owning view; `MainWindow.axaml.cs` keeps
  only window-level concerns.
- Zero user-visible behavior change; existing UI/integration tests pass unchanged.
- `MainWindow.axaml` reduced to chrome (DockPanel, menu, footer, toast overlay) plus a
  `TabControl` of four `<local:XView/>` references.

**Non-Goals:**
- No change to view-models, application/infrastructure layers, or persisted settings.
- No new layout, no orientation toggle (that is the follow-up change).
- No re-theming, no markup cleanup beyond what extraction mechanically requires.
- Not moving the composition root or dialog wiring out of `MainWindow` — those stay.

## Decisions

### Decision: Use UserControls with inherited DataContext (not per-view VMs)

Each extracted view inherits the shell from the parent `TabControl`/`TabItem`. This
mirrors `SettingsView` exactly and keeps the composition root in one place (`MainWindow`
constructor), preserving the existing test/design-time constructor pattern.

- **Alternative considered:** give each view its own constructor-injected VM. Rejected —
  it would fragment the composition root, complicate the existing
  `MainWindow(MainWindowShellViewModel)` test constructor, and exceed the refactor's
  scope.

### Decision: Wire named elements in the view via DataContextChanged

`PrimeGearView` wires `shell.WishlistDropdownButton`-equivalent flyout close in its own
`OnDataContextChanged`, exactly as `SettingsView` does for `ProfileDropdownButton`.
`RelicsMissionsView` subscribes to its own `Loaded`/`SizeChanged`/splitter events and to
`shell.PropertyChanged` / `shell.WishlistRelics.PropertyChanged` from within the view.

- **Alternative considered:** keep wiring in `MainWindow` reaching into the views via
  `FindControl`. Rejected — it re-introduces the coupling this change removes and breaks
  encapsulation.

### Decision: Splitter persistence stays routed through the shell

`RelicsMissionsView` continues to write `shell.RelicsPanelHeight` on drag-complete, and
`MainWindow.OnWindowClosing` continues to read it for the close-time save. The shell is
the shared channel, so the persistence seam is unchanged and no save logic moves.

- **Alternative considered:** have the view persist directly on `Unloaded`. Rejected for
  this change — it would alter the persistence timing/ownership and risks regressions;
  defer any such change to the layout follow-up if needed.

### Decision: Move splitter code-behind verbatim

The splitter/min-size methods move into `RelicsMissionsView.axaml.cs` essentially
unchanged (renamed only as needed), referencing the named elements now local to that
view. This minimizes regression surface; the logic itself is not redesigned here.

## Risks / Trade-offs

- **Named elements change owning scope (`MainWindow` → view)** → Tests or code that look
  up controls by name on the window may break. Mitigation: audit `FindControl`/named
  references; update them to query the view; run the full UI/integration suite.
- **DataContext timing for splitter init** → The Relics grid initialization depends on
  the tab becoming visible and measured. Moving it into the view changes when `Loaded`/
  `SizeChanged` first fire. Mitigation: preserve the existing one-shot
  `SizeChanged`-until-measured guard inside the view and verify first-show sizing.
- **Tab-guard index coupling** → `OnTabSelectionChanged` uses a hardcoded Settings tab
  index. Extraction must not reorder tabs. Mitigation: keep tab order identical; the
  guard stays in `MainWindow`.
- **Double subscription / leaks** → Views subscribing to `shell.PropertyChanged` must
  unsubscribe or be safe across DataContext changes. Mitigation: follow the
  `SettingsView` DataContextChanged pattern and guard against re-wiring.

## Migration Plan

Incremental, one view per step, building and running tests after each:
1. Extract `DropTablesView` (lowest risk — no code-behind).
2. Extract `PrimeGearView` (single flyout wiring).
3. Extract `RelicsMissionsView` (move all splitter/min-size code-behind).
4. Trim `MainWindow.axaml`/`.axaml.cs` to chrome + composition + window concerns.

Rollback is per-step: revert the offending view extraction; earlier steps are
independent.

## Open Questions

- None blocking. (Whether to also hoist shared card `DataTemplate`s into a
  `ResourceDictionary` is deferred to the layout follow-up, where it pays off.)
