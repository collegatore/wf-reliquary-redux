## 1. Phase A — Side-by-side POC (no persistence, no code-behind)

- [x] 1.1 In `RelicsMissionsView`, add a side-by-side layout subtree: a
  `Grid ColumnDefinitions="*,4,*"` with the Relics panel (control bar + scrolling card
  area) in column 0, a `GridSplitter Grid.Column="1" ResizeDirection="Columns"`, and the
  Missions panel in column 2.
- [x] 1.2 Give each column a declarative `MinWidth` derived from `RelicCardWidth` (+
  margins); keep card `Width` fixed to avoid WrapPanel re-wrap churn. Add no code-behind.
- [x] 1.3 Add a non-persisted in-session orientation flag and a throwaway toggle control;
  bind the stacked and side-by-side subtrees' `IsVisible` to it so only one shows at a
  time and the hidden one realizes no cards.
- [x] 1.4 Build and run; switch between layouts and confirm side-by-side renders, the
  vertical splitter drags, columns respect their minimum width, and both panels scroll
  independently. **GATE: evaluate usefulness before Phase B.**

## 2. Phase B — Shared card templates (only if Phase A approved)

- [x] 2.1 Hoist the relic-card and mission-card markup into `DataTemplate`s keyed by
  `DataType` in a shared `ResourceDictionary`; reference it from the view's resources.
- [x] 2.2 Reduce both layout subtrees to `<ItemsControl ItemsSource="{Binding …Cards}"/>`
  relying on type-resolved templates; confirm both layouts render identically to before.

## 3. Phase B — Orientation persistence

- [x] 3.1 Add a `RelicsMissionsOrientation` enum and a side-by-side split-position value
  to `UserSettings` (with sensible defaults: stacked orientation, default ratio).
- [x] 3.2 Update `LocalUserSettingsStore` round-trip and any settings DTO/mapping; add
  store tests for the new fields (default-on-missing, save/load).
- [x] 3.3 Surface orientation + side-by-side position on the shell
  (`MainWindowShellViewModel`) the way `RelicsPanelHeight` is surfaced today.
- [x] 3.4 Restore the saved orientation on launch and apply each orientation's own split
  position; default to stacked when no preference exists.
- [x] 3.5 Save orientation and side-by-side position on explicit Save and on window close
  (extend `OnWindowClosing` and the Settings save path alongside `RelicsPanelHeight`).

## 4. Phase B — Real toggle and finish

- [x] 4.1 Replace the throwaway toggle with the production control (placement decided from
  the POC: control-bar button / View menu / Settings).
- [x] 4.2 Confirm switching is immediate and does not reload relic/mission data.
- [x] 4.3 Run the full build and test suite; add/adjust UI/integration tests for
  orientation switching and per-orientation position memory.
- [x] 4.4 Sync the `relics-missions-layout-orientation` and `split-panel-persistence`
  delta specs to main specs and archive the change.
