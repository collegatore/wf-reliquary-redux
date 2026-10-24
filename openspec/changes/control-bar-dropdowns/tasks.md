# Tasks

## 1. Settings

- [ ] 1.1 In `UserSettings`, add `RelicsHideVaulted`, `GearHideVaulted`, `GearHideOwned`, `GearHideNotWishlisted` and `ShowAllRewards` (all `false` by default), and change `SortDescending`'s default to `true`. In `LocalUserSettingsStore`, add the matching nullable `SettingsFile` fields, write them, and read them with `?? UserSettings.Default.X`. Extend `UserSettingsStoreTests` to cover:
  - a round trip of every new field;
  - a file without the new fields (nothing hidden, Show all rewards off);
  - a file without `SortDescending` (descending);
  - a file with `SortDescending: false` (kept ascending).

  Verify `dotnet test` passes.

## 2. Relics view model

- [ ] 2.1 In `WishlistRelicsViewModel`, rename `ShowVaulted` to `HideVaulted` (inverting every reader), make `SortMode`'s mode-change default "descending unless Alphabetical", and remove `IsFilterPanelOpen`, `IsSortPanelOpen`, their toggle commands, and `SortDirectionLabel`. Update `WishlistRelicsViewModelTests` and `WishlistRelicsSortingTests`, including:
  - selecting Era gives descending;
  - selecting Alphabetical gives ascending;
  - `LoadSortSettings` keeps a saved Era ascending;
  - `HideVaulted` hides vaulted relics in cards and table rows.

  Verify `dotnet test` passes.
- [ ] 2.2 Add `IsFilterActive`, `IsSortNonDefault` and `IsDisplayNonDefault`, and the `FilterSummary`, `SortSummary` and `DisplaySummary` strings (design: "Non-default marker", "Tooltips"). Raise each whenever its inputs change. Add tests for each flag's default and non-default cases, and for a sample of summary texts. Verify `dotnet test` passes.
- [ ] 2.3 Add `LoadFilterSettings` / `LoadDisplaySettings`, which set state without saving, and `SaveFilterSettingsAsync` / `SaveDisplaySettingsAsync` hooks, fired on user changes to `HideVaulted`, `ShowAllRewards` and `ViewMode`. Add tests that a user change invokes the hook and a load does not. Verify `dotnet test` passes.

## 3. Shared styles

- [ ] 3.1 Inspect the Avalonia 12.0.5 Fluent `MenuItem` template for the check and radio mark parts, and record them in the header comment of `Dropdowns.axaml`. Then add, scoped to `app-menu`:
  - the marker styles (`SelectionAccentBrush`);
  - the `MenuItem.menu-caption` style (muted, smaller, not interactive);
  - the `non-default` styles: `Button.dropdown-field.non-default PathIcon` and `ComboBox.non-default PathIcon.sort-icon`, both using `SelectionAccentBrush`.

  Verify with snapshot scenes (task 7.1) that markers and captions render as intended in both palettes.

## 4. Relics bar

- [ ] 4.1 In `RelicsPanelView.axaml`, replace the Filters/Sorting buttons and both panel rows with:
  - the Filters button (`Icon.Funnel`, "Hide" caption, Vaulted checkbox item);
  - the sort pair: the `ComboBox` with a `SelectionBoxItemTemplate` showing `Icon.Sort` plus the mode, and the icon-only direction button showing `Icon.SortDescending` or `Icon.SortAscending`;
  - the Display button (`Icon.Eye`, "View" caption, three radio items, separator, Show all rewards checkbox item disabled in Table mode).

  Each control gets `Classes.non-default` and its `ToolTip.Tip` summary. Verify in the running app that every menu opens below its button, that toggles apply immediately, that checkbox items keep the menu open while a view-mode choice closes it, and that markers and tooltips update.
- [ ] 4.2 In `MainWindow.axaml.cs`, restore the filter and display settings through the new `Load*` methods at startup, and wire the `Save*` hooks (and the existing sort hook) as `current with { … }` writes serialized by one shared `SemaphoreSlim`. Verify in the running app that Vaulted, Show all rewards, view mode and sort survive a restart, and that toggling several in quick succession keeps all of them.

## 5. Appearance window

- [ ] 5.1 Remove the relic view mode row from `AppearanceWindow.axaml` (shrinking its fixed height to fit) and `SelectedViewMode` / `ApplyViewMode` / the view-mode save from `AppearanceViewModel` and its wiring in `MainWindowShellViewModel`. Update `AppearanceViewModelTests`. Verify `dotnet test` passes and the window shows the remaining controls unclipped.

## 6. Prime Gear

- [ ] 6.1 In `PrimeGearViewModel`, rename `ShowVaulted` / `ShowOwned` / `ShowWishlistedOnly` to `HideVaulted` / `HideOwned` / `HideNotWishlisted` (inverting every reader: visibility, `IsShown`, fade-out on mark owned, hidden-match hint counts). Add `IsFilterActive`, `FilterSummary`, `LoadFilterSettings` and `SaveFilterSettingsAsync`, and remove `IsFilterPanelOpen` and its command. Update `PrimeGearViewModelTests` for:
  - the renames;
  - each Hide flag's effect;
  - the hint counts;
  - selection clearing on each filter;
  - the summary and the active flag.

  Verify `dotnet test` passes.
- [ ] 6.2 In `PrimeGearView.axaml`, replace the Filters toggle and the filter row with the Filters menu button (`Icon.Funnel`, "Hide" caption, Vaulted / Owned / Not wishlisted checkbox items, marker, tooltip), and flip the Vaulted section's `IsVisible` to `!HideVaulted`. Make Mark owned / Unmark owned an always-joined pair with Unmark owned always visible. Wire the Prime Gear filter load and save in `MainWindow.axaml.cs`. Verify in the running app that:
  - the menu works and persists across a restart;
  - owned and vaulted gear show by default;
  - Unmark owned stays shown, inactive, while Owned is hidden.

## 7. Snapshot scenes and documentation

- [ ] 7.1 Update `StandIn.cs` for the renamed and new bindings, drop the panel-row properties, and add scenes for:
  - the Relics bar at rest, default and non-default;
  - the open Relics Filters, Display (in Compact and in Table mode) and sort-mode menus;
  - the open Prime Gear Filters menu;
  - Stone variants of the open menus;
  - the group-operations row with Owned hidden.

  Verify the PNGs show the funnel, double-arrow, direction and eye icons, accent markers on non-default controls, captions and markers in the menus, and the always-joined Mark/Unmark pair.
- [ ] 7.2 Update `docs/user-guide.md` (Prime Gear filters, relic sorting and display, view mode no longer in Appearance), `docs/technical-reference.md` (bar menu buttons, the `non-default` class, `menu-caption`, the settings fields), and `CHANGELOG.md` under `## [Unreleased]`, marking the new defaults (everything visible, Era descending) as changes. Verify the docs name files, classes and settings that exist.

## 8. Integration check

- [ ] 8.1 In the running app, under both palettes and at Small and Large font sizes:
  - Use every menu with mouse and keyboard.
  - Check that the accent markers and tooltips track each change.
  - Restart and confirm every filter, the sort, the view mode and Show all rewards are restored.
  - Load an older settings file (no new fields) and confirm everything is visible with Era descending.

  Run `dotnet test` and verify it passes.

## Workflow follow-up

- On archive, update the Purpose sections of the `relics-filter-panel`, `relics-sorting-panel`, `prime-gear-filter-panel` and `control-bar-visual-style` main specs, which still describe panels and rows.
