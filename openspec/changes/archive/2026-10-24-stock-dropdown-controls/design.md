# Design

## Context

- Both selectors (`PrimeGearView.axaml` Wishlist, `SettingsView.axaml` Profile) are a `Button.dropdown-field` with a `Button.Flyout` → `<Flyout Placement="Bottom">`. The popup is a `StackPanel` of flat `Button`s:
  - choices from an `ItemsControl`, each entry calling its own `SelectCommand`,
  - a `Separator`,
  - three action buttons.

  `WishlistViewModel.FlyoutCloseRequested` exists only so the view can hide the flyout after a selection.
- `WishlistViewModel` / `ProfileViewModel` hold `ActiveEntry` (private setter, flips `IsActive` on entries) and `SelectAsync(entry)`, which persists the choice and raises `OnActiveWishlistChanged`. `ReloadAsync` clears and refills the collection on profile change. Both view models have integration tests (`WishlistViewModelTests`, `ProfileViewModelTests`).
- `Styles/Dropdowns.axaml` already gives every `ComboBox` and `ComboBoxItem` the palette field look, hover tint and selection color, and styles `MenuFlyoutPresenter.card-menu` (plain open, hover/pressed tints, `keyboard-nav` highlight via `CardMenuKeyboardHighlight`).
- Avalonia 12.0.5 provides the stock pieces this change uses:
  - `ComboBox.SelectionBoxItemTemplate`: a different template for the closed field than for list items.
  - `MenuItem.Icon`.
  - `DropDownButton`.
  - `MenuItem.ToggleType` / `GroupName` / `StaysOpenOnClick`, for the follow-up change.
- Font size scaling is done by `AppearanceViewModel.ApplyFontSize`, which writes scaled app resources (`RelicCardWidth`, `MissionCardWidth`, `SectionHeadingFontSize`).
- Table mode pictograms are built in code: `RelicsPanelView.axaml.cs` `BuildPictogramCell(row, string glyph, …)`.

## Goals / Non-Goals

**Goals:**
- Use stock controls for every dropdown and menu. Custom code is limited to styles and small behaviors the app already has a pattern for.
- One menu look (`MenuFlyout` presenter class) that the follow-up's Filters and Display menus reuse as is.
- One icon dictionary that later changes extend.

**Non-Goals:**
- Restyling ComboBox internals beyond what `Dropdowns.axaml` already does.
- Replacing the card `⋮` button glyphs, `✕`, `✓` and `⚠`. The icon set makes that a cheap follow-up.

## Decisions

### Selectors: `ComboBox` bound to the existing collections
- `ItemsSource` stays `Wishlists` / `Profiles`, and `SelectedItem` binds two-way to a new public `SelectedEntry` property on each view model:
  - The getter returns `ActiveEntry`, and `ActiveEntry` also raises `SelectedEntry` changes.
  - The setter ignores `null` and the current entry, and otherwise calls `SelectAsync(value).SafeFireAndForget()`.
- The `null` guard matters: `ReloadAsync` clears the collection, which makes the ComboBox push `null` back through the binding.
- Removed: `WishlistEntryViewModel.SelectCommand` / `ProfileEntryViewModel.SelectCommand`, `FlyoutCloseRequested` and its hook in `PrimeGearView.axaml.cs` (the ComboBox closes itself), and the entries' `IsActive`/`.active` styling, if nothing else uses it.

Alternative considered: binding `SelectedItem` straight to `ActiveEntry` and making its setter public. That would put async persistence inside a property setter that `ReloadAsync` and `CreateAsync` also set internally, so every internal assignment would re-persist. A separate `SelectedEntry` keeps "user chose" apart from "state changed".

### Open lists: left-aligned, with a gap
The Fluent ComboBox opens its list in `Popup#PART_Popup` with `Placement=Bottom`, which centers a list wider than its field under it, flush against the field. An app-wide style on `ComboBox /template/ Popup#PART_Popup` sets `Placement=BottomEdgeAlignedLeft` and a 4px `VerticalOffset`, so every dropdown's list hangs from its field's left edge. Wishlist entries trim to `NameDisplayMaxWidth` with a tooltip, as profile entries already did, so one long name can't stretch the list.

### "Wishlist:" caption: `SelectionBoxItemTemplate`
The closed field uses a `SelectionBoxItemTemplate` showing the muted `field-caption` "Wishlist:" followed by the name, trimmed with a tooltip as today. The `ItemTemplate` shows the name only. The Profile selector needs no caption: its label sits outside, in the Settings row.

### Actions button: `Button` + `MenuFlyout`
- The button is a plain `Button` with `Classes="dropdown-field icon-only"`, content `PathIcon` `Icon.MoreVertical`, and a `Button.Flyout`. This is the pattern the card `⋮` menus already use. `Button.dropdown-field:flyout-open` keeps the hover outline while the menu is open.
- It sits in a `StackPanel` with `Spacing` 4, tighter than the bar's 10, so it groups with its selector (`control-bar-visual-style` spacing rule). In Settings, a negative left margin does the same inside the row's existing panel.
- The `MenuFlyout` uses `Placement="BottomEdgeAlignedLeft"` and `FlyoutPresenterClasses="app-menu"`. Its `MenuItem`s have `Icon` = `PathIcon` (pencil, trash, plus) and bind to the existing `RenameCommand` / `DeleteCommand` / `CreateCommand`.
- Disabled state comes from the commands' `CanExecute`. The MenuItem template greys out `Icon` and header together because `PathIcon` inherits `Foreground`.

Alternative considered (and first planned): a stock `DropDownButton`. It would need its built-in chevron hidden, and its Fluent template uses different part names from `Button`, so the `dropdown-field` styles wouldn't apply to it. `Button` + `Flyout` reuses the existing styles and still exposes `:flyout-open`.

### One menu class: rename `card-menu` → `app-menu`
- The three `MenuFlyoutPresenter.card-menu` styles in `Dropdowns.axaml` and the `CardMenuKeyboardHighlight` behavior become `app-menu`. They are applied to the card menus (`App.axaml`, `PrimeGearView.axaml`) and to the new actions menus.
- `CardMenuKeyboardHighlight` is renamed `MenuKeyboardHighlight` if its name is the only card-specific thing about it; task 3.1 checks.
- The destructive hover is `MenuItem.destructive:pointerover:not(:disabled)` → `Foreground` = `DestructiveForegroundBrush`, an app-level brush in `App.axaml` rather than a palette key (the spec makes it palette-independent). The theme recolors `ContentPresenter#PART_HeaderPresenter` itself on hover, so the style sets the brush there too.

### Icons: `StreamGeometry` resources from Material Design Icons
- A new `Styles/Icons.axaml` resource dictionary is merged in `App.axaml`. Keys:
  - `Icon.MoreVertical`, `Icon.Pencil`, `Icon.Trash`, `Icon.Plus`, `Icon.Table.Rewards`, `Icon.Table.Recruit`, used now.
  - `Icon.Funnel`, `Icon.Sort`, `Icon.SortAscending`, `Icon.SortDescending`, `Icon.Eye`, for the follow-up.
- MDI (Pictogrammers, Apache-2.0) paths are fill-based, so `PathIcon` fills them with the inherited `Foreground`. That gives "icons follow the text color" without extra styles. Lucide was rejected because it's stroke-based and needs `Path` + `Stroke`, which doesn't scale cleanly.
- MDI gets an entry in `NOTICE`.

### Icon size: an `IconSize` resource
- `ApplyFontSize` also writes `resources["IconSize"] = 16 * ratio`, next to the other scaled resources. A default `PathIcon` style binds `Width` and `Height` to `{DynamicResource IconSize}`.
- `App.axaml` declares a default `IconSize`, so the snapshot tool renders sensibly without `ApplyFontSize`.
- Inside menus, `MenuItem`'s icon column sizes from its content, so icons scale there too.

### Table pictograms take an icon key and a tooltip
- `BuildPictogramCell(row, string iconKey, string tooltip, …)` sets `Content` to a `PathIcon` with `Data` looked up from app resources, plus `ToolTip.Tip`.
- `Foreground` is inherited, so the icon follows the row text color and `RowOpacity`.

### Snapshot scenes
- A `Scene` gets an optional `Prepare` step that runs once the window is shown. It opens the Wishlist ComboBox (`IsDropDownOpen`), opens an actions or card menu (`Flyout.ShowAt`), or sends key presses. Headless capture includes the popups, so no stand-alone fallback is needed.
- The selector stand-ins are the real `WishlistViewModel` / `ProfileViewModel` over in-memory stub stores (`StubStores.cs`), because the views' typed item templates need the real entry types.

## Risks / Trade-offs

- [The two-way `SelectedItem` binding fires during `ReloadAsync` / `CreateAsync` / `DeleteAsync`] → The `SelectedEntry` setter ignores `null` and the current entry. Integration tests cover reload-on-profile-switch and create, with the binding simulated by setting `SelectedEntry`.
- [Rename changes `Name` on the selected entry. The ComboBox's closed field must refresh] → The `SelectionBoxItemTemplate` binds to `Name` on the entry, which already raises change notifications. Verified by hand in task 2.1.
- [The actions are one click further away than before] → Accepted for stock behavior. The `⋮` sits right next to the selector and names itself on hover.
- [Headless snapshots may not capture popups] → Use the fallback scenes above. Real placement is checked once in the running app.
- [The `card-menu` → `app-menu` rename touches existing card menus] → It's a mechanical rename, and the existing card-menu scenarios in `color-palette-setting` now live under "Menus follow the palette" and are re-checked in task 6.1.

## Migration Plan

Presentation-only change, with no data or settings migration. Rollback is a revert.
