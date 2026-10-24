# Proposal

## Why

The Wishlist and Profile selectors are custom: a button whose popup mixes the list of choices with commands (Rename, Delete, New). This puts them outside the stock controls, so they reimplement placement, width and selection marking, and still don't get them right. The popup opens centered on the button, looks like a separate window, and has no arrow-key navigation. The app also has no icons: actions use text glyphs such as `⊞` and `💬`, which ignore the palette and render differently per machine.

The upcoming control-bar dropdowns (Filters, Sort, Display) should be built from stock Avalonia controls too. That needs the icon set and a palette-styled menu look in place first.

## What Changes

- **Selectors become stock ComboBoxes.** The Wishlist selector (Prime Gear bar) and the Profile selector (Settings) list only their choices, and they get the stock ComboBox's anchoring, field-width popup, keyboard navigation, and the selection color on the active entry. The Wishlist selector keeps its muted "Wishlist:" caption inside the closed field.
- **Commands move to an actions menu.** A `⋮` button next to each selector opens a stock menu with **Rename…**, **Delete** and **New**, each with an icon. "+ New list" / "+ New profile" become "New list" / "New profile". Delete shows a destructive color on hover and stays disabled when only one entry exists.
- **One menu look for the app.** The palette styling that only the card `⋮` menus have today becomes the look of every menu opened from a `⋮` button: card menus and the new actions menus. The follow-up's Filters and Display menus will use it too.
- **Icon set.** Add vector icons, defined as geometry resources in the app's styles (no icon package): more-vertical, pencil, trash, plus, the two Table mode actions, funnel, sort (double arrow), sort direction (up and down), and eye. Icons take the surrounding text color and scale with the font size setting.
- **Table mode icons.** Replace the pictogram glyphs (`⊞` rewards modal, `💬` Recruiting Chat message) with icons, and add tooltips. The Recruiting Chat message dialog's `📋 Copy` button gets the standard copy icon.
- **Snapshot scenes** for an open selector and an open actions menu.

Non-goals: the card `⋮` buttons, the `✕` close buttons, and the owned `✓` and ghost `⚠` markers keep their glyphs. The control-bar Filters, Sort and Display dropdowns are the follow-up change `control-bar-dropdowns`.

## Capabilities

### New Capabilities
- `selector-actions-menu`: the `⋮` actions button next to a selector and its menu. Covers placement next to the selector, entries with icons, the disabled state, and the destructive hover on Delete.
- `ui-iconography`: the in-app vector icon set. Icons replace text glyphs for actions and follow the text color and font size.

### Modified Capabilities
- `wishlist-management`:
  - The selector lists only wishlists.
  - "Wishlist management actions in dropdown" is replaced by an actions menu next to the selector.
  - "+ New list" becomes "New list", and the delete-disabled scenario moves to the menu.
- `player-profiles`: the Profiles section's actions move to an actions menu next to the Profile selector.
- `color-palette-setting`:
  - The selectors are ComboBoxes, so the dropdown requirements drop the "selector popup" wording.
  - The card-menu palette and highlight rules widen to every `⋮` menu.
  - The selection-color coverage list now names menus instead of only card menus.
- `control-bar-visual-style`: the "Wishlist:" caption sits inside the selector's closed field (a ComboBox) rather than inside a dropdown button.
- `relic-view-modes`: the Table mode rewards-modal and Recruiting Chat message pictograms are drawn as icons, with tooltips.

## Impact

- `src/Presentation/WF.Reliquary.Presentation/`:
  - `PrimeGearView.axaml` and `SettingsView.axaml`: the selectors become `ComboBox`es bound to the existing wishlist and profile lists, plus a `⋮` button with a `MenuFlyout`.
  - `WishlistViewModel` / `ProfileViewModel`: expose a selected item for two-way binding in place of per-entry select commands.
  - `RelicsPanelView.axaml.cs`: pictogram cells take an icon and a tooltip.
- `Styles/`:
  - New `Icons.axaml`.
  - `Dropdowns.axaml` loses the flyout-button entry styles, and the `card-menu` styles widen to a shared menu class.
  - A palette-independent destructive brush.
- `AppearanceViewModel.ApplyFontSize`: also scales an icon size resource.
- `NOTICE`: Material Design Icons (Apache-2.0).
- `tools/WF.Reliquary.UiSnapshots`: new scenes, and stand-ins for the changed bindings.
- No data, settings, or Application/Infrastructure changes.
