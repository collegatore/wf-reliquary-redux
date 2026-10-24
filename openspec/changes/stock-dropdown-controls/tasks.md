# Tasks

## 1. Icon set

- [x] 1.1 Create `Styles/Icons.axaml` with the MDI `StreamGeometry` keys from design ("Icons"), both the ones used now and the follow-up ones, and merge it in `App.axaml`. Verify the build succeeds and a scratch snapshot scene shows each icon.
- [x] 1.2 Add an `IconSize` resource (16 at Medium) with a default in `App.axaml`, write it from `AppearanceViewModel.ApplyFontSize` (`16 * ratio`), and add a default `PathIcon` style that binds `Width`/`Height` to it. Verify with the `relics-small-font` / `prime-gear-large-font` snapshot scenes that icons scale with the text.
- [x] 1.3 Add a Material Design Icons (Pictogrammers, Apache-2.0) entry to `NOTICE`'s third-party section. Verify the entry names the project, URL and license.

## 2. Selectors as ComboBoxes

- [x] 2.1 Wishlist selector:
  - In `WishlistViewModel`, add `SelectedEntry` (getter `ActiveEntry`; setter ignores `null` and the current entry, otherwise `SelectAsync(...).SafeFireAndForget()`) and raise its change from `ActiveEntry`.
  - Remove `FlyoutCloseRequested` and `WishlistEntryViewModel.SelectCommand`.
  - In `PrimeGearView.axaml`, replace the selector button and flyout with a `ComboBox` (`ItemsSource` `Wishlists`, `SelectedItem` two-way `SelectedEntry`, name-only `ItemTemplate`, `SelectionBoxItemTemplate` with the muted "Wishlist:" caption and the trimmed name plus tooltip). Remove the flyout-close hook in `PrimeGearView.axaml.cs`.
  - Add `WishlistViewModelTests` covering: setting `SelectedEntry` activates and persists; setting `null` or the current entry does nothing; reload after a profile switch doesn't re-persist.

  Verify `dotnet test` passes. In the running app, check that picking a list with the mouse and with the arrow keys + Enter switches it, and that renaming updates the closed field.
- [x] 2.2 Profile selector: apply the same change to `ProfileViewModel` / `ProfileEntryViewModel` and the selector in `SettingsView.axaml` (no caption), with matching `ProfileViewModelTests`. Verify `dotnet test` passes, and that switching profiles in the running app reloads the wishlist selector and the views.
- [x] 2.3 Remove the now-unused flyout entry styles (`FlyoutPresenter Button…`, `Button.active`) and, if nothing else uses it, `Button.dropdown-field` from `Dropdowns.axaml`. Verify the build succeeds and no remaining view references a removed class (grep).

- [x] 2.4 Left-align every ComboBox's open list under its field with a 4px gap (style on `ComboBox /template/ Popup#PART_Popup`; the theme centers wider lists), and trim long wishlist names in the list with a tooltip. Verify with the `prime-gear-wishlist-open-long` snapshot scene that a list wider than the field starts at the field's left edge, below a gap.

## 3. Actions menus

- [x] 3.1 Rename the `card-menu` presenter class to `app-menu` in `Dropdowns.axaml` and on the existing card `MenuFlyout`s. Rename `CardMenuKeyboardHighlight` to `MenuKeyboardHighlight` if nothing card-specific remains in it. Add `DestructiveForegroundBrush` to `App.axaml` and the `MenuItem.destructive:pointerover:not(:disabled)` style. Verify that the card menus still open plain, highlight on hover and with the Down arrow key, and use the palette tints (existing snapshot scenes and a manual check).
- [x] 3.2 Add the actions button (a `Button` with a `MenuFlyout`, see design) after each selector:
  - Classes `dropdown-field icon-only`.
  - `Icon.MoreVertical` content and a "Wishlist actions" / "Profile actions" tooltip.
  - Tighter spacing to its selector.
  - A `MenuFlyout` (`BottomEdgeAlignedLeft`, `app-menu`) with Rename… / Delete (`destructive`) / New items, each with its `PathIcon`, bound to the existing commands.
  - Labels "New list" / "New profile".

  Verify in the running app that each menu opens below its button, that Delete is greyed out (icon and label) with one entry and turns the destructive color on hover with two, and that every action still works.

## 4. Table pictograms

- [x] 4.1 Change `BuildPictogramCell` in `RelicsPanelView.axaml.cs` to take an icon resource key and a tooltip text. It renders a `PathIcon` that inherits the row's foreground and opacity, muted to 60% until hover (`Button.table-action` style in `App.axaml`). Use `Icon.Table.Rewards` (mdi:treasure-chest-outline) with the tooltip "View this relic's rewards" (not "Show all rewards", which names the rewards toggle) and `Icon.Table.Recruit` (mdi:chat-outline) with "Compose Recruiting Chat message…". Verify in the running app that both icons show in Table mode, follow the palette and the vaulted-row opacity, show their tooltips, and open the rewards modal and the composer.

- [x] 4.2 Replace the `📋` glyph on the Recruiting Chat message dialog's Copy button with an `Icon.Copy` (mdi:content-copy) icon before the "Copy" label. Verify with the `recruiting-message-dialog` snapshot scene that the icon renders and greys out with the label when Copy is unavailable.

## 5. Snapshot scenes and documentation

- [x] 5.1 Update `StandIn.cs` for the new bindings. Add the `prime-gear-wishlist-open` and `prime-gear-wishlist-actions` scenes plus `-stone` variants (headless capture includes popups, so no fallback is needed). Verify the PNGs show the open list with the selection color and the actions menu with icons, in both palettes.
- [x] 5.2 Update `docs/technical-reference.md`'s styling notes:
  - Icons live in `Styles/Icons.axaml` (MDI) and size from `IconSize`.
  - Selectors are ComboBoxes with a `DropDownButton` actions menu.
  - Menus use `FlyoutPresenterClasses="app-menu"`, with `destructive` items.
  - The new snapshot scenes exist.

  Add a `CHANGELOG.md` entry under `## [Unreleased]` for the selector and actions-menu change and the Table mode icons. Verify the docs name files, classes and keys that exist.

## 6. Integration check

- [ ] 6.1 In the running app, under both palettes and at Small and Large font sizes:
  - Switch wishlists and profiles with mouse and keyboard.
  - Rename, create and delete via the actions menus.
  - Open a card `⋮` menu.
  - Use both Table mode icons.
  - Shrink the window and confirm the selector lists and menus stay on screen.

  Run `dotnet test` and verify it passes.

## Workflow follow-up

- Archive the change after review, then build `control-bar-dropdowns` on top of it.
