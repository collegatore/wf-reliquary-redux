# Design

## Context

- **Relics bar** (`RelicsPanelView.axaml`):
  - A "Relics" heading, then Filters and Sorting toggle buttons.
  - Each toggle shows a `control-bar` row bound to `WishlistRelicsViewModel.IsFilterPanelOpen` / `IsSortPanelOpen`.
  - The filter row holds `ShowVaulted` / `ShowAllRewards` chips. The sort row holds a `ComboBox` (`SortModeIndex`) and a direction button (`SortDirectionLabel`).
- **Prime Gear bar** (`PrimeGearView.axaml`): a Filters toggle opens a row with three chips bound to `PrimeGearViewModel.ShowVaulted` / `ShowOwned` / `ShowWishlistedOnly`. The group-operations row hides Unmark owned (and un-joins the pair) unless `ShowOwned`.
- **Settings:**
  - Sort is persisted: `UserSettings.SortMode` / `SortDescending` (default `false`), written through `WishlistRelicsViewModel.SaveSortSettingsAsync`, which `MainWindow.axaml.cs` wires to a read-modify-write of the settings store.
  - `LoadSortSettings` restores at startup without the mode-change default.
  - In `LocalUserSettingsStore`'s `SettingsFile`, every field is nullable, with `UserSettings.Default` filling the gaps.
  - Filters and Show all rewards are session-only.
- **View mode:** it lives in the Appearance window (`AppearanceViewModel.SelectedViewMode`), which saves it and pushes it to `WishlistRelicsViewModel.ViewMode` through `ApplyViewMode`.
- **Available from `stock-dropdown-controls`:**
  - `Button.dropdown-field.icon-only` with a `MenuFlyout` in `FlyoutPresenterClasses="app-menu"`, which opens plain and highlights with the keyboard.
  - The icon set (`Icon.Funnel`, `Icon.Sort`, `Icon.SortAscending`, `Icon.SortDescending`, `Icon.Eye`) and `IconSize`.
  - ComboBox lists open left-aligned.
- **Existing behavior to keep:** `PrimeGearViewModel` already clears the group selection when any of its three filters changes, and the search hint already says "… hidden by filters".

## Goals / Non-Goals

**Goals:**
- Stock controls only: `Button` + `MenuFlyout` with toggle `MenuItem`s, the existing `ComboBox`, and plain buttons. New code is limited to view-model state, settings fields, and styles.
- Each view model owns "is my state non-default" and "describe my state". Views only bind, so it's testable without UI.

**Non-Goals:**
- The missions bar (Show exact drop chances stays a chip).
- A Display menu for Prime Gear.
- Migrating the sort direction of existing installs.

## Decisions

### Menu buttons: the actions-button pattern, plus toggle items
- Filters and Display are `Button Classes="dropdown-field icon-only"` with a `PathIcon` and a `Button.Flyout` → `MenuFlyout` (`Placement="BottomEdgeAlignedLeft"`, `FlyoutPresenterClasses="app-menu"`).
- Entries are stock `MenuItem`s:
  - **Filters:** `ToggleType="CheckBox"`, `IsChecked` bound two-way to the view model's `Hide*` property, and `StaysOpenOnClick="True"`, so several entries can be toggled in one visit.
  - **View modes:** `ToggleType="Radio"` with a shared `GroupName`, `IsChecked` bound through the existing `EnumToBoolConverter` (two-way, `ConverterParameter` = mode). They leave `StaysOpenOnClick` off, so choosing a mode closes the menu.
  - **Show all rewards:** `ToggleType="CheckBox"`, `StaysOpenOnClick="True"`, and `IsEnabled="{Binding !IsViewModeTable}"`.
  - **"Hide" / "View" captions:** a `MenuItem` with `IsEnabled="False"` and `Classes="menu-caption"`, styled muted and smaller. A disabled item is skipped by keyboard navigation, so it can't be "chosen".
- Alternative considered: CheckBoxes inside a plain `Flyout`. Rejected, because the user asked for stock menus, and `MenuItem` toggles give keyboard navigation and the menu look for free.

### Sort controls: keep the ComboBox, add an icon through `SelectionBoxItemTemplate`
- The sort-mode `ComboBox` and the direction button move into the Relics bar's row, inside a `StackPanel` with `Spacing` 4.
- The ComboBox keeps its three `ComboBoxItem`s. A `SelectionBoxItemTemplate` (which receives the selected item's string content) shows `PathIcon Classes="sort-icon"` (`Icon.Sort`) before the text, so the list itself stays text-only.
- The direction button becomes `dropdown-field icon-only`. Its icon is `Icon.SortDescending` or `Icon.SortAscending`, chosen by `SortDescending` through two `PathIcon`s with `IsVisible` bindings, so no converter is needed.
- `SortDirectionLabel` is removed; the tooltip carries the words.

### Non-default marker: a `non-default` class on the control
- Each control binds `Classes.non-default` to a view-model bool:
  - `WishlistRelicsViewModel.IsFilterActive`, `IsSortNonDefault`, `IsDisplayNonDefault`.
  - `PrimeGearViewModel.IsFilterActive`.
- Styles in `Styles/Dropdowns.axaml`:
  - `Button.dropdown-field.non-default PathIcon` sets `Foreground` to `SelectionAccentBrush`.
  - `ComboBox.non-default PathIcon.sort-icon` does the same.
- Using the palette key means the marker follows Linen and Stone like the chip tick.
- "Default" definitions live next to the state they describe:
  - Filters: any `Hide*` true.
  - Sort: anything but Era + descending.
  - Display: view mode ≠ Compact, or `ShowAllRewards`.

### Tooltips: view-model summary strings, default delay
- `FilterSummary`, `SortSummary` and `DisplaySummary` are computed string properties, raised alongside the state they describe. They look like "Filters: nothing hidden", "Filters: hiding vaulted, owned", "Sorted by Era, descending", "Display: Compact".
- They are bound to `ToolTip.Tip`. The sort summary goes on both the dropdown and the direction button.
- Avalonia's default `ToolTip.ShowDelay` (400 ms) is the "short delay". No custom value is needed.

### Filter flags invert; settings gain nullable fields
- **Presentation:**
  - `WishlistRelicsViewModel.ShowVaulted` → `HideVaulted`.
  - `PrimeGearViewModel.ShowVaulted` / `ShowOwned` / `ShowWishlistedOnly` → `HideVaulted` / `HideOwned` / `HideNotWishlisted`.
  - Every reader inverts accordingly: visibility rules, the hidden-match hint counts, `IsShown`, the fade-out-on-mark-owned path, and the XAML `IsVisible` bindings on the Vaulted section.
- **`UserSettings`:** gains `bool RelicsHideVaulted`, `bool GearHideVaulted`, `bool GearHideOwned`, `bool GearHideNotWishlisted` and `bool ShowAllRewards`, all defaulting to `false`. `SortDescending`'s default becomes `true`.
- **`LocalUserSettingsStore`:** adds the matching nullable fields to `SettingsFile`, writes them, and reads them with `?? UserSettings.Default.X`. A missing `SortDescending` therefore loads as `true`, while a saved `false` stays `false`, which is the agreed "existing installs keep Era ascending".
- **Load and save** follow the sort pattern:
  - `LoadFilterSettings(...)` / `LoadDisplaySettings(...)` set the backing fields without saving or applying defaults.
  - `SaveFilterSettingsAsync` / `SaveDisplaySettingsAsync` hooks are fired-and-forgotten on user changes and wired in `MainWindow.axaml.cs` as `current with { … }` writes, like `SaveSortSettingsAsync`.
- **Sort default:** `SortMode`'s setter applies "descending unless Alphabetical" instead of "descending only for WishlistScore".

### View mode leaves the Appearance window
- The view-mode radio row, `SelectedViewMode`, `ApplyViewMode` and the view-mode save leave `AppearanceViewModel` and `AppearanceWindow.axaml`. The window's fixed height shrinks to fit.
- `WishlistRelicsViewModel.ViewMode`'s setter now also fires `SaveDisplaySettingsAsync`. The startup path (`MainWindow.axaml.cs`) keeps setting it from `initialSettings.ViewMode`, through `LoadDisplaySettings`, so loading doesn't save.

### Group operations: always-joined pair
In `PrimeGearView.axaml`, drop `IsVisible="{Binding PrimeGear.ShowOwned}"` from Unmark owned and the `Classes.joined` binding from its group (always `joined`). `UnmarkSelectedCommand`'s existing `CanExecute` (`HasOwnedSelected`) already gives the inactive state.

### Menu markers in the selection accent
The Fluent `MenuItem` template draws its check and radio marks in named template parts. The `app-menu` styles set their `Foreground` / `Fill` to `SelectionAccentBrush`. Task 3.1 reads the 12.0.5 template, records the part names in the header comment of `Dropdowns.axaml` (as that file does for ComboBox and MenuItem), and only then writes the selectors.

## Risks / Trade-offs

- [Fire-and-forget settings saves from three hooks can interleave: two read-modify-write saves started together may each write a stale copy of the other's field] → The existing sort hook already has this shape. To close it rather than copy it, the three hooks share one `SemaphoreSlim` in `MainWindow.axaml.cs`, so saves run one at a time, each reading the latest settings.
- [Inverting `Show*` to `Hide*` touches every visibility rule, which risks a missed negation] → The renames are done by the compiler, not by search. Existing `PrimeGearViewModel` tests are updated to the new names, and tests cover each Hide flag's effect, plus the hint counts.
- [A disabled `MenuItem` used as a caption might still render the theme's disabled look] → The `menu-caption` style sets its own foreground and smaller font size. Verified in a snapshot of each open menu.
- [Existing installs with Era ascending saved show the accent marker until they flip it] → Accepted in exploration. It is correct ("not the default"), and one click clears it.
- [Removing the view mode from Appearance moves a control people may look for there] → The Display menu sits on the bar the view mode affects, and the user guide is updated.

## Migration Plan

No data migration. New settings fields are nullable on read and default to "nothing hidden" and "Show all rewards off". A missing sort direction now means descending, and saved values are kept. Rollback is a revert. Settings files written by the new version stay readable by the old one, which ignores the unknown fields.

## Open Questions

- Exact tooltip wording beyond the patterns above can be settled during implementation without affecting specs or tasks.
