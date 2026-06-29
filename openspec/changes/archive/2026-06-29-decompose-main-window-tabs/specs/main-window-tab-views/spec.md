# main-window-tab-views Specification

## ADDED Requirements

### Requirement: Per-Tab View Decomposition

Each tab hosted by the main window's `TabControl` SHALL have its visual content defined
in a dedicated Avalonia `UserControl` (one per tab), rather than inline within
`MainWindow.axaml`. The main window SHALL reference each tab's content only as that
UserControl element. This applies to the Prime Gear, Relics and Missions, Drop Tables,
and Settings tabs.

#### Scenario: Each everyday tab is a dedicated UserControl

- **WHEN** the application is built
- **THEN** the Prime Gear, Relics and Missions, Drop Tables, and Settings tab contents
  are each provided by their own UserControl, and `MainWindow.axaml` contains no inline
  tab-content markup beyond the UserControl references

#### Scenario: Tab content renders identically after decomposition

- **WHEN** the user opens any tab
- **THEN** the tab displays the same controls, layout, and data as before the
  decomposition, with no user-visible change

### Requirement: Shell DataContext Inheritance

Each tab UserControl SHALL inherit the shell view-model as its `DataContext` from the
main window rather than constructing or assigning its own. Bindings within a tab view
SHALL resolve against the same shell sub-view-models the inline markup used.

#### Scenario: A tab view binds against the inherited shell

- **WHEN** a tab UserControl is displayed inside the main window whose DataContext is the
  shell view-model
- **THEN** the view's bindings resolve against the shell's sub-view-models and display
  data without the view assigning its own DataContext

### Requirement: View-Owned Named-Element Wiring

A tab's code-behind that operates on one of its named elements SHALL reside in that tab's
UserControl code-behind, not in `MainWindow`. This includes flyout-close handling and the
Relics/Missions splitter and minimum-size handling. A view that wires a shell callback to
one of its named elements SHALL do so when its DataContext becomes the shell view-model.

#### Scenario: Relics/Missions splitter behavior is owned by its view

- **WHEN** the user drags the Relics/Missions splitter or toggles options that affect
  card sizing
- **THEN** the splitter initialization, drag-completion persistence, and minimum-size
  enforcement behave exactly as before, driven by code-behind that resides in the
  Relics/Missions view

#### Scenario: Dropdown flyout close is owned by its view

- **WHEN** the user performs an action that closes a tab's dropdown flyout (such as the
  Prime Gear wishlist dropdown)
- **THEN** the flyout closes as before, driven by wiring that resides in that tab's view

### Requirement: Main Window Retains Composition and Chrome Only

After decomposition, `MainWindow` SHALL retain only window-level responsibilities: the
composition root (constructing the shell and its view-models and wiring dialog
callbacks), window size and position restore and save, the toast/error reporter, the
footer spinner, and the menu-accessed-tab guard. Splitter position persistence SHALL
continue to flow through the shell so that the window's close-time save remains correct.

#### Scenario: Window-level behaviors are preserved

- **WHEN** the application starts, the window is resized or moved, an error toast is
  shown, or the window is closed
- **THEN** the saved window size and position are restored on next launch, error toasts
  appear, the footer spinner animates, the Settings tab-guard prompts on unsaved
  changes, and the Relics/Missions splitter position persists across restarts — all
  exactly as before
